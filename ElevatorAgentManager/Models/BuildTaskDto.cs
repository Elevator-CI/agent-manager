using Models;

namespace Elevator.Agent.Manager.Api.Models
{
    public sealed class BuildTaskDto
    {
        public string StartedByUserId { get; set; }

        public BuildConfig BuildConfig { get; set; }
    }
}
