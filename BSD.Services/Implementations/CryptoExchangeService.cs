using BSD.Core.Configuration;
using BSD.Core.Models;
using BSD.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace BSD.Services.Implementations;

public class CryptoExchangeService : ICryptoExchangeService
{
    private readonly CryptoExchangeSettings _settings;

    public CryptoExchangeService(IOptions<CryptoExchangeSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task<List<OrderBook>> GetOrderBooksAsync()
    {
        return await ReadOrderBooksFromFile(_settings.OrderBooksPath, _settings.MaxNumberOfRecords);
    }

    public async Task<List<CryptoExchange>> GetCryptoExchangesAsync()
    {
        return await ReadCryptoExchangesFromFile(_settings.CryptoExchangesPath, _settings.MaxNumberOfRecords);
    }


    private async Task<List<OrderBook>> ReadOrderBooksFromFile(string filePath, int maxLines)
    {
        var orderBooks = new List<OrderBook>();
        int count = 0;

        await foreach (var line in File.ReadLinesAsync(filePath))
        {
            if (count >= maxLines)
                break;

            if (string.IsNullOrWhiteSpace(line))
                continue;

            // Find where JSON starts
            int jsonStart = line.IndexOf('{');
            if (jsonStart == -1)
                continue;

            var json = line.Substring(jsonStart);
            var data = JsonSerializer.Deserialize<OrderBookJson>(json);

            if (data == null)
            {
                continue; // Skip bad line
            }

            var asks = data.Asks.Select(a => a.Order).ToList();
            var bids = data.Bids.Select(b => b.Order).ToList();

            orderBooks.Add(new OrderBook { Asks = asks, Bids = bids, CryptoExchangeId = ++count });
        }
        return orderBooks;
    }

    private async Task<List<CryptoExchange>> ReadCryptoExchangesFromFile(string filePath, int maxLines)
    {
        var exchanges = new List<CryptoExchange>();
        int count = 0;

        await foreach (var line in File.ReadLinesAsync(filePath))
        {
            if (count >= maxLines) break;
            if (string.IsNullOrWhiteSpace(line)) continue;

            var data = JsonSerializer.Deserialize<CryptoExchange>(line);

            if (data == null)
            {
                continue; // Skip bad line
            }
            //TODO check that there is id set
            exchanges.Add(data);

            count++;
        }

        return exchanges;
    }
    private class OrderBookJson
    {
        public List<OrderWrapper> Bids { get; set; } = [];
        public List<OrderWrapper> Asks { get; set; } = [];
    }
    private class OrderWrapper
    {
        public required Order Order { get; set; }
    }
}

