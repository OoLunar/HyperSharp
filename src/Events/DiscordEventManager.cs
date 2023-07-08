using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DSharpPlus;
using Microsoft.Extensions.DependencyInjection;

namespace @RepositoryOwner.@RepositoryName.Events
{
    public sealed class DiscordEventManager
    {
        public DiscordIntents Intents { get; private set; }
        private readonly IServiceProvider ServiceProvider;
        private readonly List<MethodInfo> EventHandlers = new();

        public DiscordEventManager(IServiceProvider serviceProvider) => ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        public void GatherEventHandlers(Assembly assembly)
        {
            ArgumentNullException.ThrowIfNull(assembly, nameof(assembly));
            foreach (Type type in assembly.GetExportedTypes())
            {
                foreach (MethodInfo methodInfo in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
                {
                    if (methodInfo.GetCustomAttribute<DiscordEventAttribute>() is DiscordEventAttribute eventAttribute)
                    {
                        Intents |= eventAttribute.Intents;
                        EventHandlers.Add(methodInfo);
                    }
                }
            }
        }

        public void RegisterEventHandlers(object obj)
        {
            ArgumentNullException.ThrowIfNull(obj, nameof(obj));
            foreach (EventInfo eventInfo in obj.GetType().GetEvents(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            {
                foreach (MethodInfo methodInfo in EventHandlers)
                {
                    if (eventInfo.EventHandlerType!.GetGenericArguments().SequenceEqual(methodInfo.GetParameters().Select(parameter => parameter.ParameterType)))
                    {
                        Delegate handler = methodInfo.IsStatic
                            ? Delegate.CreateDelegate(eventInfo.EventHandlerType, methodInfo)
                            : Delegate.CreateDelegate(eventInfo.EventHandlerType, ActivatorUtilities.CreateInstance(ServiceProvider, methodInfo.DeclaringType!), methodInfo);

                        eventInfo.AddEventHandler(obj, handler);
                    }
                }
            }
        }
    }
}
