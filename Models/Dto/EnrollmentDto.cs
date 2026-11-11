namespace RaceBay.MVC.Models.Dto;

public class EnrollmentDto
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public string ParticipantEmail { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // "Pending", "Confirmed"
    public DateTime EnrolledAt { get; set; }
}