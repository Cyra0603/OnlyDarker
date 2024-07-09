using OnlyDarker.CommonUsing;
using System.Linq;


namespace OnlyDarker.GameProcess.SpriteClasses
{
    public class SpriteStandartObstacle : IYSortable
    {
        private readonly Texture2D _texture;
        public Vector2 Position { get; set; }
        public Vector2 Origin { get; protected set; }
        public Rectangle MovementCollider { get; private set; }
        public Rectangle Bounds { get; private set; }
        private Rectangle _nonTransparentBounds;
        private float _transparencyFadeTime = 250F;
        private Timer _transparencyTimer = new(0);
        private Color _shadowColor = new(Color.Black, 0.20F);
        public bool IsExpired { get; private set; } = false;
        public SpriteStandartObstacle(Texture2D texture, SpriteStandartTile parentTile)
        {
            _texture = texture;
            Origin = new(texture.Width / 2, texture.Height / 2);
            Position = new(parentTile.Position.X, parentTile.Position.Y - texture.Width / 2);
            MovementCollider = new(new Point(
                (int)parentTile.Position.X - (int)parentTile.GetTextureWidth() / 2,
                (int)parentTile.Position.Y - (int)parentTile.GetTextureHeight() / 2),
                new((int)parentTile.GetTextureWidth(),
                (int)parentTile.GetTextureHeight())
                );
            Bounds = new(new(Position.ToPoint().X - _texture.Width / 2, Position.ToPoint().Y - _texture.Height / 2), new(_texture.Width, _texture.Height));
            _nonTransparentBounds = new(new(Bounds.Size.X - MovementCollider.Size.X, Bounds.Size.Y - MovementCollider.Size.Y), MovementCollider.Size);
        }
        public void Draw()
        {
            if (IntersectsEssentialObjects())
            {
                _transparencyTimer.TimeLeft = _transparencyFadeTime;
                GlobalUse.SpriteBatch.Draw(_texture, Position, null, Color.White * 0.5F, 0F, Origin, 1F, SpriteEffects.None, 0.4F);
                GlobalUse.SpriteBatch.Draw(_texture, MovementCollider, _nonTransparentBounds, Color.White);
            }
            else
                GlobalUse.SpriteBatch.Draw(_texture, Position, null, Color.White * (1F - (0.5F * (_transparencyTimer.TimeLeft / _transparencyFadeTime))), 0F, Origin, 1F, SpriteEffects.None, 0.4F);
            if (_transparencyTimer.TimeLeft > 0.1)
                GlobalUse.SpriteBatch.Draw(_texture, MovementCollider, _nonTransparentBounds, Color.White);
        }

        private bool IntersectsEssentialObjects()
        {
            return Bounds.Intersects(
                GameBody.GetGameInstance().MainCharacter.MovementCollider) 
                || GameBody.GetGameInstance().SceneManager.CurrentRoom.Portals.Any(portal => portal.MovementCollider.Intersects(Bounds)
                || GameBody.GetGameInstance().SceneManager.CurrentRoom.Damageables.Any(entity => entity.BodyHitbox.Intersects(Bounds)));
        }

        public void DrawShadow()
        {
            GlobalUse.SpriteBatch.Draw(_texture, Position, null, _shadowColor, -0.4F, Origin * 1.5F, 1F, SpriteEffects.None, 0.4F);
        }
        public void UpdateTransparencyTimer(float elapsedMilliseconds)
        {
            _transparencyTimer.Update(elapsedMilliseconds);
        }
    }
}
