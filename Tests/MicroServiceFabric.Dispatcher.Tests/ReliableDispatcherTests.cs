﻿using System;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using NSubstitute;
using Xunit;

namespace MicroServiceFabric.Dispatcher.Tests
{
    public sealed class ReliableDispatcherTests
    {
        [Fact]
        public void ctor_ReliableQueueRequired()
        {
            Assert.Throws<ArgumentNullException>(
                "reliableQueue",
                () =>
                    new ReliableDispatcher<object>(
                        null,
                        Substitute.For<ITransactionFactory>()));
        }

        [Fact]
        public void ctor_TransactionFactoryRequired()
        {
            Assert.Throws<ArgumentNullException>(
                "transactionFactory",
                () =>
                    new ReliableDispatcher<object>(
                        Substitute.For<Lazy<IReliableQueue<object>>>(),
                        null));
        }

        [Fact]
        public async Task EnqueueAsync_ItemRequired()
        {
            var reliableDispatcher = CreateReliableDispatcher();

            await Assert.ThrowsAsync<ArgumentNullException>(
                "item",
                () => reliableDispatcher.EnqueueAsync(null));
        }

        [Fact]
        public async Task EnqueueAsync_EnqueuesItemOnReliableQueue()
        {
            var reliableQueue = Substitute.For<IReliableQueue<object>>();
            var transaction = Substitute.For<ITransaction>();
            var item = Substitute.For<object>();
            var reliableDispatcher = CreateReliableDispatcher(reliableQueue, CreateTransactionFactory(transaction));

            await reliableDispatcher.EnqueueAsync(item);

            await reliableQueue
                .Received()
                .EnqueueAsync(transaction, item);
        }

        public void EnqueueAsync_CommitsTransactionAfterEnqueuing() { }

        public void EnqueueAsync_DisposesTransaction() { }

        private static IReliableDispatcher<object> CreateReliableDispatcher(IReliableQueue<object> reliableQueue = null,
            ITransactionFactory transactionFactory = null)
        {
            return new ReliableDispatcher<object>(
                new Lazy<IReliableQueue<object>>(() => reliableQueue ?? Substitute.For<IReliableQueue<object>>()),
                transactionFactory ?? Substitute.For<ITransactionFactory>());
        }

        private static ITransactionFactory CreateTransactionFactory(ITransaction transaction)
        {
            var transactionFactory = Substitute.For<ITransactionFactory>();

            transactionFactory
                .Create()
                .Returns(transaction);

            return transactionFactory;
        }
    }
}
