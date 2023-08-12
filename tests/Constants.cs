using System;
using Microsoft.Extensions.DependencyInjection;

namespace OoLunar.HyperSharp.Tests
{
    public static class Constants
    {
        public static readonly IServiceProvider ServiceProvider;

        static Constants() => ServiceProvider = new ServiceCollection().BuildServiceProvider();
    }
}
