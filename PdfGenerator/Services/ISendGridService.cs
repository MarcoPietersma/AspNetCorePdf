using SendGrid.Helpers.Mail;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Macaw.Pdf.Services
{
    public interface ISendGridService
    {
        Task SendPdfToRecipient(EmailAddress To, string subject, string body, Dictionary<string, string> props, string documentPath);
    }
}