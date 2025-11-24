# MetaExchange demo

Web service and console app for computing the best execution plan across multiple crypto exchanges

## Prerequisites

- .NET 9.0 SDK or later

## Setup Instructions

### 1. Clone/Extract the Project

Extract or clone the project to your local machine.

### 2. Configure Database Connection

Edit `BSD.Api/appsettings.json` and update CryptoExchangeSettings if needed

```json
  "CryptoExchangeSettings": {
    "OrderBooksPath": "Data/order_books_data",
    "CryptoExchangesPath": "Data/crypto_exchanges",
    "MaxNumberOfRecords":  10
  },
```

### 3. Install Dependencies

```bash
dotnet restore
```

## Running the Service

```bash
# From the solution root
dotnet run --project BSD.Api
```

The service will start at `https://localhost:7267` (or `http://localhost:5261`)

## Testing the Service

### Run Unit Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity normal
```

### Test with Swagger UI

Once the service is running, open your browser and navigate to:
```
https://localhost:7267/swagger/index.html
```

## Running the Console App

```bash
Basic usage:
# From the solution root
dotnet run --project BSD.ConsoleApp buy 2 -n 2
```

```bash
Usage:
  dotnet run <OrderType> <Amount> [-o OrderBooksPath] [-c ExchangesPath] [-n MaxOrderBooks]

Required Arguments:
  OrderType    : 'buy' or 'sell' (case insensitive)
  Amount       : Positive decimal number (coin amount)

Optional Arguments:
  -o <path>    : Path to order books file (default: ./data/order_books_data)
  -c <path>    : Path to exchanges balance file (default: ./data/crypto_exchanges)
  -n <number>  : Maximum number of order books / crypto exchanges to load (default: 10)

Examples:
  dotnet run buy 10.5
  dotnet run sell 5 -n 10
  dotnet run buy 3.2 -o books.txt -n 100
  dotnet run sell 7.8 -o books.txt -c exchanges.txt -n 50
```

## Running with Docker

### Build and Run
```bash
# Build the image
docker build -t bsd-metaexchange:latest -f Dockerfile .

# Run the container
docker run -d -p 8080:8080 --name bsd-metaexchange bsd-metaexchange:latest

# View logs
docker logs -f bsd-metaexchange
```

### Access
- Swagger UI: http://localhost:8080/swagger
- API: http://localhost:8080/api/MetaExchange/best-execution

### Management
```bash
docker stop bsd-metaexchange     # Stop
docker start bsd-metaexchange    # Start
docker restart bsd-metaexchange  # Restart
docker rm bsd-metaexchange       # Remove
```