using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// Handles CRUD operations for customers.
[Authorize]
[AutoValidateAntiforgeryToken]
public class CustomerController : Controller
{
    private readonly AppDbContext _context;
    public CustomerController(AppDbContext context) { _context = context; }

    public async Task<IActionResult> Index() => View(await _context.Customers.ToListAsync());

    public IActionResult Create() => View();

    [HttpPost]
    public async Task<IActionResult> Create(Customer c)
    {
        if (ModelState.IsValid)
        {
            _context.Add(c);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(c);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var c = await _context.Customers.FindAsync(id);
        if (c == null)
            return NotFound();
        return View(c);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, Customer c)
    {
        if (id != c.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            _context.Update(c);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(c);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var c = await _context.Customers.FindAsync(id);
        if (c == null)
            return NotFound();
        return View(c);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        // Block deletion if the customer has existing reservations to prevent orphaned records.
        var hasReservations = await _context.Reservations.AnyAsync(r => r.CustomerId == id);
        if (hasReservations)
        {
            TempData["DeleteError"] = "This customer has existing reservations and cannot be deleted.";
            return RedirectToAction(nameof(Delete), new { id });
        }

        var c = await _context.Customers.FindAsync(id);
        if (c != null)
        {
            _context.Customers.Remove(c);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}