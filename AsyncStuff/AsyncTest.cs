using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace AsyncStuff
{
    using System.Globalization;
    using System.Runtime.Remoting.Messaging;
    using System.Threading;
    using System.Windows.Forms;

    [TestFixture]
    public class AsyncTest
    {
        [Test]
        public void MethodReturnsTaskOfTWithoutWait()
        {
            // what is the problem with this (only relevant with certain sync contexts)
            const string expected = "foo";
            
            var actual = SyncMethods
                            .WaitASecondAndReturnAValue(expected)
                            .Result;
            
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void MethodReturnsTaskOfTWithoutWaitUsingTaskCompletionSource()
        {
            // what is the problem with this (only relevant with certain sync contexts)
            const string expected = "foo";
            
            var actual = SyncMethods
                .WaitASecondAndReturnAValueUsingTaskCompletionSource(expected)
                .Result;
            
            Assert.AreEqual(expected, actual);
        }
        
        [Test]
        public void MethodReturnsATaskOfTAndMutatesGlobalState()
        {
            const int expected = 5;

            SyncMethods.MutateGlobalState();
            
            Assert.AreEqual(expected, MutableGlobalState.State);
        }
        
        [Test]
        public async Task AsyncMethodReturnsTaskOfT()
        {             
            const string expected = "foo";
            
            var actual = await AsyncMethods
                .WaitASecondAndReturnAValueAsync(expected);
            
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public async Task AsyncMethodMutatesGlobalStateWithoutAwait()
        {
            const int expected = 6;
            
            AsyncMethods.MutateGlobalStateAfterASecondAsync(expected);
            
            Assert.AreEqual(expected, MutableGlobalState.State);
        }
        
        [Test]
        public async Task AsyncMethodMutatesGlobalStateWithAwait()
        {
            const int expected = 7;
            
            await AsyncMethods.MutateGlobalStateAfterASecondAsync(expected);
            
            Assert.AreEqual(expected, MutableGlobalState.State);
        }

        [Test]
        public void SyncExceptionHandlingWithoutAwait()
        {
            Exception exceptionThrown = null;
            try
            {
                SyncMethods.ThrowAnExceptionAfterOneSecond(1);
            }
            catch (Exception ex)
            {
                exceptionThrown = ex;
            }
            Assert.IsNotNull(exceptionThrown);
        }     
        
        [Test]
        public async Task SyncExceptionHandlingWithAwait()
        {
            Exception exceptionThrown = null;
            try
            {                
                await SyncMethods.ThrowAnExceptionAfterOneSecond(1);
            }
            catch (Exception ex)
            {
                exceptionThrown = ex;
            }
            Assert.IsNotNull(exceptionThrown);
            Assert.IsInstanceOf<AggregateException>(exceptionThrown);
        }

        [Test]
        public void AwaitedAsyncWrappedInNonAwaitedAction()
        {
            Exception exceptionThrown = null;
            Action willThrow = async () => await SyncMethods.ThrowAnExceptionAfterOneSecond(1);
            try
            {
                willThrow();
            }
            catch (Exception ex)
            {
                exceptionThrown = ex;
            }
            Assert.IsNotNull(exceptionThrown);            
        }
        
        [Test]
        public async Task AwaitedAsyncWrappedInAwaitedAction()
        {
//            Exception exceptionThrown = null;
//            Action willThrow = async () => await SyncMethods.ThrowAnExceptionAfterOneSecond();
//            try
//            {
//                await willThrow();
//            }
//            catch (Exception ex)
//            {
//                exceptionThrown = ex;
//            }
//            Assert.IsNotNull(exceptionThrown);   
            Assert.Fail("This will not compile because void (returned by action) is not awaitable");
        }

        [Test]
        public async Task AwaitedAsyncWrappedInNonAwaitedFuncOfTask()
        {
            Exception exceptionThrown = null;
            Func<Task> willThrow = async () => await SyncMethods.ThrowAnExceptionAfterOneSecond(1);
            try
            {
                //awaitable because it's a func
                await willThrow();
            }
            catch (Exception ex)
            {
                exceptionThrown = ex;
            }
            Assert.IsNotNull(exceptionThrown);                        
        }

        [Test]
        public async Task AwaitingAcrossALock()
        {
            //Luckily this won't compile
//            lock (MutableGlobalState.Lease)
//            {
//                await AsyncMethods.MutateGlobalStateAfterASecondAsync(10);
//            }
            
        }

        [Test]
        public async Task LosingMyException()
        {
            Exception exceptionThrown = null;
            var delay = Task.Delay(TimeSpan.FromSeconds(0.5));
            var errorProneTask = SyncMethods.ThrowAnExceptionAfterOneSecond(1);

            try
            {
                var firstTaskToComplete = await Task.WhenAny(delay, errorProneTask);

                Assert.AreEqual(delay, firstTaskToComplete);

                //give the error prone task a chance to fail.
                await Task.Delay(1);

            }
            catch (Exception ex)
            {
                exceptionThrown = ex;
            }
            Assert.IsNotNull(exceptionThrown);
        }
        
        [Test]
        public async Task FindingMyException()
        {
            Exception exceptionThrown = null;
            var delay = Task.Delay(TimeSpan.FromSeconds(0.5));
            var errorProneTask = SyncMethods
                .ThrowAnExceptionAfterOneSecond(1)
                .ContinueWith(task => exceptionThrown = task.Exception);

            var firstTaskToComplete = await Task.WhenAny(delay, errorProneTask);
    
            Assert.AreEqual(delay, firstTaskToComplete);

            //give the error prone task a chance to fail.
            await Task.Delay(1);

            Assert.IsNotNull(exceptionThrown);
        }

        [Test]
        public async Task WhatHappensWhenTwoOperationsThrowAnExceptionAndUsingWhenAny()
        {
            Exception exceptionThrowByA = null;
            Exception exceptionThrowByB = null;

            var aWithExceptionHandler = AsyncMethods
                                            .ThrowAnExceptionAfterNSecondAsync(1)
                                            .ContinueWith(task => exceptionThrowByA = task.Exception);


            var bWithExceptionHandler = AsyncMethods
                                            .ThrowAnExceptionAfterNSecondAsync(2)
                                            .ContinueWith(task =>  exceptionThrowByB = task.Exception);
                                        
            await Task.WhenAny
                        (aWithExceptionHandler,
                         bWithExceptionHandler);
                            
            Assert.IsNotNull(exceptionThrowByA, "a - exception is missing");
            Assert.IsNotNull(exceptionThrowByB, "b - exception is missing");
        }
        
        [Test]
        public async Task WhatHappensWhenTwoOperationsThrowAnExceptionWhenAll()
        {
            Exception exceptionThrowByA = null;
            Exception exceptionThrowByB = null;

            var aWithExceptionHandler = AsyncMethods
                .ThrowAnExceptionAfterNSecondAsync(1)
                .ContinueWith(task => exceptionThrowByA = task.Exception);


            var bWithExceptionHandler = AsyncMethods
                .ThrowAnExceptionAfterNSecondAsync(2)
                .ContinueWith(task =>  exceptionThrowByB = task.Exception);
                                        
            await Task.WhenAll
                    (aWithExceptionHandler,
                        bWithExceptionHandler);
                            
            Assert.IsNotNull(exceptionThrowByA, "a - exception is missing");
            Assert.IsNotNull(exceptionThrowByB, "b - exception is missing");
        }

        [Test, Ignore("Unignore this when you want to observe a deadlock")]
        public void WhatAboutDeadlocksWhenUsingResult()
        {
            var blockingSyncContext = new WindowsFormsSynchronizationContext();            
            SynchronizationContext.SetSynchronizationContext(blockingSyncContext);                       
            
            // Result is invoked on the same thread as the blocking sync context
            // Because the sync context blocked when calling the async method,
            // The call to result is deadlocked as it's waiting for the original call
            // to release the thread.
            // A way to see this visually is to execute this from a GUI, it would become non responsive. 
            var theValue = AsyncMethods
                                .WaitASecondAndReturnAValueAsync("this is doomed to deadlock")
                                .Result;            
        }

        [Test, Ignore("Unignore this when you want to observe a deadlock")]
        public void SomeProblemWithGetAwaiterGetResult()
        {
            var blockingSyncContext = new WindowsFormsSynchronizationContext();            
            SynchronizationContext.SetSynchronizationContext(blockingSyncContext);                       
            var theValue = AsyncMethods
                .WaitASecondAndReturnAValueAsync("this too is doomed")                            
                .GetAwaiter()
                .GetResult();            
        }
        
        [Test]
        public void HowDoIFixThisDeadLockByAllowingCompletionOnADifferentThread()
        {
            var blockingSyncContext = new WindowsFormsSynchronizationContext();            
            SynchronizationContext.SetSynchronizationContext(blockingSyncContext);
            Func<Task<string>> returnValueFunc = () => AsyncMethods
                                                        .WaitASecondAndReturnAValueAsync("this works around it");
            
            //this now executes on a different thread
            //and wont deadlock when calling result
            var theValue = Task
                            .Run(returnValueFunc) 
                            .Result;
            
            Assert.AreEqual("this works around it", theValue);
        }

        [Test]
        public void IGetBetterExceptionInformationWhenUsingGetAwaiterGetResult()
        {

            Assert.Throws<AggregateException>(() =>
            {
                var resultThatWillNeverBe = AsyncMethods
                                              .ThrowAnExceptionAfterNSecondAsync(1)
                                              .Result;
            },"Using result gives you an aggregate exception and this is not as nice as...");

            Assert.Throws<SuperSpecificException>(() =>
            {
                var resultThatWillNeverBe = AsyncMethods
                                             .ThrowAnExceptionAfterNSecondAsync(1)
                                             .GetAwaiter()
                                             .GetResult();
            },"Using get awaiter, and then get result, actually throws the original exception from the awaiter");
            
        }
    }
}
