using SoftRenderingApp3D.Buffer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SoftRenderingApp3D.Controls
{
    public class PickingControl : MouseControlBase
    {
        public IReadOnlyList<int> SelectedFacets => selectedFacets;

        public PickingControl(Control control, FrameBuffer frameBuffer, List<int>? selectedFacets = null) : base(control)
        {
            this.frameBuffer = frameBuffer;
            if(selectedFacets != null)
                this.selectedFacets.AddRange(selectedFacets);
        }
        
        private List<int> selectedFacets = new List<int>();
        
        private FrameBuffer frameBuffer;

        private Func<Point, int> GetFacetIdForPixel;

        public override void OnMouseClicked(Point point)
        {
            var facetId = frameBuffer.GetFacetIdForPixel(point.X, point.Y);
            
            if(facetId != -1)
                selectedFacets.Add(facetId);
        }
    }
}
