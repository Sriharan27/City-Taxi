using City_Taxi.Data;
using City_Taxi.Interfaces;
using City_Taxi.Models;
using City_Taxi.Repository;
using City_Taxi.Service;
using City_Taxi.ViewModel;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using SendGrid.Helpers.Mail;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace City_Taxi.Controllers
{
    public class HomeController : Controller
    {
        private readonly SmsService _smsService;

        private readonly EmailService _emailService;

        private static readonly Dictionary<string, string> OtpStorage = new();

        private readonly ILogger<HomeController> _logger;

        private readonly IPassengerRepository _passengerRepository;

        private readonly IDriverRepository _driverRepository;

        private readonly ILoginRepository _loginRepository;

        private readonly ISystemUserRepository _systemUserRepository;

        private readonly IDriverStatusRepository _driverStatusRepository;
        public HomeController(ILogger<HomeController> logger, IPassengerRepository passengerRepository, IDriverRepository driverRepository, ILoginRepository loginRepository, EmailService emailService, ISystemUserRepository systemUserRepository, SmsService smsService, IDriverStatusRepository driverStatusRepository)
        {
            _logger = logger;
            _passengerRepository = passengerRepository;
            _driverRepository = driverRepository;
            _loginRepository = loginRepository;
            _emailService = emailService;
            _systemUserRepository = systemUserRepository;
            _smsService = smsService;
            _driverStatusRepository = driverStatusRepository;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult PassengerSignUp()
        {
            return View();
        }
        public IActionResult DriverSignUp()
        {
            var model = new DriverViewModel
            {
                Banks = _driverRepository.GetAllBanks().ToList()
            };
            return View(model);
        }
        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<IActionResult> PassengerSignUp(Passenger passenger, string username, string password)
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
        public async Task<IActionResult> DriverSignUp(DriverViewModel driverVM, string username, string password)
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

            var emailBody = $"Welcome aboard {driver.FirstName +" "+ driver.LastName}! Your account has been created.\n\nUsername: {username}\nPassword: {password}\n\n Please keep these details highly confidential";
            await _emailService.SendEmailAsync(driver.Email, "Your City Taxi Account Information", emailBody);


            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewData["ErrorMessage"] = "Username and password are required.";
                return View("Index");
            }

            var login = await _loginRepository.GetByUsernameAsync(username);
            if (login == null)
            {
                ViewData["ErrorMessage"] = "Invalid username or password.";
                return View("Index");
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, login.PasswordHash);
            if (!isPasswordValid)
            {
                ViewData["ErrorMessage"] = "Invalid username or password.";
                return View("Index");
            }

            switch (login.UserType)
            {
                case "Passenger":
                    var passenger = await _passengerRepository.GetByIdAsync(login.UserID);
                    HttpContext.Session.SetString("Username", passenger.FirstName +" "+ passenger.LastName);
                    HttpContext.Session.SetString("UserType", login.UserType);
                    HttpContext.Session.SetInt32("UserID", login.UserID);
                    return RedirectToAction("Index", "Passenger");
                case "Driver":
                    var driver = await _driverRepository.GetByIdAsync(login.UserID);
                    var driverStatus = await _driverStatusRepository.GetDriverStatusAsync(driver.DriverID);
                    driverStatus.Status = "AVAILABLE";
                    _driverStatusRepository.UpdateStatus(driverStatus);
                    HttpContext.Session.SetString("Username", driver.FirstName + " " + driver.LastName);
                    HttpContext.Session.SetString("UserType", login.UserType);
                    HttpContext.Session.SetInt32("UserID", login.UserID);
                    return RedirectToAction("Index", "Driver");
                case "Admin":
                    var user = await _systemUserRepository.GetByIdAsync(login.UserID);
                    HttpContext.Session.SetString("Username", user.FirstName + " " + user.LastName);
                    HttpContext.Session.SetString("UserType", login.UserType);
                    HttpContext.Session.SetInt32("UserID", login.UserID);
                    return RedirectToAction("Index", "Admin");
                case "TeleOperator":
                    var teleoperator = await _systemUserRepository.GetByIdAsync(login.UserID);
                    HttpContext.Session.SetString("Username", teleoperator.FirstName + " " + teleoperator.LastName);
                    HttpContext.Session.SetString("UserType", login.UserType);
                    HttpContext.Session.SetInt32("UserID", login.UserID);
                    return RedirectToAction("Index", "Operator");
                default:
                    ViewData["ErrorMessage"] = "Unknown user type.";
                    return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string phonenumber, string password, string rpassword)
        {
            if (password != rpassword)
            {
                ViewData["ErrorMessage"] = "Passwords do not match.";
                return View();
            }

            var login = await _loginRepository.GetByUsernameAsync(phonenumber);
            if (login == null)
            {
                ViewData["ErrorMessage"] = "Invalid phone number.";
                return View();
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            login.PasswordHash = passwordHash;

            _loginRepository.UpdateLogin(login);

            dynamic Detail = null;

            if (login.UserType == "Passenger") {
                 Detail = await _passengerRepository.GetByIdAsync(login.UserID);
            }
            else if (login.UserType == "Driver")
            {
                 Detail = await _driverRepository.GetByIdAsync(login.UserID);
            }

            var emailBody = $"Hi {Detail.FirstName + " " + Detail.LastName}! Your Password has been reset succesfully.\n\nUsername: {phonenumber}\nPassword: {password}\n\n Please keep these details highly confidential";
            await _emailService.SendEmailAsync(Detail.Email, "Your City Taxi Account Information", emailBody);

            return RedirectToAction("Index");
        }
        //----------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> SendOtp([FromBody] PhoneNumberModel model)
        {
            var phoneNumberWithCode = "+94" + model.PhoneNumber;
            var login = await _loginRepository.GetByUsernameAsync(phoneNumberWithCode);

            if (login != null)
            {
                return Json(new { success = false, errorMessage = "User already exists. To reset the password, please try the forgot password option." });
            }

            var otp = new Random().Next(1000, 9999).ToString();

            var message = $"City Taxi\n\n\nYour OTP is: {otp}\n\n\nDo not share with anyone!";

            var result = await _smsService.SendSmsAsync(phoneNumberWithCode, message);

            if (result.StartsWith("OK"))
            {
                OtpStorage[model.PhoneNumber] = otp;

                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        [HttpPost]
        public IActionResult VerifyOtp([FromBody] OtpModel model)
        {
            if (OtpStorage.TryGetValue(model.PhoneNumber, out var storedOtp) && storedOtp == model.Otp)
            {
                OtpStorage.Remove(model.PhoneNumber);
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear(); // Clear session data
            return RedirectToAction("Index");
        }
    }
    public class PhoneNumberModel
    {
        public string PhoneNumber { get; set; }
    }

    public class OtpModel
    {
        public string Otp { get; set; }
        public string PhoneNumber { get; set; }
    }
}
