using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models
{
    public class Organiser
    {
        [Key]
        public int OrganiserID { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string ContactEmail { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }
}