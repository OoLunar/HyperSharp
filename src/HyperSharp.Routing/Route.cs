using System;
using System.Threading.Tasks;
using HyperSharp.Protocol;

namespace HyperSharp.Routing
{
    public delegate Task<HyperStatus> RouteDelegate(RouteContext context);
    public sealed record Route(Uri Uri, RouteArgument[] Arguments, RouteDelegate Delegate);
}
