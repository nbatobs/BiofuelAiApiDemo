namespace Data.Models;
using Data.Models.Enums;

public class User
{
    public int Id { get; set; }

    public int? CompanyId { get; set; }
    public Company? Company { get; set; }

    public string? IdpSub { get; set; }
    public string? IdpIssuer { get; set; }
    public required string Email { get; set; }
    public string? Name { get; set; }
    public UserRole Role { get; set; }
    public bool IsIndividual { get; set; }    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastLogin { get; set; }
}
