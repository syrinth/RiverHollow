using RiverHollow.Game_Managers;
using RiverHollow.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.ViewportAdapters;


namespace RiverHollow
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class RiverHollow : Game
    {
        public static float Scale = 1.5f;
        public enum GameState { WorldMap, Combat, Paused, Build, Information, Input}
        private static GameState _gameState;
        public static GameState State { get => _gameState; }
        public GraphicsDeviceManager _graphicsDeviceManager;
        public SpriteBatch spriteBatch;
        public static int ScreenWidth = 1920;
        public static int ScreenHeight = 1080;

        public ViewportAdapter ViewportAdapter { get; private set; }
        
        public RiverHollow()
        {
            _graphicsDeviceManager = new GraphicsDeviceManager(this);
            _graphicsDeviceManager.IsFullScreen = true;
            Content.RootDirectory = "Content";

            _graphicsDeviceManager.HardwareModeSwitch = false;
            _graphicsDeviceManager.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            _graphicsDeviceManager.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            Camera.SetViewport(GraphicsDevice.Viewport);
            InventoryManager.Init();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            InputManager.Load();
            GameContentManager.LoadContent(Content);
            ObjectManager.LoadContent(Content);
            GUIManager.LoadContent();
            MapManager.LoadContent(Content, GraphicsDevice);
            spriteBatch = new SpriteBatch(GraphicsDevice);
            GameCalendar.NewCalendar();
            CharacterManager.LoadContent(Content);
            DropManager.LoadContent(Content);
            
            GUIManager.SetScreen(GUIManager.Screens.MainMenu);
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            Content.Unload();
        }

        protected override void Update(GameTime gameTime)
        {
            if (this.IsActive)
            {
                MouseState ms = Mouse.GetState();
                KeyboardState ks = Keyboard.GetState();

                if (_gameState != GameState.Input)
                {
                    if (InputManager.CheckKey(Keys.Escape))
                    {
                        if (_gameState == GameState.Combat)
                        {
                            CombatManager.EndBattle();
                        }
                        else if (_gameState == GameState.WorldMap || _gameState == GameState.Build)
                        {
                            Exit();
                        }
                    }
                    if (InputManager.CheckKey(Keys.P))
                    {
                        if (_gameState == GameState.Paused)
                            _gameState = GameState.Paused;
                        else
                            _gameState = GameState.WorldMap;
                    }
                    if (InputManager.CheckKey(Keys.X))
                    {
                        PlayerManager.Save();
                    }
                    if (GUIManager.CurrentGUIScreen != GUIManager.Screens.ItemCreation || GUIManager.CurrentGUIScreen != GUIManager.Screens.HUD)
                    {
                        if (InputManager.CheckKey(Keys.C))
                        {
                            if (GUIManager.CurrentGUIScreen == GUIManager.Screens.ItemCreation)
                            {
                                GUIManager.SetScreen(GUIManager.Screens.HUD);
                            }
                            else
                            {
                                GUIManager.SetScreen(GUIManager.Screens.ItemCreation);
                            }
                        }
                    }
                    if (GUIManager.CurrentGUIScreen != GUIManager.Screens.Inventory || GUIManager.CurrentGUIScreen != GUIManager.Screens.HUD)
                    {
                        if (InputManager.CheckKey(Keys.I))
                        {
                            if (GUIManager.CurrentGUIScreen == GUIManager.Screens.Inventory)
                            {
                                ChangeGameState(GameState.WorldMap);
                            }
                            else
                            {
                                GUIManager.SetScreen(GUIManager.Screens.Inventory);
                            }
                        }
                    }
                }

                GUIManager.Update(gameTime);

                Point mousePoint = Mouse.GetState().Position;
                Vector3 translate = Camera._transform.Translation;
                if (ms.RightButton == ButtonState.Pressed && GraphicCursor.LastMouseState.RightButton == ButtonState.Released)
                {
                    if (!GUIManager.ProcessRightButtonClick(mousePoint) && (_gameState == GameState.WorldMap || _gameState == GameState.Build))
                    {
                        //GUI does NOT use Camera translations
                        mousePoint.X = (int)((mousePoint.X - translate.X)/Scale);
                        mousePoint.Y = (int)((mousePoint.Y - translate.Y)/Scale);
                        MapManager.ProcessRightButtonClick(mousePoint);
                    }
                }
                else if (ms.LeftButton == ButtonState.Pressed && GraphicCursor.LastMouseState.LeftButton == ButtonState.Released)
                {
                    if (!GUIManager.ProcessLeftButtonClick(mousePoint) && (_gameState == GameState.WorldMap || _gameState == GameState.Build))
                    {
                        mousePoint.X = (int)((mousePoint.X - translate.X) / Scale);
                        mousePoint.Y = (int)((mousePoint.Y - translate.Y) / Scale);
                        if (!MapManager.ProcessLeftButtonClick(mousePoint))
                        {
                            PlayerManager.ProcessLeftButtonClick(mousePoint);
                        }
                    }
                }
                else
                {
                    if ((_gameState == GameState.Build || _gameState == GameState.WorldMap || _gameState == GameState.Combat) && !GUIManager.ProcessHover(mousePoint))
                    {
                        mousePoint.X = (int)((mousePoint.X - translate.X) / Scale);
                        mousePoint.Y = (int)((mousePoint.Y - translate.Y) / Scale);
                        MapManager.ProcessHover(mousePoint);
                    }
                }
                GraphicCursor.LastMouseState = ms;

                if (_gameState == GameState.WorldMap || _gameState == GameState.Build)
                {
                    Camera.Update(gameTime);
                    MapManager.Update(gameTime);

                    if(_gameState == GameState.WorldMap)
                    {
                        PlayerManager.Update(gameTime);
                        GameCalendar.Update(gameTime);
                    }
                }

                base.Update(gameTime);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            {
                spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, null, null, null, null, Camera._transform);
                //If we're in an informational state, then only the GUIScreen data should be visible, don't draw anything except for the GUI
                if (_gameState != GameState.Information)
                {
                    MapManager.Draw(spriteBatch);
                    PlayerManager.Draw(spriteBatch);
                }
                spriteBatch.End();
            }
            {
                spriteBatch.Begin();
                GUIManager.Draw(spriteBatch);
                if (_gameState != GameState.Information) { 
                    GameCalendar.Draw(spriteBatch);
                }
                spriteBatch.End();
            }
            base.Draw(gameTime);
        }

        public static void ChangeGameState(GameState state)
        {
            _gameState = state;

            if (_gameState == GameState.Combat)
            {
                GUIManager.SetScreen(GUIManager.Screens.Combat);
            }
            if (GUIManager.CurrentGUIScreen != GUIManager.Screens.HUD && _gameState == GameState.WorldMap)
            {
                GUIManager.SetScreen(GUIManager.Screens.HUD);
            }
        }

        public static void ResetCamera()
        {
            Camera.ResetObserver();
            MapManager.BackToPlayer();
            GUIManager.SetScreen(GUIManager.Screens.HUD);
        }

        public static void NewGame()
        {
            PlayerManager.NewPlayer();
            MapManager.PopulateMaps(false);
            ChangeGameState(RiverHollow.GameState.WorldMap);
        }

        public static void LoadGame()
        {
            PlayerManager.Load();
            MapManager.PopulateMaps(true);
            RiverHollow.ChangeGameState(RiverHollow.GameState.WorldMap);
        }

        public static void RollOver()
        {
            PlayerManager.Rollover();
            foreach(Building b in PlayerManager.Buildings)
            {
                b.Rollover();
            }
        }
    }
}
