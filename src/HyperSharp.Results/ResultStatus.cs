using System;

namespace OoLunar.HyperSharp.Results
{
    [Flags]
    public enum ResultStatus
    {
        None = 0 << 0,
        IsSuccess = 1 << 0,
        HasValue = 1 << 1,
    }
}
