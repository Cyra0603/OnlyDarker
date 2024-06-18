
using Microsoft.Xna.Framework.Graphics;
using OnlyDarker.CommonUsing;
using OnlyDarker.CommonUsing.Rendering;
using OnlyDarker.GameProcess;
using OnlyDarker.GameProcess.SpriteClasses;
using OnlyDarker.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace OnlyDarker
{
    public class GameBody : Game
    {
        private MainCanvas _mainCanvas;
        private GraphicsDeviceManager _graphics;
        public static SceneManager SceneManager { get; private set; }
        public static BindManager BindManager { get; private set; }
        public static Character? MainCharacter { get; private set; } = null;
        private static CharacterHealthbar _characterHealthbar;
        private static CharacterStaminaBar _staminaBar;
        private static StatsBar _statsBar;
        private static CurrentFloorBar _currentFloorBar;
        private static Texture2D _hitboxTexture;
        private static Texture2D _emptyTexture;
        private static Minimap _minimap;
        public static List<EffectAnimationManager> EffectAnimationManagers { get; private set; } = new();
        private long _fixedElapsedTime = 0; //add fixed elapsed time event

        public const long ONE_TICK = 78125L;
        private string _netgraph = "0";
        private int FPS = 0;
        private Vector2 _netgraphPosition;
        public static Floor CurrentFloorType { get; private set; }
        private Matrix _cameraView;
        private static float _cameraZoom = 0.5F;

        public GameBody()
        {
            _graphics = new GraphicsDeviceManager(this);
            GlobalUse.Content = Content;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            GlobalUse.WindowSize = new(1920, 1080);

            _graphics.PreferredBackBufferWidth = GlobalUse.WindowSize.X;

            _graphics.PreferredBackBufferHeight = GlobalUse.WindowSize.Y;

            GlobalUse.Arial = Content.Load<SpriteFont>("Fonts/Arial");

            GlobalUse.MainFont = Content.Load<SpriteFont>("Fonts/MainFont");

            _netgraphPosition = new Vector2(_graphics.PreferredBackBufferWidth - GlobalUse.Arial.MeasureString(_netgraph).X, 0);

            _graphics.SynchronizeWithVerticalRetrace = false;

            IsFixedTimeStep = false;

            _graphics.ApplyChanges();

            BindManager = new();

            BindManager.ExitApplication.KeyPressed += Exit;

            BindManager.ToggleDebug.KeyPressed += GlobalUse.ToggleDebugMode;

            SceneManager = new(new Level(Floor.One));

            _mainCanvas = new(_graphics.GraphicsDevice, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);

            _mainCanvas.SetDestinationRectangle();

            _hitboxTexture = new Texture2D(_graphics.GraphicsDevice, 1, 1);
            _hitboxTexture.SetData(new Color[] { Color.Red });

            _emptyTexture = new Texture2D(_graphics.GraphicsDevice, 1, 1);
            _emptyTexture.SetData(new[] { Color.White });

            MainCharacter = new(
                GlobalUse.Content.Load<Texture2D>("Character/MainCharacter"),
                GlobalUse.Content.Load<Texture2D>("Character/MainCharacterHand"),
                SceneManager.CurrentRoom._tiles[2, 2]);

            MainCharacter.SetRoomBounds(SceneManager.CurrentRoom.RoomSize, SceneManager.CurrentRoom.TileSize);

            foreach (var room in SceneManager.CurrentLevel.BuiltFloor)
            {
                room.ObjectsYSorted.Add(MainCharacter);
            }

            _characterHealthbar = new(GlobalUse.Content.Load<Texture2D>("UI/Heart"));

            _statsBar = new();

            _currentFloorBar = new();

            _minimap = new(_graphics.GraphicsDevice);

            _staminaBar = new(_graphics.GraphicsDevice);

            UpdateFPS();

            ControlsManager.CharacterInputsDisabled(false);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            GlobalUse.SpriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            _fixedElapsedTime += gameTime.ElapsedGameTime.Ticks;

            SceneManager.CurrentRoom.SortObjectsByY();

            foreach (var mngr in EffectAnimationManagers)
            {
                if(mngr.IsActive)
                    mngr.Update((float)gameTime.ElapsedGameTime.TotalMilliseconds);
            }

            EffectAnimationManagers.RemoveAll(mngr => !mngr.IsActive);

            _staminaBar.Update((float)gameTime.ElapsedGameTime.TotalMilliseconds);

            if (_fixedElapsedTime >= ONE_TICK)
            {
                CalculateCameraView();
                MainCharacter.Update(GlobalUse.TicksToMilliseconds(_fixedElapsedTime));
                SceneManager.CurrentRoom.UpdatePortals();
                SceneManager.CurrentRoom.UpdateObstaclesTransparency(GlobalUse.TicksToMilliseconds(_fixedElapsedTime));
                _fixedElapsedTime = 0;
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            //Rendering

            _mainCanvas.Activate();
            //Background
            GlobalUse.SpriteBatch.Begin(sortMode: SpriteSortMode.BackToFront, blendState: BlendState.AlphaBlend, samplerState: SamplerState.LinearWrap/*, transformMatrix: _cameraView*/);
            SceneManager.CurrentRoom.CurrentBackground.Draw();
            GlobalUse.SpriteBatch.End();
            //MainScene
            GlobalUse.SpriteBatch.Begin(sortMode: SpriteSortMode.Deferred, blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp, transformMatrix: _cameraView);
            SceneManager.CurrentRoom.Draw();
            //MainCharacter.Draw();
            if (GlobalUse.IsDebugMode)
            {
                foreach (var hitbox in SceneManager.CurrentRoom.RoomColliders)
                {
                    DrawRectangleOutline(hitbox, Color.Red, 5);
                }
                foreach (var bounds in SceneManager.CurrentRoom.ObstaclesBounds)
                {
                    DrawRectangleOutline(bounds, Color.Black, 5);
                }
                GlobalUse.SpriteBatch.Draw(_hitboxTexture, MainCharacter.MovementCollider, Color.Blue);
                if (SceneManager.CurrentRoom.PortalBack is not null)
                    GlobalUse.SpriteBatch.Draw(_hitboxTexture, SceneManager.CurrentRoom.PortalBack.MovementCollider, Color.Blue);
                if (SceneManager.CurrentRoom.PortalNext is not null)
                    GlobalUse.SpriteBatch.Draw(_hitboxTexture, SceneManager.CurrentRoom.PortalNext.MovementCollider, Color.Blue);
            }
            foreach (var mngr in EffectAnimationManagers)
            {
                if (mngr.IsActive)
                    mngr.Draw();
            }
            GlobalUse.SpriteBatch.End();
            _mainCanvas.Deactivate();

            _minimap.RenderMinimap();

            //Drawing
            _mainCanvas.Draw(GlobalUse.SpriteBatch);

            _minimap.StandaloneDraw();
            _characterHealthbar.StandaloneDraw();
            GlobalUse.SpriteBatch.Begin();
            ShowFPS();
            _statsBar.DrawStats();
            _currentFloorBar.Draw();
            _staminaBar.Draw();
            GlobalUse.SpriteBatch.End();

            base.Draw(gameTime);
            FPS++;
        }

        private void CalculateCameraView()
        {
            var lx = GlobalUse.WindowSize.X / 2 / _cameraZoom - MainCharacter.Position.X;
            //lx = MathHelper.Clamp(lx, -CurrentRoom.RoomSize.X + GlobalUse.WindowSize.X / _cameraZoom + (CurrentRoom.TileSize.X / 2), CurrentRoom.TileSize.X / 2);
            var ly = GlobalUse.WindowSize.Y / 2 / _cameraZoom - MainCharacter.Position.Y;
            //ly = MathHelper.Clamp(ly, -CurrentRoom.RoomSize.Y + GlobalUse.WindowSize.Y / _cameraZoom + (CurrentRoom.TileSize.Y / 2), CurrentRoom.TileSize.Y / 2);
            _cameraView = Matrix.CreateTranslation(lx, ly, 0F) * Matrix.CreateScale(_cameraZoom, _cameraZoom, 0);
        }
        private void ShowFPS()
        {
            GlobalUse.SpriteBatch.DrawString(GlobalUse.Arial, _netgraph, _netgraphPosition, Color.White, 0F, Vector2.Zero, 0.3F, SpriteEffects.None, 0F);
        }
        private async void UpdateFPS()
        {
            while (MainCharacter is not null)
            {
                _netgraph = FPS.ToString();
                FPS = 0;
                await Task.Delay(1000);
            }
        }

        private static void DrawHitbox(Rectangle hitbox)
        {
            GlobalUse.SpriteBatch.Draw(_hitboxTexture, hitbox, Color.White);
        }
        public static void DrawRectangleOutline(Rectangle rect, Color color, int borderWidth = 1)
        {
            GlobalUse.SpriteBatch.Draw(_emptyTexture, new Rectangle(rect.Left, rect.Top, borderWidth, rect.Height), color);
            GlobalUse.SpriteBatch.Draw(_emptyTexture, new Rectangle(rect.Right, rect.Top, borderWidth, rect.Height), color);
            GlobalUse.SpriteBatch.Draw(_emptyTexture, new Rectangle(rect.Left, rect.Top, rect.Width, borderWidth), color);
            GlobalUse.SpriteBatch.Draw(_emptyTexture, new Rectangle(rect.Left, rect.Bottom, rect.Width, borderWidth), color);
        }
    }
}
