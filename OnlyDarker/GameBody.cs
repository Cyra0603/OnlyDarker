
using Microsoft.Win32;
using Microsoft.Xna.Framework.Graphics;
using OnlyDarker.CommonUsing;
using OnlyDarker.CommonUsing.Rendering;
using OnlyDarker.GameProcess;
using OnlyDarker.GameProcess.SpriteClasses;
using OnlyDarker.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        private static InteractionMessageBar _interactionMessageBar;
        private static Minimap _minimap;
        public static Texture2D EmptyTexture;
        private static Texture2D _hitboxTexture;
        public static List<EffectAnimationManager> EffectAnimationManagers { get; private set; } = new();
        public static List<DamageNumberAnimationManager> DamageNumberAnimationManagers { get; private set; } = new();
        public static List<ProjectileSprite> ProjectileSprites { get; private set; } = new();
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
        public static Floor CurrentFloorType { get; private set; }
        private Matrix _cameraView;
        private static float _cameraZoom = 2F;
        private static float _collectiblesTimeAccumulator = 0F;

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

            EmptyTexture = new Texture2D(_graphics.GraphicsDevice, 1, 1);
            EmptyTexture.SetData(new[] { Color.White });

            TextureMapper.LoadTextures();

            BindManager = BindManager.GetBindManagerInstance();

            BindManager.ExitApplication.KeyPressed += Exit;

            _timeElapsed += FixedTimeStepUpdate;

            BindManager.ToggleDebug.KeyPressed += GlobalUse.ToggleDebugMode;

            SceneManager = new(new Level(Floor.One));

            _mainCanvas = new(_graphics.GraphicsDevice, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);

            _mainCanvas.SetDestinationRectangle();

            _hitboxTexture = new Texture2D(_graphics.GraphicsDevice, 1, 1);
            _hitboxTexture.SetData(new Color[] { Color.Red });

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

            _interactionMessageBar = InteractionMessageBar.GetInstance();

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
            SceneManager.CurrentRoom.SortObjectsByY();
            _collectiblesTimeAccumulator += (float)gameTime.ElapsedGameTime.TotalSeconds;
            foreach (var mngr in EffectAnimationManagers)
            {
                if(mngr.IsActive)
                    mngr.Update((float)gameTime.ElapsedGameTime.TotalMilliseconds);
            }
            foreach (var mngr in DamageNumberAnimationManagers)
            {
                if (mngr.IsActive)
                    mngr.Update((float)gameTime.ElapsedGameTime.TotalMilliseconds);
            }
            foreach(var proj in ProjectileSprites)
            {
                proj.Update((float)gameTime.ElapsedGameTime.TotalMilliseconds);
            }
            EffectAnimationManagers.RemoveAll(mngr => !mngr.IsActive);
            DamageNumberAnimationManagers.RemoveAll(mngr => !mngr.IsActive);
            ProjectileSprites.RemoveAll(proj => proj.Lifetime.TimeLeft <= 0);

            _staminaBar.Update((float)gameTime.ElapsedGameTime.TotalMilliseconds);

            _fixedElapsedTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            base.Update(gameTime);
        }
        private void FixedTimeStepUpdate(float milliseconds)
        {
            _interactionMessageBar.Update();
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
                DrawRectangleOutline(new(MainCharacter.Position.ToPoint(), new(2,2)), Color.Red);
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
            foreach (var mngr in EffectAnimationManagers)
            {
                if (mngr.IsActive)
                    mngr.Draw();
            }
            foreach (var mngr in DamageNumberAnimationManagers)
            {
                if (mngr.IsActive)
                    mngr.Draw();
            }
            foreach (var proj in ProjectileSprites)
            {
                proj.Draw();
            }
            foreach (var entity in SceneManager.CurrentRoom.Damageables)
            {
                if(entity.HealthPoints > 0)
                    entity.DrawHPBar();
            }
            GlobalUse.SpriteBatch.End();
            _mainCanvas.Deactivate();

            _minimap.RenderMinimap();

            //Drawing
            _mainCanvas.Draw(GlobalUse.SpriteBatch);

            _characterHealthbar.StandaloneDraw();
            GlobalUse.SpriteBatch.Begin(blendState: BlendState.AlphaBlend);
            _minimap.Draw();
            ShowFPS();
            _statsBar.Draw();
            _currentFloorBar.Draw();
            _staminaBar.Draw();
            _interactionMessageBar.Draw();
            GlobalUse.SpriteBatch.End();

            base.Draw(gameTime);
            FPS++;
        }
        private void CalculateCameraView()
        {
            var lx = GlobalUse.WindowSize.X / 2 / _cameraZoom - MainCharacter.Position.X;
            //lx = MathHelper.Clamp(lx, -_сurrentRoom.RoomSize.X + GlobalUse.WindowSize.X / _cameraZoom + (_сurrentRoom.TileSize.X / 2), _сurrentRoom.TileSize.X / 2);
            var ly = GlobalUse.WindowSize.Y / 2 / _cameraZoom - MainCharacter.Position.Y;
            //ly = MathHelper.Clamp(ly, -_сurrentRoom.RoomSize.Y + GlobalUse.WindowSize.Y / _cameraZoom + (_сurrentRoom.TileSize.Y / 2), _сurrentRoom.TileSize.Y / 2);
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
        public static float GetSwayFunctionValue()
        {
            return _collectiblesTimeAccumulator;
        }
        public static void DrawRectangleOutline(Rectangle rect, Color color, int borderWidth = 1)
        {
            GlobalUse.SpriteBatch.Draw(EmptyTexture, new Rectangle(rect.Left, rect.Top, borderWidth, rect.Height), color);
            GlobalUse.SpriteBatch.Draw(EmptyTexture, new Rectangle(rect.Right, rect.Top, borderWidth, rect.Height), color);
            GlobalUse.SpriteBatch.Draw(EmptyTexture, new Rectangle(rect.Left, rect.Top, rect.Width, borderWidth), color);
            GlobalUse.SpriteBatch.Draw(EmptyTexture, new Rectangle(rect.Left, rect.Bottom, rect.Width, borderWidth), color);
        }
    }
}
