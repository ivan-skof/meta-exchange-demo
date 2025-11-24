using BSD.Core.Enums;

namespace BSD.Core.DTOs;

public class ExecutionOrder
{
    public OrderType OrderType { get; set; }
    public decimal Amount { get; set; }
    public decimal Price { get; set; }

    public int CryptoExchangeId { get; set; }
}
