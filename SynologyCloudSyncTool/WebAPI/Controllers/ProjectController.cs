using System;
using System.Threading.Tasks;
using Akka.Actor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebAPI.Actors;
using WebAPI.Data;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("Projects")]
    public class ProjectController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IActorRef _projectActorProvider;

        public ProjectController(ProjectActorProvider provider, ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
            _projectActorProvider = provider();
        }

        [HttpPost]
        public async Task<ActionResult<Project>> SubmitProject(Project project)
        {
            project.Id = Guid.NewGuid().ToString();
            this._projectActorProvider.Tell(project);
            return Ok(project);
        }
    }
}