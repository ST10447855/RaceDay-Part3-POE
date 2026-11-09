using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace RaceDay.API.Models
{
    public class EventType
    {
        public int EventTypeID { get; set; }

        public string TypeName { get; set; }
    }
}
