using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
}