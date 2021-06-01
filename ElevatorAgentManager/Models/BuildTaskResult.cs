using System.Collections.Generic;
using System.Collections.Immutable;

namespace Elevator.Agent.Manager.Api.Models
{
    public class BuildTaskResult
    {
        public List<string> Logs { get; set; }

        public TaskStatus Status { get; set; }
    }

    public enum TaskStatus
    {
        InProgress,
        Success,
        Failed
    }
}
