using OnlyDarker.CommonUsing;
using OnlyDarker.GameProcess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker
{
    public static class ControlsManager
    {
        private static BindManager _bindManager => GameBody.BindsManager;
        private static Vector2 Direction;
        public static Vector2 MousePosition
        {
            get
            {
                return
                    Mouse.GetState().Position.ToVector2()
                    + GameBody.MainCharacter.Position -
                    new Vector2(GlobalUse.WindowSize.X / 2, GlobalUse.WindowSize.Y / 2);
            }
        }
        public const float DIRECTION_X_MAX_VALUE = 7F;
        public const float DIRECTION_Y_MAX_VALUE = 4.667F;
        public readonly static float Friction = 0.88F;
        public static bool InputsBlocked { get; private set; } = true;
        public static bool Paralyzed { get; private set; } = false;
        public static void UpdatePlayerControls()
        {
            if (!InputsBlocked)
            {
                var keyboardState = Keyboard.GetState();
                if (keyboardState.IsKeyDown(_bindManager.MoveUp.Key))
                {
                    Direction.Y--;
                    Direction.Y = MathHelper.Max(Direction.Y, -DIRECTION_Y_MAX_VALUE);
                }
                if (keyboardState.IsKeyDown(_bindManager.MoveDown.Key))
                {
                    Direction.Y++;
                    Direction.Y = MathHelper.Min(Direction.Y, DIRECTION_Y_MAX_VALUE);
                }
                if (keyboardState.IsKeyDown(_bindManager.MoveLeft.Key))
                {
                    Direction.X--;
                    Direction.X = MathHelper.Max(Direction.X, -DIRECTION_X_MAX_VALUE);
                }
                if (keyboardState.IsKeyDown(_bindManager.MoveRight.Key))
                {
                    Direction.X++;
                    Direction.X = MathHelper.Min(Direction.X, DIRECTION_X_MAX_VALUE);
                }
            }
            //
            if (Direction != Vector2.Zero)
            {
                Vector2.Normalize(Direction);
            }
            if (Paralyzed)
            {
                Direction.X = Direction.Y = 0;
            }
            AddFriction();
        }
        public static void AddFriction()
        {
            Direction.Y *= Friction;
            Direction.X *= Friction;
        }

        public static async void CharacterParalyze(int milliseconds)
        {
            InputsBlocked = Paralyzed = true;
            Direction.X = Direction.Y = 0;
            await Task.Delay(milliseconds);
            InputsBlocked = Paralyzed = false;
        }
        public static void ToggleDisableInputs()
        {
            InputsBlocked = !InputsBlocked;
        }
        public static void CharacterInputsDisabled(bool isBlocked)
        {
            InputsBlocked = isBlocked;
        }
        public static Vector2 GetDirection()
        {
            return Direction;
        }
        public static void ZeroDirectionY()
        {
            Direction.Y = 0;
        }
        public static void ZeroDirectionX()
        {
            Direction.X = 0;
        }
    }
}
