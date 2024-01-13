using SoftRenderingApp3D.DataStructures.Materials;
using SoftRenderingApp3D.DataStructures.Meshes;

namespace SoftRenderingApp3D.DataStructures.Drawables
{
    public class Drawable : IDrawable
    {
        public Drawable():this(new Mesh(), new MaterialBase())
        { }

        public Drawable(IMesh mesh, IMaterial material)
        {
            Mesh = mesh;
            Material = material;
        }

        public IMesh Mesh { get; }
        public IMaterial Material { get; }
    }
}
