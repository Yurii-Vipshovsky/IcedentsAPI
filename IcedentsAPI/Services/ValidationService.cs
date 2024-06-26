using System.Text.RegularExpressions;

namespace IncedentsAPI.Services
{
    public class ValidationService
    {
        public static bool IsValidEmail(string email)
        {
            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailPattern);
        }
    }
}
