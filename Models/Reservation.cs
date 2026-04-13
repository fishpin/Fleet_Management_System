using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Reservation
{
    public int Id { get; set; }

    [Required]
    public int VehicleId { get; set; }

    [Required]
    public int CustomerId { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public decimal TotalCost { get; set; }

    // Navigation Properties
    public Vehicle Vehicle { get; set; }
    public Customer Customer { get; set; }
}