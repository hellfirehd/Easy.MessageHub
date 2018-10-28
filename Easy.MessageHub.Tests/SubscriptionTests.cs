namespace Easy.MessageHub.Tests
{
    using Shouldly;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public sealed class SubscriptionTests
    {
        [Fact]
        public void When_creating_a_subscription_with_no_throttle()
        {
            var result = String.Empty;

            var type = typeof(String);
            var token = Guid.NewGuid();
            var throttleBy = TimeSpan.Zero;
            Func<String, Task> handler = msg => { result = msg; return Task.CompletedTask; };

            var subscription = new Subscription(type, token, throttleBy, handler);

            subscription.Type.ShouldBe(typeof(String));
            subscription.Token.ShouldBe(token);

            subscription.HandleAsync("Foo");
            result.ShouldBe("Foo");

            subscription.HandleAsync("Bar");
            result.ShouldBe("Bar");
        }

        [Fact]
        public void When_creating_a_subscription_with_throttle()
        {
            var result = String.Empty;

            var type = typeof(String);
            var token = Guid.NewGuid();
            var throttleBy = TimeSpan.FromMilliseconds(150);
            Func<String, Task> handler = msg => { result = msg; return Task.CompletedTask; };

            var subscription = new Subscription(type, token, throttleBy, handler);

            subscription.Type.ShouldBe(typeof(String));
            subscription.Token.ShouldBe(token);

            subscription.HandleAsync("Foo");
            result.ShouldBe("Foo");

            subscription.HandleAsync("Bar");
            result.ShouldBe("Foo");

            Thread.Sleep(300);
            subscription.HandleAsync("Bar");
            result.ShouldBe("Bar");
        }
    }
}