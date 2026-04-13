public class Billing
{
    public int Id { get; set; }

    public int ReservationId { get; set; }

    public decimal Tax { get; set; }
    public decimal ExtraCharges { get; set; }

    public decimal FinalAmount { get; set; }

    public Reservation Reservation { get; set; }
}