using Adventure.Characters.Monsters;
using Adventure.Characters.NPCs;
using Adventure.GUIObjects;
using Adventure.Items;
using Adventure.Screens;
using Adventure.Tile_Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.ViewportAdapters;
using System;
using System.Collections.Generic;

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

        public ViewportAdapter ViewportAdapter { get; private set; }

        public Dictionary<string, TileMap> _tileMaps;
        public TileMap _currentMap;
        public Player _player;
        public Wizard _wiz;

        public InventoryDisplay inventoryDisplay;
        public GraphicCursor graphicCursor;

        private MouseState lastMouseState = new MouseState();

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
            ItemList.LoadContent(Content);
            spriteBatch = new SpriteBatch(GraphicsDevice);
            LoadMaps();
            GameCalendar.NewCalender(Content, SCREEN_WIDTH, SCREEN_HEIGHT);
            _player = new Player(Content);
            _wiz = new Wizard(new Vector2(300, 300), Content);
            _wiz.MakeDailyItem();

            inventoryDisplay = InventoryDisplay.GetInstance(Content, SCREEN_WIDTH);
            graphicCursor = GraphicCursor.GetInstance(Content);
        }

        public void LoadMaps()
        {
            _tileMaps = new Dictionary<string, TileMap>();
            TileMap newMap = new TileMap();
            newMap.LoadContent(Content, GraphicsDevice, @"Maps\Map1");
            _tileMaps.Add(newMap._name, newMap);

            newMap = new TileMap();
            newMap.LoadContent(Content, GraphicsDevice, @"Maps\Map2");
            _tileMaps.Add(newMap._name, newMap);

            _currentMap = _tileMaps[@"Maps\Map1"];
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

            if (Mouse.GetState().RightButton == ButtonState.Pressed && lastMouseState.RightButton == ButtonState.Released)
            {
                Point mousePoint = Mouse.GetState().Position;
                Vector3 translate = Camera._transform.Translation;

                mousePoint.X -= (int)translate.X;
                mousePoint.Y -= (int)translate.Y;
                if (_wiz.MouseInside(mousePoint) && _wiz.PlayerInRange(_player.GetRectangle()) &&
                    _player.HasSpaceInInventory(_wiz.WhatAreYouHolding()))
                {
                    _player.AddItemToFirstAvailableInventory(_wiz.TakeItem());
                    _wiz.MakeDailyItem();
                }
            }

            if (Mouse.GetState().LeftButton == ButtonState.Pressed && lastMouseState.LeftButton == ButtonState.Released)
            {
                //ToDo better processing logic but
                if (graphicCursor.HeldItem != null && inventoryDisplay.GiveItem(_player, graphicCursor.HeldItem))
                {
                    graphicCursor.DropItem();
                }
                else
                {
                    graphicCursor.GrabItem(inventoryDisplay.TakeItem(_player));
                }
            }

            lastMouseState = Mouse.GetState();

            if (!_paused)
            {
                GameCalendar.Update(gameTime);
                // TODO: Add your update logic here
                Camera.Update(gameTime, this);
                if(GameCalendar.CurrentHour == 2)
                {
                    RollOver();
                }

                _currentMap.Update(gameTime, _player);
                _player.Update(gameTime, _currentMap);
                if (!string.IsNullOrEmpty(_player.GoToMap))
                {
                    _currentMap = _tileMaps[@"Maps\"+_player.GoToMap];
                    _player.GoToMap = "";
                }
                _wiz.Update(gameTime, _currentMap);
            }

            inventoryDisplay.Update(gameTime, _player);
            graphicCursor.Update();

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

            _currentMap.Draw(spriteBatch);
            _wiz.Draw(spriteBatch);

            _player.Draw(gameTime, spriteBatch);

            spriteBatch.End();

            //TODO: Check if this is kosher
            spriteBatch.Begin();

            inventoryDisplay.Draw(spriteBatch);
            GameCalendar.Draw(spriteBatch);
            graphicCursor.Draw(spriteBatch);

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

        private void RollOver()
        {
            _wiz.MakeDailyItem();
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
