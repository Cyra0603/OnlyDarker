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
    public class RoomPortalSprite : IYSortable
    {
        private readonly Texture2D _texture;
        public Vector2 Position { get; set; }
        public Vector2 Origin { get; protected set; }
        public Vector2 CenterCords { get; protected set; }
        public Vector2 ExitPosition { get; protected set; }
        public Direction Direction { get; protected set; }
        public Room ExitRoom { get; protected set; }
        public Rectangle MovementCollider;
        public readonly Room ParentRoomReference;
        private bool _isActive;
        public RoomPortalSprite(Texture2D texture, Vector2 position, Direction portalDirection, Room parentRoomReference)
        {
            _texture = texture;
            Position = position;
            Origin = new(_texture.Width / 2, texture.Height / 2);
            Direction = portalDirection;
            CenterCords = Position + Origin;
            MovementCollider = new((int)Position.X - (int)Origin.X, (int)Position.Y - (int)Origin.Y, _texture.Width, _texture.Height);
            ParentRoomReference = parentRoomReference;
            _isActive = true;
        }
        public void Update()
        {
            if (MovementCollider.Intersects(GameBody.MainCharacter.MovementCollider) && _isActive)
            {
                PlayerTeleport();
            }
        }
        public void Draw()
        {
            GlobalUse.SpriteBatch.Draw(_texture, Position, null, Color.White, 0F, Origin, 1F, SpriteEffects.None, 0.8F);
        }
        public float GetTextureWidth()
        {
            return _texture.Width;
        }
        public float GetTextureHeight()
        {
            return _texture.Height;
        }
        public void ActivatePortal()
        {
            _isActive = true;
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
            ParentRoomReference.DeactivatePortals();
            ExitRoom.DeactivatePortals();
            GameBody.SceneManager.GoToRoom(ExitRoom.GridCords);
            GameBody.MainCharacter.SetPosition(ExitPosition);
            ParentRoomReference.ActivatePortals(2000);
            ParentRoomReference.Updateables.RemoveAll(items => items.IsExpired);
            ExitRoom.ActivatePortals(2000);
            GameBody.ProjectileSprites.Clear();
            GC.Collect();
        }
    }
}
