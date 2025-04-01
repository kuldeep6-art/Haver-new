using System.ComponentModel.DataAnnotations;

namespace haver.Models
{
    public class UserSelection
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string SelectionJson { get; set; } // Store selection as JSON

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
