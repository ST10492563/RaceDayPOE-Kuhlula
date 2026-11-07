
using System.ComponentModel.DataAnnotations;

public class ProfileForm
{
    [Required, MaxLength(50)]
    public string FirstName { get; set; } = "";

    [Required, MaxLength(50)]
    public string LastName { get; set; } = "";

    [Required, EmailAddress, MaxLength(120)]
    public string Email { get; set; } = "";

    [MaxLength(25)]
    public string? Phone { get; set; }

    public IFormFile? ProfilePicture { get; set; }
}

public class ParticipantViewModel
{
    public int UserID { get; set; }

    public string FirstName { get; set; } = "";

    public string LastName { get; set; } = "";

    public string Email { get; set; } = "";
}
