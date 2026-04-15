using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class ReservationController : Controller
{
    private readonly AppDbContext _context;
    public ReservationController(AppDbContext context) { _context = context; }

    public async Task<IActionResult> Index()
    {
        var data = _context.Reservations.Include(r => r.Vehicle).Include(r => r.Customer);
        return View(await data.ToListAsync());
    }

    public IActionResult Create()
    {
        ViewBag.Vehicles = _context.Vehicles.ToList();
        ViewBag.Customers = _context.Customers.ToList();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Reservation r)
    {
        var vehicle = await _context.Vehicles.FindAsync(r.VehicleId);
        if (vehicle.IsAvailable)
        {
            vehicle.IsAvailable = false;
            var days = (r.EndDate - r.StartDate).Days;
            r.TotalCost = days * vehicle.PricePerDay;

            _context.Add(r);
            await _context.SaveChangesAsync();
        }
        
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var r = await _context.Reservations.Include(res => res.Vehicle).Include(res => res.Customer).FirstOrDefaultAsync(res => res.Id == id);
        if (r == null)
            return NotFound();

        ViewBag.Vehicles = _context.Vehicles.ToList();
        ViewBag.Customers = _context.Customers.ToList();
        return View(r);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, Reservation r)
    {
        if (id != r.Id)
            return NotFound();

        var vehicle = await _context.Vehicles.FindAsync(r.VehicleId);
        if (vehicle != null)
        {
            var days = (r.EndDate - r.StartDate).Days;
            r.TotalCost = days * vehicle.PricePerDay;
        }

        _context.Update(r);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var r = await _context.Reservations.Include(res => res.Vehicle).Include(res => res.Customer).FirstOrDefaultAsync(res => res.Id == id);
        if (r == null)
            return NotFound();
        return View(r);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var r = await _context.Reservations.FindAsync(id);
        if (r != null)
        {
            // Make vehicle available again
            var vehicle = await _context.Vehicles.FindAsync(r.VehicleId);
            if (vehicle != null)
            {
                vehicle.IsAvailable = true;
            }

            _context.Reservations.Remove(r);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}