using OnlyDarker.CommonUsing;
using OnlyDarker.GameProcess;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.IngameMenu
{
    public class Menu
    {
        private static Menu _menuInstance;
        public Rectangle WindowBounds => new(GlobalUse.WindowSize.X / 4, GlobalUse.WindowSize.Y / 4, GlobalUse.WindowSize.X / 2, GlobalUse.WindowSize.Y / 2);
        public Stack<IMenuWindow> WindowsStack { get; private set; }
        private readonly MainWindow _mainWindow;
        public readonly SettingsWindow SettingsWindow;
        public readonly ControlsWindow ControlsWindow;
        public delegate void ButtonPress();
        public const int BUTTON_OFFSET = 10;
        private KeyboardState _lastKeyboardState;
        public ButtonState LastMouseState { get; private set; }
        protected Menu()
        {
            _menuInstance = this;
            WindowsStack = new();
            _mainWindow = new();
            SettingsWindow = new();
            ControlsWindow = new(BindManager.GetInstance());
            _lastKeyboardState = Keyboard.GetState();
            BindManager.GetInstance().TogglePause.KeyPressed += BackButtonAction;
        }
        public static Menu GetInstance()
        {
            if (_menuInstance != null)
                return _menuInstance;
            else return new Menu();
        }
        public void Show()
        {
            WindowsStack.Push(_mainWindow);
            WindowsStack.Push(_mainWindow); // Works for BackButtonAction but can easily break
        }
        public void Update()
        {
            if (WindowsStack.Count == 0)
            {
                Debug.WriteLine("WindowsStack is empty");
                GameBody.GetGameInstance().GameUnpause();
                return;
            }
            var mstate = Mouse.GetState();
            var kstate = Keyboard.GetState();
            WindowsStack.Peek().Update(in mstate, in kstate);
            LastMouseState = mstate.LeftButton;
            if (GameConsole.GetInstance().IsActive)
                GameConsole.GetInstance().Update(in kstate);
            if(kstate.IsKeyDown(Keys.OemTilde) && _lastKeyboardState.IsKeyUp(Keys.OemTilde))
            {
                GameConsole.GetInstance().IsActive = true;
            }
        }
        public void Draw()
        {
            if (WindowsStack.Count == 0)
            {
                return;
            }
            WindowsStack.Peek().Draw();
            if (GameConsole.GetInstance().IsActive)
                GameConsole.GetInstance().Draw();
        }
        public void BackButtonAction()
        {
            if(WindowsStack.Count > 0)
                WindowsStack.Pop();
        }
        public void OpenSettingsWindow()
        {
            WindowsStack.Push(SettingsWindow);
        }
        public void OpenControlsWindow()
        {
            WindowsStack.Push(ControlsWindow);
        }
    }
    public interface IMenuButton
    {
        Rectangle Bounds { get; }
        Vector2 ButtonCenter { get; }
        string Title { get; set; }
        string Description { get; }
        int OrderNumber { get; }
        IMenuWindow ParentWindow { get; }
        Menu.ButtonPress ButtonPressed { get; set; }
        bool IsHighlighted { get; set; }
        void Update(in MouseState mstate)
        {
            IsHighlighted = false;
            var cursorRect = new Rectangle(mstate.Position.X, mstate.Position.Y, 1, 1);
            if (GameBody.GetGameInstance().IsActive && Bounds.Intersects(cursorRect))
            {
                IsHighlighted = true;
                if (mstate.LeftButton == ButtonState.Released && Menu.GetInstance().LastMouseState == ButtonState.Pressed)
                {
                    ButtonPressed?.Invoke();
                }
            }
        }
        void Draw()
        {
            float fx = Bounds.Size.Y / GlobalUse.MainFont.MeasureString(Title).Y / 2;
            if (fx > 0.5F)
                fx = 0.5F;
            GlobalUse.SpriteBatch.DrawString(
                GlobalUse.MainFont,
                Title,
                Bounds.Center.ToVector2()/*new(ButtonCenter.X - GlobalUse.MainFont.MeasureString(Title).X / 2, ButtonCenter.Y - GlobalUse.MainFont.MeasureString(Title).Y)*/,
                Color.White,
                0F,
                GlobalUse.MainFont.MeasureString(Title) / 2,
                fx,
                SpriteEffects.None,
                0F);
            if (IsHighlighted)
            {
                GlobalUse.SpriteBatch.Draw(GameBody.EmptyTexture, Bounds, Color.White * 0.2F);
                GameBody.DrawRectangleOutline(Bounds, Color.White, 3);
                GlobalUse.SpriteBatch.DrawString(
                GlobalUse.MainFont,
                Title,
                Bounds.Center.ToVector2()/*new(ButtonCenter.X - GlobalUse.MainFont.MeasureString(Title).X / 2, ButtonCenter.Y - GlobalUse.MainFont.MeasureString(Title).Y)*/,
                Color.Yellow * 0.2F,
                0F,
                GlobalUse.MainFont.MeasureString(Title) / 2,
                fx,
                SpriteEffects.None,
                0F);
            }
            if (IsHighlighted && Menu.GetInstance().LastMouseState == ButtonState.Pressed)
            {
                GlobalUse.SpriteBatch.Draw(GameBody.EmptyTexture, Bounds, Color.White * 0.1F);
            }
        }
    }
    public interface IMenuWindow
    {
        Rectangle Bounds { get; }
        string Title { get; }
        IMenuButton[] Buttons { get; }
        void Update(in MouseState mstate, in KeyboardState kstate)
        {
            foreach (var button in Buttons)
            {
                button.Update(in mstate);
            }
        }
        void Draw()
        {
            GlobalUse.SpriteBatch.DrawString
                (GlobalUse.MainFont,
                Title,
                new Vector2(Bounds.Center.X - GlobalUse.MainFont.MeasureString(Title).X / 2, Bounds.Center.Y - Bounds.Top * 1.5F),
                Color.White
                );
            foreach (var button in Buttons)
            {
                button.Draw();
            }
        }
    }
    public class MainWindow : IMenuWindow
    {
        public Rectangle Bounds => Menu.GetInstance().WindowBounds;

        public string Title { get; }

        public IMenuButton[] Buttons { get; }
        public MainWindow()
        {
            Title = "menu";
            Buttons = new IMenuButton[4];
            Buttons[0] = new MenuButton(this, 1, "new game", String.Empty);
            Buttons[0].ButtonPressed += GameBody.GetGameInstance().Exit;
            Buttons[1] = new MenuButton(this, 2, "controls", String.Empty);
            Buttons[1].ButtonPressed += Menu.GetInstance().OpenControlsWindow;
            Buttons[2] = new MenuButton(this, 3, "settings", String.Empty);
            Buttons[2].ButtonPressed += Menu.GetInstance().OpenSettingsWindow;
            Buttons[3] = new MenuButton(this, 4, "resume", String.Empty);
            Buttons[3].ButtonPressed += GameBody.GetGameInstance().GamePause;
        }
    }
    public class SettingsWindow : IMenuWindow
    {
        public Rectangle Bounds => Menu.GetInstance().WindowBounds;

        public string Title { get; }

        public IMenuButton[] Buttons { get; }
        public SettingsWindow()
        {
            Title = "settings";
            Buttons = new IMenuButton[4];
            Buttons[0] = new MenuButton(this, 1, $"{GlobalUse.WindowSize.X}x{GlobalUse.WindowSize.Y}", "resolution");

            Buttons[1] = new MenuButton(this, 2, "toggle fullscreen", String.Empty);
            Buttons[1].ButtonPressed += GameBody.GetGameInstance().AppToggleFullscreen;
            Buttons[2] = new MenuButton(this, 3, "settings", String.Empty);

            Buttons[3] = new MenuButton(this, 4, "back", String.Empty);
            Buttons[3].ButtonPressed += Menu.GetInstance().BackButtonAction;
        }
    }
    public class ControlsWindow : IMenuWindow
    {
        public Rectangle Bounds => Menu.GetInstance().WindowBounds;
        private BindManager _bindManager;
        public string Title { get; }

        public IMenuButton[] Buttons { get; }
        public ControlsWindow(BindManager bindManager)
        {
            _bindManager = bindManager;
            Title = "controls";
            Buttons = new IMenuButton[_bindManager.BindList.Count + 1];
            for (int i = 0; i < _bindManager.BindList.Count; i++)
            {
                Buttons[i] = new MenuButton(this, i, _bindManager.BindList[i].Key.ToString(), _bindManager.BindList[i].Description);
                Buttons[i].ButtonPressed += _bindManager.BindList[i].SetControlKey;
            }
            Buttons[^1] = new MenuButton(this, Buttons.Length - 1, "back", String.Empty);
            Buttons[^1].ButtonPressed += Menu.GetInstance().BackButtonAction;
        }
        public void UpdateTitles()
        {
            for(int i = 0; i < Buttons.Length - 1; i++)
            {
                Buttons[i].Title = _bindManager.BindList.First(bind => bind.Description == Buttons[i].Description).Key.ToString();
            }
        }
    }
    public class MenuButton : IMenuButton
    {
        public IMenuWindow ParentWindow { get; }
        public Rectangle Bounds => new(
            ParentWindow.Bounds.Location.X + ParentWindow.Bounds.Width / 4,
            ParentWindow.Bounds.Location.Y + (int)GlobalUse.MainFont.MeasureString(ParentWindow.Title).Y + OrderNumber * ParentWindow.Bounds.Height / (int)(ParentWindow.Buttons.Length * 1.5F),
            ParentWindow.Bounds.Width / 2/* / ParentWindow.Buttons.Length*/,
            ParentWindow.Bounds.Height / (int)(ParentWindow.Buttons.Length * 1.5F) - Menu.BUTTON_OFFSET);
        public Vector2 ButtonCenter => Bounds.Center.ToVector2();
        public string Title { get; set; }
        public string Description { get; }
        public int OrderNumber { get; }
        public bool IsHighlighted { get; set; } = false;
        public Menu.ButtonPress ButtonPressed { get; set; }
        public MenuButton(IMenuWindow parentWindow, int orderNumber, string title, string description)
        {
            ParentWindow = parentWindow;
            OrderNumber = orderNumber;
            Title = title;
            Description = description;
        }
    }
}
