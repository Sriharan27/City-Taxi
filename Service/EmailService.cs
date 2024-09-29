using System;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

public class EmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var apiKey = _configuration["SendGrid:ApiKey"];
        var client = new SendGridClient(apiKey);
        var from = new EmailAddress("Sriharanmoorthy@gmail.com", "City Taxi");
        var to = new EmailAddress(toEmail);
        var plainTextContent = body;
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, null);

        var response = await client.SendEmailAsync(msg);
    }
}
