using SoftRenderingApp3D.DataStructures.Materials;
using SoftRenderingApp3D.DataStructures.Meshes;

namespace SoftRenderingApp3D.DataStructures.Drawables
{
    public interface IDrawable<T>
    where T : IMaterial
    {
        IMesh Mesh { get; }

        T Material { get; }
    }
}
