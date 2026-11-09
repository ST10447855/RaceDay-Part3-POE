using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class RaceEvent
    {
        [Key]
        public int EventId { get; set; }

        [Required]
        [MaxLength(100)]
        public string EventName { get; set; } = string.Empty;

        [Required]
        public DateTime EventDate { get; set; }

        [MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
    }

    public class Registration
    {
        [Key]
        public int RegistrationId { get; set; }

        [Required]
        public int EventId { get; set; }

        [ForeignKey("EventId")]
        public RaceEvent? RaceEvent { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty; // Links to your ApplicationUser

        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        public string Status { get; set; } = "Confirmed";
    }
}
    