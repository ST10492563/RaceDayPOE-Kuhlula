public class EnrollmentViewModel
{
    public int EnrollmentID { get; set; }

    public int EventID { get; set; }

    public int ParticipantID { get; set; }

    public int CategoryID { get; set; }

    public DateTime EnrollmentDate { get; set; }

    public string? RaceNumber { get; set; }

    public string Status { get; set; } = "Registered";

    public ResultViewModel? Result { get; set; }

    public ParticipantViewModel? Participant { get; set; }
}