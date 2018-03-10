using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Characters.CombatStuff;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.GUIObjects;
using RiverHollow.WorldObjects;
using System;
using System.Collections.Generic;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class CombatScreen : GUIScreen
    {
        const int _iPositions = 8;

        GUIImage _giBackground;
        BattleLocation[] _arrCombatants;
        GUITextWindow _gtwTextWindow;
        CmbtMenu _cmbtMenu;
        StatDisplay _sdStamina;

        int _iTarget = -1;
        bool _bDrawItem;

        public CombatScreen()
        {
            Mob m = CombatManager.CurrentMob;
            _giBackground = new GUIImage(Vector2.Zero, new Rectangle(0, 0, 800, 480), RiverHollow.ScreenWidth, RiverHollow.ScreenHeight, GameContentManager.GetTexture(@"Textures\battle"));
            Controls.Add(_giBackground);

            _sdStamina = new StatDisplay(StatDisplay.DisplayEnum.Energy);
            Controls.Add(_sdStamina);

            _cmbtMenu = new CmbtMenu();
            _arrCombatants = new BattleLocation[_iPositions];
            
            //Get the Players' party and assign each of them a battle position
            List<CombatCharacter> party = CombatManager.Party;
            for (int i = 0; i < party.Count; i++)
            {
                if (party[i] != null)
                {
                    bool even = (i % 2 == 0);
                    _arrCombatants[i] = new BattleLocation(new Vector2(100 + i * 200, even ? 700 : 600), party[i]);
                }

            }
            //Get the Enemies and assign each of them a battle position
            for (int i = 0; i < m.Monsters.Count; i++)
            {
                if (m.Monsters[i] != null)
                {
                    bool even = (i % 2 == 0);
                    _arrCombatants[(_iPositions / 2) + i] = new BattleLocation(new Vector2(1000 + i * 200, even ? 700 : 600), m.Monsters[i]);
                }
            }
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            //If the current Phase is skill selection, allow the user to pick a skill for the currentCharacter
            switch (CombatManager.CurrentPhase)
            {
                case CombatManager.PhaseEnum.SelectSkill:
                    rv = _cmbtMenu.ProcessLeftButtonClick(mouse);
                    break;

                case CombatManager.PhaseEnum.ChooseSkillTarget:
                    HandleLeftClickTargeting(true);
                    break;

                case CombatManager.PhaseEnum.ChooseItemTarget:
                    HandleLeftClickTargeting(false);
                    break;
            }
            
            return rv;
        }

        internal void HandleLeftClickTargeting(bool useSkill)
        {
            BattleLocation loc = _arrCombatants[_iTarget];
            loc.Selected = false;
            if (useSkill) { CombatManager.SetSkillTarget(loc); }
            else { CombatManager.SetItemTarget(loc); }
            _iTarget = -1;
        }

        //Right clicking will deselect the chosen skill
        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = true;
            CancelAction();
            return rv;
        }

        internal void CancelAction()
        {
            foreach (BattleLocation bl in _arrCombatants)
            {
                if (bl != null) { bl.Selected = false; }
            }

            _iTarget = -1;
            _cmbtMenu.ProcessRightButtonClick();
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;
            rv = _sdStamina.ProcessHover(mouse);
            if(CombatManager.PhaseChooseSkillTarget()){
                rv = HandleHoverTargeting();
            }
            if (CombatManager.PhaseChooseItemTarget())
            {
                rv = HandleHoverTargeting();
            }
            
            return rv;
        }

        internal bool HandleHoverTargeting()
        {
            bool rv = false;
            foreach (BattleLocation p in _arrCombatants)
            {
                if (rv) { break; }
                if (p != null && p.Occupied())
                {
                    rv = p.ProcessHover(GraphicCursor.Position.ToPoint());
                    if (rv)
                    {
                        int newTarget = Array.FindIndex<BattleLocation>(_arrCombatants, loc => loc == p);
                        if (newTarget != _iTarget)
                        {
                            _arrCombatants[_iTarget].Selected = false;
                            _iTarget = newTarget;
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
                case CombatManager.PhaseEnum.EnemyTurn:
                    _cmbtMenu.NewTurn();
                    CombatManager.SetSkillTarget(_arrCombatants[CombatManager.PlayerTarget]);
                    break;

                case CombatManager.PhaseEnum.NewTurn:
                    _cmbtMenu.NewTurn();
                    CombatManager.ChosenItem = null;
                    CombatManager.ChosenSkill = null;
                    CombatManager.CurrentPhase = CombatManager.PhaseEnum.SelectSkill;
                    break;

                case CombatManager.PhaseEnum.SelectSkill:
                    _cmbtMenu.SelectSkill();
                    break;

                case CombatManager.PhaseEnum.ChooseSkillTarget:
                    HandleUpdateTargeting(true);
                    break;

                case CombatManager.PhaseEnum.ChooseItemTarget:
                    HandleUpdateTargeting(false);
                    break;

                case CombatManager.PhaseEnum.DisplayAttack:
                    if (!string.IsNullOrEmpty(CombatManager.Text))
                    {
                        if (_gtwTextWindow == null) {
                            _gtwTextWindow = new GUITextWindow(CombatManager.Text, 0.5);
                            _gtwTextWindow.CenterOnScreen();
                        }
                        else
                        {
                            _gtwTextWindow.Update(gameTime);
                            if (_gtwTextWindow.Duration <= 0)
                            {
                                _gtwTextWindow = null;
                                CombatManager.CurrentPhase = CombatManager.PhaseEnum.PerformAction;
                            }
                        }
                    }
                    break;

                case CombatManager.PhaseEnum.PerformAction:
                    if (CombatManager.ChosenSkill != null) { CombatManager.ChosenSkill.HandlePhase(gameTime); }
                    else if (CombatManager.ChosenItem != null) {
                        bool finished = false;
                        CombatCharacter c = CombatManager.ActiveCharacter;
                        if (!c.IsCurrentAnimation("Cast"))
                        {
                            c.PlayAnimation("Cast");
                            _bDrawItem = true;
                        }
                        else if (c.AnimationPlayedXTimes(3))
                        {
                            c.PlayAnimation("Walk");
                            _bDrawItem = false;
                            finished = true;
                        }

                        if (finished) { CombatManager.UseItem(); }
                    }
                    break;
            }

            //Update everyone in the party's battleLocation
            foreach (BattleLocation p in _arrCombatants)
            {
                if (p != null && p.Occupied()) { p.Update(gameTime); }
            }

            //Update everyone in the enemys's battleLocation
            foreach (BattleLocation p in _arrCombatants)
            {
                if (p != null && p.Occupied())
                {
                    if (!CombatManager.Party.Contains(p.Character) && !CombatManager.Monsters.Contains(p.Character)) { p.Kill(); }
                    else { p.Update(gameTime); }
                }
            }

            //Cancel out of selections made if escape is hit
            if (InputManager.CheckKey(Keys.Escape))
            {
                CancelAction();
            }
        }

        public void HandleUpdateTargeting(bool useSkill)
        {
            if (_iTarget == -1)
            {
                _iTarget = SkipToNextTarget(useSkill ? _iPositions / 2 : 0, true);
            }

            if (InputManager.CheckKey(Keys.A) || InputManager.CheckKey(Keys.S))
            {
                int test = _iTarget - 1;
                if (test >= 0)
                {
                    MoveTarget(test, false);
                }
            }
            else if (InputManager.CheckKey(Keys.D) || InputManager.CheckKey(Keys.W))
            {
                int test = _iTarget + 1;
                if (test < _arrCombatants.Length)
                {
                    MoveTarget(test, true);
                }
            }
            _arrCombatants[_iTarget].Selected = true;

            if (InputManager.CheckKey(Keys.Enter))
            {
                BattleLocation loc = _arrCombatants[_iTarget];
                loc.Selected = false;
                if (useSkill) { CombatManager.SetSkillTarget(loc); }
                else { CombatManager.SetItemTarget(loc); }
                _iTarget = -1;
            }
        }

        internal void MoveTarget(int test, bool add)
        {
            test = SkipToNextTarget(test, add);
            if (test >= 0 && test < _arrCombatants.Length)
            {
                _arrCombatants[_iTarget].Selected = false;
                _iTarget = test;
            }
        }

        internal int SkipToNextTarget(int i, bool add)
        {
            int rv = -1;
            do
            {
                rv = add ? i++ : i--;
                if(rv < 0 || rv == _arrCombatants.Length) {
                    rv = -1;
                    break;
                }
            } while (_arrCombatants[rv] == null || !_arrCombatants[rv].Occupied());

            return rv;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            _cmbtMenu.Draw(spriteBatch, _arrCombatants);
            foreach (BattleLocation p in _arrCombatants)
            {
                if (p != null) { p.Draw(spriteBatch); }
            }

            if (_bDrawItem)     //We want to draw the item above the character's head
            {
                CombatCharacter c = CombatManager.ActiveCharacter;
                CombatItem useItem = CombatManager.ChosenItem;
                Point p = c.Position.ToPoint();
                p.X += c.Width / 2 - 16;
                useItem.Draw(spriteBatch, new Rectangle(p, new Point(32, 32)));
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
                Color c = Color.White;
                int tmp = _iDmg;
                if(_iDmg < 0) {
                    tmp *= -1;
                    c = Color.LightGreen;
                }
                spriteBatch.DrawString(_fDmg, tmp.ToString(), new Vector2(_vCenter.X, _vCenter.Y - (_iDmgTimer++) / 2), c);
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

        public void Heal(int x)
        {
            _iDmg = -x;
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
        public enum Display { None, Items, Spells }
        public Display DisplayType = Display.None;

        CmbtMenuWindow _gwMenu;
        CmbtStatusWin _statusWindow;
        CmbtUseMenuWindow _useMenuWindow;

        public CmbtMenu()
        {
            int totalMenuWidth = RiverHollow.ScreenWidth / 3;
            int menuSec = totalMenuWidth / 3;

            _gwMenu = new CmbtMenuWindow(totalMenuWidth, menuSec);
            _statusWindow = new CmbtStatusWin(menuSec * 2, _gwMenu.Height);
            _statusWindow.AnchorAndAlignToObject(_gwMenu, GUIObject.SideEnum.Right, GUIObject.SideEnum.Bottom);

            _useMenuWindow = new CmbtUseMenuWindow(totalMenuWidth, _gwMenu.Width + _statusWindow.Width + GUIWindow.GreyWin.Edge * 2);
        }

        public void Update(GameTime gameTime)
        {
            if (CombatManager.PhaseSelectSkill()) {
                if (DisplayType != Display.None) { _useMenuWindow.Update(gameTime); }
                else { _gwMenu.Update(gameTime); }
            }
            _statusWindow.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch, BattleLocation[] locations)
        {
            _statusWindow.Draw(spriteBatch, locations);
            _gwMenu.Draw(spriteBatch);
            if (DisplayType != Display.None)
            {
                _useMenuWindow.Draw(spriteBatch);
            }
        }

        internal void NewTurn()
        {
            DisplayType = Display.None;
            Clear();
            ClearState();
            _gwMenu.Assign(CombatManager.ActiveCharacter.AbilityList);
        }

        internal void SelectSkill()
        {
            ProcessActionChoice();
        }

        internal bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            if (DisplayType != Display.None)
            {
                rv = _useMenuWindow.ProcessLeftButtonClick(mouse);
            }
            else
            {
                rv = _gwMenu.ProcessLeftButtonClick(mouse);
            }
            ProcessActionChoice();

            return rv;
        }

        internal bool ProcessRightButtonClick()
        {
            bool rv = false;
            //If the current Phase is skill selection, allow the user to pick a skill for the currentCharacter
            if (CombatManager.PhaseChooseSkillTarget() || CombatManager.PhaseChooseItemTarget())
            {
                rv = true;

                _gwMenu.ClearChosenAbility();
                _useMenuWindow.ClearChoices();
                CombatManager.ChosenSkill = null;
                CombatManager.CurrentPhase = CombatManager.PhaseEnum.SelectSkill;
            }
            else if (DisplayType != Display.None)
            {
                ClearState();
                _useMenuWindow.Clear();
                DisplayType = Display.None;
            }
            return rv;
        }

        internal void ProcessActionChoice()
        {
            if (DisplayType != Display.None)
            {
                if (DisplayType == Display.Spells)
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
                else if (DisplayType == Display.Items)
                {
                    CombatItem it = _useMenuWindow.ChosenItem;
                    if(it != null)
                    {
                        CombatManager.ProcessItemChoice(it);
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
                        if (a.ActionID == 2)
                        {
                            DisplayType = Display.Spells;
                            _useMenuWindow.AssignSpells(CombatManager.ActiveCharacter.SpellList);
                        }
                        else if (a.ActionID == 3)
                        {
                            DisplayType = Display.Items;
                            _useMenuWindow.AssignItems(InventoryManager.GetPlayerCombatItems());
                        }
                    }
                }
            }
        }

        internal void ClearState()
        {
            DisplayType = Display.None;
            _gwMenu.ClearChosenAbility();
            _useMenuWindow.ClearChoices();
        }

        internal void Clear()
        {
            _gwMenu.Clear();
            _useMenuWindow.Clear();
        }
    }

    internal class CmbtStatusWin : GUIWindow
    {
        GUIImage _giCurrentTurn;
        SpriteFont _fFont;
        float _fCharacterHeight;

        public CmbtStatusWin(int width, int height)
        {
            _fFont = GameContentManager.GetFont(@"Fonts\MenuFont");
            _fCharacterHeight = _fFont.MeasureString("Q").Y;
            Width = width;
            Height = height;
            _winData = GreyWin;

            _giCurrentTurn = new GUIImage(new Vector2((int)Position().X + _iInnerBorder, (int)Position().Y + _iInnerBorder), new Rectangle(288, 96, 32, 32), (int)_fCharacterHeight, (int)_fCharacterHeight, @"Textures\Dialog");
        }

        public void Draw(SpriteBatch spriteBatch, BattleLocation[] locations)
        {
            base.Draw(spriteBatch);

            //MAR
            int xindex = (int)Position().X + _iInnerBorder;
            int yIndex = (int)Position().Y + _iInnerBorder;
            int lastHP = (int)Position().X + Width - _iInnerBorder;
            int lastMP = (int)(Position().X + Width - _iInnerBorder - _fFont.MeasureString("XXXX/XXXX").X);
            foreach (CombatCharacter p in PlayerManager.GetParty())
            {
                Color c = (CombatManager.ActiveCharacter == p) ? Color.Green : Color.White;
                spriteBatch.DrawString(_fFont, p.Name, new Vector2(xindex, yIndex), c);

                string strHp = string.Format("{0}/{1}", p.CurrentHP, p.MaxHP);
                int hpStart = lastHP - (int)_fFont.MeasureString(strHp).X;
                spriteBatch.DrawString(_fFont, strHp, new Vector2(hpStart, yIndex), c);

                string strMp = string.Format("{0}/{1}", p.CurrentMP, p.MaxMP);
                int mpStart = lastMP - (int)_fFont.MeasureString(strMp).X;
                spriteBatch.DrawString(_fFont, strMp, new Vector2(mpStart, yIndex), c);

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

            Width = width;
            Height = (int)(_vecMenuSize.Y * _iMaxMenuActions);
            _winData = GreyWin;

            //MAR
            Position(new Vector2(startX, RiverHollow.ScreenHeight - GreyWin.Edge - (_vecMenuSize.Y * _iMaxMenuActions) - RiverHollow.ScreenHeight / 100));

            _giSelection = new GUIImage(InnerTopLeft(), new Rectangle(288, 96, 32, 32), (int)_characterHeight, (int)_characterHeight, @"Textures\Dialog");
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
            if (CombatManager.ActiveCharacter.IsCombatAdventurer())
            {
                int xindex = (int)Position().X + _iInnerBorder;
                int yIndex = (int)Position().Y + _iInnerBorder;

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
        public enum Display { Items, Spells }
        Display _displayType;
        const int _iMaxMenuActions = 8;
        int _textColOne;
        int _textColTwo;
        int _selectWidth;
        List<CombatAction> _liActions;
        CombatAction _chosenAction;
        public CombatAction ChosenAction { get => _chosenAction; }
        List<CombatItem> _liItems;
        CombatItem _chosenItem;
        public CombatItem ChosenItem { get => _chosenItem; }
        Vector2 _vecMenuSize;
        SpriteFont _fFont;

        public CmbtUseMenuWindow(int startX, int width)
        {
            _iOptionsOffsetY = 0;
            _diOptions = new Dictionary<int, string>();
            _fFont = GameContentManager.GetFont(@"Fonts\MenuFont");
            _vecMenuSize = _fFont.MeasureString("XXXXXXXX");

            Width = width;
            Height = (int)(_vecMenuSize.Y * (_iMaxMenuActions/2));   //Two columns
            _winData = GreyWin;

            Position(new Vector2(startX, RiverHollow.ScreenHeight - GreyWin.Edge - (Height) - RiverHollow.ScreenHeight / 100));

            _giSelection = new GUIImage(InnerTopLeft(), new Rectangle(288, 96, 32, 32), (int)_characterHeight, (int)_characterHeight, @"Textures\Dialog");

            _selectWidth = _giSelection.Width;
            _textColOne = (int)Position().X + _iInnerBorder + _selectWidth;
            _textColTwo = (int)Position().X + _iInnerBorder + (Width / 2) + _selectWidth;
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
                        _giSelection.MoveImageTo(new Vector2(_textColOne - _selectWidth, _giSelection.Position().Y));
                        _giSelection.MoveImageBy(new Vector2(0, _characterHeight));
                    }
                    else
                    {
                        _giSelection.MoveImageTo(new Vector2(_textColTwo - _selectWidth, _giSelection.Position().Y));
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
                        _giSelection.MoveImageTo(new Vector2(_textColOne - _selectWidth, _giSelection.Position().Y));
                    }
                    else
                    {
                        _giSelection.MoveImageTo(new Vector2(_textColTwo - _selectWidth, _giSelection.Position().Y));
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
                    if (_iKeySelection - 2 >= 0 && GraphicCursor.Position.Y < _giSelection.Position().Y)
                    {
                        _giSelection.MoveImageBy(new Vector2(0, -_characterHeight));
                        _iKeySelection -= 2;
                    }
                    else if (_iKeySelection + 2 < _diOptions.Count && GraphicCursor.Position.Y > _giSelection.Position().Y + _giSelection.Height)
                    {
                        _giSelection.MoveImageBy(new Vector2(0, _characterHeight));
                        _iKeySelection += 2;
                    }
                    else if (_iKeySelection + 1 < _diOptions.Count && GraphicCursor.Position.Y >= _giSelection.Position().Y && GraphicCursor.Position.Y <= _giSelection.Position().Y + _giSelection.Height && GraphicCursor.Position.X > _textColTwo)
                    {
                        if (_iKeySelection % 2 == 0)
                        {
                            _giSelection.MoveImageTo(new Vector2(_textColTwo - _selectWidth, _giSelection.Position().Y));
                            _iKeySelection++;
                        }
                    }
                    else if (_iKeySelection - 1 >= 0 && GraphicCursor.Position.Y >= _giSelection.Position().Y && GraphicCursor.Position.Y <= _giSelection.Position().Y + _giSelection.Height && GraphicCursor.Position.X < _textColTwo)
                    {
                        if (_iKeySelection % 2 != 0)
                        {
                            _giSelection.MoveImageTo(new Vector2(_textColOne - _selectWidth, _giSelection.Position().Y));
                            _iKeySelection--;
                        }
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
            int yIndex = (int)Position().Y + _iInnerBorder;

            if (_diOptions.Count > 0) { _giSelection.Draw(spriteBatch); }

            yIndex += _iOptionsOffsetY;
            int i = Math.Max(0, _iKeySelection - _iMaxMenuActions);
            foreach (KeyValuePair<int, string> kvp in _diOptions)
            {
                if (kvp.Key >= i)
                {
                    string display = string.Empty;
                    Color c = Color.White;

                    if (_displayType == Display.Spells) {
                        int iMPCost = _liActions[kvp.Key].MPCost;
                        display = string.Format("{0}", iMPCost);
                        c = (_chosenAction != null && kvp.Value == _chosenAction.Name) ? Color.Green : CombatManager.ActiveCharacter.CanCast(iMPCost) ? Color.White : Color.Gray;
                    }
                    else
                    {
                        int num = _liItems[kvp.Key].Number;
                        display = string.Format("{0}", num);
                        c = (_chosenAction != null && kvp.Value == _chosenAction.Name) ? Color.Green : Color.White;
                    }

                    spriteBatch.DrawString(_fFont, kvp.Value, new Vector2(xindex, yIndex), c);
                    //Even numbered spell
                    if (kvp.Key % 2 == 0)
                    {
                        xindex = _textColTwo;
                        spriteBatch.DrawString(_fFont, display, new Vector2(xindex - _fFont.MeasureString(display).X - _selectWidth, yIndex), c);
                    }
                    else
                    {
                        spriteBatch.DrawString(_fFont, display, new Vector2(Position().X + Width - _fFont.MeasureString(display).X - _selectWidth, yIndex), c);

                        xindex = _textColOne;
                        yIndex += (int)_characterHeight;
                    }
                }
            }
        }

        public void AssignSpells(List<CombatAction> abilities)
        {
            int key = 0;
            _displayType = Display.Spells;
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
        public void AssignItems(List<CombatItem> items)
        {
            int key = 0;
            _displayType = Display.Items;
            if (_diOptions.Count == 0)
            {
                _liItems = items;
                _iKeySelection = 0;
                foreach (Item i in items)
                {
                    _diOptions.Add(key++, i.Name);
                }
            }
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
            if (_displayType == Display.Spells)
            {
                if (CombatManager.ActiveCharacter.CanCast(_liActions[_iKeySelection].MPCost))
                {
                    _chosenAction = _liActions[_iKeySelection];
                }
            }
            else if (_displayType == Display.Items)
            {
                _chosenItem = _liItems[_iKeySelection];
            }
        }

        public void ClearChoices()
        {
            _chosenAction = null;
            _chosenItem = null;
        }
    }
}
