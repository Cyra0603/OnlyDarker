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
        public readonly MouseState _defaultState = new();
        public readonly static List<Bind> BindList = new();
        public readonly Bind ToggleDebug;
        public readonly Bind ExitApplication;
        public readonly Bind MoveLeft;
        public readonly Bind MoveRight;
        public readonly Bind MoveUp;
        public readonly Bind MoveDown;
        public readonly Bind Dash;
        public readonly Bind ClickAction;
        public readonly Bind DamageCharacter;
        public readonly Bind HealCharacter;
        public readonly Bind Attack;
        public delegate void KeyPress();
        public BindManager()
        {
            if (_managerInstance is not null)
                throw new Exception("BindManager instance already exists");

            bool canBeHold = true;
            ToggleDebug = new(Keys.F1, !canBeHold);
            ExitApplication = new(Keys.Back, !canBeHold);
            MoveLeft = new(Keys.A, canBeHold);
            MoveRight = new(Keys.D, canBeHold);
            MoveUp = new(Keys.W, canBeHold);
            MoveDown = new(Keys.S, canBeHold);
            Dash = new(Keys.LeftShift, !canBeHold);
            DamageCharacter = new(Keys.F11, !canBeHold);
            HealCharacter = new(Keys.F12, !canBeHold);
            Attack = new(_defaultState.LeftButton, canBeHold);
            _managerInstance = this;
        }
        public void SetControlKey(Bind bind)
        {

        }
        public class Bind
        {
            public Keys Key { get; set; }
            public ButtonState ButtonState { get; set; }
            public bool CanBeHold { get; }
            private bool _lastIskeyDown;
            public bool IsKeyDown
            {
                set
                {
                    if (value == true && CanBeHold || value == true && _lastIskeyDown == false)
                        KeyPressed.Invoke();
                    _lastIskeyDown = value;
                }
            }
            public event KeyPress KeyPressed;
            public Bind(Keys key, bool canBeHold)
            {
                Key = key;
                ButtonState = ButtonState.Released;
                CanBeHold = canBeHold;
                BindList.Add(this);
            }
            public Bind(ButtonState buttonstate, bool canBeHold)
            {
                Key = Keys.None;
                ButtonState = buttonstate;
                CanBeHold = canBeHold;
                BindList.Add(this);
            }
        }
    }
}
