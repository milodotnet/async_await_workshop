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
                SyncMethods.ThrowAnExceptionAfterOneSecond();
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
                await SyncMethods.ThrowAnExceptionAfterOneSecond();
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
            Action willThrow = async () => await SyncMethods.ThrowAnExceptionAfterOneSecond();
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
            Func<Task> willThrow = async () => await SyncMethods.ThrowAnExceptionAfterOneSecond();
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
            var errorProneTask = SyncMethods.ThrowAnExceptionAfterOneSecond();

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
                .ThrowAnExceptionAfterOneSecond()
                .ContinueWith(task => exceptionThrown = task.Exception);

            var firstTaskToComplete = await Task.WhenAny(delay, errorProneTask);
    
            Assert.AreEqual(delay, firstTaskToComplete);

            //give the error prone task a chance to fail.
            await Task.Delay(1);

            Assert.IsNotNull(exceptionThrown);
        }
    }
}
