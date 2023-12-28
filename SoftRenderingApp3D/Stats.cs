using System.Diagnostics;

namespace SoftRenderingApp3D {
    public class Stats {
        private readonly Stopwatch calcSw = new Stopwatch();
        private readonly Stopwatch paintSw = new Stopwatch();
        public int BehindViewTriangleCount;
        public int BehindZPixelCount;

        public int DrawnPixelCount;
        public int DrawnTriangleCount;
        public int FacingBackTriangleCount;
        public int OutOfViewTriangleCount;
        public int TotalTriangleCount;

        public long CalculationTimeMs {
            get {
                return calcSw.ElapsedMilliseconds;
            }
        }

        public long PainterTimeMs {
            get {
                return paintSw.ElapsedMilliseconds;
            }
        }

        public void PaintTime() {
            calcSw.Stop();
            paintSw.Start();
        }

        public void CalcTime() {
            paintSw.Stop();
            calcSw.Start();
        }

        public void StopTime() {
            paintSw.Stop();
            calcSw.Stop();
        }

        public void Clear() {
            paintSw.Reset();
            calcSw.Reset();
            TotalTriangleCount = 0;
            DrawnTriangleCount = 0;
            FacingBackTriangleCount = 0;
            OutOfViewTriangleCount = 0;
            BehindViewTriangleCount = 0;
            DrawnPixelCount = 0;
            BehindZPixelCount = 0;
        }
    }
}