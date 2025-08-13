using System;
using System.Collections.Generic;
using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Client;
using sib_api_v3_sdk.Model;

namespace ElectronicsStore.Services
{
    public class EmailSender
    {
        /// <summary>
        /// Sends a transactional email via Brevo (Sendinblue).
        /// </summary>
        public static void SendEmail(
             string apiKey,
            string senderName,
            string senderEmail,
            string recipientName,
            string recipientEmail,
            string subject,
            string htmlContent = null,
            long? templateId = null,
            Dictionary<string, object> parameters = null,
            DateTime? scheduledAt = null  // Changed from string to nullable DateTime
        )
        {
            Configuration.Default.ApiKey.Clear();
            Configuration.Default.ApiKey.Add("api-key", apiKey);

            var apiInstance = new TransactionalEmailsApi();

            var sender = new SendSmtpEmailSender(name: senderName, email: senderEmail);
            var to = new List<SendSmtpEmailTo>
            {
                new SendSmtpEmailTo(email: recipientEmail, name: recipientName)
            };

            var emailRequest = new SendSmtpEmail(
                sender: sender,
                to: to,
                subject: subject
            );

            if (!string.IsNullOrEmpty(htmlContent))
            {
                emailRequest.HtmlContent = htmlContent;
            }

            if (templateId.HasValue)
            {
                emailRequest.TemplateId = templateId;
                if (parameters != null && parameters.Count > 0)
                {
                    emailRequest.Params = parameters;
                }
            }

            if (scheduledAt.HasValue)
            {
                emailRequest.ScheduledAt = scheduledAt.Value;  // Correct type assignment
            }

            try
            {
                CreateSmtpEmail result = apiInstance.SendTransacEmail(emailRequest);
                Console.WriteLine("Email scheduled/sent! Message ID: " + result.MessageId);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error sending email: " + ex.Message);
            }
        }
    }
}
