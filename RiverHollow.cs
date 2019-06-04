using RiverHollow.Game_Managers;
using RiverHollow.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.Game_Managers.GUIComponents.Screens;

using static RiverHollow.Game_Managers.GameManager;
using RiverHollow.Buildings;
using RiverHollow.Actors;

namespace RiverHollow
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class RiverHollow : Game
    {
        private static bool _exit = false;

        public GraphicsDeviceManager _graphicsDeviceManager;
        public SpriteBatch spriteBatch;
        public static int ScreenWidth = 1920;
        public static int ScreenHeight = 1080;

        //TEST
        public static Texture2D lightMask;
        public static Effect effect1;
        static RenderTarget2D lightsTarget;
        static RenderTarget2D mainTarget;

        public RiverHollow()
        {
            _graphicsDeviceManager = new GraphicsDeviceManager(this)
            {
                IsFullScreen = true,
                HardwareModeSwitch = false,
                PreferredBackBufferWidth = ScreenWidth,
                PreferredBackBufferHeight = ScreenHeight
            };

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            Camera.SetViewport(GraphicsDevice.Viewport);
            InventoryManager.InitPlayerInventory();
            GoToInformation();
            
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
            MapManager.LoadContent(Content, GraphicsDevice);
            ObjectManager.LoadContent(Content);
            GUIManager.LoadContent();
            
            //TravelManager.Calculate();
            DropManager.LoadContent(Content);
            CutsceneManager.LoadContent(Content);
            GameManager.LoadQuests(Content);
            
            var pp = GraphicsDevice.PresentationParameters;
            lightsTarget = new RenderTarget2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight);
            mainTarget = new RenderTarget2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight);
            lightMask = GameContentManager.GetTexture(@"Textures\lightmask");
            effect1 = Content.Load<Effect>(@"lighteffect");

            PlayerManager.Initialize();

            MissionManager.Load();

            //Set the Main Menu Screen
            GUIManager.SetScreen(new IntroMenuScreen());
            DontReadInput();
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
                    if (!GUIManager.ProcessRightButtonClick(mousePoint) && OnMap())
                    {
                        //GUI does NOT use Camera translations
                        mousePoint.X = (int)((mousePoint.X - translate.X) / Scale);
                        mousePoint.Y = (int)((mousePoint.Y - translate.Y) / Scale);
                        if (IsRunning())
                        {
                            MapManager.ProcessRightButtonClick(mousePoint);
                        }
                    }
                }
                else if (ms.LeftButton == ButtonState.Pressed && GraphicCursor.LastMouseState.LeftButton == ButtonState.Released)
                {
                    if (!GUIManager.ProcessLeftButtonClick(mousePoint) && OnMap())
                    {
                        mousePoint.X = (int)((mousePoint.X - translate.X) / Scale);
                        mousePoint.Y = (int)((mousePoint.Y - translate.Y) / Scale);
                        if (IsRunning())
                        {
                            MapManager.ProcessLeftButtonClick(mousePoint);
                        }
                    }
                }
                else
                {
                    if (!GUIManager.ProcessHover(mousePoint))
                    {
                        mousePoint.X = (int)((mousePoint.X - translate.X) / Scale);
                        mousePoint.Y = (int)((mousePoint.Y - translate.Y) / Scale);
                        if (IsRunning())
                        {
                            MapManager.ProcessHover(mousePoint);
                        }
                    }
                }

                GraphicCursor.LastMouseState = ms;

                if (OnMap())
                {
                    Camera.Update(gameTime);
                    if (CutsceneManager.Playing) { CutsceneManager.Update(gameTime); }
                    else
                    {
                        if (IsRunning())
                        {
                            MapManager.Update(gameTime);
                            GameCalendar.Update(gameTime);
                            if (!Scrying()) { PlayerManager.Update(gameTime); }
                        }
                    }
                }

                base.Update(gameTime);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            //GraphicsDevice.SetRenderTarget(lightsTarget);
            //GraphicsDevice.Clear(Color.DarkGray);
            //spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Camera._transform);
            ////draw light mask where there should be torches etc...
            //spriteBatch.Draw(lightMask, new Vector2(800, 576), Color.White);
            //spriteBatch.End();

            GraphicsDevice.SetRenderTarget(mainTarget);
            GraphicsDevice.Clear(Color.Transparent);
            {
                spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Camera._transform);
                //If we're in an informational state, then only the GUIScreen data should be visible, don't draw anything except for the GUI
                if (!Informational())
                {
                    MapManager.DrawBase(spriteBatch);
                    PlayerManager.Draw(spriteBatch);
                }
                spriteBatch.End();
            }
            {
                spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
                if (!Informational())
                {
                    MapManager.DrawUpper(spriteBatch);
                }

                spriteBatch.End();

                GraphicsDevice.SetRenderTarget(null);
                GraphicsDevice.Clear(Color.Black);

                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null);
                //if (OnMap())
                //{
                //    effect1.Parameters["lightMask"].SetValue(lightsTarget);
                //    effect1.CurrentTechnique.Passes[0].Apply();
                //}
                spriteBatch.Draw(mainTarget, Vector2.Zero, Color.White);
                spriteBatch.End();

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null);

                GUIManager.Draw(spriteBatch);
                if (!Informational() && !InCombat())
                {
                    GameCalendar.Draw(spriteBatch);
                }
                spriteBatch.End();
            }
            base.Draw(gameTime);
        }

        public void HandleImportantInput()
        {
            if (!Informational() && !TakingInput())
            {
                if (OnMap() && InputManager.CheckPressedKey(Keys.Escape))
                {
                    if (!GUIManager.IsMenuScreenOpen())
                    {
                        GUIManager.OpenMenu();
                    }
                }
                if (InputManager.CheckPressedKey(Keys.P))
                {
                    if (IsPaused()) { Pause(); }
                    else { Unpause(); }
                }
                if (!GUIManager.IsItemCreationScreen() || !GUIManager.IsHUD())
                {
                    if (InputManager.CheckPressedKey(Keys.C))
                    {
                        if (GUIManager.IsItemCreationScreen())
                        {
                            GUIManager.SetScreen(new HUDScreen());
                        }
                        else
                        {
                           // GUIManager.SetScreen(new CraftingScreen());
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

        public static void NewGame(WorldAdventurer a, WorldAdventurer b)
        {
            PlayerManager.NewPlayer();
            MapManager.PopulateMaps(false);
            PlayerManager.Buildings[0].AddWorker(a);
            PlayerManager.Buildings[0].AddWorker(b);

            GameCalendar.NewCalendar();
            BackToMain();
        }

        public static void Rollover()
        {
            MissionManager.Rollover();
            PlayerManager.Rollover();
            foreach(Building b in PlayerManager.Buildings)
            {
                b.Rollover();
            }
            ObjectManager.Rollover();
            MapManager.Rollover();
        }

        public static void PrepExit()
        {
            _exit = true;
        }

        public static void HomeMapPlacement()
        {
            GUIManager.SetScreen(null);
            GameManager.Scry(true);
            Camera.UnsetObserver();
            MapManager.ViewMap(MapManager.HomeMap);
        }
    }
}
