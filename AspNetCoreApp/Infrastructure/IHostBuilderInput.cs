using Microsoft.Extensions.Hosting;

namespace AspNetCoreApp.Infrastructure
{
    public interface IHostBuilderInput
    {
        IHostBuilder HostBuilder { get; set; }
    }
}