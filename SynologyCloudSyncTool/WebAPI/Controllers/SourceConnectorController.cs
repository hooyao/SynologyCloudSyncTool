using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebAPI.Data;
using WebAPI.IO;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("sourceconnectors")]
    public class SourceConnectorController : ControllerBase
    {
        private readonly AzureStorageService _azureStorageService;
        private readonly ILogger<WeatherForecastController> _logger;

        public SourceConnectorController(AzureStorageService azureStorageService, ILogger<WeatherForecastController> logger)
        {
            _azureStorageService = azureStorageService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult> CreateConnector(ConnectorDto connectorDto)
        {
            if (connectorDto.Type == ConnectorType.SOURCE_AZURE_BLOB)
            {
                var id = this._azureStorageService.InitblobService(connectorDto.ConnString);
                connectorDto.Id = id;
                return Ok(connectorDto);
            }

            return BadRequest();
        }

        [HttpGet("{connectorId}/{containerName}/items")]
        public async Task<ActionResult<List<ItemDto>>> GetItems(
            string connectorId,
             string containerName)
        {
            return Ok(await this._azureStorageService.ListItemsAsync(connectorId, containerName));
        }
        
    }
}