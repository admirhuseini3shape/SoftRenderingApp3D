using System;
using System.Drawing;
using System.Windows.Forms;

namespace SoftRenderingApp3D.Controls {
    public partial class SliderIn : Control {
        private readonly float inc = 0.1f;
        private float pixelStep = 1;

        private float value;

        private float xHit;
        private float xValueHit;

        private float yHit;
        private float yValueHit;

        public SliderIn() {
            InitializeComponent();
            DoubleBuffered = true;

            Orientation = Orientation.Horizontal;

            Paint += SuperSlider_Paint;

            MouseDown += SuperSlider_MouseDown;
            MouseMove += SuperSlider_MouseMove;
            MouseWheel += SuperSlider_MouseWheel;

            Layout += SuperSlider_Layout;
        }

        public Orientation Orientation {
            get;
            set;
        }

        public float TickEvery { get; set; } = 10;
        public float NumberEvery { get; set; } = 20;

        public float Value {
            get {
                return value;
            }
            set {
                if(PropertyChangedHelper.ChangeValue(ref this.value, value)) {
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                    Invalidate();
                }
            }
        }

        public float PixelStep {
            get {
                return pixelStep;
            }
            set {
                if(PropertyChangedHelper.ChangeValue(ref pixelStep, value)) {
                    PixelStepChanged?.Invoke(this, EventArgs.Empty);
                    Invalidate();
                }
            }
        }

        public float Min { get; set; } = -180;
        public float Max { get; set; } = 180;
        public event EventHandler ValueChanged;
        public event EventHandler PixelStepChanged;

        private void SuperSlider_Layout(object sender, LayoutEventArgs e) {
            Invalidate();
        }

        private void SuperSlider_MouseWheel(object sender, MouseEventArgs e) {
            var v = PixelStep;
            if(e.Delta > 0) {
                v += inc;
            }
            else if(PixelStep > inc) {
                v -= inc;
            }

            PixelStep = (float)Math.Round(v, 2);

            Invalidate();
        }

        private void SuperSlider_MouseMove(object sender, MouseEventArgs e) {
            if(e.Button == MouseButtons.Left) {
                switch(Orientation) {
                    case Orientation.Horizontal:
                        Value = Math.Min(Max, Math.Max(Min, xValueHit - (e.X - xHit) * PixelStep));
                        break;
                    case Orientation.Vertical:
                        Value = Math.Min(Max, Math.Max(Min, yValueHit - (e.Y - yHit) * PixelStep));
                        break;
                }

                ;
            }
        }

        private void SuperSlider_MouseDown(object sender, MouseEventArgs e) {
            switch(Orientation) {
                case Orientation.Horizontal:
                    xHit = e.X;
                    xValueHit = Value;
                    break;

                case Orientation.Vertical:
                    yHit = e.Y;
                    yValueHit = Value;
                    break;
            }
        }

        private void drawIndexH(float x, Graphics g) {
            g.DrawLine(Pens.Red, new PointF(x - 1, 0), new PointF(x - 1, Height));
            g.DrawLine(Pens.Red, new PointF(x + 1, 0), new PointF(x + 1, Height));
        }

        private void drawIndexV(float y, Graphics g) {
            g.DrawLine(Pens.Red, new PointF(0, y - 1), new PointF(Width, y - 1));
            g.DrawLine(Pens.Red, new PointF(0, y + 1), new PointF(Width, y + 1));
        }

        private float transform(float v) {
            return Orientation switch {
                Orientation.Horizontal => Width / 2f + v / PixelStep - Value / PixelStep,
                Orientation.Vertical => Height / 2f + v / PixelStep - Value / PixelStep,
                _ => throw new Exception()
            };
        }

        private void SuperSlider_Paint(object sender, PaintEventArgs e) {
            var g = e.Graphics;
            using var f = new Font(Font.FontFamily, 7f);

            switch(Orientation) {
                case Orientation.Horizontal:
                    for(var i = Min; i <= Max; i += TickEvery) {
                        var x = transform(i);
                        g.DrawLine(Pens.DarkGray, new PointF(x, 0), new PointF(x, 3));
                    }

                    for(var i = Min; i <= Max; i += NumberEvery) {
                        var label = $"{i}";
                        var x = transform(i);
                        g.DrawString(label, f, Brushes.Gray, x, 12,
                            new StringFormat { Alignment = StringAlignment.Center });
                    }

                    drawIndexH(transform(Value), g);
                    break;

                case Orientation.Vertical:
                    for(var i = Min; i <= Max; i += TickEvery) {
                        var y = transform(i);
                        g.DrawLine(Pens.DarkGray, new PointF(0, y), new PointF(5, y));
                    }


                    for(var i = Min; i <= Max; i += NumberEvery) {
                        var label = $"{i}";
                        var y = transform(i);
                        g.DrawString(label, f, Brushes.Gray, 12, y - 1,
                            new StringFormat {
                                Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center
                            });
                    }

                    drawIndexV(transform(Value), g);
                    break;
            }
        }
    }
}