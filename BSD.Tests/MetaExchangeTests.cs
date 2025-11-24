using BSD.Services.Implementations;
using System.Text.Json;
using BSD.Core.Models;
using BSD.Core.Enums;

namespace BSD.Tests;

public class MetaExchangeTests
{
    private List<OrderBook> orderBooks;
    private List<CryptoExchange> exchangesBalance;

    [SetUp]
    public void Setup()
    {
        exchangesBalance = new List<CryptoExchange>
        {
            new CryptoExchange { Id = 1, MoneyBalance = 8750, CoinBalance = 10m},
            new CryptoExchange { Id = 2, MoneyBalance = 15200m, CoinBalance = 6m},
            new CryptoExchange { Id = 3, MoneyBalance = 16450m, CoinBalance = 6m},
        };

        orderBooks = new List<OrderBook>
        {
            new OrderBook
            {
                Asks = new List<Order>
                {
                    new Order { Price = 3000m, Amount = 7m },
                    new Order { Price = 3300m, Amount = 4m },
                    new Order { Price = 3500m, Amount = 9m }
                },
                Bids = new List<Order>
                {
                    new Order { Price = 2950m, Amount = 1m },
                    new Order { Price = 2900m, Amount = 2m },
                    new Order { Price = 2850m, Amount = 10m }
                },
                CryptoExchangeId = 1
            },
            new OrderBook
            {
                Asks = new List<Order>
                {
                    new Order { Price = 3100m, Amount = 3m },
                    new Order { Price = 3200m, Amount = 5m },
                    new Order { Price = 3400m, Amount = 6m }
                },

                Bids = new List<Order>
                {
                    new Order { Price = 3050m, Amount = 4m },
                    new Order { Price = 3000m, Amount = 7m },
                    new Order { Price = 2910m, Amount = 6m }
                },
                CryptoExchangeId = 2
            },
            new OrderBook
            {
                Asks = new List<Order>
                {
                    new Order { Price = 2950m, Amount = 2m },
                    new Order { Price = 3050m, Amount = 3m },
                    new Order { Price = 3150m, Amount = 4m }
                },
                Bids = new List<Order>
                {
                    new Order { Price = 2750m, Amount = 5m },
                    new Order { Price = 2700m, Amount = 6m },
                    new Order { Price = 2650m, Amount = 3m }
                },
                CryptoExchangeId = 3
            }
        };
    }

    [Test]
    public void TestGetExecutionPlan_SingleOrderBook_Buy()
    {
        var service = new MetaExchangeService();
        var result = service.GetBestExecution(OrderType.Buy, 9, orderBooks.Slice(0,1), exchangesBalance.Slice(0,1));
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result[0].Amount, Is.EqualTo(7));
        Assert.That(result[0].Price, Is.EqualTo(3000));
        Assert.That(result[1].Amount, Is.EqualTo(2));
        Assert.That(result[1].Price, Is.EqualTo(3300));
        Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
    }

    [Test]
    public void TestGetExecutionPlan_SingleOrderBook_Sell()
    {
        var service = new MetaExchangeService();
        var result = service.GetBestExecution(OrderType.Sell, 2, orderBooks.Slice(0, 1), exchangesBalance.Slice(0, 1));
        Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result[0].Amount, Is.EqualTo(1));
        Assert.That(result[0].Price, Is.EqualTo(2950));
        Assert.That(result[1].Amount, Is.EqualTo(1));
        Assert.That(result[1].Price, Is.EqualTo(2900));
    }

    [Test]
    public void TestGetExecutionPlan_MultiOrderBooks_Buy()
    {
        var service = new MetaExchangeService();
        var result = service.GetBestExecution(OrderType.Buy, 22, orderBooks, exchangesBalance);
        Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(7));
        Assert.That(result.Sum(x => x.Amount), Is.EqualTo(22));


        Assert.That(result[0].CryptoExchangeId, Is.EqualTo(3));  
        Assert.That(result[0].Amount, Is.EqualTo(2));
        Assert.That(result[0].Price, Is.EqualTo(2950));

        Assert.That(result[1].CryptoExchangeId, Is.EqualTo(1));
        Assert.That(result[1].Amount, Is.EqualTo(7));
        Assert.That(result[1].Price, Is.EqualTo(3000));

        Assert.That(result[2].CryptoExchangeId, Is.EqualTo(3));
        Assert.That(result[2].Amount, Is.EqualTo(3));
        Assert.That(result[2].Price, Is.EqualTo(3050));

        Assert.That(result[3].CryptoExchangeId, Is.EqualTo(2));
        Assert.That(result[3].Amount, Is.EqualTo(3));
        Assert.That(result[3].Price, Is.EqualTo(3100));

        Assert.That(result[4].CryptoExchangeId, Is.EqualTo(3));
        Assert.That(result[4].Amount, Is.EqualTo(1));
        Assert.That(result[4].Price, Is.EqualTo(3150));

        Assert.That(result[5].CryptoExchangeId, Is.EqualTo(2));
        Assert.That(result[5].Amount, Is.EqualTo(3));
        Assert.That(result[5].Price, Is.EqualTo(3200));

        Assert.That(result[6].CryptoExchangeId, Is.EqualTo(1));
        Assert.That(result[6].Amount, Is.EqualTo(3));
        Assert.That(result[6].Price, Is.EqualTo(3300));

    }

    [Test]
    public void TestGetExecutionPlan_MultiOrderBooks_BuyTooMuch()
    {
        var service = new MetaExchangeService();
        var result = service.GetBestExecution(OrderType.Buy, 23, orderBooks, exchangesBalance);
        Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(7));
        Assert.That(result.Sum(x => x.Amount), Is.EqualTo(22));


        Assert.That(result[0].CryptoExchangeId, Is.EqualTo(3));
        Assert.That(result[0].Amount, Is.EqualTo(2));
        Assert.That(result[0].Price, Is.EqualTo(2950));

        Assert.That(result[1].CryptoExchangeId, Is.EqualTo(1));
        Assert.That(result[1].Amount, Is.EqualTo(7));
        Assert.That(result[1].Price, Is.EqualTo(3000));

        Assert.That(result[2].CryptoExchangeId, Is.EqualTo(3));
        Assert.That(result[2].Amount, Is.EqualTo(3));
        Assert.That(result[2].Price, Is.EqualTo(3050));

        Assert.That(result[3].CryptoExchangeId, Is.EqualTo(2));
        Assert.That(result[3].Amount, Is.EqualTo(3));
        Assert.That(result[3].Price, Is.EqualTo(3100));

        Assert.That(result[4].CryptoExchangeId, Is.EqualTo(3));
        Assert.That(result[4].Amount, Is.EqualTo(1));
        Assert.That(result[4].Price, Is.EqualTo(3150));

        Assert.That(result[5].CryptoExchangeId, Is.EqualTo(2));
        Assert.That(result[5].Amount, Is.EqualTo(3));
        Assert.That(result[5].Price, Is.EqualTo(3200));

        Assert.That(result[6].CryptoExchangeId, Is.EqualTo(1));
        Assert.That(result[6].Amount, Is.EqualTo(3));
        Assert.That(result[6].Price, Is.EqualTo(3300));

    }

    [Test]
    public void TestGetExecutionPlan_MultiOrderBooks_Sell()
    {
        var service = new MetaExchangeService();
        var result = service.GetBestExecution(OrderType.Sell, 10, orderBooks, exchangesBalance);
        Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(5));
        Assert.That(result.Sum(x => x.Amount), Is.EqualTo(10));


        Assert.That(result[0].CryptoExchangeId, Is.EqualTo(2));
        Assert.That(result[0].Amount, Is.EqualTo(4));
        Assert.That(result[0].Price, Is.EqualTo(3050));

        Assert.That(result[1].CryptoExchangeId, Is.EqualTo(2));
        Assert.That(result[1].Amount, Is.EqualTo(1));
        Assert.That(result[1].Price, Is.EqualTo(3000));

        Assert.That(result[2].CryptoExchangeId, Is.EqualTo(1));
        Assert.That(result[2].Amount, Is.EqualTo(1));
        Assert.That(result[2].Price, Is.EqualTo(2950));

        Assert.That(result[3].CryptoExchangeId, Is.EqualTo(1));
        Assert.That(result[3].Amount, Is.EqualTo(2));
        Assert.That(result[3].Price, Is.EqualTo(2900));

        Assert.That(result[4].CryptoExchangeId, Is.EqualTo(3));
        Assert.That(result[4].Amount, Is.EqualTo(2));
        Assert.That(result[4].Price, Is.EqualTo(2750));
    }

    [Test]
    public void TestGetExecutionPlan_MultiOrderBooks_Sell_TooMuch()
    {
        var service = new MetaExchangeService();
        var result = service.GetBestExecution(OrderType.Sell, 15, orderBooks, exchangesBalance);
        Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(6));
        Assert.That(result.Sum(x => x.Amount), Is.EqualTo(14));


        Assert.That(result[0].CryptoExchangeId, Is.EqualTo(2));
        Assert.That(result[0].Amount, Is.EqualTo(4));
        Assert.That(result[0].Price, Is.EqualTo(3050));

        Assert.That(result[1].CryptoExchangeId, Is.EqualTo(2));
        Assert.That(result[1].Amount, Is.EqualTo(1));
        Assert.That(result[1].Price, Is.EqualTo(3000));

        Assert.That(result[2].CryptoExchangeId, Is.EqualTo(1));
        Assert.That(result[2].Amount, Is.EqualTo(1));
        Assert.That(result[2].Price, Is.EqualTo(2950));

        Assert.That(result[3].CryptoExchangeId, Is.EqualTo(1));
        Assert.That(result[3].Amount, Is.EqualTo(2));
        Assert.That(result[3].Price, Is.EqualTo(2900));

        Assert.That(result[4].CryptoExchangeId, Is.EqualTo(3));
        Assert.That(result[4].Amount, Is.EqualTo(5));
        Assert.That(result[4].Price, Is.EqualTo(2750));

        Assert.That(result[5].CryptoExchangeId, Is.EqualTo(3));
        Assert.That(result[5].Amount, Is.EqualTo(1));
        Assert.That(result[5].Price, Is.EqualTo(2700));
    }

    [Test]
    public void TestGetExecutionPlan_EmptyOrderBook()
    {
        var orderBooks = new List<OrderBook>() { new OrderBook { Asks = null, Bids = [], CryptoExchangeId = 1 } };
        
        var service = new MetaExchangeService();
        var result = service.GetBestExecution(OrderType.Buy, 9, orderBooks, exchangesBalance.Slice(0, 1));
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(0));

        result = service.GetBestExecution(OrderType.Sell, 9, orderBooks, exchangesBalance.Slice(0, 1));
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(0));

    }

    [Test]
    public void TestGetExecutionPlan_MissingCryptoExchange()
    {
        var service = new MetaExchangeService();
        var result = service.GetBestExecution(OrderType.Buy, 4, orderBooks.Slice(0,2), exchangesBalance.Slice(1, 2));

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));

        Assert.That(result[0].CryptoExchangeId, Is.EqualTo(2));
        Assert.That(result[0].Amount, Is.EqualTo(3));
        Assert.That(result[0].Price, Is.EqualTo(3100));

        Assert.That(result[1].CryptoExchangeId, Is.EqualTo(2));
        Assert.That(result[1].Amount, Is.EqualTo(1));
        Assert.That(result[1].Price, Is.EqualTo(3200));

    }

}
