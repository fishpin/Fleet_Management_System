using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// Handles CRUD operations for vehicles and the reports view.
[Authorize]
[AutoValidateAntiforgeryToken]
public class VehicleController : Controller
{
    private readonly AppDbContext _context;
    
    public VehicleController(AppDbContext context) 
    { 
        _context = context; 
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Reservations = await _context.Reservations
            .Select(r => new {
                vehicleId = r.VehicleId,
                start = r.StartDate.ToString("yyyy-MM-dd"),
                end = r.EndDate.ToString("yyyy-MM-dd")
            })
            .ToListAsync();
        return View(await _context.Vehicles.ToListAsync());
    }

    public IActionResult Create() => View();

    [HttpPost]
    public async Task<IActionResult> Create(Vehicle v)
    {
        if (ModelState.IsValid)
        {
            _context.Add(v);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(v);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var v = await _context.Vehicles.FindAsync(id);
        if (v == null) return NotFound();
        return View(v);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Vehicle v)
    {
        if (ModelState.IsValid)
        {
            _context.Update(v);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(v);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var v = await _context.Vehicles.FindAsync(id);
        if (v == null) return NotFound();
        return View(v);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        // Block deletion if the vehicle has existing reservations to prevent orphaned records.
        var hasReservations = await _context.Reservations.AnyAsync(r => r.VehicleId == id);
        if (hasReservations)
        {
            TempData["DeleteError"] = "This vehicle has existing reservations and cannot be deleted.";
            return RedirectToAction(nameof(Delete), new { id });
        }

        var v = await _context.Vehicles.FindAsync(id);
        if (v != null)
        {
            _context.Vehicles.Remove(v);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Reports()
    {
        var data = _context.Reservations
            .Include(r => r.Vehicle)
            .Include(r => r.Customer)
            .ToList();

        return View(data);
    }
}