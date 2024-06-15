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
    public class RoomPortalSprite
    {
        private readonly Texture2D _texture;
        public Vector2 Position { get; protected set; }
        public Vector2 Origin { get; protected set; }
        public Vector2 CenterCords { get; protected set; }
        public Vector2 ExitPosition { get; protected set; }
        public Room ExitRoom { get; protected set; }
        public Rectangle MovementCollider;
        public readonly Room ParentRoomReference;
        private bool _isActive;
        public RoomPortalSprite(Texture2D texture, Vector2 position, Room parentRoomReference)
        {
            _texture = texture;
            Position = position;
            Origin = new(_texture.Width / 2, texture.Height / 2);
            CenterCords = Position + Origin;
            MovementCollider = new((int)Position.X - (int)Origin.X, (int)Position.Y - (int)Origin.Y, _texture.Width, _texture.Height);
            ParentRoomReference = parentRoomReference;
            _isActive = true;
            Debug.WriteLine(parentRoomReference.OrderNumber.ToString());
        }
        public void Update()
        {
            if (MovementCollider.Intersects(GameBody.MainCharacter.MovementCollider) && _isActive)
            {
                Debug.WriteLine("Found intersection at" + ParentRoomReference.OrderNumber.ToString());
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
        private Room GetPreviousRoom()
        {
            return ParentRoomReference.ParentLevelReference.BuiltFloor[ParentRoomReference.OrderNumber - 1];
        }
        private Room GetNextRoom()
        {
            return ParentRoomReference.ParentLevelReference.BuiltFloor[ParentRoomReference.OrderNumber + 1];
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
            Debug.WriteLine("teleporting from" + GameBody.SceneManager.CurrentRoom.OrderNumber.ToString());
            ParentRoomReference.DeactivatePortals();
            ExitRoom.DeactivatePortals();
            GameBody.SceneManager.GoToRoom(ExitRoom.OrderNumber);
            GameBody.MainCharacter.SetPosition(ExitPosition);
            Debug.WriteLine("to" + GameBody.SceneManager.CurrentRoom.OrderNumber.ToString());
            ParentRoomReference.ActivatePortals(2000);
            ExitRoom.ActivatePortals(2000);
            GC.Collect();
        }
    }
}
