using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class BillingController : Controller
{
    private readonly AppDbContext _context;
    public BillingController(AppDbContext context) { _context = context; }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Billings.Include(b => b.Reservation).ToListAsync());
    }

    public IActionResult Create() => View();

    [HttpPost]
    public async Task<IActionResult> Create(Billing b)
    {
        var reservation = await _context.Reservations.FindAsync(b.ReservationId);
        b.FinalAmount = reservation.TotalCost + b.Tax + b.ExtraCharges;

        _context.Add(b);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}