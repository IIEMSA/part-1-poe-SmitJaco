using EventEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Diagnostics; // For debugging

namespace EventEase.Controllers
{
    public class BookingsController : Controller
    {
        private readonly EventEaseDBContext _context;

        public BookingsController(EventEaseDBContext context)
        {
            _context = context;
        }

        // Bookings
        public async Task<IActionResult> Index()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .ToListAsync();
            return View(bookings);
        }

        // Bookings Details 
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // Bookings Create
        public IActionResult Create()
        {
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName");
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName");
            return View();
        }

        // Bookings Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingId,EventId,VenueId,BookingDate")] Booking booking)
        {
            // Log input data for debugging
            Debug.WriteLine($"Create Booking: BookingId={booking.BookingId}, EventId={booking.EventId}, VenueId={booking.VenueId}, BookingDate={booking.BookingDate}");

            // Validate selected event
            var selectedEvent = await _context.Events.FirstOrDefaultAsync(e => e.EventId == booking.EventId);
            if (selectedEvent == null)
            {
                ModelState.AddModelError("", "Selected event not found.");
                ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
                ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
                return View(booking);
            }

            // Check for double booking based on BookingDate
            var conflict = await _context.Bookings
                .AnyAsync(b => b.VenueId == booking.VenueId &&
                               b.BookingDate.Date == booking.BookingDate.Date);

            if (conflict)
            {
                // Debug: Log conflicting bookings
                var conflictingBookings = await _context.Bookings
                    .Where(b => b.VenueId == booking.VenueId &&
                                b.BookingDate.Date == booking.BookingDate.Date)
                    .ToListAsync();
                Debug.WriteLine($"Found {conflictingBookings.Count} conflicting bookings for VenueId: {booking.VenueId}, Date: {booking.BookingDate.Date}");
                foreach (var cb in conflictingBookings)
                {
                    Debug.WriteLine($"Conflict: BookingId={cb.BookingId}, EventId={cb.EventId}, VenueId={cb.VenueId}, BookingDate={cb.BookingDate}");
                }

                ModelState.AddModelError("", "This venue is already booked for that date.");
                ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
                ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
                return View(booking);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(booking);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Booking created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    // Log the exception for debugging
                    Debug.WriteLine($"DbUpdateException: {ex.InnerException?.Message}");

                    ModelState.AddModelError("", "Unable to save booking. The venue may already be booked for this date.");
                    ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
                    ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
                    return View(booking);
                }
            }

            // Ensure ViewData is populated for invalid ModelState
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
            return View(booking);
        }

        // Bookings Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
            return View(booking);
        }

        // Bookings Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingId,EventId,VenueId,BookingDate")] Booking booking)
        {
            if (id != booking.BookingId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Check for double booking (excluding the current booking)
                    var existingBooking = await _context.Bookings
                        .FirstOrDefaultAsync(b => b.BookingId != id && b.VenueId == booking.VenueId && b.BookingDate.Date == booking.BookingDate.Date);
                    if (existingBooking != null)
                    {
                        ModelState.AddModelError("", "This venue is already booked on the selected date.");
                        ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
                        ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
                        return View(booking);
                    }

                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Bookings.Any(e => e.BookingId == booking.BookingId))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
            return View(booking);
        }

        // Bookings/Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // Bookings Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        //Search
        public async Task<IActionResult> BookingDisplay(string searchString)
        {
            var query = _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .Select(b => new BookingViewModel
                {
                    BookingId = b.BookingId,
                    EventName = b.Event!.EventName,
                    EventDate = b.Event.EventDate,
                    VenueName = b.Venue!.VenueName,
                    Location = b.Venue.Location,
                    BookingDate = b.BookingDate
                });

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(b =>
                    b.BookingId.ToString().Contains(searchString) ||
                    b.EventName.Contains(searchString));
            }

            var results = await query.ToListAsync();
            return View(results);
        }
    }
}