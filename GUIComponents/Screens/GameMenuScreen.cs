using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.GUIObjects;
using System.Collections.Generic;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class GameMenuScreen : GUIScreen
    {
        const int BTN_PADDING = 10;
        GUIButton _btnExitGame;
        GUIButton _btnQuestLog;
        GUIButton _btnInventory;
        GUIButton _btnParty;
        GUIButton _btnManagement;
        GUIButton _btnOptions;
        GUIButton _btnFriendship;
        GUIScreen _infoScreen;
        List<GUIObject> _liButtons;

        bool _open = false;
        bool _close = false;

        public GameMenuScreen()
        {
            _btnInventory = new GUIButton("Inventory", BtnInventory);
            _btnParty = new GUIButton("Party", BtnParty);
            _btnQuestLog = new GUIButton("Quest Log", BtnQuestLog);
            _btnExitGame = new GUIButton("Exit Game", BtnExitGame);
            _btnOptions = new GUIButton("Options", BtnOptions);
            _btnManagement = new GUIButton("Buildings", BtnManagement);
            _btnFriendship = new GUIButton("Friends", BtnFriendship);

            _liButtons = new List<GUIObject>() { _btnInventory, _btnParty, _btnManagement, _btnQuestLog, _btnOptions, _btnFriendship, _btnExitGame  };
            GUIObject.CreateSpacedColumn(ref _liButtons, -GUIButton.BTN_WIDTH, 0, RiverHollow.ScreenHeight, BTN_PADDING);
            foreach(GUIObject o in _liButtons) { Controls.Add(o); }

            GameManager.Pause();
            _open = true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            int _openingFinished = 0;
            foreach(GUIObject o in Controls)
            {
                int val = 0;
                if (_open)
                {
                    if (o.Position().X < 0) { val = 16; }
                }
                if (_close)
                {
                    if (o.Position().X > -GUIButton.BTN_WIDTH) { val = -16; }
                }

                Vector2 temp = o.Position();
                temp.X += val;
                o.Position(temp);
                if (_open && o.Position().X == 0) { _openingFinished++; }
                if (_close && o.Position().X == -GUIButton.BTN_WIDTH) { GameManager.BackToMain(); }
            }
            if(_openingFinished == _liButtons.Count) { _open = false; }
            if(_infoScreen != null)
            {
                _infoScreen.Update(gameTime);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_infoScreen != null)
            {
                _infoScreen.Draw(spriteBatch);
            }
            base.Draw(spriteBatch);
        }        

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (base.ProcessLeftButtonClick(mouse))
            {
                rv = true;
            }
            else
            {
                foreach (GUIObject c in Controls)
                {
                    rv = c.ProcessLeftButtonClick(mouse);
                    if (rv) { break; }
                }

                if (!rv && _infoScreen != null && _infoScreen.Contains(mouse))
                {
                    _infoScreen.ProcessLeftButtonClick(mouse);
                    rv = true;
                }
            }

            return rv;
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = true;
            if(_infoScreen == null)
            {
                _close = true;
            }
            else if (!_infoScreen.Contains(mouse))
            {
                _close = true;
            }
            else if (_infoScreen.Contains(mouse))
            {
                _infoScreen.ProcessRightButtonClick(mouse);
            }
            return rv;
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;
            if (!base.ProcessHover(mouse))
            {
                _btnExitGame.IsMouseHovering = _btnExitGame.Contains(mouse);
                _btnInventory.IsMouseHovering = _btnInventory.Contains(mouse);
                _btnParty.IsMouseHovering = _btnParty.Contains(mouse);
                if (_infoScreen != null)
                {
                    _infoScreen.ProcessHover(mouse);
                }
            }
            return rv;
        }

        #region Buttons
        public void BtnExitGame()
        {
            RiverHollow.PrepExit();
        }
        public void BtnInventory()
        {
            _infoScreen = new InventoryScreen();
        }
        public void BtnQuestLog()
        {
            _infoScreen = new QuestScreen();
        }
        public void BtnParty()
        {
            _infoScreen = new PartyScreen();
        }
        public void BtnOptions()
        {
            _infoScreen = new OptionScreen();
        }
        public void BtnManagement()
        {
            _infoScreen = new ManagementScreen();
        }
        public void BtnFriendship()
        {
            _infoScreen = new FriendshipScreen();
        }  
        #endregion

        public override bool IsGameMenuScreen() { return true; }
    }
}
