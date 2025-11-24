using BSD.Services.Implementations;
using BSD.Core.Configuration;
using System.Text.Json;
using Microsoft.Extensions.Options;
using BSD.Core.Enums;

namespace BSD.ConsoleApp;

class Program
{
    private const string DEFAULT_ORDER_BOOKS_DATA_PATH = ".\\data\\order_books_data";
    private const string DEFAULT_CRYPTO_EXCHANGES_PATH = ".\\data\\crypto_exchanges";
    private const int DEFAULT_MAX_ORDER_BOOKS = 10;

    static async Task Main(string[] args)
    {
        try
        {
            var parsedArgs = ParseArguments(args);

            var settings = new CryptoExchangeSettings
            {
                OrderBooksPath = parsedArgs.OrderBooksPath,
                CryptoExchangesPath = parsedArgs.ExchangesPath,
                MaxNumberOfRecords = parsedArgs.MaxOrderBooks
            };

            var cryptoExchangeService = new CryptoExchangeService(Options.Create(settings));

            var orderBooks = await cryptoExchangeService.GetOrderBooksAsync();
            Console.WriteLine($"Successfully loaded {orderBooks.Count} order books.");

            var exchanges = await cryptoExchangeService.GetCryptoExchangesAsync();
            Console.WriteLine($"Successfully loaded {exchanges.Count} crypto exchanges.");

            var service = new MetaExchangeService();
            var result = service.GetBestExecution(parsedArgs.OrderType, parsedArgs.Amount, orderBooks, exchanges);
            Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            PrintUsage();
        }
    }

    static ProgramArguments ParseArguments(string[] args)
    {
        if (args.Length < 2)
        {
            throw new ArgumentException("Missing required arguments order type ('Buy' or 'Sell') and amount");
        }

        var orderTypeStr = args[0];
        var amountStr = args[1];

        if (!Enum.TryParse<OrderType>(orderTypeStr, ignoreCase: true, out var orderType))
        {
            throw new ArgumentException($"Invalid order type: {orderTypeStr}. Must be 'Buy' or 'Sell'");
        }

        if (!decimal.TryParse(amountStr, out var amount) || amount <= 0)
        {
            throw new ArgumentException($"Invalid amount: {amountStr}. Must be a positive number");
        }

        // default values for optional arguments
        int maxOrderBooks = DEFAULT_MAX_ORDER_BOOKS;
        string orderBooksPath = DEFAULT_ORDER_BOOKS_DATA_PATH;
        string exchangesPath = DEFAULT_CRYPTO_EXCHANGES_PATH;

        for (int i = 2; i < args.Length; i++)
        {
            if (args[i] == "-o" && i + 1 < args.Length)
            {
                orderBooksPath = args[i + 1];
                i++;
            }
            else if (args[i] == "-c" && i + 1 < args.Length)
            {
                exchangesPath = args[i + 1];
                i++;
            }
            else if (args[i] == "-n" && i + 1 < args.Length)
            {
                if (!int.TryParse(args[i + 1], out maxOrderBooks) || maxOrderBooks <= 0)
                {
                    throw new ArgumentException($"Invalid number of order books: {args[i + 1]}. Must be a positive integer");
                }
                i++;
            }
        }

        return new ProgramArguments
        {
            OrderType = orderType,
            Amount = amount,
            OrderBooksPath = orderBooksPath,
            ExchangesPath = exchangesPath,
            MaxOrderBooks = maxOrderBooks
        };
    }

    static void PrintUsage()
    {
        Console.WriteLine("\nUsage:");
        Console.WriteLine("  dotnet run <OrderType> <Amount> [-o OrderBooksPath] [-c ExchangesPath] [-n MaxOrderBooks]");
        Console.WriteLine("\nRequired Arguments:");
        Console.WriteLine("  OrderType    : 'buy' or 'sell' (case insensitive)");
        Console.WriteLine("  Amount       : Positive decimal number (coin amount)");
        Console.WriteLine("\nOptional Arguments:");
        Console.WriteLine("  -o <path>    : Path to order books file (default: ./data/order_books_data)");
        Console.WriteLine("  -c <path>    : Path to exchanges balance file (default: ./data/crypto_exchanges)");
        Console.WriteLine("  -n <number>  : Maximum number of order books / crypto exchanges to load (default: 10)");
        Console.WriteLine("\nExamples:");
        Console.WriteLine("  dotnet run buy 10.5");
        Console.WriteLine("  dotnet run sell 5 -n 10");
        Console.WriteLine("  dotnet run buy 3.2 -o books.txt -n 100");
        Console.WriteLine("  dotnet run sell 7.8 -o books.txt -c exchanges.txt -n 50");
    }
    private class ProgramArguments
    {
        public OrderType OrderType { get; set; }
        public decimal Amount { get; set; }
        public required string OrderBooksPath { get; set; }
        public required string ExchangesPath { get; set; }
        public int MaxOrderBooks { get; set; }
    }
}