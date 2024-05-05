using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace OnlyDarker.CommonUsing
{
    public enum Direction
    {
        Left = 1,
        Right,
        Up,
        Down
    }
    public enum Floor
    {
        One = 1,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Eleven,
        Twelve,
        Thirteen
    }
    public enum RoomType
    {
        Entry,
        Treasure,
        Battle,
        Secret,
        Puzzle,
        Boss,
    }
    public static class GlobalUse
    {
        public static float Time { get; set; }
        public static ContentManager Content { get; set; }
        public static SpriteBatch SpriteBatch { get; set; }
        public static Point WindowSize { get; set; }
        public static void Update(GameTime gameTime)
        {
            Time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
    }

    public class Sprite
    {
        private readonly Texture2D _texture;
        public Vector2 Position { get; protected set; }
        public Vector2 Center { get; protected set; }
        public Sprite(Texture2D texture, Vector2 position)
        {
            _texture = texture;
            Position = position;
            Center = new(_texture.Width * 0.5F, texture.Height * 0.5F);
        }
        public void Draw()
        {
            GlobalUse.SpriteBatch.Draw(_texture, Position, null, Color.White, 0F, Center, 1F, SpriteEffects.None, 0F);
        }
    }

    public static class InputManager
    {
        private static Vector2 _direction;
        public static Vector2 Direction => _direction;
        public static void Update()
        {
            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.W))
            {
                _direction.Y--;
            }
            if (keyboardState.IsKeyDown(Keys.A))
            {
                _direction.X--;
            }
            if (keyboardState.IsKeyDown(Keys.S))
            {
                _direction.Y++;
            }
            if (keyboardState.IsKeyDown(Keys.D))
            {
                _direction.X++;
            }

            if (_direction != Vector2.Zero)
            {
                Vector2.Normalize(_direction);
            }
        }
    }

}
