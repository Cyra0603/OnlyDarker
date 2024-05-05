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
        private SpriteBatch _spriteBatch;
        public SpriteFont Arial;
        public int FPS { get; set; } = 0;
        private string Netgraph { get; set; } = "0";
        public int RoomWidth = 64;
        public int RoomHeight = 48;
        public int CameraY { get; set; }
        public int CameraX { get; set; }
        public int CameraSpeed { get; set; } = 15;
        const int MAX_CAMERA_OFFSET = 300;
        public static Floor CurrentFloor { get; private set; }
        private readonly Room _currentRoom;
        public static Character? MainCharacter { get; private set; } = null;

        public GameBody()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _currentRoom = new(Floor.One, RoomType.Entry);
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
            // TODO: Add your initialization logic here
            UpdateFpsCounter();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Arial = Content.Load<SpriteFont>("Fonts/Arial");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();
            InputManager.Update();    
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin();
            _currentRoom.Draw();
            _spriteBatch.DrawString(Arial, Netgraph, new(0, 0), Color.Green);
            _spriteBatch.End();
            base.Draw(gameTime);
            FPS++;
        }
    }
}
