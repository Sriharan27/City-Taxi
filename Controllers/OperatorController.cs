using City_Taxi.Interfaces;
using City_Taxi.Models;
using Microsoft.AspNetCore.Mvc;

namespace City_Taxi.Controllers
{
    public class OperatorController : Controller
    {
        private readonly IPassengerRepository _passengerRepository;
        private readonly ILoginRepository _loginRepository;
        private readonly EmailService _emailService;
        public readonly IConfiguration _configuration;
        public OperatorController(IPassengerRepository passengerRepository, ILoginRepository loginRepository, EmailService emailService, IConfiguration configuration)
        {
            _emailService = emailService;
            _passengerRepository = passengerRepository;
            _loginRepository = loginRepository;
            _configuration = configuration;
        }
        public IActionResult Index()
        {
            ViewData["Key"] = _configuration["GoogleApi:Key"];
            return View();
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

            HttpContext.Session.Clear();

            HttpContext.Session.SetString("Username", passenger.FirstName + " " + passenger.LastName);
            HttpContext.Session.SetString("UserType", login.UserType);
            HttpContext.Session.SetInt32("UserID", login.UserID);

            return RedirectToAction("Index", "Passenger");
        }
    }
}
