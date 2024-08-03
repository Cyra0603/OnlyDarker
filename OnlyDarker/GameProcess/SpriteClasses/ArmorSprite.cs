using OnlyDarker.CommonUsing;
using OnlyDarker.GameProcess.SpriteClasses;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess
{
    public class ArmorSprite : IYSortable, IInteractive, ICollectible
    {
        public Texture2D Texture { get; }
        public Rectangle MovementCollider => new((int)Position.X/* - Texture.Width / 2*/, (int)Position.Y/* - Texture.Height / 2*/, Texture.Width, Texture.Height);
        public Resistance SliceResistance { get; private set; }
        public Resistance PokeResistance { get; private set; }
        public Resistance BluntResistance { get; private set; }
        public List<Resistance> Resistances { get; }
        public string IngameName { get; }
        public string TextureFileName { get; }
        public ArmorType Type { get; }
        public Vector2 Position { get; set; }
        public bool IsExpired { get; private set; } = false;
        public string InteractionMessage => INTERACTION_MESSAGE;
        const string INTERACTION_MESSAGE = " pick up ";
        public string Description { get; }
        public ArmorSprite(ArmorType armorType, Vector2 position, string ingameName, string textureFileName, string description, float sliceX = 1F, float pokeX = 1F, float bluntX = 1F)
        {
            Type = armorType;
            Texture = GlobalUse.Content.Load<Texture2D>("Armors/" + textureFileName);
            TextureFileName = textureFileName;
            Position = position;
            Resistances = new()
            {
                (SliceResistance = new(DamageType.Slice, sliceX)),
                (PokeResistance = new(DamageType.Poke, pokeX)),
                (BluntResistance = new(DamageType.Blunt, bluntX))
            };
            IngameName = ingameName;
            Description = description;
        }
        public ArmorSprite(ArmorType armorType, Vector2 position, Texture2D texture, string ingameName, string description, Resistance slice, Resistance poke, Resistance blunt)
        {
            Type = armorType;
            Texture = texture;
            TextureFileName = string.Empty;
            Position = position;
            SliceResistance = slice;
            PokeResistance = poke;
            BluntResistance = blunt;
            Resistances = new()
            {
                SliceResistance,
                PokeResistance,
                BluntResistance,
            };
            IngameName = ingameName;
            Description = description;
        }
        public void AddFlatArmor(float value)
        {
            foreach (var res in Resistances)
            {
                res.AddFlatValue(value);
            }
        }
        public void AddCommonModifier(float value)
        {
            foreach (Resistance res in Resistances)
            {
                res.AddModifier(value);
            }
        }
        public void ProcessDamageInstance(ref DamageInstance damageInstance)
        {
            foreach (var res in Resistances)
            {
                if (damageInstance.Type == res.Type)
                {
                    damageInstance *= res;
                    break;
                }
            }
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
            if (GameBody.GetGameInstance().MainCharacter.Inventory.TryWear(this))
            {
                GameBody.GetGameInstance().SceneManager.CurrentRoom.ObjectsYSorted.Remove(this);
                GameBody.GetGameInstance().SceneManager.CurrentRoom.Interactives.Remove(this);
            }
        }
    }
    public class BaseArmor
    {
        public Resistance SliceResistance { get; private set; }
        public Resistance PokeResistance { get; private set; }
        public Resistance BluntResistance { get; private set; }
        public List<Resistance> Resistances { get; }
        public BaseArmor(float sliceX = 1F, float pokeX = 1F, float bluntX = 1F)
        {
            Resistances = new()
            {
                (SliceResistance = new(DamageType.Slice, sliceX)),
                (PokeResistance = new(DamageType.Poke, pokeX)),
                (BluntResistance = new(DamageType.Blunt, bluntX))
            };
        }
        public void AddFlatArmor(float value)
        {
            foreach (var res in Resistances)
            {
                res.AddFlatValue(value);
            }
        }
        public void AddCommonModifier(float value)
        {
            foreach (Resistance res in Resistances)
            {
                res.AddModifier(value);
            }
        }
        public void ProcessDamageInstance(ref DamageInstance damageInstance)
        {
            foreach (var res in Resistances)
            {
                if (damageInstance.Type == res.Type)
                {
                    damageInstance *= res;
                    break;
                }
            }
        }
    }
    public class Resistance
    {
        public float Modifier { get; private set; } = 1F;
        public float FlatValue { get; private set; }
        public DamageType Type { get; }
        public Resistance(DamageType type, float modifier = 1F, float flatValue = 0F)
        {
            FlatValue = flatValue;
            Type = type;
            Modifier = modifier;
        }
        public void AddFlatValue(float value)
        {
            FlatValue += value;
        }
        public void AddModifier(float value)
        {
            Modifier += value;
        }
    }
    public class PremadeArmorSprites
    {
        private static PremadeArmorSprites _instance;
        private readonly List<ArmorSprite> _armorSprites;
        private PremadeArmorSprites() //Rework to json
        {
            _armorSprites = new List<ArmorSprite>
            {
                new ArmorSprite(ArmorType.Helmet, Vector2.Zero, "Leather helmet",  "LeatherHelmet","Common leather helmet, nothing special", sliceX: 0.95F, pokeX: 0.95F),
                new ArmorSprite(ArmorType.Chest, Vector2.Zero, "Leather armor",  "LeatherChestArmor","Common leather armor, nothing special", sliceX: 0.95F, pokeX: 0.95F, bluntX: 0.95F),
                new ArmorSprite(ArmorType.Boots, Vector2.Zero, "Leather boots",  "LeatherBoots","Common leather boots, nothing special", sliceX: 0.95F, pokeX: 0.95F, bluntX: 0.95F),
                new ArmorSprite(ArmorType.Accessory, Vector2.Zero, "Iron ring",  "IronRing","Shiny heavy iron ring", sliceX: 0.95F, bluntX: 0.95F),
            };
            _instance = this;
        }
        public static PremadeArmorSprites GetInstance()
        {
            if (_instance is not null)
            {
                return _instance;
            }
            return new PremadeArmorSprites();
        }
        public ArmorSprite GetNewSprite(string armorName)
        {
            var sprite = _armorSprites.FirstOrDefault(sprite => sprite.IngameName == armorName) ?? throw new KeyNotFoundException($"{armorName} not found in PremadeArmorSprites");
            var newSprite = new ArmorSprite(sprite.Type, Vector2.Zero, sprite.Texture, sprite.IngameName, sprite.Description, sprite.SliceResistance, sprite.PokeResistance, sprite.BluntResistance);
            return newSprite;
        }
        public ArmorSprite GetNewSprite(string armorName, Vector2 position)
        {
            var sprite = _armorSprites.FirstOrDefault(sprite => sprite.IngameName == armorName) ?? throw new KeyNotFoundException($"{armorName} not found in PremadeArmorSprites");
            var newSprite = new ArmorSprite(sprite.Type, position, sprite.Texture, sprite.IngameName, sprite.Description, sprite.SliceResistance, sprite.PokeResistance, sprite.BluntResistance);
            return newSprite;
        }
    }
}
