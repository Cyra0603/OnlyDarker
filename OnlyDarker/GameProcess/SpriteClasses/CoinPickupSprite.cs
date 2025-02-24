using OnlyDarker.CommonUsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.GameProcess.SpriteClasses
{
    public class CoinPickupSprite : ICollectible, IMyUpdateable, IYSortable
    {
        public Texture2D Texture { get; }
        public Vector2 Position { get; set; }
        private Vector2 _finalPosition { get; }
        private const string _ingameName = "Coin";
        public string IngameName => _ingameName;
        public string TextureFileName { get; }
        public string Description { get; }
        public bool IsExpired { get; private set; } = false;
        public Timer _spawnTimer;
        public Rectangle MovementCollider => new((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
        public CoinPickupSprite(Texture2D texture, Vector2 spawnPosition, Vector2 finalPosition)
        {
            Texture = texture;
            Position = spawnPosition;
            _finalPosition = finalPosition;
            _spawnTimer = new(500F);
            Description = string.Empty;
            GameBody.GetGameInstance().SceneManager.CurrentRoom.Updateables.Add(this);
            GameBody.GetGameInstance().SceneManager.CurrentRoom.ObjectsYSorted.Add(this);
        }
        private void Collect()
        {
            GameBody.GetGameInstance().MainCharacter.Stats.CoinCount++;
            GameBody.GetGameInstance().SceneManager.CurrentRoom.ObjectsYSorted.Remove(this);
            IsExpired = true;
        }

        public void Update(float elapsedMilliseconds)
        {
            _spawnTimer.Update(elapsedMilliseconds);
            if (Position != _finalPosition && _spawnTimer.TimeLeft > 0)
            {
                Position = Vector2.SmoothStep(Position, _finalPosition, 0.1F);
            }
            if (_spawnTimer.TimeLeft <= 0 && MovementCollider.Intersects(GameBody.GetGameInstance().MainCharacter.MovementCollisionAura))
                Collect();
            (this as ICollectible).ManageCollisions();
        }

        public void Draw()
        {
            var offset = Math.Abs((float)Math.Sin(GameBody.GetGameInstance().GetSwayFunctionValue()));
            var shadowRect = (this as ICollectible).ShadowRect;
            shadowRect.Width = (int)(shadowRect.Width * offset);
            shadowRect.X -= (int)(shadowRect.Width / 2 * offset);
            var newX = Position.X + ICollectible.SwayOffset.X;
            var newY = Position.Y + ICollectible.SwayOffset.Y;
            var textureRect = new Rectangle((int)newX, (int)newY, (int)(Texture.Width * offset), Texture.Height);
            textureRect.X -= (int)(Texture.Width / 2 * offset);
            GlobalUse.SpriteBatch.Draw(TextureMapper.GetInstance().ShadowTexture, shadowRect, Color.White);
            GlobalUse.SpriteBatch.Draw(Texture, textureRect, Color.White);
        }
    }

}
