using BSD.Api.ExceptionHandlers;
using BSD.Core.Configuration;
using BSD.Services.Implementations;
using BSD.Services.Interfaces;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Bind CryptoExchangeSettings from appsettings.json
builder.Services.Configure<CryptoExchangeSettings>(
    builder.Configuration.GetSection("CryptoExchangeSettings"));

builder.Services.AddSingleton<ICryptoExchangeService, CryptoExchangeService>();
builder.Services.AddSingleton<IMetaExchangeService, MetaExchangeService>();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();



builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);

    options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();
app.Run();
