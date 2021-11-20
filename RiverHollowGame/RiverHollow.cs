using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.Screens;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class RiverHollow : Game
    {
        static bool _bExit = false;

        public const CombatStyleEnum COMBAT_STYLE = CombatStyleEnum.Lite;

        public GraphicsDeviceManager _graphicsDeviceManager;
        public SpriteBatch spriteBatch;
        public static int ScreenWidth = 1920;
        public static int ScreenHeight = 1080;

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
            GameManager.ShowMap();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            DungeonManager.Instantiate();
            InputManager.Load();
            SoundManager.LoadContent(Content);
            DataManager.LoadContent(Content);
            EnvironmentManager.Initialize();
            GameManager.Initialize();
            MapManager.LoadContent(Content, GraphicsDevice);
            MapManager.LoadObjects();

            GUIManager.LoadContent();

            CutsceneManager.LoadContent(Content);
            GameManager.LoadManagedDataLists();

            var pp = GraphicsDevice.PresentationParameters;
            _renderLights = new RenderTarget2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight);
            _renderMain = new RenderTarget2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight);
            lightMask = DataManager.GetTexture(@"Textures\lightmask");
            _effectLights = Content.Load<Effect>(@"Effects\lighteffect");

            PlayerManager.Initialize();

            MissionManager.Load();

            GUICursor.ResetCursor();
           // GameManager.Pause();
            //Set the Main Menu Screen
            GUIManager.SetScreen(new IntroMenuScreen());
            StopTakingInput();
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

                SoundManager.Update(gTime);

                if (HarpManager.PlayingMusic) { HarpManager.Update(gTime); }

                if (TacticalCombatManager.InCombat) { TacticalCombatManager.Update(gTime);}

                //If anything is queued up for pathing, handle it
                TravelManager.DequeuePathingRequest();

                //GUIManager always needs to update, regardless of game state
                GUIManager.Update(gTime);

                Point mousePoint = Mouse.GetState().Position;
                Vector3 translate = Camera._transform.Translation;

                if (ms.RightButton == ButtonState.Pressed && GUICursor.LastMouseState.RightButton == ButtonState.Released)
                {
                    if (!GUIManager.ProcessRightButtonClick(mousePoint) && IsMapShown())
                    {
                        //GUI does NOT use Camera translations
                        mousePoint.X = (int)((mousePoint.X - translate.X) / CurrentScale);
                        mousePoint.Y = (int)((mousePoint.Y - translate.Y) / CurrentScale);
                        if (IsRunning() || Scrying())
                        {
                            MapManager.ProcessRightButtonClick(mousePoint);
                        }
                    }
                }
                else if (ms.LeftButton == ButtonState.Pressed && GUICursor.LastMouseState.LeftButton == ButtonState.Released)
                {
                    if (!GUIManager.ProcessLeftButtonClick(mousePoint) && IsMapShown())
                    {
                        mousePoint.X = (int)((mousePoint.X - translate.X) / CurrentScale);
                        mousePoint.Y = (int)((mousePoint.Y - translate.Y) / CurrentScale);
                        if (IsRunning() || Scrying())
                        {
                            MapManager.ProcessLeftButtonClick(mousePoint);
                        }
                    }
                }
                else
                {
                    if (!GUIManager.ProcessHover(mousePoint))
                    {
                        mousePoint.X = (int)((mousePoint.X - translate.X) / CurrentScale);
                        mousePoint.Y = (int)((mousePoint.Y - translate.Y) / CurrentScale);
                        if (IsRunning() || Scrying())
                        {
                            MapManager.ProcessHover(mousePoint);
                        }
                    }
                }

                GUICursor.LastMouseState = ms;

                if (IsMapShown())
                {
                    Camera.Update(gTime);
                    GUICursor.UpdateTownBuildObject(gTime);
                    if (CutsceneManager.Playing) { CutsceneManager.Update(gTime); }
                    else
                    {
                        //During combat, if the game is not running, only update the CurrentMap
                        //Otherwise, if not in combat, or in combat and game is running, update all maps
                        //This is so that time does not pass outside of combat while decisions are being made
                        //but NPCs and animations will still run
                        if (TacticalCombatManager.InCombat && !IsRunning())
                        {
                            MapManager.CurrentMap.Update(gTime);
                            foreach(TacticalCombatActor c in PlayerManager.GetTacticalParty())
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
            //This is when we start drawing the World
            //If we're in an informational state, then only the GUIScreen data should be visible, don't draw anything except for the GUI
            if (GameManager.IsMapShown())
            {
                //Start rendering to the main target
                GraphicsDevice.SetRenderTarget(_renderMain);
                GraphicsDevice.Clear(Color.Transparent);

                spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Camera._transform);
                MapManager.DrawBase(spriteBatch);
                spriteBatch.End();

                spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Camera._transform);
                MapManager.DrawGround(spriteBatch);
                PlayerManager.Draw(spriteBatch);
                spriteBatch.End();

                spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Camera._transform);
                MapManager.DrawUpper(spriteBatch);
                TacticalCombatManager.DrawUpperCombatLayer(spriteBatch);
                spriteBatch.End();
            }
            else
            {
                GraphicsDevice.SetRenderTarget(_renderMain);
                GraphicsDevice.Clear(Color.Black);
            }

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            DrawLighting();

            //This is the portion of code where we draw to the screen. If we are on the map, we want to apply the
            //lighting effect. Since we will be drawing on the _renderMain, the effectsfile.
            //testMask is the name of the texture contained in the _effectLights file.
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null);
            if (IsMapShown() && LightingActive())
            {
                _effectLights.Parameters["lightMask"].SetValue(_renderLights);
                _effectLights.CurrentTechnique.Passes[0].Apply();
            }
            spriteBatch.Draw(_renderMain, Vector2.Zero, Color.White);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null);
            GUIManager.Draw(spriteBatch);
            spriteBatch.End();

            base.Draw(gTime);
        }

        private void DrawLighting()
        {
            if (LightingActive())
            {
                GraphicsDevice.SetRenderTarget(_renderLights);
                GraphicsDevice.Clear(EnvironmentManager.GetAmbientLight());
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Camera._transform);
                //draw light mask where there should be torches etc...
                MapManager.DrawLights(spriteBatch);
                spriteBatch.End();
                GraphicsDevice.SetRenderTarget(null);
            }
        }

        private bool LightingActive()
        {
            return GameCalendar.CurrentHour >= 18 && MapManager.CurrentMap.IsOutside;
        }

        public static void ResetCamera()
        {
            Camera.ResetObserver();
            MapManager.BackToPlayer();
        }

        /// <summary>
        /// Readies a new game by placing objects onto the map, and establishing a new calendar
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="playIntro"></param>
        public static void NewGame(Adventurer a, Adventurer b, bool playIntro)
        {
            PlayerManager.NewPlayer();
            MapManager.PopulateMaps(false);

           // PlayerManager._diBuildings[0].AddWorker(a);
            //PlayerManager._diBuildings[0].AddWorker(b);

            //PlayerManager.AddToParty(a);
            //PlayerManager.AddToParty(b);

            //MapManager.Maps[PlayerManager.Buildings[0].MapName].AddBuildingObjectsToMap(PlayerManager.Buildings[0]);

            GoToHUDScreen();
            GameCalendar.NewCalendar();

            CutsceneManager.TriggerCutscene(1);
            if (!playIntro) { CutsceneManager.SkipCutscene(); }

            StartGame();
        }

        /// <summary>
        /// Reads data fromt he indicated save file and loads the info to
        /// already previously loaded objects.
        /// </summary>
        /// <param name="savefile"></param>
        public static void LoadGame(string savefile)
        {
            SaveManager.Load(savefile);
            MapManager.PopulateMaps(true);

            GoToHUDScreen();
            StartGame();
        }

        /// <summary>
        /// Common method to call methods required by both Load and New game
        /// </summary>
        private static void StartGame()
        {
            SoundManager.PlayBackgroundMusic();

            //Places NPCs on the map
            foreach (Villager v in DataManager.DIVillagers.Values)
            {
                if (v.LivesInTown)
                {
                    v.MoveToSpawn();
                    v.DetermineValidSchedule();
                }
            }

            Camera.SetObserver(PlayerManager.World);
        }

        /// <summary>
        /// Call this method to trigger all component to Rollover at the end of the day
        /// </summary>
        public static void Rollover()
        {
            GameManager.RollOver();
            MissionManager.Rollover();
            PlayerManager.Rollover();

            foreach (Villager n in DataManager.DIVillagers.Values) {
                n.RollOver();
            }
            foreach (Merchant m in DataManager.DIMerchants.Values) {
                m.RollOver();
            }

            if (GameManager.MerchantQueue.Count > 0)
            {
                Merchant chosenMerchant = GameManager.MerchantQueue[0];
                chosenMerchant.ArriveInTown();
                GameManager.MerchantQueue.Remove(chosenMerchant);
            }

            MapManager.Rollover();
            DungeonManager.ResetDungeons();
        }

        public static void PrepExit()
        {
            _bExit = true;
        }
    }
}
