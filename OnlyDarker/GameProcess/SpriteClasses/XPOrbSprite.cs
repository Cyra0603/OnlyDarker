using OnlyDarker.CommonUsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess.SpriteClasses
{
    public class XPOrbSprite : INonSortable, IMyUpdateable
    {
        public Texture2D Texture { get; }
        public int XPReward { get; }
        public Vector2 Position { get; set; }
        public List<Vector2> TrailPositions { get; set; }
        public bool IsExpired { get; set; }
        public float ColorScale { get; set; }
        //public float ColorPulseValue => (float)Math.Sin(GameBody.GetGameInstance().GetSwayFunctionValue() * PULSE_FREQUENCY) * PULSE_AMPLITUDE;
        //const float PULSE_AMPLITUDE = 2F;
        //const float PULSE_FREQUENCY = 5F;
        public const float SCALE_ONE_PERCENT = 500F;
        public XPOrbSprite(Texture2D texture, int xpReward, Vector2 position)
        {
            Texture = texture;
            Position = position;
            var trailLength = 10;
            TrailPositions = new(trailLength);
            for (int i = 0; i < trailLength; i++)
            {
                TrailPositions.Add(Position);
            }
            IsExpired = false;
            XPReward = xpReward;
            SetColorScale(xpReward);
        }
        public void Draw()
        {
            if (IsExpired)
                return;
            GlobalUse.SpriteBatch.Draw(Texture, Position, null, Color.White, 0F, Texture.Bounds.Size.ToVector2() / 2, 0.2F, SpriteEffects.None, 1F);
            GlobalUse.SpriteBatch.Draw(Texture, Position, null, Color.Red * ColorScale, 0F, Texture.Bounds.Size.ToVector2() / 2, 0.2F, SpriteEffects.None, 1F);
            float transparency = 0.5F;
            Span<float> tvalues = stackalloc float[TrailPositions.Count];
            tvalues[^1] = transparency;
            for (int i = 2; i <= TrailPositions.Count; i++)
            {
                transparency *= 0.85F;
                tvalues[^i] = transparency;
            }
            int j = 0;
            foreach (var position in TrailPositions)
            {
                GlobalUse.SpriteBatch.Draw(Texture, position, null, Color.White * tvalues[j], 0F, Texture.Bounds.Size.ToVector2() / 2, 0.2F, SpriteEffects.None, 1F);
                GlobalUse.SpriteBatch.Draw(Texture, position, null, Color.Red * ColorScale * tvalues[j], 0F, Texture.Bounds.Size.ToVector2() / 2, 0.2F, SpriteEffects.None, 1F);
                j++;
            }
        }
        private void SetColorScale(float xpReward)
        {
            if (xpReward < SCALE_ONE_PERCENT)
                ColorScale = 0;
            ColorScale = (xpReward / SCALE_ONE_PERCENT) / 100;
            if (ColorScale > 1)
                ColorScale = 1;
        }
        public void Update(float elapsedMilliseconds)
        {

            Position = Vector2.Lerp(Position, GameBody.GetGameInstance().MainCharacter.Position, 0.1F);
            Position = Vector2.SmoothStep(Position, GameBody.GetGameInstance().MainCharacter.Position, 0.05F);
            TrailPositions.Add(Position);
            TrailPositions.RemoveAt(0);
            if (GameBody.GetGameInstance().MainCharacter.BodyHitbox.Contains(Position))
            {
                GameBody.GetGameInstance().MainCharacter.AddXP(XPReward);
                IsExpired = true;
            }
        }
    }
}
