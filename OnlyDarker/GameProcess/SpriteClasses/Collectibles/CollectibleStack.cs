using OnlyDarker.GameProcess.Interfaces;
using System;
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

        public bool IsExpired => Container.IsExpired;

        public string IngameName => Container.IngameName + $" ({Size})";

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

        public void Collect() { }

        public void Draw()
        {
            (this as ICollectible).CollectibleDraw();
        }

        public void Interact()
        {
            if (GameBody.GetGameInstance().MainCharacter.Inventory.TryStore(this, out _))
            {
                GameBody.GetGameInstance().SceneManager.CurrentRoom.ObjectsYSorted.Remove(this);
                GameBody.GetGameInstance().SceneManager.CurrentRoom.Interactives.Remove(this);
            }
        }

        public void Merge(CollectibleStack collectibletack, out int leftover)
        {
            Size += collectibletack.Size;
            leftover = 0;
            if (Size > MaxSize)
                leftover = Size - MaxSize;
        }
    }
}
