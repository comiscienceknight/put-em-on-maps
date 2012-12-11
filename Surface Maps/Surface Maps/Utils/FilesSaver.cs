using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace Surface_Maps.Utils
{
    public class FilesSaver<T> where T : class
    {
        public static async Task SaveData(ObservableCollection<T> datas,
                                                     string fileName = "ListPushpin")
        {
            await Helper.CreatEntity(datas, fileName);
        }
    }
}
