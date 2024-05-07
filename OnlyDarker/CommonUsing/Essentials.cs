using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Reflection.Metadata.Ecma335;

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
        Encounter,
        Secret,
        Puzzle,
        Boss,
    }
    public static class GlobalUse
    {
        public static float Time { get; set; }
        public const float DIMENSION_DRAWING_OFFSET = 0.667F;
        public static ContentManager Content { get; set; }
        public static SpriteBatch SpriteBatch { get; set; }
        public static Point WindowSize { get; set; }
        public static void Update(GameTime gameTime)
        {
            Time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
    }
    public static class InputManager
    {
        private static Vector2 _direction;
        public static Vector2 Direction => _direction;
        private const float DIRECTIONX_MAX_VALUE = 4F;
        private const float DIRECTIONY_MAX_VALUE = 2.6F;
        private const long ONE_TICK = 166667L;
        private readonly static float Friction = 0.75F;
        public static bool InputsBlocked { get; private set; } = true;
        public static bool Paralyzed { get; private set; } = false;
        public static async void UpdateCharacterControls()
        {
            while (GameBody.MainCharacter is not null)
            {
                if (!InputsBlocked)
                {
                    var keyboardState = Keyboard.GetState();

                    if (keyboardState.IsKeyDown(Keys.F1))
                    {
                        CharacterParalyze(1);
                    }
                    if (keyboardState.IsKeyDown(Keys.W))
                    {
                        if (_direction.Y > -DIRECTIONY_MAX_VALUE)
                            _direction.Y--;
                    }
                    if (keyboardState.IsKeyDown(Keys.A))
                    {
                        if (_direction.X > -DIRECTIONX_MAX_VALUE)
                            _direction.X--;
                    }
                    if (keyboardState.IsKeyDown(Keys.S))
                    {
                        if (_direction.Y < DIRECTIONY_MAX_VALUE)
                            _direction.Y++;
                    }
                    if (keyboardState.IsKeyDown(Keys.D))
                    {
                        if (_direction.X < DIRECTIONX_MAX_VALUE)
                            _direction.X++;
                    }

                    //
                    AddFriction(keyboardState);

                }
                if (_direction != Vector2.Zero)
                {
                    Vector2.Normalize(_direction);
                }
                if (Paralyzed)
                {
                    _direction.X = _direction.Y = 0;
                }
                await Task.Delay(TimeSpan.FromTicks(ONE_TICK));
            }
        }

        private static void AddFriction(KeyboardState keyboardState)
        {
            if (!keyboardState.IsKeyDown(Keys.W) && !keyboardState.IsKeyDown(Keys.S))
            {
                _direction.Y *= Friction;
            }
            if (!keyboardState.IsKeyDown(Keys.A) && !keyboardState.IsKeyDown(Keys.D))
            {
                _direction.X *= Friction;
            }
        }

        public static async void CharacterParalyze(int milliseconds)
        {
            InputsBlocked = Paralyzed = true;
            _direction.X = _direction.Y = 0;
            await Task.Delay(milliseconds);
            InputsBlocked = Paralyzed = false;
        }
        public static void ToggleDisableInputs()
        {
            InputsBlocked = !InputsBlocked;
        }
        public static void SetDisableInputs(bool isBlocked)
        {
            InputsBlocked = isBlocked;
        }
    }

}
