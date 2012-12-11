using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Windows.UI.Xaml.Media.Imaging;
using System.Xml.Serialization;

namespace Surface_Maps.DataModel
{
    public class INotifyPropertyChangedBaseClass : INotifyPropertyChanged
    {
        #region Property Changed
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(String changedPropertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(changedPropertyName));
            }
        }
        #endregion
    }

    public class LifeMapStructure : INotifyPropertyChangedBaseClass
    {
        private string id;
        public string Id
        {
            get { return id; }
            set
            {
                id = value;
                NotifyPropertyChanged("Id");
            }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                NotifyPropertyChanged("Name");
            }
        }

        private string imagePath;
        public string ImagePath
        {
            get { return imagePath; }
            set
            {
                imagePath = value;
                NotifyPropertyChanged("ImagePath");
            }
        }

        private double width = 0;
        public double Width
        {
            get { return width; }
            set
            {
                width = value;
                NotifyPropertyChanged("Width");
            }
        }

        private double height = 0;
        public double Height
        {
            get { return height; }
            set
            {
                height = value;
                NotifyPropertyChanged("Height");
            }
        }

        [XmlIgnore]
        private BitmapImage image;
        [XmlIgnore]
        public BitmapImage Image
        {
            get { return image; }
            set
            {
                image = value;
                NotifyPropertyChanged("Image");
            }
        }
    }

    public enum PushpinTypeEnum
    {
        PhotoVideo,
        File
    }

    public class PushpinDataStructure : INotifyPropertyChangedBaseClass
    {
        private string id;
        public string Id
        {
            get { return id; }
            set
            {
                id = value;
                NotifyPropertyChanged("Id");
            }
        }

        private double latitude;
        public double Latitude
        {
            get { return latitude; }
            set
            {
                latitude = value;
                NotifyPropertyChanged("Latitude");
            }
        }

        private PushpinTypeEnum pushpinType = PushpinTypeEnum.PhotoVideo;
        public PushpinTypeEnum PushpinType
        {
            get { return pushpinType; }
            set
            {
                pushpinType = value;
                NotifyPropertyChanged("PushpinType");
            }
        }

        private string filePath;
        public string FilePath
        {
            get { return filePath; }
            set
            {
                filePath = value;
                NotifyPropertyChanged("FilePath");
            }
        }

        private double longitude;
        public double Longitude
        {
            get { return longitude; }
            set
            {
                longitude = value;
                NotifyPropertyChanged("Longitude");
            }
        }

        private string albumCollectionName = "(Title, Titre, 标题)";
        public string AlbumCollectionName
        {
            get { return albumCollectionName; }
            set
            {
                albumCollectionName = value;
                NotifyPropertyChanged("AlbumCollectionName");
            }
        }

        private string backgroundPhotoPath;
        public string BackgroundPhotoPath
        {
            get { return backgroundPhotoPath; }
            set
            {
                backgroundPhotoPath = value;
                NotifyPropertyChanged("BackgroundPhotoPath");
            }
        }
    }

    public class AlbumDataStructure : INotifyPropertyChangedBaseClass
    {
        private string id;
        public string Id
        {
            get { return id; }
            set
            {
                id = value;
                NotifyPropertyChanged("Id");
            }
        }

        private string pushpinId;
        public string PushpinId
        {
            get { return pushpinId; }
            set
            {
                pushpinId = value;
                NotifyPropertyChanged("PushpinId");
            }
        }

        private double latitude;
        public double Latitude
        {
            get { return latitude; }
            set
            {
                latitude = value;
                NotifyPropertyChanged("Latitude");
            }
        }

        private double longitude;
        public double Longitude
        {
            get { return longitude; }
            set
            {
                longitude = value;
                NotifyPropertyChanged("Longitude");
            }
        }

        private string albumName;
        public string AlbumName
        {
            get { return albumName; }
            set
            {
                albumName = value;
                NotifyPropertyChanged("AlbumName");
            }
        }

        private DateTime date;
        public DateTime Date
        {
            get { return date; }
            set
            {
                date = value;
                NotifyPropertyChanged("Date");
            }
        }

        [XmlIgnore]
        public string AlbumSubCollectionDate
        {
            get
            {
                string month = (Date.Month < 10) ? "0" + Date.Month.ToString() : Date.Month.ToString();
                string day = (Date.Day < 10) ? "0" + Date.Day.ToString() : Date.Day.ToString();
                string s = Date.Year.ToString() + "-" + month + "-" + day;
                return s;
            }
        }

        [XmlIgnore]
        private string childItemsCount;
        public string ChildItemsCount
        {
            get { return childItemsCount; }
            set
            {
                childItemsCount = value;
                NotifyPropertyChanged("ChildItemsCount");
            }
        }

        private string albumType = "Photo";
        public string AlbumType
        {
            get { return albumType; }
            set
            {
                albumType = value;
                NotifyPropertyChanged("AlbumType");
            }
        }

        [XmlIgnore]
        public string IsPlayButtonVisible
        {
            get
            {
                if (AlbumType == "Video")
                    return "Visible";
                else if (AlbumType == "Photo" || AlbumType == "Photos")
                    return "Collapsed";
                return "Collapsed";
            }
        }

        [XmlIgnore]
        private BitmapSource imagePath;
        [XmlIgnore]
        public BitmapSource ImagePath
        {
            get
            {
                return imagePath;
            }
            set
            {
                imagePath = value;
                NotifyPropertyChanged("ImagePath");
            }
        }

        [XmlIgnore]
        public double AlbumHeight
        {
            get
            {
                return Utils.Constants.ScreenHeight - 340;
                //return 400;
            }
        }

        [XmlIgnore]
        public double AlbumWidth
        {
            get
            {
                return Utils.Constants.ScreenWidth / 5;
                //return 400;
            }
        }
    }

    public class ItemDataStructure : INotifyPropertyChangedBaseClass
    {
        private string itemId;
        public string ItemId
        {
            get { return itemId; }
            set
            {
                itemId = value;
                NotifyPropertyChanged("ItemId");
            }
        }

        private double latitude;
        public double Latitude
        {
            get { return latitude; }
            set
            {
                latitude = value;
                NotifyPropertyChanged("Latitude");
            }
        }

        private double longitude;
        public double Longitude
        {
            get { return longitude; }
            set
            {
                longitude = value;
                NotifyPropertyChanged("Longitude");
            }
        }

        private string albumId;
        public string AlbumId
        {
            get { return albumId; }
            set
            {
                albumId = value;
                NotifyPropertyChanged("AlbumId");
            }
        }

        private string comment = "";
        public string Comment
        {
            get { return comment; }
            set
            {
                comment = value;
                NotifyPropertyChanged("Comment");
            }
        }
    }

    public class PhotoDataStructure : ItemDataStructure
    {
        private string path;
        public string ImagePath
        {
            get { return path; }
            set
            {
                path = value;
                NotifyPropertyChanged("ImagePath");
                //NotifyPropertyChanged("Image");
            }
        }
    }

    public class VideoDataStructure : ItemDataStructure
    {
        private string videoPath;
        public string VideoPath
        {
            get { return videoPath; }
            set
            {
                videoPath = value;
                NotifyPropertyChanged("VideoPath");
            }
        }
    }
}
