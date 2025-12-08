# Data Model Documentation

Complete guide to all entities in the BiofuelAiApiDemo system.

---

## Table of Contents

1. [Overview](#overview)
2. [Core Entities](#core-entities)
   - [Company](#1-company)
   - [User](#2-user)
   - [Site](#3-site)
   - [UserSiteAccess](#4-usersiteaccess)
3. [Configuration Entities](#configuration-entities)
   - [SiteDataSchema](#5-sitedataschema)
   - [DataCleaningRule](#6-datacleaningrule)
   - [DataValidationRule](#7-datavalidationrule)
   - [Dashboard](#8-dashboard)
4. [Data Entities](#data-entities)
   - [DataRow](#9-datarow)
   - [Upload](#10-upload)
5. [ML Entities](#ml-entities)
   - [ModelVersion](#11-modelversion)
   - [TrainingJob](#12-trainingjob)
   - [InferenceRequest](#13-inferencerequest)
   - [PredictionResult](#14-predictionresult)
6. [Entity Relationships](#entity-relationships)
7. [Delete Behaviors](#delete-behaviors)

---

## Overview

The BiofuelAiApiDemo system uses a **multi-tenant architecture** with 14 core entities organized into 4 PostgreSQL schemas:

| Schema | Purpose | Entities |
|--------|---------|----------|
| **core** | User management & multi-tenancy | Company, User, Site, UserSiteAccess |
| **config** | Site configuration & rules | SiteDataSchema, DataCleaningRule, DataValidationRule, Dashboard |
| **data** | Operational data storage | DataRow, Upload |
| **ml** | Machine learning operations | ModelVersion, TrainingJob, InferenceRequest, PredictionResult |

---

## Core Entities

### 1. Company

**Schema**: `core.Companies`

**Purpose**: Top-level tenant in the multi-tenant architecture. Represents a biofuel production company with one or more sites.

**Key Fields**:
```csharp
public int Id { get; set; }                    // Primary key
public required string Name { get; set; }      // Company name (max 200 chars, unique)
public DateTime CreatedAt { get; set; }        // Registration date
```

**Business Rules**:
- Company name must be unique across the system
- Each company can have multiple sites
- When a company is deleted, all its sites and users are also deleted (cascade)

**Example**:
```
Company: Acme Biofuel Corporation
  └── Sites: Plant A, Plant B, Plant C
  └── Users: John (admin), Jane (manager), Bob (consultant)
```

---

### 2. User

**Schema**: `core.Users`

**Purpose**: Represents system users who can access one or more sites. Supports both company users and individual users (e.g., consultants, trial users).

**Key Fields**:
```csharp
public int Id { get; set; }                    // Primary key
public int? CompanyId { get; set; }            // Nullable FK to Company (null for individuals)
public Company? Company { get; set; }          // Navigation property
public required string Email { get; set; }     // Unique email (max 256 chars)
public UserRole Role { get; set; }             // SystemAdmin, CompanyAdmin, User
public bool IsIndividual { get; set; }         // True for consultants/trial users
public DateTime CreatedAt { get; set; }        // Account creation date
public DateTime? LastLogin { get; set; }       // Last login timestamp
```

**Enums**:
```csharp
public enum UserRole
{
    SystemAdmin,    // Platform administrator
    CompanyAdmin,   // Company-level administrator
    User            // Regular user
}
```

**Business Rules**:
- Email must be unique across the system
- Individual users (IsIndividual = true) can have CompanyId = null
- Company users belong to exactly one company
- When a company is deleted, all its users are also deleted (cascade)
- Users can be deleted without affecting their created records (audit trails use SetNull)

**Example**:
```
User: john@acme.com
  CompanyId: 1 (Acme Corp)
  Role: CompanyAdmin
  IsIndividual: false
  Access: Site A (Admin), Site B (Viewer)

User: consultant@example.com
  CompanyId: null
  Role: User
  IsIndividual: true
  Access: Site A (Viewer) - temporary access
```

---

### 3. Site

**Schema**: `core.Sites`

**Purpose**: Represents a physical biofuel production facility. Each site has its own data schema, models, and configuration.

**Key Fields**:
```csharp
public int Id { get; set; }                          // Primary key
public int CompanyId { get; set; }                   // FK to Company
public required Company Company { get; set; }        // Navigation property
public required string SiteName { get; set; }        // Site name (max 200 chars)
public string? Location { get; set; }                // Physical address (max 500 chars)
public string? TimeZone { get; set; }                // e.g., "America/New_York" (max 100 chars)
public int? CurrentSchemaVersionId { get; set; }     // FK to SiteDataSchema (nullable)

// Site configuration (JSONB)
public required string ConfigJson { get; set; }      // Equipment IDs, thresholds, custom settings

// Automation settings
public bool AutoInferenceEnabled { get; set; }       // Enable automatic predictions
public TimeSpan? InferenceSchedule { get; set; }     // e.g., run at 2 AM daily
public bool AutoRetrainingEnabled { get; set; }      // Enable automatic model retraining
public int? RetrainingFrequencyDays { get; set; }    // e.g., retrain every 30 days
public bool TrainOnEveryUpload { get; set; }         // Trigger retraining after each upload

// Power BI integration
public bool PowerBiEnabled { get; set; }             // Enable Power BI integration
public string? PowerBiWorkspaceId { get; set; }      // Power BI workspace ID (max 100 chars)

// Audit fields
public DateTime CreatedAt { get; set; }
public DateTime UpdatedAt { get; set; }
```

**ConfigJson Example**:
```json
{
  "equipmentIds": {
    "reactor1": "R-001",
    "reactor2": "R-002"
  },
  "thresholds": {
    "minBiogasProduction": 1000,
    "maxTemperature": 40,
    "alertEmail": "ops@acme.com"
  },
  "customSettings": {
    "reactorVolume": 1000,
    "workingHours": "24/7"
  }
}
```

**Business Rules**:
- Each site belongs to exactly one company
- Sites can have different data schemas (columns can vary by site)
- When a site is deleted, all its data (DataRows, Uploads, Models, etc.) are also deleted (cascade)
- CurrentSchemaVersionId can be null if no schema is defined yet

**Example**:
```
Site: Acme Biofuel Plant A
  Location: "123 Industrial Park, Austin, TX"
  TimeZone: "America/Chicago"
  AutoInferenceEnabled: true
  InferenceSchedule: 02:00:00 (daily at 2 AM)
  AutoRetrainingEnabled: true
  RetrainingFrequencyDays: 30
  PowerBiEnabled: true
```

---

### 4. UserSiteAccess

**Schema**: `core.UserSiteAccess`

**Purpose**: Junction table that defines which users can access which sites and with what permissions. Implements site-level access control.

**Key Fields**:
```csharp
// Composite primary key: (UserId, SiteId)
public int UserId { get; set; }                      // FK to User
public required User User { get; set; }              // Navigation property
public int SiteId { get; set; }                      // FK to Site
public required Site Site { get; set; }              // Navigation property
public SiteRole RoleOnSite { get; set; }             // Viewer, Operator, SiteAdmin
public DateTime GrantedAt { get; set; }              // When access was granted
```

**Enums**:
```csharp
public enum SiteRole
{
    Viewer,      // Read-only: View dashboards, predictions
    Operator,    // Upload data, view dashboards, request predictions
    SiteAdmin    // Full control: Manage rules, retrain models, manage users
}
```

**Business Rules**:
- A user can have access to multiple sites with different roles
- When a user is deleted, all their access records are deleted (cascade)
- When a site is deleted, all access records for that site are deleted (cascade)
- Same user can be Admin on Site A, Operator on Site B, and Viewer on Site C

**Example**:
```
User: John (Plant Manager)
  Access:
    - Site A: SiteAdmin (full control of Plant A)
    - Site B: Viewer (read-only access to Plant B)

User: Jane (Operations Director)
  Access:
    - Site A: SiteAdmin
    - Site B: SiteAdmin
    - Site C: SiteAdmin

User: Bob (Consultant - temporary)
  Access:
    - Site A: Viewer (expires after project)
```

---

## Configuration Entities

### 5. SiteDataSchema

**Schema**: `config.SiteDataSchemas`

**Purpose**: Defines the data structure (columns, data types, validation rules) for each site. Supports schema evolution over time as sites add new measurements or change their data collection process.

**Key Fields**:
```csharp
public int Id { get; set; }                          // Primary key
public int SiteId { get; set; }                      // FK to Site
public required Site Site { get; set; }              // Navigation property
public decimal VersionNumber { get; set; }           // e.g., 1.0, 1.1, 2.0
public required string SchemaDefinition { get; set; }// JSONB: column definitions
public DateTime EffectiveFrom { get; set; }          // When this schema became active
public string? ChangeDescription { get; set; }       // e.g., "Added VFA measurement" (max 1000 chars)

// Audit fields
public int? CreatedById { get; set; }                // FK to User (nullable)
public User? CreatedBy { get; set; }                 // Who created this schema version
public DateTime CreatedAt { get; set; }
```

**SchemaDefinition Example**:
```json
{
  "version": "1.1",
  "description": "Added VFA measurement for improved predictions",
  "columns": [
    {
      "name": "Temperature",
      "displayName": "Reactor Temperature",
      "dataType": "decimal",
      "unit": "°C",
      "required": true,
      "minValue": 0,
      "maxValue": 100,
      "decimalPlaces": 1
    },
    {
      "name": "pH",
      "displayName": "pH Level",
      "dataType": "decimal",
      "unit": "",
      "required": true,
      "minValue": 0,
      "maxValue": 14,
      "decimalPlaces": 2
    },
    {
      "name": "FlowRate",
      "displayName": "Flow Rate",
      "dataType": "decimal",
      "unit": "L/hr",
      "required": true,
      "minValue": 0,
      "decimalPlaces": 1
    },
    {
      "name": "COD",
      "displayName": "Chemical Oxygen Demand",
      "dataType": "decimal",
      "unit": "mg/L",
      "required": true,
      "minValue": 0
    },
    {
      "name": "VFA",
      "displayName": "Volatile Fatty Acids",
      "dataType": "decimal",
      "unit": "mg/L",
      "required": false,
      "minValue": 0,
      "description": "Added in v1.1 - monthly lab measurement"
    }
  ]
}
```

**Business Rules**:
- Each site can have multiple schema versions (v1.0, v1.1, v2.0)
- DataRows reference a specific schema version (SchemaVersionId)
- When a schema is deleted, DataRows keep their data but SchemaVersionId becomes null (SetNull)
- VersionNumber uses decimal for precision (18,2)
- When a site is deleted, all its schemas are deleted (cascade)

**Version History Example**:
```
Site: Acme Plant A

Schema v1.0 (2024-01-01):
  Columns: Temperature, pH, FlowRate, COD
  DataRows using this: Jan 1 - Mar 31, 2024 (90 days)

Schema v1.1 (2024-04-01):
  Columns: Temperature, pH, FlowRate, COD, VFA
  ChangeDescription: "Added VFA measurement for better predictions"
  DataRows using this: Apr 1 - Present
```

---

### 6. DataCleaningRule

**Schema**: `config.DataCleaningRules`

**Purpose**: Defines automated data cleaning and transformation rules applied to incoming data. Fixes common data quality issues (missing values, outliers, noise) before storage or model training.

**Key Fields**:
```csharp
public int Id { get; set; }                          // Primary key
public int SiteId { get; set; }                      // FK to Site
public required Site Site { get; set; }              // Navigation property
public CleaningRuleType RuleType { get; set; }       // FillMissing, RemoveOutliers, Smooth, etc.
public required string ConfigJson { get; set; }      // JSONB: rule-specific configuration
public int Priority { get; set; }                    // Execution order (1, 2, 3...)
public bool IsActive { get; set; }                   // Enable/disable rule
public decimal VersionNumber { get; set; }           // Rule version (18,2 precision)

// Audit fields
public int? CreatedById { get; set; }                // FK to User (nullable)
public User? CreatedBy { get; set; }
public DateTime CreatedAt { get; set; }
public DateTime UpdatedAt { get; set; }
```

**Enums**:
```csharp
public enum CleaningRuleType
{
    FillMissing,       // Fill missing values (forward fill, backward fill, mean, median)
    RemoveOutliers,    // Remove or cap outlier values (IQR, Z-score, percentile)
    Smooth,            // Smooth noisy data (moving average, exponential smoothing)
    UnitConversion,    // Convert units (Fahrenheit to Celsius, etc.)
    Custom             // Custom transformation logic
}
```

**ConfigJson Examples**:

**1. Fill Missing Values:**
```json
{
  "ruleType": "FillMissing",
  "column": "Temperature",
  "method": "ForwardFill",
  "maxGap": 3,
  "description": "Use last known value if gap is ≤ 3 hours"
}
```

**2. Remove Outliers:**
```json
{
  "ruleType": "RemoveOutliers",
  "column": "pH",
  "method": "IQR",
  "multiplier": 1.5,
  "action": "cap",
  "description": "Cap pH outliers to IQR boundaries"
}
```

**3. Smooth Noisy Data:**
```json
{
  "ruleType": "Smooth",
  "column": "FlowRate",
  "method": "MovingAverage",
  "windowSize": 5,
  "description": "5-point moving average to reduce sensor noise"
}
```

**4. Unit Conversion:**
```json
{
  "ruleType": "UnitConversion",
  "column": "Temperature",
  "fromUnit": "Fahrenheit",
  "toUnit": "Celsius",
  "formula": "(value - 32) * 5/9"
}
```

**Business Rules**:
- Rules execute in priority order (1 → 2 → 3...)
- Rules can be temporarily disabled (IsActive = false)
- When a site is deleted, all its cleaning rules are deleted (cascade)
- When rule creator is deleted, rule remains with CreatedById = null (SetNull)

**Execution Example**:
```
Incoming data: Temperature = null, pH = 20, FlowRate = 1000

Priority 1 (FillMissing): Temperature null → 35.5 (last value)
Priority 2 (RemoveOutliers): pH 20 → 14 (cap to max)
Priority 3 (RemoveOutliers): FlowRate 1000 → 500 (cap to IQR)
Priority 4 (Smooth): FlowRate 500 → 480 (5-point average)

Final cleaned data: Temperature = 35.5, pH = 14, FlowRate = 480
```

---

### 7. DataValidationRule

**Schema**: `config.DataValidationRules`

**Purpose**: Defines quality gates that incoming data must pass before being accepted into the system. Prevents bad data from entering the database.

**Key Fields**:
```csharp
public int Id { get; set; }                          // Primary key
public int SiteId { get; set; }                      // FK to Site
public required Site Site { get; set; }              // Navigation property
public required string ColumnName { get; set; }      // Which column to validate (max 200 chars)
public ValidationRuleType RuleType { get; set; }     // Required, Range, DataType, Format, etc.
public required string ConfigJson { get; set; }      // JSONB: rule-specific configuration
public int Priority { get; set; }                    // Validation order (1, 2, 3...)
public bool IsActive { get; set; }                   // Enable/disable rule

// Audit fields
public int? CreatedById { get; set; }                // FK to User (nullable)
public User? CreatedBy { get; set; }
public DateTime CreatedAt { get; set; }
```

**Enums**:
```csharp
public enum ValidationRuleType
{
    Required,         // Field must not be null/empty
    Range,            // Value must be within min/max
    DataType,         // Must match expected type (int, decimal, DateTime)
    Format,           // Must match regex pattern
    Enum,             // Must be one of allowed values
    Custom            // Custom validation logic
}
```

**ConfigJson Examples**:

**1. Required Field:**
```json
{
  "ruleType": "Required",
  "column": "Temperature",
  "errorMessage": "Temperature is required for all records"
}
```

**2. Range Check:**
```json
{
  "ruleType": "Range",
  "column": "pH",
  "min": 0,
  "max": 14,
  "errorMessage": "pH must be between 0 and 14"
}
```

**3. Data Type:**
```json
{
  "ruleType": "DataType",
  "column": "Date",
  "expectedType": "DateTime",
  "format": "yyyy-MM-dd",
  "errorMessage": "Date must be in format YYYY-MM-DD"
}
```

**4. Format (Regex):**
```json
{
  "ruleType": "Format",
  "column": "SampleID",
  "pattern": "^SAMPLE-\\d{6}$",
  "errorMessage": "Sample ID must be in format SAMPLE-123456"
}
```

**5. Enum (Allowed Values):**
```json
{
  "ruleType": "Enum",
  "column": "ReactorStatus",
  "allowedValues": ["Running", "Stopped", "Maintenance"],
  "errorMessage": "Invalid reactor status"
}
```

**6. Cross-Column Validation:**
```json
{
  "ruleType": "Custom",
  "expression": "StartTime < EndTime",
  "errorMessage": "Start time must be before end time"
}
```

**Business Rules**:
- Validation runs BEFORE cleaning rules
- If validation fails, upload is rejected with specific error messages
- Rules execute in priority order (1 → 2 → 3...)
- When a site is deleted, all its validation rules are deleted (cascade)
- When rule creator is deleted, rule remains with CreatedById = null (SetNull)

**Upload Workflow**:
```
1. User uploads daily_data.xlsx
   ↓
2. System parses file → 365 rows
   ↓
3. Validate against DataValidationRules:
   Row 5: pH = 20 ❌ (exceeds max of 14)
   Row 12: Temperature = null ❌ (required field missing)
   ↓
4. Reject upload with error report:
   "Upload failed. Fix these errors:
    - Row 5: pH value 20 exceeds maximum of 14
    - Row 12: Temperature is required but missing"
   ↓
5. User fixes errors and re-uploads
   ↓
6. Validation passes ✅
   ↓
7. Apply DataCleaningRules
   ↓
8. Store cleaned data in DataRow table
```

---

### 8. Dashboard

**Schema**: `config.Dashboards`

**Purpose**: Stores custom dashboard configurations created by users. Allows users to save and share visualizations (Plotly charts) for quick access to frequently used views.

**Key Fields**:
```csharp
public int Id { get; set; }                          // Primary key
public int SiteId { get; set; }                      // FK to Site
public required Site Site { get; set; }              // Navigation property
public required string Name { get; set; }            // Dashboard name (max 200 chars)
public string? Description { get; set; }             // Dashboard description (max 1000 chars)
public required string PlotlyConfigJson { get; set; }// JSONB: Plotly chart configuration
public bool IsPublic { get; set; }                   // Share with all site users?

// Audit fields
public int? CreatedById { get; set; }                // FK to User (nullable)
public User? CreatedBy { get; set; }
public DateTime CreatedAt { get; set; }
public DateTime? LastViewedAt { get; set; }          // Usage tracking
```

**PlotlyConfigJson Example**:
```json
{
  "layout": "grid-2x2",
  "title": "Daily Production Overview",
  "charts": [
    {
      "id": "chart1",
      "type": "line",
      "title": "Biogas Production Over Time",
      "xAxis": {
        "column": "Date",
        "label": "Date"
      },
      "yAxis": {
        "column": "BiogasProduction",
        "label": "Biogas Production (L/day)"
      },
      "filters": {
        "dateRange": "last30days"
      },
      "style": {
        "color": "#1f77b4",
        "lineWidth": 2
      }
    },
    {
      "id": "chart2",
      "type": "scatter",
      "title": "pH vs Methane Content",
      "xAxis": {
        "column": "pH",
        "label": "pH"
      },
      "yAxis": {
        "column": "MethaneContent",
        "label": "Methane Content (%)"
      },
      "trendline": true
    },
    {
      "id": "chart3",
      "type": "bar",
      "title": "Monthly COD Removal Rate",
      "xAxis": {
        "column": "Month",
        "label": "Month"
      },
      "yAxis": {
        "column": "CODRemovalRate",
        "label": "COD Removal Rate (%)"
      },
      "aggregation": "average"
    },
    {
      "id": "chart4",
      "type": "heatmap",
      "title": "Temperature vs Flow Rate",
      "xAxis": {
        "column": "Temperature",
        "label": "Temperature (°C)"
      },
      "yAxis": {
        "column": "FlowRate",
        "label": "Flow Rate (L/hr)"
      },
      "colorScale": "Viridis"
    }
  ]
}
```

**Business Rules**:
- Personal dashboards (IsPublic = false): Only creator can view
- Public dashboards (IsPublic = true): All users with site access can view
- When a site is deleted, all its dashboards are deleted (cascade)
- When creator is deleted, dashboard remains with CreatedById = null (SetNull)
- LastViewedAt tracks dashboard usage for analytics

**Example Use Cases**:

**1. Personal Dashboard:**
```
User: John
Dashboard: "My Daily Checks"
  IsPublic: false
  Description: "Quick morning review of key metrics"
  Charts: [Temperature trend, pH status, Production summary]
```

**2. Team Dashboard:**
```
User: Admin
Dashboard: "Operations Overview"
  IsPublic: true
  Description: "Real-time KPIs for all team members"
  Charts: [Production, Quality, Efficiency, Alerts]
```

**3. Scenario Analysis Dashboard:**
```
User: Engineer
Dashboard: "pH Optimization Scenarios"
  IsPublic: false
  Description: "Compare baseline vs. high pH scenarios"
  Charts: [Side-by-side scenario comparisons]
```

---

## Data Entities

### 9. DataRow

**Schema**: `data.DataRows`

**Purpose**: Stores individual rows of operational data from SCADA systems, lab tests, and manual readings. This is the core dataset used for ML model training, predictions, and dashboard visualizations.

**Key Fields**:
```csharp
public int Id { get; set; }                          // Primary key
public int SiteId { get; set; }                      // FK to Site
public required Site Site { get; set; }              // Navigation property
public int? SchemaVersionId { get; set; }            // FK to SiteDataSchema (nullable)
public SiteDataSchema? SchemaVersion { get; set; }   // Which schema version defines this data
public DateTime Date { get; set; }                   // Data timestamp (daily or hourly)

// Flexible JSONB storage
public required string SensorDataJson { get; set; }  // All sensor/lab measurements

// Audit
public DateTime CreatedAt { get; set; }              // When row was added to database
```

**SensorDataJson Example (Daily Aggregated)**:
```json
{
  "Temperature_Avg": 35.5,
  "Temperature_Min": 33.2,
  "Temperature_Max": 37.8,
  "Temperature_StdDev": 1.2,
  "pH_Avg": 7.2,
  "pH_Min": 7.0,
  "pH_Max": 7.4,
  "FlowRate_Avg": 450,
  "FlowRate_Total": 10800,
  "COD_Inlet": 5000,
  "COD_Outlet": 750,
  "COD_RemovalRate": 85.0,
  "VFA": 250,
  "BiogasProduction_Total": 1250.5,
  "MethaneContent_Avg": 62.3,
  "ReactorVolume": 1000
}
```

**Business Rules**:
- JSONB storage provides flexibility for different site schemas
- SchemaVersionId can be null if schema was deleted (preserves historical data)
- When a site is deleted, all its data rows are deleted (cascade)
- When a schema version is deleted, data rows keep their data but SchemaVersionId becomes null (SetNull)
- Date field is indexed for fast time-series queries

**Example Query Use Cases**:
```sql
-- Get last 30 days of data for training
SELECT * FROM data.DataRows 
WHERE SiteId = 123 AND Date >= NOW() - INTERVAL '30 days'
ORDER BY Date;

-- Get data for specific date range
SELECT * FROM data.DataRows 
WHERE SiteId = 123 
  AND Date BETWEEN '2024-01-01' AND '2024-12-31';

-- Get latest reading
SELECT * FROM data.DataRows 
WHERE SiteId = 123 
ORDER BY Date DESC 
LIMIT 1;
```

---

### 10. Upload

**Schema**: `data.Uploads`

**Purpose**: Tracks every file upload from users. Provides audit trail, error tracking, and data lineage.

**Key Fields**:
```csharp
public int Id { get; set; }                          // Primary key
public int SiteId { get; set; }                      // FK to Site
public required Site Site { get; set; }              // Navigation property
public int? UserId { get; set; }                     // FK to User (nullable)
public User? User { get; set; }                      // Who uploaded the file
public DateTime UploadedAt { get; set; }             // Upload timestamp
public required string FileName { get; set; }        // Original filename (max 500 chars)
public int RowCount { get; set; }                    // Number of rows in file
public UploadValidationStatus ValidationStatus { get; set; } // Pending, Processing, Completed, Failed
```

**Enums**:
```csharp
public enum UploadValidationStatus
{
    Pending,      // Upload received, not yet processed
    Processing,   // Currently validating and cleaning data
    Completed,    // Successfully processed and stored
    Failed        // Validation or processing failed
}
```

**Business Rules**:
- When a site is deleted, all its uploads are deleted (cascade)
- When uploader is deleted, upload record remains with UserId = null (SetNull)
- Successful uploads trigger data cleaning rules and storage to DataRow table

**Upload Workflow**:
```
1. User uploads "daily_data_2024-12-08.xlsx"
   → Create Upload record
   Status: Pending
   FileName: "daily_data_2024-12-08.xlsx"
   RowCount: 365

2. System starts processing
   → Update Status: Processing

3a. If validation succeeds:
    → Apply DataCleaningRules
    → Store 365 DataRow records
    → Update Status: Completed
    → User sees: "Upload successful! 365 rows processed."

3b. If validation fails:
    → Update Status: Failed
    → User sees: "Upload failed. Fix rows 5, 12, 25 and re-upload."
```

**Example Records**:
```
Upload 1:
  FileName: "daily_data_2024-12-08.xlsx"
  RowCount: 365
  Status: Completed
  UploadedAt: 2024-12-08 08:30:00
  UserId: 123 (John)

Upload 2:
  FileName: "monthly_data_2024-11.xlsx"
  RowCount: 30
  Status: Failed
  UploadedAt: 2024-12-08 09:15:00
  UserId: 456 (Jane)
```

---

## ML Entities

### 11. ModelVersion

**Schema**: `ml.ModelVersions`

**Purpose**: Tracks trained machine learning models for each site, including storage location, performance metrics, and training metadata. Acts as an ML model registry.

**Key Fields**:
```csharp
public int Id { get; set; }                          // Primary key
public int SiteId { get; set; }                      // FK to Site
public required Site Site { get; set; }              // Navigation property
public required string BlobStoragePath { get; set; } // Azure Blob Storage path (max 1000 chars)
public DateTime? TrainedAt { get; set; }             // When model was trained
public DateTime? TrainingDataStart { get; set; }     // Training data start date
public DateTime? TrainingDataEnd { get; set; }       // Training data end date
public string? MetricsJson { get; set; }             // JSONB: model performance metrics
public bool IsActive { get; set; }                   // Is this the active model for predictions?
public decimal VersionNumber { get; set; }           // Model version: 1.0, 1.1, 2.0 (18,2 precision)
```

**BlobStoragePath Example**:
```
"models/site_123/model_v2.0.pkl"
"models/site_123/model_v2.1.onnx"
"models/site_456/xgboost_model_v1.0.pkl"
```

**MetricsJson Example**:
```json
{
  "training": {
    "mae": 0.15,                    // Mean Absolute Error
    "rmse": 0.22,                   // Root Mean Squared Error
    "r2": 0.89,                     // R-squared
    "mape": 8.5,                    // Mean Absolute Percentage Error
    "trainingTime": 45,             // seconds
    "dataPoints": 730               // 2 years of daily data
  },
  "validation": {
    "mae": 0.18,
    "rmse": 0.25,
    "r2": 0.86
  },
  "featureImportance": {
    "Temperature": 0.25,
    "pH": 0.22,
    "FlowRate": 0.20,
    "COD": 0.18,
    "VFA": 0.15
  },
  "hyperparameters": {
    "algorithm": "XGBoost",
    "max_depth": 5,
    "learning_rate": 0.1,
    "n_estimators": 100
  }
}
```

**Business Rules**:
- Each site can have multiple model versions
- Only ONE model per site can be IsActive = true (used for predictions)
- When a site is deleted, all its models are deleted (cascade)
- When model is deleted, TrainingJobs and PredictionResults referencing it have ModelVersionId set to null (SetNull)
- VersionNumber uses decimal for precision

**Model Version Timeline Example**:
```
Site: Acme Plant A

ModelVersion 1.0
  TrainedAt: 2024-01-15 02:00:00
  TrainingData: 2022-01-01 to 2024-01-01 (2 years)
  Metrics: {"mae": 0.20, "r2": 0.85}
  IsActive: false (replaced)
  BlobPath: "models/site_123/model_v1.0.pkl"

ModelVersion 1.1
  TrainedAt: 2024-02-15 02:00:00
  TrainingData: 2022-01-01 to 2024-02-01 (1 month more data)
  Metrics: {"mae": 0.18, "r2": 0.87}
  IsActive: false (replaced)
  BlobPath: "models/site_123/model_v1.1.pkl"

ModelVersion 2.0
  TrainedAt: 2024-03-15 02:00:00
  TrainingData: 2022-01-01 to 2024-03-01
  Metrics: {"mae": 0.15, "r2": 0.89}
  IsActive: true ← Currently used for predictions
  BlobPath: "models/site_123/model_v2.0.onnx"
```

**Model Activation Flow**:
```
1. TrainingJob completes → creates ModelVersion 2.0
2. Compare metrics: v2.0 (mae=0.15) vs v1.1 (mae=0.18)
3. v2.0 is better → Set IsActive:
   - v1.1.IsActive = false
   - v2.0.IsActive = true
4. All future predictions now use ModelVersion 2.0
```

---

### 12. TrainingJob

**Schema**: `ml.TrainingJobs`

**Purpose**: Tracks background jobs that retrain ML models. Monitors job status, logs, and results for debugging and performance tracking.

**Key Fields**:
```csharp
public int Id { get; set; }                          // Primary key
public int SiteId { get; set; }                      // FK to Site
public required Site Site { get; set; }              // Navigation property
public DateTime ScheduledAt { get; set; }            // When job was scheduled
public DateTime? StartedAt { get; set; }             // When training actually started
public DateTime? CompletedAt { get; set; }           // When training finished
public TrainingJobStatus Status { get; set; }        // Scheduled, Running, Completed, Failed
public DateTime TrainingDataStart { get; set; }      // Training data start date
public DateTime TrainingDataEnd { get; set; }        // Training data end date
public int? ModelVersionId { get; set; }             // FK to ModelVersion (result, nullable)
public ModelVersion? ModelVersion { get; set; }      // Navigation property
public required string ConfigJson { get; set; }      // JSONB: training hyperparameters
public int? TriggeredById { get; set; }              // FK to User (nullable)
public User? TriggeredBy { get; set; }               // Who triggered this job
```

**Enums**:
```csharp
public enum TrainingJobStatus
{
    Scheduled,    // Job scheduled, not yet started
    Running,      // Currently training
    Completed,    // Training completed successfully
    Failed        // Training failed with errors
}
```

**ConfigJson Example**:
```json
{
  "algorithm": "XGBoost",
  "hyperparameters": {
    "max_depth": 5,
    "learning_rate": 0.1,
    "n_estimators": 100,
    "min_child_weight": 1,
    "subsample": 0.8,
    "colsample_bytree": 0.8
  },
  "features": [
    "Temperature",
    "pH",
    "FlowRate",
    "COD",
    "VFA"
  ],
  "target": "BiogasProduction",
  "validation": {
    "method": "TimeSeriesSplit",
    "n_splits": 5
  },
  "earlyStoppingRounds": 10
}
```

**Business Rules**:
- When a site is deleted, all its training jobs are deleted (cascade)
- When triggering user is deleted, job remains with TriggeredById = null (SetNull)
- When created model is deleted, job remains with ModelVersionId = null (SetNull)
- Duration = CompletedAt - StartedAt (typically 30-120 seconds)

**Training Job Lifecycle**:

**Scheduled Training (Automated)**:
```
1. Scheduler runs every Sunday at 2 AM
   → Create TrainingJob
   Status: Scheduled
   ScheduledAt: 2024-12-08 02:00:00
   TriggeredById: null (automated)

2. Training Container picks up job
   → Update Status: Running
   StartedAt: 2024-12-08 02:00:15

3. Training executes (75 seconds)
   → Load DataRows (2022-01-01 to 2024-12-07)
   → Apply feature engineering
   → Train XGBoost model
   → Evaluate performance
   → Save model to Blob Storage

4. Create ModelVersion 2.1
   → Update TrainingJob:
   Status: Completed
   CompletedAt: 2024-12-08 02:01:30
   ModelVersionId: 456
   LogsJson: {training logs}

5. Dashboard shows: "Training completed. Model v2.1 now active."
```

**Manual Training**:
```
1. User clicks "Retrain Model" button
   → Create TrainingJob
   Status: Scheduled
   TriggeredById: 123 (User John)

2. Job executes...

3. If successful:
   Status: Completed
   ModelVersionId: 789

4. If failed:
   Status: Failed
   ModelVersionId: null
```

---

### 13. InferenceRequest

**Schema**: `ml.InferenceRequests`

**Purpose**: Tracks every prediction request made to the system. Provides audit trail, performance monitoring, and debugging capability.

**Key Fields**:
```csharp
public int Id { get; set; }                          // Primary key
public int SiteId { get; set; }                      // FK to Site
public required Site Site { get; set; }              // Navigation property
public int? UserId { get; set; }                     // FK to User (nullable)
public User? User { get; set; }                      // Who requested prediction
public DateTime RequestedAt { get; set; }            // Request timestamp
public InferenceRequestStatus Status { get; set; }   // Pending, Running, Completed, Failed
public required string InputDataJson { get; set; }   // JSONB: input data for prediction
public int? PredictionResultId { get; set; }         // FK to PredictionResult (nullable)
public PredictionResult? PredictionResult { get; set; } // Navigation property
public DateTime? CompletedAt { get; set; }           // When prediction finished
public long? DurationMs { get; set; }                // Prediction duration in milliseconds
public string? ErrorMessage { get; set; }            // Error details if failed (max 2000 chars)
```

**Enums**:
```csharp
public enum InferenceRequestStatus
{
    Pending,      // Request received, not yet processed
    Running,      // Currently running prediction
    Completed,    // Prediction completed successfully
    Failed        // Prediction failed
}
```

**InputDataJson Example**:
```json
{
  "Date": "2024-12-08",
  "Temperature": 35.5,
  "pH": 7.2,
  "FlowRate": 450,
  "COD": 5000,
  "VFA": 250,
  "ReactorVolume": 1000
}
```

**Business Rules**:
- When a site is deleted, all its inference requests are deleted (cascade)
- When requesting user is deleted, request remains with UserId = null (SetNull)
- When prediction result is deleted, request remains with PredictionResultId = null (SetNull)
- DurationMs = (CompletedAt - RequestedAt).TotalMilliseconds (typically 2000-10000ms)

**Request Types**:

**1. Dashboard Request (User-Initiated)**:
```
User clicks "Predict Today's Output"
  ↓
Create InferenceRequest:
  SiteId: 123
  UserId: 456 (John)
  Status: Pending
  InputDataJson: {today's sensor readings}
  RequestedAt: 2024-12-08 10:30:00
  ↓
Inference Container processes (5 seconds)
  ↓
Update InferenceRequest:
  Status: Completed
  PredictionResultId: 789
  CompletedAt: 2024-12-08 10:30:05
  DurationMs: 5000
  ↓
Display prediction in dashboard
```

**2. Automated Daily Inference**:
```
Scheduler runs every night at midnight
  ↓
For each site with new data:
  Create InferenceRequest:
    SiteId: 123
    UserId: null (automated)
    Status: Pending
    InputDataJson: {latest data}
  ↓
Inference runs → Status: Completed
  ↓
Store result → Update PredictionResultId
```

**3. Failed Request**:
```
User requests prediction
  ↓
Status: Running
  ↓
Error: No active model found
  ↓
Status: Failed
ErrorMessage: "No active model found for Site 123. Please train a model first."
DurationMs: null
PredictionResultId: null
```

**Performance Monitoring**:
```sql
-- Average inference duration per site
SELECT SiteId, AVG(DurationMs) as AvgDurationMs
FROM ml.InferenceRequests
WHERE Status = 'Completed'
GROUP BY SiteId;

-- Daily inference volume
SELECT DATE(RequestedAt) as Date, COUNT(*) as Requests
FROM ml.InferenceRequests
GROUP BY DATE(RequestedAt)
ORDER BY Date DESC;
```

---

### 14. PredictionResult

**Schema**: `ml.PredictionResults`

**Purpose**: Stores actual prediction outputs from ML models. Supports both automated daily predictions and user-driven scenario analysis ("what-if" simulations).

**Key Fields**:
```csharp
public int Id { get; set; }                          // Primary key
public int SiteId { get; set; }                      // FK to Site
public required Site Site { get; set; }              // Navigation property
public DateTime CreatedAt { get; set; }              // When prediction was made
public int? ModelVersionId { get; set; }             // FK to ModelVersion (nullable)
public ModelVersion? ModelVersion { get; set; }      // Which model made prediction
public required string ScenarioName { get; set; }    // "Baseline", "High pH Test" (max 200 chars)
public string? ScenarioDescription { get; set; }     // Description of scenario (max 1000 chars)
public string? ScenarioParametersJson { get; set; }  // JSONB: user-adjusted parameters
public required string InputDataJson { get; set; }   // JSONB: actual input used
public required string PredictionOutputJson { get; set; } // JSONB: model predictions
public PredictionStatus Status { get; set; }         // Success, Failed
public int? UserId { get; set; }                     // FK to User (nullable)
public User? User { get; set; }                      // Who requested prediction
```

**Enums**:
```csharp
public enum PredictionStatus
{
    Success,    // Prediction completed successfully
    Failed      // Prediction failed
}
```

**Example 1: Daily Automated Prediction**:
```json
{
  "Id": 1,
  "SiteId": 123,
  "CreatedAt": "2024-12-08T00:15:00Z",
  "ModelVersionId": 456,
  "ScenarioName": "Daily Prediction",
  "ScenarioDescription": null,
  "ScenarioParametersJson": null,
  "InputDataJson": {
    "Date": "2024-12-08",
    "Temperature": 35.5,
    "pH": 7.2,
    "FlowRate": 450,
    "COD": 5000,
    "VFA": 250
  },
  "PredictionOutputJson": {
    "BiogasProduction": 1250.5,
    "MethaneContent": 62.3,
    "CODRemovalRate": 85.2,
    "Confidence": 0.89,
    "PredictionInterval": {
      "BiogasProduction_Lower": 1180.0,
      "BiogasProduction_Upper": 1320.0,
      "MethaneContent_Lower": 60.1,
      "MethaneContent_Upper": 64.5
    }
  },
  "Status": "Success",
  "UserId": null
}
```

**Example 2: User Scenario Analysis ("What-If")**:
```json
{
  "Id": 2,
  "SiteId": 123,
  "CreatedAt": "2024-12-08T10:30:00Z",
  "ModelVersionId": 456,
  "ScenarioName": "High pH Test",
  "ScenarioDescription": "What if we increase pH from 7.2 to 8.0?",
  "ScenarioParametersJson": {
    "changedParameters": {
      "pH": {
        "original": 7.2,
        "modified": 8.0,
        "percentChange": 11.1
      }
    },
    "keptParameters": {
      "Temperature": 35.5,
      "FlowRate": 450,
      "COD": 5000,
      "VFA": 250
    }
  },
  "InputDataJson": {
    "Temperature": 35.5,
    "pH": 8.0,
    "FlowRate": 450,
    "COD": 5000,
    "VFA": 250
  },
  "PredictionOutputJson": {
    "BiogasProduction": 1320.8,
    "MethaneContent": 64.1,
    "CODRemovalRate": 87.5,
    "Confidence": 0.85,
    "PredictionInterval": {
      "BiogasProduction_Lower": 1250.0,
      "BiogasProduction_Upper": 1390.0
    }
  },
  "Status": "Success",
  "UserId": 789
}
```

**Example 3: Multi-Scenario Comparison**:
```
User creates 3 scenarios to compare:

Scenario 1: "Baseline"
  pH: 7.2 (current)
  → BiogasProduction: 1250.5

Scenario 2: "High pH"
  pH: 8.0 (increased)
  → BiogasProduction: 1320.8 (+5.6%)

Scenario 3: "Low Temperature"
  Temperature: 30.0 (decreased from 35.5)
  → BiogasProduction: 1150.0 (-8.0%)

Dashboard displays side-by-side comparison chart
```

**Business Rules**:
- When a site is deleted, all its prediction results are deleted (cascade)
- When predicting user is deleted, result remains with UserId = null (SetNull)
- When model version is deleted, result remains with ModelVersionId = null (SetNull)
- ScenarioName is required (defaults to "Daily Prediction" for automated runs)
- PredictionOutputJson structure depends on model output schema

**Query Use Cases**:
```sql
-- Get prediction history for site
SELECT * FROM ml.PredictionResults
WHERE SiteId = 123
ORDER BY CreatedAt DESC
LIMIT 30;

-- Compare scenarios by user
SELECT ScenarioName, 
       PredictionOutputJson->>'BiogasProduction' as Production
FROM ml.PredictionResults
WHERE UserId = 789 AND CreatedAt >= NOW() - INTERVAL '1 day'
ORDER BY CreatedAt;

-- Model performance tracking
SELECT ModelVersionId,
       AVG((PredictionOutputJson->>'Confidence')::decimal) as AvgConfidence,
       COUNT(*) as PredictionCount
FROM ml.PredictionResults
WHERE Status = 'Success'
GROUP BY ModelVersionId;
```

---

## Entity Relationships

### Relationship Diagram

```
Company (1) ──────── (N) Site
   │                     │
   │                     ├── (N) SiteDataSchema
   │                     ├── (N) DataCleaningRule
   │                     ├── (N) DataValidationRule
   │                     ├── (N) Dashboard
   │                     ├── (N) DataRow
   │                     ├── (N) Upload
   │                     ├── (N) ModelVersion
   │                     ├── (N) TrainingJob
   │                     ├── (N) InferenceRequest
   │                     └── (N) PredictionResult
   │
   └── (N) User
        │
        └── (N) UserSiteAccess (N) ──── Site

DataRow (N) ──── (1) SiteDataSchema [nullable]

TrainingJob (N) ──── (1) ModelVersion [nullable, result]
InferenceRequest (N) ──── (1) PredictionResult [nullable]
PredictionResult (N) ──── (1) ModelVersion [nullable]

User (1) ──── (N) SiteDataSchema [CreatedBy, nullable]
User (1) ──── (N) DataCleaningRule [CreatedBy, nullable]
User (1) ──── (N) DataValidationRule [CreatedBy, nullable]
User (1) ──── (N) Dashboard [CreatedBy, nullable]
User (1) ──── (N) Upload [nullable]
User (1) ──── (N) TrainingJob [TriggeredBy, nullable]
User (1) ──── (N) InferenceRequest [nullable]
User (1) ──── (N) PredictionResult [nullable]
```

### Key Relationship Patterns

**1. Multi-Tenancy (Company → Site → Data)**:
```
Company
  └── Site A
      ├── DataRows (millions)
      ├── ModelVersions (3-5)
      └── Users with access (5-10)
  └── Site B
      └── ...
```

**2. Audit Trail (User → Created Records)**:
```
User (John)
  ├── Created: SiteDataSchema v1.0
  ├── Created: DataCleaningRule (Fill Missing)
  ├── Created: Dashboard "Daily Overview"
  └── Uploaded: daily_data_2024-12-08.xlsx

[If User John is deleted, all records remain with CreatedBy = null]
```

**3. ML Pipeline (Data → Training → Model → Inference → Results)**:
```
DataRows (historical data)
  ↓
TrainingJob (train model)
  ↓
ModelVersion (saved model)
  ↓
InferenceRequest (predict)
  ↓
PredictionResult (output)
```

**4. Schema Evolution (SchemaVersion → DataRows)**:
```
SiteDataSchema v1.0 (Jan-Mar)
  └── DataRows: 90 records

SiteDataSchema v1.1 (Apr-Present)
  └── DataRows: 245 records

[If v1.0 is deleted, its 90 DataRows remain with SchemaVersionId = null]
```

---

## Delete Behaviors

### Delete Behavior Strategy

| Behavior | Use Case | Example |
|----------|----------|---------|
| **Cascade** | Owned data deletion | Company deleted → Sites deleted → DataRows deleted |
| **SetNull** | Preserve records, remove references | User deleted → Upload.UserId = null (upload record preserved) |
| **Restrict** | Not used | Would block deletions unnecessarily |

### Cascade Relationships (Parent Deleted → Children Deleted)

```
Company → Site
  Site → SiteDataSchema
  Site → DataCleaningRule
  Site → DataValidationRule
  Site → Dashboard
  Site → DataRow
  Site → Upload
  Site → ModelVersion
  Site → TrainingJob
  Site → InferenceRequest
  Site → PredictionResult

User → UserSiteAccess

UserSiteAccess → (cascade from both User and Site)
```

**Example**: Deleting Company "Acme Corp"
```
DELETE FROM core.Companies WHERE Id = 1;

Cascades to:
  └── Sites (3 sites)
      ├── DataRows (1,000,000 rows)
      ├── Uploads (500 uploads)
      ├── ModelVersions (10 models)
      ├── TrainingJobs (50 jobs)
      ├── InferenceRequests (10,000 requests)
      ├── PredictionResults (10,000 results)
      ├── Dashboards (20 dashboards)
      ├── SiteDataSchemas (5 schemas)
      ├── DataCleaningRules (15 rules)
      └── DataValidationRules (20 rules)
```

### SetNull Relationships (Preserve Records, Nullify FK)

```
User → SiteDataSchema.CreatedBy
User → DataCleaningRule.CreatedBy
User → DataValidationRule.CreatedBy
User → Dashboard.CreatedBy
User → Upload.User
User → TrainingJob.TriggeredBy
User → InferenceRequest.User
User → PredictionResult.User

SiteDataSchema → DataRow.SchemaVersion
ModelVersion → TrainingJob.ModelVersion
ModelVersion → PredictionResult.ModelVersion
PredictionResult → InferenceRequest.PredictionResult
```

**Example**: Deleting User "John"
```
DELETE FROM core.Users WHERE Id = 123;

Preserves records with CreatedBy = null:
  ├── SiteDataSchema v1.0 (CreatedBy: null)
  ├── DataCleaningRule "Fill Missing" (CreatedBy: null)
  ├── Dashboard "Daily Overview" (CreatedBy: null)
  └── Upload "daily_data.xlsx" (User: null)

UI displays: "Created by: [Deleted User]"
```

**Example**: Deleting SiteDataSchema v1.0
```
DELETE FROM config.SiteDataSchemas WHERE Id = 456;

Preserves historical data:
  └── DataRows (90 records)
      SchemaVersionId: null
      SensorDataJson: {...} ← Data still intact
      
UI displays: "Schema: [Deleted/Migrated]"
```

### Best Practices

1. **Cascade for Ownership**: Use when child cannot exist without parent
2. **SetNull for Audit Trails**: Use when historical context should be preserved
3. **Never Restrict**: Would create operational burden (can't delete anything)
4. **Soft Delete Alternative**: For critical audit records, consider soft delete (IsDeleted flag) instead of physical deletion

---

## Summary

| Entity | Schema | Purpose | Key Features |
|--------|--------|---------|--------------|
| **Company** | core | Multi-tenant root | Unique name, cascade deletes |
| **User** | core | System users | Email unique, supports individuals |
| **Site** | core | Production facilities | Per-site config, automation settings |
| **UserSiteAccess** | core | Access control | Composite key, role-based permissions |
| **SiteDataSchema** | config | Data structure definitions | Versioned, JSONB schema, evolution support |
| **DataCleaningRule** | config | Data transformation | Priority order, JSONB config |
| **DataValidationRule** | config | Quality gates | Priority order, detailed errors |
| **Dashboard** | config | Saved visualizations | Plotly config, public/private |
| **DataRow** | data | Operational data | JSONB storage, flexible schema |
| **Upload** | data | File upload tracking | Audit trail, error details |
| **ModelVersion** | ml | ML model registry | Blob storage, metrics, active flag |
| **TrainingJob** | ml | Training job tracking | Status, logs, duration |
| **InferenceRequest** | ml | Prediction requests | Audit trail, performance tracking |
| **PredictionResult** | ml | Prediction outputs | Scenarios, confidence, intervals |

---

**Document Version**: 1.0  
**Last Updated**: December 8, 2025  
**Database Schema Version**: 1.0  
**Total Entities**: 14
