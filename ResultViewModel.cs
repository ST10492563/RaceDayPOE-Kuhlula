public class ResultViewModel
{
    public int ResultID { get; set; }

    public int EnrollmentID { get; set; }

    public TimeSpan? FinishTime { get; set; }

    public int? Position { get; set; }

    public string? AveragePace { get; set; }

    public string ResultStatus { get; set; } = "Finished";
}
