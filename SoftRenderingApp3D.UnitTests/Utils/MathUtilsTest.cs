using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SoftRenderingApp3D.UnitTests.Utils {
    [TestClass]
    public class MathUtilsTest {
        [TestMethod]
        public void LerpAmountCorrectTest() {
            var start = 0;
            var end = 10;
            var amount = 5;

            var expectedResult = 5.0f;

            Assert.ThrowsException<ArgumentException>(() => MathUtils.Lerp(start, end, amount));
        }


    }
}
