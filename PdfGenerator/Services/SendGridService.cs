using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Macaw.Pdf.Services
{
    public class SendGridService : ISendGridService
    {
        private readonly IConfiguration Configuration;
        private readonly ISendGridClient SendGridClient;

        public SendGridService(ISendGridClient sendGridClient, IConfiguration configuration)
        {
            SendGridClient = sendGridClient;
            Configuration = configuration;
        }

        public async Task SendPdfToRecipient(EmailAddress To, string subject, string body, Dictionary<string, string> props, string documentPath)
        {
            await SendMail(To, subject, body, props, documentPath);
        }

        private async Task SendMail(EmailAddress To, string subject, string body, Dictionary<string, string> substitutions, string documentPath)
        {
            var from = new EmailAddress(Configuration["SendGrid_From"]);
            var message = new SendGridMessage
            {
                From = from,
                Subject = subject
            };
            message.AddTo(To, 0, new Personalization() { Substitutions = substitutions });
            message.AddContent(MimeType.Html, body);
            message.AddAttachment("InspectieRapport.pdf", Convert.ToBase64String(File.ReadAllBytes(documentPath)), "application/pdf", "attachment");
            var response = await SendGridClient.SendEmailAsync(message);

            Debug.WriteLine($"Sending mail to ${To.Email} with response from sendgrid ${response.StatusCode}");
            if (!response.IsSuccessStatusCode)
            {
                try
                {
                    var content = await response.Body.ReadAsStringAsync();
                    throw new WebException($"sendgrid came back with an error response {response.StatusCode} error ${response}, Content : [{content}]"); ;
                }
                catch (Exception ex)
                {
                    throw new WebException($"sendgrid came back with an error response {response.StatusCode} error ${response}"); ;
                }
            }
        }
    }
}