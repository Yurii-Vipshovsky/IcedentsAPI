using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IncedentsAPI.Models
{
    public class Incident
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [StringLength(100)]
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<Account> Accounts { get; set; }
    }
}
