using OnlyDarker.CommonUsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess.SpriteClasses
{
    public class EntitySprite : IDamageable
    {
        private readonly Texture2D _bodyTexture;
        public Vector2 Position { get; set; }
        private Vector2 _lastAvailablePosition;
        public Vector2 Origin { get; protected set; }
        private SpriteStandartTile _spawnParentTile;
        private Vector2 _minPosition, _maxPosition;
        public Rectangle MovementCollisionAura => new(new Point(MovementCollider.Center.X - _bodyTexture.Width, MovementCollider.Center.Y - _bodyTexture.Height / 4), new(_bodyTexture.Width * 2, _bodyTexture.Height / 2));
        public Rectangle MovementCollider => new(new Point(
            (int)Position.X - _bodyTexture.Width / 2, (int)Position.Y + _bodyTexture.Height / 2 - (int)GlobalUse.PIXEL_OFFSET * 8),
            new(_bodyTexture.Width, (int)GlobalUse.PIXEL_OFFSET * 8)
            );
        public Rectangle BodyHitbox => new(Position.ToPoint(), new(_bodyTexture.Width, _bodyTexture.Height));
        public Armor BaseArmor { get; private set; } = new(ArmorType.Base);
        public List<Armor> ArmorSet { get; set; } = new();
        public float Speed { get; private set; } = 0.5F;
        public bool IsInvincible { get; set; } = false;
        private float _healthPoints = 10;
        public float HealthPoints
        {
            get => _healthPoints;
            set
            {
                var previousValue = _healthPoints;
                _healthPoints = value;
                if (previousValue != _healthPoints)
                {
                    OnChangingHealth?.Invoke(_healthPoints);
                    if (_healthPoints > previousValue)
                    {
                        OnHealing?.Invoke(_healthPoints);
                    }
                    if (_healthPoints < previousValue)
                    {
                        OnTakingDamage?.Invoke(_healthPoints);
                    }
                }
            }
        }

        public EntitySprite(Texture2D bodyTexture, SpriteStandartTile parentTile)
        {
            _bodyTexture = bodyTexture;
            Origin = new(bodyTexture.Width / 2, bodyTexture.Height / 2);
            Position = new(parentTile.Position.X, parentTile.Position.Y - (parentTile.GetTextureWidth() - bodyTexture.Width) / 2);
            _spawnParentTile = parentTile;
        }
        public delegate void ObserveHP(float healthPoints);
        public event ObserveHP OnChangingHealth;
        public event ObserveHP OnTakingDamage;
        public event ObserveHP OnHealing;

        private void Patrol()
        {
            //Patrol code
        }
        public void Draw()
        {
            GlobalUse.SpriteBatch.Draw(_bodyTexture, Position, null, Color.White, 0F, Origin, 1F, SpriteEffects.None, 0.5F);
        }

        public void SetRoomBounds(Point roomSize, Point tileSize)
        {
            _minPosition = new Vector2((-tileSize.X / 2) + Origin.X, (-tileSize.Y / 2) + Origin.Y - _bodyTexture.Height + GlobalUse.PIXEL_OFFSET);
            _maxPosition = new Vector2(roomSize.X - (tileSize.X / 2) - Origin.X, roomSize.Y - (tileSize.Y / 2) - Origin.Y);
        }

        public void Update()
        {
            if (GameBody.SceneManager.CurrentRoom.RoomColliders.Any(collider => collider.Intersects(MovementCollisionAura)))
            {
                var obstacles = GameBody.SceneManager.CurrentRoom.RoomColliders.Where(collider => collider.Intersects(MovementCollisionAura)).ToList();
                for (int i = 0, j = 7/*length of the movement vector*/; i < j; i++)
                {
                    var currentDirection = Vector2.One/*current direction*/;
                    CalculatePossibleCollisions(obstacles, ref currentDirection);
                    Position += Vector2.One/*current direction divided by j*/ * Speed;
                }
            }
            else
            {
                //movement
            }
            Position = Vector2.Clamp(Position, _minPosition, _maxPosition);
        }

        private void CalculatePossibleCollisions(List<Rectangle> obstacles, ref Vector2 currentDirection)
        {
            var ly = currentDirection.Y;
            var lx = currentDirection.X;
            if (currentDirection.Y < 0)
            {
                if (obstacles.Any(collider => collider.Intersects(CalculateMovementCollider(Position + new Vector2(lx, ly - 1) * Speed))))
                {
                    currentDirection.Y = 0;
                }
            }
            if (currentDirection.Y > 0)
            {
                if (obstacles.Any(collider => collider.Intersects(CalculateMovementCollider(Position + new Vector2(lx, ly + 1) * Speed))))
                {
                    currentDirection.Y = 0;
                }
            }
            if (currentDirection.X < 0)
            {
                if (obstacles.Any(collider => collider.Intersects(CalculateMovementCollider(Position + new Vector2(lx - 1, ly) * Speed))))
                {
                    currentDirection.X = 0;
                }
            }
            if (currentDirection.X > 0)
            {
                if (obstacles.Any(collider => collider.Intersects(CalculateMovementCollider(Position + new Vector2(lx + 1, ly) * Speed))))
                {
                    currentDirection.X = 0;
                }
            }
        }
        public Rectangle CalculateMovementCollider(Vector2 position)
        {
            return new(new Point(
            (int)position.X - _bodyTexture.Width / 2,
            (int)position.Y + _bodyTexture.Height / 2 - (int)GlobalUse.PIXEL_OFFSET * 8),
            new(_bodyTexture.Width, (int)GlobalUse.PIXEL_OFFSET * 8)
            );
        }
        public void SetPosition(Vector2 position)
        {
            Position = position;
        }
        //public void TakeDamage(DamageInstance damage)
        //{
        //    var test = Stopwatch.StartNew();
        //    if (!IsInvincible)
        //    {
        //        HealthPoints -= damage.ExtractValue(BaseArmor.Resistances.AsParallel().First(res => res.Type == damage.Type));
        //    }
        //    else return;
        //    test.Stop();
        //    Debug.WriteLine($"taking damage took {test.ElapsedMilliseconds} ms");
        //}
        public void Heal(float healAmount)
        {
            HealthPoints += healAmount;
        }
    }

}
