using System.ComponentModel.DataAnnotations;

public class Vehicle
{
    public int Id { get; set; }

    [Required]
    public string Make { get; set; }

    [Required]
    public string Model { get; set; }

    public int Year { get; set; }

    public decimal PricePerDay { get; set; }

    public bool IsAvailable { get; set; } = true;
}