using System.ComponentModel.DataAnnotations;

namespace IncedentsAPI.Models
{
    public class NewAccountModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string ContactEmail { get; set; }
        public string? IncedentName { get; set; }
    }
}
