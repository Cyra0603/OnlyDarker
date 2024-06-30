using OnlyDarker.CommonUsing;
using OnlyDarker.GameProcess;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace OnlyDarker.UI
{
    public class Minimap
    {
        public Vector2 Position { get; protected set; }
        public Rectangle Bounds { get; protected set; }
        private Rectangle _backGroundRect;
        private List<Texture2D> _minimapIcons;
        private Texture2D _background;
        private Texture2D _minimapFrame;
        private RenderTarget2D _minimapTarget;
        private readonly GraphicsDevice _graphicsDevice;
        private const int FRAME_OFFSET = 3;
        private static List<Room> CurrentLevel => GameBody.SceneManager.CurrentLevel.BuiltFloor;
        private static Room _сurrentRoom => GameBody.SceneManager.CurrentRoom;
        private float _minimapScale = 1F;
        private Matrix _minimapView;
        private Vector2 _iconPos;
        private Vector2 _framePos;

        public Minimap(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _minimapFrame = GlobalUse.Content.Load<Texture2D>($"UI/MinimapFrame");
            _minimapIcons = LoadMinimapIcons();
            Bounds = new(new(GlobalUse.WindowSize.X - (int)(_minimapFrame.Width * _minimapScale), 0), new((int)(_minimapFrame.Width * _minimapScale), (int)(_minimapFrame.Height * _minimapScale)));
            Position = Bounds.Center.ToVector2();
            _minimapTarget = new(_graphicsDevice, Bounds.Width, Bounds.Height);
            _iconPos = new Vector2(_minimapIcons[^1].Width, _minimapIcons[^1].Height);
            _framePos = new Vector2(_сurrentRoom.GridCords.X * _minimapIcons[^1].Width - _minimapFrame.Width / 2, _сurrentRoom.GridCords.Y * _minimapIcons[^1].Height - _minimapFrame.Height / 2);
            _backGroundRect = new Rectangle(_framePos.ToPoint(), Bounds.Size);
        }
        public void RenderMinimap()
        {
            ActivateRenderTarget();
            DrawMinimap();
            DeactivateRenderTarget();
        }
        private void DrawMinimap()
        {
            GlobalUse.SpriteBatch.Begin(blendState: BlendState.AlphaBlend, transformMatrix: _minimapView);
            GlobalUse.SpriteBatch.Draw(GameBody.EmptyTexture, _backGroundRect, Color.Black * 0.8F);
            foreach (var room in CurrentLevel)
            {
                int i = (int)room.InstanceRoomType - 1;
                GlobalUse.SpriteBatch.Draw(_minimapIcons[i], _iconPos * room.GridCords.ToVector2(), Color.White);
            }
            GlobalUse.SpriteBatch.Draw(_minimapIcons[^1], _iconPos * _сurrentRoom.GridCords.ToVector2(), Color.White);
            GlobalUse.SpriteBatch.Draw(_minimapFrame, _framePos, Color.White);
            GlobalUse.SpriteBatch.End();
        }

        private static List<Texture2D> LoadMinimapIcons()
        {
            var textures = new List<Texture2D>();
            foreach (var roomType in Enum.GetNames<RoomType>())
            {
                if (roomType == "Empty") continue;
                textures.Add(GlobalUse.Content.Load<Texture2D>($"MinimapIcons/{roomType}Icon"));
            }
            textures.Add(GlobalUse.Content.Load<Texture2D>($"MinimapIcons/CurrentRoomIcon"));
            return textures;
        }
        public void SetScale(float scale = 1F)
        {
            _minimapScale = scale;
        }
        public void BoundsRescale()
        {
            Bounds = new(new(GlobalUse.WindowSize.X - (int)(_minimapFrame.Width * _minimapScale), 0), new((int)(_minimapFrame.Width * _minimapScale), (int)(_minimapFrame.Height * _minimapScale)));
        }
        public void RenderTargetRescale()
        {
            _minimapTarget = new(_graphicsDevice, Bounds.Width, Bounds.Height);
        }
        public void Update()
        {
            _minimapView = CalculateMinimapView();
            _framePos = new Vector2(_сurrentRoom.GridCords.X * _minimapIcons[^1].Width - _minimapFrame.Width / 2, _сurrentRoom.GridCords.Y * _minimapIcons[^1].Height - _minimapFrame.Height / 2);
            _backGroundRect = new Rectangle(_framePos.ToPoint(), Bounds.Size);
        }
        private Matrix CalculateMinimapView()
        {
            var lx = Bounds.Width / 2 - GameBody.SceneManager.CurrentRoom.GridCords.X * _minimapIcons[0].Width;
            var ly = Bounds.Height / 2 - GameBody.SceneManager.CurrentRoom.GridCords.Y * _minimapIcons[0].Height;
            return Matrix.CreateTranslation(lx, ly, 0F);
        }
        private void ActivateRenderTarget()
        {
            _graphicsDevice.SetRenderTarget(_minimapTarget);
            _graphicsDevice.Clear(Color.Transparent);
        }
        private void DeactivateRenderTarget()
        {
            _graphicsDevice.SetRenderTarget(null);
            _graphicsDevice.Clear(Color.Transparent);
        }
        public void Draw()
        {
            GlobalUse.SpriteBatch.Draw(_minimapTarget, Bounds, Color.White);
        }
    }
}
