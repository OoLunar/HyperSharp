using System;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace HyperSharp.SourceGeneration
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder().AddCommandLine(args).Build();
            foreach (ITask task in typeof(Program).Assembly.GetTypes().Where(type => type.IsAssignableTo(typeof(ITask)) && type != typeof(ITask)).Select(type => (ITask)Activator.CreateInstance(type)!))
            {
                if (!task.Execute(configuration))
                {
                    Environment.Exit(1);
                }
            }
        }
    }
}
