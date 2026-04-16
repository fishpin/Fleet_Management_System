public class Billing
{
    public int Id { get; set; }

    public int ReservationId { get; set; }

    public DateTime IssuedDate { get; set; } = DateTime.Today;

    public decimal Tax { get; set; }
    public decimal ExtraCharges { get; set; }

    public decimal FinalAmount { get; set; }

    public bool IsPaid { get; set; } = false;

    public Reservation Reservation { get; set; }
    public ICollection<BillingLineItem> LineItems { get; set; } = new List<BillingLineItem>();
}