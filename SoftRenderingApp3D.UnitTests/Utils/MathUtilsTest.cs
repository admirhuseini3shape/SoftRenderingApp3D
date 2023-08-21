using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Numerics;

namespace SoftRenderingApp3D.UnitTests.Utils {
    [TestClass]
    public class MathUtilsTest {

        // Tests for the MathUtils.Lerp function ==================================================
        [TestMethod]
        public void LerpAmountTooLargeTest() {
            var start = 0;
            var end = 10;
            var amount = 5;

            Assert.ThrowsException<ArgumentException>(() => MathUtils.Lerp(start, end, amount));
        }

        [TestMethod]
        public void LerpAmountCorrectTest() {
            var start = 0;
            var end = 1;
            var amount = 0.5f;

            var expectedResult = 0.5f;

            Assert.AreEqual(expectedResult, MathUtils.Lerp(start, end, amount));
        }

        [TestMethod]
        public void LerpAmountNegativeTest() {
            var start = 0;
            var end = 1;
            var amount = -0.5f;

            Assert.ThrowsException<ArgumentException>(() => MathUtils.Lerp(start, end, amount));
        }

        // Tests for the MathUtils.Clamp function =================================================
        [TestMethod]
        public void ClampValueLessThanZero() {
            var value = -5;

            var expectedResult = 0;

            Assert.AreEqual(expectedResult, MathUtils.Clamp(value));
        }

        [TestMethod]
        public void ClampValueLargerThanOne() {
            var value = 5;

            var expectedResult = 1;

            Assert.AreEqual(expectedResult, MathUtils.Clamp(value));
        }

        [TestMethod]
        public void ClampValueBetweenNegativeValues() {
            var min = -2;
            var max = -1;
            var value = -1.5f;

            var expectedResult = -1.5f;

            Assert.AreEqual(expectedResult, MathUtils.Clamp(value, min, max));
        }

        [TestMethod]
        public void ClampValueBetweenPositiveValues() {
            var min = 1;
            var max = 2;
            var value = 1.5f;

            var expectedResult = 1.5f;

            Assert.AreEqual(expectedResult, MathUtils.Clamp(value, min, max));
        }
        [TestMethod]
        public void ClampMinLargerThanMax() {
            var min = 5;
            var max = 2;
            var value = 1.5f;

            Assert.ThrowsException<ArgumentException>(() => MathUtils.Clamp(value, min, max));
        }

        // Tests for the MathUtils.ComputeNDotL function =================================================
        [TestMethod]
        public void ComputeNDotLLightAt0Degrees() {
            var normal = new Vector3(0, 1.0f, 0);
            var vertexCenter = new Vector3(0, 1.0f, 0);
            var lightPosition = new Vector3(0, 2.0f, 0);

            var expectedResult = 1;

            Assert.AreEqual(expectedResult, MathUtils.ComputeNDotL(vertexCenter, normal, lightPosition));
        }
        [TestMethod]
        public void ComputeNDotLLightAt90Degrees() {
            var normal = new Vector3(0, 1.0f, 0);
            var vertexCenter = new Vector3(0, 1.0f, 0);
            var lightPosition = new Vector3(1.0f, 1.0f, 0);

            var expectedResult = 0.0f;

            Assert.AreEqual(expectedResult, MathUtils.ComputeNDotL(vertexCenter, normal, lightPosition));
        }
        [TestMethod]
        public void ComputeNDotLLightBehindSurface() {
            var normal = new Vector3(0, 1.0f, 0);
            var vertexCenter = new Vector3(0, 1.0f, 0);
            var lightPosition = new Vector3(0, -1.0f, 0);

            var expectedResult = 0.0f;

            Assert.AreEqual(expectedResult, MathUtils.ComputeNDotL(vertexCenter, normal, lightPosition));
        }

    }
}
