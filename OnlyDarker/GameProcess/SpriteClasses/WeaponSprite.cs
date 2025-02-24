using OnlyDarker;
using OnlyDarker.CommonUsing;
using OnlyDarker.CommonUsing.Rendering;
using OnlyDarker.PlayerClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OnlyDarker.GameProcess.SpriteClasses
{
    public abstract class WeaponSprite : IYSortable, IInteractive, ICollectible
    {
        public Texture2D Texture { get; protected set; }
        public Texture2D AttackAnimation { get; protected set; }
        public Vector2 Position { get; set; }
        public Rectangle MovementCollider => new((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
        public WeaponData Data;
        public string Description => Data.Description;
        public string IngameName => Data.WeaponName;
        public string TextureFileName { get; }
        public string PickupSound { get; } //temp
        public bool IsExpired { get; private set; } = false;
        public string InteractionMessage => INTERACTION_MESSAGE;
        const string INTERACTION_MESSAGE = " swap to ";

        public WeaponSprite(Vector2 position, string textureFileName, WeaponData weaponData)
        {
            Data = weaponData;
            if (weaponData.WeaponName != "Fist")
            {
                Texture = GlobalUse.Content.Load<Texture2D>("Weapons/" + textureFileName);
                //AttackAnimation = GlobalUse.Content.Load<Texture2D>("Weapons/" + textureFileName + "AttackAnimation");
            }
            else
            {
                Texture = GameBody.EmptyTexture;
                //AttackAnimation = GlobalUse.Content.Load<Texture2D>("Weapons/FistAttackAnimation");
            }
            Position = position;
        }
        public WeaponSprite(Vector2 position, Texture2D texture, Texture2D attackAnimation, WeaponData weaponData)
        {
            Data = weaponData;
            Texture = texture;
            AttackAnimation = attackAnimation;
            Position = position;
        }
        public void Draw()
        {
            (this as ICollectible).CollectibleDraw();
        }
        public virtual void Interact()
        {
            if (GameBody.GetGameInstance().MainCharacter.Inventory.TryPickupWeapon(this))
            {
                GameBody.GetGameInstance().SceneManager.CurrentRoom.ObjectsYSorted.Remove(this);
                GameBody.GetGameInstance().SceneManager.CurrentRoom.Interactives.Remove(this);
            }
        }
        public abstract void Attack(ControlsManager controlsManager, Stats stats, in Vector2 direction, in Vector2 position, in Vector2 attackOrigin);

        public abstract WeaponSprite GetCopy();
        public abstract WeaponSprite GetCopyAtPosition(Vector2 Position);
    }
    public class RangedWeaponSprite : WeaponSprite
    {
        private RangedWeaponData _data => (RangedWeaponData)Data;
        private ProjectileSprite _projectileSprite;
        public RangedWeaponSprite(Vector2 position, string textureFileName, ProjectileSprite projectileSprite, RangedWeaponData rangedWeaponData) : base(position, textureFileName, rangedWeaponData)
        {
            _projectileSprite = projectileSprite;
            AttackAnimation = GlobalUse.Content.Load<Texture2D>("Weapons/RangedAttackAnimation");
        }
        public RangedWeaponSprite(Vector2 position, Texture2D texture, Texture2D attackAnimation, ProjectileSprite projectileSprite, RangedWeaponData rangedWeaponData) : base(position, texture, attackAnimation, rangedWeaponData)
        {
            Data = rangedWeaponData;
            Texture = texture;
            AttackAnimation = attackAnimation;
            Position = position;
            _projectileSprite = projectileSprite;
        }
        public override void Attack(ControlsManager controlsManager, Stats stats, in Vector2 direction, in Vector2 position, in Vector2 attackOrigin)
        {
            var flipsf = SpriteEffects.None;
            if (controlsManager.RelativeMousePosition.X < position.X)
                flipsf = SpriteEffects.FlipVertically;
            var range = (int)(_data.AttackRange + stats.Range);
            var critModifier = stats.CritDamage / 100F;
            bool isCrit = GlobalUse.Roll(stats.CritChance);
            var dmg = _data.AttackDamage + stats.Damage;
            if (isCrit)
                dmg *= critModifier;
            var projectileHeight = Vector2.Distance(position, attackOrigin);
            var projectile = new AllyProjectileSprite(_projectileSprite.Texture, position, direction * _data.ProjectileSpeed, new(dmg, 1F, DamageType.Poke, isCrit), Vector2.Distance(Position, (Position + direction * range)) / _data.ProjectileSpeed, _projectileSprite.IsRotating, projectileHeight);

            GameBody.GetGameInstance().SceneManager.CurrentRoom.ObjectsYSorted.Add(projectile);
            GameBody.GetGameInstance().SceneManager.CurrentRoom.Updateables.Add(projectile);
            //GameBody.GetGameInstance().ProjectileSprites.Add(projectile);
            CreateAttackAnimation(controlsManager, position, flipsf, 10F);
        }
        private void CreateAttackAnimation(ControlsManager controlsManager, Vector2 source, SpriteEffects flipEffect, float range)
        {
            var animation = new EffectAnimationManager(AttackAnimation, AttackAnimation.Height, AttackAnimation.Height, AttackAnimation.Width / AttackAnimation.Height, animationFrequency: 8.3F * Data.AttackSpeed);

            animation.Activate(
            new(source,
            rotation: (float)Math.Atan2(controlsManager.RelativeMousePosition.Y - source.Y, controlsManager.RelativeMousePosition.X - source.X),
            scale: range / (animation.SourceRectangleSize.X / 2),
            spriteEffect: flipEffect));
        }
        public override WeaponSprite GetCopy()
        {
            return new RangedWeaponSprite(this.Position, this.Texture, this.AttackAnimation, this._projectileSprite, this._data);
        }
        public override WeaponSprite GetCopyAtPosition(Vector2 position)
        {
            return new RangedWeaponSprite(position, this.Texture, this.AttackAnimation, this._projectileSprite, this._data);
        }
    }
    public class MeleeWeaponSprite : WeaponSprite
    {
        private MeleeWeaponData _data => (MeleeWeaponData)Data;
        public MeleeWeaponSprite(Vector2 position, string textureFileName, MeleeWeaponData meleeWeaponData) : base(position, textureFileName, meleeWeaponData)
        {
            AttackAnimation = GlobalUse.Content.Load<Texture2D>("Weapons/MeleeAttackAnimation");
        }
        public MeleeWeaponSprite(Vector2 position, Texture2D texture, Texture2D attackAnimation, MeleeWeaponData meleeWeaponData) : base(position, texture, attackAnimation, meleeWeaponData)
        {
            Data = meleeWeaponData;
            Texture = texture;
            AttackAnimation = attackAnimation;
            Position = position;
        }
        public override void Attack(ControlsManager controlsManager, Stats stats, in Vector2 direction, in Vector2 position, in Vector2 attackOrigin)
        {
            var flipsf = SpriteEffects.None;
            if (controlsManager.RelativeMousePosition.X < position.X)
                flipsf = SpriteEffects.FlipVertically;
            var range = (int)(_data.AttackRange + stats.Range);
            var attackPoint = position + (direction * range);
            var halfHeight = position + (direction * (range * 0.2F));
            var triangleZ = attackOrigin;
            var triangleX = halfHeight.RotateAround(attackPoint, -90);
            var triangleY = halfHeight.RotateAround(attackPoint, 90);
            Triangle attackArea = new(triangleX, triangleY, triangleZ);
            CreateAttackAnimation(controlsManager, triangleZ, flipsf, Vector2.Distance(triangleZ, attackPoint));
            foreach (var target in GameBody.GetGameInstance().SceneManager.CurrentRoom.Damageables)
            {
                if (!attackArea.Intersects(target.BodyHitbox))
                {
                    continue;
                }
                if (target.HealthPoints <= 0)
                {
                    continue;
                }
                bool isCrit = GlobalUse.Roll(stats.CritChance);
                var dmg = _data.AttackDamage + stats.Damage;
                var critModifier = 1F;
                if (isCrit)
                    critModifier = stats.CritDamage / 100F;
                target.TakeDamage(new(dmg * critModifier, 1F, _data.WeaponDamageType, isCrit));
            }
        }
        private void CreateAttackAnimation(ControlsManager controlsManager, Vector2 source, SpriteEffects flipEffect, float range)
        {
            var animation = new EffectAnimationManager(AttackAnimation, 128, 128, 10, animationFrequency: 8.3F * Data.AttackSpeed);

            animation.Activate(
            new(source,
            rotation: (float)Math.Atan2(controlsManager.RelativeMousePosition.Y - source.Y, controlsManager.RelativeMousePosition.X - source.X),
            scale: range / (animation.SourceRectangleSize.X / 2),
            spriteEffect: flipEffect));
        }
        public override WeaponSprite GetCopy()
        {
            return new MeleeWeaponSprite(this.Position, this.Texture, this.AttackAnimation, this._data);
        }
        public override WeaponSprite GetCopyAtPosition(Vector2 position)
        {
            return new MeleeWeaponSprite(position, this.Texture, this.AttackAnimation, this._data);
        }
    }
    public class WeaponData
    {
        public float AttackRange { get; }
        public float AttackDamage { get; }
        public float AttackSpeed { get; }
        public DamageType WeaponDamageType { get; }
        public string WeaponName { get; }
        public string Description { get; }
        public WeaponData(float range, float damage, float attackSpeed, DamageType damageType, string weaponName, string description)
        {
            AttackRange = range;
            AttackDamage = damage;
            AttackSpeed = attackSpeed;
            WeaponDamageType = damageType;
            WeaponName = weaponName;
            Description = description;
        }
        public virtual List<DescriptionElement> GetDescriptionElements()
        {
            var elements = new List<DescriptionElement>()
            {
                new("Damage type",WeaponDamageType.GetName()),
                new("Attack damage", Math.Round(AttackDamage, 2).ToString()),
                new("Attack speed", Math.Round(AttackSpeed, 2).ToString()),
                new("Attack range", Math.Round(AttackRange, 2).ToString())
            };
            return elements;
        }
    }
    public class RangedWeaponData : WeaponData
    {
        public float ProjectileSpeed { get; set; }
        public float ProjectileHeight { get; set; }
        public RangedWeaponData(float projectileSpeed, float range, float damage, float attackSpeed, DamageType damageType, string weaponName, string description) : base(range, damage, attackSpeed, damageType, weaponName, description)
        {
            ProjectileSpeed = projectileSpeed;
            ProjectileHeight = 0F;
        }
        public override List<DescriptionElement> GetDescriptionElements()
        {
            var elements = new List<DescriptionElement>()
            {
                new("Damage type",WeaponDamageType.GetName()),
                new("Attack damage", Math.Round(AttackDamage, 2).ToString()),
                new("Attack speed", Math.Round(AttackSpeed, 2).ToString()),
                new("Attack range", Math.Round(AttackRange, 2).ToString()),
                new("Projectile speed", Math.Round(ProjectileSpeed, 2).ToString())
            };
            return elements;
        }
    }
    public class MeleeWeaponData : WeaponData
    {
        public float ProjectileSpeed { get; }
        public MeleeWeaponData(float range, float damage, float attackSpeed, DamageType damageType, string weaponName, string description) : base(range, damage, attackSpeed, damageType, weaponName, description) { }
    }

    public class PremadeWeaponSprites
    {
        private static PremadeWeaponSprites _instance;
        private readonly List<WeaponSprite> _weaponDatas;
        private PremadeWeaponSprites()
        {
            _weaponDatas =
            //rework to json
            [
                new MeleeWeaponSprite(Vector2.Zero,  "Sword",new(100F, 6F, 2F, DamageType.Slice, "Sword", "A trusty sword")),
                new MeleeWeaponSprite(Vector2.Zero, "Stick", new(150F, 4F, 3F, DamageType.Blunt, "Stick", "A trusty stick")),
                new MeleeWeaponSprite(Vector2.Zero, "Lance", new(150F, 8F, 1.5F, DamageType.Poke, "Lance", "A long trusty lance")),
                new MeleeWeaponSprite(Vector2.Zero, "Fist", new(100F, 2F, 2F, DamageType.Blunt, "Fist", "Your own fist (Yeah, just one of them)")),
                new RangedWeaponSprite(Vector2.Zero, "WoodenBow", new AllyProjectileSprite(TextureMapper.GetInstance().ArrowProjectileSprite, Vector2.Zero, Vector2.Zero, new(), 0F, false), new(8F, 10000F, 4F, 3F, DamageType.Poke, "Wooden bow", "A wooden bow")),
                new RangedWeaponSprite(Vector2.Zero, "IronShuriken", new AllyProjectileSprite(TextureMapper.GetInstance().IronShurikenSprite, Vector2.Zero, Vector2.Zero, new(), 0F, true), new(3F, 8000F, 2F, 6F, DamageType.Poke, "Iron shuriken", "An iron shuriken")),
            ];
            _instance = this;
        }
        public static PremadeWeaponSprites GetInstance()
        {
            if (_instance is not null)
            {
                return _instance;
            }
            return new PremadeWeaponSprites();
        }
        public WeaponSprite GetExistingSprite(string weaponName)
        {
            var sprite = _weaponDatas.FirstOrDefault(sprite => sprite.IngameName == weaponName) ?? throw new KeyNotFoundException($"{weaponName} not found in PremadeWeaponSprites");
            return sprite;
        }
        public WeaponSprite GetNewSprite(string weaponName)
        {
            var sprite = _weaponDatas.FirstOrDefault(sprite => sprite.IngameName == weaponName) ?? throw new KeyNotFoundException($"{weaponName} not found in PremadeWeaponSprites");
            return sprite.GetCopy();
        }
        public WeaponSprite GetNewSprite(string weaponName, Vector2 position)
        {
            var sprite = _weaponDatas.FirstOrDefault(sprite => sprite.IngameName == weaponName) ?? throw new KeyNotFoundException($"{weaponName} not found in PremadeWeaponSprites");
            return sprite.GetCopyAtPosition(position);
        }
    }
}
