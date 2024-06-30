using OnlyDarker.CommonUsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.Menu
{
    public static class Menu
    {
        public static Rectangle WindowBounds => new(GlobalUse.WindowSize.X / 4, GlobalUse.WindowSize.Y / 4, GlobalUse.WindowSize.X / 2, GlobalUse.WindowSize.Y / 2);
        public static Stack<IMenuWindow> WindowsStack { get; private set; } = new();
        public static void Update()
        {

        }
        public static void Draw()
        {
            GlobalUse.SpriteBatch.Draw(GameBody.EmptyTexture, WindowBounds, Color.Black);
        }
    }
    public interface IMenuButton
    {
        Rectangle Bounds { get; }
        string Title { get; }
        void ClickAction();
    }
    public interface IMenuWindow
    {
        Rectangle Bounds { get; }
        string Title { get; }
        IMenuButton[] Buttons { get; }
        void Update();
        void Draw();
    }
    public class MainWindow : IMenuWindow
    {
        public Rectangle Bounds => Menu.WindowBounds;

        public string Title { get; }

        public IMenuButton[] Buttons { get; }
        public MainWindow()
        {
            Title = "Menu";
            Buttons = new IMenuButton[4];
        }

        public void Draw()
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
            throw new NotImplementedException();
        }
    }
    public class MainWindowNewGameButton : IMenuButton
    {
        public Rectangle Bounds { get; }

        public string Title {get;}
        public MainWindowNewGameButton()
        {
            Bounds = new Rectangle();
        }
        public void ClickAction()
        {
            throw new NotImplementedException();
        }
    }
}
