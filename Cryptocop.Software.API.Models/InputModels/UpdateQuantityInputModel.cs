using System.ComponentModel.DataAnnotations;

namespace Cryptocop.Software.API.Models.InputModels;

public class UpdateQuantityInputModel
{
    [Required]
    [Range(0.0, float.MaxValue)]
    public float? Quantity { get; set; }
}
