using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core;
using Vostok.Logging.Abstractions;

namespace Elevator.Agent.Manager.Client.Providers
{
    public abstract class BaseProvider
    {
        protected readonly string ApiUrl;
        protected readonly HttpClient HttpClient;

        protected BaseProvider(string apiUrl)
        {
            if (apiUrl == null)
                throw new ArgumentNullException(nameof(apiUrl));
            ApiUrl = apiUrl;
            HttpClient = new HttpClient();
        }
    }
}
