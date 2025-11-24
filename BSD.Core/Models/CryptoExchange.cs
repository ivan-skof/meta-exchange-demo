using BSD.Core.Enums;

namespace BSD.Core.Models;

public class CryptoExchange
{
    public int Id { get; set; }
    public decimal MoneyBalance { get; set; } 
    public decimal CoinBalance { get; set; } 

    /// <summary>
    /// Calculates the maximum amount of coins the exchange can trade based on available balance.
    /// When the user is selling, calculates how much coins the exchange can buy with available money.
    /// When the user is buying, returns the available amount of coins.
    /// </summary>
    /// <param name="orderType">The type of user's order (Buy or Sell)</param>
    /// <param name="pricePerBtc">The price per coin</param>
    /// <returns>The maximum amount of available coins</returns>
    public decimal GetAvailableAmount(OrderType orderType, decimal pricePerBtc)
    {
        if (orderType == OrderType.Sell)
        {
            // How much coins can cryptoexchange buy with available EUR?
            return MoneyBalance / pricePerBtc;
        }
        else // Buy
        {
            // How much coins can crypto exchange sell?
            return CoinBalance;
        }
    }
    
    /// <summary>
    /// Updates the exchange's balance.
    /// When the user sells, the exchange spends money and receives coins.
    /// When the user buys, the exchange receives money and spends coins.
    /// </summary>
    /// <param name="orderType">The type of user's order (Buy or Sell)</param>
    /// <param name="coinAmount">The amount of coins</param>
    /// <param name="pricePerCoin">The price per coin</param>
    public void UpdateBalance(OrderType orderType, decimal coinAmount, decimal pricePerCoin)
    {
        if (orderType == OrderType.Sell)
        {
            // user is selling so crypthoexchange is buying
            // decrease money, increase coin
            decimal cost = coinAmount * pricePerCoin;
            MoneyBalance -= cost;
            CoinBalance += coinAmount;
        }
        else
        {   
            // user is buying so crypthoexchange is selling
            // increase money, decrease coin
            decimal moneyReceived = coinAmount * pricePerCoin;
            CoinBalance -= coinAmount;
            MoneyBalance += moneyReceived;
        }
    }

    /// <summary>
    /// Checks if the exchange has any remaining balance to continue processing orders.
    /// When the user is selling, checks if the exchange has any money.
    /// When the user is buying, checks if the exchange has any coins.
    /// </summary>
    /// <param name="orderType">The type of user's order (Buy or Sell)</param>
    /// <returns>True if the exchange has remaining balance for the order type, otherwise false</returns>
    public bool HasRemainingBalance(OrderType orderType)
    {
        if (orderType == OrderType.Sell)
        {
            // User is selling so cryptoexchange needs to have money
            return MoneyBalance > 0;
        }
        else // Sell
        {
            // User is buying so cryptoexchange needs to have coins
            return CoinBalance > 0;
        }
    }
}
