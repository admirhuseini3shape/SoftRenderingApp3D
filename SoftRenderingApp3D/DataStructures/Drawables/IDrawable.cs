using SoftRenderingApp3D.DataStructures.Materials;
using SoftRenderingApp3D.DataStructures.Meshes;

namespace SoftRenderingApp3D.DataStructures.Drawables
{
    public interface IDrawable : IDrawable<IMaterial>
    { }

    public interface IDrawable<out T>
    where T : IMaterial
    {
        IMesh Mesh { get; }

        T Material { get; }

        //IBufferManager BufferManager;
    }
}
