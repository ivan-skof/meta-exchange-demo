using BSD.Core.DTOs;
using BSD.Core.Enums;
using BSD.Core.Models;

namespace BSD.Services.Interfaces;

public interface IMetaExchangeService
{
    List<ExecutionOrder> GetBestExecution(
        OrderType orderType,
        decimal amount,
        List<OrderBook> orderBooks,
        List<CryptoExchange> balance);
}
