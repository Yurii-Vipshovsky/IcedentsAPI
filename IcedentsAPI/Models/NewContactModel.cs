using System.ComponentModel.DataAnnotations;

namespace IncedentsAPI.Models
{
    public class NewContactModel
    {
        [Required]
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? AccountName { get; set; }
    }
}
