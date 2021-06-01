using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Elevator.Agent.Manager.Api.AgentCommunication;
using Elevator.Agent.Manager.Api.AgentCommunication.Models;
using Elevator.Agent.Manager.Api.Models;
using Elevator.Agent.Manager.Api.Repositories;
using Elevator.Agent.Manager.Queue;
using Elevator.Agent.Manager.Queue.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Models;
using TaskStatus = Elevator.Agent.Manager.Api.Models.TaskStatus;

namespace Elevator.Agent.Manager.Api.Hosting.Tasks
{
    public class TasksService: IHostedService, IDisposable
    {
        private readonly ILogger<TasksService> logger;
        private readonly PriorityQueue<BuildTask> queue;
        private Timer timer;
        private readonly AgentsService agentsService;
        private BuildTasksService buildTasksService;
        private readonly ICurrentTasksRepository currentTasksRepository;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly IServiceScope scope;

        public TasksService(ILogger<TasksService> logger,
                            PriorityQueue<BuildTask> queue,
                            AgentsService agentsService,
                            ICurrentTasksRepository currentTasksRepository,
                            IServiceScopeFactory serviceScopeFactory)
        {
            this.logger = logger;
            this.queue = queue;
            this.agentsService = agentsService;
            this.currentTasksRepository = currentTasksRepository;
            this.serviceScopeFactory = serviceScopeFactory;
            scope = serviceScopeFactory.CreateScope();
            buildTasksService = scope.ServiceProvider.GetService<BuildTasksService>();
        }

        private async Task Process()
        {
            logger.LogInformation($"Iteration started");

            if (!agentsService.IsServiceReady)
            {
                logger.LogInformation("Agent service is not ready. Skip iteration...");
                return;
            }

            try
            {
                await ProcessQueue();
                await ProcessCurrentTasks();
                logger.LogInformation("All is ok");
            }
            catch (Exception e)
            {
                logger.LogError("Catch", e.Message);
            }
        }

        private async Task ProcessQueue()
        {
            var dequeued = queue.TryDequeue(out var task);
            if (!dequeued)
            {
                logger.LogInformation("No tasks in queue. Skip iteration...");
                return;
            }

            logger.LogInformation($"Running process build task with id='{task.Id}'");

            logger.LogInformation("Searching for free agent");
            var findFreeAgentResult = await FindFreeAgentAsync();
            if (!findFreeAgentResult.IsSuccessful)
            {
                logger.LogInformation("Free agent did not found. Skip iteration...");
                queue.Enqueue(task, Priority.High);
                return;
            }

            logger.LogInformation("Pushing task to agent...");

            var agent = findFreeAgentResult.Value;
            var pushTaskResult = await agent.PushTaskAsync(task);
            if (!pushTaskResult.IsSuccessful)
            {
                logger.LogError("Push task to agent failed");
                queue.Enqueue(task, Priority.High);
                return;
            }

            await currentTasksRepository.AddAsync(task, agent);
            await buildTasksService.ChangeBuildStatus(task.BuildId, BuildStatus.InProgress, null);
        }

        private async Task<OperationResult<AgentClient>> FindFreeAgentAsync()
        {
            foreach (var agent in agentsService.Agents)
            {
                var getAgentStatusResult = await agent.GetAgentStatusAsync();
                if (!getAgentStatusResult.IsSuccessful)
                    logger.LogError("Cannot get status from agent");
                else if (getAgentStatusResult.Value.Status == AgentStatus.Free)
                    return OperationResult<AgentClient>.Success(agent);
            }
            return OperationResult<AgentClient>.Failed();
        }

        private async Task ProcessCurrentTasks()
        {
            foreach (var (task, agent) in await currentTasksRepository.GetAllAsync())
            {
                logger.LogInformation($"Updating status of task with id='{task.Id}'");
                var pullTaskResult = await agent.PullResultAsync();
                if (!pullTaskResult.IsSuccessful)
                {
                    logger.LogError("Cannot get task result from agent");
                    return;
                }

                await buildTasksService.ChangeBuildStatus(task.BuildId,
                    ConvertToBuildStatus(pullTaskResult.Value.Status), pullTaskResult.Value.Logs);

                var getAgentStatusResult = await agent.GetAgentStatusAsync();
                if (!getAgentStatusResult.IsSuccessful)
                {
                    logger.LogError("Cannot get status from agent");
                    return;
                }

                if (getAgentStatusResult.Value.Status == AgentStatus.Finished)
                {
                    var freeAgentResult = await agent.FreeAgentAsync();
                    if (!freeAgentResult.IsSuccessful)
                    {
                        logger.LogError("Cannot free agent");
                        return;
                    }

                    await buildTasksService.ChangeBuildStatus(task.BuildId,
                        ConvertToBuildStatus(freeAgentResult.Value.Status), freeAgentResult.Value.Logs);
                    await buildTasksService.SetFinishedTime(task.BuildId, DateTime.Now);
                    await currentTasksRepository.RemoveAsync(task.Id);
                }
            }
        }

        private BuildStatus ConvertToBuildStatus(TaskStatus taskStatus)
        {
            switch (taskStatus)
            {
                case TaskStatus.InProgress:
                    return BuildStatus.InProgress;
                case TaskStatus.Success:
                    return BuildStatus.Success;
                case TaskStatus.Failed:
                    return BuildStatus.Failed;
                default:
                    throw new ArgumentOutOfRangeException(nameof(taskStatus), taskStatus, null);
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            timer = new Timer(_ => Process(), null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await timer.DisposeAsync();
        }

        public void Dispose()
        {
            timer?.Dispose();
            scope.Dispose();
        }
    }
}
