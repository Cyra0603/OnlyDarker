using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OnlyDarker.CommonUsing;
using OnlyDarker.GameProcess;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace OnlyDarker
{
    public class GameBody : Game
    {
        private GraphicsDeviceManager _graphics;
        //private SpriteBatch _spriteBatch;
        public SpriteFont Arial;
        public int FPS { get; set; } = 0;
        private string Netgraph { get; set; } = "0";
        public static Floor CurrentFloorType { get; private set; }
        private Room _currentRoom;
        public static Character? MainCharacter { get; private set; } = null;
        private Matrix _cameraView;

        public GameBody()
        {
            _graphics = new GraphicsDeviceManager(this);
            GlobalUse.Content = Content;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

        }
        private void CalculateCameraView()
        {
            var lx = (GlobalUse.WindowSize.X / 2) - MainCharacter.Position.X;
            var ly = (GlobalUse.WindowSize.Y / 2) - MainCharacter.Position.Y;
            _cameraView = Matrix.CreateTranslation(lx, ly, 0F);
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

        protected override void Initialize()
        {
            GlobalUse.WindowSize = new(1920, 1080);

            _graphics.PreferredBackBufferWidth = GlobalUse.WindowSize.X;

            _graphics.PreferredBackBufferHeight = GlobalUse.WindowSize.Y;

            _graphics.SynchronizeWithVerticalRetrace = false;

            IsFixedTimeStep = false;

            _graphics.ApplyChanges();

            _currentRoom = new(Floor.One, RoomType.Entry);

            MainCharacter =
                new(GlobalUse.Content.Load<Texture2D>("Character/MainCharacter"),
                _currentRoom._tiles[2, 2]);

            MainCharacter.SetRoomBounds(_currentRoom.RoomSize, _currentRoom.TileSize);

            UpdateFpsCounter();

            InputManager.UpdateCharacterControls();

            InputManager.ToggleDisableInputs();

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
            GlobalUse.Update(gameTime);
            MainCharacter.Update();
            CalculateCameraView();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            GlobalUse.SpriteBatch.Begin(transformMatrix: _cameraView);

            _currentRoom.Draw();

            MainCharacter.Draw();

            ShowFPS();

            GlobalUse.SpriteBatch.End();
            //
            base.Draw(gameTime);
            FPS++;
        }

        private void ShowFPS()
        {
            var lx = MainCharacter.Position.X - GlobalUse.WindowSize.X / 2;
            var ly = MainCharacter.Position.Y - GlobalUse.WindowSize.Y / 2;
            GlobalUse.SpriteBatch.DrawString(Arial, Netgraph, new(lx, ly), Color.Red, 0F, Vector2.Zero, 1.2F, SpriteEffects.None, 1F);
        }
    }
}
