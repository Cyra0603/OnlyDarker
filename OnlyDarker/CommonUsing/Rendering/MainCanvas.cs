using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.CommonUsing.Rendering
{
    public class MainCanvas
    {
        private readonly RenderTarget2D _target;
        private readonly GraphicsDevice _graphicsDevice;
        public Rectangle DestinationRectangle { get;  private set; }

        public MainCanvas(GraphicsDevice graphicsDevice, int width, int height)
        {
            _graphicsDevice = graphicsDevice;
            _target = new(_graphicsDevice, width, height);
        }

        public void SetDestinationRectangle()
        {
            var screenSize = _graphicsDevice.PresentationParameters.Bounds;

            float scaleX = (float)screenSize.Width / _target.Width;
            float scaleY = (float)screenSize.Height / _target.Height;
            float scale = Math.Min(scaleX, scaleY);

            int newWidth = (int)(_target.Width * scale);
            int newHeight = (int)(_target.Height * scale);

            int posX = (screenSize.Width - newWidth) / 2;
            int posY = (screenSize.Height - newHeight) / 2;

            DestinationRectangle = new Rectangle(posX, posY, newWidth, newHeight);
        }

        public void Activate()
        {
            _graphicsDevice.SetRenderTarget(_target);
            _graphicsDevice.Clear(Color.DarkGray);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _graphicsDevice.SetRenderTarget(null);
            _graphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();
            spriteBatch.Draw(_target, DestinationRectangle, Color.White);
            spriteBatch.End();
        }
    }
}
