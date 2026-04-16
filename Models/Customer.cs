using System.ComponentModel.DataAnnotations;

public class Customer
{
    public int Id { get; set; }

    [Required]
    public string FullName { get; set; }

    [EmailAddress]
    public string Email { get; set; }

    public string Phone { get; set; }

    public string? Address { get; set; }

    public string? LicenseNumber { get; set; }
}