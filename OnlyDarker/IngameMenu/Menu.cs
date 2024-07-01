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
    public static class Menu
    {
        public static Rectangle WindowBounds => new(GlobalUse.WindowSize.X / 4, GlobalUse.WindowSize.Y / 4, GlobalUse.WindowSize.X / 2, GlobalUse.WindowSize.Y / 2);
        public static Stack<IMenuWindow> WindowsStack { get; private set; } = new();
        private readonly static MainWindow _mainWindow = new();
        public readonly static SettingsWindow SettingsWindow = new();
        public readonly static ControlsWindow ControlsWindow = new();
        public delegate void ButtonPress();
        public const int BUTTON_OFFSET = 10;
        public static ButtonState LastMouseState { get; private set; }
        public static void Show()
        {
            WindowsStack.Push(_mainWindow);
        }
        public static void Update()
        {
            if (WindowsStack.Count == 0)
            {
                Debug.WriteLine("WindowsStack is empty");
                return;
            }
            var mstate = Mouse.GetState();
            var kstate = Keyboard.GetState();
            WindowsStack.Peek().Update(in mstate, in kstate);
            LastMouseState = mstate.LeftButton;
        }
        public static void Draw()
        {
            if (WindowsStack.Count == 0)
            {
                return;
            }
            WindowsStack.Peek().Draw();
        }
        public static void BackButtonAction()
        {
            WindowsStack.Pop();
        }
        public static void OpenSettingsWindow()
        {
            WindowsStack.Push(SettingsWindow);
        }
        public static void OpenControlsWindow()
        {
            WindowsStack.Push(ControlsWindow);
        }
    }
    public interface IMenuButton
    {
        Rectangle Bounds { get; }
        Vector2 ButtonCenter { get; }
        string Title { get; }
        string Description { get; }
        int OrderNumber { get; }
        IMenuWindow ParentWindow { get; }
        Menu.ButtonPress ButtonPressed { get; set; }
        bool IsHighlighted { get; set; }
        void Update(in MouseState mstate)
        {
            IsHighlighted = false;
            var cursorRect = new Rectangle(mstate.Position.X, mstate.Position.Y, 1, 1);
            if (Bounds.Intersects(cursorRect))
            {
                IsHighlighted = true;
                if (mstate.LeftButton == ButtonState.Released && Menu.LastMouseState == ButtonState.Pressed)
                {
                    ButtonPressed?.Invoke();
                }
            }
        }
        void Draw()
        {
            GlobalUse.SpriteBatch.DrawString(
                GlobalUse.MainFont,
                Title,
                new(ButtonCenter.X - GlobalUse.MainFont.MeasureString(Title).X / 2, ButtonCenter.Y - GlobalUse.MainFont.MeasureString(Title).Y),
                Color.White);
            if (IsHighlighted)
            {
                GlobalUse.SpriteBatch.Draw(GameBody.EmptyTexture, Bounds, Color.White * 0.2F);
                GameBody.DrawRectangleOutline(Bounds, Color.White, 3);
                GlobalUse.SpriteBatch.DrawString(
                    GlobalUse.MainFont,
                    Title,
                    new(ButtonCenter.X - GlobalUse.MainFont.MeasureString(Title).X / 2, ButtonCenter.Y - GlobalUse.MainFont.MeasureString(Title).Y),
                    Color.Yellow * 0.2F);
            }
            if (IsHighlighted && Menu.LastMouseState == ButtonState.Pressed)
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
                new Vector2(Bounds.Center.X - GlobalUse.MainFont.MeasureString(Title).X / 2, Bounds.Center.Y - Bounds.Top),
                Color.White);
            foreach (var button in Buttons)
            {
                button.Draw();
            }
        }
    }
    public class MainWindow : IMenuWindow
    {
        public Rectangle Bounds => Menu.WindowBounds;

        public string Title { get; }

        public IMenuButton[] Buttons { get; }
        public MainWindow()
        {
            Title = "menu";
            Buttons = new IMenuButton[4];
            Buttons[0] = new MenuButton(this, 1, "new game", String.Empty);

            Buttons[1] = new MenuButton(this, 2, "controls", String.Empty);
            Buttons[1].ButtonPressed += Menu.OpenControlsWindow;
            Buttons[2] = new MenuButton(this, 3, "settings", String.Empty);
            Buttons[2].ButtonPressed += Menu.OpenSettingsWindow;
            Buttons[3] = new MenuButton(this, 4, "resume", String.Empty);
            Buttons[3].ButtonPressed += GameBody.TogglePause;
        }
    }
    public class SettingsWindow : IMenuWindow
    {
        public Rectangle Bounds => Menu.WindowBounds;

        public string Title { get; }

        public IMenuButton[] Buttons { get; }
        public SettingsWindow()
        {
            Title = "settings";
            Buttons = new IMenuButton[4];
            Buttons[0] = new MenuButton(this, 1, $"{GlobalUse.WindowSize.X}x{GlobalUse.WindowSize.Y}", "resolution");

            Buttons[1] = new MenuButton(this, 2, "controls", String.Empty);

            Buttons[2] = new MenuButton(this, 3, "settings", String.Empty);

            Buttons[3] = new MenuButton(this, 4, "back", String.Empty);
            Buttons[3].ButtonPressed += Menu.BackButtonAction;
        }
    }
    public class ControlsWindow : IMenuWindow
    {
        public Rectangle Bounds => Menu.WindowBounds;

        public string Title { get; }

        public IMenuButton[] Buttons { get; }
        public ControlsWindow()
        {
            Title = "controls";
            Buttons = new IMenuButton[BindManager.BindList.Count + 1];
            for(int i = 0; i < BindManager.BindList.Count; i++)
            {
                Buttons[i] = new MenuButton(this, i, BindManager.BindList[i].Key.ToString(), BindManager.BindList[i].Description + ":");
                Buttons[i].ButtonPressed += BindManager.BindList[i].SetControlKey;
            }
            Buttons[^1] = new MenuButton(this, Buttons.Length - 1, "back", String.Empty);
            Buttons[^1].ButtonPressed += Menu.BackButtonAction;
        }
    }
    public class MenuButton : IMenuButton
    {
        public IMenuWindow ParentWindow { get; }
        public Rectangle Bounds => new(
            ParentWindow.Bounds.Location.X + ParentWindow.Bounds.Width / 4,
            ParentWindow.Bounds.Location.Y + OrderNumber * ParentWindow.Bounds.Height / (int)(ParentWindow.Buttons.Length * 1.5F),
            ParentWindow.Bounds.Width / 2,
            ParentWindow.Bounds.Height / (int)(ParentWindow.Buttons.Length * 1.5F) - Menu.BUTTON_OFFSET);
        public Vector2 ButtonCenter => Bounds.Center.ToVector2();
        public string Title { get; }
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
