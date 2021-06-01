using System;
using System.Threading.Tasks;
using Elevator.Agent.Manager.Api.AgentCommunication;
using Elevator.Agent.Manager.Api.Models;

namespace Elevator.Agent.Manager.Api.Repositories
{
    public interface ICurrentTasksRepository
    {
        Task AddAsync(BuildTask task, AgentClient agent);

        Task<bool> ExistsAsync(Guid id);

        Task<(BuildTask, AgentClient)> GetByIdAsync(Guid id);

        Task RemoveAsync(Guid id);

        Task<(BuildTask, AgentClient)[]> GetAllAsync();
    }
}