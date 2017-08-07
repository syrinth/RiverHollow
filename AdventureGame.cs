using Adventure.SpriteAnimations;
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

        public TileMap myMap = new TileMap();
        public Player player;
        public Monster mon;

        public bool _isPaused = false;

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
            player = new Player(Content);
            mon = new Monster(Content, new Vector2(500, 600));

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

            Keys[] pressedKeys = Keyboard.GetState().GetPressedKeys();
            foreach(Keys k in pressedKeys)
            {
                if(k == Keys.P)
                {
                    _isPaused = !_isPaused;
                }
            }

            if (!_isPaused)
            {
                GameCalendar.Update(gameTime);
                // TODO: Add your update logic here
                Camera.Update(gameTime, this);

                player.Update(gameTime, myMap);
                mon.Update(gameTime, myMap, player);
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
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null,null,null, Camera._transform);

        
            for (int y = 0; y < myMap.MapHeight; y++)
            {
                for (int x = 0; x < myMap.MapWidth; x++)
                {
                    int tileID = myMap.Rows[y].Columns[x].TileID;

                    spriteBatch.Draw(
                        Tile.TileSetTexture,
                        new Rectangle((x * Tile.TILE_WIDTH), (y * Tile.TILE_HEIGHT), Tile.TILE_WIDTH, Tile.TILE_HEIGHT),
                        Tile.GetSourceRectangle(tileID),
                        Color.White);
                }
            }

            player.Draw(spriteBatch);
            mon.Draw(spriteBatch);

            spriteBatch.End();

            //TODO: Check if this is kosher
            spriteBatch.Begin();
            GameCalendar.Draw(spriteBatch);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
