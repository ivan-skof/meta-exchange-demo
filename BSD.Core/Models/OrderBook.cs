using BSD.Core.Enums;

namespace BSD.Core.Models;

public class OrderBook
{
    public List<Order> Asks { get; set; } = [];
    public List<Order> Bids { get; set; } = [];
    public int CryptoExchangeId { get; set; }

    public List<Order> GetOrders(OrderType orderType)
    {
        return orderType == OrderType.Buy ? Asks : Bids;
    }
}
