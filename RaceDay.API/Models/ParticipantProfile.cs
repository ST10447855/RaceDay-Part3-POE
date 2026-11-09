using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class ParticipantProfile
    {
        [Key]
        public int ProfileID { get; set; }

        // This is the Foreign Key property
        public int UserID { get; set; }

        // This is the Navigation Property that connects to the User model
        [ForeignKey("UserID")]
        public User User { get; set; } = null!;

        public DateTime DateOfBirth { get; set; }

        [MaxLength(500)]
        public string? ProfilePictureURL { get; set; }
    }
}