using BSD.Core.DTOs;
using BSD.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BSD.Api.Controllers
{
    /// <summary>
    /// Controller for executing cryptocurrency orders across multiple exchanges
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MetaExchangeController : ControllerBase
    {
        private readonly IMetaExchangeService _metaExchangeService;
        private readonly ICryptoExchangeService _cryptoExchangeService;
        private readonly ILogger<MetaExchangeController> _logger;

        /// <summary></summary>
        public MetaExchangeController(
            IMetaExchangeService metaExchangeService,
            ICryptoExchangeService cryptoExchangeService,
            ILogger<MetaExchangeController> logger)
        {
            _metaExchangeService = metaExchangeService;
            _cryptoExchangeService = cryptoExchangeService;
            _logger = logger;
        }

        /// <summary>
        /// Finds the best execution plan for a cryptocurrency order
        /// </summary>
        /// <param name="request">The order request containing order type and amount</param>
        /// <returns>Execution orders distributed across exchanges for optimal pricing</returns>
        /// <response code="200">Returns the execution plan</response>
        /// <response code="400">If the request is invalid</response>
        /// <response code="500">If a server error occurs</response>
        [HttpPost("best-execution")]
        [ProducesResponseType(typeof(BestExecutionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BestExecutionResponse>> GetBestExecution([FromBody] BestExecutionRequest request)
        {
            _logger.LogInformation("Received best execution request: {OrderType} {Amount}", request.OrderType, request.Amount);

            var orderBooks = await _cryptoExchangeService.GetOrderBooksAsync();
            var balances = await _cryptoExchangeService.GetCryptoExchangesAsync();

            var result = _metaExchangeService.GetBestExecution(
                request.OrderType,
                request.Amount,
                orderBooks,
                balances);

            var totalExecuted = result.Sum(o => o.Amount);
            var isComplete = totalExecuted >= request.Amount;

            _logger.LogInformation(
                "Best execution completed: {OrderType} {RequestedAmount} - Executed {ExecutedAmount} across {OrderCount} orders, Complete: {IsComplete}",
                request.OrderType, request.Amount, totalExecuted, result.Count, isComplete);

            return Ok(new BestExecutionResponse
            {
                ExecutionOrders = result,
                Total = result.Count,
                TotalAmount = totalExecuted,
                RequestedAmount = request.Amount,
                IsComplete = isComplete
            });
        }
    }
}