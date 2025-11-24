using BSD.Core.Models;

namespace BSD.Services.Interfaces;

public interface ICryptoExchangeService
{
    Task<List<OrderBook>> GetOrderBooksAsync();
    Task<List<CryptoExchange>> GetCryptoExchangesAsync();
}
