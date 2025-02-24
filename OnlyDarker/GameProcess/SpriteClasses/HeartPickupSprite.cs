using OnlyDarker.CommonUsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace OnlyDarker.GameProcess.SpriteClasses
{
    public class HeartPickupSprite : ICollectible,IMyUpdateable,IYSortable
    {
        public Texture2D Texture { get; }
        public Vector2 Position { get; set; }
        private Vector2 _finalPosition;
        private const string _ingameName = "Heart";
        public string IngameName => _ingameName;
        public string TextureFileName { get; }
        public string Description { get; }
        public bool IsExpired { get; private set; } = false;
        public Timer _spawnTimer;
        public Rectangle MovementCollider => new((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);

        public HeartPickupSprite(Texture2D texture, Vector2 spawnPosition, Vector2 finalPosition)
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
            if (GameBody.GetGameInstance().MainCharacter.Stats.HealthPoints >= GameBody.GetGameInstance().MainCharacter.Stats.MaxHealthPoints)
            {
                return;
            }
            GameBody.GetGameInstance().MainCharacter.Stats.HealthPoints++;
            GameBody.GetGameInstance().SceneManager.CurrentRoom.ObjectsYSorted.Remove(this);
            IsExpired = true;
        }

        public void Update(float elapsedMilliseconds)
        {
            _spawnTimer.Update(elapsedMilliseconds);
            if(Position != _finalPosition && _spawnTimer.TimeLeft > 0)
            {
                Position = Vector2.SmoothStep(Position, _finalPosition, 0.1F);
            }
            if (_spawnTimer.TimeLeft <= 0 && MovementCollider.Intersects(GameBody.GetGameInstance().MainCharacter.MovementCollisionAura))
                Collect();
            (this as ICollectible).ManageCollisions();
        }

        public void Draw()
        {
            (this as ICollectible).CollectibleDraw();
        }
    }
}
