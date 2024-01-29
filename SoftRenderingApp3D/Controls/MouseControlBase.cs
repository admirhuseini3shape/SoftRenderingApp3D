using System;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;

namespace SoftRenderingApp3D.Controls
{
    public abstract class MouseControlBase
    {
        protected Control Control;

        protected MouseControlBase(Control control)
        {
            Control = control;
            Control.MouseDown += Control_MouseDown;
            Control.MouseMove += Control_MouseMove;
            Control.MouseUp += Control_MouseUp;
            Control.MouseWheel += Control_MouseWheel;
        }

        protected abstract void Control_MouseDown(object sender, MouseEventArgs e);
        protected abstract void Control_MouseMove(object sender, MouseEventArgs e);
        protected abstract void Control_MouseUp(object sender, MouseEventArgs e);
        protected abstract void Control_MouseWheel(object sender, MouseEventArgs e);

    }

}
