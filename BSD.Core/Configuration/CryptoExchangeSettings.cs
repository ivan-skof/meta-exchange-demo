namespace BSD.Core.Configuration;

public class CryptoExchangeSettings
{
    public required string OrderBooksPath { get; set; }
    public required string CryptoExchangesPath { get; set; }
    public int MaxNumberOfRecords { get; set; }
}