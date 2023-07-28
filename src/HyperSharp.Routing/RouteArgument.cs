using System;
using OoLunar.HyperSharp.Responders;
using OoLunar.HyperSharp.Results;

namespace OoLunar.HyperSharp.Routing
{
    public sealed record RouteArgument(string Name, Type Type, IResponder<RouteContext, Result>[] RouteArgumentValidators);
}
