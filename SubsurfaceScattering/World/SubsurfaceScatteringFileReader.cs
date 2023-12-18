using System.Collections.Generic;

namespace SubsurfaceScattering.World {
    public abstract class SubsurfaceScatteringFileReader {
        public abstract IEnumerable<SubsurfaceScatteringVolume> ReadFile(string fileName);
    }
}
