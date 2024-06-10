using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess
{
    public class BindsManager
    {
        private static BindsManager _managerInstance = null;

        public static Bind ExitApplication;
        public static Bind MoveLeft;
        public static Bind MoveRight;
        public static Bind MoveUp;
        public static Bind MoveDown;
        public BindsManager()
        {
            if (_managerInstance is not null)
            {
                throw new Exception("BindsManager instance already exists");
            }
            ExitApplication = new(Keys.F12);
            MoveLeft = new(Keys.A);
            MoveRight = new(Keys.D);
            MoveUp = new(Keys.W);
            MoveDown = new(Keys.S);
            _managerInstance = this;
        }
        public void SetControlKey(Bind controlKey)
        {
            
        }
        public class Bind
        {
            public Keys Key { get; set; }
            public Bind(Keys key)
            {
                Key = key;
            }
        }
    }
}
