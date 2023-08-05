using System;
using System.Reflection;
using System.Threading;
using OoLunar.HyperSharp.Results;

namespace OoLunar.HyperSharp.Responders
{
    public delegate Result<TOutput> ResponderDelegate<TContext, TOutput>(TContext context, CancellationToken cancellationToken = default);

    public interface IResponder : IResponderBase
    {
        Result Respond(object context, CancellationToken cancellationToken = default);

        private static readonly MethodInfo _respondMethod = typeof(IResponder<,>).GetMethod(nameof(Respond), BindingFlags.Public | BindingFlags.Instance) ?? throw new MissingMethodException(nameof(IResponder), nameof(Respond));

        public static ResponderDelegate<TContext, TOutput> GetResponderDelegate<TContext, TOutput>(IResponder<TContext, TOutput> responder)
        {
            ArgumentNullException.ThrowIfNull(responder);

            return (ResponderDelegate<TContext, TOutput>)Delegate.CreateDelegate(
                typeof(ResponderDelegate<TContext, TOutput>),
                responder,
                _respondMethod.MakeGenericMethod(typeof(TContext), typeof(TOutput))
            ) ?? throw new InvalidOperationException($"Could not create responder delegate for responder \"{responder.GetType().Name}\".");
        }
    }

    public interface IResponder<TContext, TOutput> : IResponder
    {
        Result<TOutput> Respond(TContext context, CancellationToken cancellationToken = default);
        Result IResponder.Respond(object context, CancellationToken cancellationToken) => Respond((TContext)context, cancellationToken);
    }
}
