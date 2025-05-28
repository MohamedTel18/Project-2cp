using System.Threading.Tasks;

namespace RestaurantApp.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
        Task SendAccountActivationEmailAsync(string email, string username, string userId, string token);
        Task SendPasswordResetEmailAsync(string email, string token);
    }
}