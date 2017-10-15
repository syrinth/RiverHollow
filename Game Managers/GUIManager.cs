using RiverHollow.Characters;
using RiverHollow.Characters.NPCs;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.Game_Managers.GUIComponents.Screens;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.Game_Managers.GUIObjects.Screens;
using RiverHollow.GUIObjects;
using RiverHollow.Items;
using RiverHollow.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiverHollow.Game_Managers
{
    public static class GUIManager
    {
        private static GUIScreen _currentGUIScreen;
        private static Screens _currentScreen;
        public static Screens CurrentGUIScreen { get => _currentScreen; }
        public  enum Screens {None, Combat, WorkerShop, BuildingShop, DayEnd, HUD, Inventory, ItemCreation, MainMenu,  Shop,  Text, TextInput };
        private static GUIImage _fadeImg;
        private static float _fadeVal = 1f;
        private static bool _fading = false;
        public static bool Fading { get => _fading; }

        public static void LoadContent()
        {
            _fadeImg = new GUIImage(new Vector2(0, 0), new Rectangle(160, 128, 32, 32), RiverHollow.ScreenWidth*2, RiverHollow.ScreenHeight*2, @"Textures\Dialog");
            GraphicCursor.LoadContent();
        }

        public static void Update(GameTime gameTime)
        {
            if (_fading)
            {
                UpdateFade();
            }
            if (_currentGUIScreen != null)
            {
                _currentGUIScreen.Update(gameTime);
            }
            GraphicCursor.Update();
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            if (_fading)
            {
                _fadeImg.Draw(spriteBatch, _fadeVal);

            }
            if (_currentGUIScreen != null)
            {
                _currentGUIScreen.Draw(spriteBatch);
            }

            GraphicCursor.Draw(spriteBatch);
        }

        public static bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (_currentGUIScreen != null)
            {
                rv = _currentGUIScreen.ProcessLeftButtonClick(mouse);
            }

            return rv;
        }

        public static bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = false;
            if (_currentGUIScreen != null)
            {
                rv = _currentGUIScreen.ProcessRightButtonClick(mouse);
            }

            return rv;
        }

        public static bool ProcessHover(Point mouse)
        {
            bool rv = false;
            if (_currentGUIScreen != null)
            {
                rv = _currentGUIScreen.ProcessHover(mouse);
            }

            return rv;
        }

        public static void SetScreen(Screens newScreen)
        {
            _currentScreen = newScreen;
            switch (newScreen)
            {
                case Screens.DayEnd:
                    _currentGUIScreen = new DayEndScreen();
                    return;
                case Screens.HUD:
                    _currentGUIScreen = new HUDScreen();
                    return;
                case Screens.Combat:
                    _currentGUIScreen = new CombatScreen();
                    return;
                case Screens.Inventory:
                    _currentGUIScreen = new InventoryScreen();
                    return;
                case Screens.ItemCreation:
                    _currentGUIScreen = new ItemCreationScreen();
                    return;
                case Screens.MainMenu:
                    _currentGUIScreen = new MainMenuScreen();
                    return;
                case Screens.None:
                    _currentGUIScreen = null;
                    return;
            }
        }

        public static void AddTextSelection(Food f, string text)
        {
            _currentGUIScreen.AddTextSelection(f, text);
        }

        public static void RemoveComponent(GUIObject g)
        {
            _currentGUIScreen.RemoveComponent(g);
        }

        public static void LoadScreen(Screens newScreen, Worker w)
        {
            _currentScreen = newScreen;
            switch (newScreen)
            {
                case Screens.TextInput:
                    _currentGUIScreen = new TextInputScreen(w);
                    return;
            }
        }

        public static void LoadScreen(Screens newScreen, List<Merchandise> merch)
        {
            GraphicCursor._currentType = GraphicCursor.CursorType.Normal;
            _currentScreen = newScreen;
            switch (newScreen)
            {
                case Screens.BuildingShop:
                    _currentGUIScreen = new PurchaseBuildingsScreen(merch);
                    return;
                case Screens.WorkerShop:
                    _currentGUIScreen = new PurchaseWorkersScreen(merch);
                    return;
            }
        }

        public static void LoadScreen(Screens newScreen, Container c)
        {
            _currentScreen = newScreen;
            switch (newScreen)
            {
                case Screens.Inventory:
                    _currentGUIScreen = new InventoryScreen(c);
                    return;
            }
        }

        public static void LoadScreen(Screens newScreen, string text)
        {
            _currentScreen = newScreen;
            switch (newScreen)
            {
                case Screens.Text:
                    _currentGUIScreen = new TextScreen(text);
                    return;
            }
        }

        public static void LoadScreen(Screens newScreen, NPC talker, string text)
        {
            _currentScreen = newScreen;
            switch (newScreen)
            {
                case Screens.Text:
                    _currentGUIScreen = new TextScreen(talker, text);
                    return;
            }
        }

        public static void LoadScreen(Screens newScreen, WorldCharacter c, string text)
        {
            //
        }

        public static void FadeOut()
        {
            _fading = true;
        }

        private static void UpdateFade()
        {
            _fadeVal -= 0.05f;
            if (_fadeVal <= 0)
            {
                _fadeVal = 1;
                _fading = false;
            }
        }
    }
}
