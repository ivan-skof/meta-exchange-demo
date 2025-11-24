using BSD.Core.DTOs;
using BSD.Core.Enums;
using BSD.Core.Models;
using BSD.Services.Interfaces;

namespace BSD.Services.Implementations;

public class MetaExchangeService : IMetaExchangeService
{
    /// <summary>
    /// Finds the best execution plan for a cryptocurrency order across multiple crypto exchanges.
    /// For Buy orders, selects from Asks, lowest prices first, constraint is coin balance.
    /// For Sell orders, selects from Bids, highest prices first, constraint is money balance
    /// </summary>
    /// <param name="orderType">The type of user's order (Buy or Sell)</param>
    /// <param name="amount">The total amount of coin to buy or sell</param>
    /// <param name="orderBooks">List of order books from different crypto exchanges, each containing Asks and Bids and CryptoExchangeId</param>
    /// <param name="balance">List of crypto exchanges, indicating available money and coins for each exchange</param>
    /// <returns>
    /// A list of execution orders representing the optimal trade distribution across exchanges.
    /// Returns partial results if insufficient liquidity is available to fulfill the complete order.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when:
    /// - orderBooks is null or empty
    /// - balance is null or empty
    /// - There are more order books than crypto exchanges entries
    /// </exception>
    /// <remarks>
    /// The algorithm works as follows:
    /// 1. Creates a priority queue with the best order from each exchange's order book
    /// 2. Iteratively selects the best available price from the queue
    /// 3. Executes for available amount
    /// 4. Updates exchange balances after each execution
    /// 5. Continues until the full amount is executed or no more liquidity is available
    /// 
    /// Balance constraints:
    /// - When user buys, the exchange must have sufficient BTC to sell
    /// - When user sells, the exchange must have sufficient EUR to buy
    /// </remarks>    
    public List<ExecutionOrder> GetBestExecution(
    OrderType orderType,
        decimal amount,
        List<OrderBook> orderBooks, 
        List<CryptoExchange> balance)
    {
        Validate(amount, orderBooks, balance);

        //convert to dictionary for easier lookup by cryptoexchange id
        var balanceDict = balance.ToDictionary(x => x.Id, x => x);

        var executionOrders = new List<ExecutionOrder>();

        PriorityQueue<BestOrder, decimal> bestOrders = FillPriorityQueue(orderType, orderBooks);

        while (amount > 0)
        {
            // are there any more orders in order books?
            if (bestOrders.Count == 0)
            {
                // couldn't fulfill order, returning partial order list
                return executionOrders;
            }

            // top element in bestOrders queue is best from the best so use it for execution order
            var bestOrder = bestOrders.Peek();

            // get cryptoexchange balance of corresponding order book
            if (!balanceDict.TryGetValue(bestOrder.CryptoExchangeId, out var bestOrderBalance))
            {
                // order book has no corresponding cryptoexchange
                // so remove it from queue
                bestOrders.Dequeue();
                continue;
            }

            var executionAmount = GetExecutionAmount(amount, bestOrderBalance, orderType, bestOrder);

            // this should not happen but just in case
            if (executionAmount <= 0)
            {
                bestOrders.Dequeue();
                continue;
            }

            executionOrders.Add(new ExecutionOrder
            {
                Amount = executionAmount,
                Price = bestOrder.Order.Price,
                CryptoExchangeId = bestOrder.CryptoExchangeId,
                OrderType = orderType
            });

            // decrease amount of coins we still need to buy or sell
            amount -= executionAmount;
                
            if (amount <= 0)
            {
                return executionOrders;
            }

            // we need to update cryptoexchange balance to determine if it have enough balance to process any more orders
            bestOrderBalance.UpdateBalance(orderType, executionAmount, bestOrder.Order.Price);

            // remove best order from queue. 
            // note: there can be only one order from each order book in a queue. 
            bestOrders.Dequeue();

            // are there any remaining orders in order book
            var orders = orderBooks[bestOrder.OrderBookIndex].GetOrders(orderType);
            var hasNextOrderInBook = orders.Count > bestOrder.OrderIndex + 1;

            // add new best order to queue only if
            // there is enough balance in corresponding cryptoexchange
            // and there is a next order in order book
            if (bestOrderBalance.HasRemainingBalance(orderType)
                && hasNextOrderInBook)
            {
                int nextIndex = bestOrder.OrderIndex + 1;
                var nextBestOrderInOrderBook = orders[nextIndex];

                bestOrders.Enqueue(
                    new BestOrder
                    {
                        Order = nextBestOrderInOrderBook,
                        OrderBookIndex = bestOrder.OrderBookIndex,
                        OrderIndex = nextIndex,
                        CryptoExchangeId = bestOrder.CryptoExchangeId
                    },
                    nextBestOrderInOrderBook.Price);
            }            
        }

        return executionOrders;
    }

    private decimal GetExecutionAmount(decimal amount, CryptoExchange bestOrderBalance, OrderType orderType, BestOrder bestOrder)
    {
        // get available amount from corresponding crypto exchange
        var availableBalanceAmount = bestOrderBalance.GetAvailableAmount(orderType, bestOrder.Order.Price);

        // use smaller between what best order offers and what exchange balance is
        var availableAmount = Math.Min(availableBalanceAmount, bestOrder.Order.Amount);

        var executionAmount = Math.Min(availableAmount, amount);
        
        return executionAmount;
    }

    private PriorityQueue<BestOrder, decimal> FillPriorityQueue(OrderType orderType, List<OrderBook> orderBooks)
    {
        // to remember from which orderbook came best offer
        int currentOrderBookIndex = 0;

        // Using min heap priority queue with Ask orders for "Buy"
        // and max heap with Bid orders for "Sell" order type
        var bestOrders = orderType == OrderType.Buy
            ? new PriorityQueue<BestOrder, decimal>()
            : new PriorityQueue<BestOrder, decimal>(Comparer<decimal>.Create((x, y) => y.CompareTo(x)));

        // fill bestOrders priority queue with one best order from each order book
        foreach (OrderBook orderBook in orderBooks)
        {
            //orders are sorted, so the first one from an order book is the best one
            var orders = orderBook.GetOrders(orderType);
            if (orders == null || orders.Count == 0)
            {
                //skip this order book
                currentOrderBookIndex++;
                continue;
            }

            var bestOrder = orders.First();
            bestOrders.Enqueue(
                new BestOrder
                {
                    Order = bestOrder,
                    OrderBookIndex = currentOrderBookIndex++, //remember from which order book came this order
                    OrderIndex = 0, //remember the position of order in order book
                    CryptoExchangeId = orderBook.CryptoExchangeId
                },
                bestOrder.Price);
        }

        return bestOrders;
    }

    private void Validate(decimal amount, List<OrderBook> orderBooks, List<CryptoExchange> balance)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than zero.");
        }
        if (orderBooks == null || orderBooks.Count == 0)
        {
            throw new ArgumentException("Order books data null or empty.");
        }
        if (balance == null || balance.Count == 0)
        {
            throw new ArgumentException("Crypto exchange data null or empty.");
        }
        // we are getting order books from n cryptoexchanges so make sure list of cryptoexchanges is big enough
        if (orderBooks.Count > balance.Count)
        {
            throw new ArgumentException("Mismatch: more order books than balance entries.");
        }
    }

    private class BestOrder
    {
        public required Order Order { get; set; }
        public int OrderBookIndex { get; set; }
        public int OrderIndex { get; set; }
        public int CryptoExchangeId { get; set; }
    }
}