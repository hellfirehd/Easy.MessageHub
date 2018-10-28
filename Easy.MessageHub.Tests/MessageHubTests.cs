namespace Easy.MessageHub.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Shouldly;
    using Xunit;

    public sealed class MessageHubTests
    {
        [Fact]
        public async Task Run()
        {
            await UsageExamples();
            await MessageHubAssertions.Run();
        }

        private static async Task UsageExamples()
        {
            var hub = MessageHub.Instance;

            // As the hub is a singleton, we have to add this as a hack to allow 
            // multiple assertions each with their own GlobalHandler registered to pass
            var isUsageExampleRunning = true;

            var errors = new Queue<KeyValuePair<Guid, Exception>>();
            hub.RegisterGlobalErrorHandler(
                (token, e) => errors.Enqueue(new KeyValuePair<Guid, Exception>(token, e)));

            var auditQueue = new Queue<MessageBase>();

            var allMessagesQueue = new Queue<MessageBase>();
            var commandsQueue = new Queue<Command>();
            var ordersQueue = new Queue<Order>();

            void AuditHandler(Type type, Object msg)
            {
                // ReSharper disable once AccessToModifiedClosure
                if (!isUsageExampleRunning) { return; }

                msg.ShouldBeAssignableTo<MessageBase>();
                auditQueue.Enqueue((MessageBase) msg);
            }

            hub.RegisterGlobalHandler(AuditHandler);

            await hub.PublishAsync(new MessageBase { Name = "Base" });

            commandsQueue.ShouldBeEmpty();
            ordersQueue.ShouldBeEmpty();

            auditQueue.Count.ShouldBe(1);
            auditQueue.Dequeue().Name.ShouldBe("Base");

            allMessagesQueue.ShouldBeEmpty();

            hub.Subscribe<MessageBase>(async msg =>
            {
                msg.ShouldBeAssignableTo<MessageBase>();
                allMessagesQueue.Enqueue(msg);
            });

            hub.Subscribe<Command>(async msg =>
            {
                msg.ShouldBeAssignableTo<MessageBase>();
                msg.ShouldBeAssignableTo<Command>();
                commandsQueue.Enqueue(msg);
            });

            hub.Subscribe<OpenCommand>(async msg =>
            {
                msg.ShouldBeAssignableTo<Command>();
                msg.ShouldBeOfType<OpenCommand>();
                commandsQueue.Enqueue(msg);
            });

            hub.Subscribe<CloseCommand>(async msg =>
            {
                msg.ShouldBeAssignableTo<Command>();
                msg.ShouldBeOfType<CloseCommand>(); 
                commandsQueue.Enqueue(msg);
            });

            hub.Subscribe<Order>(async msg =>
            {
                msg.ShouldBeOfType<Order>();
                ordersQueue.Enqueue(msg);
            });

            await hub.PublishAsync(new Command { Name = "Command" });

            auditQueue.Count.ShouldBe(1);
            auditQueue.Dequeue().Name.ShouldBe("Command");

            allMessagesQueue.Count.ShouldBe(1);
            allMessagesQueue.Dequeue().Name.ShouldBe("Command");

            commandsQueue.Count.ShouldBe(1);
            commandsQueue.Dequeue().Name.ShouldBe("Command");

            ordersQueue.ShouldBeEmpty();

            await hub.PublishAsync(new Order { Name = "Order1" });

            auditQueue.Count.ShouldBe(1);
            auditQueue.Dequeue().Name.ShouldBe("Order1");

            allMessagesQueue.Count.ShouldBe(1);
            allMessagesQueue.Dequeue().Name.ShouldBe("Order1");

            commandsQueue.ShouldBeEmpty();

            ordersQueue.Count.ShouldBe(1);
            ordersQueue.Dequeue().Name.ShouldBe("Order1");

            hub.Subscribe(new Func<Order, Task>(o => { ordersQueue.Enqueue(o); return Task.CompletedTask; }));

            await hub.PublishAsync(new Order { Name = "Order2" });

            auditQueue.Count.ShouldBe(1);
            auditQueue.Dequeue().Name.ShouldBe("Order2");

            allMessagesQueue.Count.ShouldBe(1);
            allMessagesQueue.Dequeue().Name.ShouldBe("Order2");

            ordersQueue.Count.ShouldBe(2);
            ordersQueue.Dequeue().Name.ShouldBe("Order2");
            ordersQueue.Dequeue().Name.ShouldBe("Order2");

            commandsQueue.ShouldBeEmpty();

            await hub.PublishAsync(new OpenCommand { Name = "OpenCommand"});

            auditQueue.Count.ShouldBe(1);
            auditQueue.Dequeue().Name.ShouldBe("OpenCommand");

            allMessagesQueue.Count.ShouldBe(1);
            allMessagesQueue.Dequeue().Name.ShouldBe("OpenCommand");

            ordersQueue.ShouldBeEmpty();

            commandsQueue.Count.ShouldBe(2);

            errors.ShouldBeEmpty("No errors should have occurred!");

            isUsageExampleRunning = false;
        }
    }

    internal class MessageBase
    {
        public String Name { get; set; }
    }

    internal class Command : MessageBase {}
    internal sealed class OpenCommand : Command {}
    internal sealed class CloseCommand : Command {}
    internal sealed class Order : MessageBase {}
}