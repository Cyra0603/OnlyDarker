using OnlyDarker.CommonUsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess.SpriteClasses
{
    public class RoomGatesSprite
    {
        private readonly Texture2D _texture;
        public Vector2 Position { get; protected set; }
        public Vector2 Origin { get; protected set; }
        public Vector2 ExitPosition;
        public Rectangle MovementCollider;
        private Room _parentRoomReference;
        private readonly bool _isNextRoomGate;
        public RoomGatesSprite(Texture2D texture, Vector2 position, Room parentRoomReference, bool isNextRoomGate)
        {
            _texture = texture;
            Position = position;
            Origin = new(_texture.Width / 2, texture.Height / 2);
            MovementCollider = new((int)Position.X, (int)Position.Y, _texture.Width, _texture.Height);
            _isNextRoomGate = isNextRoomGate;
            _parentRoomReference = parentRoomReference;
        }
        public void Draw()
        {
            GlobalUse.SpriteBatch.Draw(_texture, Position, null, Color.White, 0F, Origin, 1F, SpriteEffects.None, 0.9F);
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
            return _parentRoomReference.ParentLevelReference.BuiltFloor[_parentRoomReference.OrderNumber - 1];
        }
        private Room GetNextRoom()
        {
            return _parentRoomReference.ParentLevelReference.BuiltFloor[_parentRoomReference.OrderNumber + 1];
        }
    }
}
