using OnlyDarker.CommonUsing;
using OnlyDarker.CommonUsing.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess.SpriteClasses
{

    public class ChestSprite : IYSortable, IInteractive, IMyUpdateable
    {
        private Texture2D _spriteSheet;
        private EffectAnimationManager _animator;
        public Vector2 Position { get; set; }
        public ChestType Type { get; }
        public string Name => Type + " chest";
        private Timer _lootDropDelay;
        private float _lootDropDelayValue;
        private int _maxAnimationSteps { get; }
        public bool IsClosed = true;
        public bool IsExpired { get; private set; } = false;
        private Room _parentRoomRef;
        public string InteractionMessage => "to open ";

        public Rectangle MovementCollider => new((int)Position.X - _spriteSheet.Width / _maxAnimationSteps / 2, (int)Position.Y - _spriteSheet.Height / 2, _spriteSheet.Width / _maxAnimationSteps, _spriteSheet.Height);

        public ChestSprite(Vector2 position, Room parentRoomRef, ChestType type = ChestType.Wooden)
        {
            int width = 42;
            int height = 28;
            int animationSteps = _maxAnimationSteps = 7;
            float animFrequency = 42F;
            _lootDropDelayValue = animFrequency * animationSteps;
            Position = position;
            Type = type;
            _spriteSheet = GlobalUse.Content.Load<Texture2D>($"Chests/{type}Chest");
            _animator = new(_spriteSheet, width, height, animationSteps, animFrequency);
            _parentRoomRef = parentRoomRef;
            _parentRoomRef.RoomColliders.Add(MovementCollider);
            _parentRoomRef.Interactives.Add(this);
            _parentRoomRef.Updateables.Add(this);
            _parentRoomRef.ObjectsYSorted.Add(this);
        }

        public void Draw()
        {
            if(IsClosed && _lootDropDelay.TimeLeft == 0) 
                _animator.Draw(Position);
            else if (!IsClosed && _lootDropDelay.TimeLeft <= 0)
            {
                _animator.Step = _maxAnimationSteps - 1;
                _animator.Draw();
            }
            else if(!IsClosed && _lootDropDelay.TimeLeft > 0)
            {
                _animator.Draw();
            }
        }

        public void Interact()
        {
            IsClosed = false;
            _animator.Activate(new(Position));
            _lootDropDelay = new(_lootDropDelayValue);
                DropLoot();
            _parentRoomRef.Interactives.Remove(this);
        }

        public void Update(float elapsedMilliseconds)
        {
            if (IsExpired)
                return;
            if (!IsClosed)
            {
                _lootDropDelay.Update(elapsedMilliseconds);
            }
            if (!IsClosed && _lootDropDelay.TimeLeft <= 0)
            {
                IsExpired = true;
            }
        }
        private void DropLoot()
        {
            var loottablevalue = 5;//TEMP
            var loot = new List<HeartPickupSprite>(loottablevalue);
            for(int i = 0; i < loottablevalue; i++)
            {
                var offsetx = RandomNumberGenerator.GetInt32(-50, 51);
                var offsety = RandomNumberGenerator.GetInt32(-50, 51);
                loot.Add(new HeartPickupSprite(TextureMapper.HeartPickupSpriteTexture, Position, new Vector2(Position.X + offsetx, Position.Y + offsety)));
            }
        }
    }
}
