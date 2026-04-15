using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class VehicleController : Controller
{
    private readonly AppDbContext _context;
    
    public VehicleController(AppDbContext context) 
    { 
        _context = context; 
    }

    public async Task<IActionResult> Index() => View(await _context.Vehicles.ToListAsync());

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
        return View(v);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Vehicle v)
    {
        _context.Update(v);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var v = await _context.Vehicles.FindAsync(id);
        _context.Vehicles.Remove(v);
        await _context.SaveChangesAsync();
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