using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace IncedentsAPI.Models
{
    public class Contact
    {
        [Key]
        [Required]
        [StringLength(100)]
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [StringLength(100)]
        public string? AccountName { get; set; }
        [ForeignKey("AccountName")]
        [JsonIgnore]
        public Account Account { get; set; }
    }
}
