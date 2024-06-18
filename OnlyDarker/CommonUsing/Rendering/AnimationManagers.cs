using System;
using System.Collections.Generic;
using System.Linq;
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
            if(FrameTimer.TimeLeft <= 0)
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
                        Step = 0;
                }
            }
            public void Draw(Vector2 position, float rotation = 0F, float scale = 1F, SpriteEffects spriteEffect = SpriteEffects.None, float layerDepth = 0F)
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
        }
    }
}
