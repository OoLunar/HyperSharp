using System;

namespace HyperSharp.Results
{
    /// <summary>
    /// Represents the status of a result.
    /// </summary>
    [Flags]
    public enum ResultStatus
    {
        /// <summary>
        /// The result has failed and has no value.
        /// </summary>
        None = 0 << 0,

        /// <summary>
        /// The result has succeeded.
        /// </summary>
        IsSuccess = 1 << 0,

        /// <summary>
        /// The result does not contain a value.
        /// </summary>
        HasValue = 1 << 1,
    }
}
