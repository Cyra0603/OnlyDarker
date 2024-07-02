
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
    public class ControlsManager
    {
        public BindManager BindManager { get; }
        private Vector2 _direction;
        public Vector2 ForceSum { get; set; }
        public Vector2 MousePosition
        {
            get
            {
                return
                    Mouse.GetState().Position.ToVector2()
                    + GameBody.GetGameInstance().MainCharacter.Position -
                    new Vector2(GlobalUse.WindowSize.X / 2, GlobalUse.WindowSize.Y / 2);
            }
        }
        public const float DIRECTION_X_MAX_VALUE = 1.75F;
        public const float DIRECTION_Y_MAX_VALUE = 1.15F;
        private readonly float _friction = 0.88F;
        public bool InputsBlocked { get; private set; } = true;
        public bool Paralyzed { get; private set; } = false;
        public ControlsManager(BindManager bindManager)
        {
            BindManager = bindManager;
            BindManager.MoveUp.KeyPressed += PlayerMoveUp;
            BindManager.MoveDown.KeyPressed += PlayerMoveDown;
            BindManager.MoveLeft.KeyPressed += PlayerMoveLeft;
            BindManager.MoveRight.KeyPressed += PlayerMoveRight;
            BindManager.Interact.KeyPressed += GameBody.GetGameInstance().MainCharacter.Interact;
            BindManager.HealCharacter.KeyPressed += GameBody.GetGameInstance().MainCharacter.TestHealing;
            BindManager.DamageCharacter.KeyPressed += GameBody.GetGameInstance().MainCharacter.TestTakingDamage;
            BindManager.Dash.KeyPressed += GameBody.GetGameInstance().MainCharacter.Dash;
            BindManager.Attack.KeyPressed += GameBody.GetGameInstance().MainCharacter.Attack;
        }
        public void UpdateInputs()
        {
            var keyboardState = Keyboard.GetState();
            foreach (var bind in BindManager.AppHotKeys)
            {
                bind.IsKeyDown = keyboardState.IsKeyDown(bind.Key);
                if (bind.Key == Keys.None)
                {
                    bind.IsKeyDown = ButtonStateToBool(Mouse.GetState().LeftButton);
                }
            }
        }
        public void UpdatePlayerControls()
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
        public void UpdatePlayerMovement(float elapsedMilliseconds)
        {

            ClampDirectionVector();
            _direction += ForceSum;
            AddFriction();
            ForceSum = Vector2.Zero;
        }

        private void NegateCloseToZeroValues()
        {
            if (Math.Abs(_direction.Y) < 0.01F)
                _direction.Y = 0;
            if (Math.Abs(_direction.X) < 0.01F)
                _direction.X = 0;
        }
        private bool ButtonStateToBool(ButtonState buttonState)
        {
            return buttonState == ButtonState.Pressed;
        }
        private void ClampDirectionVector()
        {
            _direction = Vector2.Clamp(_direction, new(-DIRECTION_X_MAX_VALUE, -DIRECTION_Y_MAX_VALUE), new(DIRECTION_X_MAX_VALUE, DIRECTION_Y_MAX_VALUE));
        }
        private void NormalizeDirectionVector()
        {
            if (_direction != Vector2.Zero)
                _direction = Vector2.Normalize(_direction);

        }
        private void PlayerMoveUp() => _direction.Y -= 0.33F;
        private void PlayerMoveDown() => _direction.Y += 0.33F;
        private void PlayerMoveLeft() => _direction.X -= 0.33F;
        private void PlayerMoveRight() => _direction.X += 0.33F;
        public void AddFriction()
        {
            _direction.Y *= _friction;
            _direction.X *= _friction;
        }

        public void CharacterParalyze(int milliseconds) //rework to ingame time
        {
            //InputsBlocked = Paralyzed = true;
            //_direction.X = _direction.Y = 0;
            //await Task.Delay(milliseconds);
            //InputsBlocked = Paralyzed = false;
        }
        public void ToggleDisableInputs()
        {
            InputsBlocked = !InputsBlocked;
        }
        public void CharacterInputsDisabled(bool isDisabled)
        {
            InputsBlocked = isDisabled;
        }
        public void CharacterInputsEnabled(bool isEnabled)
        {
            InputsBlocked = isEnabled;
        }
        public Vector2 GetDirection()
        {
            return _direction;
        }
        public Vector2 GetMaxDirectionVector()
        {
            return new Vector2(DIRECTION_X_MAX_VALUE, DIRECTION_Y_MAX_VALUE);
        }
        public void ZeroDirectionY()
        {
            _direction.Y = 0;
        }
        public void ZeroDirectionX()
        {
            _direction.X = 0;
        }
    }
}
