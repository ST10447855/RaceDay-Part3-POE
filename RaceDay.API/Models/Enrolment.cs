using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace RaceDay.API.Models
{
    public class Enrolment
    {
        [Key]
        public int EnrolmentID { get; set; }

        public int ParticipantID { get; set; }
        [ForeignKey("ParticipantID")]
        public User Participant { get; set; } = null!;

        public int EventID { get; set; }
        [ForeignKey("EventID")]
        public Event Event { get; set; } = null!;

        public int CategoryID { get; set; }
        [ForeignKey("CategoryID")]
        public Category Category { get; set; } = null!;

        public DateTime EnrolmentDate { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Confirmed";

        // Navigation property for Result (1 to 0..1)
        public Result? Result { get; set; }
    }
}
