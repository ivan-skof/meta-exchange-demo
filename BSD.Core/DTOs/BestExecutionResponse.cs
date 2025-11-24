namespace BSD.Core.DTOs;

public class BestExecutionResponse
{
    public List<ExecutionOrder> ExecutionOrders { get; set; } = [];
    public int Total { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal RequestedAmount { get; set; }
    public bool IsComplete { get; set; }
}
