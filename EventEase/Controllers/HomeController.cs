using EventEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace EventEase.Controllers
{
    public class HomeController : Controller
    {
        private readonly EventEaseDBContext _context;

        public HomeController(EventEaseDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            //  featured venues 
            var featuredVenues = await _context.Venues
                .OrderByDescending(v => v.Capacity)
                .Take(3)
                .ToListAsync();

            //  upcoming events
            var upcomingEvents = await _context.Events
                .Include(e => e.Venue)
                .Where(e => e.EventDate >= DateTime.Today)
                .OrderBy(e => e.EventDate)
                .Take(3)
                .ToListAsync();

            // Pass the data to the view using ViewBag or a view model
            ViewBag.FeaturedVenues = featuredVenues;
            ViewBag.UpcomingEvents = upcomingEvents;

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