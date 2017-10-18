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
            var expected = "foo";
            var actual = Methods.WaitASecondAndReturnAValue(expected);
            Assert.AreEqual(expected, actual.Result);
        }     
    }
}
