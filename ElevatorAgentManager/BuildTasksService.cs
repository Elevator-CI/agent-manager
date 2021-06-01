using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elevator.Agent.Manager.Api.Models;
using Elevator.Agent.Manager.Queue;
using Elevator.Agent.Manager.Queue.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Logging;
using Models;
using Repositories.Repositories;

namespace Elevator.Agent.Manager.Api
{
    public class BuildTasksService
    {
        private readonly PriorityQueue<BuildTask> queue;
        private readonly BuildStepRepository buildStepRepository;
        private readonly ProjectRepository projectRepository;
        private readonly BuildRepository buildRepository;
        private readonly ILogger<BuildTasksService> logger;

        public BuildTasksService(PriorityQueue<BuildTask> queue, BuildStepRepository buildStepRepository, ILogger<BuildTasksService> logger, ProjectRepository projectRepository, BuildRepository buildRepository)
        {
            this.queue = queue;
            this.buildStepRepository = buildStepRepository;
            this.logger = logger;
            this.projectRepository = projectRepository;
            this.buildRepository = buildRepository;
        }

        public async Task<Build> PushAsync(Guid projectId, BuildTaskDto taskDto)
        {
            logger.LogInformation("Pushing task into queue");
            var build = await CreateBuildAsync(taskDto);
            var buildSteps = await buildStepRepository.GetAllFromBuildConfigAsync(taskDto.BuildConfig.Id);
            var commands = buildSteps.Select(bs => new BuildCommand
            {
                Command = bs.BuildStepScript.Command,
                Arguments = bs.BuildStepScript.Arguments
            }).ToList();
            var project = await projectRepository.FindByIdAsync(projectId);
            var buildTask = new BuildTask
            {
                Id = Guid.NewGuid(),
                Commands = commands,
                BuildId = build.Id,
                GitToken = project.GitToken,
                ProjectUrl = project.ProjectUri.ToString()
            };  
            queue.Enqueue(buildTask, Priority.Normal);
            await ChangeBuildStatus(build.Id, BuildStatus.WaitingToStart, null);
            return build;
        }

        public async Task ChangeBuildStatus(Guid id, BuildStatus status, List<string> logs)
        {
            var build = await buildRepository.FindByIdAsync(id);
            build.BuildStatus = status;
            build.Logs = logs;
            await buildRepository.UpdateAsync(build);
        }

        public async Task SetFinishedTime(Guid id, DateTime time)
        {
            var build = await buildRepository.FindByIdAsync(id);
            build.FinishTime = time;
            await buildRepository.UpdateAsync(build);
        }

        private async Task<Build> CreateBuildAsync(BuildTaskDto buildTaskDto)
        {
            var dbBuild = new global::Repositories.Database.Models.Build
            {
                BuildConfigId = buildTaskDto.BuildConfig.Id,
                BuildStatus = BuildStatus.WaitingToGetPlaceInQueue,
                FinishTime = null,
                Logs = null,
                StartedByUserId = buildTaskDto.StartedByUserId,
                StartTime = DateTime.Now
            };

            var resultBuildDb = await buildRepository.AddAsync(dbBuild);
            return new Build
            {
                Id = resultBuildDb.Id,
                BuildConfigId = resultBuildDb.BuildConfigId,
                BuildStatus = resultBuildDb.BuildStatus,
                FinishTime = resultBuildDb.FinishTime,
                Logs = resultBuildDb.Logs,
                StartedByUserId = resultBuildDb.StartedByUserId,
                StartTime = DateTime.Now
            };
        }
    }
}
