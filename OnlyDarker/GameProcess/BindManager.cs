using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess
{
    public class BindManager
    {
        private BindManager _managerInstance = null;

        public readonly Bind ExitApplication;
        public readonly Bind MoveLeft;
        public readonly Bind MoveRight;
        public readonly Bind MoveUp;
        public readonly Bind MoveDown;
        public static readonly List<Bind> BindList;
        public BindManager()
        {
            if (_managerInstance is not null)
            {
                throw new Exception("BindManager instance already exists");
            }
            ExitApplication = new(Keys.Back);
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
            public KeyState LastState;
            public KeyState State 
            {
                set
                {
                    if (value is KeyState.Down)
                    {
                        KeyPressed.Invoke(this, EventArgs.Empty);
                    }
                    LastState = value;
                } 
            }
            public EventHandler KeyPressed;
            public Bind(Keys key)
            {
                Key = key;
                BindList.Add(this);
            }
        }
    }
}
