
using System.ComponentModel.DataAnnotations;

namespace RaceDayMvc.Models
{
    public class EventViewModel
    {
        public int EventID { get; set; }
        public int OrganiserID { get; set; }

        [Required]
        [StringLength(150)]
        public string EventName { get; set; } = "";

        public string? Description { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        public TimeSpan StartTime { get; set; }

        [Required]
        public string Location { get; set; } = "";

        [Required]
        public string Province { get; set; } = "";

        [Range(0, 999999)]
        public decimal EntryFee { get; set; }

        [Range(1, 100000)]
        public int MaxParticipants { get; set; }

        public string Status { get; set; } = "Upcoming";
    }

    public class CategoryViewModel
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = "";
        public string? Description { get; set; }
    }

    public class EnrollmentViewModel
    {
        public int EnrollmentID { get; set; }
        public int EventID { get; set; }
        public int ParticipantID { get; set; }
        public int CategoryID { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public string? RaceNumber { get; set; }
        public string Status { get; set; } = "Registered";

        public EventViewModel? Event { get; set; }
        public CategoryViewModel? Category { get; set; }
        public ResultViewModel? Result { get; set; }
    }

    public class ResultViewModel
    {
        public int ResultID { get; set; }
        public int EnrollmentID { get; set; }
        public TimeSpan? FinishTime { get; set; }
        public int? Position { get; set; }
        public string? AveragePace { get; set; }
        public string ResultStatus { get; set; } = "Finished";
    }
}


