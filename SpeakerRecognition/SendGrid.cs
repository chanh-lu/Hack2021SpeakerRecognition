using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace SpeakerRecognition
{
    public static class SendGrid
    {
        public static async Task<bool> Send(string task, string speechToText)
        {
            var client = new SendGridClient(Settings.SendGridApiKey);
            var from = new EmailAddress("", "Holla Back");
            var subject = $"[{speechToText}] {task}";
            var to = new EmailAddress("", "Holla Back");
            var plainTextContent = task;
            var htmlContent = $"<strong>{speechToText}</strong>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);

            return response.IsSuccessStatusCode;
        }
    }
}
