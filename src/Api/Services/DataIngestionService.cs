using System.Text.Json;
using Api.DTOs.Sites;
using Data;
using Data.Models;
using Data.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

/// <summary>
/// Service for handling data uploads and validation.
/// </summary>
public class DataIngestionService : IDataIngestionService
{
    private readonly AppDbContext _context;
    private readonly ILogger<DataIngestionService> _logger;

    public DataIngestionService(AppDbContext context, ILogger<DataIngestionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DataUploadResponse> ProcessUploadAsync(
        int siteId,
        int userId,
        DataUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = new DataUploadResponse
        {
            RowsParsed = request.Rows.Count
        };

        // Get the site and its current schema
        var site = await _context.Sites
            .FirstOrDefaultAsync(s => s.Id == siteId, cancellationToken);

        if (site == null)
        {
            response.Errors.Add(new ValidationError
            {
                RowIndex = -1,
                Date = DateTime.MinValue,
                Field = "siteId",
                Message = $"Site with ID {siteId} not found"
            });
            return response;
        }

        // Create the upload record
        var upload = new Upload
        {
            SiteId = siteId,
            Site = site,
            UserId = userId,
            UploadedAt = DateTime.UtcNow,
            FileName = $"api-upload-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json",
            RowsParsed = request.Rows.Count,
            ValidationStatus = UploadValidationStatus.Pending
        };

        _context.Uploads.Add(upload);
        await _context.SaveChangesAsync(cancellationToken);
        response.UploadId = upload.Id;

        // Validate data if not skipped
        if (!request.SkipValidation)
        {
            var (warnings, errors) = await ValidateDataAsync(siteId, request.Rows, cancellationToken);
            response.Warnings = warnings;
            response.Errors = errors;

            if (errors.Count > 0)
            {
                upload.ValidationStatus = UploadValidationStatus.Invalid;
                upload.ErrorMessage = $"{errors.Count} validation error(s) found";
                await _context.SaveChangesAsync(cancellationToken);
                return response;
            }
        }

        // Get existing dates for this site to check for duplicates
        var uploadDates = request.Rows.Select(r => r.Date.Date).ToList();
        var existingDates = await _context.DataRows
            .Where(dr => dr.SiteId == siteId && uploadDates.Contains(dr.Date.Date))
            .Select(dr => dr.Date.Date)
            .ToListAsync(cancellationToken);

        var existingDateSet = existingDates.ToHashSet();

        // Process each row
        foreach (var (row, index) in request.Rows.Select((r, i) => (r, i)))
        {
            var dateOnly = row.Date.Date;
            var sensorDataJson = JsonSerializer.Serialize(row.SensorData);

            if (existingDateSet.Contains(dateOnly))
            {
                if (request.OverwriteExisting)
                {
                    // Update existing row
                    var existingRow = await _context.DataRows
                        .FirstOrDefaultAsync(dr => dr.SiteId == siteId && dr.Date.Date == dateOnly, cancellationToken);

                    if (existingRow != null)
                    {
                        existingRow.SensorDataJson = sensorDataJson;
                        existingRow.SchemaVersionId = site.CurrentSchemaVersionId;
                        response.RowsUpdated++;
                    }
                }
                else
                {
                    // Skip duplicate
                    response.RowsSkipped++;
                    response.Warnings.Add(new ValidationWarning
                    {
                        RowIndex = index,
                        Date = row.Date,
                        Field = "Date",
                        Message = $"Data for {dateOnly:yyyy-MM-dd} already exists. Use OverwriteExisting to update."
                    });
                }
            }
            else
            {
                // Insert new row
                var dataRow = new DataRow
                {
                    SiteId = siteId,
                    Site = site,
                    SchemaVersionId = site.CurrentSchemaVersionId,
                    Date = dateOnly,
                    SensorDataJson = sensorDataJson,
                    CreatedAt = DateTime.UtcNow
                };

                _context.DataRows.Add(dataRow);
                response.RowsInserted++;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Update upload record with results
        upload.RowsInserted = response.RowsInserted + response.RowsUpdated;
        upload.ValidationStatus = response.Errors.Count == 0 
            ? UploadValidationStatus.Validated 
            : UploadValidationStatus.Invalid;

        await _context.SaveChangesAsync(cancellationToken);

        response.Success = response.Errors.Count == 0;

        // Check if we should trigger inference
        if (response.Success && site.AutoInferenceEnabled)
        {
            var inferenceRequest = await TriggerInferenceAsync(siteId, userId, cancellationToken);
            if (inferenceRequest != null)
            {
                response.InferenceTriggered = true;
                response.InferenceRequestId = inferenceRequest.Id;
            }
        }

        _logger.LogInformation(
            "Upload {UploadId} for site {SiteId}: {Inserted} inserted, {Updated} updated, {Skipped} skipped",
            upload.Id, siteId, response.RowsInserted, response.RowsUpdated, response.RowsSkipped);

        return response;
    }

    public async Task<(List<ValidationWarning> Warnings, List<ValidationError> Errors)> ValidateDataAsync(
        int siteId,
        List<DataRowInput> rows,
        CancellationToken cancellationToken = default)
    {
        var warnings = new List<ValidationWarning>();
        var errors = new List<ValidationError>();

        // Get the site's current schema
        var site = await _context.Sites
            .FirstOrDefaultAsync(s => s.Id == siteId, cancellationToken);

        if (site?.CurrentSchemaVersionId == null)
        {
            // No schema defined - accept all data but warn
            warnings.Add(new ValidationWarning
            {
                RowIndex = -1,
                Date = DateTime.MinValue,
                Field = "Schema",
                Message = "No schema defined for this site. Data will be accepted without validation."
            });
            return (warnings, errors);
        }

        var schema = await _context.SiteDataSchemas
            .FirstOrDefaultAsync(s => s.Id == site.CurrentSchemaVersionId, cancellationToken);

        if (schema == null)
        {
            warnings.Add(new ValidationWarning
            {
                RowIndex = -1,
                Date = DateTime.MinValue,
                Field = "Schema",
                Message = "Schema version not found. Data will be accepted without validation."
            });
            return (warnings, errors);
        }

        // Parse schema definition
        Dictionary<string, SchemaColumn>? schemaColumns = null;
        try
        {
            schemaColumns = JsonSerializer.Deserialize<Dictionary<string, SchemaColumn>>(schema.SchemaDefinition);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse schema definition for site {SiteId}", siteId);
            warnings.Add(new ValidationWarning
            {
                RowIndex = -1,
                Date = DateTime.MinValue,
                Field = "Schema",
                Message = "Schema definition is invalid. Data will be accepted without validation."
            });
            return (warnings, errors);
        }

        if (schemaColumns == null || schemaColumns.Count == 0)
        {
            return (warnings, errors);
        }

        // Validate each row
        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];

            // Validate date is not in the future
            if (row.Date.Date > DateTime.UtcNow.Date)
            {
                errors.Add(new ValidationError
                {
                    RowIndex = i,
                    Date = row.Date,
                    Field = "Date",
                    Message = "Date cannot be in the future"
                });
            }

            // Check for required fields
            foreach (var (columnName, column) in schemaColumns)
            {
                if (column.Required && !row.SensorData.ContainsKey(columnName))
                {
                    errors.Add(new ValidationError
                    {
                        RowIndex = i,
                        Date = row.Date,
                        Field = columnName,
                        Message = $"Required field '{columnName}' is missing"
                    });
                }
            }

            // Validate field values
            foreach (var (fieldName, fieldValue) in row.SensorData)
            {
                if (!schemaColumns.TryGetValue(fieldName, out var column))
                {
                    // Unknown field - warn but don't error
                    warnings.Add(new ValidationWarning
                    {
                        RowIndex = i,
                        Date = row.Date,
                        Field = fieldName,
                        Message = $"Unknown field '{fieldName}' not in schema"
                    });
                    continue;
                }

                // Validate numeric ranges
                if (fieldValue != null && column.DataType == "number")
                {
                    if (double.TryParse(fieldValue.ToString(), out var numValue))
                    {
                        if (column.Min.HasValue && numValue < column.Min.Value)
                        {
                            warnings.Add(new ValidationWarning
                            {
                                RowIndex = i,
                                Date = row.Date,
                                Field = fieldName,
                                Message = $"Value {numValue} is below minimum {column.Min.Value}"
                            });
                        }

                        if (column.Max.HasValue && numValue > column.Max.Value)
                        {
                            warnings.Add(new ValidationWarning
                            {
                                RowIndex = i,
                                Date = row.Date,
                                Field = fieldName,
                                Message = $"Value {numValue} is above maximum {column.Max.Value}"
                            });
                        }
                    }
                }
            }
        }

        return (warnings, errors);
    }

    private async Task<InferenceRequest?> TriggerInferenceAsync(
        int siteId,
        int userId,
        CancellationToken cancellationToken)
    {
        // Check if there's an active model
        var activeModel = await _context.ModelVersions
            .FirstOrDefaultAsync(mv => mv.SiteId == siteId && mv.IsActive, cancellationToken);

        if (activeModel == null)
        {
            _logger.LogInformation("No active model for site {SiteId}, skipping inference", siteId);
            return null;
        }

        // Get the site for the required navigation property
        var site = await _context.Sites.FindAsync(new object[] { siteId }, cancellationToken);
        if (site == null)
        {
            return null;
        }

        var inferenceRequest = new InferenceRequest
        {
            SiteId = siteId,
            Site = site,
            RequestedAt = DateTime.UtcNow,
            UserId = userId,
            Status = InferenceRequestStatus.Pending,
            InputDataJson = $"{{\"modelVersionId\": {activeModel.Id}}}" // Will be populated by inference worker
        };

        _context.InferenceRequests.Add(inferenceRequest);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Triggered inference request {RequestId} for site {SiteId} using model {ModelId}",
            inferenceRequest.Id, siteId, activeModel.Id);

        return inferenceRequest;
    }

    /// <summary>
    /// Schema column definition for validation.
    /// </summary>
    private class SchemaColumn
    {
        public string DataType { get; set; } = "string";
        public bool Required { get; set; }
        public double? Min { get; set; }
        public double? Max { get; set; }
        public string? Unit { get; set; }
        public string? Description { get; set; }
    }
}
