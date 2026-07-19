using OnlyDarker.CommonUsing;
using OnlyDarker.GameProcess;
using OnlyDarker.GameProcess.Interfaces;
using OnlyDarker.GameProcess.SpriteClasses;
using OnlyDarker.GameProcess.SpriteClasses.Collectibles;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.PlayerClasses
{
    public class Inventory
    {
        //data
        public readonly Stats Stats;
        public readonly InventorySlot[,] Stash;
        private InventorySlot _tempSlot;
        private InventorySlot _draggedSlot;
        private InventorySlot _hoveredSlot;
        private List<DescriptionElement> _currentDescription;
        private ContextMenu _contextMenu;

        public InventorySlot WeaponSlot;
        private WeaponSprite WeaponFist;

        private readonly List<ArmorInventorySlot> _armorInventorySlots;
        public ArmorInventorySlot HelmetSlot;
        public ArmorInventorySlot ChestSlot;
        public ArmorInventorySlot GlovesSlot;
        public ArmorInventorySlot BootsSlot;
        public ArmorInventorySlot PantsSlot;
        public ArmorInventorySlot AccessorySlot;

        private bool _isActive = false;
        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                if (_tempSlot.Container is not null)
                {
                    _draggedSlot.Container = _tempSlot.Container;
                    _tempSlot.Container = null;
                    _draggedSlot = null;
                }
                GC.Collect();
            }
        }
        //drawing
        private const int SLOTS_DRAWING_OFFSET = 5;
        public Texture2D WeaponDefaultTexture { get; }
        public Texture2D SlotDefaultTexture { get; }
        public Rectangle MainBounds => new(0, GlobalUse.WindowSize.Y / 4, GlobalUse.WindowSize.X / 5, (GlobalUse.WindowSize.Y / 2) - SLOTS_DRAWING_OFFSET / 2);
        public Rectangle WeaponSlotBounds => new(MainBounds.Location.X + SLOTS_DRAWING_OFFSET + MainBounds.Width / 2 + SlotSize.X / 2, MainBounds.Location.Y + MainBounds.Height / 3 - SlotSize.Y / 2, SlotSize.X, SlotSize.Y);
        public Rectangle ContextMenuRect;
        public Point SlotSize => new(StashSlotSize.X * 2, StashSlotSize.Y * 2);/*new(SlotsBounds.Width - SLOTS_DRAWING_OFFSET, SlotsBounds.Width - SLOTS_DRAWING_OFFSET);*/
        public Point StashSlotSize => new(MainBounds.Width / 12, MainBounds.Width / 12);
        private Color _hoveredSlotColor;
        public Color InventoryBackgroundColor;
        public Color SlotBackgroundColor;
        private bool _contextMenuShouldBeDrawn = false;

        public Inventory(Stats stats, WeaponSprite weapon)
        {
            Stats = stats;
            Stash = new InventorySlot[6, 12];
            _tempSlot = new InventorySlot(Rectangle.Empty, Point.Zero);
            _draggedSlot = new InventorySlot(Rectangle.Empty, Point.Zero);
            _hoveredSlot = null;
            WeaponFist = weapon;
            ChestSlot = new(PremadeArmorSprites.GetInstance().GetNewSprite("Leather chestplate").Texture, ArmorType.Chest, new(WeaponSlotBounds.X - SLOTS_DRAWING_OFFSET - SlotSize.X, WeaponSlotBounds.Y, WeaponSlotBounds.Width, WeaponSlotBounds.Height), Point.Zero);
            HelmetSlot = new(PremadeArmorSprites.GetInstance().GetNewSprite("Leather helmet").Texture, ArmorType.Helmet, new(ChestSlot.Bounds.X, ChestSlot.Bounds.Y - SLOTS_DRAWING_OFFSET - SlotSize.Y, ChestSlot.Bounds.Width, ChestSlot.Bounds.Height), Point.Zero);
            GlovesSlot = new(PremadeArmorSprites.GetInstance().GetNewSprite("Leather gloves").Texture, ArmorType.Gloves, new(ChestSlot.Bounds.X - SLOTS_DRAWING_OFFSET - SlotSize.X, ChestSlot.Bounds.Y, ChestSlot.Bounds.Width, ChestSlot.Bounds.Height), Point.Zero);
            PantsSlot = new(PremadeArmorSprites.GetInstance().GetNewSprite("Leather pants").Texture, ArmorType.Pants, new(ChestSlot.Bounds.X, ChestSlot.Bounds.Y + SLOTS_DRAWING_OFFSET + SlotSize.Y, ChestSlot.Bounds.Width, ChestSlot.Bounds.Height), Point.Zero);
            BootsSlot = new(PremadeArmorSprites.GetInstance().GetNewSprite("Leather boots").Texture, ArmorType.Boots, new(PantsSlot.Bounds.X, PantsSlot.Bounds.Y + SLOTS_DRAWING_OFFSET + SlotSize.Y, PantsSlot.Bounds.Width, PantsSlot.Bounds.Height), Point.Zero);
            AccessorySlot = new(PremadeArmorSprites.GetInstance().GetNewSprite("Iron necklace").Texture, ArmorType.Accessory, new(HelmetSlot.Bounds.X - SLOTS_DRAWING_OFFSET - SlotSize.X, HelmetSlot.Bounds.Y, HelmetSlot.Bounds.Width, HelmetSlot.Bounds.Height), Point.Zero);
            _armorInventorySlots = new()
            {
                HelmetSlot,
                ChestSlot,
                GlovesSlot,
                BootsSlot,
                PantsSlot,
                AccessorySlot,
            };
            for (int y = 0; y < Stash.GetLength(0); y++)
            {
                for (int x = 0; x < Stash.GetLength(1); x++)
                {
                    Stash[y, x] = new InventorySlot(new(MainBounds.X + (x * StashSlotSize.X), BootsSlot.Bounds.Y + BootsSlot.Bounds.Height * 2 + (y * StashSlotSize.Y), StashSlotSize.X, StashSlotSize.Y), Point.Zero);
                }
            }
            WeaponDefaultTexture = PremadeWeaponSprites.GetInstance().GetExistingSprite("Sword").Texture;
            SlotDefaultTexture = GlobalUse.Content.Load<Texture2D>("UI/SlotTexture");
            InventoryBackgroundColor = new Color(42, 42, 42);
            SlotBackgroundColor = new Color(71, 71, 71);
            WeaponSlot = new InventorySlot(Rectangle.Empty, WeaponSlotBounds.Location)
            {
                Container = weapon
            };
            TryStore(PremadeArmorSprites.GetInstance().GetNewSprite("Leather helmet"), out _);
            TryStore(PremadeArmorSprites.GetInstance().GetNewSprite("Leather chestplate"), out _);
            TryStore(PremadeArmorSprites.GetInstance().GetNewSprite("Leather boots"), out _);
            TryStore(PremadeArmorSprites.GetInstance().GetNewSprite("Iron ring"), out _);
            TryStore(PremadeArmorSprites.GetInstance().GetNewSprite("Iron necklace"), out _);
            TryStore(PremadeArmorSprites.GetInstance().GetNewSprite("Leather gloves"), out _);
            TryStore(PremadeArmorSprites.GetInstance().GetNewSprite("Leather pants"), out _);
            TryStore(PremadeWeaponSprites.GetInstance().GetNewSprite("Wooden bow"), out _);
            TryStore(PremadeWeaponSprites.GetInstance().GetNewSprite("Iron shuriken"), out _);
            TryStore(new CollectibleStack("Portal key", Vector2.Zero, 9), out _);
            TryStore(new CollectibleStack("Portal key", Vector2.Zero, 1), out _);
            TryStore(new CollectibleStack("Portal key", Vector2.Zero, 10), out _);
            TryStore(new CollectibleStack("Iron key", Vector2.Zero, 4), out _);
            TryStore(new CollectibleStack("Iron key", Vector2.Zero, 2), out _);
            TryStore(new CollectibleStack("Iron key", Vector2.Zero, 10), out _);
        }
        public void Update()
        {
            if (!IsActive)
                return;
            var localMouseState = GameBody.GetGameInstance().ControlsManager.CurrentMouseState;
            var localLastMouseState = GameBody.GetGameInstance().ControlsManager.LastMouseState;
            var localKeyboardState = GameBody.GetGameInstance().ControlsManager.CurrentKeyboardState;
            var cursor = localMouseState.Position;
            _hoveredSlot = null;
            _contextMenuShouldBeDrawn = false;
            if (_tempSlot.Container is not null)
            {
                _hoveredSlotColor = Color.Transparent;
            }
            else
            {
                _hoveredSlotColor = Color.NavajoWhite;
            }
            HandleStashIntersection(in localMouseState, in localLastMouseState, in cursor);
            HandleWeaponSlotIntersection(in localMouseState, in localLastMouseState, in cursor);
            HandleArmorSlotsIntersection(in localMouseState, in localLastMouseState, in cursor);
            if (localMouseState.LeftButton == ButtonState.Released && localLastMouseState.LeftButton == ButtonState.Pressed && _tempSlot.Container is not null)
            {
                DropItem(_tempSlot);
            }
            if (_hoveredSlot is not null)
            {
                _contextMenuShouldBeDrawn = true;
                if (_hoveredSlot.Container is not null)
                {
                    switch (_hoveredSlot.Container)
                    {
                        case WeaponSprite:
                            _currentDescription = (_hoveredSlot.Container as WeaponSprite).Data.GetDescriptionElements();
                            _currentDescription.Add(new("DPS~", $"{Math.Round(CalculateDPS(_hoveredSlot.Container as WeaponSprite), 2)}"));
                            _contextMenu = new(new(MainBounds.Location.X + MainBounds.Width, MainBounds.Location.Y), _hoveredSlot.Container, _currentDescription);
                            break;
                        case ArmorSprite:
                            _currentDescription = GetDescriptionElements(_hoveredSlot.Container as ArmorSprite);
                            _contextMenu = new(new(MainBounds.Location.X + MainBounds.Width, MainBounds.Location.Y), _hoveredSlot.Container, _currentDescription);
                            break;
                        case CollectibleStack:
                            _currentDescription = GetDescriptionElements(_hoveredSlot.Container);
                            _contextMenu = new(new(MainBounds.Location.X + MainBounds.Width, MainBounds.Location.Y), _hoveredSlot.Container, _currentDescription);
                            break;
                    }
                }
                else
                {
                    _contextMenuShouldBeDrawn = false;
                    _currentDescription = null;
                    _contextMenu = null;
                }
            }
            else
            {
                _currentDescription = null;
                _contextMenuShouldBeDrawn = false;
                _contextMenu = null;
            }
        }
        private void HandleArmorSlotsIntersection(in MouseState localMouseState, in MouseState localLastMouseState, in Point cursor)
        {
            foreach (var slot in _armorInventorySlots)
            {
                if (slot.Bounds.Contains(cursor))
                {
                    _hoveredSlot = slot;
                    if (_tempSlot.Container is not null && _tempSlot.Container is ArmorSprite && (_tempSlot.Container as ArmorSprite).Type == slot.ArmorType)
                    {
                        _hoveredSlotColor = Color.NavajoWhite;
                    }
                    if (localMouseState.LeftButton == ButtonState.Pressed && localLastMouseState.LeftButton == ButtonState.Released)
                    {

                        if (slot.Container is not null)
                        {
                            _tempSlot.Container = slot.Container;
                            _draggedSlot = slot;
                            slot.Container = null;
                        }
                    }
                    if (localMouseState.LeftButton == ButtonState.Released && localLastMouseState.LeftButton == ButtonState.Pressed && _tempSlot.Container is not null)
                    {
                        if (_tempSlot.Container is ArmorSprite && (_tempSlot.Container as ArmorSprite).Type == slot.ArmorType)
                        {
                            _draggedSlot.Container = slot.Container;
                            slot.Container = _tempSlot.Container;
                            _tempSlot.Container = null;
                        }
                        else
                        {
                            _draggedSlot.Container = _tempSlot.Container;
                            _tempSlot.Container = null;
                        }
                    }
                    break;
                }
            }
        }
        private void HandleWeaponSlotIntersection(in MouseState localMouseState, in MouseState localLastMouseState, in Point cursor)
        {
            if (WeaponSlotBounds.Contains(cursor))
            {
                _hoveredSlot = WeaponSlot;
                _hoveredSlot.Bounds = WeaponSlotBounds;
                if (_tempSlot.Container is not null && _tempSlot.Container is WeaponSprite)
                {
                    _hoveredSlotColor = Color.NavajoWhite;
                }
                if (localMouseState.LeftButton == ButtonState.Pressed && localLastMouseState.LeftButton == ButtonState.Released)
                {

                    if (WeaponSlot.Container is not null && WeaponSlot.Container.IngameName != WeaponFist.IngameName)
                    {
                        _tempSlot.Container = WeaponSlot.Container;
                        _draggedSlot = WeaponSlot;
                        WeaponSlot.Container = WeaponFist;
                    }
                }
                if (localMouseState.LeftButton == ButtonState.Released && localLastMouseState.LeftButton == ButtonState.Pressed && _tempSlot.Container is not null)
                {
                    if (_tempSlot.Container is WeaponSprite)
                    {
                        if (WeaponSlot.Container.IngameName != WeaponFist.IngameName)
                            _draggedSlot.Container = WeaponSlot.Container;
                        WeaponSlot.Container = _tempSlot.Container;
                        _tempSlot.Container = null;
                    }
                    else
                    {
                        _draggedSlot.Container = _tempSlot.Container;
                        _tempSlot.Container = null;
                    }
                }
            }
        }
        private void HandleStashIntersection(in MouseState localMouseState, in MouseState localLastMouseState, in Point cursor)
        {
            for (int y = 0; y < Stash.GetLength(0); y++)
            {
                for (int x = 0; x < Stash.GetLength(1); x++)
                {
                    if (Stash[y, x].Bounds.Contains(cursor))
                    {
                        _hoveredSlot = Stash[y, x];
                        _hoveredSlotColor = Color.NavajoWhite;
                        if (localMouseState.LeftButton == ButtonState.Pressed && localLastMouseState.LeftButton == ButtonState.Released)
                        {
                            _tempSlot.Container = Stash[y, x].Container;
                            _draggedSlot = Stash[y, x];
                            Stash[y, x].Container = null;
                        }
                        if (localMouseState.LeftButton == ButtonState.Released && localLastMouseState.LeftButton == ButtonState.Pressed && _tempSlot.Container is not null)
                        {
                            if (Stash[y, x].Container is null)
                            {
                                Stash[y, x].Container = _tempSlot.Container;
                                _tempSlot.Container = null;
                            }
                            else
                            {
                                if (Stash[y, x].Container is CollectibleStack stack1 && !stack1.IsFull && _tempSlot.Container is CollectibleStack stack2 && stack1.StackableID == stack2.StackableID)
                                {
                                    stack1.Merge(stack2, out _);
                                    _draggedSlot.Container = _tempSlot.Container;
                                    _tempSlot.Container = null;
                                    if (stack2.Size < 1)
                                        _draggedSlot.Container = null;
                                }
                                else
                                {
                                    _draggedSlot.Container = Stash[y, x].Container;
                                    Stash[y, x].Container = _tempSlot.Container;
                                    _tempSlot.Container = null;
                                }
                            }
                        }
                        break;
                    }
                }
            }
        }
        public void Draw()
        {
            if (!IsActive)
                return;
            GlobalUse.SpriteBatch.Draw(GameBody.EmptyTexture, MainBounds, InventoryBackgroundColor);
            //GlobalUse.SpriteBatch.DrawTriangle(GameBody.EmptyTexture, SlotsBounds, Color.Black);
            //foreach (var slot in Slots)
            //{
            //    GlobalUse.SpriteBatch.Draw(GameBody.EmptyTexture, slot.Bounds, Color.DarkGray);
            //    GameBody.DrawRectangleOutline(slot.Bounds, Color.White, borderWidth: 2);
            //    if (slot.Container is not null)
            //    {
            //        GlobalUse.SpriteBatch.Draw(slot.Container.Texture, slot.Bounds, Color.White);
            //    }
            //}
            foreach (var slot in Stash)
            {
                GlobalUse.SpriteBatch.Draw(SlotDefaultTexture, slot.Bounds, Color.White);
                //GameBody.DrawRectangleOutline(slot.Bounds, Color.Gray, borderWidth: 1);
                if (slot.Container is not null)
                {
                    GlobalUse.SpriteBatch.Draw(slot.Container.Texture, slot.Bounds, Color.White);
                    if (slot.Container is CollectibleStack stack)
                    {
                        GlobalUse.SpriteBatch.DrawString(GlobalUse.Arial, $"{stack.Size}", slot.Bounds.Location.ToVector2(), Color.White, 0F, Vector2.Zero, 0.15F, SpriteEffects.None, 1F);
                    }
                }
            }
            GlobalUse.SpriteBatch.Draw(GameBody.EmptyTexture, WeaponSlotBounds, /*Color.DarkGray*/SlotBackgroundColor);
            foreach (var slot in _armorInventorySlots)
            {
                GlobalUse.SpriteBatch.Draw(GameBody.EmptyTexture, slot.Bounds, /*Color.DarkGray*/SlotBackgroundColor);
                if (slot.Container is not null)
                {
                    GlobalUse.SpriteBatch.Draw(slot.Container.Texture, slot.Bounds, Color.White);
                }
                else
                {
                    GlobalUse.SpriteBatch.Draw(slot.DefaultTexture, slot.Bounds, Color.Black * 0.25F);
                }
            }
            if (_hoveredSlot is not null)
            {
                GameBody.DrawRectangleOutline(_hoveredSlot.Bounds, _hoveredSlotColor, borderWidth: 2);
            }
            if (WeaponSlot.Container.IngameName != WeaponFist.IngameName)
                GlobalUse.SpriteBatch.Draw(WeaponSlot.Container.Texture, WeaponSlotBounds, Color.White);
            else
            {
                GlobalUse.SpriteBatch.Draw(WeaponDefaultTexture, WeaponSlotBounds, Color.Black * 0.25F);
            }
            if (_tempSlot.Container is not null)
            {
                GlobalUse.SpriteBatch.Draw(_tempSlot.Container.Texture, GameBody.GetGameInstance().ControlsManager.CurrentMouseState.Position.ToVector2(), Color.White * 0.8F);
            }
            if (_contextMenuShouldBeDrawn)
            {
                _contextMenu?.Draw();
            }
        }
        public IEnumerable<ArmorSprite> GetArmorSet()
        {
            foreach (var slot in _armorInventorySlots)
            {
                yield return slot.Container as ArmorSprite;
            }
        }
        public void ToggleInventory()
        {
            IsActive = !IsActive;
        }
        public bool TryStore(ICollectible collectible, out string message)
        {
            for (int y = 0; y < Stash.GetLength(0); y++)
            {
                for (int x = 0; x < Stash.GetLength(1); x++)
                {
                    if (Stash[y, x].Container is null)
                    {
                        Stash[y, x].Container = collectible;
                        message = $"Picked up {collectible.IngameName}";
                        return true;
                    }
                }
            }
            message = $"Not enough space for {collectible.IngameName}";
            return false;
        }
        public bool StoreCollectibleStack(CollectibleStack collectibleStack, out string message)
        {
            int pickedUpAmount = CalculatePickedUpAmount(collectibleStack);
            if (pickedUpAmount >= 1)
            {
                message = $"Picked up {collectibleStack.IngameName} x{pickedUpAmount}";
                return true;
            }
            else
            {
                if (TryStore(collectibleStack, out message))
                    return true;
                else
                {
                    message = $"Not enough space for {collectibleStack.IngameName}";
                    return false;
                }
            }
            int CalculatePickedUpAmount(CollectibleStack collectibleStack)
            {
                int pickedUpAmount = 0;
                for (int y = 0; y < Stash.GetLength(0); y++)
                {
                    for (int x = 0; x < Stash.GetLength(1); x++)
                    {
                        if (Stash[y, x].Container is not null && Stash[y, x].Container is CollectibleStack stack)
                        {
                            if (stack.StackableID != collectibleStack.StackableID || stack.Size >= stack.MaxSize)
                                continue;
                            stack.Merge(collectibleStack, out int merged);
                            pickedUpAmount += merged;
                            if (collectibleStack.Size < 1)
                                return pickedUpAmount;
                        }
                    }
                }
                return pickedUpAmount;
            }
        }
        public bool TryWear(ArmorSprite newItem/*, out string message, out ArmorSprite currentItem*/)
        {
            ArmorInventorySlot slot = newItem.Type switch
            {
                ArmorType.Helmet => HelmetSlot,
                ArmorType.Boots => BootsSlot,
                ArmorType.Chest => ChestSlot,
                ArmorType.Pants => PantsSlot,
                ArmorType.Gloves => GlovesSlot,
                ArmorType.Accessory => AccessorySlot,
                _ => throw new NotImplementedException(),
            };
            if (slot.Container is null)
            {
                //currentItem = null;
                slot.Container = newItem;
                //message = $"Picked up {newItem.IngameName}";
                return true;
            }
            else
            {
                if (TryStore(newItem, out _))
                    return true;
            }
            return false;
        }
        public bool TryPickupWeapon(WeaponSprite newWeapon/*, out string message, out WeaponSprite currentWeapon*/)
        {
            if (WeaponSlot.Container.IngameName == WeaponFist.IngameName)
            {
                //currentWeapon = WeaponSlot.Container as WeaponSprite;
                //message = $"Picked up {newWeapon.IngameName}";
                WeaponSlot.Container = newWeapon;
                return true;
            }
            else
            {
                TryStore(newWeapon, out _);
                //currentWeapon = WeaponSlot.Container as WeaponSprite;
                //DropItem(WeaponSlot);
                //message = $"Swapped to {newWeapon.IngameName}";
                //WeaponSlot.Container = newWeapon;
                return true;
            }
        }
        public void ShowOptions(InventorySlot currentSlot)
        {
            if (currentSlot is null)
                return;
        }

        private List<DescriptionElement> GetDescriptionElements(ArmorSprite armor)
        {
            var elements = new List<DescriptionElement>
            {
                new("Armor type", armor.Type.GetName())
            };
            foreach (var resistance in armor.Resistances)
            {
                elements.Add(new DescriptionElement($"{resistance.Type.GetName()} resistance", $"{Math.Round((1 - resistance.Modifier) * 100)} %"));
            }
            return elements;
        }
        private List<DescriptionElement> GetDescriptionElements(ICollectible collectible)
        {
            var elements = new List<DescriptionElement>
            {
                new(string.Empty, string.Empty)
            };
            return elements;
        }
        private static void DropItem(InventorySlot slot)
        {
            slot.Container.Position = GameBody.GetGameInstance().MainCharacter.Position;
            //GameBody.GetGameInstance().SceneManager.CurrentRoom.SpawnEntity(slot.Container);
            GameBody.GetGameInstance().SceneManager.CurrentRoom.EntitiesToSpawn.Push(slot.Container);
            slot.Container = null;
        }
        private float CalculateDPS(WeaponSprite weapon)
        {
            float d = Stats.Damage + weapon.Data.AttackDamage;
            float cC = Stats.CritChance/* + weapon.Data.CriticalChance*/ / 100F;
            float cD = Stats.CritDamage/* + weapon.Data.CriticalDamage*/ / 100F;
            float aS = Stats.AttackSpeed + weapon.Data.AttackSpeed;
            return aS * (d * (1F + cC * (cD - 1F)));
        }
    }
    public class InventorySlot
    {
        public Rectangle Bounds { get; set; }
        public ICollectible Container { get; set; }
        public Point DrawnPosition { get; set; }
        public Point HiddenPosition => new(Bounds.Width * -2, DrawnPosition.Y);
        public InventorySlot(Rectangle bounds, Point drawnPosition)
        {
            Bounds = bounds;
            DrawnPosition = drawnPosition;
            Container = null;
        }
        public InventorySlot(ICollectible container, Rectangle bounds, Point drawnPosition)
        {
            Bounds = bounds;
            DrawnPosition = drawnPosition;
            Container = container;
        }
    }
    public class ArmorInventorySlot : InventorySlot
    {
        public Texture2D DefaultTexture { get; }
        public ArmorType ArmorType { get; set; }
        public ArmorInventorySlot(Texture2D defaultTexture, ArmorType armorType, Rectangle bounds, Point drawnPosition) : base(bounds, drawnPosition)
        {
            DefaultTexture = defaultTexture;
            ArmorType = armorType;
        }
    }
    public class ContextMenu
    {
        public Rectangle Bounds;
        public Rectangle DescriptionBounds;
        public Texture2D ItemTexture;
        public string Title;
        public string Description;
        public List<string> StringsToDraw;
        public float TextSize = 0.2F;
        public ContextMenu(Point location, ICollectible item, List<DescriptionElement> elements)
        {
            int elementsCount = 0;
            var font = GlobalUse.Arial;
            Title = item.IngameName;
            if (item is CollectibleStack stack)
                Title += $" {stack.Size}/{stack.MaxSize}";
            elementsCount++;
            Description = item.Description;
            elementsCount++;
            ItemTexture = item.Texture;
            StringsToDraw = new List<string>(elements.Count);
            foreach (var element in elements)
            {
                if (element.Name == string.Empty)
                    continue;
                StringsToDraw.Add($"{element.Name} : {element.Value}");
                elementsCount++;
            };
            int stringHeight = (int)(font.MeasureString(Title).Y * TextSize);
            int maxheight = stringHeight * elementsCount + ItemTexture.Height;
            int maxwidth = (int)(GetLongestStringLength() * TextSize) + maxheight;
            DescriptionBounds = new(location.X, location.Y + maxheight + maxheight * elements.Count, maxwidth, maxheight + maxheight);
            Bounds = new(location.X, location.Y, maxwidth, maxheight + item.Texture.Height + DescriptionBounds.Height);
        }
        public void Draw()
        {
            int offset = 45;
            int maxheight = 30;
            float textSize = 0.2F;
            var titleLength = GlobalUse.Arial.MeasureString(Title) * textSize;
            var descriptionLength = GlobalUse.Arial.MeasureString(Description) * textSize;
            var titlePos = new Vector2(Bounds.Location.X + Bounds.Width / 2 - titleLength.X / 2, Bounds.Location.Y + maxheight / 2);
            GlobalUse.SpriteBatch.Draw(GameBody.EmptyTexture, Bounds, Color.Gray);
            GlobalUse.SpriteBatch.DrawString(GlobalUse.Arial, Title, titlePos, Color.White, 0F, titleLength / 2, textSize, SpriteEffects.None, 0F);
            var texturePos = new Vector2(Bounds.X + Bounds.Width / 2 - ItemTexture.Width / 2, Bounds.Y + titleLength.Y + maxheight);
            var textureRect = ItemTexture.Bounds;
            textureRect.Location = texturePos.ToPoint();
            GlobalUse.SpriteBatch.Draw(ItemTexture, textureRect, Color.White);
            var descPos = new Vector2(Bounds.X + offset, texturePos.Y + maxheight * 2);
            GlobalUse.SpriteBatch.DrawString(GlobalUse.Arial, Description, descPos, Color.White, 0F, descriptionLength / 2, textSize, SpriteEffects.None, 0F);
            var stringsPos = new Vector2(descPos.X, descPos.Y + maxheight);
            foreach (var str in StringsToDraw)
            {
                var strLength = GlobalUse.Arial.MeasureString(str) * textSize;
                GlobalUse.SpriteBatch.DrawString(GlobalUse.Arial, str, stringsPos, Color.White, 0F, strLength / 2, textSize, SpriteEffects.None, 0F);
                stringsPos.Y += maxheight;
            }
            GameBody.DrawRectangleOutline(Bounds, Color.White, borderWidth: 2);
            GameBody.DrawRectangleOutline(textureRect, Color.White);
        }
        private int GetLongestStringLength()
        {
            int maxwidth = 0;
            float title = GlobalUse.Arial.MeasureString(Title).X;
            if (title > maxwidth)
                maxwidth = (int)title;
            float description = GlobalUse.Arial.MeasureString(Description).X;
            if (description > maxwidth)
                maxwidth = (int)description;
            foreach (var str in StringsToDraw)
            {
                float strLength = GlobalUse.Arial.MeasureString(str).X;
                if (strLength > maxwidth)
                    maxwidth = (int)strLength;
            }
            return maxwidth;
        }
    }
    public class DescriptionElement(string name, string value)
    {
        public string Name { get; set; } = name;
        public string Value { get; set; } = value;
    }
    public class StashCell
    {
        public ICollectible Container { get; set; }
        public bool IsOccupied;
        public StashCell()
        {
            Container = null;
            IsOccupied = false;
        }
    }

}
