using System;
using FluentResults;
using OoLunar.HyperSharp.Responders;

namespace OoLunar.HyperSharp.Routing
{
    public sealed record RouteArgument(string Name, Type Type, IResponder<RouteContext, Result>[] RouteArgumentValidators);
}
