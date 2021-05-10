using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

namespace Elevator.Agent.Manager.Api.Utils
{
    public static class FreeTcpPortFinder
    {
        //todo(likvidator): эта штука в лоб не хочет видеть занятые докером порты, надо будет доразбираться, чо с этим делать
        //todo(likvidator): хотя по-хорошему, порты докера === tcpListeners, и они там должны быть
        public static IEnumerable<int> GetAvailablePort(int startingPort, int count = 1)
        {
            var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();

            var connectionsEndpoints = ipGlobalProperties.GetActiveTcpConnections().Select(c => c.LocalEndPoint);
            var tcpListenersEndpoints = ipGlobalProperties.GetActiveTcpListeners();
            var udpListenersEndpoints = ipGlobalProperties.GetActiveUdpListeners();

            var portsInUse = connectionsEndpoints.Concat(tcpListenersEndpoints)
                .Concat(udpListenersEndpoints)
                .Select(e => e.Port);

           return Enumerable.Range(startingPort, ushort.MaxValue - startingPort + 1).Except(portsInUse).Take(count);
        }
    }
}
