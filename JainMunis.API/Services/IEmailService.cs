using System.Collections.Generic;
using System.Threading.Tasks;

namespace JainMunis.API.Services;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string to, string subject, string htmlContent, string? plainTextContent = null);
    Task<bool> SendEmailWithTemplateAsync(string to, string templateId, Dictionary<string, string> templateData);
    Task<bool> SendBatchEmailsAsync(List<string> recipients, string subject, string htmlContent, string? plainTextContent = null);
    Task<bool> SendWeeklyDigestAsync(string to, Dictionary<string, object> digestData);
    Task<bool> SendScheduleNotificationAsync(string to, Dictionary<string, object> scheduleData);
    Task<bool> SendWelcomeEmailAsync(string to, string userName);
    Task<bool> SendVerificationEmailAsync(string to, string verificationToken);
}