using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Characters;
using RiverHollow.Characters.CombatStuff;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.GUIObjects;
using RiverHollow.Misc;
using RiverHollow.SpriteAnimations;
using System;
using System.Collections.Generic;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class CombatScreen : GUIScreen
    {
        const int _iPositions = 4;

        GUIImage _giBackground;
        BattleLocation[] _arrParty;
        BattleLocation[] _arrEnemies;
        GUITextWindow _gtwTextWindow;
        CmbtMenu _cmbtMenu;
        StatDisplay _sdStamina;

        int _iTarget = -1;

        public CombatScreen()
        {
            Mob m = CombatManager.CurrentMob;
            _giBackground = new GUIImage(new Vector2(0, 0), new Rectangle(0, 0, 800, 480), RiverHollow.ScreenWidth, RiverHollow.ScreenHeight, GameContentManager.GetTexture(@"Textures\battle"));
            Controls.Add(_giBackground);

            _sdStamina = new StatDisplay(StatDisplay.Display.Energy, new Vector2(30, 30), 5);
            Controls.Add(_sdStamina);

            _cmbtMenu = new CmbtMenu();
            _arrParty = new BattleLocation[_iPositions];
            _arrEnemies = new BattleLocation[_iPositions];
            
            //Get the Players' party and assign each of them a battle position
            List<CombatCharacter> party = CombatManager.Party;
            for (int i = 0; i < party.Count; i++)
            {
                if (party[i] != null)
                {
                    bool even = (i % 2 == 0);
                    _arrParty[i] = new BattleLocation(new Vector2(100 + i * 200, even ? 700 : 600), party[i]);
                }

            }
            //Get the Enemies and assign each of them a battle position
            for (int i = 0; i < m.Monsters.Count; i++)
            {
                if (m.Monsters[i] != null)
                {
                    bool even = (i % 2 == 0);
                    _arrEnemies[i] = new BattleLocation(new Vector2(1000 + i * 200, even ? 700 : 600), m.Monsters[i]);
                }
            }
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            //If the current Phase is skill selection, allow the user to pick a skill for the currentCharacter
            switch (CombatManager.CurrentPhase)
            {
                case CombatManager.Phase.SelectSkill:
                    rv = _cmbtMenu.ProcessLeftButtonClick(mouse);

                    break;
                case CombatManager.Phase.ChooseSkillTarget:
                    BattleLocation loc = _arrEnemies[_iTarget];
                    loc.Selected = false;
                    CombatManager.SetSkillTarget(loc);
                    _iTarget = -1;

                    break;
            }
            
            return rv;
        }

        //Right clicking will deselectthe chosen skill
        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = false;

            rv = _cmbtMenu.ProcessRightButtonClick(mouse);

            return rv;
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;
            rv = _sdStamina.ProcessHover(mouse);
            if(CombatManager.CurrentPhase == CombatManager.Phase.ChooseSkillTarget){
                foreach (BattleLocation p in _arrParty)
                {
                    if (rv) { break; }
                    if (p != null) {
                        //rv = p.ProcessHover(GraphicCursor.Position.ToPoint());
                    }
                }
                foreach (BattleLocation p in _arrEnemies)
                {
                    if (rv) { break; }
                    if (p != null && p.Occupied()) {
                        rv = p.ProcessHover(GraphicCursor.Position.ToPoint());
                        if (rv)
                        {
                            int newTarget = Array.FindIndex<BattleLocation>(_arrEnemies, loc => loc == p);
                            if (newTarget != _iTarget)
                            {
                                _arrEnemies[_iTarget].Selected = false;
                                _iTarget = newTarget;
                            }
                        }
                    }
                }
            }

            return rv;
        }

        //First, call the update for the CombatManager
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            CombatManager.Update(gameTime);
            _cmbtMenu.Update(gameTime);

            switch (CombatManager.CurrentPhase)
            {
                case CombatManager.Phase.EnemyTurn:
                    _cmbtMenu.NewTurn();
                    CombatManager.SetSkillTarget(_arrParty[CombatManager.PlayerTarget]);
                    break;

                case CombatManager.Phase.NewTurn:
                    _cmbtMenu.NewTurn();
                    CombatManager.CurrentPhase = CombatManager.Phase.SelectSkill;
                    break;

                case CombatManager.Phase.SelectSkill:
                    _cmbtMenu.SelectSkill();
                    break;

                case CombatManager.Phase.ChooseSkillTarget:
                    if (_iTarget == -1) {
                        _iTarget = SkipToNextTarget(0, true);
                    }

                    if (InputManager.CheckKey(Keys.A) || InputManager.CheckKey(Keys.S))
                    {
                        int test = _iTarget - 1;
                        if (test >= 0)
                        {
                            test = SkipToNextTarget(test, false);
                            if (test >= 0 && test < _arrEnemies.Length)
                            {
                                _arrEnemies[_iTarget].Selected = false;
                                _iTarget = test;
                            }
                        }
                    }
                    else if (InputManager.CheckKey(Keys.D) || InputManager.CheckKey(Keys.W))
                    {
                        int test = _iTarget + 1;
                        if (test < _arrEnemies.Length)
                        {
                            test = SkipToNextTarget(test, true);
                            if (test >= 0 && test < _arrEnemies.Length)
                            {
                                _arrEnemies[_iTarget].Selected = false;
                                _iTarget = test;
                            }
                        }
                    }
                    _arrEnemies[_iTarget].Selected = true;

                    if (InputManager.CheckKey(Keys.Enter))
                    {
                        BattleLocation loc = _arrEnemies[_iTarget];
                        loc.Selected = false;
                        CombatManager.SetSkillTarget(loc);
                        _iTarget = -1;
                    }

                    break;

                case CombatManager.Phase.DisplayAttack:
                    if (!string.IsNullOrEmpty(CombatManager.Text))
                    {
                        if (_gtwTextWindow == null)
                        {
                            _gtwTextWindow = new GUITextWindow(CombatManager.Text, 0.5);
                        }
                        else
                        {
                            _gtwTextWindow.Update(gameTime);
                            if (_gtwTextWindow.Duration <= 0)
                            {
                                _gtwTextWindow = null;
                                CombatManager.CurrentPhase = CombatManager.Phase.UseSkill;
                            }
                        }
                    }

                    break;

                case CombatManager.Phase.UseSkill:
                    CombatManager.ChosenSkill.HandlePhase(gameTime);
                    break;
            }

            //Update everyone in the party's battleLocation
            foreach (BattleLocation p in _arrParty)
            {
                if (p != null && p.Occupied()) { p.Update(gameTime); }
            }

            //Update everyone in the enemys's battleLocation
            foreach (BattleLocation p in _arrEnemies)
            {
                if (p != null && p.Occupied())
                {
                    if (!CombatManager.Monsters.Contains(p.Character)) { p.Kill(); }
                    else { p.Update(gameTime); }
                }
            }
        }

        public int SkipToNextTarget(int i, bool add)
        {
            int rv = -1;
            do
            {
                rv = add ? i++ : i--;
                if(rv < 0 || rv == _arrEnemies.Length) {
                    rv = -1;
                    break;
                }
            } while (_arrEnemies[rv] == null || !_arrEnemies[rv].Occupied());

            return rv;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            _cmbtMenu.Draw(spriteBatch, _arrParty);
            foreach (BattleLocation p in _arrParty)
            {
                if (p != null) { p.Draw(spriteBatch); }
            }
            foreach (BattleLocation p in _arrEnemies)
            {
                if (p != null) { p.Draw(spriteBatch); }
            }

            if (_gtwTextWindow != null) { _gtwTextWindow.Draw(spriteBatch, true); }
            if (CombatManager.ChosenSkill != null && CombatManager.ChosenSkill.Sprite.IsAnimating) { CombatManager.ChosenSkill.Sprite.Draw(spriteBatch, false); }
        }  
    }

    public class BattleLocation
    {
        GUIImage _giTarget;
        CombatCharacter _character;
        public CombatCharacter Character { get => _character; }

        SpriteFont _fDmg;
        int _iDmg;
        int _iDmgTimer = 40;
        Vector2 _vCenter;
        public bool Selected;

        public BattleLocation(Vector2 vec, CombatCharacter c)
        {
            Selected = false;
            _character = c;
            _character.Position = vec;
            _vCenter = _character.Center;
            _fDmg = GameContentManager.GetFont(@"Fonts\Font");
            _giTarget = new GUIImage(_character.Position + new Vector2(50, -100), new Rectangle(256, 96, 32, 32), 32, 32, @"Textures\Dialog");
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Occupied()) { _character.Draw(spriteBatch, false); }

            if (Selected) { _giTarget.Draw(spriteBatch); }

            if (_iDmgTimer < 40)
            {
                spriteBatch.DrawString(_fDmg, _iDmg.ToString(), new Vector2(_vCenter.X, _vCenter.Y - (_iDmgTimer++) / 2), Color.White);
            }
        }

        public void Update(GameTime gameTime)
        {
            _character.Update(gameTime);
        }

        public void AssignDamage(int x)
        {
            _iDmg = x;
            _iDmgTimer = 0;
        }

        public bool Occupied()
        {
            return _character != null;
        }

        public bool Contains(Point mouse) { return Occupied() && _character.Contains(mouse); }

        public void Kill() { _character = null; }

        public bool ProcessHover(Point mouse)
        {
            bool rv = false;
            if (Contains(mouse))
            {
                Selected = true;
                rv = true;
            }

            return rv;
        }

        public Vector2 GetAttackVec(Vector2 from, Vector2 widthHeight)
        {
            Vector2 rv = _character.Position;
            int xOffset = _character.Width + 1;
            rv.X += _character.Center.X < from.X ? xOffset : -xOffset;
            rv.Y += _character.Height - widthHeight.Y;

            return rv;
        }
    }

    internal class CmbtMenu
    {
        CmbtMenuWindow _gwMenu;
        CmbtStatusWin _statusWindow;
        CmbtUseMenuWindow _useMenuWindow;

        public bool UseMenu = false;

        public CmbtMenu()
        {
            int totalMenuWidth = RiverHollow.ScreenWidth / 3;
            int menuSec = totalMenuWidth / 3;

            _gwMenu = new CmbtMenuWindow(totalMenuWidth, menuSec);
            _statusWindow = new CmbtStatusWin(_gwMenu.Position + new Vector2(_gwMenu.Width + GUIWindow.GreyDialogEdge * 2, 0), menuSec * 2, _gwMenu.Height);
            _useMenuWindow = new CmbtUseMenuWindow(totalMenuWidth, _gwMenu.Width + _statusWindow.Width + GUIWindow.GreyDialogEdge * 2);
        }

        public void Update(GameTime gameTime)
        {
            if (CombatManager.CurrentPhase == CombatManager.Phase.SelectSkill) {
                if (UseMenu) { _useMenuWindow.Update(gameTime); }
                else { _gwMenu.Update(gameTime); }
            }
            _statusWindow.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch, BattleLocation[] locations)
        {
            _statusWindow.Draw(spriteBatch, locations);
            _gwMenu.Draw(spriteBatch);
            if (UseMenu)
            {
                _useMenuWindow.Draw(spriteBatch);
            }
        }

        internal void NewTurn()
        {
            UseMenu = false;
            _useMenuWindow.ClearChosenAbility();
            _gwMenu.ClearChosenAbility();
            _gwMenu.Assign(CombatManager.ActiveCharacter.AbilityList);
        }

        internal void SelectSkill()
        {
            ProcessActionChoice();
        }

        internal bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            rv = _gwMenu.ProcessLeftButtonClick(mouse);

            ProcessActionChoice();

            return rv;
        }

        internal bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = false;
            //If the current Phase is skill selection, allow the user to pick a skill for the currentCharacter
            if (CombatManager.CurrentPhase == CombatManager.Phase.ChooseSkillTarget || UseMenu)
            {
                rv = true;

                ClearState();
                CombatManager.ChosenSkill = null;
                CombatManager.CurrentPhase = CombatManager.Phase.SelectSkill;

                UseMenu = false;
            }
            return rv;
        }

        internal void ProcessActionChoice()
        {
            if (UseMenu)
            {
                CombatAction a = _useMenuWindow.ChosenAction;
                if (a != null)
                {
                    if (CombatManager.ActiveCharacter.CanCast(a.MPCost))
                    {
                        CombatManager.ProcessActionChoice(a);
                    }
                }
            }
            else
            {
                MenuAction a = _gwMenu.ChosenAction;
                if (a != null)
                {
                    if (!a.IsMenu())
                    {
                        if (CombatManager.ActiveCharacter.CanCast(((CombatAction)a).MPCost))
                        {
                            CombatManager.ProcessActionChoice((CombatAction)a);
                        }
                    }
                    else
                    {
                        UseMenu = true;
                        _useMenuWindow.AssignSpells(CombatManager.ActiveCharacter.SpellList);
                    }
                }
            }
        }

        internal void ClearState()
        {
            UseMenu = false;
            _gwMenu.ClearChosenAbility();
            _useMenuWindow.ClearChosenAbility();
        }
    }

    internal class CmbtStatusWin : GUIWindow
    {
        GUIImage _giCurrentTurn;
        SpriteFont _fFont;
        float _fCharacterHeight;

        public CmbtStatusWin(Vector2 position, int width, int height)
        {
            _fFont = GameContentManager.GetFont(@"Fonts\MenuFont");
            _fCharacterHeight = _fFont.MeasureString("Q").Y;
            _width = width;
            _height = height;
            _edgeSize = GreyDialogEdge;
            _sourcePoint = GreyDialog;

            Position = position;

            _giCurrentTurn = new GUIImage(new Vector2((int)_position.X + _innerBorder, (int)_position.Y + _innerBorder), new Rectangle(288, 96, 32, 32), (int)_fCharacterHeight, (int)_fCharacterHeight, @"Textures\Dialog");
        }

        public void Draw(SpriteBatch spriteBatch, BattleLocation[] locations)
        {
            base.Draw(spriteBatch);
            int xindex = (int)_position.X + _innerBorder;
            int yIndex = (int)_position.Y + _innerBorder;
            int lastHP = (int)Position.X + _width - _innerBorder;
            int lastMP = (int)(Position.X + _width - _innerBorder - _fFont.MeasureString("XXXX/XXXX").X);
            foreach (BattleLocation bl in locations)
            {
                if (bl != null)
                {
                    Color c = (CombatManager.ActiveCharacter == bl.Character) ? Color.Green : Color.White;
                    spriteBatch.DrawString(_fFont, bl.Character.Name, new Vector2(xindex, yIndex), c);

                    string strHp = string.Format("{0}/{1}", bl.Character.CurrentHP, bl.Character.MaxHP);
                    int hpStart = lastHP - (int)_fFont.MeasureString(strHp).X;
                    spriteBatch.DrawString(_fFont, strHp, new Vector2(hpStart, yIndex), c);

                    string strMp = string.Format("{0}/{1}", bl.Character.CurrentMP, bl.Character.MaxMP);
                    int mpStart = lastMP - (int)_fFont.MeasureString(strMp).X;
                    spriteBatch.DrawString(_fFont, strMp, new Vector2(mpStart, yIndex), c);
                }
                yIndex += (int)_fCharacterHeight;
            }
        }
    }

    internal class CmbtMenuWindow : GUITextSelectionWindow
    {
        const int _iMaxMenuActions = 4;
        List<MenuAction> _liAbilities;
        MenuAction _chosenAction;
        public MenuAction ChosenAction { get => _chosenAction; }
        Vector2 _vecMenuSize;
        SpriteFont _fFont;

        public CmbtMenuWindow(int startX, int width)
        {
            _iOptionsOffsetY = 0;
            _diOptions = new Dictionary<int, string>();
            _fFont = GameContentManager.GetFont(@"Fonts\MenuFont");
            _vecMenuSize = _fFont.MeasureString("XXXXXXXX");

            _width = width;
            _height = (int)(_vecMenuSize.Y * _iMaxMenuActions);
            _edgeSize = GreyDialogEdge;
            _sourcePoint = GreyDialog;

            Position = new Vector2(startX, RiverHollow.ScreenHeight - GreyDialogEdge - (_vecMenuSize.Y * _iMaxMenuActions) - RiverHollow.ScreenHeight / 100);

            _giSelection = new GUIImage(new Vector2((int)_position.X + _innerBorder, (int)_position.Y + _innerBorder), new Rectangle(288, 96, 32, 32), (int)_characterHeight, (int)_characterHeight, @"Textures\Dialog");
        }

        public void Assign(List<MenuAction> abilities)
        {
            int key = 0;
            if (_diOptions.Count == 0)
            {
                _liAbilities = abilities;
                _iKeySelection = 0;
                foreach (MenuAction a in abilities)
                {
                    _diOptions.Add(key++, a.Name);
                }
            }
        }

        public void Clear()
        {
            _diOptions.Clear();
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (Contains(mouse))
            {
                SelectAction();
                rv = true;
            }
            return rv;
        }

        protected override void SelectAction()
        {
            _chosenAction = _liAbilities[_iKeySelection];
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.DrawWindow(spriteBatch);
            if (CombatManager.ActiveCharacter.GetType().Equals(typeof(CombatAdventurer)))
            {
                int xindex = (int)_position.X + _innerBorder;
                int yIndex = (int)_position.Y + _innerBorder;

                if (_diOptions.Count > 0) { _giSelection.Draw(spriteBatch); }

                xindex += 32;
                yIndex += _iOptionsOffsetY;
                int i = Math.Max(0, _iKeySelection - _iMaxMenuActions);
                foreach (KeyValuePair<int, string> kvp in _diOptions)
                {
                    if (kvp.Key >= i)
                    {
                        Color c = (_chosenAction != null && kvp.Value == _chosenAction.Name) ? Color.Green : Color.White;
                        spriteBatch.DrawString(_fFont, kvp.Value, new Vector2(xindex, yIndex), c);
                        yIndex += (int)_characterHeight;
                    }
                }
            }
        }

        public void ClearChosenAbility()
        {
            _chosenAction = null;
        }
    }

    internal class CmbtUseMenuWindow : GUITextSelectionWindow
    {
        const int _iMaxMenuActions = 8;
        int _textColOne;
        int _textColTwo;
        int _selectWidth;
        List<CombatAction> _liActions;
        CombatAction _chosenAction;
        public CombatAction ChosenAction { get => _chosenAction; }
        Vector2 _vecMenuSize;
        SpriteFont _fFont;

        public CmbtUseMenuWindow(int startX, int width)
        {
            _iOptionsOffsetY = 0;
            _diOptions = new Dictionary<int, string>();
            _fFont = GameContentManager.GetFont(@"Fonts\MenuFont");
            _vecMenuSize = _fFont.MeasureString("XXXXXXXX");

            _width = width;
            _height = (int)(_vecMenuSize.Y * (_iMaxMenuActions/2));   //Two columns
            _edgeSize = GreyDialogEdge;
            _sourcePoint = GreyDialog;

            Position = new Vector2(startX, RiverHollow.ScreenHeight - GreyDialogEdge - (_height) - RiverHollow.ScreenHeight / 100);

            _giSelection = new GUIImage(new Vector2((int)_position.X + _innerBorder, (int)_position.Y + _innerBorder), new Rectangle(288, 96, 32, 32), (int)_characterHeight, (int)_characterHeight, @"Textures\Dialog");

            _selectWidth = _giSelection.Width;
            _textColOne = (int)_position.X + _innerBorder + _selectWidth;
            _textColTwo = (int)_position.X + _innerBorder + (Width / 2) + _selectWidth;
        }

        public override void Update(GameTime gameTime)
        {
            if (InputManager.CheckKey(Keys.W) || InputManager.CheckKey(Keys.Up))
            {
                if (_iKeySelection - 2 >= 0)
                {
                    _giSelection.MoveImageBy(new Vector2(0, -_characterHeight));
                    _iKeySelection -= 2;
                }
            }
            else if (InputManager.CheckKey(Keys.S) || InputManager.CheckKey(Keys.Down))
            {
                if (_iKeySelection + 2 < _diOptions.Count)
                {
                    _giSelection.MoveImageBy(new Vector2(0, _characterHeight));
                    _iKeySelection += 2;
                }
            }
            else if (InputManager.CheckKey(Keys.D) || InputManager.CheckKey(Keys.Right))
            {
                int test = _iKeySelection + 1;
                if (test < _diOptions.Count)
                {
                    if(test % 2 == 0)   //moving to an even number, needs to move to firstCol
                    {
                        _giSelection.MoveImageTo(new Vector2(_textColOne - _selectWidth, _giSelection.Position.Y));
                        _giSelection.MoveImageBy(new Vector2(0, _characterHeight));
                    }
                    else
                    {
                        _giSelection.MoveImageTo(new Vector2(_textColTwo - _selectWidth, _giSelection.Position.Y));
                    }
                    
                    _iKeySelection += 1;
                }
            }
            else if (InputManager.CheckKey(Keys.A) || InputManager.CheckKey(Keys.Left))
            {
                int test = _iKeySelection - 1;
                if (test >= 0)
                {
                    if (test % 2 == 0)   //moving to an even number, needs to move to firstCol
                    {
                        _giSelection.MoveImageTo(new Vector2(_textColOne - _selectWidth, _giSelection.Position.Y));
                    }
                    else
                    {
                        _giSelection.MoveImageTo(new Vector2(_textColTwo - _selectWidth, _giSelection.Position.Y));
                        _giSelection.MoveImageBy(new Vector2(0, -_characterHeight));
                    }

                    _iKeySelection -= 1;
                }
            }
            else
            {
                //Until fixed for specific motion
                if (_poiMouse != GraphicCursor.Position.ToPoint() && Contains(GraphicCursor.Position.ToPoint()))
                {
                    _poiMouse = GraphicCursor.Position.ToPoint();
                    if (_iKeySelection - 1 >= 0 && GraphicCursor.Position.Y < _giSelection.Position.Y)
                    {
                        _giSelection.MoveImageBy(new Vector2(0, -_characterHeight));
                        _iKeySelection--;
                    }
                    else if (_iKeySelection + 1 < _diOptions.Count && GraphicCursor.Position.Y > _giSelection.Position.Y + _giSelection.Height)
                    {
                        _giSelection.MoveImageBy(new Vector2(0, _characterHeight));
                        _iKeySelection++;
                    }
                }
            }

            if (InputManager.CheckKey(Keys.Enter))
            {
                SelectAction();
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.DrawWindow(spriteBatch);
            int xindex = _textColOne;
            int yIndex = (int)_position.Y + _innerBorder;

            if (_diOptions.Count > 0) { _giSelection.Draw(spriteBatch); }

            yIndex += _iOptionsOffsetY;
            int i = Math.Max(0, _iKeySelection - _iMaxMenuActions);
            foreach (KeyValuePair<int, string> kvp in _diOptions)
            {
                if (kvp.Key >= i)
                {
                    int iMPCost = _liActions[kvp.Key].MPCost;
                    string mpCost = string.Format("{0}", iMPCost);

                    Color c = (_chosenAction != null && kvp.Value == _chosenAction.Name) ? Color.Green : CombatManager.ActiveCharacter.CanCast(iMPCost) ? Color.White : Color.Gray;

                    spriteBatch.DrawString(_fFont, kvp.Value, new Vector2(xindex, yIndex), c);
                    //Even numbered spell
                    if (kvp.Key % 2 == 0)
                    {
                        xindex = _textColTwo;
                        spriteBatch.DrawString(_fFont, mpCost, new Vector2(xindex - _fFont.MeasureString(mpCost).X - _selectWidth, yIndex), c);
                    }
                    else
                    {
                        spriteBatch.DrawString(_fFont, mpCost, new Vector2(_position.X + Width - _fFont.MeasureString(mpCost).X - _selectWidth, yIndex), c);

                        xindex = _textColOne;
                        yIndex += (int)_characterHeight;
                    }
                }

            }
        }

        public void AssignSpells(List<CombatAction> abilities)
        {
            int key = 0;
            if (_diOptions.Count == 0)
            {
                _liActions = abilities;
                _iKeySelection = 0;
                foreach (CombatAction s in abilities)
                {
                    _diOptions.Add(key++, s.Name);
                }
            }
        }

        public void Clear()
        {
            _diOptions.Clear();
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (Contains(mouse))
            {
                SelectAction();
                rv = true;
            }
            return rv;
        }

        protected override void SelectAction()
        {
            if (CombatManager.ActiveCharacter.CanCast(_liActions[_iKeySelection].MPCost)) { 
                _chosenAction = _liActions[_iKeySelection];
            }
        }

        public void ClearChosenAbility()
        {
            _chosenAction = null;
        }
    }

}
