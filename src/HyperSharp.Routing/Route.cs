using System;
using System.Threading.Tasks;

namespace OoLunar.HyperSharp.Routing
{
    public delegate Task<HyperStatus> RouteDelegate(RouteContext context);
    public sealed record Route(Uri Uri, RouteArgument[] Arguments, RouteDelegate Delegate);
}
