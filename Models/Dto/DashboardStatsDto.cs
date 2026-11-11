namespace RaceBay.MVC.Models.Dto;

public class DashboardStatsDto
{
    public int TotalEvents { get; set; }
    public int TotalEnrollments { get; set; }
    public List<EventDto> UpcomingEvents { get; set; } = new();
}