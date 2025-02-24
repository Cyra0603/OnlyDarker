using OnlyDarker.GameProcess.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess.SpriteClasses.Collectibles
{
    public class StackableDataTable
    {
        private static StackableDataTable _instance;
        private readonly List<IStackable> _data;
        StackableDataTable() //rework to loading data from disk and setting IDs
        {
            _data = [];
            int idcounter = 0;
            _data.Add(new PortalKey(idcounter));
            idcounter++;
            _data.Add(new IronKey(idcounter));
            //CheckForMultipleItemsWithSameID();
        }
        public static StackableDataTable GetInstance()
        {
            if (_instance is null)
            {
                throw new Exception("Instance called before StackableDataTable content was loaded");
            }
            return _instance;
        }
        public static void LoadContent()
        {
            if (_instance is not null)
            {
                throw new Exception("Loading called after StackableDataTable has been already loaded");
            }
            _instance = new StackableDataTable();
        }
        public IStackable GetStackableByID(int id)
        {
            return _data[id];
        }
        public IStackable GetStackableByIngameName(string ingameName)
        {
            return _data.First(s => s.IngameName == ingameName);
        }
    }
}
