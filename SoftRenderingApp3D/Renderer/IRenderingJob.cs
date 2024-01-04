using SoftRenderingApp3D.Buffer;
using System.Threading.Tasks;

namespace SoftRenderingApp3D.Renderer
{
    public interface IRenderingJobBase
    {
        int JobPriority { get; set; }
        bool CanExecute();
        void PrepareData();
    }
    public interface IRenderingJob: IRenderingJobBase
    {
        FrameBuffer Execute();
    }

    public interface IRenderingJobAsync:IRenderingJobBase
    {
        bool CanExecuteAsync();
        Task<FrameBuffer> ExecuteAsync();
    }
}
