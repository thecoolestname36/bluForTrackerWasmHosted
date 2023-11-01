using BluForTracker.Shared;
using System.ComponentModel.DataAnnotations;

namespace BluForTracker.Client.Shared.DataModels;
public class MarkerFormModel
{
    public const int LabelMaxChars = 28;

    [Required]
    public Team? Team { get; set; }

    [StringLength(LabelMaxChars, ErrorMessage = "Label must be 28 characters or less.")]
    public string Label { get; set; } = "";

    [RegularExpression("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Invalid Format")]
    public string Color { get; set; } = "#000000";
}
