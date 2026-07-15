
using Microsoft.Win32;
using Microsoft.Xna.Framework.Graphics;
using OnlyDarker.CommonUsing;
using OnlyDarker.CommonUsing.Rendering;
using OnlyDarker.GameProcess;
using OnlyDarker.GameProcess.SpriteClasses;
using OnlyDarker.GameProcess.SpriteClasses.Collectibles;
using OnlyDarker.IngameMenu;
using OnlyDarker.PlayerClasses;
using OnlyDarker.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
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
        private LoadingScreen _loadingScreen;
        private bool _isLoading;
        public static Texture2D EmptyTexture { get; private set; }
        public List<EffectAnimationManager> EffectAnimationManagers { get; private set; } = new();
        public List<DamageNumberAnimationManager> DamageNumberAnimationManagers { get; private set; } = new();
        public List<Vector2> CoinPositions;
        public int CoinsToSpawn;
        public Vector2 CoinsSpawnPosition => new(GlobalUse.WindowSize.X / 2, GlobalUse.WindowSize.Y / 2);
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

        //Diagnostics
        private Process _currentProcess;
        private Pinger _pinger;
        public int Ping => _pinger.Ping;
        private Stopwatch _CPUFrameTimer;
        private long _CPUFrameTime;
        public long DrawnCPUFrameTime { get; private set; }
        private Stopwatch _GPUFrameTimer;
        private long _GPUFrameTime;
        public long DrawnGPUFrameTime { get; private set; }
        public double AllocatedMemoryInMB { get; private set; }
        private float _countersTimeStep = 50F;
        private float _counterElapsedTime = 0F;
        private float _memoryCounterTimeStep = 1000F;
        private float _memoryCounterElapsedTime = 0F;
        private string _netgraph = "0";
        private int FPS = 0;
        private Vector2 _netgraphPosition;
        //View
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

            _CPUFrameTimer = new Stopwatch();
            _CPUFrameTimer.Start();
            _GPUFrameTimer = new Stopwatch();
            _GPUFrameTimer.Start();

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

            StackableDataTable.LoadContent();

            MainCharacter = new(
                GlobalUse.Content.Load<Texture2D>("Character/MainCharacter"),
                GlobalUse.Content.Load<Texture2D>("Character/MainCharacterHand"),
                new SpriteSheet(GlobalUse.Content.Load<Texture2D>("Character/AnimationSpriteSheets/CharacterWalkingAnimation"), 32, 50, 12), //temp
                SceneManager.CurrentRoom._tiles[2, 2],
                new(1F, 24F));

            MainCharacter.SetRoomBounds(SceneManager.CurrentRoom.RoomSize, SceneManager.CurrentRoom.TileSize);

            SceneManager.CurrentLevel.SetExplorationStates(SceneManager.CurrentRoom);

            BindManager = BindManager.GetInstance();

            BindManager.TogglePause.KeyPressed += GamePause;

            BindManager.ToggleDebug.KeyPressed += GlobalUse.ToggleDebugMode;

            BindManager.SimulateLoading.KeyPressed += RunLoadingSimulation;

            ControlsManager = new ControlsManager(BindManager);

            Menu = Menu.GetInstance();

            foreach (var room in SceneManager.CurrentLevel.BuiltFloor)
            {
                room.ObjectsYSorted.Add(MainCharacter);
            }

            CoinPositions = new();

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

            _currentProcess = Process.GetCurrentProcess();

            _pinger = new();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            GlobalUse.SpriteBatch = new SpriteBatch(GraphicsDevice);
            //StackableDataTable.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            _CPUFrameTimer.Restart();
            if (_isLoading)
            {
                _counterElapsedTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                _memoryCounterElapsedTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                _CPUFrameTime = _CPUFrameTimer.ElapsedMilliseconds;
                UpdateDiagnosers();
                return;
            }
            CheckWindowFocus();
            ControlsManager.GetInputStates();
            ControlsManager.UpdateInputs();
            if (_gameState == GameState.IsRunning)
            {
                SceneManager.CurrentRoom.SortObjectsByY();
                _collectiblesTimeAccumulator += (float)gameTime.ElapsedGameTime.TotalSeconds;
                _staminaBar.Update((float)gameTime.ElapsedGameTime.TotalMilliseconds);
                _fixedElapsedTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                if (MainCharacter.Inventory.IsActive)
                {
                    MainCharacter.Inventory.Update();
                }
            }
            if (_gameState == GameState.Paused)
            {
                Menu.Update();
            }
            ControlsManager.SaveInputStates();
            base.Update(gameTime);
            _counterElapsedTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            _memoryCounterElapsedTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            _CPUFrameTime = _CPUFrameTimer.ElapsedMilliseconds;
            UpdateDiagnosers();
        }

        private void CheckWindowFocus()
        {
            if (!this.IsActive && _gameState != GameState.Paused)
            {
                _gameState = GameState.Paused;
                Menu.Show();
            }
        }
        private void UpdateDiagnosers()
        {
            if (_counterElapsedTime >= _countersTimeStep)
            {
                DrawnCPUFrameTime = _CPUFrameTime;
                DrawnGPUFrameTime = _GPUFrameTime;
                _counterElapsedTime = 0;
            }
            if (_memoryCounterElapsedTime >= _memoryCounterTimeStep)
            {
                AllocatedMemoryInMB = _currentProcess.WorkingSet64 / 1048576;
                _memoryCounterElapsedTime = 0;
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
            //foreach (var proj in ProjectileSprites)
            //{
            //    proj.Update(milliseconds);
            //}
            if (CoinsToSpawn > 0)
            {
                float randomx = RandomNumberGenerator.GetInt32(-50, 50);
                float randomy = RandomNumberGenerator.GetInt32(-50, 50);
                Vector2 newPos = new(CoinsSpawnPosition.X + randomx, CoinsSpawnPosition.Y + randomy);
                CoinPositions.Add(newPos);
                CoinsToSpawn--;
            }
            for (int i = 0; i < CoinPositions.Count; i++)
            {
                CoinPositions[i] = Vector2.Lerp(CoinPositions[i], _statsBar.CoinRect.Location.ToVector2(), 0.1F);
                CoinPositions[i] = Vector2.SmoothStep(CoinPositions[i], _statsBar.CoinRect.Location.ToVector2(), 0.05F);

                if (_statsBar.CoinRect.Contains(CoinPositions[i]))
                {
                    MainCharacter.AddCoins(1);
                }
            }
            CoinPositions.RemoveAll(pos => _statsBar.CoinRect.Contains(pos));
            EffectAnimationManagers.RemoveAll(mngr => !mngr.IsActive);
            DamageNumberAnimationManagers.RemoveAll(mngr => !mngr.IsActive);
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
            _GPUFrameTimer.Restart();
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
            //        mngr.DrawTriangle();
            //}
            Span<EffectAnimationManager> eamAsSpan = CollectionsMarshal.AsSpan(EffectAnimationManagers);
            for (int i = 0; i < eamAsSpan.Length; i++)
            {
                //var mngr = eamAsSpan[i];
                //mngr.Draw();
                eamAsSpan[i].Draw();
            }
            //foreach (var mngr in DamageNumberAnimationManagers)
            //{
            //    if (mngr.IsActive)
            //        mngr.DrawTriangle();
            //}
            Span<DamageNumberAnimationManager> dmamAsSpan = CollectionsMarshal.AsSpan(DamageNumberAnimationManagers);
            for (int i = 0; i < dmamAsSpan.Length; i++)
            {
                var mngr = dmamAsSpan[i];
                mngr.Draw();
            }
            //foreach (var proj in ProjectileSprites)
            //{
            //    proj.DrawTriangle();
            //}

            //Span<ProjectileSprite> projectilesAsSpan = CollectionsMarshal.AsSpan(ProjectileSprites);
            //for (int i = 0; i < projectilesAsSpan.Length; i++)
            //{
            //    var proj = projectilesAsSpan[i];
            //    proj.Draw();
            //}

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

            GlobalUse.SpriteBatch.Begin(blendState: BlendState.AlphaBlend, samplerState: SamplerState.AnisotropicWrap, rasterizerState: RasterizerState.CullNone);
            MainCharacter.Inventory.Draw();
            _minimap.Draw();
            _statsBar.Draw();
            _currentFloorBar.Draw();
            _staminaBar.Draw();
            _interactionMessageBar.Draw();
            _bossHPBar.Draw();
            foreach (var pos in CoinPositions)
            {
                DrawCoin(pos);
            }
            GlobalUse.SpriteBatch.End();
            if (_gameState == GameState.Paused)
            {
                GlobalUse.SpriteBatch.Begin(blendState: BlendState.AlphaBlend);
                GlobalUse.SpriteBatch.Draw(EmptyTexture, new Rectangle(0, 0, GlobalUse.WindowSize.X, GlobalUse.WindowSize.Y), Color.Black * 0.5F);
                Menu.Draw();
                GlobalUse.SpriteBatch.End();
            }
            if (_isLoading)
            {
                GlobalUse.SpriteBatch.Begin(blendState: BlendState.AlphaBlend);
                _loadingScreen.Draw();
                GlobalUse.SpriteBatch.End();
            }

            base.Draw(gameTime);

            ShowFPS();
            FPS++;
            _GPUFrameTime = _GPUFrameTimer.ElapsedMilliseconds;
        }
        private void DrawCoin(Vector2 coinPos)
        {
            GlobalUse.SpriteBatch.Draw(TextureMapper.CoinTexture, coinPos, Color.White);
        }
        public void SpawnCoins(int amount)
        {
            CoinsToSpawn += amount;
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
        public void RunLoadingSimulation()
        {
            if (_isLoading)
                return;
            var thread = new Thread(SimulateLoading);
            thread.Start();
        }
        public void SimulateLoading()
        {
            if (_isLoading)
                return;
            _isLoading = true;
            int tasksCount = RandomNumberGenerator.GetInt32(0, 10);
            _loadingScreen = new(tasksCount);
            for (int i = 0; i < tasksCount; i++)
            {
                SimulateWork().Wait();
                _loadingScreen.CurrentTask++;
            }
            _isLoading = false;
        }
        public Task SimulateWork()
        {
            var task = Task.Delay(RandomNumberGenerator.GetInt32(0, 1000));
            task.Wait();
            return Task.CompletedTask;
        }
    }
}
