using Microsoft.Xna.Framework;
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
        GUICmbtTile[,] _arrAllies;
        GUICmbtTile[,] _arrEnemies;
        GUITextWindow _gtwTextWindow;
        GUIStatDisplay _sdStamina;
        GUIText _gResults;

        List<GUIText> _liTurns;
        ActionSelectObject _gActionSelect;

        TurnOrderDisplay _gTurnOrder;

        public object UtilMapManager { get; private set; }

        public CombatScreen()
        {
            _liTurns = new List<GUIText>();

            _gResults = new GUIText();

            _sdStamina = new GUIStatDisplay(GUIStatDisplay.DisplayEnum.Energy);
            Controls.Add(_sdStamina);

            _gActionSelect = new ActionSelectObject();

            CombatManager.ConfigureAllies(ref _arrAllies);
            CombatManager.ConfigureEnemies(ref _arrEnemies);

            _gTurnOrder = new TurnOrderDisplay();
            _gTurnOrder.AnchorToScreen(SideEnum.Top);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            //If the current Phase is skill selection, allow the user to pick a skill for the currentCharacter
            switch (CombatManager.CurrentPhase)
            {
                case CombatManager.PhaseEnum.SelectSkill:
                    rv = _gActionSelect.ProcessLeftButtonClick(mouse);
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
                    GUIManager.SetScreen(new TextScreen(ActorManager.DiNPC[7], ActorManager.DiNPC[7].GetDialogEntry("Healed")));
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
            CombatManager.ClearSelectedTile();
            CombatManager.SelectedAction = null;
            _gActionSelect.CancelAction();

            if(CombatManager.CurrentPhase == CombatManager.PhaseEnum.ChooseTarget) { CombatManager.CurrentPhase = CombatManager.PhaseEnum.SelectSkill; }
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;
            rv = _sdStamina.ProcessHover(mouse);

            if (!rv) { rv = _gActionSelect.ProcessHover(mouse); }
            if (!rv) { rv = _gTurnOrder.ProcessHover(mouse); }
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
            _gTurnOrder.Update(gameTime);

            switch (CombatManager.CurrentPhase)
            {
                case CombatManager.PhaseEnum.EnemyTurn:
                    _gTurnOrder.CalculateTurnOrder();
                    CombatManager.SelectedAction.SetSkillTarget();
                    break;

                case CombatManager.PhaseEnum.NewTurn:
                    if (!CombatManager.ActiveCharacter.IsMonster())
                    {
                        _gActionSelect.SetCharacter(CombatManager.ActiveCharacter);
                        _gActionSelect.AnchorToScreen(SideEnum.Bottom);
                    }
                    _gTurnOrder.CalculateTurnOrder();
                    CombatManager.SelectedAction = null;
                    CombatManager.CurrentPhase = CombatManager.PhaseEnum.SelectSkill;
                    break;

                case CombatManager.PhaseEnum.SelectSkill:
                    break;

                case CombatManager.PhaseEnum.ChooseTarget:
                    CombatManager.HandleKeyboardTargetting();
                    break;

                case CombatManager.PhaseEnum.DisplayAttack:
                    _gActionSelect.SetCharacter(null);
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
                _dResultsTimer = 1.0f;
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
            _gActionSelect.Draw(spriteBatch);
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

            _gTurnOrder.Draw(spriteBatch);
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

                if (array != _arrEnemies) { array = _arrEnemies; }
                else { loop = false; }
            }
        }
    }

    public class GUICmbtTile : GUIObject
    {
        GUIStatDisplay _gHP;
        GUIStatDisplay _gMP;
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

            _gTile = new GUIImage(new Rectangle(128, 0, 32, 32), 32, 32, @"Textures\Dialog");
            _gTile.SetScale(CombatManager.CombatScale);
            _gTargetter = new GUIImage(new Rectangle(256, 96, 32, 32), 32, 32, @"Textures\Dialog");

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

                if (!(CombatManager.CurrentPhase == CombatManager.PhaseEnum.PerformAction && CombatManager.ActiveCharacter == _mapTile.Character)
                    && !(_mapTile.Character.IsMonster() && _mapTile.Character.IsCurrentAnimation(CActorAnimEnum.KO)))
                {
                    _gHP.Draw(spriteBatch);
                    if (_gMP != null) { _gMP.Draw(spriteBatch); }

                    foreach (GUIStatus stat in _liStatus)
                    {
                        if (_mapTile.Character.DiConditions[stat.Status])
                        {
                            stat.Draw(spriteBatch);
                        }
                    }
                }
            }

            if (_gEffect != null && _iDmgTimer < 40)
            {
                _gEffect.Draw(spriteBatch);
            }

            if (_mapTile.Selected) { _gTargetter.Draw(spriteBatch); }
        }

        public override void Update(GameTime gameTime)
        {
            if (Occupied())
            {
                if (_gSummon != null) { _gSummon.Update(gameTime); }
                _gSprite.Update(gameTime);
                _gHP.Update(gameTime);
                if (_gMP != null) { _gMP.Update(gameTime); }
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
                if (_gMP != null) { _gMP.AnchorAndAlignToObject(_gHP, SideEnum.Bottom, SideEnum.Left); }

                _gEffect = new GUIText();
                _gEffect.AnchorAndAlignToObject(_gSprite, SideEnum.Top, SideEnum.CenterX);

                for (int i = 0; i < _liStatus.Count; i++)
                {
                    GUIStatus temp = _liStatus[i];
                    if (i == 0) { temp.AnchorAndAlignToObject(_gMP == null ? _gHP : _gMP, SideEnum.Bottom, SideEnum.Left); }
                    else { temp.AnchorAndAlignToObject(_liStatus[i - 1], SideEnum.Right, SideEnum.Bottom); }
                }
            }
        }

        public void SyncGUIObjects(bool occupied)
        {
            if (occupied)
            {
                _gSprite = new GUISprite(_mapTile.Character.BodySprite);
                _gSprite.PlayAnimation(CActorAnimEnum.Idle);
                _gHP = new GUIStatDisplay(GUIStatDisplay.DisplayEnum.Health, _mapTile.Character, 100);
                if (_mapTile.Character.MaxMP > 0) { _gMP = new GUIStatDisplay(GUIStatDisplay.DisplayEnum.Mana, _mapTile.Character, 100); }
            }
            else
            {
                _gSprite = null;
                _gHP = null;
                _gMP = null;
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

        public void AssignEffect(int x, bool harms)
        {
            AssignEffect(x.ToString(), harms);
        }
        public void AssignEffect(string x, bool harms)
        {
            if (_mapTile.TargetPlayer)
            {
                _iDmgTimer = 0;
                _gEffect.SetText(x);
                _gEffect.SetColor(harms ? Color.Red : Color.LightGreen);
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
            if (target.Equals(TargetEnum.Enemy) && found == null)       //If it targets an enemy, we add it
            {
                _liStatus.Add(new GUIStatus(c));
            }
            else if(target.Equals(TargetEnum.Ally) && found != null)    //If it targets an ally, we remove it
            {
                _liStatus.Remove(found);
            }

            _liStatus.Sort((x, y) => x.Status.CompareTo(y.Status));
            for(int i = 0; i< _liStatus.Count; i++)
            {
                if(i == 0) { _liStatus[i].AnchorAndAlignToObject(_gMP == null ? _gHP : _gMP, SideEnum.Bottom, SideEnum.Left); }
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
            if (_gMP != null) { _gMP.ProcessHover(mouse); }

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

    public class ActionSelectObject : GUIObject
    {
        GUIText _gText;
        ActionBar _gActionBar;

        public ActionSelectObject()
        {
            _gActionBar = new ActionBar();
            _gText = new GUIText();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _gActionBar.Draw(spriteBatch);
            _gText.Draw(spriteBatch);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            return _gActionBar.ProcessLeftButtonClick(mouse);
        }

        public bool ProcessHover(Point mouse)
        {
            bool rv = false;

            if (CombatManager.CurrentPhase != CombatManager.PhaseEnum.ChooseTarget && _gActionBar.ProcessHover(mouse))
            {
                rv = true;
                SyncText();
            }

            return rv;
        }

        public void SetCharacter(CombatActor activeCharacter)
        {
            _gActionBar.SetCharacter(activeCharacter);
            SyncText();

            Width = _gActionBar.Width;
            Height = _gActionBar.Height + _gText.Height;
        }

        public override void Position(Vector2 value)
        {
            base.Position(value);
            _gActionBar.Position(value);
            _gText.AnchorAndAlignToObject(_gActionBar, SideEnum.Bottom, SideEnum.CenterX);
        }

        private void SyncText()
        {
            MenuAction selectedAction = _gActionBar.SelectedAction;
            Item selectedItem = _gActionBar.SelectedItem;

            string actionName = string.Empty;

            if(selectedAction != null)
            {
                actionName = selectedAction.Name;
                if (!selectedAction.IsMenu() && ((CombatAction)selectedAction).MPCost > 0)
                {
                    actionName = actionName + "  " + ((CombatAction)selectedAction).MPCost.ToString() + " MP";
                }
            }
            else if(selectedItem != null)
            {
                actionName = selectedItem.Name + "  x" + selectedItem.Number;
            }

            _gText.SetText(actionName);
            _gText.AnchorAndAlignToObject(_gActionBar, SideEnum.Bottom, SideEnum.CenterX);
        }

        public void CancelAction()
        {
            _gActionBar.CancelAction();
            SyncText();
        }

        public class ActionBar : GUIObject
        {
            ActionMenu _actionMenu;
            CombatActor _actor;
            ActionButton _gSelectedAction;
            ActionButton _gSelectedMenu;
            List<ActionButton> _liActionButtons;

            public MenuAction SelectedAction => _gSelectedAction != null ? _gSelectedAction.Action : null;
            public Item SelectedItem => (_actionMenu != null && _actionMenu.SelectedAction.Item != null) ? _actionMenu.SelectedAction.Item : null;

            public ActionBar()
            {
                _liActionButtons = new List<ActionButton>();
            }

            public override void Draw(SpriteBatch spriteBatch)
            {
                foreach (ActionButton ab in _liActionButtons)
                {
                    if (ab != _gSelectedAction && ab != _gSelectedMenu)
                    {
                        ab.Draw(spriteBatch);
                    }
                }

                if (_gSelectedMenu != null) { _gSelectedMenu.Draw(spriteBatch); }
                if (_gSelectedAction != null) { _gSelectedAction.Draw(spriteBatch); }

                if (_actionMenu != null) { _actionMenu.Draw(spriteBatch); }
            }

            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = false;
                if (_actionMenu != null)
                {
                    if (_actionMenu.ProcessLeftButtonClick(mouse))
                    {
                        rv = true;
                        if (_actionMenu.ShowSpells())
                        {
                            MenuAction a = _actionMenu.SelectedAction.Action;
                            if (CombatManager.ActiveCharacter.CanCast(((CombatAction)a).MPCost))
                            {
                                CombatManager.ProcessActionChoice((CombatAction)a);
                            }
                        }
                        else if (_actionMenu.ShowItems())
                        {
                            CombatManager.ProcessItemChoice((CombatItem)_actionMenu.SelectedAction.Item);
                        }
                    }
                }

                for (int i = 0; i < _liActionButtons.Count; i++)
                {
                    ActionButton ab = _liActionButtons[i];
                    if (ab.ProcessLeftButtonClick(mouse))
                    {
                        rv = true;
                        MenuAction a = ab.Action;
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
                                _gSelectedMenu = ab;
                                _actionMenu = new ActionMenu(CombatManager.ActiveCharacter.SpellList);
                                _actionMenu.AnchorAndAlignToObject(this, SideEnum.Top, SideEnum.CenterX, 10);
                            }
                            else if (a.IsMenu() && a.IsUseItem() && InventoryManager.GetPlayerCombatItems().Count > 0)
                            {
                                _gSelectedMenu = ab;
                                _actionMenu = new ActionMenu(InventoryManager.GetPlayerCombatItems());
                                _actionMenu.AnchorAndAlignToObject(this, SideEnum.Top, SideEnum.CenterX, 10);
                            }
                        }

                        break;
                    }
                }

                return rv;
            }

            public bool ProcessHover(Point mouse)
            {
                bool rv = false;

                if (_actionMenu != null)
                {
                    if (_actionMenu.ProcessHover(mouse))
                    {
                        rv = true;
                        _gSelectedAction = _actionMenu.SelectedAction;
                    }
                }
                else
                {
                    for (int i = 0; i < _liActionButtons.Count; i++)
                    {
                        ActionButton ab = _liActionButtons[i];
                        if (ab.Contains(mouse))
                        {
                            rv = true;
                            if (_gSelectedAction != null) { _gSelectedAction.Unselect(); }
                            _gSelectedAction = ab;
                            _gSelectedAction.Select();
                            break;
                        }
                    }
                }

                return rv;
            }

            public void CancelAction()
            {
                if (_actionMenu != null)
                {
                    if (CombatManager.CurrentPhase == CombatManager.PhaseEnum.ChooseTarget)
                    {
                        _gSelectedAction = (_gSelectedMenu != null) ? _gSelectedMenu : _liActionButtons[0];
                    }
                    else
                    {
                        _actionMenu = null;
                        _gSelectedAction = (_gSelectedMenu != null) ? _gSelectedMenu : _liActionButtons[0];
                    }

                    ProcessHover(GraphicCursor.Position.ToPoint());
                }
            }

            public void SetCharacter(CombatActor activeCharacter)
            {
                _actionMenu = null;
                _gSelectedMenu = null;
                _gSelectedAction = null;

                _actor = activeCharacter;
                _liActionButtons.Clear();

                if (_actor != null)
                {
                    foreach (MenuAction ca in _actor.AbilityList)
                    {
                        ActionButton ab = new ActionButton(ca);
                        _liActionButtons.Add(ab);

                        if (ab.Action.IsMenu() && ab.Action.IsCastSpell() && CombatManager.ActiveCharacter.Silenced())
                        {
                            ab.Enable(false);
                        }
                        if (ab.Action.IsMenu() && ab.Action.IsUseItem() && InventoryManager.GetPlayerCombatItems().Count == 0)
                        {
                            ab.Enable(false);
                        }
                    }

                    _gSelectedAction = _liActionButtons[0];

                    Width = _liActionButtons.Count * _liActionButtons[0].Width;
                    Height = _liActionButtons[0].Height;
                }
            }

            public override void Position(Vector2 value)
            {
                base.Position(value);
                _liActionButtons[0].Position(value);
                for (int i = 1; i < _liActionButtons.Count; i++)
                {
                    _liActionButtons[i].AnchorAndAlignToObject(_liActionButtons[i - 1], SideEnum.Right, SideEnum.CenterY);
                }

                if (_actionMenu != null) { _actionMenu.AnchorAndAlignToObject(this, SideEnum.Top, SideEnum.CenterX, 10); }
            }

            private class ActionMenu : GUIObject
            {
                public enum DisplayEnum { Spells, Items };
                private DisplayEnum _display;

                List<ActionButton> _liActions;

                ActionButton _gSelectedAction;
                public ActionButton SelectedAction => _gSelectedAction;

                public ActionMenu(List<CombatAction> spellList)
                {
                    _display = DisplayEnum.Spells;
                    _liActions = new List<ActionButton>();

                    for (int i = 0; i < spellList.Count; i++)
                    {
                        _liActions.Add(new ActionButton(spellList[i]));
                        if (spellList[i].MPCost > CombatManager.ActiveCharacter.CurrentMP)
                        {
                            _liActions[i].Enable(false);
                        }
                    }

                    Width = Math.Min(_liActions.Count, 5) * _liActions[0].Width;
                    int numRows = 0;
                    int temp = _liActions.Count;
                    do
                    {
                        numRows++;
                        temp -= 5;
                    } while (temp > 0);
                    Height = numRows * _liActions[0].Height;

                    Position(Position());
                }

                public ActionMenu(List<CombatItem> itemList)
                {
                    _display = DisplayEnum.Items;
                    _liActions = new List<ActionButton>();

                    for (int i = 0; i < itemList.Count; i++)
                    {
                        _liActions.Add(new ActionButton(itemList[i]));
                    }

                    Width = Math.Min(_liActions.Count, 5) * _liActions[0].Width;
                    int numRows = 0;
                    int temp = _liActions.Count;
                    do
                    {
                        numRows++;
                        temp -= 5;
                    } while (temp > 0);
                    Height = numRows * _liActions[0].Height;

                    Position(Position());
                }

                public override void Draw(SpriteBatch spriteBatch)
                {
                    if (_liActions != null)
                    {
                        foreach (ActionButton ab in _liActions)
                        {
                            if (ab != SelectedAction)
                            {
                                ab.Draw(spriteBatch);
                            }
                        }

                        if (SelectedAction != null) { SelectedAction.Draw(spriteBatch); }
                    }
                }

                public override bool ProcessLeftButtonClick(Point mouse)
                {
                    bool rv = false;
                    if (_liActions != null)
                    {
                        for (int i = 0; i < _liActions.Count; i++)
                        {
                            rv = _liActions[i].Contains(mouse);
                            if (rv) { break; }
                        }
                    }

                    return rv;
                }

                public bool ProcessHover(Point mouse)
                {
                    bool rv = false;

                    if (_liActions != null)
                    {
                        for (int i = 0; i < _liActions.Count; i++)
                        {
                            ActionButton ab = _liActions[i];
                            if (ab.Contains(mouse))
                            {
                                rv = true;
                                if (_gSelectedAction != null) { _gSelectedAction.Unselect(); }
                                _gSelectedAction = ab;
                                _gSelectedAction.Select();
                                break;
                            }
                        }
                    }

                    return rv;
                }

                public override void Position(Vector2 value)
                {
                    base.Position(value);
                    if (_liActions != null)
                    {
                        _liActions[0].AnchorToInnerSide(this, SideEnum.BottomLeft);
                        for (int i = 1; i < _liActions.Count; i++)
                        {
                            if (i % 5 == 0)
                            {
                                _liActions[i].AnchorAndAlignToObject(_liActions[i - 5], SideEnum.Top, SideEnum.Left);
                            }
                            else
                            {
                                _liActions[i].AnchorAndAlignToObject(_liActions[i - 1], SideEnum.Right, SideEnum.Bottom);
                            }
                        }
                    }
                }

                public bool ShowItems() { return _display == DisplayEnum.Items; }
                public bool ShowSpells() { return _display == DisplayEnum.Spells; }
            }

            private class ActionButton : GUIImage
            {
                MenuAction _action;
                public MenuAction Action => _action;

                Item _item;
                public Item Item => _item;

                GUIImage _gItem;

                public ActionButton(MenuAction action) : base(new Rectangle((int)action.IconGrid.X * TileSize, (int)action.IconGrid.Y * TileSize, TileSize, TileSize), TileSize, TileSize, @"Textures\texCmbtActions")
                {
                    _action = action;
                    SetScale(CombatManager.CombatScale);
                }

                public ActionButton(Item i) : base(new Rectangle(288, 32, 32, 32), 16, 16, @"Textures\Dialog")
                {
                    _item = i;
                    _gItem = new GUIImage(_item.SourceRectangle, Width, Height, _item.Texture);
                    SetScale(CombatManager.CombatScale);
                }

                public override void Draw(SpriteBatch spriteBatch)
                {
                    spriteBatch.Draw(_texture, _drawRect, _sourceRect, _cEnabled * Alpha);
                    if (_item != null) { _gItem.Draw(spriteBatch); }
                }

                public string GetName()
                {
                    string rv = string.Empty;

                    if (_action != null) { rv = _action.Name; }
                    else if (_item != null) { rv = _item.Name; }

                    return rv;
                }

                public void Select()
                {
                    int firstWidth = Width;
                    int firstHeight = Height;
                    SetScale(CombatManager.CombatScale + 1);

                    int diffWidth = Width - firstWidth;
                    int diffHeight = Height - firstHeight;

                    MoveBy(new Vector2(-diffWidth / 2, -diffHeight / 2));
                }
                public void Unselect()
                {
                    int firstWidth = Width;
                    int firstHeight = Height;
                    SetScale(CombatManager.CombatScale);

                    int diffWidth = firstWidth - Width;
                    int diffHeight = firstHeight - Height;

                    MoveBy(new Vector2(diffWidth / 2, diffHeight / 2));
                }

                public override void Position(Vector2 value)
                {
                    base.Position(value);
                    if (_gItem != null) { _gItem.Position(value); }
                }

                public override void SetScale(int x)
                {
                    base.SetScale(x);
                    if (_gItem != null) { _gItem.SetScale(x); }
                }
            }
        }
    }
    
    public class TurnOrderDisplay : GUIObject
    {
        const int MAX_SHOWN = 10;
        int _iCurrUpdate = MAX_SHOWN;
        double _dTimer = 0;
        bool _bUpdate = false;
        bool _bTriggered = false;
        bool _bSyncing = false;

        GUIImage[] _arrBarDisplay;
        TurnDisplay[] _arrTurnDisplay;
        List<CombatActor> _liNewTurnOrder;
        GUIWindow _gWindow;

        public TurnOrderDisplay()
        {
            _arrTurnDisplay = new TurnDisplay[MAX_SHOWN];
            _arrBarDisplay = new GUIImage[MAX_SHOWN];

            _liNewTurnOrder = CombatManager.CalculateTurnOrder(MAX_SHOWN);

            for (int i = 0; i < MAX_SHOWN; i++)
            {
                _arrTurnDisplay[i] = new TurnDisplay(_liNewTurnOrder[i], _arrBarDisplay);
                _arrBarDisplay[i] = new GUIImage(new Rectangle(48, 58, 10, 2), 10, 2, @"Textures\Dialog");
                _arrBarDisplay[i].SetScale(CombatManager.CombatScale);
            }

            Width = MAX_SHOWN * _arrTurnDisplay[0].Width;
            Height = (2 * _arrTurnDisplay[0].Height) + _arrBarDisplay[0].Height;

            Position(Position());
        }
        
        public override void Update(GameTime gameTime)
        {
            if (_bUpdate)
            {
                _dTimer -= gameTime.ElapsedGameTime.TotalSeconds;

                if (_dTimer <= 0)
                {
                    _arrTurnDisplay[_iCurrUpdate].Update(gameTime);
                    if(!_bSyncing && _iCurrUpdate == MAX_SHOWN -1 && _arrTurnDisplay[_iCurrUpdate].Finished && _arrTurnDisplay[_iCurrUpdate].Action == TurnDisplay.ActionEnum.Insert)
                    {
                        _bUpdate = false;
                    }
                }

                if (_dTimer <= 0) { _dTimer = 0.03; }

                if (_bUpdate && (_arrTurnDisplay[_iCurrUpdate].Finished))
                {
                    _iCurrUpdate++;

                    //After incrememnting the count, which will bring us one to the left, we set
                    //The next box, ie: the box we just left, to equal the current, leftmost, box.
                    //So, 9 -> 8, 8 -> 7, 7 -> 6, 6 -> 5, 5 -> 4, 4 -> 3, 3 -> 2, 2 -> 1, 1 -> 0
                    if (!_bSyncing && _iCurrUpdate < MAX_SHOWN && _arrTurnDisplay[_iCurrUpdate - 1].Action != TurnDisplay.ActionEnum.Insert)
                    {
                        _arrTurnDisplay[_iCurrUpdate - 1] = _arrTurnDisplay[_iCurrUpdate];
                        _arrTurnDisplay[_iCurrUpdate].SetIndex(_iCurrUpdate - 1);
                    }
                    //If we're Syncing, we want to re-insert a node at the position we just popped
                    else if (_bSyncing && _arrTurnDisplay[_iCurrUpdate - 1].Action == TurnDisplay.ActionEnum.Pop)
                    {
                        _iCurrUpdate--;
                        _arrTurnDisplay[_iCurrUpdate].SetActor(_liNewTurnOrder[_iCurrUpdate]);
                        _arrTurnDisplay[_iCurrUpdate].Insert(_iCurrUpdate);
                    }
                    //When we get to the last node, we need to do some special actions
                    else if (_iCurrUpdate == MAX_SHOWN)
                    {
                        //If we're syncing, STAHP
                        if (_bSyncing)
                        {
                            _bSyncing = false;
                            _bUpdate = false;
                        }
                        else
                        {
                            int mod = --_iCurrUpdate;   //We need to act on the new, 9th box so we need to bump it back one.

                            _arrTurnDisplay[mod] = new TurnDisplay(_liNewTurnOrder[mod], _arrBarDisplay);
                            _arrTurnDisplay[mod].Insert(mod);
                        }
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            foreach (TurnDisplay displ in _arrTurnDisplay) { displ.Draw(spriteBatch); }
            foreach (GUIImage bar in _arrBarDisplay) { bar.Draw(spriteBatch); }

            if(_gWindow != null) { _gWindow.Draw(spriteBatch); }
        }

        public bool ProcessHover(Point mouse)
        {
            bool rv = false;
            CombatActor a = null;
            foreach (TurnDisplay t in _arrTurnDisplay)
            {
                if (t.Contains(mouse))
                {
                    rv = true;
                    a = t.Actor;
                    break;
                }
            }

            if (rv)
            {
                _gWindow = new GUIWindow(GUIWindow.RedWin, 10, 10);
                GUIText gText = new GUIText(a.Name);
                gText.AnchorToInnerSide(_gWindow, SideEnum.TopLeft);
                _gWindow.Resize();
                _gWindow.AnchorToScreen(SideEnum.BottomRight);
            }
            else
            {
                _gWindow = null;
            }

            return rv;
        }

        //Called to acquire the next turn order sequence and start off the new updates
        //Tell the currently active turn to fade out to start the updates
        public void CalculateTurnOrder()
        {
            if (_bTriggered) { 
                List<CombatActor> newList = CombatManager.CalculateTurnOrder(MAX_SHOWN);

                bool change = false;
                //Assume that only one entry can be wrong for insertions
                for(int i = 0; i< MAX_SHOWN -1; i++)
                {
                    if (_bUpdate || _liNewTurnOrder[i + 1] != newList[i])
                    {
                        change = true;
                        break;
                    }
                }

                _liNewTurnOrder = newList;
                

                if (change)
                {
                    for (int i = 0; i < MAX_SHOWN; i++)
                    {
                        _arrTurnDisplay[i].Pop();
                        _bSyncing = true;
                    }
                }
                else
                {
                    _arrTurnDisplay[0].Pop();
                }
            }
            else
            {
                _bTriggered = true;
            }

            _iCurrUpdate = 0;
            _bUpdate = true;
            _dTimer = 0.03;
        }

        public override void Position(Vector2 value)
        {
            base.Position(value);

            _arrTurnDisplay[MAX_SHOWN - 1].Position(value);     //Just used as a base point to start it off
            for (int i = MAX_SHOWN - 1; i >= 0; i--)
            {
                if (i == MAX_SHOWN - 1) { _arrBarDisplay[i].AnchorAndAlignToObject(_arrTurnDisplay[i], SideEnum.Bottom, SideEnum.CenterX); }
                else { _arrBarDisplay[i].AnchorAndAlignToObject(_arrBarDisplay[i + 1], SideEnum.Right, SideEnum.CenterY); }

                _arrTurnDisplay[i].Insert(i, 0.5f);
            }
        }

        public class TurnDisplay : GUIObject
        {
            public enum ActionEnum { Pop, Insert, Move };
            public ActionEnum Action;
            bool _bInParty;
            GUIText _gName;
            GUIImage _gImage;
            CombatActor _actor;
            public CombatActor Actor => _actor;

            bool _bFadeIn;
            public bool FadeIn => _bFadeIn;
            bool _bFadeOut;
            public bool Finished;

            float _fFadeSpeed;
            int _iIndex;
            Vector2 _vMoveTo = new Vector2(0, 0);

            GUIImage[] _arrBarDisplay;

            public TurnDisplay(CombatActor actor, GUIImage[] barDisplay)
            {
                _actor = actor;
                _bInParty = !actor.IsMonster();
                _gName = new GUIText(actor.Name.Substring(0, 1));
                _gImage = new GUIImage(new Rectangle(48, 48, 10, 10), 10, 10, @"Textures\Dialog");
                _gImage.SetScale(CombatManager.CombatScale);

                _arrBarDisplay = barDisplay;
                Width = _gImage.Width;
                Height = _gImage.Height;
            }

            public override void Update(GameTime gameTime)
            {
                if (_bFadeOut)
                {
                    UpdateFadeOut(gameTime);
                }
                else if (_bFadeIn)
                {
                    UpdateFadeIn(gameTime);
                }
                else if (_vMoveTo != Vector2.Zero)
                {
                    UpdateMove(gameTime);
                }
            }
            private void UpdateFadeOut(GameTime gameTime)
            {
                if (Alpha > 0)
                {
                    SetAlpha(Alpha - _fFadeSpeed);
                }
                else
                {
                    _bFadeOut = false;
                    Finished = true;
                }
            }
            private void UpdateFadeIn(GameTime gameTime)
            {
                if (Alpha < 1)
                {
                    SetAlpha(Alpha + _fFadeSpeed);
                }
                else
                {
                    _bFadeIn = false;
                    Finished = true;
                }
            }
            private void UpdateMove(GameTime gameTime)
            {
                if (Position() != _vMoveTo)
                {
                    Vector2 moveDir = new Vector2(Width / 2, 0);
                    MoveBy(moveDir);
                }
                else
                {
                    _vMoveTo = Vector2.Zero;
                    Finished = true;
                }
            }

            public override void Draw(SpriteBatch spriteBatch)
            {
                if (_actor != null)
                {
                    _gImage.Draw(spriteBatch);
                    _gName.Draw(spriteBatch);
                }
            }

            public override void Position(Vector2 value)
            {
                base.Position(value);
                _gImage.Position(value);
                _gName.CenterOnObject(_gImage);
            }

            public bool IsInParty()
            {
                return _bInParty;
            }

            public void SetActor(CombatActor c)
            {
                _actor = c;
                _bInParty = !_actor.IsMonster();
                _gName.SetText(c != null ? _actor.Name.Substring(0, 1) : string.Empty);
                _gName.CenterOnObject(_gImage);
            }
            public void SetIndex(int val)
            {
                _iIndex = val;

                _vMoveTo = GetAlignToObject(_arrBarDisplay[_iIndex], SideEnum.CenterX);

                Finished = false;
                Action = ActionEnum.Move;
            }

            public bool Occupied() { return _actor != null; }

            public void SetAlpha(float alpha) {
                Alpha = alpha;
                _gImage.Alpha = alpha;
                _gName.Alpha = alpha;
            }

            public string GetName()
            {
                return _actor != null ? _actor.Name : string.Empty;
            }

            public void Pop()
            {
                Finished = false;
                _bFadeOut = true;
                Action = ActionEnum.Pop;
            }
            public void Insert(int index, float speed = 0.3f)
            {
                if (_actor != null)
                {
                    Action = ActionEnum.Insert;
                    _iIndex = index;
                    _bFadeIn = true;
                    _fFadeSpeed = speed;
                    Finished = false;
                    SetAlpha(0);
                    AnchorAndAlignToObject(_arrBarDisplay[_iIndex], IsInParty() ? SideEnum.Top : SideEnum.Bottom, SideEnum.CenterX);
                }
            }
        }
    }
}
