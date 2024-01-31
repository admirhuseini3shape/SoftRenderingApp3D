using g3;
using SoftRenderingApp3D.Buffer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SoftRenderingApp3D.Controls
{
    public class PickingControl : MouseControlBase
    {
        private readonly FrameBuffer frameBuffer;
        private List<int> selectedFacets = new List<int>();
        public IReadOnlyList<int> SelectedFacets => selectedFacets;

        public PickingControl(Control control, FrameBuffer frameBuffer, List<int>? selectedFacets = null) : base(control)
        {
            this.frameBuffer = frameBuffer;
            if(selectedFacets != null)
                this.selectedFacets.AddRange(selectedFacets);
        }

        protected override void OnMouseClicked(Point point)
        {
            
            Console.WriteLine("PickingControl: OnMouseClicked");

            // This normalizes coordinates relative to frame buffer size
            var normalizedX = (int)((point.X / (float)Control.Width) * frameBuffer.Width);
            var normalizedY = (int)((point.Y / (float)Control.Height) * frameBuffer.Height);

            // Ensure coordinates are within bounds
            normalizedX = MathUtil.Clamp(normalizedX, 0, frameBuffer.Width - 1);
            normalizedY = MathUtil.Clamp(normalizedY, 0, frameBuffer.Height - 1);

            var facetId = frameBuffer.GetFacetIdForPixel(normalizedX, normalizedY);
            
            Console.WriteLine(@"PickingControl: OnMouseClicked: facetId = {0}", facetId);

    
            if (facetId != -1)
                selectedFacets.Add(facetId);
        }
    }
}
