using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class Event
    {
        [Key]
        public int EventID { get; set; }

        public int OrganiserID { get; set; }
        [ForeignKey("OrganiserID")]
        public User Organiser { get; set; } = null!;

        public int EventTypeID { get; set; }
        [ForeignKey("EventTypeID")]
        public EventType EventType { get; set; } = null!;

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime EventDate { get; set; }

        [MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        [Column(TypeName = "decimal(6,2)")]
        public decimal Distance { get; set; }

        [MaxLength(500)]
        public string? BannerImageURL { get; set; }
    }
}