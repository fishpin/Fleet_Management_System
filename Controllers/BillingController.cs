using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
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

    public async Task<IActionResult> Edit(int id)
    {
        var b = await _context.Billings.FindAsync(id);
        if (b == null)
            return NotFound();
        return View(b);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, Billing b)
    {
        if (id != b.Id)
            return NotFound();

        var reservation = await _context.Reservations.FindAsync(b.ReservationId);
        b.FinalAmount = reservation.TotalCost + b.Tax + b.ExtraCharges;

        _context.Update(b);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var b = await _context.Billings.FindAsync(id);
        if (b == null)
            return NotFound();
        return View(b);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var b = await _context.Billings.FindAsync(id);
        if (b != null)
        {
            _context.Billings.Remove(b);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}