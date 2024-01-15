using SoftRenderingApp3D.App.DataStructures;
using SoftRenderingApp3D.App.Utils;
using SoftRenderingApp3D.DataStructures.Drawables;
using System.Collections.Generic;
using System.Linq;

namespace SoftRenderingApp3D.App.DisplayModels
{
    public class DisplayModelsManager
    {
        private readonly List<DisplayModelData> displayModelsData;
        private readonly Dictionary<string, IDrawable> loadedDrawables;
        private readonly List<DisplayModelName> displayModelNames;
        public IReadOnlyList<DisplayModelName> DisplayModelNames => displayModelNames;

        public DisplayModelsManager(string jSonFileName)
        {
            displayModelsData = JsonHelpers.DisplayModelsFromJsonFile(jSonFileName);
            displayModelNames = displayModelsData
                .Select(x => new DisplayModelName(x.Id, x.DisplayName))
                .ToList();
            loadedDrawables = new Dictionary<string, IDrawable>(displayModelsData.Count);
        }

        public bool TryGetDrawable(string id, out IDrawable drawable)
        {
            drawable = null;
            if(loadedDrawables.TryGetValue(id, out drawable))
                return true;

            if(!TryGetDisplayModel(id, out var displayModel))
                return false;

            drawable = DisplayModelHelpers.GetDrawable(displayModel);
            loadedDrawables.Add(id, drawable);
            return true;
        }
        
        public bool TryGetDisplayModel(string id, out DisplayModelData displayModel)
        {
            displayModel = null;
            var i = displayModelsData.FindIndex(x => x.Id == id);

            if(i == -1)
                return false;

            displayModel = displayModelsData[i];
            return true;
        }
    }
}
