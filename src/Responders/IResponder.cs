using System.Threading.Tasks;
using FluentResults;

namespace OoLunar.HyperSharp
{
    public interface IResponder
    {
        string[] Implements { get; init; }
        string[] DependsOn { get; init; }

        Task<Result<HyperStatus>> RespondAsync(HyperContext context);
    }
}
