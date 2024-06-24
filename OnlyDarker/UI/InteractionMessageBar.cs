using OnlyDarker.CommonUsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.UI
{
    public class InteractionMessageBar
    {
        private static InteractionMessageBar _instance = null;
        private Vector2 _position => new(GlobalUse.WindowSize.X / 2 - GlobalUse.MainFont.MeasureString(_currentMessage).X * SCALE / 2, GlobalUse.WindowSize.Y - GlobalUse.MainFont.MeasureString(_currentMessage).Y * SCALE * 3);
        private Vector2 _origin => new(GlobalUse.MainFont.MeasureString(_currentMessage).X / 2, GlobalUse.MainFont.MeasureString(_currentMessage).Y / 2);
        private const float SCALE = 0.6F;
        private static Stack<string> _messageStack;
        private static string _currentMessage
        {
            get
            {
                if (_messageStack.Count == 0)
                    return "";
                else return _messageStack.Peek();
            }
        }
        protected InteractionMessageBar()
        {
            _messageStack = new();
            _instance = this;
        }
        public static InteractionMessageBar GetInstance()
        {
            if (_instance == null)
                return new InteractionMessageBar();
            else return _instance;
        }
        public void Update()
        {
            _messageStack.Clear();
        }
        public void Draw()
        {
            GlobalUse.SpriteBatch.DrawString(GlobalUse.MainFont, _currentMessage, _position, Color.White, 0F, Vector2.Zero, SCALE, SpriteEffects.None, 1F);
        }
        public void PushMessage(string message)
        {
            _messageStack.Push(message);
        }
    }
}
