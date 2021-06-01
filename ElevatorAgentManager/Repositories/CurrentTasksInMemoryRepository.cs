using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elevator.Agent.Manager.Api.AgentCommunication;
using Elevator.Agent.Manager.Api.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Elevator.Agent.Manager.Api.Repositories
{
    public class CurrentTasksInMemoryRepository: ICurrentTasksRepository
    {
        private ConcurrentDictionary<Guid, (BuildTask, AgentClient)> tasks;

        public CurrentTasksInMemoryRepository()
        {
            tasks = new();
        }

        public Task AddAsync(BuildTask task, AgentClient agent)
        {
            if (tasks.ContainsKey(task.Id))
                throw new InvalidOperationException($"BuildTask with id='{task.Id}' already exists");
            tasks[task.Id] = (task, agent);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(Guid id)
        {
            return Task.FromResult(tasks.ContainsKey(id));
        }

        public Task<(BuildTask, AgentClient)> GetByIdAsync(Guid id)
        {
            return Task.FromResult(tasks[id]);
        }

        public Task RemoveAsync(Guid id)
        {
            tasks.TryRemove(id, out _);
            return Task.CompletedTask;
        }

        public Task<(BuildTask, AgentClient)[]> GetAllAsync()
        {
            return Task.FromResult(tasks.Select(kv => kv.Value).ToArray());
        }
    }
}
