using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace IncedentsAPI.Models
{
    public class Account
    {
        [Key]
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        [StringLength(100)]
        public string? IncedentName { get; set; }
        [ForeignKey("IncedentName")]
        [JsonIgnore]
        public Incident Incident { get; set; }
        public ICollection<Contact> Contacts { get; set; }
    }
}
