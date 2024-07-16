
using Microsoft.Win32;
using Microsoft.Xna.Framework.Graphics;
using OnlyDarker.CommonUsing;
using OnlyDarker.CommonUsing.Rendering;
using OnlyDarker.GameProcess;
using OnlyDarker.GameProcess.SpriteClasses;
using OnlyDarker.IngameMenu;
using OnlyDarker.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace OnlyDarker
{
    public class GameBody : Game
    {
        private static GameBody _gameInstance;
        private MainCanvas _mainCanvas;
        public readonly GraphicsDeviceManager GraphicsManager;
        public SceneManager SceneManager { get; private set; }
        public ControlsManager ControlsManager { get; private set; }
        public BindManager BindManager { get; private set; }
        public TextureMapper TextureMapper { get; private set; }
        public Menu Menu { get; private set; }
        public Character MainCharacter { get; private set; } = null;
        private CharacterHealthbar _characterHealthbar;
        private CharacterStaminaBar _staminaBar;
        private StatsBar _statsBar;
        private CurrentFloorBar _currentFloorBar;
        private InteractionMessageBar _interactionMessageBar;
        private BossHPBar _bossHPBar;
        private Minimap _minimap;
        public static Texture2D EmptyTexture { get; private set; }
        public List<EffectAnimationManager> EffectAnimationManagers { get; private set; } = new();
        public List<DamageNumberAnimationManager> DamageNumberAnimationManagers { get; private set; } = new();
        public List<ProjectileSprite> ProjectileSprites { get; private set; } = new();
        private GameState _gameState;
        private float _fixedElapsedTimeMilliseconds;
        private float _fixedElapsedTime
        {
            get => _fixedElapsedTimeMilliseconds;
            set
            {
                _fixedElapsedTimeMilliseconds = value;
                if (_fixedElapsedTimeMilliseconds >= FIXED_TIME_STEP)
                    _timeElapsed.Invoke(_fixedElapsedTimeMilliseconds);
            }
        }
        private delegate void TimeElapsed(float milliseconds);
        private event TimeElapsed _timeElapsed;
        public const float FIXED_TIME_STEP = 7.8125F;
        private string _netgraph = "0";
        private int FPS = 0;
        private Vector2 _netgraphPosition;
        private Matrix _cameraView;
        private float _cameraZoom = 2F;
        private float _collectiblesTimeAccumulator = 0F;

        public GameBody()
        {
            GraphicsManager = new GraphicsDeviceManager(this);
            GlobalUse.Content = Content;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _gameInstance = this;
        }
        public static GameBody GetGameInstance()
        {
            if (_gameInstance is not null)
                return _gameInstance;
            else throw new Exception("_gameInstance returned null");
        }
        protected override void Initialize()
        {
            GlobalUse.WindowSize = new(1920, 1080);

            GraphicsManager.PreferredBackBufferWidth = GlobalUse.WindowSize.X;

            GraphicsManager.PreferredBackBufferHeight = GlobalUse.WindowSize.Y;

            GlobalUse.Arial = Content.Load<SpriteFont>("Fonts/Arial");

            GlobalUse.MainFont = Content.Load<SpriteFont>("Fonts/MainFont");

            _netgraphPosition = new Vector2(GraphicsManager.PreferredBackBufferWidth - GlobalUse.Arial.MeasureString(_netgraph).X, 0);

            GraphicsManager.SynchronizeWithVerticalRetrace = false;

            IsFixedTimeStep = false;

            RasterizerState rasterizerState = new()
            {
                CullMode = CullMode.None
            };

            GraphicsDevice.RasterizerState = rasterizerState;

            GraphicsManager.ApplyChanges();

            EmptyTexture = new Texture2D(GraphicsManager.GraphicsDevice, 1, 1);
            EmptyTexture.SetData(new[] { Color.White });

            TextureMapper = TextureMapper.GetInstance();

            _timeElapsed += FixedTimeStepUpdate;

            SceneManager = new(new Level(Floor.One));

            _mainCanvas = new(GraphicsManager.GraphicsDevice, GraphicsManager.PreferredBackBufferWidth, GraphicsManager.PreferredBackBufferHeight);

            _mainCanvas.SetDestinationRectangle();

            MainCharacter = new(
                GlobalUse.Content.Load<Texture2D>("Character/MainCharacter"),
                GlobalUse.Content.Load<Texture2D>("Character/MainCharacterHand"),
                SceneManager.CurrentRoom._tiles[2, 2],
                new(1F,24F));

            MainCharacter.SetRoomBounds(SceneManager.CurrentRoom.RoomSize, SceneManager.CurrentRoom.TileSize);

            SceneManager.CurrentLevel.SetExplorationStates(SceneManager.CurrentRoom);

            BindManager = BindManager.GetInstance();

            BindManager.TogglePause.KeyPressed += GamePause;

            BindManager.ToggleDebug.KeyPressed += GlobalUse.ToggleDebugMode;

            ControlsManager = new ControlsManager(BindManager);

            Menu = Menu.GetInstance();

            foreach (var room in SceneManager.CurrentLevel.BuiltFloor)
            {
                room.ObjectsYSorted.Add(MainCharacter);
            }

            _characterHealthbar = new(GlobalUse.Content.Load<Texture2D>("UI/Heart"));

            _statsBar = new();

            _currentFloorBar = new();

            _minimap = new(GraphicsManager.GraphicsDevice);

            _staminaBar = new(GraphicsManager.GraphicsDevice);

            _interactionMessageBar = InteractionMessageBar.GetInstance();

            _bossHPBar = BossHPBar.GetInstance();

            DamageType.Poke.ToString();

            UpdateFPS();

            UpdateMinimap();

            ControlsManager.CharacterInputsDisabled(false);

            _gameState = GameState.IsRunning;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            GlobalUse.SpriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            CheckWindowFocus();
            ControlsManager.UpdateInputs();
            if (_gameState == GameState.IsRunning)
            {
                SceneManager.CurrentRoom.SortObjectsByY();
                _collectiblesTimeAccumulator += (float)gameTime.ElapsedGameTime.TotalSeconds;
                _staminaBar.Update((float)gameTime.ElapsedGameTime.TotalMilliseconds);
                _fixedElapsedTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            }
            if (_gameState == GameState.Paused)
            {
                Menu.Update();
            }
            base.Update(gameTime);
        }

        private void CheckWindowFocus()
        {
            if (!this.IsActive && _gameState != GameState.Paused)
            {
                _gameState = GameState.Paused;
                Menu.Show();
            }
        }

        private void FixedTimeStepUpdate(float milliseconds)
        {
            foreach (var mngr in EffectAnimationManagers)
            {
                if (mngr.IsActive)
                    mngr.Update(milliseconds);
            }
            foreach (var mngr in DamageNumberAnimationManagers)
            {
                if (mngr.IsActive)
                    mngr.Update(milliseconds);
            }
            foreach (var proj in ProjectileSprites)
            {
                proj.Update(milliseconds);
            }
            EffectAnimationManagers.RemoveAll(mngr => !mngr.IsActive);
            DamageNumberAnimationManagers.RemoveAll(mngr => !mngr.IsActive);
            ProjectileSprites.RemoveAll(proj => proj.Lifetime.TimeLeft <= 0);
            _interactionMessageBar.Update();
            ControlsManager.UpdatePlayerControls();
            MainCharacter.Update(milliseconds);
            CalculateCameraView();
            SceneManager.CurrentRoom.Update(milliseconds);
            SceneManager.CurrentRoom.UpdateObstaclesTransparency(milliseconds);
            _fixedElapsedTime = 0;
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
            DebugModeDraw();
            //foreach (var mngr in EffectAnimationManagers)
            //{
            //    if (mngr.IsActive)
            //        mngr.Draw();
            //}
            Span<EffectAnimationManager> eamAsSpan = CollectionsMarshal.AsSpan(EffectAnimationManagers);
            for (int i = 0; i < eamAsSpan.Length; i++)
            {
                var mngr = eamAsSpan[i];
                mngr.Draw();
            }
            //foreach (var mngr in DamageNumberAnimationManagers)
            //{
            //    if (mngr.IsActive)
            //        mngr.Draw();
            //}
            Span<DamageNumberAnimationManager> dmamAsSpan = CollectionsMarshal.AsSpan(DamageNumberAnimationManagers);
            for (int i = 0; i < dmamAsSpan.Length; i++)
            {
                var mngr = dmamAsSpan[i];
                mngr.Draw();
            }
            //foreach (var proj in ProjectileSprites)
            //{
            //    proj.Draw();
            //}
            Span<ProjectileSprite> projectilesAsSpan = CollectionsMarshal.AsSpan(ProjectileSprites);
            for (int i = 0; i < projectilesAsSpan.Length; i++)
            {
                var proj = projectilesAsSpan[i];
                proj.Draw();
            }
            //foreach (var entity in SceneManager.CurrentRoom.Damageables)
            //{
            //    if (entity.HealthPoints > 0)
            //        entity.DrawHPBar();
            //}
            Span<IDamageable> damageablesAsSpan = CollectionsMarshal.AsSpan(SceneManager.CurrentRoom.Damageables);
            for (int i = 0; i < damageablesAsSpan.Length; i++)
            {
                var damageable = damageablesAsSpan[i];
                if (damageable.HealthPoints > 0)
                    damageable.DrawHPBar();
            }
            GlobalUse.SpriteBatch.End();
            _minimap.RenderMinimap();

            _mainCanvas.Deactivate();

            _mainCanvas.Draw(GlobalUse.SpriteBatch);

            _characterHealthbar.StandaloneDraw();
            GlobalUse.SpriteBatch.Begin(blendState: BlendState.AlphaBlend);
            _minimap.Draw();
            _statsBar.Draw();
            _currentFloorBar.Draw();
            _staminaBar.Draw();
            _interactionMessageBar.Draw();
            _bossHPBar.Draw();
            GlobalUse.SpriteBatch.End();
            if (_gameState == GameState.Paused)
            {
                GlobalUse.SpriteBatch.Begin(blendState: BlendState.AlphaBlend);
                GlobalUse.SpriteBatch.Draw(EmptyTexture, new Rectangle(0, 0, GlobalUse.WindowSize.X, GlobalUse.WindowSize.Y), Color.Black * 0.5F);
                Menu.Draw();
                GlobalUse.SpriteBatch.End();
            }
            base.Draw(gameTime);

            ShowFPS();
            FPS++;
        }

        private void DebugModeDraw()
        {
            if (GlobalUse.IsDebugMode)
            {
                foreach (var hitbox in SceneManager.CurrentRoom.RoomColliders)
                {
                    DrawRectangleOutline(hitbox, Color.Red, 2);
                }
                foreach (var bounds in SceneManager.CurrentRoom.ObstaclesBounds)
                {
                    DrawRectangleOutline(bounds, Color.Black, 2);
                }
                foreach (var bounds in SceneManager.CurrentRoom.TempRectDrawList)
                {
                    DrawRectangleOutline(bounds, Color.Black, 2);
                }
                foreach (var proj in ProjectileSprites)
                {
                    DrawRectangleOutline(proj.HurtBox, Color.Black, 2);
                }
                DrawRectangleOutline(MainCharacter.MovementCollider, Color.Black);
                DrawRectangleOutline(MainCharacter.MovementCollisionAura, Color.Black);
                DrawRectangleOutline(MainCharacter.InteractionAura, Color.Yellow);
                DrawRectangleOutline(new(MainCharacter.Position.ToPoint(), new(2, 2)), Color.Red);
                foreach (var portal in SceneManager.CurrentRoom.Portals)
                {
                    DrawRectangleOutline(portal.MovementCollider, Color.Yellow);
                }
                foreach (var entity in SceneManager.CurrentRoom.Damageables)
                {
                    DrawRectangleOutline(entity.BodyHitbox, Color.Yellow);
                }
                DrawRectangleOutline(MainCharacter.BodyHitbox, Color.Red, 2);
            }
        }

        private void CalculateCameraView()
        {
            var lx = GlobalUse.WindowSize.X / 2 / _cameraZoom - MainCharacter.Position.X;
            var ly = GlobalUse.WindowSize.Y / 2 / _cameraZoom - MainCharacter.Position.Y;
            _cameraView = Matrix.CreateTranslation(lx, ly, 0F) * Matrix.CreateScale(_cameraZoom, _cameraZoom, 0);
        }
        private void ShowFPS()
        {
            GlobalUse.SpriteBatch.Begin();
            GlobalUse.SpriteBatch.DrawString(GlobalUse.Arial, _netgraph, _netgraphPosition, Color.White, 0F, Vector2.Zero, 0.3F, SpriteEffects.None, 0F);
            GlobalUse.SpriteBatch.End();
        }
        private async void UpdateFPS()
        {
            while (_gameInstance is not null)
            {
                _netgraph = FPS.ToString();
                FPS = 0;
                await Task.Delay(1000);
            }
        }
        public void UpdateMinimap()
        {
            _minimap.Update();
        }
        public void GamePause()
        {
            if (_gameState == GameState.IsRunning)
            {
                _gameState = GameState.Paused;
                Menu.Show();
                return;
            }
        }
        public void GameUnpause()
        {
            if (_gameState == GameState.Paused)
            {
                _gameState = GameState.IsRunning;
                return;
            }
        }
        public float GetSwayFunctionValue()
        {
            return _collectiblesTimeAccumulator;
        }
        public static void DrawRectangleOutline(Rectangle rect, Color color, int borderWidth = 1)
        {
            GlobalUse.SpriteBatch.Draw(EmptyTexture, new Rectangle(rect.Left, rect.Top, borderWidth, rect.Height), color);
            GlobalUse.SpriteBatch.Draw(EmptyTexture, new Rectangle(rect.Right, rect.Top, borderWidth, rect.Height), color);
            GlobalUse.SpriteBatch.Draw(EmptyTexture, new Rectangle(rect.Left, rect.Top, rect.Width, borderWidth), color);
            GlobalUse.SpriteBatch.Draw(EmptyTexture, new Rectangle(rect.Left, rect.Bottom, rect.Width + borderWidth, borderWidth), color);
        }
        public void AppToggleFullscreen()
        {
            GraphicsManager.IsFullScreen = !GraphicsManager.IsFullScreen;
            GraphicsManager.ApplyChanges();
        }
    }
}
