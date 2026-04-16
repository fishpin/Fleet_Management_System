using FleetManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace FleetManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        public HomeController(AppDbContext context) { _context = context; }

        // Loads all dashboard metrics: fleet summary, today's pickups/returns, recent reservations, and monthly revenue.
        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var monthEnd = monthStart.AddMonths(1);

            ViewBag.TotalVehicles     = await _context.Vehicles.CountAsync();
            ViewBag.AvailableVehicles = await _context.Vehicles.CountAsync(v => v.IsAvailable);
            ViewBag.TotalCustomers    = await _context.Customers.CountAsync();
            ViewBag.TotalReservations = await _context.Reservations.CountAsync();

            ViewBag.MonthlyRevenue = await _context.Billings
                .Include(b => b.Reservation)
                .Where(b => b.Reservation.StartDate >= monthStart && b.Reservation.StartDate < monthEnd)
                .SumAsync(b => (decimal?)b.FinalAmount) ?? 0m;

            ViewBag.PickupsToday = await _context.Reservations
                .Include(r => r.Vehicle)
                .Include(r => r.Customer)
                .Where(r => r.StartDate.Date == today)
                .OrderBy(r => r.Customer.FullName)
                .ToListAsync();

            ViewBag.ReturnsToday = await _context.Reservations
                .Include(r => r.Vehicle)
                .Include(r => r.Customer)
                .Where(r => r.EndDate.Date == today)
                .OrderBy(r => r.Customer.FullName)
                .ToListAsync();

            ViewBag.RecentReservations = await _context.Reservations
                .Include(r => r.Vehicle)
                .Include(r => r.Customer)
                .OrderByDescending(r => r.StartDate)
                .Take(8)
                .ToListAsync();

            ViewBag.LowAvailability = (int)ViewBag.AvailableVehicles < 3 && (int)ViewBag.TotalVehicles > 0;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
