using Microsoft.Extensions.Configuration;

namespace HyperSharp.SourceGeneration
{
    public interface ITask
    {
        bool Execute(IConfiguration configuration);
    }
}
