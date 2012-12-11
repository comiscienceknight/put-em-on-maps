using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace Surface_Maps.Utils
{
    public class PageToPageParameterManager
    {
        public static PageToPageParameters parameters;
    }

    public class PageToPageParameters
    {
        public string id;
    }

    public class FromMapToCollectioin : PageToPageParameters
    {
        public DataModel.PushpinDataStructure pushpin { get; set; }
        public DataModel.AlbumDataStructure slectedAlbum { get; set; }

    }

    public class FromVideoAlbumToVideoPlay : PageToPageParameters
    {
        public DataModel.VideoDataStructure video { get; set; }
        public DataModel.AlbumDataStructure album { get; set; }
    }

    public class FromExpositionToDiaporama : PageToPageParameters
    {
        public FromMapToCollectioin navigateParameter;
        public ObservableCollection<Pages.PhotoDataStructure> ListPhoto = new ObservableCollection<Pages.PhotoDataStructure>();
        public string albumName = "";
        public int selectedIndex = 0;
    }
}
