using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class Category
    {
        public int CategoryID { get; set; }

        //This is the Foreign key
        public int EventID { get; set; }
        [ForeignKey("EventID")]
        public Event Event {  get; set; }

        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; }

        
        public int MinimumAge { get; set; }
        public int MaximumAge { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        public decimal Distance { get; set; }
        

    }
}
