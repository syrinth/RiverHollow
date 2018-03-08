using RiverHollow.Game_Managers;
using RiverHollow.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.ViewportAdapters;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.Game_Managers.GUIComponents.Screens;

namespace RiverHollow
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class RiverHollow : Game
    {
        private static float Scale = GameManager.Scale;
        private static bool _exit = false;

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
            GameManager.GoToInformation();
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            InputManager.Load();
            SoundManager.LoadContent(Content);
            GameContentManager.LoadContent(Content);
            GameManager.LoadContent(Content);
            ObjectManager.LoadContent(Content);
            GUIManager.LoadContent();
            MapManager.LoadContent(Content, GraphicsDevice);
            GameCalendar.NewCalendar();
            CharacterManager.LoadContent(Content);
            DropManager.LoadContent(Content);
            CutsceneManager.LoadContent(Content);

            //MAR
            PlayerManager.Initialize();

            //Set the Main Menu Screen
            GUIManager.SetScreen(new IntroMenuScreen());
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
                if (_exit)
                {
                    Exit();
                }
                MouseState ms = Mouse.GetState();
                KeyboardState ks = Keyboard.GetState();

                //If we're not in the game and we're not on an input screen, handle input
                HandleImportantInput();

                //GUIManager always needs to update, regardless of game state
                GUIManager.Update(gameTime);

                Point mousePoint = Mouse.GetState().Position;
                Vector3 translate = Camera._transform.Translation;

                if (ms.RightButton == ButtonState.Pressed && GraphicCursor.LastMouseState.RightButton == ButtonState.Released)
                {
                    if (!GUIManager.ProcessRightButtonClick(mousePoint) && GameManager.OnMap())
                    {
                        //GUI does NOT use Camera translations
                        mousePoint.X = (int)((mousePoint.X - translate.X) / Scale);
                        mousePoint.Y = (int)((mousePoint.Y - translate.Y) / Scale);
                        if (GameManager.IsRunning())
                        {
                            MapManager.ProcessRightButtonClick(mousePoint);
                        }
                    }
                }
                else if (ms.LeftButton == ButtonState.Pressed && GraphicCursor.LastMouseState.LeftButton == ButtonState.Released)
                {
                    if (!GUIManager.ProcessLeftButtonClick(mousePoint) && GameManager.OnMap())
                    {
                        mousePoint.X = (int)((mousePoint.X - translate.X) / Scale);
                        mousePoint.Y = (int)((mousePoint.Y - translate.Y) / Scale);
                        if (GameManager.IsRunning())
                        {
                            if (!MapManager.ProcessLeftButtonClick(mousePoint))
                            {
                                PlayerManager.ProcessLeftButtonClick(mousePoint);
                            }
                        }
                    }
                }
                else
                {
                    if (!GUIManager.ProcessHover(mousePoint))
                    {
                        mousePoint.X = (int)((mousePoint.X - translate.X) / Scale);
                        mousePoint.Y = (int)((mousePoint.Y - translate.Y) / Scale);
                        if (GameManager.IsRunning())
                        {
                            MapManager.ProcessHover(mousePoint);
                        }
                    }
                }

                GraphicCursor.LastMouseState = ms;

                if (GameManager.OnMap())
                {
                    Camera.Update(gameTime);
                    if (CutsceneManager.Playing) { CutsceneManager.Update(gameTime); }
                    else
                    {
                        if (GameManager.IsRunning())
                        {
                            MapManager.Update(gameTime);
                            GameCalendar.Update(gameTime);
                            if (!GameManager.Scrying()) { PlayerManager.Update(gameTime); }
                        }
                    }
                }

                base.Update(gameTime);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            {
                spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Camera._transform);
                //If we're in an informational state, then only the GUIScreen data should be visible, don't draw anything except for the GUI
                if (!GameManager.Informational())
                {
                    MapManager.DrawBase(spriteBatch);
                    PlayerManager.Draw(spriteBatch);
                    
                }
                spriteBatch.End();
            }
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null);
                if (!GameManager.Informational())
                {
                    MapManager.DrawUpper(spriteBatch);
                }

                GUIManager.Draw(spriteBatch);

                if (!GameManager.Informational()) { 
                    GameCalendar.Draw(spriteBatch);
                }
                spriteBatch.End();
            }
            base.Draw(gameTime);
        }

        public void HandleImportantInput()
        {
            if (!GameManager.Informational() && !GameManager.TakingInput())
            {
                if (GameManager.OnMap() && InputManager.CheckKey(Keys.Escape))
                {
                    if (!GUIManager.IsGameMenuScreen())
                    {
                        GUIManager.SetScreen(new GameMenuScreen());
                    }
                }
                if (InputManager.CheckKey(Keys.P))
                {
                    if (GameManager.IsPaused()) { GameManager.Pause(); }
                    else { GameManager.Unpause(); }
                }
                if (!GUIManager.IsItemCreationScreen() || !GUIManager.IsHUD())
                {
                    if (InputManager.CheckKey(Keys.C))
                    {
                        if (GUIManager.IsItemCreationScreen())
                        {
                            GUIManager.SetScreen(new HUDScreen());
                        }
                        else
                        {
                            GUIManager.SetScreen(new ItemCreationScreen());
                        }
                    }
                }
            }
        }

        public static void ResetCamera()
        {
            Camera.ResetObserver();
            MapManager.BackToPlayer();
            GUIManager.SetScreen(new HUDScreen());
        }

        public static void NewGame()
        {
            PlayerManager.NewPlayer();
            MapManager.PopulateMaps(false);
            GameManager.BackToMain();
        }

        public static void RollOver()
        {
            GameCalendar.RollOver();
            PlayerManager.Rollover();
            foreach(WorkerBuilding b in PlayerManager.Buildings)
            {
                b.Rollover();
            }
            CharacterManager.RollOver();
            MapManager.Maps[MapManager.HomeMap].RollOver();
        }

        public static void PrepExit()
        {
            _exit = true;
        }
    }
}
