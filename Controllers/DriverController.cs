using City_Taxi.Interfaces;
using City_Taxi.Models;
using City_Taxi.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace City_Taxi.Controllers
{
    public class DriverController : Controller
    {
        public readonly IVehicleRepository _vehicleRepository;
        public readonly IVehicleTypeRepository _vehicleTypeRepository;
        public readonly IDriverRepository _driverRepository;
        public readonly IDriverStatusRepository _driverStatusRepository;
        public readonly IConfiguration _configuration;
        public readonly IReservationRepository _reservationRepository;

        public DriverController(IVehicleRepository vehicleRepository, IVehicleTypeRepository vehicleTypeRepository, IDriverRepository driverRepository, IDriverStatusRepository driverStatusRepository,IConfiguration configuration, IReservationRepository reservationRepository)
        {
            _vehicleRepository = vehicleRepository;
            _vehicleTypeRepository = vehicleTypeRepository;
            _driverRepository = driverRepository;
            _driverStatusRepository = driverStatusRepository;
            _configuration = configuration;
            _reservationRepository = reservationRepository;
        }
        public async Task<IActionResult> Index()
        {
            ViewData["Key"] = _configuration["GoogleApi:Key"];
            var userId = HttpContext.Session.GetInt32("UserID") ?? 0;
            var driverStatus = await _driverStatusRepository.GetDriverStatusAsync(userId);
            if (driverStatus == null)
            {
                return NotFound("Driver not found.");
            }

            return View(driverStatus);
        }
        public async Task<IActionResult> AddVehicle()
        {
            ViewData["VehicleTypes"] = await _vehicleTypeRepository.GetAll();
            return View();
        }
        public async Task<IActionResult> EditVehicle(int id)
        {
            var vehicles = await _vehicleRepository.GetByVehicleIdAsync(id);
            ViewData["VehicleTypes"] = await _vehicleTypeRepository.GetAll();
            return View(vehicles);
        }
        public async Task<IActionResult> ManageVehicle()
        {
            var userId = HttpContext.Session.GetInt32("UserID") ?? 0;
            var vehicles = await _vehicleRepository.GetAllByDriverIdAsync(userId);
            return View(vehicles);
        }
        [HttpPost]
        public IActionResult EditVehicle(Vehicle vehicle)
        {
            ModelState.Remove("Driver");
            ModelState.Remove("VehicleType");
            if (ModelState.IsValid)
            {
                _vehicleRepository.Update(vehicle);
                return RedirectToAction("ManageVehicle", "Driver");
            }

            ViewData["VehicleTypes"] = _vehicleTypeRepository.GetAll();
            return View(vehicle);
        }
        [HttpPost]
        public IActionResult AddVehicle(Vehicle vehicle)
        {
            ModelState.Remove("Driver");
            ModelState.Remove("VehicleType");
            if (ModelState.IsValid)
            {
                _vehicleRepository.Add(vehicle);
                return RedirectToAction("ManageVehicle","Driver");
            }

            ViewData["VehicleTypes"] = _vehicleTypeRepository.GetAll();
            return View(vehicle);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteVehicle(int vehicleId)
        {
            var vehicle = await _vehicleRepository.GetByVehicleIdAsync(vehicleId);

            if (vehicle != null)
            {
                _vehicleRepository.Delete(vehicle);
            }

            return RedirectToAction("ManageVehicle");
        }
        [HttpPost]
        public async Task<IActionResult> UpdateLocation(int driverStatusId, string latitude, string longitude)
        {
            var driverStatus = await _driverStatusRepository.GetByDriverStatusIdAsync(driverStatusId);
            if (driverStatus == null)
            {
                return NotFound(new { message = "Driver not found." });
            }

            // Update the passenger's location
            driverStatus.Latitude = latitude;
            driverStatus.Longitude = longitude;
            driverStatus.LastUpdated = DateTime.Now;

            _driverStatusRepository.UpdateStatus(driverStatus);

            return Ok(driverStatus);
        }
        [HttpGet]
        public IActionResult GetBranchesByBank(int bankId)
        {
            var branches = _driverRepository.GetBranchesByBankId(bankId);
            return Json(branches);
        }
        public async Task<IActionResult> Logout()
        {
            var driverId = HttpContext.Session.GetInt32("UserID") ?? 0;
            var driverStatus = await _driverStatusRepository.GetDriverStatusAsync(driverId);
            driverStatus.Status = "OFFLINE";
            _driverStatusRepository.UpdateStatus(driverStatus);
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public async Task<IActionResult> CheckForNewReservation(string driverStatusId)
        {
            var reservation = await _reservationRepository.GetWaitingReservationByDriverIDAsync(int.Parse(driverStatusId));
            if (reservation != null)
            {
                return Json(new { Reservation = true, reservationId = reservation.ReservationID, tripDistance = reservation.TripDistance, tripFare = reservation.TripPrice });
            }

            return Json(new { Reservation = false });
        }
        [HttpPost]
        public async Task<IActionResult> UpdateReservationStatus(string reservationId, string status)
        {
            var reservation = await _reservationRepository.GetReservationByReservationIDAsync(int.Parse(reservationId));

            if (status == "Trip Started") 
            {
                reservation.PickupTime = DateTime.Now;
            }
            else if (status == "Trip Ended")
            {
                reservation.DropoffTime = DateTime.Now;
                reservation.PaymentStatus = "Paid";
            }

            reservation.ReservationStatus = status;
            
            _reservationRepository.Update(reservation);

            return Json(new
            {
                Success = true,
                ReservationId = reservation.ReservationID,
                PickupLocation = reservation.PickupLocation, // Assuming you have this in your model
                DropoffLocation = reservation.DropoffLocation,
                PassengerPhone = reservation.Passenger.PhoneNumber, // Add this to your reservation model
                PassengerName = reservation.Passenger.FirstName + reservation.Passenger.LastName,
                TripDistance = reservation.TripDistance,
                TripFare = reservation.TripPrice,
                PaymentType = reservation.PaymentType,
                PickupTime = reservation.PickupTime,
                DropoffTime = reservation.DropoffTime,
            });
        }
        [HttpPost]
        public async Task<IActionResult> UpdateDriverStatus(string status, int userId)
        {
            if (userId == 0 || string.IsNullOrEmpty(status))
            {
                return BadRequest("Invalid data.");
            }

            // Fetch the user from the database
            var driverStatus = await _driverStatusRepository.GetDriverStatusAsync(userId);
            if (driverStatus == null)
            {
                return NotFound("User not found.");
            }

            driverStatus.Status = status;

            _driverStatusRepository.UpdateStatus(driverStatus);

            return Ok("Status updated successfully.");
        }

    }
}
