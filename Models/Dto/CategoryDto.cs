namespace RaceBay.MVC.Models.Dto;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Distance { get; set; } // e.g., "10km", "42km"
}