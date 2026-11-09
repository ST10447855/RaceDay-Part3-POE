using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class Result
    {
        [Key]
        public int ResultID { get; set; }

        [Required]
        public int EnrolmentID { get; set; }

        [ForeignKey("EnrolmentID")]
        public Enrolment Enrolment { get; set; } = null!;

        public string FinishingTime { get; set; } = string.Empty;
        public int Position { get; set; }
    }
}