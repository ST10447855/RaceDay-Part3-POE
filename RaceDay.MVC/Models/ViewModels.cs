using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace RaceDay.MVC.Models
{
    public class EventTypeVm
    {
        public int EventTypeID { get; set; }
        public string TypeName { get; set; } = string.Empty;
    }

    public class EventVm
    {
        public int EventID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public decimal Distance { get; set; }
        public int EventTypeID { get; set; }
        public string EventTypeName { get; set; } = string.Empty;
        public string? BannerImageURL { get; set; }
        public int CategoryCount { get; set; }
        public int EnrolmentCount { get; set; }
    }

    public class CategoryVm
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int MinimumAge { get; set; }
        public int MaximumAge { get; set; }
        public decimal Distance { get; set; }
    }

    public class EventDetailVm : EventVm
    {
        public List<CategoryVm> Categories { get; set; } = new();
    }

    public class EnrolmentVm
    {
        public int EnrolmentID { get; set; }
        public int EventID { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime EnrolmentDate { get; set; }
    }

    public class ResultVm
    {
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string FinishingTime { get; set; } = string.Empty;
        public int Position { get; set; }
        public int TotalFinishers { get; set; }
    }

    public class ProfileVm
    {
        [Required(ErrorMessage = "First name is required"), StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required"), StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        public string? Email { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Date of birth is required")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? DateOfBirth { get; set; }

        public string? ProfilePictureURL { get; set; }
    }

    public class HomeIndexVm
    {
        public List<EventVm> Events { get; set; } = new();
        public List<EventTypeVm> Types { get; set; } = new();
        public string? Search { get; set; }
        public int? TypeId { get; set; }
    }

    public class EventFormVm
    {
        public int EventID { get; set; }

        [Required(ErrorMessage = "Event name is required"), StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Event date is required")]
        public DateTime EventDate { get; set; }

        [Required(ErrorMessage = "Location is required"), StringLength(200)]
        public string Location { get; set; } = string.Empty;

        [Range(0.1, 1000, ErrorMessage = "Distance must be between 0.1 and 1000 km")]
        public decimal Distance { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please choose an event type")]
        public int EventTypeID { get; set; }

        public string? BannerImageURL { get; set; }

        public IFormFile? Banner { get; set; }

        [ValidateNever]
        public List<EventTypeVm> EventTypes { get; set; } = new();
    }

    public class CategoryFormVm
    {
        public int EventID { get; set; }

        [Required(ErrorMessage = "Category name is required"), StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [Range(0, 120)]
        public int MinimumAge { get; set; }

        [Range(0, 120)]
        public int MaximumAge { get; set; }

        [Range(0.1, 1000, ErrorMessage = "Distance must be between 0.1 and 1000 km")]
        public decimal Distance { get; set; }
    }

    public class OrganiserEnrolmentVm
    {
        public int EnrolmentID { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public string ParticipantEmail { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime EnrolmentDate { get; set; }
        public bool HasResult { get; set; }
        public string? FinishingTime { get; set; }
        public int? Position { get; set; }
    }

    /// <summary>Event picker + the rows for the selected event (categories, enrolments or results pages).</summary>
    public class OrganiserEventPickerVm
    {
        public List<EventVm> Events { get; set; } = new();
        public int? SelectedEventId { get; set; }
    }

    public class OrganiserEventPageVm<T> : OrganiserEventPickerVm
    {
        public List<T> Items { get; set; } = new();
    }
}
