﻿namespace Easy.MessageHub.Tests
{
    using Shouldly;
    using System;
    using System.Threading.Tasks;
    using Xunit;

    public sealed class SubscriptionsTests
    {
        [Fact]
        public void When_subscribing_value_type()
        {
            Func<Int32, Task> action = n => Task.CompletedTask;
            var key = Subscriptions.Register(TimeSpan.Zero, action);
            key.ShouldNotBeNull();

            Subscriptions.IsRegistered(Guid.NewGuid()).ShouldBeFalse();
            Subscriptions.IsRegistered(key).ShouldBeTrue();
            Subscriptions.UnRegister(key);
            Subscriptions.IsRegistered(key).ShouldBeFalse();

            var newKey = Subscriptions.Register(TimeSpan.Zero, action);
            Subscriptions.IsRegistered(newKey).ShouldBeTrue();
            Subscriptions.Clear();
            Subscriptions.IsRegistered(newKey).ShouldBeFalse();

            var subscriptionsSnapshotMain = Subscriptions.GetTheLatestSubscriptions();
            subscriptionsSnapshotMain.Length.ShouldBe(0);

            var keyA = Subscriptions.Register(TimeSpan.Zero, action);
            var subscriptionsSnapshotA = Subscriptions.GetTheLatestSubscriptions();
            subscriptionsSnapshotA.Length.ShouldBe(1);

            var keyB = Subscriptions.Register(TimeSpan.Zero, action);
            var subscriptionsSnapshotB = Subscriptions.GetTheLatestSubscriptions();
            subscriptionsSnapshotB.Length.ShouldBe(2);

            Subscriptions.IsRegistered(keyA).ShouldBeTrue();
            var subscriptionsSnapshotC = Subscriptions.GetTheLatestSubscriptions();
            subscriptionsSnapshotC.Length.ShouldBe(2);
            subscriptionsSnapshotC.ShouldBeSameAs(subscriptionsSnapshotB);

            Subscriptions.UnRegister(keyB);
            var subscriptionsSnapshotD = Subscriptions.GetTheLatestSubscriptions();
            subscriptionsSnapshotD.Length.ShouldBe(1);

            Subscriptions.Clear();
            var subscriptionsSnapshotE = Subscriptions.GetTheLatestSubscriptions();
            subscriptionsSnapshotE.Length.ShouldBe(0);
        }
    }
}