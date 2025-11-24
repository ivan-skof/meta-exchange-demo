using BSD.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace BSD.Core.DTOs;

public class BestExecutionRequest
{
    [Required(ErrorMessage = "OrderType is required")]
    public OrderType OrderType { get; set; }
    
    [Required(ErrorMessage = "Amount is required")]
    [Range(0.00000001, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
    public decimal Amount { get; set; }
}
