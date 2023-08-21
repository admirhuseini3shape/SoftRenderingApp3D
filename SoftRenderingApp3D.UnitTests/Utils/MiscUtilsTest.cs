using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftRenderingApp3D.UnitTests.Utils {
    [TestClass]
    public class MiscUtilsTest {
        [TestMethod]
        public void SwapFloat() {
            var a = 1.0f;
            var aOriginal = a;
            var b = 2.0f;
            var bOriginal = b;

            MiscUtils.Swap(ref a, ref b);

            Assert.AreEqual(aOriginal, b);
            Assert.AreEqual(bOriginal, a);
        }
        [TestMethod]
        public void SwapInt() {
            var a = 1;
            var aOriginal = a;
            var b = 2;
            var bOriginal = b;

            MiscUtils.Swap(ref a, ref b);

            Assert.AreEqual(aOriginal, b);
            Assert.AreEqual(bOriginal, a);
        }
    }
}
