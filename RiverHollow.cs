using RiverHollow.Game_Managers;
using RiverHollow.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Game_Managers.GUIObjects;

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
        private static bool _bLightingOn = false;
        private static bool _bExit = false;

        public GraphicsDeviceManager _graphicsDeviceManager;
        public SpriteBatch spriteBatch;
        public static int ScreenWidth = 1920;
        public static int ScreenHeight = 1080;

        //TEST
        public static Texture2D lightMask;
        public static Effect _effectLights;
        static RenderTarget2D _renderLights;
        static RenderTarget2D _renderMain;

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
            ZoneManager.Initialize();
            GoToInformation();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            InputManager.Load();
            SoundManager.LoadContent(Content);
            DataManager.LoadContent(Content);
            GameManager.LoadContent(Content);
            MapManager.LoadContent(Content, GraphicsDevice);
            MapManager.LoadObjects();

            GUIManager.LoadContent();

            CutsceneManager.LoadContent(Content);
            GameManager.LoadQuests(Content);

            var pp = GraphicsDevice.PresentationParameters;
            _renderLights = new RenderTarget2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight);
            _renderMain = new RenderTarget2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight);
            lightMask = DataManager.GetTexture(@"Textures\lightmask");
            _effectLights = Content.Load<Effect>(@"Effects\lighteffect");

            PlayerManager.Initialize();
            Camera.SetObserver(PlayerManager.World);

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

        protected override void Update(GameTime gTime)
        {
            if (this.IsActive)
            {
                if (_bExit)
                {
                    Exit();
                }
                MouseState ms = Mouse.GetState();
                KeyboardState ks = Keyboard.GetState();
                //If we're not in the game and we're not on an input screen, handle input
                HandleImportantInput();

                if (HarpManager.PlayingMusic)
                {
                    HarpManager.Update(gTime);
                }

                if (CombatManager.InCombat) { CombatManager.Update(gTime);}

                //GUIManager always needs to update, regardless of game state
                GUIManager.Update(gTime);

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
                    Camera.Update(gTime);
                    if (CutsceneManager.Playing) { CutsceneManager.Update(gTime); }
                    else
                    {
                        //Only update the player and the CurrentMap if the player is
                        //in combat while paused, or while the game is running
                        if (CombatManager.InCombat && !IsRunning())
                        {
                            MapManager.CurrentMap.Update(gTime);
                            foreach(CombatActor c in PlayerManager.GetParty())
                            {
                                c.Update(gTime);
                            }
                        }
                        else if (IsRunning())
                        {
                            MapManager.Update(gTime);
                            GameCalendar.Update(gTime);
                            if (!Scrying()) { PlayerManager.Update(gTime); }
                        }
                    }
                }

                base.Update(gTime);
            }
        }

        protected override void Draw(GameTime gTime)
        {
            if (_bLightingOn)
            {
                GraphicsDevice.SetRenderTarget(_renderLights);
                GraphicsDevice.Clear(GameCalendar.GetLightColor());
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, null, null, null, Camera._transform);
                //draw light mask where there should be torches etc...
                MapManager.DrawLights(spriteBatch);
                spriteBatch.End();
            }

            //This is when we start drawing the World
            //If we're in an informational state, then only the GUIScreen data should be visible, don't draw anything except for the GUI
            if (!Informational())
            {
                //Start rendering to the main target
                GraphicsDevice.SetRenderTarget(_renderMain);
                GraphicsDevice.Clear(Color.Transparent);

                spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Camera._transform);
                MapManager.DrawBase(spriteBatch);
                PlayerManager.Draw(spriteBatch);
                spriteBatch.End();

                spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Camera._transform);
                MapManager.DrawUpper(spriteBatch);
                CombatManager.DrawUpperCombatLayer(spriteBatch);
                spriteBatch.End();
            }

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            //This is the portion of code where we draw to the screen. If we are on the map, we want to apply the
            //lighting effect. Since we will be drawing on the _renderMain, the effectsfile.
            //testMask is the name of the texture contained in the _effectLights file.
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null);
            if (OnMap() && _bLightingOn)
            {
                _effectLights.Parameters["lightMask1"].SetValue(_renderLights);
                _effectLights.CurrentTechnique.Passes[0].Apply();
            }
            spriteBatch.Draw(_renderMain, Vector2.Zero, Color.White);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null);

            GUIManager.Draw(spriteBatch);
            if (!Informational())
            {
                GameCalendar.Draw(spriteBatch);
            }
            spriteBatch.End();
            base.Draw(gTime);
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

        public static void NewGame(Adventurer a, Adventurer b, bool playIntro)
        {
            PlayerManager.NewPlayer();
            MapManager.PopulateMaps(false);

            foreach(Villager v in DataManager.DiNPC.Values) {
                v.MoveToSpawn();
                v.CalculatePathing();
            }

            PlayerManager.Buildings[0].AddWorker(a);
            PlayerManager.Buildings[0].AddWorker(b);

            PlayerManager.AddToParty(a);
            PlayerManager.AddToParty(b);

            MapManager.Maps[PlayerManager.Buildings[0].MapName].AddBuildingObjectsToMap(PlayerManager.Buildings[0]);

            GameCalendar.NewCalendar();
            if (playIntro)
            {
                CutsceneManager.TriggerCutscene(1);
            }
            else
            {
                PlayerManager.AddToQuestLog(GameManager.DiQuests[2]);
            }
            BackToMain();
        }

        public static void Rollover()
        {
            MissionManager.Rollover();
            PlayerManager.Rollover();
            foreach (Building b in PlayerManager.Buildings)
            {
                b.Rollover();
            }

            foreach (Villager n in DataManager.DiNPC.Values)
            {
                n.RollOver();
            }
            MapManager.Rollover();
        }

        public static void PrepExit()
        {
            _bExit = true;
        }

        public static void HomeMapPlacement()
        {
            GUIManager.CloseMainObject();
            GameManager.Scry(true);
            Camera.UnsetObserver();
            MapManager.ViewMap(MapManager.HomeMap);
        }
    }
}
