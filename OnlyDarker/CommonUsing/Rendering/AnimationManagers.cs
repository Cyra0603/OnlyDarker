using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.CommonUsing.Rendering
{
    public class EntityAnimationManager
    {
        public enum EntityState
        {
            Idle,
            Walking,
            Attacking1,
            Attacking2,
            Attacking3,
            Attacking4,
            Attacking5,
            Sub1,
            Sub2,
            Sub3,
            Sub4
        }
        private EntityState _currentState;
        public EntityState CurrentState
        {
            get => _currentState;
            set
            {
                _currentState = value;
                Step = 1;
            }
        }
        private readonly Texture2D _spriteAtlas;
        public Point SourceRectangleSize;
        private Vector2 _defaultOrigin;
        public float AnimationFrequency;
        public float FrequencyModifier = 1F;
        public int Step;
        private int _maxSteps;
        public Timer FrameTimer;
        public EntityAnimationManager(Texture2D spriteAtlas, int sourceRectWidth, int sourceRectHeight, int maxSteps, float animationFrequency = 42F)
        {
            _spriteAtlas = spriteAtlas;
            _maxSteps = maxSteps;
            SourceRectangleSize = new(sourceRectWidth, sourceRectHeight);
            _defaultOrigin = new(SourceRectangleSize.X / 2, SourceRectangleSize.Y / 2);
            AnimationFrequency = animationFrequency;
            FrameTimer = new(animationFrequency);
        }
        public void Update(float elapsedMilliseconds)
        {
            FrameTimer.Update(elapsedMilliseconds);
            if (FrameTimer.TimeLeft <= 0)
            {
                FrameTimer.TimeLeft = AnimationFrequency / FrequencyModifier;
                Step++;
                if (Step > _maxSteps)
                    Step = 0;
            }
        }
        public void Draw(Vector2 position, float rotation = 0F, float scale = 1F, SpriteEffects spriteEffect = SpriteEffects.None, float layerDepth = 0F)
        {
            GlobalUse.SpriteBatch.Draw(
                _spriteAtlas,
                position,
                new Rectangle(Step * SourceRectangleSize.X, (int)CurrentState * SourceRectangleSize.Y, SourceRectangleSize.X, SourceRectangleSize.Y),
                Color.White,
                rotation,
                _defaultOrigin,
                scale,
                spriteEffect,
                layerDepth);
        }

    }
    public class EffectAnimationManager
    {
        private readonly Texture2D _spriteSheet;
        public Point SourceRectangleSize;
        private Vector2 _defaultOrigin;
        public float AnimationFrequency;
        public float FrequencyModifier = 1F;
        public int Step;
        private int _maxSteps;
        public Timer FrameTimer;
        public bool IsActive = false;
        public DrawCallArgs? DrawCallArgs;
        public EffectAnimationManager(Texture2D spriteSheet, int sourceRectWidth, int sourceRectHeight, int maxSteps, float animationFrequency = 42F)
        {
            _spriteSheet = spriteSheet;
            _maxSteps = maxSteps;
            SourceRectangleSize = new(sourceRectWidth, sourceRectHeight);
            _defaultOrigin = new(SourceRectangleSize.X / 2, SourceRectangleSize.Y / 2);
            AnimationFrequency = animationFrequency;
            FrameTimer = new(animationFrequency);
        }
        public void Update(float elapsedMilliseconds)
        {
            FrameTimer.Update(elapsedMilliseconds);
            if (FrameTimer.TimeLeft <= 0)
            {
                FrameTimer.TimeLeft = AnimationFrequency / FrequencyModifier;
                Step++;
                if (Step > _maxSteps)
                {
                    Step = 0;
                    IsActive = false;
                }
            }
        }
        public void Draw(Vector2 position, float rotation = 0F, float scale = 1F, SpriteEffects spriteEffect = SpriteEffects.None, float layerDepth = 1F)
        {
            GlobalUse.SpriteBatch.Draw(
                _spriteSheet,
                position,
                new Rectangle(Step * SourceRectangleSize.X, 0, SourceRectangleSize.X, SourceRectangleSize.Y),
                Color.White,
                rotation,
                _defaultOrigin,
                scale,
                spriteEffect,
                layerDepth);
        }
        public void Draw()
        {
            GlobalUse.SpriteBatch.Draw(
                _spriteSheet,
                DrawCallArgs.Position,
                new Rectangle(Step * SourceRectangleSize.X, 0, SourceRectangleSize.X, SourceRectangleSize.Y),
                Color.White,
                DrawCallArgs.Rotation,
                _defaultOrigin,
                DrawCallArgs.Scale,
                DrawCallArgs.SpriteEffect,
                DrawCallArgs.LayerDepth);
        }
        public void Activate(DrawCallArgs drawCallArgs)
        {
            if (IsActive)
                return;
            Step = 0;
            IsActive = true;
            DrawCallArgs = drawCallArgs;
            GameBody.GetGameInstance().EffectAnimationManagers.Add(this);
        }
    }
    public class DamageNumberAnimationManager
    {
        public const float COMMON_LIFETIME = 800;
        public const float CRIT_LIFETIME = 1000;
        public Vector2 Position;
        public float ColorDencity = 1F;
        public const float CONST_DENCITY = 0.5F;
        public const int POSITION_DISPLACEMENT = 10;
        private int _positionDisplacement;
        public Timer FrameTimer;
        public bool IsCritical;
        public bool IsActive = true;
        private string _message;
        public DamageNumberAnimationManager(Vector2 position, string message, bool isCritical)
        {
            Position = position;
            _message = message;
            IsCritical = isCritical;
            if (isCritical)
                FrameTimer = new(CRIT_LIFETIME);
            else
                FrameTimer = new(COMMON_LIFETIME);
            _positionDisplacement = RandomNumberGenerator.GetInt32(-POSITION_DISPLACEMENT, POSITION_DISPLACEMENT);
            Position.X += _positionDisplacement;
            Position.Y += _positionDisplacement;
            GameBody.GetGameInstance().DamageNumberAnimationManagers.Add(this);
        }
        public void Update(float elapsedMilliseconds)
        {
            FrameTimer.Update(elapsedMilliseconds);
            if (FrameTimer.TimeLeft <= 0)
                IsActive = false;
            Position = new(Position.X, Position.Y - elapsedMilliseconds / 32);
            ColorDencity = FrameTimer.TimeLeft / CRIT_LIFETIME + CONST_DENCITY;
        }
        public void Draw()
        {
            if (IsCritical)
                GlobalUse.SpriteBatch.DrawString(GlobalUse.MainFont, "-" + _message, Position, Color.Red * ColorDencity, 0F, Vector2.Zero, 0.25F, SpriteEffects.None, 0F);
            else
                GlobalUse.SpriteBatch.DrawString(GlobalUse.MainFont, "-" + _message, Position, Color.White * ColorDencity, 0F, Vector2.Zero, 0.2F, SpriteEffects.None, 0F);
        }
    }
    public class DrawCallArgs 
    {
        public Vector2 Position;
        public float Rotation;
        public float Scale;
        public SpriteEffects SpriteEffect;
        public float LayerDepth;
        public DrawCallArgs(Vector2 position, float rotation = 0F, float scale = 1F, SpriteEffects spriteEffect = SpriteEffects.None, float layerDepth = 0F)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
            SpriteEffect = spriteEffect;
            LayerDepth = layerDepth;
        }
        ~DrawCallArgs()
        {
            Debug.WriteLine("Drawcallargs disposed");
        }
    }
}
