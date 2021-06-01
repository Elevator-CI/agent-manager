using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Elevator.Agent.Manager.Api.Models;
using Elevator.Agent.Manager.Queue;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace Elevator.Agent.Manager.Api.Controllers
{
    [ApiController]
    [Route("buildTasks")]
    public class TasksController: Controller
    {
        private readonly BuildTasksService buildTasksService;

        public TasksController(BuildTasksService buildTasksService)
        {
            this.buildTasksService = buildTasksService;
        }

        [HttpPost]
        public async Task<HttpOperationResult<Build>> PushTaskAsync([FromBody] BuildTaskDto buildTaskDto)
        {
            var build = await buildTasksService.PushAsync(buildTaskDto.BuildConfig.ProjectId, buildTaskDto);

            return HttpOperationResult<Build>.Ok(build);
        }

        [HttpGet("test")]
        public string Test()
        {
            return "poshel nahui";
        }
    }
}
