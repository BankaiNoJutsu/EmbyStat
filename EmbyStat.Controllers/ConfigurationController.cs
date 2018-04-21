using AutoMapper;
using EmbyStat.Controllers.ViewModels.Configuration;
using EmbyStat.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EmbyStat.Controllers
{
	[Produces("application/json")]
	[Route("api/[controller]")]
	public class ConfigurationController : Controller
	{
		private readonly IConfigurationService _configurationService;
		private readonly ILogger<ConfigurationController> _logger;

		public ConfigurationController(IConfigurationService configurationService, ILogger<ConfigurationController> logger)
		{
			_configurationService = configurationService;
			_logger = logger;
		}

		[HttpGet]
		public IActionResult Get()
		{
			_logger.LogInformation("Getting server configuration from database.");
			var configuration = _configurationService.GetServerSettings();
			return Ok(Mapper.Map<ConfigurationViewModel>(configuration));
		}

		[HttpPut]
		public IActionResult Update([FromBody] ConfigurationViewModel configuration)
		{
			_logger.LogInformation("Updating the new server configuration.");
			var config = Mapper.Map<Common.Models.Configuration>(configuration);
			_configurationService.SaveServerSettings(config);

			config = _configurationService.GetServerSettings();
			return Ok(Mapper.Map<ConfigurationViewModel>(config));
		}
	}
}
