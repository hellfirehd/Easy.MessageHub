﻿namespace Easy.MessageHub.Tests.Unit
{
    using System;
    using System.Threading;
    using NUnit.Framework;
    using Shouldly;

    [TestFixture]
    internal sealed class SubscriptionTests
    {
        [Test]
        public void When_creating_a_subscription_with_no_throttle()
        {
            var result = string.Empty;

            var type = typeof(string);
            var token = Guid.NewGuid();
            var throttleBy = TimeSpan.Zero;
            Action<string> handler = msg => result = msg;

            var subscription = new Subscription(type, token, throttleBy, handler);

            subscription.Type.ShouldBe(typeof(string));
            subscription.Token.ShouldBe(token);

            subscription.HandleAsync("Foo");
            result.ShouldBe("Foo");

            subscription.HandleAsync("Bar");
            result.ShouldBe("Bar");
        }

        [Test]
        public void When_creating_a_subscription_with_throttle()
        {
            var result = string.Empty;

            var type = typeof(string);
            var token = Guid.NewGuid();
            var throttleBy = TimeSpan.FromMilliseconds(150);
            Action<string> handler = msg => result = msg;

            var subscription = new Subscription(type, token, throttleBy, handler);

            subscription.Type.ShouldBe(typeof(string));
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