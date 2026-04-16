using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// Handles billing operations: viewing, creating invoices, and managing line items.
[Authorize]
[AutoValidateAntiforgeryToken]
public class BillingController : Controller
{
    private readonly AppDbContext _context;
    public BillingController(AppDbContext context) { _context = context; }

    public async Task<IActionResult> Index()
    {
        var billings = await _context.Billings
            .Include(b => b.Reservation)
                .ThenInclude(r => r.Customer)
            .Include(b => b.Reservation)
                .ThenInclude(r => r.Vehicle)
            .OrderBy(b => b.IsPaid)
            .ThenByDescending(b => b.Id)
            .ToListAsync();
        return View(billings);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var b = await _context.Billings
            .Include(b => b.Reservation)
                .ThenInclude(r => r.Vehicle)
            .Include(b => b.Reservation)
                .ThenInclude(r => r.Customer)
            .Include(b => b.LineItems)
            .FirstOrDefaultAsync(b => b.Id == id);
        if (b == null) return NotFound();
        return View(b);
    }

    [HttpPost]
    public async Task<IActionResult> SetPaid(int id, bool paid)
    {
        var b = await _context.Billings.FindAsync(id);
        if (b == null) return NotFound();
        b.IsPaid = paid;
        await _context.SaveChangesAsync();
        return Json(new { isPaid = b.IsPaid });
    }

    [HttpPost]
    public async Task<IActionResult> AddLineItem(int billingId, string reason, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(reason) || amount <= 0)
            return BadRequest("A description and a positive amount are required.");

        var item = new BillingLineItem { BillingId = billingId, Reason = reason, Amount = amount };
        _context.BillingLineItems.Add(item);
        await _context.SaveChangesAsync();

        var billing = await _context.Billings
            .Include(b => b.Reservation)
            .Include(b => b.LineItems)
            .FirstOrDefaultAsync(b => b.Id == billingId);

        // Recalculate totals after the line item change.
        billing.ExtraCharges = billing.LineItems.Sum(i => i.Amount);
        billing.FinalAmount = billing.Reservation.TotalCost + billing.Tax + billing.ExtraCharges;
        await _context.SaveChangesAsync();

        var subtotal = billing.FinalAmount - billing.Tax;
        return Json(new {
            itemId = item.Id,
            reason = item.Reason,
            amount = item.Amount,
            amountFormatted = item.Amount.ToString("F2"),
            subtotal = subtotal.ToString("F2"),
            finalAmount = billing.FinalAmount.ToString("F2")
        });
    }

    [HttpPost]
    public async Task<IActionResult> RemoveLineItem(int id)
    {
        var item = await _context.BillingLineItems.FindAsync(id);
        if (item == null) return NotFound();

        int billingId = item.BillingId;
        _context.BillingLineItems.Remove(item);
        await _context.SaveChangesAsync();

        var billing = await _context.Billings
            .Include(b => b.Reservation)
            .Include(b => b.LineItems)
            .FirstOrDefaultAsync(b => b.Id == billingId);

        // Recalculate totals after the line item is removed.
        billing.ExtraCharges = billing.LineItems.Sum(i => i.Amount);
        billing.FinalAmount = billing.Reservation.TotalCost + billing.Tax + billing.ExtraCharges;
        await _context.SaveChangesAsync();

        var subtotal = billing.FinalAmount - billing.Tax;
        return Json(new { subtotal = subtotal.ToString("F2"), finalAmount = billing.FinalAmount.ToString("F2") });
    }

    public async Task<IActionResult> Create(int? reservationId)
    {
        ViewBag.Reservations = await _context.Reservations
            .Include(r => r.Vehicle)
            .Include(r => r.Customer)
            .Where(r => !_context.Billings.Any(b => b.ReservationId == r.Id))
            .OrderByDescending(r => r.StartDate)
            .ToListAsync();
        return View(new Billing { ReservationId = reservationId ?? 0 });
    }

    [HttpPost]
    public async Task<IActionResult> Create(Billing b)
    {
        if (b.ReservationId == 0)
        {
            ModelState.AddModelError(string.Empty, "Please select a reservation.");
            ViewBag.Reservations = await _context.Reservations
                .Include(r => r.Vehicle).Include(r => r.Customer)
                .Where(r => !_context.Billings.Any(bi => bi.ReservationId == r.Id))
                .OrderByDescending(r => r.StartDate)
                .ToListAsync();
            return View(b);
        }

        var reservation = await _context.Reservations.FindAsync(b.ReservationId);
        if (reservation == null) return NotFound();
        b.Tax = Math.Round(reservation.TotalCost * BusinessRules.TaxRate, 2);
        b.FinalAmount = reservation.TotalCost + b.Tax + b.ExtraCharges;

        _context.Add(b);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
