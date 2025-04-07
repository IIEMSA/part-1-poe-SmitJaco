using EventEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Controllers
{
    public class VenuesController : Controller
    {
        
            private readonly EventEaseDBContext _context;

            public VenuesController(EventEaseDBContext context)
            {
                _context = context;
            }

            //: Venues
            public async Task<IActionResult> Index()
            {
                var venues = await _context.Venues.ToListAsync();
                return View(venues);
            }

            // Venues Details
            public async Task<IActionResult> Details(int? id)
            {
                if (id == null)
                {
                    return NotFound();
                }

                var venue = await _context.Venues.FirstOrDefaultAsync(m => m.VenueId == id);
                if (venue == null)
                {
                    return NotFound();
                }

                return View(venue);
            }

            // Venues Create
            public IActionResult Create()
            {
                return View();
            }

            // Venues Create
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Create([Bind("VenueId,VenueName,Location,Capacity,ImageURL")] Venue venue)
            {
                if (ModelState.IsValid)
                {
                    _context.Add(venue);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                return View(venue);
            }

            // Venues Edit
            public async Task<IActionResult> Edit(int? id)
            {
                if (id == null)
                {
                    return NotFound();
                }

                var venue = await _context.Venues.FindAsync(id);
                if (venue == null)
                {
                    return NotFound();
                }
                return View(venue);
            }

            //  Venues Edit
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Edit(int id, [Bind("VenueId,VenueName,Location,Capacity,ImageURL")] Venue venue)
            {
                if (id != venue.VenueId)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(venue);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!_context.Venues.Any(e => e.VenueId == venue.VenueId))
                        {
                            return NotFound();
                        }
                        throw;
                    }
                    return RedirectToAction(nameof(Index));
                }
                return View(venue);
            }

            //  Venues Delete
            public async Task<IActionResult> Delete(int? id)
            {
                if (id == null)
                {
                    return NotFound();
                }

                var venue = await _context.Venues
                    .FirstOrDefaultAsync(m => m.VenueId == id);
                if (venue == null)
                {
                    return NotFound();
                }

                return View(venue);
            }

            //  Venues Delete
            [HttpPost, ActionName("Delete")]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> DeleteConfirmed(int id)
            {
                var venue = await _context.Venues.FindAsync(id);
                if (venue != null)
                {
                    // Check if there are any bookings or events associated with this venue
                    var hasBookings = await _context.Bookings.AnyAsync(b => b.VenueId == id);
                    var hasEvents = await _context.Events.AnyAsync(e => e.VenueId == id);
                    if (hasBookings || hasEvents)
                    {
                        ModelState.AddModelError("", "Cannot delete venue because it has associated bookings or events.");
                        return View(venue);
                    }

                    _context.Venues.Remove(venue);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
        }

}

