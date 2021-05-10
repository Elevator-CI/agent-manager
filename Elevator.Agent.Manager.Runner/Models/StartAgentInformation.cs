namespace Elevator.Agent.Manager.Runner.Models
{
    public class StartAgentInformation
    {
        public int Port { get; }

        public StartAgentInformation(int port)
        {
            Port = port;
        }
    }
}
