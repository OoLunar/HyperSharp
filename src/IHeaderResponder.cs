using System.Threading.Tasks;
using FluentResults;

namespace OoLunar.HyperSharp
{
    public interface IHeaderResponder
    {
        Task<Result<HyperStatus>> RespondAsync(HyperContext context);
    }
}
