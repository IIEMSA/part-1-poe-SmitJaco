using EventEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Diagnostics;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace EventEase.Controllers
{
    public class VenuesController : Controller
    {
        private readonly EventEaseDBContext _context;

        public VenuesController(EventEaseDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var venues = await _context.Venues.ToListAsync();
            return View(venues);
        }

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

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VenueId,VenueName,Location,Capacity,ImageUrl,ImageFile")] Venue venue)
        {
            Debug.WriteLine($"Create Venue: VenueId={venue.VenueId}, VenueName={venue.VenueName}, Location={venue.Location}, Capacity={venue.Capacity}, ImageUrl={venue.ImageUrl}, ImageFile={(venue.ImageFile != null ? venue.ImageFile.FileName : "null")}");

            // Check for duplicate VenueName
            if (await _context.Venues.AnyAsync(v => v.VenueName == venue.VenueName))
            {
                ModelState.AddModelError("VenueName", "A venue with this name already exists.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (venue.ImageFile != null)
                    {
                        var blobUrl = await UploadImageToBlobAsync(venue.ImageFile);
                        venue.ImageUrl = blobUrl;
                    }

                    _context.Add(venue);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Venue created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error creating venue: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    }
                    ModelState.AddModelError("", $"An error occurred while creating the venue. {ex.Message}");
                }
            }

            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Debug.WriteLine($"ModelState Error: {error.ErrorMessage}");
            }

            return View(venue);
        }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("VenueId,VenueName,Location,Capacity,ImageUrl,ImageFile")] Venue venue)
        {
            if (id != venue.VenueId)
            {
                return NotFound();
            }

            Debug.WriteLine($"Edit Venue: VenueId={venue.VenueId}, VenueName={venue.VenueName}, Location={venue.Location}, Capacity={venue.Capacity}, ImageUrl={venue.ImageUrl}, ImageFile={(venue.ImageFile != null ? venue.ImageFile.FileName : "null")}");

            // Check for duplicate VenueName (excluding current venue)
            if (await _context.Venues.AnyAsync(v => v.VenueName == venue.VenueName && v.VenueId != venue.VenueId))
            {
                ModelState.AddModelError("VenueName", "A venue with this name already exists.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (venue.ImageFile != null)
                    {
                       

                        var blobUrl = await UploadImageToBlobAsync(venue.ImageFile);
                        venue.ImageUrl = blobUrl;
                    }

                    _context.Update(venue);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Venue updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VenueExists(venue.VenueId))
                    {
                        return NotFound();
                    }
                    throw;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error updating venue: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while updating the venue. Please try again.");
                }
            }

            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Debug.WriteLine($"ModelState Error: {error.ErrorMessage}");
            }

            return View(venue);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues
                .Include(v => v.Events) // Ensure Events navigation property is included
                .FirstOrDefaultAsync(m => m.VenueId == id);
            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var venue = await _context.Venues
                .Include(v => v.Events) // Include Events for validation
                .FirstOrDefaultAsync(v => v.VenueId == id);
            if (venue == null)
            {
                return NotFound();
            }

            // Check if there are any events associated with this venue
            var hasEvents = venue.Events != null && venue.Events.Any();
            if (hasEvents)
            {
                ModelState.AddModelError("", "Cannot delete venue because it has associated events.");
                return View(venue);
            }

            _context.Venues.Remove(venue);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VenueExists(int id)
        {
            return _context.Venues.Any(e => e.VenueId == id);
        }

              // This is Step 5 (C): Upload selected image to Azure Blob Storage.
        // It completes the entire uploading process inside Step 5 â€” from connecting to Azure to returning the Blob URL after upload.
        // This will upload the Image to Blob Storage Account
        // Uploads an image to Azure Blob Storage and returns the Blob URL
        private async Task<string> UploadImageToBlobAsync(IFormFile imageFile)
        {


            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(Guid.NewGuid() + Path.GetExtension(imageFile.FileName));

            var blobHttpHeaders = new Azure.Storage.Blobs.Models.BlobHttpHeaders
            {
                ContentType = imageFile.ContentType
            };

            using (var stream = imageFile.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new Azure.Storage.Blobs.Models.BlobUploadOptions
                {
                    HttpHeaders = blobHttpHeaders
                });
            }

            return blobClient.Uri.ToString();
        }
    }
}