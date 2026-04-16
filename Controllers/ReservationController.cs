using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// Handles CRUD operations for reservations. Creating a reservation also generates a billing record.
[Authorize]
[AutoValidateAntiforgeryToken]
public class ReservationController : Controller
{
    private readonly AppDbContext _context;
    public ReservationController(AppDbContext context) { _context = context; }

    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;
        ViewBag.ActiveReservations = await _context.Reservations
            .Include(r => r.Vehicle).Include(r => r.Customer)
            .Where(r => r.StartDate.Date <= today && r.EndDate.Date >= today)
            .OrderBy(r => r.EndDate)
            .ToListAsync();
        ViewBag.UpcomingReservations = await _context.Reservations
            .Include(r => r.Vehicle).Include(r => r.Customer)
            .Where(r => r.StartDate.Date > today)
            .OrderBy(r => r.StartDate)
            .ToListAsync();
        ViewBag.PastReservations = await _context.Reservations
            .Include(r => r.Vehicle).Include(r => r.Customer)
            .Where(r => r.EndDate.Date < today)
            .OrderByDescending(r => r.EndDate)
            .ToListAsync();
        ViewBag.BillingMap = await _context.Billings
            .ToDictionaryAsync(b => b.ReservationId, b => b.Id);
        return View();
    }

    public IActionResult Create(int? vehicleId)
    {
        ViewBag.Vehicles = _context.Vehicles.ToList();
        ViewBag.Customers = _context.Customers.ToList();
        ViewBag.Reservations = _context.Reservations
            .Select(r => new {
                vehicleId = r.VehicleId,
                start = r.StartDate.ToString("yyyy-MM-dd"),
                end = r.EndDate.ToString("yyyy-MM-dd")
            })
            .ToList();
        return View(new Reservation { VehicleId = vehicleId ?? 0 });
    }

    [HttpPost]
    public async Task<IActionResult> Create(Reservation r)
    {
        if (r.EndDate <= r.StartDate)
        {
            ModelState.AddModelError(string.Empty, "End date must be after the start date.");
            ViewBag.Vehicles = _context.Vehicles.ToList();
            ViewBag.Customers = _context.Customers.ToList();
            return View(r);
        }

        var vehicle = await _context.Vehicles.FindAsync(r.VehicleId);
        if (vehicle == null || !vehicle.IsAvailable)
        {
            ModelState.AddModelError(string.Empty, "This vehicle is not available for the selected period.");
            ViewBag.Vehicles = _context.Vehicles.ToList();
            ViewBag.Customers = _context.Customers.ToList();
            return View(r);
        }

        // Wrap reservation + billing creation in a transaction so both succeed or neither does.
        using var transaction = await _context.Database.BeginTransactionAsync();

        vehicle.IsAvailable = false;
        var days = (r.EndDate - r.StartDate).Days;
        r.TotalCost = days * vehicle.PricePerDay + (r.IncludesInsurance ? days * BusinessRules.InsurancePerDay : 0m);

        _context.Add(r);
        await _context.SaveChangesAsync();

        var tax = Math.Round(r.TotalCost * BusinessRules.TaxRate, 2);
        _context.Add(new Billing {
            ReservationId = r.Id,
            Tax = tax,
            ExtraCharges = 0,
            FinalAmount = r.TotalCost + tax
        });
        await _context.SaveChangesAsync();

        await transaction.CommitAsync();

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

        if (r.EndDate <= r.StartDate)
        {
            ModelState.AddModelError(string.Empty, "End date must be after the start date.");
            ViewBag.Vehicles = _context.Vehicles.ToList();
            ViewBag.Customers = _context.Customers.ToList();
            return View(r);
        }

        var vehicle = await _context.Vehicles.FindAsync(r.VehicleId);
        if (vehicle == null)
        {
            ModelState.AddModelError(string.Empty, "The selected vehicle could not be found.");
            ViewBag.Vehicles = _context.Vehicles.ToList();
            ViewBag.Customers = _context.Customers.ToList();
            return View(r);
        }

        var days = (r.EndDate - r.StartDate).Days;
        r.TotalCost = days * vehicle.PricePerDay + (r.IncludesInsurance ? days * BusinessRules.InsurancePerDay : 0m);

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