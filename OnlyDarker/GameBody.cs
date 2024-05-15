
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
        private GraphicsDeviceManager _graphics;
        private MainCanvas _mainCanvas;
        public static SceneManager SceneManager { get; private set; }
        public SpriteFont Arial;
        public static Character? MainCharacter { get; private set; } = null;
        private static CharacterHealthbar _characterHealthbar;
        private static Texture2D _hitboxTexture;
        private long _fixedElapsedTime = 0;
        public const long ONE_TICK = 78125L;
        public int FPS { get; set; } = 0;
        private string _netgraph = "0";
        private Vector2 _netgraphPosition;
        public static Floor CurrentFloorType { get; private set; }
        private Matrix _cameraView;
        private static float _cameraZoom = 0.5F;
        private bool _drawHitboxes = false;
        private KeyboardState _lastKeyboardState;

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

            Arial = Content.Load<SpriteFont>("Fonts/Arial");

            _netgraphPosition = new Vector2(_graphics.PreferredBackBufferWidth - Arial.MeasureString(_netgraph).X, 0);

            _graphics.SynchronizeWithVerticalRetrace = false;

            IsFixedTimeStep = false;

            _graphics.ApplyChanges();

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

            _characterHealthbar = new(GlobalUse.Content.Load<Texture2D>("UI/Heart"));

            UpdateFpsCounter();

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

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.F1) && !_lastKeyboardState.IsKeyDown(Keys.F1)) SceneManager.GoToScene(1);
            if (Keyboard.GetState().IsKeyDown(Keys.F2) && !_lastKeyboardState.IsKeyDown(Keys.F2)) SceneManager.GoToScene(2);
            if (Keyboard.GetState().IsKeyDown(Keys.F3) && !_lastKeyboardState.IsKeyDown(Keys.F3)) SceneManager.GoToScene(3);
            if (Keyboard.GetState().IsKeyDown(Keys.F4) && !_lastKeyboardState.IsKeyDown(Keys.F4)) SceneManager.GoToScene(4);
            if (Keyboard.GetState().IsKeyDown(Keys.F10) && !_lastKeyboardState.IsKeyDown(Keys.F10)) SceneManager.GoToScene(0);
            if (Keyboard.GetState().IsKeyDown(Keys.F11) && !_lastKeyboardState.IsKeyDown(Keys.F11)) MainCharacter.TakeDamage(1);
            if (Keyboard.GetState().IsKeyDown(Keys.F12) && !_lastKeyboardState.IsKeyDown(Keys.F12)) MainCharacter.Heal(1);
            _lastKeyboardState = Keyboard.GetState();

            if (_fixedElapsedTime >= ONE_TICK)
            {
                CalculateCameraView();
                MainCharacter.Update();
                _fixedElapsedTime = 0;
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            //GraphicsDevice.Clear(Color.Black);

            _mainCanvas.Activate();
            //Background
            GlobalUse.SpriteBatch.Begin(sortMode: SpriteSortMode.BackToFront, blendState: BlendState.AlphaBlend, samplerState: SamplerState.LinearWrap, transformMatrix: _cameraView);
            SceneManager.CurrentRoom.CurrentBackground.Draw();
            GlobalUse.SpriteBatch.End();
            //MainScene
            GlobalUse.SpriteBatch.Begin(sortMode: SpriteSortMode.BackToFront, blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp, transformMatrix: _cameraView);
            SceneManager.CurrentRoom.Draw();
            MainCharacter.Draw();
            if (_drawHitboxes)
            {
                foreach (var hitbox in SceneManager.CurrentRoom.RoomColliders)
                {
                    DrawHitbox(hitbox);
                }
                GlobalUse.SpriteBatch.Draw(_hitboxTexture, MainCharacter.MovementCollider, Color.Blue);
            }
            GlobalUse.SpriteBatch.End();

            _mainCanvas.Draw(GlobalUse.SpriteBatch);
            //UI
            GlobalUse.SpriteBatch.Begin();
            ShowFPS();
            GlobalUse.SpriteBatch.End();
            _characterHealthbar.StandaloneDraw();
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
        private async void UpdateFpsCounter()
        {
            while (true)
            {
                await Task.Delay(1000);
                _netgraph = FPS.ToString();
                FPS = 0;
            }
        }


        private void ShowFPS()
        {
            GlobalUse.SpriteBatch.DrawString(Arial, _netgraph, _netgraphPosition, Color.Red, 0F, Vector2.Zero, 0.3F, SpriteEffects.None, 0F);
        }

        private void DrawHitbox(Rectangle hitbox)
        {
            GlobalUse.SpriteBatch.Draw(_hitboxTexture, hitbox, Color.White);
        }
    }
}
