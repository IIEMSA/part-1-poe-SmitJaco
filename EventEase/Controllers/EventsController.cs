
using EventEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace EventEase.Controllers
{
    public class EventsController : Controller
    {
        private readonly EventEaseDBContext _context;

        public EventsController(EventEaseDBContext context)
        {
            _context = context;
        }

        //: Events
        public async Task<IActionResult> Index()
        {
            var events = await _context.Events.Include(e => e.Venue).ToListAsync();
            return View(events);
        }

        //: Events/Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var evt = await _context.Events
                .Include(e => e.Venue) 
                .FirstOrDefaultAsync(m => m.EventId == id);

            if (evt == null)
            {
                return NotFound();
            }

            return View(evt);
        }

        //: Events/Create
        public IActionResult Create()
        {
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName");
            return View();
        }

        //: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventId,EventName,EventDate,Description,VenueId")] Event eventItem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(eventItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", eventItem.VenueId);
            return View(eventItem);
        }

        //: Events/Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null)
            {
                return NotFound();
            }
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", eventItem.VenueId);
            return View(eventItem);
        }

        //: Events/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EventId,EventName,EventDate,Description,VenueId")] Event eventItem)
        {
            if (id != eventItem.EventId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(eventItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Events.Any(e => e.EventId == eventItem.EventId))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", eventItem.VenueId);
            return View(eventItem);
        }

        // Events Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventItem = await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Bookings) // Include Bookings for consistency
                .FirstOrDefaultAsync(m => m.EventId == id);
            if (eventItem == null)
            {
                return NotFound();
            }

            return View(eventItem);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var eventItem = await _context.Events
                .Include(e => e.Bookings) // Include Bookings for validation
                .FirstOrDefaultAsync(e => e.EventId == id);
            if (eventItem == null)
            {
                return NotFound();
            }

            // Check if there are any bookings associated with this event
            var hasBookings = eventItem.Bookings != null && eventItem.Bookings.Any();
            if (hasBookings)
            {
                ModelState.AddModelError("", "Cannot delete event because it has associated bookings.");
                return View(eventItem);
            }

            _context.Events.Remove(eventItem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}