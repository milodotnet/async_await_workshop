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
            var actual = Methods
                            .WaitASecondAndReturnAValue(expected)
                            .Result;
            Assert.AreEqual(expected, actual);
        }     
    }
}
