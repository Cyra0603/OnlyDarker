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
using System.IO;

namespace OnlyDarker.PlayerClasses
{

    public class Character : IYSortable
    {
        private readonly Texture2D _bodyTexture;
        private readonly SpriteSheet _bodyAnimationsSpriteSheet;
        private readonly Texture2D _handTexture;
        private static ControlsManager ControlsManager => GameBody.GetGameInstance().ControlsManager;
        private List<Vector2> _dashFrames = new();
        private Stack<IInteractive> _thingsToInteract = new();
        //body animation data
        private float distanceToWalk = 7F;
        private float walkedDistance;
        private int skippedUpdates;
        private int currentFrameIndex;
        private int maxFrameIndex = 12;
        //
        private Vector2 lastPosition;
        public Vector2 Position { get; set; }
        public Vector2 Origin { get; protected set; }
        private Vector2 _handOrigin;
        private float _handRotationValue => (float)Math.Atan2((ControlsManager.RelativeMousePosition - Position).Y, (ControlsManager.RelativeMousePosition - Position).X);
        private Vector2 _minPosition, _maxPosition;
        public Vector2 RightHandPosition => Position;
        public Vector2 LeftHandPosition { get; private set; }
        private Vector2 _dashForce;
        public Rectangle MovementCollisionAura => new(new Point(MovementCollider.Center.X - _bodyTexture.Width, MovementCollider.Center.Y - _bodyTexture.Height / 4), new(_bodyTexture.Width * 2, _bodyTexture.Height / 2));
        public Rectangle MovementCollider => new(new Point(
            (int)Position.X - _bodyTexture.Width / 2, (int)Position.Y + _bodyTexture.Height / 2 - (int)GlobalUse.PIXEL_OFFSET * 2),
            new(_bodyTexture.Width, (int)GlobalUse.PIXEL_OFFSET * 2)
            );
        public Rectangle InteractionAura => new(new Point((int)Position.X - _bodyTexture.Width, (int)Position.Y - _bodyTexture.Height / 2), new(_bodyTexture.Width * 2, (int)(_bodyTexture.Height * 1.25F)));
        public Rectangle BodyHitbox => new(new((int)(Position.X - _bodyTexture.Width / 2), (int)(Position.Y - _bodyTexture.Height / 2)), new(_bodyTexture.Width, _bodyTexture.Height));
        public Rectangle ShadowRect => new(BodyHitbox.Location.X, BodyHitbox.Location.Y + BodyHitbox.Height - (BodyHitbox.Height / 5 / 2), BodyHitbox.Width, BodyHitbox.Height / 5);
        public Stats Stats { get; private set; }
        public Inventory Inventory { get; private set; }
        public BaseArmor BaseArmor { get; private set; }
        public WeaponSprite CurrentWeapon => Inventory.WeaponSlot.Container as WeaponSprite;
        private Texture2D _attackAnimation = GlobalUse.Content.Load<Texture2D>("Character/AnimationSpriteSheets/CharacterAttackAnimation2");
        public ActionTimer? DashTimer;
        public ActionTimer? DashEffectTimer;
        public Timer AttackCooldown = new(0);
        public Timer InvincibilityTimer = new(0);
        public Timer DamagedEffectTimer = new(0);
        public Line line1;
        public Line line2;
        public Line line3;
        public Line line4;
        private SpriteEffects _flipEffect = SpriteEffects.None;
        public bool IsExpired { get; private set; }
        public bool IsInvincible
        {
            get => InvincibilityTimer.TimeLeft > 0;
            set { }
        }
        public bool IsPushable { get; set; } = false;
        public bool ShouldDrawAttackArea => GlobalUse.IsDebugMode;
        private Triangle AttackArea;
        public Character(Texture2D bodyTexture, Texture2D handTexture, SpriteSheet bodyAnimationSpriteSheet, SpriteStandartTile parentTile, Stats stats)
        {
            _bodyTexture = bodyTexture;
            _bodyAnimationsSpriteSheet = bodyAnimationSpriteSheet;
            _handTexture = handTexture;
            _handOrigin = new(handTexture.Width / 2, handTexture.Height / 2);
            Origin = new(bodyTexture.Width / 2, bodyTexture.Height / 2);
            Position = new(parentTile.Position.X, parentTile.Position.Y - (parentTile.GetTextureWidth() - bodyTexture.Width) / 2);
            Stats = stats;
            BaseArmor = new BaseArmor();
            Inventory = new(Stats, PremadeWeaponSprites.GetInstance().GetExistingSprite("Fist"));
            DashTimer = new ActionTimer(0);
            DashEffectTimer = new ActionTimer(0);
            DashTimer.TimeUpdated += DashAction;
            DashEffectTimer.TimeElapsed += DashEnded;
        }
        public void RunIFrames(float durationMilliseconds)
        {
            InvincibilityTimer.TimeLeft = Math.Max(InvincibilityTimer.TimeLeft, durationMilliseconds);
        }
        public void Draw()
        {
            //var weaponOrigin = new Vector2(CurrentWeapon.Texture.Width / 2, CurrentWeapon.Texture.Height / 2);
            var animSourceRect = _bodyAnimationsSpriteSheet.GetSourceRectangle(currentFrameIndex);
            if (DashTimer is not null && DashEffectTimer.IsRunning)
                for (int i = 0; i < _dashFrames.Count; i++)
                {
                    GlobalUse.SpriteBatch.Draw(_bodyAnimationsSpriteSheet.Texture, _dashFrames[i], animSourceRect, Color.White * (0.5F / (_dashFrames.Count - i)), 0F, Origin, 1F, _flipEffect, 0.5F);
                    //GlobalUse.SpriteBatch.Draw(CurrentWeapon.Texture, _dashFrames[i], null, Color.White * (0.5F / (_dashFrames.Count - i)), _handRotationValue, weaponOrigin, 1F, SpriteEffects.None, 0.5F);
                }
            GlobalUse.SpriteBatch.Draw(TextureMapper.GetInstance().ShadowTexture, ShadowRect, Color.White);
            GlobalUse.SpriteBatch.Draw(_bodyAnimationsSpriteSheet.Texture, Position, animSourceRect, Color.White, 0F, Origin, 1F, _flipEffect, 0.5F);
            GlobalUse.SpriteBatch.Draw(_bodyAnimationsSpriteSheet.Texture, Position, animSourceRect, Color.White, 0F, Origin, 1F, _flipEffect, 0.5F);
            //GlobalUse.SpriteBatch.Draw(CurrentWeapon.Texture, Position, null, Color.White, _handRotationValue, weaponOrigin, 1F, SpriteEffects.None, 0.5F);
            if (DamagedEffectTimer.TimeLeft > 0)
            {
                GlobalUse.SpriteBatch.Draw(_bodyAnimationsSpriteSheet.Texture, Position, animSourceRect, Color.Red * (DamagedEffectTimer.TimeLeft / 1000), 0F, Origin, 1F, _flipEffect, 0.5F);
                //GlobalUse.SpriteBatch.Draw(CurrentWeapon.Texture, Position, null, Color.Red * (DamagedEffectTimer.TimeLeft / 1000), _handRotationValue, weaponOrigin, 1F, SpriteEffects.None, 0.5F);
            }
            if (ShouldDrawAttackArea)
            {
                GlobalUse.SpriteBatch.DrawTriangle(AttackArea, Color.Black, thickness: 1F);  
            }
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
            lastPosition = Position;
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
            if (ControlsManager.RelativeMousePosition.X < Position.X)
                _flipEffect = SpriteEffects.FlipHorizontally;
            else
                _flipEffect = SpriteEffects.None;
            CheckForCollisions();
            CheckForInteractions();
            Position = Vector2.Clamp(Position, _minPosition, _maxPosition);
            UpdateBodyFrameCounter();
        }
        private void UpdateBodyFrameCounter()
        {
            var distanceWalked = Vector2.Distance(Position, lastPosition);
            if (distanceWalked >= 0.1)
                walkedDistance += distanceWalked;
            else
                currentFrameIndex = 0;
            if(walkedDistance >= distanceToWalk)
            {
                walkedDistance = 0;
                currentFrameIndex += 1;
                if(currentFrameIndex >= maxFrameIndex)
                {
                    currentFrameIndex = 0;
                }
            }
        }

        private void CheckForInteractions()
        {
            if (GameBody.GetGameInstance().SceneManager.CurrentRoom.Interactives is null)
            {
                return;
            }
            foreach (var interactive in GameBody.GetGameInstance().SceneManager.CurrentRoom.Interactives)
            {
                if (interactive.MovementCollider.Intersects(InteractionAura))
                {
                    interactive.ShowInteractionMessage();
                    _thingsToInteract.Push(interactive);
                    return;
                }
            }
        }

        private void CheckForCollisions()
        {
            if (GameBody.GetGameInstance().SceneManager.CurrentRoom.RoomColliders is null)
            {
                return;
            }
            bool shouldCheck = false;
            Span<Rectangle> rectangles = stackalloc Rectangle[GameBody.GetGameInstance().SceneManager.CurrentRoom.RoomColliders.Count];
            int rectsi = 0;
            foreach (var collider in GameBody.GetGameInstance().SceneManager.CurrentRoom.RoomColliders)
            {
                if (collider.Intersects(MovementCollisionAura))
                {
                    shouldCheck = true;
                    rectangles[rectsi] = collider.GetBounds();
                    rectsi++;
                }
            }
            if (shouldCheck)
            {
                for (int i = 0, j = (int)(ControlsManager.GetDirection().Length() + 1); i < j; i++)
                {
                    var currentDirection = ControlsManager.GetDirection();
                    CalculatePossibleCollisions(in rectangles, ref currentDirection);
                    Position += ControlsManager.GetDirection() / j * Stats.Speed;
                }
            }
            else
            {
                Position += ControlsManager.GetDirection() * Stats.Speed;
            }
        }

        private void CalculatePossibleCollisions(in Span<Rectangle> obstacles, ref Vector2 currentDirection)
        {
            var ly = currentDirection.Y;
            var lx = currentDirection.X;
            foreach (var rect in obstacles)
            {
                if (currentDirection.Y < 0 && rect.Intersects(CalculateMovementCollider(Position + new Vector2(lx, ly - 1F) * Stats.Speed)))
                {
                    ControlsManager.ZeroDirectionY();
                    ControlsManager.AddFriction();
                }
                if (currentDirection.Y > 0 && rect.Intersects(CalculateMovementCollider(Position + new Vector2(lx, ly + 1F) * Stats.Speed)))
                {
                    ControlsManager.ZeroDirectionY();
                    ControlsManager.AddFriction();
                }
                if (currentDirection.X < 0 && rect.Intersects(CalculateMovementCollider(Position + new Vector2(lx - 1F, ly) * Stats.Speed)))
                {
                    ControlsManager.ZeroDirectionX();
                    ControlsManager.AddFriction();
                }
                if (currentDirection.X > 0 && rect.Intersects(CalculateMovementCollider(Position + new Vector2(lx + 1F, ly) * Stats.Speed)))
                {
                    ControlsManager.ZeroDirectionX();
                    ControlsManager.AddFriction();
                }
            }
            Rectangle CalculateMovementCollider(Vector2 position)
            {
                return new(new Point(
                (int)position.X - _bodyTexture.Width / 2, (int)position.Y + _bodyTexture.Height / 2 - (int)GlobalUse.PIXEL_OFFSET * 2),
                new(_bodyTexture.Width, (int)GlobalUse.PIXEL_OFFSET * 2));
            }
        }
        //public Rectangle CalculateMovementCollider(Vector2 position)
        //{
        //    return new(new Point(
        //    (int)position.X - _bodyTexture.Width / 2, (int)position.Y + _bodyTexture.Height / 2 - (int)GlobalUse.PIXEL_OFFSET * 2),
        //    new(_bodyTexture.Width, (int)GlobalUse.PIXEL_OFFSET * 2));
        //}
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
            if (!float.TryParse(lArgs[0], out float value1))
            {
                throw new ArgumentException();
            }
            if (!float.TryParse(lArgs[1], out float value2))
            {
                throw new ArgumentException();
            }
            var newPos = new Vector2(value1, value2);
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
                var difference = Vector2.Normalize(ControlsManager.RelativeMousePosition - Position);
                _dashForce = difference / difference.Length() * ControlsManager.GetMaxDirectionVector(); ;
            }
            DashTimer.TimeLeft += Stats.DashLength;
            DashEffectTimer.TimeLeft += Stats.DashEffectLength;
            DashTimer.Unpause();
            DashEffectTimer.Unpause();
            RunIFrames(Stats.DashEffectLength);
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
            if (AttackCooldown.TimeLeft > 0 || Inventory.IsActive)
            {
                return;
            }
            var difference = Vector2.Normalize(ControlsManager.RelativeMousePosition - Position);
            var direction = difference / difference.Length();
            var attackOrigin = new Vector2(Position.X, Position.Y + BodyHitbox.Width / 2);
            CurrentWeapon.Attack(ControlsManager, Stats, direction, Position, attackOrigin);
            AttackCooldown.TimeLeft += 1000F / CurrentWeapon.Data.AttackSpeed;
        }

        private void CreateAttackAnimation(Vector2 source, SpriteEffects flipEffect, float range)
        {
            var animation = new EffectAnimationManager(_attackAnimation, 128, 128, 10, animationFrequency: 8.3F * CurrentWeapon.Data.AttackSpeed);
            animation.Activate(
            new(source,
            rotation: (float)Math.Atan2(ControlsManager.RelativeMousePosition.Y - source.Y, ControlsManager.RelativeMousePosition.X - source.X),
            scale: range / (animation.SourceRectangleSize.X / 2),
            spriteEffect: flipEffect));
        }

        public void TakeDamage(in DamageInstance damage)
        {
            if (IsInvincible)
            {
                return;
            }
            var locald = damage;
            BaseArmor.ProcessDamageInstance(ref locald);
            foreach (var armor in Inventory.GetArmorSet())
            {
                armor?.ProcessDamageInstance(ref locald);
            }
            var dmgTaken = locald.ExtractValue();
            var animator = new DamageNumberAnimationManager(new(Position.X, Position.Y), Math.Round((double)dmgTaken, 1).ToString(), damage.IsCritical);
            Stats.HealthPoints -= dmgTaken;
            RunIFrames(Stats.I_FRAME_TIME);
            DamagedEffectTimer.TimeLeft += Stats.I_FRAME_TIME;

        }
        public void AddXP(int amount)
        {
            Stats.XP += amount;
        }
        public void AddCoins(int amount)
        {
            Stats.CoinCount += amount;
        }
        public void ConsoleAddCoins(string amount)
        {
            string[] lArgs = amount.Split(',');
            if (lArgs.Length > 1)
                throw new ArgumentException();
            if (int.TryParse(lArgs[0], out int value))
            {
                Stats.CoinCount += value;
            }
            else
                throw new ArgumentException();
        }
        public void RemoveCoins(int amount, out bool result)
        {
            if (Stats.CoinCount < amount)
            {
                result = false;
                return;
            }
            Stats.CoinCount -= amount;
            result = true;
        }
        public void ConsoleAddXP(string xp)
        {
            string[] lArgs = xp.Split(',');
            if (lArgs.Length > 1)
                throw new ArgumentException();
            if (int.TryParse(lArgs[0], out int value))
            {
                Stats.XP += value;
            }
            else
                throw new ArgumentException();
        }
        //private async void SaveCharState()
        //{
        //    var options = new JsonSerializerOptions
        //    {
        //        IncludeFields = true,
        //        WriteIndented = true,
        //    };
        //    string fileName = $"character.state.{DateTime.UtcNow:yyyy-MM-dd}.json";
        //    await using FileStream f = File.Create(fileName);
        //    await JsonSerializer.SerializeAsync(f, this, options);
        //}
    }
    public class Stats
    {
        private int _xp;
        public int XP
        {
            get => _xp;
            set
            {
                var oldxp = _xp;
                _xp = value;
                TotalXP += _xp - oldxp;
                while (_xp > LevelThreshold)
                {
                    _xp -= LevelThreshold;
                    CharacterLevel++;
                    LevelThreshold = (int)(LevelThreshold * 1.2F);
                }
                ;
            }
        }
        public int TotalXP;
        public int CoinCount;
        private int _characterLevel;
        public int CharacterLevel
        {
            get => _characterLevel;
            set
            {
                _characterLevel = value;
                OnChangingLevel?.Invoke(_characterLevel);
            }
        }
        public int LevelThreshold;
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
        public delegate void ObserverIntStat(int statValue);
        public delegate void NoParamsVoid();
        public event NoParamsVoid OnNotEnoughStamina;
        public event ObserverIntStat OnChangingLevel;
        public event ObserveFloatStat OnChangingHealth;
        public event ObserveFloatStat OnTakingDamage;
        public event ObserveFloatStat OnHealing;
        public event ObserveFloatStat OnChangingStamina;
        public event ObserveFloatStat OnChangingMaxStamina;
        public Stats(float speed, float maxHealth)
        {
            XP = 0;
            TotalXP = 0;
            CoinCount = 0;
            LevelThreshold = 1000;
            CharacterLevel = 1;
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

