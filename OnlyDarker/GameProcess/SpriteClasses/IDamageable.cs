using OnlyDarker.CommonUsing;
using OnlyDarker.CommonUsing.Rendering;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess.SpriteClasses
{
    public interface IDamageable
    {
        Vector2 Position { get; set; }
        Hitbox BodyHitbox { get; }
        bool IsInvincible { get; }
        bool IsExpired { get; set; }
        bool IsPushable { get; }
        float HealthPoints { get; set; }
        float MaxHealthPoints { get; }
        int XPReward { get; }
        bool IsSummoned { get; }
        BaseArmor BaseArmor { get; }
        void TakeDamage(in DamageInstance damage)
        {
            if (IsInvincible)
            {
                return;
            }
            var locald = damage;
            BaseArmor.ProcessDamageInstance(ref locald);
            //foreach (var armor in ArmorSet)
            //{
            //    locald *= armor.Resistances.First(res => res.Type == locald.Type);
            //}
            var dmgTaken = locald.ExtractValue();
            if (dmgTaken < 0)
                dmgTaken = 0;
            HealthPoints -= dmgTaken;
            var animator = new DamageNumberAnimationManager(new(Position.X, Position.Y), Math.Round((double)dmgTaken, 1).ToString(), damage.IsCritical);
        }
        void DrawHPBar()
        {
            var bodyHitbox = BodyHitbox.GetBounds();
            var bounds = new Rectangle(bodyHitbox.Left, bodyHitbox.Top, bodyHitbox.Size.X, bodyHitbox.Size.X / 10);
            var currentHpBounds = new Rectangle(bounds.Location.X, bounds.Location.Y, (int)(bounds.Width * HealthPoints / MaxHealthPoints), bounds.Height);
            GlobalUse.SpriteBatch.Draw(GameBody.EmptyTexture, currentHpBounds, Color.Green);
            GlobalUse.SpriteBatch.Draw(GameBody.EmptyTexture, currentHpBounds, Color.Red * (1 - (HealthPoints / MaxHealthPoints)));
            GameBody.DrawRectangleOutline(bounds, Color.Black);
        }
        void Push(in Vector2 force)
        {
            if (IsPushable)
                Position += force;
        }
        void DropXPOrbs()
        {
            if (IsSummoned)
                return;
            if (XPReward <= 500)
            {
                SpawnOrb(XPReward);
                return;
            }
            int xpToSpread = XPReward;
            if (xpToSpread > 50001)
            {
                int xpvalue = xpToSpread - 50000;
                xpToSpread -= xpvalue;
                SpawnOrb(xpvalue);
            }
            if (xpToSpread > 5001)
            {
                int xpvalue = xpToSpread - 5000;
                xpToSpread -= xpvalue;
                SpawnOrb(xpvalue);
            }
            if (xpToSpread > 501)
            {
                int xpvalue = xpToSpread - 500;
                xpToSpread -= xpvalue;
                SpawnOrb(xpvalue);
            }
            SpawnOrb(xpToSpread);
        }
        private void SpawnOrb(int value)
        {
            var bodyHitbox = BodyHitbox.GetBounds();
            var offsetx = RandomNumberGenerator.GetInt32(-bodyHitbox.Width, bodyHitbox.Width);
            var offsety = RandomNumberGenerator.GetInt32(-bodyHitbox.Height, bodyHitbox.Height);
            //Spawn Entity is slow, but will be cleaner if optimized
            //GameBody.GetGameInstance().SceneManager.CurrentRoom.SpawnEntity(new XPOrbSprite(GameBody.GetGameInstance().TextureMapper.XPOrbSpriteTexture, value, new Vector2(Position.X + offsetx, Position.Y + offsety)));
            var orb = new XPOrbSprite(GameBody.GetGameInstance().TextureMapper.XPOrbSpriteTexture, GameBody.GetGameInstance().TextureMapper.XPOrbSpriteTrailTexture, value, new Vector2(Position.X + offsetx, Position.Y + offsety));
            //GameBody.GetGameInstance().SceneManager.CurrentRoom.ObjectsNotSorted.Add(orb);
            //GameBody.GetGameInstance().SceneManager.CurrentRoom.Updateables.Add(orb);
            GameBody.GetGameInstance().SceneManager.CurrentRoom.EntitiesToSpawn.Push(orb);
        }
    }
}

