using OnlyDarker.GameProcess.Interfaces;
using OnlyDarker.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess.SpriteClasses.Collectibles
{
    public class CollectibleStack : IYSortable, IInteractive, ICollectible
    {
        public Texture2D Texture => Container.Texture;

        public IStackable Container { get; set; }

        public Vector2 Position { get; set; }

        public Rectangle MovementCollider => new((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);

        public int Size;

        public int MaxSize => Container.MaxStackSize;

        public int StackableID => Container.ID;

        public bool IsFull => Size >= MaxSize;

        public bool IsExpired => Container.IsExpired;

        public string IngameName => Container.IngameName;

        public string TextureFileName { get; set; }

        public string InteractionMessage => Container.InteractionMessage;

        public string Description => Container.Description;

        public CollectibleStack(IStackable stackableItem, Vector2 position, int size)
        {
            if (size > stackableItem.MaxStackSize)
                throw new Exception("Collectible stack size is greater than maximum stack size");
            Container = stackableItem;
            Position = position;
            Size = size;
        }
        public CollectibleStack(string stackableIngameName, Vector2 position, int size)
        {
            var stackableItem = StackableDataTable.GetInstance().GetStackableByIngameName(stackableIngameName);
            if (size > stackableItem.MaxStackSize)
                throw new Exception("Collectible stack size is greater than maximum stack size");
            if (size < 1)
                throw new Exception("Collectible stack size cannot be less than 1");
            Container = stackableItem;
            Position = position;
            Size = size;
        }

        public void Draw()
        {
            if (Size < 1)
                return;
            (this as ICollectible).CollectibleDraw();
        }

        public void ShowInteractionMessage()
        {
            InteractionMessageBar.GetInstance().PushMessage($"[{GameBody.GetGameInstance().ControlsManager.BindManager.Interact.Key}] to " + InteractionMessage + IngameName + $" x{Size}");
        }

        public void Interact()
        {
            if (Size < 1)
                return;
            bool isStored = GameBody.GetGameInstance().MainCharacter.Inventory.StoreCollectibleStack(this, out _);
            if (Size < 1 || isStored)
            {
                GameBody.GetGameInstance().SceneManager.CurrentRoom.ObjectsYSorted.Remove(this);
                GameBody.GetGameInstance().SceneManager.CurrentRoom.Interactives.Remove(this);
            }
        }

        public void Merge(CollectibleStack collectibletack, out int mergedAmount)
        {
            mergedAmount = 0;
            int availableAmount1 = MaxSize - Size;
            int availableAmount2 = collectibletack.Size;
            mergedAmount = Math.Min(availableAmount1, availableAmount2);
            Size += mergedAmount;
            collectibletack.Size -= mergedAmount;
        }
    }
}
