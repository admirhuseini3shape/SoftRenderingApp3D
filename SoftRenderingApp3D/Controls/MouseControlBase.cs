using System;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;

namespace SoftRenderingApp3D.Controls
{
    public abstract class MouseControlBase
    {
        protected Control Control;

        public EventHandler<Point> MouseClicked;
        protected MouseControlBase(Control control)
        {
            Control = control;
            Control.MouseDown += Control_MouseDown;
            Control.MouseMove += Control_MouseMove;
            Control.MouseUp += Control_MouseUp;
            Control.MouseWheel += Control_MouseWheel;
        }

        protected virtual void Control_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                OnMouseClicked(new Point(e.X, e.Y));
            }
        }

        protected virtual void Control_MouseMove(object sender, MouseEventArgs e) { }

        protected virtual void Control_MouseUp(object sender, MouseEventArgs e) { }

        protected virtual void Control_MouseWheel(object sender, MouseEventArgs e) { }

        protected virtual void OnMouseClicked(Point point) { }
    }
}
