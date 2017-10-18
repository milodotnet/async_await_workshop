using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace AsyncStuff
{
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
         
        
    }
}
