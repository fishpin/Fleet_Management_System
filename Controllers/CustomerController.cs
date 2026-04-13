using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
}