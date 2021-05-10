using System;

namespace Elevator.Agent.Manager.Runner.Models
{
    public class AgentInformation
    {
        public Uri Url { get; }

        public AgentInformation(Uri url)
        {
            Url = url ?? throw new ArgumentNullException(nameof(url));
        }
    }
}
