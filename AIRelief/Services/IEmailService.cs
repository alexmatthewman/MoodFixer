using System.Threading.Tasks;

namespace AIRelief.Services
{
    public interface IEmailService
    {
        Task SendAsync(string toEmail, string subject, string body);
    }
}
