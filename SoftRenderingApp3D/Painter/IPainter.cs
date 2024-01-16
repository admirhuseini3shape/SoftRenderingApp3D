using SoftRenderingApp3D.Buffer;
using SoftRenderingApp3D.DataStructures;
using SoftRenderingApp3D.DataStructures.Materials;
using SoftRenderingApp3D.Renderer;
using System.Collections.Generic;
using System.Numerics;

namespace SoftRenderingApp3D.Painter
{
    public interface IPainter
    {
        IReadOnlyList<FacetPixelData> DrawTriangle(VertexBuffer vertexBuffer,
            RendererSettings rendererSettings, IReadOnlyList<Vector3> pixels, int faId);
    }

    public interface IPainter<T>: IPainter
    where T : IMaterial
    {
        T Material { get; }

        IPainter<T> Create(T  material);
    }

    public interface IPainterProvider
    {
        public IPainter GetPainter<T>(T material, RendererSettings rendererSettings)
            where T : IMaterial;
    }
}
