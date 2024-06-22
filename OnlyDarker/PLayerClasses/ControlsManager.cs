
using OnlyDarker.CommonUsing;
using OnlyDarker.GameProcess;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker
{
    public static class ControlsManager
    {
        private static BindManager _bindManager;
        private static Vector2 _direction;
        public static Vector2 ForceSum { get; set; }
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
        private static readonly float _friction = 0.88F;
        public static bool InputsBlocked { get; private set; } = true;
        public static bool Paralyzed { get; private set; } = false;
        static ControlsManager()
        {
            _bindManager = GameBody.BindManager;
            _bindManager.MoveUp.KeyPressed += PlayerMoveUp;
            _bindManager.MoveDown.KeyPressed += PlayerMoveDown;
            _bindManager.MoveLeft.KeyPressed += PlayerMoveLeft;
            _bindManager.MoveRight.KeyPressed += PlayerMoveRight;
            _bindManager.HealCharacter.KeyPressed += GameBody.MainCharacter.TestHealing;
            _bindManager.DamageCharacter.KeyPressed += GameBody.MainCharacter.TestTakingDamage;
            _bindManager.Dash.KeyPressed += GameBody.MainCharacter.Dash;
            _bindManager.Attack.KeyPressed += GameBody.MainCharacter.Attack;
        }

        public static void UpdatePlayerControls(float elapsedMilliseconds)
        {
            ////NegateCloseToZeroValues();
            if (!InputsBlocked)
            {
                var keyboardState = Keyboard.GetState();
                foreach (var bind in BindManager.BindList)
                {
                    bind.IsKeyDown = keyboardState.IsKeyDown(bind.Key);
                    if (bind.Key == Keys.None)
                    {
                        bind.IsKeyDown = ButtonStateToBool(Mouse.GetState().LeftButton);
                    }
                }
            }
            ClampDirectionVector();
            _direction += ForceSum;
            AddFriction();
            //NormalizeDirectionVector();

            ForceSum = Vector2.Zero;
        }

        private static void NegateCloseToZeroValues()
        {
            if (Math.Abs(_direction.Y) < 0.01F)
                _direction.Y = 0;
            if (Math.Abs(_direction.X) < 0.01F)
                _direction.X = 0;
        }
        private static bool ButtonStateToBool(ButtonState buttonState)
        {
            if (buttonState == ButtonState.Pressed)
                return true;
            else 
                return false;
        }
        private static void ClampDirectionVector()
        {
            _direction = Vector2.Clamp(_direction, new(-DIRECTION_X_MAX_VALUE, -DIRECTION_Y_MAX_VALUE), new(DIRECTION_X_MAX_VALUE, DIRECTION_Y_MAX_VALUE));
        }
        private static void NormalizeDirectionVector()
        {
            if (_direction != Vector2.Zero)
                _direction = Vector2.Normalize(_direction);
            
        }
        private static void PlayerMoveUp() => _direction.Y--;
        private static void PlayerMoveDown() => _direction.Y++;
        private static void PlayerMoveLeft() => _direction.X--;
        private static void PlayerMoveRight() => _direction.X++;
        public static void AddFriction()
        {
            _direction.Y *= _friction;
            _direction.X *= _friction;
        }

        public static void CharacterParalyze(int milliseconds) //rework to ingame time
        {
            //InputsBlocked = Paralyzed = true;
            //_direction.X = _direction.Y = 0;
            //await Task.Delay(milliseconds);
            //InputsBlocked = Paralyzed = false;
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
            return _direction;
        }
        public static Vector2 GetMaxDirectionVector()
        {
            return new Vector2(DIRECTION_X_MAX_VALUE, DIRECTION_X_MAX_VALUE);
        }
        public static void ZeroDirectionY()
        {
            _direction.Y = 0;
        }
        public static void ZeroDirectionX()
        {
            _direction.X = 0;
        }
    }
}
