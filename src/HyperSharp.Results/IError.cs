using System.Collections.Generic;

namespace OoLunar.HyperSharp.Results
{
    public interface IError
    {
        string Message { get; }
        IEnumerable<IError> Errors { get; }
    }
}
