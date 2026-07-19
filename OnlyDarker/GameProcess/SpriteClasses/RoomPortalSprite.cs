using OnlyDarker.CommonUsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess.SpriteClasses
{
    public class RoomPortalSprite : INonSortable, IInteractive
    {
        public Texture2D Texture { get; }
        public Texture2D BrokenTexture { get; }
        public Vector2 Position { get; set; }
        public Vector2 Origin { get; set; }
        public Vector2 CenterCords { get; set; }
        public Vector2 ExitPosition { get; set; }
        public Direction Direction { get; set; }
        public Room ExitRoom { get; set; }
        public Rectangle MovementCollider { get; set; }
        public readonly Room ParentRoomReference;
        public bool IsExpired { get; private set; } = false;
        public bool IsInteractive { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsBroken { get; set; }

        public string IngameName => "broken portal";

        public string InteractionMessage => "to repair ";

        private bool _isActive;
        public RoomPortalSprite(Vector2 position, Direction portalDirection, Room parentRoomReference)
        {
            Texture = TextureMapper.GetInstance().PortalTexture;
            BrokenTexture = TextureMapper.GetInstance().BrokenPortalTexture;
            Position = position;
            Origin = new(Texture.Width / 2, Texture.Height / 2);
            Direction = portalDirection;
            CenterCords = Position + Origin;
            MovementCollider = new((int)Position.X - (int)Origin.X, (int)Position.Y - (int)Origin.Y, Texture.Width, Texture.Height);
            ParentRoomReference = parentRoomReference;
            IsBlocked = false;
            _isActive = true;
        }
        public void Update()
        {
            if (IsBlocked && !MovementCollider.Intersects(GameBody.GetGameInstance().MainCharacter.MovementCollider))
            {
                UnblockPortal();
            }
            if (_isActive && !IsBlocked && !IsBroken && MovementCollider.Intersects(GameBody.GetGameInstance().MainCharacter.MovementCollider))
            {
                PlayerTeleport();
            }
        }
        public void Draw()
        {
            if(!IsBroken)
                GlobalUse.SpriteBatch.Draw(Texture, Position, null, Color.White, 0F, Origin, 1F, SpriteEffects.None, 0.8F);
            else
                GlobalUse.SpriteBatch.Draw(BrokenTexture, Position, null, Color.White, 0F, Origin, 1F, SpriteEffects.None, 0.8F);
        }
        public float GetTextureWidth()
        {
            return Texture.Width;
        }
        public float GetTextureHeight()
        {
            return Texture.Height;
        }
        public void Break()
        {
            IsBroken = true;
            IsInteractive = true;
        }
        public void Repair()
        {
            IsBroken = false;
            IsInteractive = false;
        }
        public void ActivatePortal()
        {
            _isActive = true;
        }
        public void BlockPortal()
        {
            IsBlocked = true;
        }
        public void UnblockPortal()
        {
            IsBlocked = false;
        }
        public void DeactivatePortal()
        {
            _isActive = false;
        }
        public void SetExitPosition(Vector2 exitPosition)
        {
            ExitPosition = exitPosition;
        }
        public void SetExitRoom(Room exitRoom)
        {
            ExitRoom = exitRoom; 
        }
        private void PlayerTeleport()
        {
            ParentRoomReference.BlockPortals();
            ExitRoom.BlockPortals();
            GameBody.GetGameInstance().SceneManager.GoToRoom(ExitRoom);
            GameBody.GetGameInstance().MainCharacter.SetPosition(ExitPosition);
            GameBody.GetGameInstance().SceneManager.CurrentLevel.SetExplorationStates(ExitRoom);
            ParentRoomReference.Updateables.RemoveAll(items => items.IsExpired);
            ParentRoomReference.ObjectsYSorted.RemoveAll(item => item.IsExpired);
            GC.Collect();
        }

        public void Interact()
        {
            Repair();
            foreach(var portal in ExitRoom.Portals)
            {
                if (portal.Direction == GlobalUse.GetOppositeDirection(Direction))
                    portal.Repair();
            }
        }
    }
}
