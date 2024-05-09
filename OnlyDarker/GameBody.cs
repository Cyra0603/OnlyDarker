
using OnlyDarker.CommonUsing;
using OnlyDarker.CommonUsing.Rendering;
using OnlyDarker.GameProcess;
using OnlyDarker.GameProcess.SpriteClasses;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace OnlyDarker
{
    public class GameBody : Game
    {
        private GraphicsDeviceManager _graphics;
        private MainCanvas _mainCanvas;
        private SceneManager _sceneManager;
        public SpriteFont Arial;
        private Room _currentRoom => _sceneManager.CurrentRoom;
        public static Character? MainCharacter { get; private set; } = null;
        public static Vector2 ScreenCenter { get; private set; }
        public int FPS { get; set; } = 0;
        private string Netgraph { get; set; } = "0";
        public static Floor CurrentFloorType { get; private set; }
        private Matrix _cameraView;
        private float _cameraZoom = 0.5F;

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

            _graphics.SynchronizeWithVerticalRetrace = false;

            IsFixedTimeStep = false;

            _graphics.ApplyChanges();

            _sceneManager = new(new Level(Floor.One));

            _mainCanvas = new(_graphics.GraphicsDevice, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);

            _mainCanvas.SetDestinationRectangle();

            MainCharacter = new(
                GlobalUse.Content.Load<Texture2D>("Character/MainCharacter"),
                GlobalUse.Content.Load<Texture2D>("Character/MainCharacterHand"),
                _currentRoom._tiles[2, 2]);
            MainCharacter.SetRoomBounds(_currentRoom.RoomSize, _currentRoom.TileSize);

            UpdateFpsCounter();

            ControlsManager.CharacterInputsDisabled(false);

            ControlsManager.UpdateCharacterControls();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            GlobalUse.SpriteBatch = new SpriteBatch(GraphicsDevice);
            Arial = Content.Load<SpriteFont>("Fonts/Arial");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.F1)) _sceneManager.GoToScene(1);
            if (Keyboard.GetState().IsKeyDown(Keys.F2)) _sceneManager.GoToScene(2);
            if (Keyboard.GetState().IsKeyDown(Keys.F3)) _sceneManager.GoToScene(3);
            if (Keyboard.GetState().IsKeyDown(Keys.F4)) _sceneManager.GoToScene(4);
            if (Keyboard.GetState().IsKeyDown(Keys.F10)) _sceneManager.GoToScene(0);

            GlobalUse.Update(gameTime);
            MainCharacter.Update();
            CalculateCameraView();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            //GraphicsDevice.Clear(Color.Black);
            _mainCanvas.Activate();

            GlobalUse.SpriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _cameraView);
            _currentRoom.Draw();
            MainCharacter.Draw();
            GlobalUse.SpriteBatch.End();

            _mainCanvas.Draw(GlobalUse.SpriteBatch);

            GlobalUse.SpriteBatch.Begin();
            ShowFPS();
            GlobalUse.SpriteBatch.End();

            base.Draw(gameTime);
            FPS++;
        }

        private void CalculateCameraView()
        {
            var lx = GlobalUse.WindowSize.X / 2 / _cameraZoom - MainCharacter.Position.X;
            lx = MathHelper.Clamp(lx, -_currentRoom.RoomSize.X + GlobalUse.WindowSize.X / _cameraZoom + (_currentRoom.TileSize.X / 2), _currentRoom.TileSize.X / 2);
            var ly = GlobalUse.WindowSize.Y / 2 / _cameraZoom - MainCharacter.Position.Y;
            ly = MathHelper.Clamp(ly, -_currentRoom.RoomSize.Y + GlobalUse.WindowSize.Y / _cameraZoom + (_currentRoom.TileSize.Y / 2), _currentRoom.TileSize.Y / 2);
            _cameraView = Matrix.CreateTranslation(lx, ly, 0F) * Matrix.CreateScale(_cameraZoom,_cameraZoom,0);
            ScreenCenter = new(lx - GlobalUse.WindowSize.X, ly - GlobalUse.WindowSize.Y);
        }
        private async void UpdateFpsCounter()
        {
            while (true)
            {
                await Task.Delay(1000);
                Netgraph = FPS.ToString();
                FPS = 0;
            }
        }


        private void ShowFPS()
        {
            GlobalUse.SpriteBatch.DrawString(Arial, Netgraph, Vector2.Zero, Color.Red, 0F, Vector2.Zero, 0.3F, SpriteEffects.None, 1F);
        }
    }
}
