using System.ComponentModel.DataAnnotations;

namespace RaceBay.MVC.Models.ViewModels;

public class ResultFormViewModel
{
    public int EventId { get; set; }

    [Required]
    public int ParticipantId { get; set; }

    [Required]
    [Display(Name = "Finish Time (HH:mm:ss)")]
    public TimeSpan FinishTime { get; set; }

    [Required]
    public int Position { get; set; }
}