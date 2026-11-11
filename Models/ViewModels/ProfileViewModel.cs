using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RaceBay.MVC.Models.ViewModels;

public class ProfileViewModel
{
    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    // For displaying the current image
    public string? ExistingProfilePictureUrl { get; set; }

    // For uploading a new image
    [Display(Name = "Profile Picture")]
    public IFormFile? ProfilePicture { get; set; }
}