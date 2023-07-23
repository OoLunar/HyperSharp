using System;

namespace OoLunar.HyperSharp.Routing
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Delegate, AllowMultiple = true, Inherited = false)]
    public sealed class RouteAttribute : Attribute
    {
        public Uri Route { get; init; }

        public RouteAttribute(string route) => Route = new Uri(route, UriKind.RelativeOrAbsolute);
    }
}
