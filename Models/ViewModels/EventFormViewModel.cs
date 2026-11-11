using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RaceBay.MVC.Models.ViewModels;

public class EventFormViewModel
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; } = DateTime.Today.AddDays(7);

    [Required]
    public string Location { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a category.")]
    public int CategoryId { get; set; }

    // File upload for Blob Storage
    public IFormFile? BannerImage { get; set; }

    // To display existing image if editing
    public string? ExistingBannerUrl { get; set; }
}