using OnlyDarker.IngameMenu;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess
{
    public class BindManager
    {
        private static BindManager _managerInstance = null;
        public readonly MouseState _defaultState = new();
        public readonly List<Bind> BindList;
        public readonly List<Bind> AppHotKeys;
        public readonly Bind ToggleDebug;
        public readonly Bind TogglePause;
        public readonly Bind ExitApplication;
        public readonly Bind MoveLeft;
        public readonly Bind MoveRight;
        public readonly Bind MoveUp;
        public readonly Bind MoveDown;
        public readonly Bind Dash;
        public readonly Bind ClickAction;
        public readonly Bind Interact;
        public readonly Bind DamageCharacter;
        public readonly Bind HealCharacter;
        public readonly Bind Attack;
        public delegate void KeyPress();
        protected BindManager()
        {
            _managerInstance = this;
            BindList = new();
            AppHotKeys = new();
            bool canBeHold = true;
            //Common hotkeys
            {
                AppHotKeys.Add(ToggleDebug = new(Keys.F1, !canBeHold,"toggle ingame debug"));
                AppHotKeys.Add(TogglePause = new(Keys.Escape, !canBeHold,"pause and call menu"));
                AppHotKeys.Add(ExitApplication = new(Keys.Back, !canBeHold, "exit application"));
            }
            //Ingame controls
            {
                BindList.Add(MoveLeft = new(Keys.A, canBeHold, "move left"));
                BindList.Add(MoveRight = new(Keys.D, canBeHold, "move right"));
                BindList.Add(MoveUp = new(Keys.W, canBeHold, "move up"));
                BindList.Add(MoveDown = new(Keys.S, canBeHold, "move down"));
                BindList.Add(Dash = new(Keys.LeftShift, !canBeHold, "dash"));
                BindList.Add(Interact = new(Keys.E, !canBeHold, "interact"));
                BindList.Add(DamageCharacter = new(Keys.F11, !canBeHold, "self harm"));
                BindList.Add(HealCharacter = new(Keys.F12, !canBeHold, "heal"));
                BindList.Add(Attack = new(_defaultState.LeftButton, !canBeHold, "attack!"));
            }
            Debug.WriteLine("Bindmanager initialized");

        }
        public static BindManager GetInstance()
        {
            if (_managerInstance == null)
                return _managerInstance = new BindManager();
            else
                return _managerInstance;
        }
        public class Bind
        {
            public Keys Key { get; set; }
            public ButtonState ButtonState { get; set; }
            public string Description { get; }
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
            public Bind(Keys key, bool canBeHold, string description)
            {
                Key = key;
                ButtonState = ButtonState.Released;
                CanBeHold = canBeHold;
                Description = description;
            }
            public Bind(ButtonState buttonstate, bool canBeHold, string description)
            {
                Key = Keys.None;
                ButtonState = buttonstate;
                CanBeHold = canBeHold;
                Description = description;  
            }
            public async void SetControlKey()
            {
                while (true && GameBody.GetGameInstance().IsActive)
                {
                    var kbstate = Keyboard.GetState();
                    var pressedKeys = kbstate.GetPressedKeys();
                    if (pressedKeys.Length > 0 && KeyIsValid(pressedKeys[0]))
                    {
                        this.Key = pressedKeys[0];
                        Menu.GetInstance().ControlsWindow.UpdateTitles();
                        return;
                    }
                    await Task.Delay(50);
                    if(Menu.GetInstance().WindowsStack.Peek() is not ControlsWindow)
                    {
                        return;
                    }
                }
            }
            private bool KeyIsValid(Keys key)
            {
                foreach (var bind in GetInstance().BindList)
                {
                    if( bind.Key == key) return false;
                }
                return true;
            }
        }
    }
}
