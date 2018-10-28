namespace Easy.MessageHub
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    internal sealed class Subscription
    {
        private const Int64 TicksMultiplier = 1000 * TimeSpan.TicksPerMillisecond;
        private readonly Int64 _throttleByTicks;
        private Double? _lastHandleTimestamp;

        internal Subscription(Type type, Guid token, TimeSpan throttleBy, Object handler)
        {
            Type = type;
            Token = token;
            Handler = handler;
            _throttleByTicks = throttleBy.Ticks;
        }

        internal Task HandleAsync<T>(T message)
        {
            if (!CanHandle()) { return Task.CompletedTask; }

            var handler = Handler as Func<T, Task>;

            return handler(message);
        }

        internal Boolean CanHandle()
        {
            if (_throttleByTicks == 0) { return true; }

            if (_lastHandleTimestamp == null)
            {
                _lastHandleTimestamp = Stopwatch.GetTimestamp();
                return true;
            }

            var now = Stopwatch.GetTimestamp();
            var durationInTicks = (now - _lastHandleTimestamp) / Stopwatch.Frequency * TicksMultiplier;

            if (durationInTicks >= _throttleByTicks)
            {
                _lastHandleTimestamp = now;
                return true;
            }

            return false;
        }

        internal Guid Token { get; }
        internal Type Type { get; }
        private Object Handler { get; }
    }
}