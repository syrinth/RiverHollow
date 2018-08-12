﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Actors;
using RiverHollow.Actors.CombatStuff;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Game_Managers.GUIComponents.Screens;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.GUIObjects;
using RiverHollow.Misc;
using RiverHollow.WorldObjects;
using System;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.GUIObjects.GUIObject;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class CombatScreen : GUIScreen
    {
        double _dResultsTimer;
        GUIImage _giBackground;
        GUICmbtTile[,] _arrAllies;
        GUICmbtTile[,] _arrEnemies;
        GUITextWindow _gtwTextWindow;
        CmbtMenu _cmbtMenu;
        GUIStatDisplay _sdStamina;
        GUIButton _btnTurnOrder;
        GUIText _gResults;

        List<GUIText> _liTurns;

        bool _bTurns;

        public object UtilMapManager { get; private set; }

        public CombatScreen()
        {
            _liTurns = new List<GUIText>();

            _gResults = new GUIText();

            _giBackground = new GUIImage(Vector2.Zero, new Rectangle(0, 0, 800, 480), RiverHollow.ScreenWidth, RiverHollow.ScreenHeight, GameContentManager.GetTexture(@"Textures\battle"));
            Controls.Add(_giBackground);

            _sdStamina = new GUIStatDisplay(GUIStatDisplay.DisplayEnum.Energy);
            Controls.Add(_sdStamina);

            _btnTurnOrder = new GUIButton("Turns", 128, 32, BtnTurnOrder);
            _btnTurnOrder.AnchorToScreen(SideEnum.TopRight, 10);
            Controls.Add(_btnTurnOrder);

            _cmbtMenu = new CmbtMenu();

            CombatManager.ConfigureAllies(ref _arrAllies);
            CombatManager.ConfigureEnemies(ref _arrEnemies);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            _btnTurnOrder.ProcessLeftButtonClick(mouse);

            //If the current Phase is skill selection, allow the user to pick a skill for the currentCharacter
            switch (CombatManager.CurrentPhase)
            {
                case CombatManager.PhaseEnum.SelectSkill:
                    rv = _cmbtMenu.ProcessLeftButtonClick(mouse);
                    break;

                case CombatManager.PhaseEnum.ChooseTarget:
                    CombatManager.SelectedAction.SetSkillTarget();
                    break;
                case CombatManager.PhaseEnum.Defeat:
                    GUIManager.SlowFadeOut();
                    BackToMain();
                    MapManager.CurrentMap = MapManager.Maps["mapHospital"];
                    PlayerManager.CurrentMap = "mapHospital";
                    PlayerManager.World.Position = Util.SnapToGrid(MapManager.CurrentMap.DictionaryCharacterLayer["playerSpawn"]);
                    GUIManager.SetScreen(new TextScreen(CharacterManager.DiNPC[7], CharacterManager.DiNPC[7].GetDialogEntry("Healed")));
                    foreach(CombatAdventurer c in PlayerManager.GetParty())
                    {
                        c.ClearConditions();
                        c.IncreaseHealth((int)(c.MaxHP * 0.10));
                    }
                    
                    break;
            }
            
            return rv;
        }

        //Right clicking will deselect the chosen skill
        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = true;
            if (CombatManager.CanCancel())
            {
                CancelAction();
            }
            return rv;
        }

        internal void CancelAction()
        {
            RefreshTurnOrder();
            CombatManager.ClearSelectedTile();
            CombatManager.SelectedAction = null;
            _cmbtMenu.ProcessRightButtonClick();
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;
            rv = _sdStamina.ProcessHover(mouse);
            if(CombatManager.PhaseChooseTarget()){
                rv = HandleHoverTargeting();
            }
            else
            {
                bool loop = true;
                GUICmbtTile[,] array = _arrAllies;
                while (loop)
                {
                    foreach (GUICmbtTile t in array)
                    {
                        rv = t.ProcessHover(mouse);
                        if (rv)
                        {
                            goto Exit;
                        }
                    }
                    if (array != _arrEnemies) { array = _arrEnemies; }
                    else { loop = false; }
                }
            }
            Exit:
            
            return rv;
        }

        internal bool HandleHoverTargeting()
        {
            bool rv = false;

            if (CombatManager.SelectedAction.TargetsEnemy())
            {
                rv = HoverTargetHelper(_arrEnemies);
            }
            else if(CombatManager.SelectedAction.TargetsAlly())
            {
                rv = HoverTargetHelper(_arrAllies);
            }

            return rv;
        }

        internal bool HoverTargetHelper(GUICmbtTile[,] array)
        {
            bool rv = false;
            foreach (GUICmbtTile p in array)
            {
                if (rv) { break; }
                rv = p.CheckForTarget(GraphicCursor.Position.ToPoint());
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
                    RefreshTurnOrder();
                    CombatManager.SelectedAction.SetSkillTarget();
                    break;

                case CombatManager.PhaseEnum.NewTurn:
                    _cmbtMenu.NewTurn();
                    RefreshTurnOrder();
                    CombatManager.SelectedAction = null;
                    CombatManager.CurrentPhase = CombatManager.PhaseEnum.SelectSkill;
                    break;

                case CombatManager.PhaseEnum.SelectSkill:
                    _cmbtMenu.SelectSkill();
                    break;

                case CombatManager.PhaseEnum.ChooseTarget:
                    RefreshTurnOrder();
                    CombatManager.HandleKeyboardTargetting();
                    break;

                case CombatManager.PhaseEnum.DisplayAttack:
                    if (!string.IsNullOrEmpty(CombatManager.Text))
                    {
                        if (_gtwTextWindow == null)
                        {
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
                    if(CombatManager.SelectedAction != null)
                    {
                        CombatManager.SelectedAction.PerformAction(gameTime);
                    }
                    
                    break;
                case CombatManager.PhaseEnum.DisplayXP:
                    if(UpdateResults(gameTime, CombatManager.EarnedXP + " Exp. Earned"))
                    {
                        CombatManager.CurrentPhase = CombatManager.PhaseEnum.DisplayLevels;
                    }
                    
                    break;
                case CombatManager.PhaseEnum.DisplayLevels:
                    if (CombatManager.LiLevels.Count > 0)
                    {
                        if (UpdateResults(gameTime, CombatManager.LiLevels[0]))
                        {
                            CombatManager.LiLevels.RemoveAt(0);
                        }
                    }
                    else
                    {
                        CombatManager.EndCombatVictory();
                    }
                    break;
                case CombatManager.PhaseEnum.Defeat:
                    GUITextWindow window = new GUITextWindow("Defeated");
                    window.CenterOnScreen();
                    Controls.Add(window);
                    break;
            }

            foreach (GUICmbtTile location in _arrAllies)
            {
                location.Update(gameTime);
            }

            //Update everyone in the party's battleLocation
            foreach (GUICmbtTile p in _arrEnemies)
            {
                p.Update(gameTime);
            }

            //Cancel out of selections made if escape is hit
            if (InputManager.CheckPressedKey(Keys.Escape))
            {
                CancelAction();
            }
        }

        private bool UpdateResults(GameTime gameTime, string str)
        {
            bool rv = false;
            if (String.IsNullOrEmpty(_gResults.Text))
            {
                _gResults.SetText(str);
                _gResults.CenterOnScreen();
                _dResultsTimer = 2.0f;
            }
            else
            {
                _dResultsTimer -= gameTime.ElapsedGameTime.TotalSeconds;
                if (_dResultsTimer > 0)
                {
                    _gResults.MoveBy(0, -1);
                }
                else
                {
                    _gResults.SetText("");
                    _dResultsTimer = 0;
                    rv = true;
                }
            }

            return rv;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            _cmbtMenu.Draw(spriteBatch);
            if (!String.IsNullOrEmpty(_gResults.Text)) { _gResults.Draw(spriteBatch); }

            Draw(spriteBatch, false);
            Draw(spriteBatch, true);

            if (CombatManager.SelectedAction != null)
            {
                CombatManager.SelectedAction.Draw(spriteBatch);
            }

            if (_gtwTextWindow != null) { _gtwTextWindow.Draw(spriteBatch); }

            foreach(GUIText t in _liTurns)
            {
                t.Draw(spriteBatch);
            }
            
        }

        private void Draw(SpriteBatch spriteBatch, bool drawCharacter)
        {
            bool loop = true;
            GUICmbtTile[,] array = _arrAllies;
            while (loop)
            {
                foreach (GUICmbtTile t in array)
                {
                    if (t != null) {
                        if (drawCharacter) { t.DrawCharacter(spriteBatch); }
                        else { t.Draw(spriteBatch); }
                    }
                }
                foreach (GUICmbtTile t in array)
                {
                    if (t != null) { t.DrawDescriptions(spriteBatch); }
                }
                if (array != _arrEnemies) { array = _arrEnemies; }
                else { loop = false; }
            }
        }

        public void BtnTurnOrder()
        {
            _bTurns = !_bTurns;

            if (_bTurns) { DisplayTurnOrder(); }
            else { _liTurns.Clear(); }
        }
        private void RefreshTurnOrder()
        {
            if (_bTurns)
            {
                _liTurns.Clear();
                DisplayTurnOrder();
            }
        }

        private void DisplayTurnOrder()
        {
            List<string> turnOrder = CombatManager.CalculateTurnOrder();
            for (int i = 0; i < turnOrder.Count; i++)
            {
                GUIText temp = new GUIText(i.ToString() + "-" + turnOrder[i]);
                _liTurns.Add(temp);
                if (i == 0) { temp.AnchorAndAlignToObject(_btnTurnOrder, SideEnum.Bottom, SideEnum.Left); }
                else { temp.AnchorAndAlignToObject(_liTurns[i - 1], SideEnum.Bottom, SideEnum.Left); }
            }
        }
    }

    public class GUICmbtTile : GUIObject
    {
        GUIStatDisplay _gHP;
        GUIImage _gTargetter;
        GUIImage _gTile;
        GUISprite _gSprite;
        public GUISprite CharacterSprite => _gSprite;
        GUIText _gEffect;
        GUISprite _gSummon;
        public GUISprite SummonSprite => _gSummon;
        GUIText _gSummonEffect;

        List<GUIStatus> _liStatus;

        CombatManager.CombatTile _mapTile;
        public CombatManager.CombatTile MapTile => _mapTile;

        SpriteFont _fDmg;
        int _iDmgTimer = 40;

        public GUICmbtTile(CombatManager.CombatTile tile)
        {
            _mapTile = tile;
            _mapTile.AssignGUITile(this);
            _fDmg = GameContentManager.GetFont(@"Fonts\Font");

            _gTile = new GUIImage(Vector2.Zero, new Rectangle(128, 0, 32, 32), 32, 32, @"Textures\Dialog");
            _gTile.SetScale(CombatManager.CombatScale);
            _gTargetter = new GUIImage(Vector2.Zero, new Rectangle(256, 96, 32, 32), 32, 32, @"Textures\Dialog");

            _liStatus = new List<GUIStatus>();

            Setup();

            Width = _gTile.Width;
            Height = _gTile.Height;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if(CombatManager.CurrentPhase == CombatManager.PhaseEnum.ChooseTarget)
            {
                _gTile.Alpha = CombatManager.SelectedAction.LegalTiles.Contains(_mapTile) ? 1 : 0.5f;
            }
            else
            {
                _gTile.Alpha = 1;
            }

            if (CombatManager.CurrentPhase != CombatManager.PhaseEnum.PerformAction && CombatManager.ActiveCharacter != null && CombatManager.ActiveCharacter == _mapTile.Character) {
                _gTile.SetColor(Color.Yellow);
            }
            else if (CombatManager.SelectedAction != null)
            {
                _gTile.SetColor(CombatManager.SelectedAction.InArea(_mapTile) ? Color.Red : Color.White);
            }
            else if (CombatManager.SelectedAction == null) { _gTile.SetColor(Color.White); }

            _gTile.Draw(spriteBatch);

            if (_mapTile.Selected) { _gTargetter.Draw(spriteBatch); }

            if (_gEffect != null && _iDmgTimer < 40)
            {
                _gEffect.Draw(spriteBatch);
            }
            if (_gSummonEffect != null && _iDmgTimer < 40)
            {
                _gSummonEffect.Draw(spriteBatch);
            }
        }
        public void DrawCharacter(SpriteBatch spriteBatch)
        {
            if (Occupied())
            {
                if(_gSummon != null) { _gSummon.Draw(spriteBatch); }
                _gSprite.Draw(spriteBatch);
                if (!(CombatManager.CurrentPhase == CombatManager.PhaseEnum.PerformAction && CombatManager.ActiveCharacter == _mapTile.Character))
                {
                    _gHP.Draw(spriteBatch);
                }
            }
        }

        public void DrawDescriptions(SpriteBatch spriteBatch)
        {
            if (Occupied())
            {
                foreach (GUIStatus stat in _liStatus)
                {
                    if (_mapTile.Character.DiConditions[stat.Status])
                    {
                        stat.Draw(spriteBatch);
                    }
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (Occupied())
            {
                if (_gSummon != null) { _gSummon.Update(gameTime); }
                _gSprite.Update(gameTime);
                _gHP.Update(gameTime);
            }

            if(_gEffect != null)
            {
                if (_iDmgTimer < 40)
                {
                    _gEffect.MoveBy(0, -1);
                    _iDmgTimer++;

                    if (_gSummonEffect != null) { _gSummonEffect.MoveBy(0, -1); }
                }
                else if (Occupied())
                {
                    if (!String.IsNullOrEmpty(_gEffect.Text))
                    {
                        _gEffect.SetText("");
                        _gEffect.AnchorAndAlignToObject(_gSprite, SideEnum.Top, SideEnum.CenterX);
                    }
                    if (_gSummonEffect != null && !String.IsNullOrEmpty(_gSummonEffect.Text))
                    {
                        _gSummonEffect.SetText("");
                        _gSummonEffect.AnchorAndAlignToObject(_gSprite, SideEnum.Top, SideEnum.CenterX);
                    }
                }
                else { _gEffect = null; }
            }
        }

        private void Setup()
        {
            _gTargetter.AnchorAndAlignToObject(_gTile, SideEnum.Top, SideEnum.CenterX, 30);
            if (Occupied())
            {
                _gSprite.CenterOnObject(_gTile);
                _gSprite.MoveBy(0, -(_gTile.Height / 3));
                _gHP.AnchorAndAlignToObject(_gSprite, SideEnum.Bottom, SideEnum.CenterX);
                _gEffect = new GUIText();
                _gEffect.AnchorAndAlignToObject(_gSprite, SideEnum.Top, SideEnum.CenterX);

                for (int i = 0; i < _liStatus.Count; i++)
                {
                    GUIStatus temp = _liStatus[i];
                    if (i == 0) { temp.AnchorAndAlignToObject(_gHP, SideEnum.Bottom, SideEnum.Left); }
                    else { temp.AnchorAndAlignToObject(_liStatus[i - 1], SideEnum.Right, SideEnum.Bottom); }
                }
            }
        }

        public void SyncGUIObjects(bool occupied)
        {
            if (occupied)
            {
                _gSprite = new GUISprite(_mapTile.Character.BodySprite);
                _gSprite.PlayAnimation(AnimationEnum.Walk);
                _gHP = new GUIStatDisplay(GUIStatDisplay.DisplayEnum.Health, _mapTile.Character, 100);
            }
            else
            {
                _gSprite = null;
                _gHP = null;
            }
            Setup();
        }
        public void LinkSummon(Summon s)
        {
            if (s != null) {
                _gSummon = new GUISprite(s.BodySprite);
                _gSummon.AnchorToObject(_gSprite, SideEnum.Top);
                _gSummon.AnchorToObject(_gSprite, SideEnum.Left);
                _gSummonEffect = new GUIText();
                _gSummonEffect.AnchorAndAlignToObject(_gSummon, SideEnum.Top, SideEnum.CenterX);
            }
            else
            {
                _gSummon = null;
                _gSummonEffect = null;
            }
        }

        public void AssignEffect(int x, bool isNegative)
        {
            AssignEffect(x.ToString(), isNegative);
        }
        public void AssignEffect(string x, bool isNegative)
        {
            if (_mapTile.TargetPlayer)
            {
                _iDmgTimer = 0;
                _gEffect.SetText(x);
                _gEffect.SetColor(isNegative ? Color.Red : Color.LightGreen);
            }
            else
            {
                AssignEffectToSummon(x);
            }
        }

        public void AssignEffectToSummon(string x)
        {
            _iDmgTimer = 0;
            _gSummonEffect.SetText(x);
            _gSummonEffect.SetColor(Color.Red);
        }

        public void ChangeCondition(ConditionEnum c, TargetEnum target)
        {
            GUIStatus found = _liStatus.Find(test => test.Status == c);
            if (target.Equals(TargetEnum.Enemy) && found == null)
            {
                _liStatus.Add(new GUIStatus(c));
            }
            else if(target.Equals(TargetEnum.Ally) && found != null)
            {
                _liStatus.Remove(found);
            }

            _liStatus.Sort((x, y) => x.Status.CompareTo(y.Status));
            for(int i = 0; i< _liStatus.Count; i++)
            {
                if(i == 0) { _liStatus[i].AnchorAndAlignToObject(_gHP, SideEnum.Bottom, SideEnum.Left); }
                else { _liStatus[i].AnchorAndAlignToObject(_liStatus[i - 1], SideEnum.Right, SideEnum.Bottom); }
            }
        }

        public bool Occupied()
        {
            return _mapTile.Occupied();
        }

        public override bool Contains(Point mouse) {
            bool rv = false;

            rv = _gTile.Contains(mouse) || (Occupied() && _gSprite.Contains(mouse));

            return rv;
        }

        public bool ProcessHover(Point mouse)
        {
            bool rv = false;

            foreach (GUIStatus s in _liStatus)
            {
                s.ProcessHover(mouse);
            }

            if (_gHP != null){ _gHP.ProcessHover(mouse); }

            return rv;
        }

        public bool CheckForTarget(Point mouse)
        {
            bool rv = false;
            if (Contains(mouse))
            {
                if (CombatManager.PhaseChooseTarget())
                {
                    CombatManager.TestHoverTile(_mapTile);
                    rv = true;
                }
            }

            return rv;
        }

        //public Vector2 GetAttackVec(Vector2 from, Vector2 widthHeight)
        //{
        //    Vector2 rv = _character.Position;
        //    int xOffset = _character.Width + 1;
        //    rv.X += _character.Center.X < from.X ? xOffset : -xOffset;
        //    rv.Y += _character.Height - widthHeight.Y;

        //    return rv;
        //}

        public override void Position(Vector2 value)
        {
            base.Position(value);
            _gTile.Position(value);
            Setup();
        }
        public Vector2 GetCharacterPosition()
        {
            Vector2 rv = Vector2.Zero;
            if(_gSprite != null)
            {
                rv = _gSprite.Position();
            }
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

        GUIButton _btnMove;

        public CmbtMenu()
        {
            int totalMenuWidth = RiverHollow.ScreenWidth / 3;
            int menuSec = totalMenuWidth / 3;

            _gwMenu = new CmbtMenuWindow(totalMenuWidth, menuSec);
            _statusWindow = new CmbtStatusWin(menuSec * 2, _gwMenu.Height);
            _statusWindow.AnchorAndAlignToObject(_gwMenu, SideEnum.Right, SideEnum.Bottom);

            _useMenuWindow = new CmbtUseMenuWindow(totalMenuWidth, _gwMenu.Width + _statusWindow.Width + GUIWindow.GreyWin.Edge * 2);

            _btnMove = new GUIButton(new Rectangle(160, 0, 16, 16), 32, 32, @"Textures\Dialog", BtnMove);
            _btnMove.AnchorAndAlignToObject(_gwMenu, SideEnum.Left, SideEnum.Bottom);
        }

        public void Update(GameTime gameTime)
        {
            if (CombatManager.PhaseSelectSkill())
            {
                if (DisplayType != Display.None) { _useMenuWindow.Update(gameTime); }
                else { _gwMenu.Update(gameTime); }
            }
            _statusWindow.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _btnMove.Draw(spriteBatch);
            _statusWindow.Draw(spriteBatch);
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

            _btnMove.ProcessLeftButtonClick(mouse);

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
            if (CombatManager.PhaseChooseTarget())
            {
                rv = true;

                _gwMenu.ClearChosenAbility();
                _useMenuWindow.ClearChoices();
                CombatManager.SelectedAction = null;
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
                    if (it != null)
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
                        if (a.IsMenu() && a.IsCastSpell() && !CombatManager.ActiveCharacter.Silenced())
                        {
                            DisplayType = Display.Spells;
                            _useMenuWindow.AssignSpells(CombatManager.ActiveCharacter.SpellList);
                        }
                        else if (a.IsMenu() && a.IsUseItem())
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

        internal void BtnMove()
        {
            CombatManager.ProcessActionChoice((CombatAction)CharacterManager.GetActionByIndex(4));
        }
    }

    internal class CmbtStatusWin : GUIWindow
    {
        GUIImage _giCurrentTurn;
        SpriteFont _fFont;
        float _fCharacterHeight;
        GUIText[] _arNames;
        GUIText[] _arMana;
        GUIText[] _arHp;

        public CmbtStatusWin(int width, int height)
        {
            _fFont = GameContentManager.GetFont(@"Fonts\MenuFont");
            _fCharacterHeight = _fFont.MeasureString("Q").Y;
            Width = width;
            Height = height;
            _winData = GreyWin;

            _giCurrentTurn = new GUIImage(new Vector2((int)Position().X, (int)Position().Y), new Rectangle(288, 96, 32, 32), (int)_fCharacterHeight, (int)_fCharacterHeight, @"Textures\Dialog");

            _arNames = new GUIText[4];
            _arMana = new GUIText[4];
            _arHp = new GUIText[4];

            List<CombatAdventurer> party = PlayerManager.GetParty();
            for (int i = 0; i < party.Count; i++)
            {
                _arNames[i] = new GUIText(party[i].Name, true, @"Fonts\MenuFont");

                string strHp = string.Format("{0}/{1}", party[i].CurrentHP, party[i].MaxHP);
                _arHp[i] = new GUIText(strHp, true, @"Fonts\MenuFont");

                string strMp = string.Format("{0}/{1}", party[i].CurrentMP, party[i].MaxMP);
                _arMana[i] = new GUIText(strMp, true, @"Fonts\MenuFont");

                if (i == 0) {
                    _arNames[i].AnchorToInnerSide(this, SideEnum.TopLeft);
                    _arMana[i].CenterOnWindow(this);
                    _arMana[i].AnchorToInnerSide(this, SideEnum.Top);
                    _arHp[i].AnchorToInnerSide(this, SideEnum.TopRight);
                }
                else
                {
                    _arNames[i].AnchorAndAlignToObject(_arNames[i - 1], SideEnum.Bottom, SideEnum.Left);
                    _arMana[i].AnchorAndAlignToObject(_arMana[i-1], SideEnum.Bottom, SideEnum.Right);
                    _arHp[i].AnchorAndAlignToObject(_arHp[i - 1], SideEnum.Bottom, SideEnum.Right);
                    AddControl(_arNames[i]);
                    AddControl(_arMana[i]);
                    AddControl(_arHp[i]);
                }
            }
        }

        public override void Update(GameTime gametime)
        {
            List<CombatAdventurer> party = PlayerManager.GetParty();
            for (int i = 0; i < party.Count; i++)
            {
                CombatAdventurer p = party[i];
                Color c = Color.White;
                if (p.Poisoned()) { c = Color.Violet; }
                c = (CombatManager.ActiveCharacter == p) ? Color.Green : c;

                _arNames[i].SetColor(c);
                _arMana[i].SetColor(c);
                _arHp[i].SetColor(c);

                string strMp = string.Format("{0}/{1}", party[i].CurrentMP, party[i].MaxMP);
                _arMana[i].SetText(strMp, true);

                string strHp = string.Format("{0}/{1}", party[i].CurrentHP, party[i].MaxHP);
                _arHp[i].SetText(strHp, true);
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
        string _sMenuFont = @"Fonts\MenuFont";

        public CmbtMenuWindow(int startX, int width)
        {
            _iOptionsOffsetY = 0;
            _vecMenuSize = GameContentManager.GetFont(_sMenuFont).MeasureString("XXXXXXXX");
            _iCharHeight = (int)_vecMenuSize.Y;

            Width = width;
            Height = (int)(_vecMenuSize.Y * _iMaxMenuActions) + 2 * _winData.Edge;
            _winData = GreyWin;

            Position(new Vector2(startX, RiverHollow.ScreenHeight - GreyWin.Edge - (_vecMenuSize.Y * _iMaxMenuActions) - RiverHollow.ScreenHeight / 100));

            _giSelection = new GUIImage(Vector2.Zero, new Rectangle(288, 96, 32, 32), _iCharHeight, _iCharHeight, @"Textures\Dialog");
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
                    _diOptions.Add(key++, new SelectionData(a.Name, "", _sMenuFont));
                }
                _giSelection.AnchorToInnerSide(this, SideEnum.TopLeft);
                AssignToColumn();
                Resize();
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
            DrawWindow(spriteBatch);
            if (CombatManager.ActiveCharacter != null && CombatManager.ActiveCharacter.IsCombatAdventurer())
            {
                _giSelection.Draw(spriteBatch);
                int index = Math.Max(0, _iKeySelection - _iMaxMenuActions);
                for (int i = 0; i < _diOptions.Count; i++)
                {
                    if (i >= index)
                    {
                        GUIText text = _diOptions[i].GText;

                        Color c = (_chosenAction != null && text.Text.Equals(_chosenAction.Name)) ? Color.Green : Color.White;

                        if (text.Text.Equals("Cast Spell") && CombatManager.ActiveCharacter.Silenced())
                        {
                            c = Color.Gray;
                        }

                        text.Draw(spriteBatch);
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

        GUIText[] _arTextAction;
        GUIText[] _arTextNum;

        public CmbtUseMenuWindow(int startX, int width)
        {
            _iOptionsOffsetY = 0;
            _fFont = GameContentManager.GetFont(@"Fonts\MenuFont");
            _vecMenuSize = _fFont.MeasureString("XXXXXXXX");
            _iCharHeight = (int)_vecMenuSize.Y;

            _winData = GreyWin;
            Width = width;
            Height = (int)(_vecMenuSize.Y * (_iMaxMenuActions / 2)) + (2*_winData.Edge);   //Two columns

            Position(new Vector2(startX, RiverHollow.ScreenHeight - GreyWin.Edge - (Height) - RiverHollow.ScreenHeight / 100));

            _giSelection = new GUIImage(InnerTopLeft(), new Rectangle(288, 96, 32, 32), _iCharHeight, _iCharHeight, @"Textures\Dialog");

            _selectWidth = _giSelection.Width;
            _textColOne = (int)Position().X + _selectWidth;
            _textColTwo = (int)Position().X + (Width / 2) + _selectWidth;
            _arTextAction = new GUIText[_iMaxMenuActions];
            _arTextNum = new GUIText[_iMaxMenuActions];

            for(int i=0; i < _iMaxMenuActions; i++)
            {
                _arTextAction[i] = new GUIText(string.Empty, true, @"Fonts\MenuFont");
                _arTextNum[i] = new GUIText(string.Empty, true, @"Fonts\MenuFont");
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (InputManager.CheckPressedKey(Keys.W) || InputManager.CheckPressedKey(Keys.Up))
            {
                if (_iKeySelection - 2 >= 0)
                {
                    _iKeySelection -= 2;
                }
            }
            else if (InputManager.CheckPressedKey(Keys.S) || InputManager.CheckPressedKey(Keys.Down))
            {
                if (_iKeySelection + 2 < _diOptions.Count)
                {
                    _iKeySelection += 2;
                }
            }
            else if (InputManager.CheckPressedKey(Keys.D) || InputManager.CheckPressedKey(Keys.Right))
            {
                int test = _iKeySelection + 1;
                if (test < _diOptions.Count)
                {
                    _iKeySelection += 1;
                }
            }
            else if (InputManager.CheckPressedKey(Keys.A) || InputManager.CheckPressedKey(Keys.Left))
            {
                int test = _iKeySelection - 1;
                if (test >= 0)
                {
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
                        _giSelection.AnchorAndAlignToObject(_diOptions[_iKeySelection - 2].GText, SideEnum.Left, SideEnum.Bottom);
                        _iKeySelection -= 2;
                    }
                    else if (_iKeySelection + 2 < _diOptions.Count && GraphicCursor.Position.Y > _giSelection.Position().Y + _giSelection.Height)
                    {
                        _giSelection.AnchorAndAlignToObject(_diOptions[_iKeySelection + 2].GText, SideEnum.Left, SideEnum.Bottom);
                        _iKeySelection += 2;
                    }
                    else if (_iKeySelection + 1 < _diOptions.Count && GraphicCursor.Position.Y >= _giSelection.Position().Y && GraphicCursor.Position.Y <= _giSelection.Position().Y + _giSelection.Height && GraphicCursor.Position.X > _textColTwo)
                    {
                        if (_iKeySelection % 2 == 0)
                        {
                            _iKeySelection++;
                        }
                    }
                    else if (_iKeySelection - 1 >= 0 && GraphicCursor.Position.Y >= _giSelection.Position().Y && GraphicCursor.Position.Y <= _giSelection.Position().Y + _giSelection.Height && GraphicCursor.Position.X < _textColTwo)
                    {
                        if (_iKeySelection % 2 != 0)
                        {
                            _iKeySelection--;
                        }
                    }
                }
            }
            _giSelection.AnchorAndAlignToObject(_arTextAction[_iKeySelection], SideEnum.Left, SideEnum.Bottom);

            if (InputManager.CheckPressedKey(Keys.Enter))
            {
                SelectAction();
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
                    _diOptions.Add(key++, new SelectionData(s.Name + ":" + s.MPCost, "", @"Fonts\MenuFont"));
                }
                AssignData();
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
                    _diOptions.Add(key++, new SelectionData(i.Name + ":" + i.Number, "", @"Fonts\MenuFont"));
                }
                AssignData();
            }
        }

        public void AssignData()
        {
            for (int i = 0; i < _diOptions.Count && i < _iMaxMenuActions; i++)
            {
                //AddControl
                string[] split = _diOptions[i].Text.Split(':');
                GUIText gText = _arTextAction[i];
                GUIText gNum = _arTextNum[i];

                gText.SetText(split[0]);
                gNum.SetText(split[1]);
                if (i == 0) {
                    _giSelection.AnchorToInnerSide(this, SideEnum.TopLeft);
                    gText.AnchorAndAlignToObject(_giSelection, SideEnum.Right, SideEnum.Bottom);
                    gNum.AnchorAndAlignToObject(gText, SideEnum.Right, SideEnum.Bottom);
                    gNum.AffixToCenter(this, SideEnum.CenterX, false);
                }
                else if (i == 1)
                {
                    _giSelection.AffixToCenter(this, SideEnum.CenterX, true);
                    gText.AnchorAndAlignToObject(_giSelection, SideEnum.Right, SideEnum.Bottom);
                    gNum.AnchorAndAlignToObject(gText, SideEnum.Right, SideEnum.Bottom);
                    gNum.AnchorToInnerSide(this, SideEnum.TopRight);
                }
                else if (i % 2 == 0) {
                    gText.AnchorAndAlignToObject(_arTextAction[i - 2], SideEnum.Bottom, SideEnum.Left);
                    gNum.AnchorAndAlignToObject(_arTextNum[i-2], SideEnum.Bottom, SideEnum.Right);
                }
                else {
                    gText.AnchorAndAlignToObject(_arTextAction[i - 1], SideEnum.Right, SideEnum.Bottom);
                    gNum.AnchorAndAlignToObject(_arTextNum[i - 1], SideEnum.Bottom, SideEnum.Right);
                }
                AddControl(gText);
                AddControl(gNum);
            }
            _giSelection.AnchorToInnerSide(this, SideEnum.TopLeft);
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
