using System;
using HyperSharp.Responders;
using HyperSharp.Results;

namespace HyperSharp.Routing
{
    public sealed record RouteArgument(string Name, Type Type, IResponder<RouteContext, Result>[] RouteArgumentValidators);
}
