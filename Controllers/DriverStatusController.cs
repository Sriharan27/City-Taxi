using City_Taxi.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace City_Taxi.Controllers
{
    public class DriverStatusController : Controller
    {
        private readonly IDriverStatusRepository _driverStatusRepository;

        public DriverStatusController(IDriverStatusRepository driverStatusRepository)
        {
            _driverStatusRepository = driverStatusRepository;
        }

        // Get the status of a driver
        [HttpGet]
        public async Task<IActionResult> GetDriverStatus(int driverID)
        {
            var driverStatus = await _driverStatusRepository.GetDriverStatusAsync(driverID);
            if (driverStatus == null)
            {
                return NotFound();
            }
            return View(driverStatus);
        }

/*        // Update the status of a driver
        [HttpPost]
        public async Task<IActionResult> UpdateDriverStatus(int driverID, string status, string latitude, string longitude)
        {
            var driverStatus = await _driverStatusRepository.GetDriverStatusAsync(driverID);

            if (driverStatus == null)
            {
                return NotFound();
            }

            driverStatus.Status = status;
            driverStatus.LastUpdated = DateTime.Now;

            // Latitude and Longitude update
            driverStatus.Latitude = latitude;
            driverStatus.Longitude = longitude;

            _driverStatusRepository.UpdateStatus(driverStatus);

            return Ok();
        }*/

        // Get nearby drivers within a certain radius
        [HttpGet]
        public async Task<IActionResult> GetNearbyDrivers(string latitude, string longitude, double radius)
        {
            var nearbyDrivers = await _driverStatusRepository.GetNearbyDriversAsync(latitude, longitude, radius);
            return View(nearbyDrivers);
        }
    }
}
