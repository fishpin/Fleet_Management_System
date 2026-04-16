public class BillingLineItem
{
    public int Id { get; set; }
    public int BillingId { get; set; }
    public string Reason { get; set; }
    public decimal Amount { get; set; }
    public Billing Billing { get; set; }
}
