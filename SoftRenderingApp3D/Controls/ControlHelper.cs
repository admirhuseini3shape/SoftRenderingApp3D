using System.Drawing;
using System.Numerics;
using System.Windows.Forms;

namespace SoftRenderingApp3D.Controls {
    internal static class ControlHelper {
        public static Vector2 NormalizePointClient(this Control control, Point position) {
            return new Vector2(position.X * (2f / control.Width) - 1.0f, position.Y * (2f / control.Height) - 1.0f);
        }

        public static void getMouseButtons(out bool left, out bool right) {
            left = Control.MouseButtons.HasFlag(MouseButtons.Left);
            right = Control.MouseButtons.HasFlag(MouseButtons.Right);
        }
    }
}