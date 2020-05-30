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
                    if (!GUIManager.ProcessRightButtonClick(mousePoint) && IsMapShown())
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
                    if (!GUIManager.ProcessLeftButtonClick(mousePoint) && IsMapShown())
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

                if (IsMapShown())
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
            if (GameManager.IsMapShown())
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
            if (IsMapShown() && _bLightingOn)
            {
                _effectLights.Parameters["lightMask1"].SetValue(_renderLights);
                _effectLights.CurrentTechnique.Passes[0].Apply();
            }
            spriteBatch.Draw(_renderMain, Vector2.Zero, Color.White);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null);
            GUIManager.Draw(spriteBatch);
            spriteBatch.End();

            base.Draw(gTime);
        }

        public static void ResetCamera()
        {
            Camera.ResetObserver();
            MapManager.BackToPlayer();
        }

        public static void NewGame(Adventurer a, Adventurer b, bool playIntro)
        {
            PlayerManager.NewPlayer();
            MapManager.PopulateMaps(false);

            foreach (Villager v in DataManager.DiNPC.Values)
            {
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
            GoToHUDScreen();
        }

        /// <summary>
        /// Call this method to trigger all component to Rollover at the end of the day
        /// </summary>
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
            GameManager.Scry();
            MapManager.ViewMap(MapManager.HomeMap);
            Camera.UnsetObserver();
        }
    }
}
