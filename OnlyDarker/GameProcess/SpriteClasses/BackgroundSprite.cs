using OnlyDarker.CommonUsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess.SpriteClasses
{
    public class BackgroundSprite
    {
        private readonly Texture2D _texture;
        public Point Size { get; private set; }
        public Rectangle Bounds { get; private set; }
        public Vector2 Origin { get; private set; }
        public BackgroundSprite(Floor floor)
        {
            _texture = GlobalUse.Content.Load<Texture2D>($"Floor/{floor}/Floor{floor}Background");
            Size = new Point(_texture.Width * 10, _texture.Height * 10);
            Bounds = new(Point.Zero, Size);
            Origin = new Vector2(_texture.Width, _texture.Height);
        }
        public void Draw()
        {
            GlobalUse.SpriteBatch.Draw(_texture, Bounds, Bounds, Color.White, 0F, Origin,SpriteEffects.None,1F);
        }
    }
}
