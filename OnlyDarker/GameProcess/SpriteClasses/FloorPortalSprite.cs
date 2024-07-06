using OnlyDarker.CommonUsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess.SpriteClasses
{
    public class FloorPortalSprite : INonSortable
    {
        private readonly Texture2D _texture;
        public Vector2 Position { get; set; }
        public Vector2 Origin { get; protected set; }
        public Vector2 CenterCords { get; protected set; }
        public Vector2 ExitPosition { get; protected set; }
        public Rectangle MovementCollider;
        public readonly Room ParentRoomReference;
        public bool IsExpired { get; private set; } = false;
        private bool _isActive;
        public FloorPortalSprite(Texture2D texture, Vector2 position, Room parentRoomReference)
        {
            _texture = texture;
            Position = position;
            Origin = new(_texture.Width / 2, texture.Height / 2);
            CenterCords = Position + Origin;
            MovementCollider = new((int)Position.X - (int)Origin.X, (int)Position.Y - (int)Origin.Y, _texture.Width, _texture.Height);
            ParentRoomReference = parentRoomReference;
            _isActive = true;
        }
        public void Update()
        {
            if (_isActive && MovementCollider.Intersects(GameBody.GetGameInstance().MainCharacter.MovementCollider))
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
        private void PlayerTeleport()
        {
            ParentRoomReference.DeactivatePortals();
            ExitRoom.DeactivatePortals();
            GameBody.GetGameInstance().SceneManager.GoToRoom(ExitRoom);
            GameBody.GetGameInstance().MainCharacter.SetPosition(ExitPosition);
            GameBody.GetGameInstance().SceneManager.CurrentLevel.SetExplorationStates(ExitRoom);
            ParentRoomReference.ActivatePortals(2000);
            ParentRoomReference.Updateables.RemoveAll(items => items.IsExpired);
            ParentRoomReference.ObjectsYSorted.RemoveAll(item => item.IsExpired);
            ExitRoom.ActivatePortals(2000);
            GameBody.GetGameInstance().ProjectileSprites.Clear();
            GC.Collect();
        }
    }
}
