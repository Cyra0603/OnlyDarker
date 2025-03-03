using OnlyDarker.CommonUsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.UI
{
    public class LoadingScreen
    {
        public Texture2D Background;
        public const string LOADING_STRING = "LOADING...";
        public Rectangle LoadingBar;
        public Vector2 StringPosition;
        public int TasksCount;
        public int CurrentTask;
        public LoadingScreen(int tasksCount)
        {
            int lbx = GlobalUse.WindowSize.X / 2;
            int lby = GlobalUse.WindowSize.Y / 2;
            int lbw = GlobalUse.WindowSize.X / 4;
            int lbh = GlobalUse.WindowSize.Y / 24;
            Background = GameBody.EmptyTexture;
            LoadingBar = new(lbx - lbw / 2, lby, lbw, lbh);
            StringPosition = LoadingBar.Location.ToVector2();
            TasksCount = tasksCount;
            CurrentTask = 0;
        }
        public void Draw()
        {
            GlobalUse.SpriteBatch.Draw(Background, new Rectangle(0, 0, GlobalUse.WindowSize.X, GlobalUse.WindowSize.Y), Color.Gray);
            GameBody.DrawRectangleOutline(LoadingBar, Color.Black, borderWidth: 2);
            GlobalUse.SpriteBatch.Draw(GameBody.EmptyTexture, new Rectangle(LoadingBar.Location, new(LoadingBar.Width / TasksCount * CurrentTask, LoadingBar.Height)), Color.Yellow);
            GlobalUse.SpriteBatch.DrawString(GlobalUse.MainFont, LOADING_STRING, LoadingBar.Location.ToVector2(), Color.White);
        }
    }
}
