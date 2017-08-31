using Adventure.Characters.Monsters;
using Adventure.Characters.NPCs;
using Adventure.Game_Managers;
using Adventure.Game_Managers.GUIComponents.Screens;
using Adventure.Game_Managers.GUIObjects;
using Adventure.GUIObjects;
using Adventure.Items;
using Adventure.Screens;
using Adventure.Tile_Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.ViewportAdapters;


namespace Adventure
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class AdventureGame : Game
    {
        public enum GameState { MainMenu, Running, Inventory, Paused, EndOfDay }
        public static GameState _gameState;
        public GraphicsDeviceManager _graphicsDeviceManager;
        public SpriteBatch spriteBatch;
        public static int ScreenWidth = 1920;
        public static int ScreenHeight = 1080;
        public static bool BuildingMode = false;

        public ViewportAdapter ViewportAdapter { get; private set; }

        private bool _paused = false;
        //private bool _pausedForGuide = false;

        public AdventureGame()
        {
            _graphicsDeviceManager = new GraphicsDeviceManager(this);
            _graphicsDeviceManager.IsFullScreen = true;
            Content.RootDirectory = "Content";

            _graphicsDeviceManager.HardwareModeSwitch = false;
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
            InputManager.Load();
            GameContentManager.LoadContent(Content);
            ObjectManager.LoadContent(Content);
            GUIManager.LoadContent();
            MapManager.LoadContent(Content, GraphicsDevice);
            PlayerManager.NewPlayer();
            spriteBatch = new SpriteBatch(GraphicsDevice);
            GameCalendar.NewCalendar();
            CharacterManager.LoadContent(Content);

            ChangeGameState(GameState.MainMenu);
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
            if (this.IsActive)
            {
                KeyboardState ks = Keyboard.GetState();
                if (InputManager.CheckKey(Keys.Escape))
                {
                    if (_gameState == GameState.Running)
                    {
                        Exit();
                    }
                }

                if (Mouse.GetState().MiddleButton == ButtonState.Pressed)
                {
                    PlayerManager.Save();
                }

                if (GUIManager.CurrentGUIScreen != GUIManager.Screens.TextInput)
                {
                    if (InputManager.CheckKey(Keys.P))
                    {
                        if (!_paused)
                            BeginPause(true);
                        else
                            EndPause();
                    }
                    if (GUIManager.CurrentGUIScreen != GUIManager.Screens.ItemCreation || GUIManager.CurrentGUIScreen != GUIManager.Screens.HUD)
                    {
                        if (InputManager.CheckKey(Keys.C))
                        {
                            if (GUIManager.CurrentGUIScreen == GUIManager.Screens.ItemCreation)
                            {
                                GUIManager.LoadScreen(GUIManager.Screens.HUD);
                            }
                            else
                            {
                                GUIManager.LoadScreen(GUIManager.Screens.ItemCreation);
                            }
                        }
                    }
                    if (GUIManager.CurrentGUIScreen != GUIManager.Screens.Inventory || GUIManager.CurrentGUIScreen != GUIManager.Screens.HUD)
                    {
                        if (InputManager.CheckKey(Keys.I))
                        {
                            if (GUIManager.CurrentGUIScreen == GUIManager.Screens.Inventory)
                            {
                                ChangeGameState(GameState.Running);
                            }
                            else
                            {
                                ChangeGameState(GameState.Inventory);
                            }
                        }
                    }
                }

                //checkPauseGuide();

                GUIManager.Update(gameTime);

                MouseState ms = Mouse.GetState();
                Point mousePoint = Mouse.GetState().Position;
                Vector3 translate = Camera._transform.Translation;
                if (ms.RightButton == ButtonState.Pressed && GraphicCursor.LastMouseState.RightButton == ButtonState.Released)
                {
                    if (!GUIManager.ProcessRightButtonClick(mousePoint))
                    {
                        //GUI does NOT use Camera translations
                        mousePoint.X -= (int)translate.X;
                        mousePoint.Y -= (int)translate.Y;
                        MapManager.ProcessRightButtonClick(mousePoint);
                    }
                }
                else if (ms.LeftButton == ButtonState.Pressed && GraphicCursor.LastMouseState.LeftButton == ButtonState.Released)
                {
                    if (!GUIManager.ProcessLeftButtonClick(mousePoint))
                    {
                        mousePoint.X -= (int)translate.X;
                        mousePoint.Y -= (int)translate.Y;
                        if (!MapManager.ProcessLeftButtonClick(mousePoint))
                        {
                            PlayerManager.ProcessLeftButtonClick(mousePoint);
                        }
                    }
                }
                else
                {
                    if (!GUIManager.ProcessHover(mousePoint))
                    {
                        mousePoint.X -= (int)translate.X;
                        mousePoint.Y -= (int)translate.Y;
                        MapManager.ProcessHover(mousePoint);
                    }
                }

                GraphicCursor.LastMouseState = ms;

                if (_gameState == GameState.Running && GUIManager.CurrentGUIScreen != GUIManager.Screens.TextInput)
                {
                    if (!_paused)
                    {

                        // TODO: Add your update logic here
                        Camera.Update(gameTime);

                        if (!BuildingMode)
                        {
                            GameCalendar.Update(gameTime);
                            if (GameCalendar.CurrentHour == 2)
                            {
                                RollOver();
                            }

                            MapManager.Update(gameTime);
                            PlayerManager.Update(gameTime);
                        }
                    }
                }

                base.Update(gameTime);
            }
        }

        public static void ChangeGameState(GameState state)
        {
            _gameState = state;

            if(_gameState == GameState.MainMenu)
            {
                GUIManager.LoadScreen(GUIManager.Screens.MainMenu);
            }
            else if (_gameState == GameState.Running)
            {
                GUIManager.LoadScreen(GUIManager.Screens.HUD);
            }
            else if (_gameState == GameState.EndOfDay)
            {
                PlayerManager.Player.Stamina = PlayerManager.Player.MaxStamina;
                GUIManager.LoadScreen(GUIManager.Screens.DayEnd);
            }
            else if (_gameState == GameState.Inventory)
            {
                GUIManager.LoadScreen(GUIManager.Screens.Inventory);
            }
        }

        public static void ResetCamera()
        {
            Camera.ResetObserver();
            MapManager.BackToPlayer();
            GUIManager.LoadScreen(GUIManager.Screens.HUD);
        }

        public static void NewGame()
        {
            MapManager.PopulateMaps(false);
            ChangeGameState(AdventureGame.GameState.Running);
        }

        public static void LoadGame()
        {
            PlayerManager.Load();
            MapManager.PopulateMaps(true);
            AdventureGame.ChangeGameState(AdventureGame.GameState.Running);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, null, null, null, null, Camera._transform);

            MapManager.Draw(spriteBatch);

            if (!BuildingMode)
            {
                PlayerManager.Draw(gameTime, spriteBatch);
            }

            spriteBatch.End();

            //TODO: Check if this is kosher
            spriteBatch.Begin();

            GUIManager.Draw(spriteBatch);
            if (_gameState == GameState.Running)
            {
                GameCalendar.Draw(spriteBatch);
            }

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

        private void RollOver()
        {
         //   _wiz.MakeDailyItem();
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
