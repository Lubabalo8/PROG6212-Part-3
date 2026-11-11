namespace RaceBay.MVC.Models.Dto;

public class EventDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? BannerImageUrl { get; set; }

    // Assuming your API returns nested categories or IDs
    public int CategoryId { get; set; }
    public CategoryDto? Category { get; set; }
}