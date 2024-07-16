using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OnlyDarker.CommonUsing;
using OnlyDarker.CommonUsing.Rendering;
using OnlyDarker.GameProcess;
using OnlyDarker.GameProcess.SpriteClasses;
using OnlyDarker.PlayerClasses;
using System.Text.Json;
using SharpDX.Direct2D1;
using System.IO;

namespace OnlyDarker
{

    public class Character : IYSortable
    {
        private readonly Texture2D _bodyTexture;
        private readonly Texture2D _handTexture;
        private static ControlsManager ControlsManager => GameBody.GetGameInstance().ControlsManager;
        private List<Vector2> _dashFrames = new();
        public Vector2 Position { get; set; }
        public Vector2 Origin { get; protected set; }
        private Vector2 _handOrigin;
        private Vector2 _minPosition, _maxPosition;
        public Vector2 RightHandPosition => new(Position.X, Position.Y);
        public Vector2 LeftHandPosition { get; private set; }
        private Vector2 _dashForce;
        public Rectangle MovementCollisionAura => new(new Point(MovementCollider.Center.X - _bodyTexture.Width, MovementCollider.Center.Y - _bodyTexture.Height / 4), new(_bodyTexture.Width * 2, _bodyTexture.Height / 2));
        public Rectangle MovementCollider => new(new Point(
            (int)Position.X - _bodyTexture.Width / 2, (int)Position.Y + _bodyTexture.Height / 2 - (int)GlobalUse.PIXEL_OFFSET * 2),
            new(_bodyTexture.Width, (int)GlobalUse.PIXEL_OFFSET * 2)
            );
        public Rectangle InteractionAura => new(new Point((int)Position.X - _bodyTexture.Width, (int)Position.Y - _bodyTexture.Height / 2), new(_bodyTexture.Width * 2, (int)(_bodyTexture.Height * 1.25F)));
        public Rectangle BodyHitbox => new(new((int)(Position.X - _bodyTexture.Width / 2), (int)(Position.Y - _bodyTexture.Height / 2)), new(_bodyTexture.Width, _bodyTexture.Height));
        public Stats Stats {get; private set;}
        public Inventory Inventory { get; private set; }
        public Armor BaseArmor { get; private set; } = new(ArmorType.Base, string.Empty);
        public List<Armor> ArmorSet { get; set; } = new();
        private Stack<IInteractive> _thingsToInteract = new();
        public IWeapon CurrentWeapon = new WeaponFist();
        private Texture2D _attackAnimation = GlobalUse.Content.Load<Texture2D>("Character/AnimationSpriteSheets/CharacterAttackAnimation");
        public ActionTimer? DashTimer;
        public ActionTimer? DashEffectTimer;
        public Timer AttackCooldown = new(0);
        public Timer InvincibilityTimer = new(0);
        public Timer DamagedEffectTimer = new(0);
        private SpriteEffects _flipEffect = SpriteEffects.None;
        public bool IsExpired { get; private set; }
        public bool IsInvincible
        {
            get => InvincibilityTimer.TimeLeft > 0;
            set { }
        }
        public bool IsPushable { get; set; } = false;

        public Character(Texture2D bodyTexture, Texture2D handTexture, SpriteStandartTile parentTile, Stats stats)
        {
            _bodyTexture = bodyTexture;
            _handTexture = handTexture;
            _handOrigin = new(handTexture.Width / 2, handTexture.Height / 2);
            Origin = new(bodyTexture.Width / 2, bodyTexture.Height / 2);
            Position = new(parentTile.Position.X, parentTile.Position.Y - (parentTile.GetTextureWidth() - bodyTexture.Width) / 2);
            Stats = stats;
            Inventory = new(ArmorSet, Stats, CurrentWeapon);
        }
        public void RunIFrames(float durationMilliseconds)
        {
            InvincibilityTimer.TimeLeft = Math.Max(InvincibilityTimer.TimeLeft, durationMilliseconds);
        }
        public void Draw()
        {
            if (DashTimer is not null && DashEffectTimer.IsRunning)
                for (int i = 0; i < _dashFrames.Count; i++)
                {
                    GlobalUse.SpriteBatch.Draw(_bodyTexture, _dashFrames[i], null, Color.White * (0.5F / (_dashFrames.Count - i)), 0F, Origin, 1F, _flipEffect/*SpriteEffects.None*/, 0.5F);
                }
            GlobalUse.SpriteBatch.Draw(_bodyTexture, Position, null, Color.White, 0F, Origin, 1F, _flipEffect, 0.5F);
            if (DamagedEffectTimer.TimeLeft > 0)
                GlobalUse.SpriteBatch.Draw(_bodyTexture, Position, null, Color.Red * (DamagedEffectTimer.TimeLeft / 1000), 0F, Origin, 1F, _flipEffect, 0.5F);
        }
        public void DrawHpBar() { }
        public void SetRoomBounds(Point roomSize, Point tileSize)
        {
            _minPosition = new Vector2((-tileSize.X / 2) + Origin.X, (-tileSize.Y / 2) + Origin.Y - _bodyTexture.Height + GlobalUse.PIXEL_OFFSET);
            _maxPosition = new Vector2(roomSize.X - (tileSize.X / 2) - Origin.X, roomSize.Y - (tileSize.Y / 2) - Origin.Y);
        }
        public void Interact()
        {
            if (_thingsToInteract.Count > 0)
                _thingsToInteract.Peek().Interact();
        }
        public void Update(float elapsedMilliseconds)
        {
            ControlsManager.UpdatePlayerMovement(elapsedMilliseconds);
            _thingsToInteract.Clear();
            DashTimer?.Update(elapsedMilliseconds);
            DashEffectTimer?.Update(elapsedMilliseconds);
            DamagedEffectTimer.Update(elapsedMilliseconds);
            InvincibilityTimer.Update(elapsedMilliseconds);
            AttackCooldown.Update(elapsedMilliseconds);
            Stats.Stamina += elapsedMilliseconds * Stats.StaminaRegenValue;
            if (GlobalUse.IsDebugMode)
            {
                Stats.Stamina = Stats.MaxStamina;
            }
            if (ControlsManager.MousePosition.X < Position.X)
                _flipEffect = SpriteEffects.FlipHorizontally;
            else
                _flipEffect = SpriteEffects.None;
            CheckForCollisions();
            CheckForInteractions();
            Position = Vector2.Clamp(Position, _minPosition, _maxPosition);
        }

        private void CheckForInteractions()
        {
            if (GameBody.GetGameInstance().SceneManager.CurrentRoom.Interactives is not null && GameBody.GetGameInstance().SceneManager.CurrentRoom.Interactives.Any(collider => collider.MovementCollider.Intersects(InteractionAura)))
            {
                GameBody.GetGameInstance().SceneManager.CurrentRoom.Interactives.First(collider => collider.MovementCollider.Intersects(InteractionAura)).ShowInteractionMessage();
                _thingsToInteract.Push(GameBody.GetGameInstance().SceneManager.CurrentRoom.Interactives.First(collider => collider.MovementCollider.Intersects(InteractionAura)));
            }
        }

        private void CheckForCollisions()
        {
            if (GameBody.GetGameInstance().SceneManager.CurrentRoom.RoomColliders.Any(collider => collider.Intersects(MovementCollisionAura)))
            {
                var obstacles = GameBody.GetGameInstance().SceneManager.CurrentRoom.RoomColliders.Where(collider => collider.Intersects(MovementCollisionAura)).ToList();
                for (int i = 0, j = 7; i < j; i++)
                {
                    var currentDirection = ControlsManager.GetDirection();
                    CalculatePossibleCollisions(obstacles, ref currentDirection);
                    Position += ControlsManager.GetDirection() / j * Stats.Speed;
                }
            }
            else
            {
                Position += ControlsManager.GetDirection() * Stats.Speed;
            }
        }

        private void CalculatePossibleCollisions(List<Rectangle> obstacles, ref Vector2 currentDirection)
        {
            var ly = currentDirection.Y;
            var lx = currentDirection.X;
            if (currentDirection.Y < 0 && obstacles.Any(collider => collider.Intersects(CalculateMovementCollider(Position + new Vector2(lx, ly - 1F) * Stats.Speed))))
            {
                ControlsManager.ZeroDirectionY();
                ControlsManager.AddFriction();
            }
            if (currentDirection.Y > 0 && obstacles.Any(collider => collider.Intersects(CalculateMovementCollider(Position + new Vector2(lx, ly + 1F) * Stats.Speed))))
            {
                ControlsManager.ZeroDirectionY();
                ControlsManager.AddFriction();
            }
            if (currentDirection.X < 0 && obstacles.Any(collider => collider.Intersects(CalculateMovementCollider(Position + new Vector2(lx - 1F, ly) * Stats.Speed))))
            {
                ControlsManager.ZeroDirectionX();
                ControlsManager.AddFriction();
            }
            if (currentDirection.X > 0 && obstacles.Any(collider => collider.Intersects(CalculateMovementCollider(Position + new Vector2(lx + 1F, ly) * Stats.Speed))))
            {
                ControlsManager.ZeroDirectionX();
                ControlsManager.AddFriction();
            }
        }
        public Rectangle CalculateMovementCollider(Vector2 position)
        {
            return new(new Point(
            (int)position.X - _bodyTexture.Width / 2, (int)position.Y + _bodyTexture.Height / 2 - (int)GlobalUse.PIXEL_OFFSET * 2),
            new(_bodyTexture.Width, (int)GlobalUse.PIXEL_OFFSET * 2));
        }
        public void SetSpeed(float value)
        {
            Stats.Speed = value;
        }
        public void AddSpeed(float amount)
        {
            Stats.Speed += amount;
            if (Stats.Speed < Stats.MIN_CHARACTER_SPEED) 
                Stats.Speed = Stats.MIN_CHARACTER_SPEED;
            if (Stats.Speed > Stats.MAX_CHARACTER_SPEED) 
                Stats.Speed = Stats.MAX_CHARACTER_SPEED;
        }
        public void SetPosition(Vector2 position)
        {
            var newPos = position;
            newPos.Y += Position.Y - MovementCollider.Location.Y;
            Position = newPos;
        }
        public void SetRange(float value)
        {
            
        }
        public void SetStamina(float value)
        {
            Stats.Stamina = Stats.MaxStamina = value;
        }
        public void SetCritChance(float value)
        {
            Stats.CritChance = value;
        }
        public void SetCritDamage(float value)
        {
            Stats.CritDamage = value;
        }
        public void ConsoleSetPosition(string args)
        {
            string[] lArgs = args.Split(',');
            if (lArgs.Length > 2)
                throw new ArgumentException();
            var newPos = new Vector2(float.Parse(lArgs[0]), float.Parse(lArgs[1]));
            newPos.Y += Position.Y - MovementCollider.Location.Y;
            Position = newPos;
        }
        public void Dash()
        {
            if (DashTimer is not null && DashEffectTimer.IsRunning)
                return;
            if (!Stats.IsEnoughStamina())
            {
                return;
            }
            Stats.Stamina -= Stats.StaminaCost;
            if (ControlsManager.GetDirection() != Vector2.Zero)
                _dashForce = ControlsManager.GetDirection();
            else
            {
                var difference = Vector2.Normalize(ControlsManager.MousePosition - Position);
                _dashForce = difference / difference.Length() * ControlsManager.GetMaxDirectionVector(); ;
            }
            DashTimer = new ActionTimer(Stats.DashLength);
            DashEffectTimer = new ActionTimer(Stats.DashEffectLength);
            RunIFrames(Stats.DashEffectLength);
            DashTimer.TimeUpdated += DashAction;
            DashEffectTimer.TimeElapsed += DashEnded;
        }
        private void DashEnded(object character, EventArgs e)
        {
            _dashFrames.Clear();
            _dashForce = Vector2.Zero;
        }
        private void DashAction(object character, EventArgs e)
        {
            ControlsManager.ForceSum += _dashForce * 5;
            var pos = Position;
            _dashFrames.Add(pos);
        }
        public void TestTakingDamage(float value)
        {
            DamageInstance testD = new(value, 1, DamageType.Blunt, false);
            TakeDamage(in testD);
        }
        public void TestTakingDamage()
        {
            DamageInstance testD = new(1, 1, DamageType.Blunt, false);
            TakeDamage(in testD);
        }
        public void TestHealing()
        {
            Heal(1);
        }
        public void Heal(float healAmount)
        {
            Stats.HealthPoints += healAmount;
        }
        public void Attack()
        {
            if (AttackCooldown.TimeLeft > 0)
            {
                //Debug.WriteLine("attack is on cooldown");
                return;
            }
            var flipsf = SpriteEffects.None;
            if (ControlsManager.MousePosition.X < Position.X)
                flipsf = SpriteEffects.FlipVertically;
            AttackCooldown.TimeLeft += (float)(1000 / CurrentWeapon.AttackSpeed); //IMPLEMENT ATTACKSPEED!
            var difference = Vector2.Normalize(ControlsManager.MousePosition - Position);
            var direction = difference / difference.Length();
            var range = (int)(CurrentWeapon.AttackRange + Stats.Range);
            var attackRect = new Rectangle(new
                ((int)(Position.X + (direction.X * range) - range / 2), (int)(Position.Y + (direction.Y * range) - range / 2)),
                new(range, range));
            var attackRect2 = new Rectangle(new
                ((int)(Position.X + (direction.X * range / 2) - range / 2), (int)(Position.Y + (direction.Y * range / 2) - range / 2)),
                new(range, range));
            CreateAttackAnimation(flipsf, range);
            //THIS DOES NOT WORK FINE
            var critModifier = Stats.CritDamage / 100F;
            foreach (var target in GameBody.GetGameInstance().SceneManager.CurrentRoom.Damageables.Where(target => target.BodyHitbox.Intersects(attackRect) || target.BodyHitbox.Intersects(attackRect2)))
            {
                if (target.HealthPoints <= 0)
                {
                    continue;
                }
                bool proc = GlobalUse.TryChance(Stats.CritChance);
                var dmg = CurrentWeapon.AttackDamage + Stats.Damage;
                if (!proc)
                    target.TakeDamage(new(dmg, 1.2F, CurrentWeapon.WeaponDamageType, proc));
                else
                {
                    target.TakeDamage(new(dmg * critModifier, 1.2F, CurrentWeapon.WeaponDamageType, proc));
                }
            }
            foreach (var target in GameBody.GetGameInstance().ProjectileSprites.Where(target => target.HurtBox.Intersects(attackRect) || target.HurtBox.Intersects(attackRect2)))
            {
                var newForce = Vector2.Lerp(difference / difference.Length(), target.Force, 0.03F);
                target.ChangeForce(newForce);
                target.Lifetime.TimeLeft /= 2;
            }
            if (GlobalUse.IsDebugMode)
            {
                GameBody.GetGameInstance().SceneManager.CurrentRoom.AddTempDrawableRect(attackRect);
                GameBody.GetGameInstance().SceneManager.CurrentRoom.AddTempDrawableRect(attackRect2);
            }
            else
            {
                GameBody.GetGameInstance().SceneManager.CurrentRoom.ClearTempDrawables();
            }
        }

        private void CreateAttackAnimation(SpriteEffects flipEffect, int range)
        {
            var animation = new EffectAnimationManager(_attackAnimation, 128, 64, 12, animationFrequency: 16.6F);
            animation.Activate(
            new(Position,
            rotation: (float)Math.Atan2(ControlsManager.MousePosition.Y - Position.Y, ControlsManager.MousePosition.X - Position.X),
            scale: range * 4 / animation.SourceRectangleSize.X,
            spriteEffect: flipEffect,
            layerDepth: 1F));
        }

        public void TakeDamage(in DamageInstance damage)
        {
            if (!IsInvincible)
            {
                var locald = damage;
                foreach (var armor in ArmorSet)
                {
                    locald *= armor.Resistances.First(res => res.Type == locald.Type);
                }
                var dmgTaken = locald.ExtractValue();
                var animator = new DamageNumberAnimationManager(new(Position.X, Position.Y), dmgTaken.ToString(), damage.IsCritical);
                Stats.HealthPoints -= dmgTaken;
                RunIFrames(Stats.I_FRAME_TIME);
                DamagedEffectTimer.TimeLeft += Stats.I_FRAME_TIME;
            }
            //if (GlobalUse.IsDebugMode)
            //{
            //    SaveCharState();
            //}
            else return;
        }
        private async void SaveCharState()
        {
            var options = new JsonSerializerOptions
            {
                IncludeFields = true,
                WriteIndented = true,
            };
            string fileName = $"character.state.{DateTime.UtcNow:yyyy-MM-dd}.json";
            await using FileStream f = File.Create(fileName);
            await JsonSerializer.SerializeAsync(f, this, options);
        }
    }
    public class Stats
    {
        public float Range { get; set; }
        public float Damage { get; set; }
        public float AttackSpeed { get; set; }
        public float Speed { get; set; }
        public const float MAX_CHARACTER_SPEED = 2F;
        public const float MIN_CHARACTER_SPEED = 0.5F;
        public const float I_FRAME_TIME = 500F;
        public float CritChance { get; set; }
        public float CritDamage { get; set; }
        public float StaminaRegenValue { get; set; }
        public float DashLength { get; set; }
        public float DashEffectLength => DashLength * 1.5F;
        public float StaminaCost { get; set; }

        private float _stamina;
        public float Stamina
        {
            get => _stamina;
            set
            {
                _stamina = value;
                if (_stamina > MaxStamina)
                    _stamina = MaxStamina;
                OnChangingStamina?.Invoke(_stamina);
            }
        }
        private float _maxStamina;
        public float MaxStamina
        {
            get
            {
                return _maxStamina;
            }
            set
            {
                _maxStamina = value;
                OnChangingMaxStamina?.Invoke(_maxStamina);
            }
        }
        private float _healthPoints;
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
        public float MaxHealthPoints { get; set; }
        public delegate void ObserveFloatStat(float statValue);
        public delegate void NoParamsVoid();
        public event NoParamsVoid OnNotEnoughStamina;
        public event ObserveFloatStat OnChangingHealth;
        public event ObserveFloatStat OnTakingDamage;
        public event ObserveFloatStat OnHealing;
        public event ObserveFloatStat OnChangingStamina;
        public event ObserveFloatStat OnChangingMaxStamina;
        public Stats(float speed, float maxHealth)
        {
            Range = 0;
            Damage = 0;
            AttackSpeed = 0;
            Speed = speed;
            CritChance = 5F;
            CritDamage = 200F;
            StaminaRegenValue = 0.02F;
            DashLength = 100F;
            StaminaCost = 50F;
            Stamina = MaxStamina = 100F;
            HealthPoints = MaxHealthPoints = maxHealth;
        }
        public bool IsEnoughStamina()
        {
            if (Stamina < StaminaCost)
            {
                OnNotEnoughStamina.Invoke();
                return false;
            }
            else return true;
        }
    }
}

