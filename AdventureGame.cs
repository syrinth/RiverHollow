using Adventure.Characters.Monsters;
using Adventure.Tile_Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Adventure
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class AdventureGame : Game
    {
        public GraphicsDeviceManager _graphicsDeviceManager;
        public SpriteBatch spriteBatch;
        public int SCREEN_WIDTH = 1920;
        public int SCREEN_HEIGHT = 1080;

        public TileMap _myMap = new TileMap();
        public Player _player;
        public Goblin gobbo;

        private bool _paused = false;
        private bool _pauseKeyDown = false;
        //private bool _pausedForGuide = false;

        public AdventureGame()
        {
            _graphicsDeviceManager = new GraphicsDeviceManager(this);
            _graphicsDeviceManager.IsFullScreen = true;
            Content.RootDirectory = "Content";

            _graphicsDeviceManager.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            _graphicsDeviceManager.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            Camera.SetViewport(GraphicsDevice.Viewport);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            GameCalendar.NewCalender(Content, SCREEN_WIDTH, SCREEN_HEIGHT);
            _player = new Player(Content);
            gobbo = new Goblin(Content, new Vector2(500, 600));

            Tile.TileSetTexture = Content.Load<Texture2D>(@"part2_tileset");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            Content.Unload();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            checkPauseKey(Keyboard.GetState(), GamePad.GetState(PlayerIndex.One));
            //checkPauseGuide();

            if (!_paused)
            {
                GameCalendar.Update(gameTime);
                // TODO: Add your update logic here
                Camera.Update(gameTime, this);

                _player.Update(gameTime, _myMap);
                gobbo.Update(gameTime, _myMap, _player);
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Camera._transform);


            for (int y = 0; y < _myMap.MapHeight; y++)
            {
                for (int x = 0; x < _myMap.MapWidth; x++)
                {
                    int tileID = _myMap.Rows[y].Columns[x].TileID;

                    spriteBatch.Draw(
                        Tile.TileSetTexture,
                        new Rectangle((x * Tile.TILE_WIDTH), (y * Tile.TILE_HEIGHT), Tile.TILE_WIDTH, Tile.TILE_HEIGHT),
                        Tile.GetSourceRectangle(tileID),
                        Color.White);
                }
            }

            _player.Draw(spriteBatch);
            gobbo.Draw(spriteBatch);

            spriteBatch.End();

            //TODO: Check if this is kosher
            spriteBatch.Begin();
            GameCalendar.Draw(spriteBatch);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void BeginPause(bool UserInitiated)
        {
            _paused = true;
            //pausedForGuide = !UserInitiated;
        }

        private void EndPause()
        {
            _paused = false;
            //pausedForGuide = false;
        }

        private void checkPauseKey(KeyboardState keyboardState,GamePadState gamePadState)
        {
            bool pauseKeyDownThisFrame = (keyboardState.IsKeyDown(Keys.P) || (gamePadState.Buttons.Y == ButtonState.Pressed));
            // If key was not down before, but is down now, we toggle the
            // pause setting
            if (!_pauseKeyDown && pauseKeyDownThisFrame)
            {
                if (!_paused)
                    BeginPause(true);
                else
                    EndPause();
            }
            _pauseKeyDown = pauseKeyDownThisFrame;
        }

        /*private void checkPauseGuide()
        {
            // Pause if the Guide is up
            if (!paused && Guide.IsVisible)
                BeginPause(false);
            // If we paused for the guide, unpause if the guide
            // went away
            else if (paused && pausedForGuide && !Guide.IsVisible)
                EndPause();
        }*/
    }
}
