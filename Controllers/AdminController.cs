using City_Taxi.Data;
using City_Taxi.Interfaces;
using City_Taxi.Models;
using City_Taxi.Repository;
using City_Taxi.ViewModel;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace City_Taxi.Controllers
{
    public class AdminController : Controller
    {
        private readonly IDriverRepository _driverRepository;
        private readonly IPassengerRepository _passengerRepository;
        private readonly ILoginRepository _loginRepository;
        private readonly EmailService _emailService;
        private readonly IDriverStatusRepository _driverStatusRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly ISystemUserRepository _systemUserRepository;
        private readonly IReservationRepository _reservationRepository;
        private readonly IDriverRatingsRepository _driverRatingsRepository;
        public readonly IConfiguration _configuration;
        public AdminController(IDriverRepository driverRepository, ILoginRepository loginRepository, EmailService emailService, IPassengerRepository passengerRepository, IDriverStatusRepository driverStatusRepository, IVehicleRepository vehicleRepository, ISystemUserRepository systemUserRepository, IReservationRepository reservationRepository, IDriverRatingsRepository driverRatingsRepository, IConfiguration configuration)
        {
            _driverRepository = driverRepository;
            _loginRepository = loginRepository;
            _emailService = emailService;
            _passengerRepository = passengerRepository;
            _driverStatusRepository = driverStatusRepository;
            _vehicleRepository = vehicleRepository;
            _systemUserRepository = systemUserRepository;
            _reservationRepository = reservationRepository;
            _driverRatingsRepository = driverRatingsRepository;
            _configuration = configuration;
        }
        public async Task<IActionResult> Index()
        {
            var DriverCount = await _driverRepository.GetDriverCountAsync();
            ViewData["DriverCount"] = DriverCount;

            var PassengerCount = await _passengerRepository.GetPassengerCountAsync();
            ViewData["PassengerCount"] = PassengerCount;

            var TripCount = await _reservationRepository.GetTripCountAsync();
            ViewData["TripCount"] = TripCount;

            var RatingsCount = await _driverRatingsRepository.GetRatingsCountAsync();
            var StarsTotal = await _driverRatingsRepository.GetTripStarsTotalAsync();
            var RatingOutOfFive = RatingsCount > 0 ? Math.Round((double)StarsTotal / RatingsCount, 2) : 0;
            ViewData["Ratings"] = RatingOutOfFive;

            return View();
        }
        public async Task<IActionResult> ManageDriver()
        {
            var drivers = await _driverRepository.GetAll(); 
            return View(drivers); 
        }
        public async Task<IActionResult> ManagePassenger()
        {
            var passengers = await _passengerRepository.GetAll(); 
            return View(passengers); 
        }
        public async Task<IActionResult> TripReport()
        {
            var userId = HttpContext.Session.GetInt32("UserID") ?? 0;
            var userRole = HttpContext.Session.GetString("UserType");
            if (userRole == "Admin")
            {
                var reservation = await _reservationRepository.GetAll();
                return View(reservation);
            }
            else if (userRole == "Passenger")
            {
                var reservation = await _reservationRepository.GetReservationByPassengerIDAsync(userId);
                return View(reservation);
            }
            else if (userRole == "Driver")
            {
                var reservation = await _reservationRepository.GetReservationByDriverIDAsync(userId);
                return View(reservation);
            }

            return View();
        }
        public async Task<IActionResult> ManageUser()
        {
            var user = await _systemUserRepository.GetAll();
            return View(user);
        }
        public IActionResult AdminLayout()
        {
            return View();
        }
        public IActionResult CreateDriver()
        {
            var model = new DriverViewModel
            {
                Banks = _driverRepository.GetAllBanks().ToList()
            };
            return View(model);
        }
        public IActionResult CreatePassenger()
        {
            return View();
        }
        public IActionResult CreateUser()
        {
            return View();
        }
        public async Task<IActionResult> RideDetailRV(string reservationId)
        {
            if (string.IsNullOrEmpty(reservationId) || !int.TryParse(reservationId, out int parsedReservationId))
            {
                return BadRequest("Invalid reservation ID.");
            }
            ViewData["Key"] = _configuration["GoogleApi:Key"];
            var model = await _reservationRepository.GetReservationByReservationIDAsync(parsedReservationId);
            var TripRating = await _driverRatingsRepository.GetRatingByReservationID(parsedReservationId);
            var stars = TripRating.Stars;

            if (model == null)
            {
                return NotFound("Reservation not found.");
            }
            ViewData["DriverID"] = stars;
            return View(model);
        }
        public async Task<IActionResult> EditDriver(int id)
        {
            var driver = await _driverRepository.GetByIdAsync(id);
            var login = await _loginRepository.GetByUserIdAsync(id);

            var branches = _driverRepository.GetBranchesByBankId(int.Parse(driver.BankCode)).ToList();

            if (driver != null)
            {
                var DriverEditModel = new DriverViewModel
                {
                    FirstName = driver.FirstName,
                    LastName = driver.LastName,
                    Email = driver.Email,
                    PhoneNumber = driver.PhoneNumber,
                    NIC = driver.NIC,
                    BankCode = driver.BankCode,
                    BranchID = driver.BranchID,
                    AccountNumber = driver.AccountNumber,
                    Banks = _driverRepository.GetAllBanks().ToList(),
                    Branches = branches,
                };

                ViewData["DriverID"] = driver.DriverID;
                ViewData["DateRegistered"] = driver.DateRegistered.ToString("dd/MM/yyyy");
                ViewData["UserName"] = login.Username;

                return View(DriverEditModel);
            }

            return RedirectToAction("ManageDriver");
        }
        public async Task<IActionResult> EditPassenger(int id)
        {
            var passenger = await _passengerRepository.GetByIdAsync(id);
            var login = await _loginRepository.GetByUserIdAsync(id);

            if (passenger != null)
            {
                var PassengerModel = new Passenger
                {
                    PassengerID = passenger.PassengerID,
                    FirstName = passenger.FirstName,
                    LastName = passenger.LastName,
                    Email = passenger.Email,
                    PhoneNumber = passenger.PhoneNumber,
                };

                ViewData["DateRegistered"] = passenger.DateRegistered.ToString("dd/MM/yyyy");
                ViewData["UserName"] = login.Username;

                return View(PassengerModel);
            }

            return RedirectToAction("ManageDriver");
        }
        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _systemUserRepository.GetByIdAsync(id);

            if (user != null)
            {
                ViewData["DateRegistered"] = user.DateRegistered.ToString("dd/MM/yyyy");

                return View(user);
            }

            return RedirectToAction("ManageUser");
        }
        [HttpPost]
        public async Task<IActionResult> CreateDriver(DriverViewModel driverVM, string username, string password)
        {
            ModelState.Remove("Banks");
            ModelState.Remove("Branches");
            if (!ModelState.IsValid)
            {
                return View(driverVM);
            }

            var driver = new Driver
            {
                FirstName = driverVM.FirstName,
                LastName = driverVM.LastName,
                Email = driverVM.Email,
                PhoneNumber = driverVM.PhoneNumber,
                NIC = driverVM.NIC,
                AccountNumber = driverVM.AccountNumber,
                BankCode = driverVM.BankCode,
                BranchID = driverVM.BranchID,
            };

            if (driverVM.ImageFile != null && driverVM.ImageFile.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await driverVM.ImageFile.CopyToAsync(memoryStream);
                    driver.Image = memoryStream.ToArray();
                }
            }

            _driverRepository.AddDriver(driver);

            var savedDriverId = driver.DriverID;

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            var login = new Login
            {
                Username = username,
                PasswordHash = passwordHash,
                UserType = "Driver",
                UserID = savedDriverId
            };

            _loginRepository.AddLogin(login);

            var driverstatus = new DriverStatus
            {
                DriverID = savedDriverId,
                Status = "OFFLINE",
                Latitude = "7.8731",
                Longitude = "80.7718"
            };

            _driverStatusRepository.AddStatus(driverstatus);

            var emailBody = $"Welcome aboard {driver.FirstName + " " + driver.LastName}! Your account has been created.\n\nUsername: {username}\nPassword: {password}\n\n Please keep these details highly confidential";
            await _emailService.SendEmailAsync(driver.Email, "Your City Taxi Account Information", emailBody);


            return RedirectToAction("ManageDriver");
        }
        [HttpPost]
        public async Task<IActionResult> UpdateDriver(DriverViewModel driverVM, int driverId, string username, string password)
        {
            // Remove validation for fields that are optional
            ModelState.Remove("password");
            ModelState.Remove("ImageFile");
            ModelState.Remove("Banks");
            ModelState.Remove("Branches");

            var driver = await _driverRepository.GetByIdAsync(driverId);

            if (!ModelState.IsValid)
            {
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        Console.WriteLine(error.ErrorMessage);
                    }
                }

                ViewData["DriverID"] = driver.DriverID;
                ViewData["DateRegistered"] = driver.DateRegistered.ToString("dd/MM/yyyy");

                return RedirectToAction("EditDriver");
            }

            if (driver != null)
            {
                if (driverVM.ImageFile != null && driverVM.ImageFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await driverVM.ImageFile.CopyToAsync(memoryStream);
                        driver.Image = memoryStream.ToArray();
                    }
                }

                driver.FirstName = driverVM.FirstName;
                driver.LastName = driverVM.LastName;
                driver.Email = driverVM.Email;
                driver.PhoneNumber = driverVM.PhoneNumber;
                driver.NIC = driverVM.NIC;
                driver.AccountNumber = driverVM.AccountNumber;
                driver.BankCode = driverVM.BankCode;
                driver.BranchID = driverVM.BranchID;

                _driverRepository.Update(driver);

                var login = await _loginRepository.GetByUserIdAsync(driverId);

                if (login.Username != username || password != null)
                {
                    login.Username = username;

                    if (password != null) 
                    {
                        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

                        login.PasswordHash = passwordHash;
                    }

                    _loginRepository.UpdateLogin(login);

                    var emailBody = $"Hi {driver.FirstName + " " + driver.LastName}! Your account details have been updated.\n\nUsername: {username}\nPassword: {password}\n\n Please keep these details highly confidential";
                    await _emailService.SendEmailAsync(driver.Email, "Your City Taxi Account Information", emailBody);
                }

                return RedirectToAction("ManageDriver");
            }

            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetDriverImage(int id)
        {
            var driver = await _driverRepository.GetByIdAsync(id);

            if (driver == null || driver.Image == null)
            {
                return NotFound();
            }

            return File(driver.Image, "image/jpeg");
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index","Home");
        }
        [HttpPost]
        public async Task<IActionResult> DeleteDriver(int driverId)
        {
            var driver = await _driverRepository.GetByIdAsync(driverId);
            var login = await _loginRepository.GetByUserIdAsync(driverId);

            if (driver != null)
            {
                _driverRepository.Delete(driver);
            }

            if (login != null)
            {
                _loginRepository.DeleteLogin(login);
            }

            return RedirectToAction("ManageDriver");
        }
        [HttpPost]
        public async Task<IActionResult> CreatePassenger(Passenger passenger, string username, string password)
        {
            ModelState.Remove("CurrentLatitude");
            ModelState.Remove("CurrentLongitude");
            if (!ModelState.IsValid)
            {
                return View(passenger);
            }

            _passengerRepository.AddPassenger(passenger);

            var savedPassengerId = passenger.PassengerID;

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            var login = new Login
            {
                Username = username,
                PasswordHash = passwordHash,
                UserType = "Passenger",
                UserID = savedPassengerId
            };

            _loginRepository.AddLogin(login);

            var emailBody = $"Welcome aboard {passenger.FirstName + " " + passenger.LastName}! Your account has been created.\n\nUsername: {username}\nPassword: {password}\n\n Please keep these details highly confidential";
            await _emailService.SendEmailAsync(passenger.Email, "Your City Taxi Account Information", emailBody);

            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> CreateUser(SystemUser systemUser, string username, string password, string usertype)
        {
            ModelState.Remove("Login");
            if (!ModelState.IsValid)
            {
                return View(systemUser);
            }

            if(usertype == null)
            {
                ViewData["ErrorMessage"] = "Please select a user type!";
                return View(systemUser);
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            var login = new Login
            {
                Username = username,
                PasswordHash = passwordHash,
                UserType = usertype,
                UserID = 0
            };

            _loginRepository.AddLogin(login);

            var savedLoginId = login.LoginID;

            systemUser.LoginID = savedLoginId;

            _systemUserRepository.Add(systemUser);

            var saveduserId = systemUser.SystemUserID;

            login.UserID = saveduserId;

            _loginRepository.UpdateLogin(login);

            var emailBody = $"Welcome aboard {systemUser.FirstName + " " + systemUser.LastName}! Your account has been created.\n\nUsername: {username}\nPassword: {password}\n\n Please keep these details highly confidential";
            await _emailService.SendEmailAsync(systemUser.Email, "Your City Taxi Account Information", emailBody);

            return RedirectToAction("ManageUser");
        }
        [HttpPost]
        public async Task<IActionResult> UpdateUser(SystemUser systemUser, string username, string password, string usertype)
        {
            ModelState.Remove("password");
            ModelState.Remove("Login");

            if (!ModelState.IsValid)
            {
                return View(systemUser);
            }

            var user = await _systemUserRepository.GetByIdAsync(systemUser.SystemUserID);

            if (user != null)
            {
                user.FirstName = systemUser.FirstName;
                user.LastName = systemUser.LastName;
                user.Email = systemUser.Email;
                user.PhoneNumber = systemUser.PhoneNumber;

                _systemUserRepository.Update(user);

                var login = await _loginRepository.GetByUserIdAsync(user.SystemUserID);

                if (usertype != "Empty")
                {
                    login.UserType = usertype;
                    _loginRepository.UpdateLogin(login);
                }

                if (login.Username != username || password != null)
                {
                    login.Username = username;

                    if (password != null)
                    {
                        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

                        login.PasswordHash = passwordHash;
                    }

                    _loginRepository.UpdateLogin(login);

                    var emailBody = $"Hi {user.FirstName + " " + user.LastName}! Your account details have been updated.\n\nUsername: {username}\nPassword: {password}\n\n Please keep these details highly confidential";
                    await _emailService.SendEmailAsync(user.Email, "Your City Taxi Account Information", emailBody);
                }

                return RedirectToAction("ManageUser");
            }

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            var user = await _systemUserRepository.GetByIdAsync(userId);
            var login = await _loginRepository.GetByUserIdAsync(userId);

            if (user != null)
            {
                _systemUserRepository.Delete(user);
            }

            if (login != null)
            {
                _loginRepository.DeleteLogin(login);
            }

            return RedirectToAction("ManageUser");
        }
        [HttpPost]
        public async Task<IActionResult> UpdatePassenger(Passenger passengerModel, string username, string password)
        {
            ModelState.Remove("password");

            var passenger = await _passengerRepository.GetByIdAsync(passengerModel.PassengerID);

            if (!ModelState.IsValid)
            {
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        Console.WriteLine(error.ErrorMessage);
                    }
                }

                ViewData["DateRegistered"] = passenger.DateRegistered.ToString("dd/MM/yyyy");

                return RedirectToAction("EditPassenger",passengerModel);
            }

            if (passenger != null)
            {
                passenger.FirstName = passengerModel.FirstName;
                passenger.LastName = passengerModel.LastName;
                passenger.Email = passengerModel.Email;
                passenger.PhoneNumber = passengerModel.PhoneNumber;

                _passengerRepository.Update(passenger);

                var login = await _loginRepository.GetByUserIdAsync(passenger.PassengerID);

                if (login.Username != username || password != null)
                {
                    login.Username = username;

                    if (password != null)
                    {
                        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

                        login.PasswordHash = passwordHash;
                    }

                    _loginRepository.UpdateLogin(login);

                    var emailBody = $"Hi {passenger.FirstName + " " + passenger.LastName}! Your account details have been updated.\n\nUsername: {username}\nPassword: {password}\n\n Please keep these details highly confidential";
                    await _emailService.SendEmailAsync(passenger.Email, "Your City Taxi Account Information", emailBody);
                }

                return RedirectToAction("ManagePassenger");
            }

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> DeletePassenger(int passengerId)
        {
            var passenger = await _passengerRepository.GetByIdAsync(passengerId);
            var login = await _loginRepository.GetByUserIdAsync(passengerId);

            if (passenger != null)
            {
                _passengerRepository.Delete(passenger);
            }

            if (login != null)
            {
                _loginRepository.DeleteLogin(login);
            }

            return RedirectToAction("ManagePassenger");
        }
        [HttpGet]
        public List<Banks> GetAllBanks()
        {

            var Banks = _driverRepository.GetAllBanks().ToList();

            return Banks;
        }
    }
}
