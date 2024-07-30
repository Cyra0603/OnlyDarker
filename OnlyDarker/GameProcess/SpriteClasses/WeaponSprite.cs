using OnlyDarker.CommonUsing;
using OnlyDarker.PlayerClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess.SpriteClasses
{
    public class WeaponSprite : IYSortable, IInteractive, ICollectible
    {
        public Texture2D Texture { get; }
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
                Texture = GlobalUse.Content.Load<Texture2D>("Weapons/" + textureFileName); //rework to remove texture loading
            else
                Texture = GameBody.EmptyTexture;
            Position = position;
        }
        public WeaponSprite(Vector2 position, Texture2D texture, WeaponData weaponData)
        {
            Data = weaponData;
            Texture = texture;
            Position = position;
        }
        public void Draw()
        {
            (this as ICollectible).CollectibleDraw();
        }
        public void Interact()
        {
            //SoundManager.PlaySoundEffect(PickupSound);
            Collect();
        }
        public void Collect()
        {
            if (GameBody.GetGameInstance().MainCharacter.Inventory.TryPickupWeapon(this))
            {
                GameBody.GetGameInstance().SceneManager.CurrentRoom.ObjectsYSorted.Remove(this);
                GameBody.GetGameInstance().SceneManager.CurrentRoom.Interactives.Remove(this);
            }
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
        public List<DescriptionElement> GetDescriptionElements()
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
    public class PremadeWeaponSprites
    {
        private static PremadeWeaponSprites _instance;
        private readonly List<WeaponSprite> _weaponDatas;
        private PremadeWeaponSprites()
        {
            _weaponDatas = new List<WeaponSprite> //rework to json
            {
                new WeaponSprite(Vector2.Zero,  "Sword",new(100F, 6F, 2F, DamageType.Slice, "Sword", "A trusty sword")),
                new WeaponSprite(Vector2.Zero, "Stick", new(150F, 3F, 3F, DamageType.Blunt, "Stick", "A trusty stick")),
                new WeaponSprite(Vector2.Zero, "Lance", new(150F, 8F, 1.5F, DamageType.Poke, "Lance", "A long trusty lance")),
                new WeaponSprite(Vector2.Zero, "Fist", new(100F, 2F, 2F, DamageType.Blunt, "Fist", "Your own fist (Yeah, just one of them)")),
            };
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
        public WeaponSprite GetNewSprite(string weaponName)
        {
            var sprite = _weaponDatas.FirstOrDefault(sprite => sprite.IngameName == weaponName) ?? throw new KeyNotFoundException($"{weaponName} not found in PremadeWeaponSprites");
            var newSprite = new WeaponSprite(Vector2.Zero, sprite.Texture, new(sprite.Data.AttackRange, sprite.Data.AttackDamage, sprite.Data.AttackSpeed, sprite.Data.WeaponDamageType, sprite.Data.WeaponName, sprite.Data.Description));
            return newSprite;
        }
        public WeaponSprite GetNewSprite(string weaponName, Vector2 position)
        {
            var sprite = _weaponDatas.FirstOrDefault(sprite => sprite.IngameName == weaponName) ?? throw new KeyNotFoundException($"{weaponName} not found in PremadeWeaponSprites");
            var newSprite = new WeaponSprite(position, sprite.Texture, new(sprite.Data.AttackRange, sprite.Data.AttackDamage, sprite.Data.AttackSpeed, sprite.Data.WeaponDamageType, sprite.Data.WeaponName, sprite.Data.Description));
            return newSprite;
        }
    }
}
