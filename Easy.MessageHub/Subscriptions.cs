namespace Easy.MessageHub
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    internal static class Subscriptions
    {
        private static readonly List<Subscription> AllSubscriptions = new List<Subscription>();
        private static Int32 _subscriptionsChangeCounter;

        [ThreadStatic]
        private static Int32 _localSubscriptionRevision;

        [ThreadStatic]
        private static Subscription[] _localSubscriptions;

        internal static Guid Register<T>(TimeSpan throttleBy, Func<T, Task> action)
        {
            var type = typeof(T);
            var key = Guid.NewGuid();
            var subscription = new Subscription(typeof(T), key, throttleBy, action);

            lock (AllSubscriptions)
            {
                AllSubscriptions.Add(subscription);
                _subscriptionsChangeCounter++;
            }

            return key;
        }

        internal static void UnRegister(Guid token)
        {
            lock (AllSubscriptions)
            {
                var subscription = AllSubscriptions.Find(s => s.Token == token);
                if (subscription == null) { return; }

                var removed = AllSubscriptions.Remove(subscription);
                if (!removed) { return; }

                if (_localSubscriptions != null)
                {
                    var localIdx = Array.IndexOf(_localSubscriptions, subscription);
                    if (localIdx >= 0) { _localSubscriptions = RemoveAt(_localSubscriptions, localIdx); }
                }

                _subscriptionsChangeCounter++;
            }
        }

        internal static void Clear()
        {
            lock (AllSubscriptions)
            {
                AllSubscriptions.Clear();
                if (_localSubscriptions != null)
                {
                    Array.Clear(_localSubscriptions, 0, _localSubscriptions.Length);
                }
                _subscriptionsChangeCounter++;
            }
        }

        internal static Boolean IsRegistered(Guid token)
        {
            lock (AllSubscriptions) { return AllSubscriptions.Any(s => s.Token == token); }
        }

        internal static Subscription[] GetTheLatestSubscriptions()
        {
            if (_localSubscriptions == null) { _localSubscriptions = new Subscription[0]; }

            var changeCounterLatestCopy = Interlocked.CompareExchange(ref _subscriptionsChangeCounter, 0, 0);
            if (_localSubscriptionRevision == changeCounterLatestCopy) { return _localSubscriptions; }

            Subscription[] latestSubscriptions;
            lock (AllSubscriptions)
            {
                latestSubscriptions = AllSubscriptions.ToArray();
            }

            _localSubscriptionRevision = changeCounterLatestCopy;
            _localSubscriptions = latestSubscriptions;
            return _localSubscriptions;
        }

        private static T[] RemoveAt<T>(T[] source, Int32 index)
        {
            var dest = new T[source.Length - 1];
            if (index > 0) { Array.Copy(source, 0, dest, 0, index); }

            if (index < source.Length - 1)
            {
                Array.Copy(source, index + 1, dest, index, source.Length - index - 1);
            }

            return dest;
        }
    }
}