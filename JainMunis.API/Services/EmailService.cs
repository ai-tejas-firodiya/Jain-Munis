using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace JainMunis.API.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly ISendGridClient _sendGridClient;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        var apiKey = _configuration.GetSection("EmailService:SendGrid:ApiKey").Value;
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("SendGrid API key is not configured.");
        }

        _sendGridClient = new SendGridClient(apiKey);
        _fromEmail = _configuration.GetSection("EmailService:SendGrid:FromEmail").Value ?? "noreply@jainmunis.app";
        _fromName = _configuration.GetSection("EmailService:SendGrid:FromName").Value ?? "Jain Munis App";
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string htmlContent, string? plainTextContent = null)
    {
        try
        {
            var from = new EmailAddress(_fromEmail, _fromName);
            var toAddress = new EmailAddress(to);
            var msg = MailHelper.CreateSingleEmail(from, toAddress, subject, plainTextContent, htmlContent);

            var response = await _sendGridClient.SendEmailAsync(msg);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email sent successfully to {Email}", to);
                return true;
            }
            else
            {
                _logger.LogError("Failed to send email to {Email}. Status: {StatusCode}, Body: {Body}",
                    to, response.StatusCode, await response.Body.ReadAsStringAsync());
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {Email}", to);
            return false;
        }
    }

    public async Task<bool> SendEmailWithTemplateAsync(string to, string templateId, Dictionary<string, string> templateData)
    {
        try
        {
            var from = new EmailAddress(_fromEmail, _fromName);
            var toAddress = new EmailAddress(to);
            var msg = MailHelper.CreateSingleTemplateEmail(from, toAddress, templateId, templateData);

            var response = await _sendGridClient.SendEmailAsync(msg);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Template email sent successfully to {Email}", to);
                return true;
            }
            else
            {
                _logger.LogError("Failed to send template email to {Email}. Status: {StatusCode}",
                    to, response.StatusCode);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending template email to {Email}", to);
            return false;
        }
    }

    public async Task<bool> SendBatchEmailsAsync(List<string> recipients, string subject, string htmlContent, string? plainTextContent = null)
    {
        try
        {
            var from = new EmailAddress(_fromEmail, _fromName);
            var toAddresses = recipients.Select(email => new EmailAddress(email)).ToList();

            // Create a single email with multiple recipients
            var msg = MailHelper.CreateSingleEmail(from, toAddresses[0], subject, plainTextContent, htmlContent);
            
            // Add additional recipients
            for (int i = 1; i < toAddresses.Count; i++)
            {
                msg.AddTo(toAddresses[i]);
            }

            var response = await _sendGridClient.SendEmailAsync(msg);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Batch email sent successfully to {Count} recipients", recipients.Count);
                return true;
            }
            else
            {
                _logger.LogError("Failed to send batch email. Status: {StatusCode}", response.StatusCode);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending batch email to {Count} recipients", recipients.Count);
            return false;
        }
    }

    public async Task<bool> SendWeeklyDigestAsync(string to, Dictionary<string, object> digestData)
    {
        try
        {
            var subject = $"Weekly Saint Schedule Digest - {DateTime.Now:MMMM dd, yyyy}";

            var htmlContent = GenerateWeeklyDigestHtml(digestData);
            var plainTextContent = GenerateWeeklyDigestText(digestData);

            return await SendEmailAsync(to, subject, htmlContent, plainTextContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending weekly digest to {Email}", to);
            return false;
        }
    }

    public async Task<bool> SendScheduleNotificationAsync(string to, Dictionary<string, object> scheduleData)
    {
        try
        {
            var saintName = scheduleData.GetValueOrDefault("saintName")?.ToString() ?? "Saint";
            var locationName = scheduleData.GetValueOrDefault("locationName")?.ToString() ?? "Location";
            var startDate = scheduleData.GetValueOrDefault("startDate")?.ToString() ?? "";
            var endDate = scheduleData.GetValueOrDefault("endDate")?.ToString() ?? "";

            var subject = $"{saintName} in {locationName} - Schedule Update";

            var htmlContent = GenerateScheduleNotificationHtml(scheduleData);
            var plainTextContent = GenerateScheduleNotificationText(scheduleData);

            return await SendEmailAsync(to, subject, htmlContent, plainTextContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending schedule notification to {Email}", to);
            return false;
        }
    }

    public async Task<bool> SendWelcomeEmailAsync(string to, string userName)
    {
        try
        {
            var subject = "Welcome to Jain Munis App";

            var htmlContent = GenerateWelcomeHtml(userName);
            var plainTextContent = GenerateWelcomeText(userName);

            return await SendEmailAsync(to, subject, htmlContent, plainTextContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending welcome email to {Email}", to);
            return false;
        }
    }

    public async Task<bool> SendVerificationEmailAsync(string to, string verificationToken)
    {
        try
        {
            var subject = "Verify Your Email - Jain Munis App";
            var verificationLink = $"{_configuration["App:BaseUrl"]}/verify-email?token={verificationToken}";

            var htmlContent = GenerateVerificationHtml(verificationLink);
            var plainTextContent = GenerateVerificationText(verificationLink);

            return await SendEmailAsync(to, subject, htmlContent, plainTextContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending verification email to {Email}", to);
            return false;
        }
    }

    private string GenerateWeeklyDigestHtml(Dictionary<string, object> digestData)
    {
        var cities = digestData.GetValueOrDefault("cities") as List<object> ?? new List<object>();
        var totalSaints = digestData.GetValueOrDefault("totalSaints")?.ToString() ?? "0";

        var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Weekly Saint Schedule Digest</title>
</head>
<body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background-color: #f8f9fa; padding: 20px; border-radius: 8px; text-align: center;'>
        <h1 style='color: #2c3e50; margin: 0;'>🙏 Weekly Saint Schedule Digest</h1>
        <p style='color: #7f8c8d; margin: 10px 0;'>
            {DateTime.Now:MMMM dd, yyyy} • {totalSaints} saints visiting this week
        </p>
    </div>

    <div style='margin: 20px 0;'>
        <h2 style='color: #2c3e50; border-bottom: 2px solid #3498db; padding-bottom: 10px;'>
            Upcoming Saint Visits
        </h2>";

        foreach (dynamic cityData in cities)
        {
            var cityName = cityData.cityName ?? "Unknown City";
            var saintCount = cityData.saintCount ?? 0;
            var saints = cityData.saints as List<object> ?? new List<object>();

            html += $@"
        <div style='background-color: #ffffff; border: 1px solid #e1e8ed; border-radius: 8px; margin: 15px 0; padding: 15px;'>
            <h3 style='color: #2c3e50; margin: 0 0 10px 0;'>📍 {cityName}</h3>
            <p style='color: #7f8c8d; margin: 0 0 10px 0;'>{saintCount} saints visiting</p>";

            foreach (dynamic saint in saints)
            {
                var saintName = saint.name ?? "Unknown Saint";
                var saintTitle = saint.title ?? "";
                var location = saint.location ?? "";
                var dates = saint.dates ?? "";

                html += $@"
            <div style='margin: 10px 0; padding: 10px; background-color: #f8f9fa; border-radius: 5px;'>
                <strong style='color: #2c3e50;'>{saintTitle} {saintName}</strong><br>
                <span style='color: #7f8c8d;'>📍 {location}</span><br>
                <span style='color: #7f8c8d;'>📅 {dates}</span>
            </div>";
            }

            html += "</div>";
        }

        html += @"
    </div>

    <div style='background-color: #ecf0f1; padding: 20px; border-radius: 8px; text-align: center; margin: 20px 0;'>
        <p style='color: #7f8c8d; margin: 0;'>
            <a href='[UnsubscribeLink]' style='color: #e74c3c; text-decoration: none;'>Unsubscribe</a> |
            <a href='[PreferencesLink]' style='color: #3498db; text-decoration: none;'>Manage Preferences</a>
        </p>
    </div>
</body>
</html>";

        return html;
    }

    private string GenerateWeeklyDigestText(Dictionary<string, object> digestData)
    {
        var cities = digestData.GetValueOrDefault("cities") as List<object> ?? new List<object>();
        var text = $"🙏 Weekly Saint Schedule Digest - {DateTime.Now:MMMM dd, yyyy}\n\n";
        text += "Upcoming Saint Visits:\n\n";

        foreach (dynamic cityData in cities)
        {
            var cityName = cityData.cityName ?? "Unknown City";
            var saints = cityData.saints as List<object> ?? new List<object>();

            text += $"\n📍 {cityName}\n";
            foreach (dynamic saint in saints)
            {
                text += $"  • {saint.title} {saint.name} - {saint.location} ({saint.dates})\n";
            }
        }

        return text;
    }

    private string GenerateScheduleNotificationHtml(Dictionary<string, object> scheduleData)
    {
        var saintName = scheduleData.GetValueOrDefault("saintName")?.ToString() ?? "Saint";
        var saintTitle = scheduleData.GetValueOrDefault("saintTitle")?.ToString() ?? "";
        var locationName = scheduleData.GetValueOrDefault("locationName")?.ToString() ?? "Location";
        var startDate = scheduleData.GetValueOrDefault("startDate")?.ToString() ?? "";
        var endDate = scheduleData.GetValueOrDefault("endDate")?.ToString() ?? "";
        var purpose = scheduleData.GetValueOrDefault("purpose")?.ToString() ?? "";
        var contactPerson = scheduleData.GetValueOrDefault("contactPerson")?.ToString() ?? "";
        var contactPhone = scheduleData.GetValueOrDefault("contactPhone")?.ToString() ?? "";

        var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Schedule Update - {saintName}</title>
</head>
<body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;	padding: 20px;'>
    <div style='background-color: #e8f5e8; padding: 20px; border-radius: 8px; text-align: center; border-left: 5px solid #28a745;'>
        <h1 style='color: #155724; margin: 0;'>🙏 Schedule Update</h1>
        <p style='color: #155724; margin: 10px 0; font-size: 18px;'>
            {saintTitle} {saintName}
        </p>
    </div>

    <div style='margin: 20px 0; background-color: #ffffff; border: 1px solid #e1e8ed; border-radius: 8px; padding: 20px;'>
        <h2 style='color: #2c3e50; margin: 0 0 15px 0;'>📍 {locationName}</h2>

        <div style='margin: 10px 0;'>
            <strong style='color: #2c3e50;'>📅 Visit Period:</strong><br>
            <span style='color: #7f8c8d;'>{startDate} to {endDate}</span>
        </div>";

        if (!string.IsNullOrEmpty(purpose))
        {
            html += $@"
        <div style='margin: 10px 0;'>
            <strong style='color: #2c3e50;'>🎯 Purpose:</strong><br>
            <span style='color: #7f8c8d;'>{purpose}</span>
        </div>";
        }

        if (!string.IsNullOrEmpty(contactPerson))
        {
            html += $@"
        <div style='margin: 10px 0;'>
            <strong style='color: #2c3e50;'>📞 Local Contact:</strong><br>
            <span style='color: #7f8c8d;'>{contactPerson}";

            if (!string.IsNullOrEmpty(contactPhone))
            {
                html += $" - {contactPhone}";
            }

            html += "</span></div>";
        }

        html += $@"
        <div style='text-align: center; margin-top: 20px;'>
            <a href='[ViewDetailsLink]' style='background-color: #007bff; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; font-weight: bold;'>
                View Details
            </a>
        </div>
    </div>

    <div style='background-color: #f8f9fa; padding: 15px; border-radius: 8px; text-align: center; margin: 20px 0;'>
        <p style='color: #6c757d; margin: 0; font-size: 14px;'>
            Receive notifications like this by subscribing to our service<br>
            <a href='[ManagePreferencesLink]' style='color: #007bff; text-decoration: none;'>Manage Preferences</a>
        </p>
    </div>
</body>
</html>";

        return html;
    }

    private string GenerateScheduleNotificationText(Dictionary<string, object> scheduleData)
    {
        var saintName = scheduleData.GetValueOrDefault("saintName")?.ToString() ?? "Saint";
        var saintTitle = scheduleData.GetValueOrDefault("saintTitle")?.ToString() ?? "";
        var locationName = scheduleData.GetValueOrDefault("locationName")?.ToString() ?? "Location";
        var startDate = scheduleData.GetValueOrDefault("startDate")?.ToString() ?? "";
        var endDate = scheduleData.GetValueOrDefault("endDate")?.ToString() ?? "";
        var purpose = scheduleData.GetValueOrDefault("purpose")?.ToString() ?? "";

        var text = $"🙏 Schedule Update\n\n{saintTitle} {saintName}\n\n";
        text += $"📍 {locationName}\n";
        text += $"📅 {startDate} to {endDate}\n";

        if (!string.IsNullOrEmpty(purpose))
        {
            text += $"🎯 {purpose}\n";
        }

        text += "\nView details: [ViewDetailsLink]";
        text += "\nManage preferences: [ManagePreferencesLink]";

        return text;
    }

    private string GenerateWelcomeHtml(string userName)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Welcome to Jain Munis App</title>
</head>
<body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; border-radius: 8px; text-align: center;'>
        <h1 style='margin: 0; font-size: 28px;'>🙏 Welcome, {userName}!</h1>
        <p style='margin: 15px 0; font-size: 18px;'>to the Jain Munis App</p>
    </div>

    <div style='margin: 20px 0; background-color: #ffffff; border: 1px solid #e1e8ed; border-radius: 8px; padding: 20px;'>
        <h2 style='color: #2c3e50; margin: 0 0 15px 0;'>Get Started</h2>

        <div style='margin: 20px 0;'>
            <div style='display: flex; align-items: center; margin-bottom: 15px;'>
                <div style='background-color: #e3f2fd; width: 40px; height: 40px; border-radius: 50%; display: flex; align-items: center; justify-content: center; margin-right: 15px;'>
                    <span>🔍</span>
                </div>
                <div>
                    <h3 style='margin: 0; color: #2c3e50;'>Find Saints</h3>
                    <p style='margin: 5px 0; color: #7f8c8d;'>Search for saints by name or location</p>
                </div>
            </div>

            <div style='display: flex; align-items: center; margin-bottom: 15px;'>
                <div style='background-color: #e8f5e8; width: 40px; height: 40px; border-radius: 50%; display: flex; align-items: center; justify-content: center; margin-right: 15px;'>
                    <span>📅</span>
                </div>
                <div>
                    <h3 style='margin: 0; color: #2c3e50;'>View Schedules</h3>
                    <p style='margin: 5px 0; color: #7f8c8d;'>See upcoming saint visits and programs</p>
                </div>
            </div>

            <div style='display: flex; align-items: center;'>
                <div style='background-color: #fff3e0; width: 40px; height: 40px; border-radius: 50%; display: flex; align-items: center; justify-content: center; margin-right: 15px;'>
                    <span>🔔</span>
                </div>
                <div>
                    <h3 style='margin: 0; color: #2c3e50;'>Get Notifications</h3>
                    <p style='margin: 5px 0; color: #7f8c8d;'>Receive alerts about saints in your area</p>
                </div>
            </div>
        </div>

        <div style='text-align: center; margin-top: 25px;'>
            <a href='[AppUrl]' style='background-color: #007bff; color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; font-weight: bold; font-size: 16px;'>
                Start Using the App
            </a>
        </div>
    </div>

    <div style='background-color: #f8f9fa; padding: 15px; border-radius: 8px; text-align: center; margin: 20px 0;'>
        <p style='color: #6c757d; margin: 0; font-size: 14px;'>
            If you have any questions, feel free to contact us<br>
            <a href='mailto:support@jainmunis.app' style='color: #007bff; text-decoration: none;'>support@jainmunis.app</a>
        </p>
    </div>
</body>
</html>";
    }

    private string GenerateWelcomeText(string userName)
    {
        return $@"🙏 Welcome to Jain Munis App, {userName}!

Thank you for joining our community. Stay connected with saint schedules and spiritual events in your area.

What you can do:
• 🔍 Search for saints by name or location
• 📅 View upcoming schedules and programs
• 🔔 Receive notifications about nearby saints
• 📍 Find saints currently in your city

Get started: [AppUrl]

Questions? Contact us at support@jainmunis.app";
    }

    private string GenerateVerificationHtml(string verificationLink)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Verify Your Email - Jain Munis App</title>
</head>
<body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background-color: #f8f9fa; padding: 30px; border-radius: 8px; text-align: center; border-left: 5px solid #007bff;'>
        <h1 style='color: #2c3e50; margin: 0;'>✉️ Verify Your Email Address</h1>
        <p style='color: #7f8c8d; margin: 15px 0;'>Please click the button below to verify your email and complete your registration.</p>
    </div>

    <div style='margin: 30px 0; text-align: center;'>
        <a href='{verificationLink}' style='background-color: #28a745; color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; font-weight: bold; font-size: 16px; display: inline-block;'>
            Verify Email Address
        </a>
    </div>

    <div style='background-color: #fff3cd; border: 1px solid #ffeaa7; border-radius: 8px; padding: 15px; margin: 20px 0;'>
        <p style='color: #856404; margin: 0; text-align: center;'>
            <strong>Important:</strong> This verification link will expire in 24 hours.
        </p>
    </div>

    <div style='text-align: center; margin: 20px 0;'>
        <p style='color: #7f8c8d; margin: 0;'>If the button doesn't work, copy and paste this link into your browser:</p>
        <p style='color: #007bff; margin: 10px 0; word-break: break-all; font-size: 14px;'>{verificationLink}</p>
    </div>

    <div style='background-color: #f8f9fa; padding: 15px; border-radius: 8px; text-align: center; margin: 20px 0;'>
        <p style='color: #6c757d; margin: 0; font-size: 14px;'>
            If you didn't request this verification, please ignore this email.
        </p>
    </div>
</body>
</html>";
    }

    private string GenerateVerificationText(string verificationLink)
    {
        return $@"✉️ Verify Your Email Address

Please click the link below to verify your email and complete your registration:

{verificationLink}

Important: This verification link will expire in 24 hours.

If you didn't request this verification, please ignore this email.";
    }
}