using City_Taxi.Data;
using City_Taxi.Interfaces;
using City_Taxi.Models;
using City_Taxi.Repository;
using City_Taxi.Service;
using City_Taxi.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace City_Taxi.Controllers
{
    public class PassengerController : Controller
    {
        private readonly IPassengerRepository _passengerRepository;
        private readonly IDriverRepository _driverRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IConfiguration _configuration;
        private readonly IPassengerCardDetailsRepository _passengerCardDetailsRepository;
        private readonly CVVEncryption _cVVEncryption;
        private readonly IReservationRepository _reservationRepository;
        private readonly IDriverRatingsRepository _driverRatingsRepository;
        private readonly SmsService _smsService;

        public PassengerController(IPassengerRepository passengerRepository, IDriverRepository driverRepository, IVehicleRepository vehicleRepository, IConfiguration configuration, IPassengerCardDetailsRepository passengerCardDetails, CVVEncryption cVVEncryption, IReservationRepository reservationRepository, IDriverRatingsRepository driverRatingsRepository, SmsService smsService)
        {
            _driverRepository = driverRepository;
            _passengerRepository = passengerRepository;
            _vehicleRepository = vehicleRepository;
            _configuration = configuration;
            _passengerCardDetailsRepository = passengerCardDetails;
            _cVVEncryption = cVVEncryption;
            _reservationRepository = reservationRepository;
            _driverRatingsRepository = driverRatingsRepository;
            _smsService = smsService;
        }
        [HttpGet]
        public IActionResult Index()
        {
            ViewData["Key"] = _configuration["GoogleApi:Key"]; 
            return View();
        }

        public IActionResult PickDropLocation()
        {
            return View();
        }
        public IActionResult AddCardDetails()
        {
            return View();
        }
        public IActionResult PaymentMethod()
        {
            var passengerId = HttpContext.Session.GetInt32("UserID") ?? 0;
            var cards = _passengerCardDetailsRepository.GetCardsByPassengerIdPMV(passengerId);

            var model = new PaymentMethodViewModel
            {
                AvailableCards = cards.ToList(),
                SelectedPaymentMethod = "Cash"
            };

            return View(model);
        }
        public async Task<IActionResult> RideDetails(string reservationId)
        {
            if (string.IsNullOrEmpty(reservationId) || !int.TryParse(reservationId, out int parsedReservationId))
            {
                return BadRequest("Invalid reservation ID.");
            }

            var model = await _reservationRepository.GetReservationByReservationIDAsync(parsedReservationId);
            ViewData["Key"] = _configuration["GoogleApi:Key"];
            if (model == null)
            {
                return NotFound("Reservation not found.");
            }

            return View(model);
        }
        public IActionResult DriverRating(string reservationId)
        {
            ViewData["reservationID"] = reservationId;

            return View();
        }
        public async Task<IActionResult> DriverRatings(DriverRatings driverRatings)
        {
            ModelState.Remove("Reservation");
            if (!ModelState.IsValid)
            {
                return View();
            }

            _driverRatingsRepository.Add(driverRatings);

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> EditCardDetails(int id)
        {
            var cardDetails = await _passengerCardDetailsRepository.GetByIdAsync(id);
            cardDetails.CVV = _cVVEncryption.DecryptCVV(cardDetails.CVV);
            return View(cardDetails);
        }
        public async Task<IActionResult> ManageCardDetails()
        {
            var passengerId = HttpContext.Session.GetInt32("UserID") ?? 0;
            var cardDetails = await _passengerCardDetailsRepository.GetAll();
            return View(cardDetails);
        }
        public async Task<IActionResult> UpdateLocation(int passengerId, string latitude, string longitude)
        {
            var passenger = await _passengerRepository.GetByIdAsync(passengerId);
            if (passenger == null)
            {
                return NotFound(new { message = "Passenger not found." });
            }

            // Update the passenger's location
            passenger.CurrentLatitude = latitude;
            passenger.CurrentLongitude = longitude;

            _passengerRepository.Update(passenger);

            // Return updated location to the view
            return Json(new
            {
                currentLatitude = passenger.CurrentLatitude,
                currentLongitude = passenger.CurrentLongitude
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetNearbyDriversByVehicleType(string latitude, string longitude, string vehicleType, double radiusKm)
        {
            // Convert latitude and longitude to double
            if (!double.TryParse(latitude, out double lat) || !double.TryParse(longitude, out double lng))
            {
                return BadRequest(new { message = "Invalid latitude or longitude." });
            }

            // Fetch nearby drivers based on the vehicle type and location
            var drivers = await _passengerRepository.GetNearbyDriversByTypeAsync(latitude, longitude, vehicleType, radiusKm);

            if (drivers == null || !drivers.Any())
            {
                return NotFound(new { message = "No drivers found." });
            }

            // Return the list of drivers
            return Json(new
            {
                nearbyDrivers = drivers.Select(d => new
                {
                    d.FirstName,
                    d.LastName,
                    d.Latitude,
                    d.Longitude,
                    d.PricePerKM,
                    d.VehicleID,
                    d.DriverID,
                })
            });
        }
        [HttpPost]
        public IActionResult AddCardDetails(PassengerCardDetails passengerCardDetails, string pageSource)
        {
            passengerCardDetails.CVV = _cVVEncryption.EncryptCVV(passengerCardDetails.CVV);
            _passengerCardDetailsRepository.Add(passengerCardDetails);
            if (pageSource == "PaymentMethodPage")
            {
                return RedirectToAction("PaymentMethod");
            }
            else if (pageSource == "ManageCardDetailsPage")
            { 
                return RedirectToAction("ManageCardDetails"); 
            }

            return RedirectToAction("ManageCardDetails");
        }
        [HttpPost]
        public async Task<IActionResult> EditCardDetails(PassengerCardDetails passengerCardDetails, string CVV, int CardID)
        {
            ModelState.Remove("Passenger");
            ModelState.Remove("CVV");
            if (!ModelState.IsValid)
            {
                View(passengerCardDetails);
            }

            var cardDetails = await _passengerCardDetailsRepository.GetByIdAsync(CardID);

            if (cardDetails != null)
            {
                cardDetails.CardNumber = passengerCardDetails.CardNumber;
                cardDetails.CardHolderName = passengerCardDetails.CardHolderName;
                cardDetails.ExpirationDate = passengerCardDetails.ExpirationDate;

                if (passengerCardDetails.CVV != null)
                {
                    cardDetails.CVV = _cVVEncryption.EncryptCVV(CVV);
                }

                _passengerCardDetailsRepository.Update(cardDetails);

                return RedirectToAction("ManageCardDetails");
            }

            return View(passengerCardDetails);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteCardDetails(int CardID)
        {
            var cardDetails = await _passengerCardDetailsRepository.GetByIdAsync(CardID);

            if (cardDetails != null)
            {
                _passengerCardDetailsRepository.Delete(cardDetails);
            }

            return RedirectToAction("ManageCardDetails");
        }

        [HttpPost]
        public IActionResult ConfirmPaymentMethod(PaymentMethodViewModel model)
        {
            // Process the selected payment method (either Card or Cash)
            if (model.SelectedPaymentMethod.StartsWith("Card_"))
            {
                var cardId = model.SelectedPaymentMethod.Split('_')[1];
                return Json(new { method = "Card", lastFourDigits = model.AvailableCards.First(c => c.CardId == int.Parse(cardId)).LastFourDigits, cardID = cardId });
            }
            else if (model.SelectedPaymentMethod == "Cash")
            {
                return Json(new { method = "Cash", lastFourDigits = "", cardID = 0 });
            }

            return BadRequest("Invalid payment method.");
        }


        [HttpPost]
        public IActionResult SubmitReservation([FromBody] Reservation model)
        {
            ModelState.Remove("Driver");
            ModelState.Remove("DropoffTime");
            ModelState.Remove("PickupTime");
            ModelState.Remove("Passenger");
            ModelState.Remove("PaymentStatus");
            ModelState.Remove("Vehicle");
            ModelState.Remove("ReservationStatus");
            ModelState.Remove("ReservationTime");
            var passengerID = HttpContext.Session.GetInt32("UserID");

            if (passengerID == null || passengerID == 0)
            {
                return BadRequest("Invalid session or user not logged in.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid reservation details.");
            }

            var reservation = new Reservation
            {
                PickupLocation = model.PickupLocation,
                DropoffLocation = model.DropoffLocation,
                DriverID = model.DriverID,
                VehicleID = model.VehicleID,
                ReservationStatus = "Waiting for driver to accept",
                ReservationTime = DateTime.Now,
                PassengerID = passengerID.Value,
                PaymentType = model.PaymentType,
                CardID = model.CardID,
                TripDistance = model.TripDistance,
                TripPrice = model.TripPrice,
                PaymentStatus = "Pending"
            };

            _reservationRepository.Add(reservation);

            return Json(new { reservationID = reservation.ReservationID });
        }
        [HttpPost]
        public IActionResult UpdateReservation([FromBody] Reservation model)
        {
            ModelState.Remove("Driver");
            ModelState.Remove("DropoffTime");
            ModelState.Remove("PickupTime");
            ModelState.Remove("Passenger");
            ModelState.Remove("PaymentStatus");
            ModelState.Remove("Vehicle");
            ModelState.Remove("ReservationStatus");
            ModelState.Remove("ReservationTime");
            var passengerID = HttpContext.Session.GetInt32("UserID");

            if (passengerID == null || passengerID == 0)
            {
                return BadRequest("Invalid session or user not logged in.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid reservation details.");
            }

            var reservation = new Reservation
            {
                ReservationID = model.ReservationID,
                PickupLocation = model.PickupLocation,
                DropoffLocation = model.DropoffLocation,
                DriverID = model.DriverID,
                VehicleID = model.VehicleID,
                ReservationStatus = "Waiting for driver to accept",
                ReservationTime = DateTime.Now,
                PassengerID = passengerID.Value,
                PaymentType = model.PaymentType,
                CardID = model.CardID,
                TripDistance = model.TripDistance,
                TripPrice = model.TripPrice,
                PaymentStatus = "Pending"
            };

            _reservationRepository.Update(reservation);

            return Json(new { reservationID = reservation.ReservationID });
        }
        [HttpGet]
        public async Task<IActionResult> checkReservationStatus(string reservationId)
        {
            var reservation = await _reservationRepository.GetReservationStatusByReservationIDAsync(int.Parse(reservationId));
            if (reservation != null)
            {
                return Json(new { Reservation = true, reservationStatus = reservation});
            }

            return Json(new { Reservation = false });
        }
        [HttpPost]
        public async Task<IActionResult> CancelRide(string reservationId)
        {
            var reservation = await _reservationRepository.GetReservationByReservationIDAsync(int.Parse(reservationId));
            if (reservation != null)
            {
                _reservationRepository.Delete(reservation);
                return Json(new { Reservation = true });
            }

            return Json(new { Reservation = false });
        }
        [HttpPost]
        public async Task<IActionResult> UpdateReservationStatus(string reservationId, string status)
        {
            var reservation = await _reservationRepository.GetReservationByReservationIDAsync(int.Parse(reservationId));
            if (reservation != null)
            {
                reservation.ReservationStatus = status;
                _reservationRepository.Update(reservation);
                return Json(new { Reservation = true });
            }

            return Json(new { Reservation = false });
        }
        [HttpPost]
        public async Task<IActionResult> SendTripDetailsSms(int reservationId)
        {
            // Fetch reservation details using the reservationId
            var reservation = await _reservationRepository.GetReservationByReservationIDAsync(reservationId);
            if (reservation == null)
            {
                return NotFound();
            }

            // Prepare the SMS message
            var message = $"City Taxi\n\n\n" +
                          $"Ride confirmed below are the trip details\n\n\n" +
                          $"Driver Name: {reservation.Driver.FirstName} {reservation.Driver.LastName}\n\n" +
                          $"Driver Phone Number: {reservation.Driver.PhoneNumber}\n\n" +
                          $"Vehicle: {reservation.Vehicle.VehicleMake} {reservation.Vehicle.VehicleModel}\n\n" +
                          $"Vehicle Number: {reservation.Vehicle.VehicleNumberPlate}\n\n" +
                          $"Trip Distance: {reservation.TripDistance}\n\n" +
                          $"Trip Fare: {reservation.TripPrice.ToString("F2")}";

            // Send the SMS
            await _smsService.SendSmsAsync(reservation.Driver.PhoneNumber, message);

            return Ok();
        }
    }

}
