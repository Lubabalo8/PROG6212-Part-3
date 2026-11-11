using System.ComponentModel.DataAnnotations;

namespace RaceBay.MVC.Models.ViewModels;

public class CategoryFormViewModel
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Distance { get; set; } = string.Empty;
}