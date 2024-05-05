using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class RiverHollow : Game
    {
        public static RiverHollow Instance;

        static bool _bExit = false;

        public GraphicsDeviceManager _graphicsDeviceManager;
        public SpriteBatch spriteBatch;
        public static int ScreenWidth = 1920;
        public static int ScreenHeight = 1080;

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
            Instance = this;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            Camera.SetViewport(GraphicsDevice.Viewport);
            GameManager.ShowMap();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            LogManager.Initialize();

            GameManager.Initialize();
            DungeonManager.Instantiate();
            InputManager.Load();
            SoundManager.LoadContent(Content);
            DataManager.LoadContent(Content);
            EnvironmentManager.Initialize();
            MapManager.LoadContent(Content, GraphicsDevice);

            GUIManager.LoadContent();

            CutsceneManager.LoadContent(Content);

            var pp = GraphicsDevice.PresentationParameters;
            _renderLights = new RenderTarget2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight);
            _renderMain = new RenderTarget2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight);
            _effectLights = Content.Load<Effect>(@"Effects\lighteffect");

            PlayerManager.Initialize();
            TownManager.Initialize();
            InventoryManager.InitPlayerInventory();

            //Done here for the WorldObjects that need to be unlocked
            DataManager.SecondaryLoad(Content);
            GameManager.LoadManagedDataLists();
            MapManager.LoadObjects();

            TaskManager.Initialize();

            GUICursor.ResetCursor();
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
            if (IsActive)
            {
                if (_bExit)
                {
                    LogManager.CloseLogFile();
                    Exit();
                }

                SoundManager.Update(gTime);

                if (HarpManager.PlayingMusic) { HarpManager.Update(gTime); }

                //If anything is queued up for pathing, handle it
                TravelManager.DequeuePathingRequest();

                //GUIManager always needs to update, regardless of game state
                GUIManager.Update(gTime);

                Point mousePoint = Mouse.GetState().Position;
                Vector3 translate = Camera._transform.Translation;

                if (GameManager.HeldObject != null)
                {
                    var val = InputManager.ScrollWheelChanged();
                    if (val != 0 && GameManager.HeldObject.BuildableType(BuildableEnum.Decor))
                    {
                        Decor obj = (Decor)GameManager.HeldObject;
                        obj.Rotate(val > 0);
                    }
                }

                if (InputManager.ButtonPressed(ButtonEnum.Right, out _))
                {
                    if (!GUIManager.ProcessRightButtonClick(mousePoint) && IsMapShown())
                    {
                        //GUI does NOT use Camera translations
                        mousePoint.X = (int)((mousePoint.X - translate.X) / CurrentScale);
                        mousePoint.Y = (int)((mousePoint.Y - translate.Y) / CurrentScale);
                        if (!PlayerManager.Defeated() && (!GamePaused() || Scrying()))
                        {
                            MapManager.ProcessRightButtonClick(mousePoint);
                        }
                    }
                }
                else if (InputManager.ButtonPressed(ButtonEnum.Left, out bool wasInterval))
                {
                    if (wasInterval || (!GUIManager.ProcessLeftButtonClick(mousePoint) && IsMapShown()))
                    {
                        mousePoint.X = (int)((mousePoint.X - translate.X) / CurrentScale);
                        mousePoint.Y = (int)((mousePoint.Y - translate.Y) / CurrentScale);
                        if (!PlayerManager.Defeated() && (!GamePaused() || Scrying()))
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
                        if (!GamePaused() || Scrying())
                        {
                            MapManager.ProcessHover(mousePoint);
                        }
                    }
                    else
                    {
                        GUICursor.ResetCursor();
                    }
                }

                //Do not move this. Needs to be after checks
                InputManager.Update(gTime);

                if (IsMapShown())
                {
                    Camera.Update(gTime);
                    GUICursor.UpdateTownBuildObject(gTime);
                    if (CutsceneManager.Playing) { CutsceneManager.Update(gTime); }
                    else if (!GamePaused())
                    {
                        MapManager.Update(gTime);
                        GameCalendar.Update(gTime);
                        if (!Scrying()) { PlayerManager.Update(gTime); }
                        FishingManager.Update(gTime);
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

                if (!string.IsNullOrEmpty(MapManager.CurrentMap.MapBelow))
                {
                    spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Camera._transform);
                    MapManager.DrawBelowBase(spriteBatch);
                    MapManager.DrawBelowGround(spriteBatch);
                    MapManager.DrawBelowUpper(spriteBatch);
                    spriteBatch.End();
                }

                spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Camera._transform);
                MapManager.DrawBase(spriteBatch);
                spriteBatch.End();

                spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Camera._transform);
                MapManager.DrawGround(spriteBatch);
                PlayerManager.Draw(spriteBatch);
                FishingManager.Draw(spriteBatch);
                spriteBatch.End();

                spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Camera._transform);
                MapManager.DrawUpper(spriteBatch);
                spriteBatch.End();

                if (!string.IsNullOrEmpty(MapManager.CurrentMap.MapAbove))
                {
                    spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Camera._transform);
                    MapManager.DrawAboveBase(spriteBatch);
                    MapManager.DrawAboveGround(spriteBatch);
                    MapManager.DrawAboveUpper(spriteBatch);
                    spriteBatch.End();
                }
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
            if (IsMapShown() && EnvironmentManager.LightingActive())
            {
                _effectLights.Parameters["lightMask"].SetValue(_renderLights);
                _effectLights.CurrentTechnique.Passes[0].Apply();
            }
            spriteBatch.Draw(_renderMain, Vector2.Zero, Color.White);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null);
            GUIManager.Draw(spriteBatch);
            //fps.DrawFps(spriteBatch, DataManager.GetBitMapFont(DataManager.FONT_NEW), new Vector2(10f, 500f), Color.MonoGameOrange);
            spriteBatch.End();

            base.Draw(gTime);
        }

        private void DrawLighting()
        {
            if (EnvironmentManager.LightingActive())
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

        public void ResetCamera()
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
        public void NewGame(bool playIntro)
        {
            TownManager.NewGame();
            PlayerManager.NewPlayer();
            MapManager.PopulateMaps(true);

            GoToHUDScreen();
            GameCalendar.NewCalendar();

            CutsceneManager.TriggerCutscene(1);
            if (!playIntro)
            {
                CutsceneManager.SkipCutscene();
                GUIManager.BeginFadeIn();
            }

            StartGame();
        }

        /// <summary>
        /// Reads data from the indicated save file and loads the info to
        /// already previously loaded objects.
        /// </summary>
        /// <param name="savefile"></param>
        public void LoadGame(string savefile)
        {
            SaveManager.Load(savefile);

            MapManager.PopulateMaps(false);

            GoToHUDScreen();
            StartGame();
        }

        /// <summary>
        /// Common method to call methods required by both Load and New game
        /// </summary>
        private static void StartGame()
        {
            TaskManager.AssignTasks();
            SoundManager.PlayBackgroundMusic();

            //Places NPCs on the map
            foreach (Villager v in TownManager.Villagers.Values)
            {
                v.SetStartingLocation();
                v.CreateDailySchedule();
            }

            Camera.SetObserver(PlayerManager.PlayerActor);
        }

        /// <summary>
        /// Call this method to trigger all component to Rollover at the end of the day
        /// </summary>
        public static void Rollover()
        {
            TravelManager.Reset();
            TaskManager.Rollover();
            PlayerManager.Rollover();
            TownManager.Rollover();
            MapManager.Rollover();
            DungeonManager.ResetDungeons();
        }

        public static void PrepExit()
        {
            _bExit = true;
        }

        public void GoToTitle()
        {
            StopTakingInput();
            GUICursor.ResetCursor();
            SoundManager.StopAll();
            EnvironmentManager.UnloadEnvironment();
            GameManager.ExitTownMode();

            SaveManager.Initialize();
            GameManager.Initialize();
            DungeonManager.Instantiate();
            MapManager.LoadContent(Content, GraphicsDevice);

            CutsceneManager.LoadContent(Content);

            PlayerManager.Initialize();
            TownManager.Initialize();
            InventoryManager.InitPlayerInventory(false);

            //Done here for the WorldObjects that need to be unlocked
            DataManager.SecondaryLoad(Content);
            GameManager.LoadManagedDataLists();
            MapManager.LoadObjects();

            TaskManager.Initialize();
            TravelManager.Reset();

            //Set the Main Menu Screen
            GUIManager.SetScreen(new IntroMenuScreen());
        }
    }
}
