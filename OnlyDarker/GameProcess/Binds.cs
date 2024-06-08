using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess
{
    public class Binds
    {
        public Bind ExitApplication;
        public Bind MoveLeft;
        public Bind MoveRight;
        public Bind MoveUp;
        public Bind MoveDown;
        public Binds()
        {
            ExitApplication = new(Keys.F12);
            MoveLeft = new(Keys.A);
            MoveRight = new(Keys.D);
            MoveUp = new(Keys.W);
            MoveDown = new(Keys.S);
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
