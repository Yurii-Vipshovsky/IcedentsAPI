using System.ComponentModel.DataAnnotations;

namespace IncedentsAPI.Models
{
    public class RequestBody
    {
        [Required]
        public string AccountName { get; set; }
        public string ContactFirstName { get; set; }
        public string ContactLastName { get; set; }
        [Required]
        public string ContactEmail { get; set; }
        public string IncidentDescription { get; set; }
    }
}
