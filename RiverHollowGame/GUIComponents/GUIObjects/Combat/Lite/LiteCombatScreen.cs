using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Actors.CombatStuff;
using RiverHollow.Characters;
using RiverHollow.Characters.Lite;
using RiverHollow.CombatStuff;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.Combat.Lite;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.SpriteAnimations;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.GUIComponents.GUIObjects.GUIObject;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class LiteCombatScreen : GUIScreen
    {
        GUICombatTile[,] _arrAllies;
        GUICombatTile[,] _arrEnemies;
        GUITextWindow _gActionTextWindow;
        GUIStatDisplay _sdStamina;
        GUIImage _gActionEffect;

        GUIPostCombatDisplay _gPostScreen;

        ActionSelectObject _gActionSelect;

        GUILiteTurnOrderDisplay _gTurnOrder;

        public LiteCombatScreen()
        {
            _sdStamina = new GUIStatDisplay(PlayerManager.GetStamina, Color.Red);
            AddControl(_sdStamina);

            _gActionSelect = new ActionSelectObject();
            AddControl(_gActionSelect);

            ConfigureGUIMap();

            LiteCombatManager.AssignPositions(ref _arrAllies);

            _gTurnOrder = new GUILiteTurnOrderDisplay();
            _gTurnOrder.AnchorToScreen(SideEnum.Top);
            AddControl(_gTurnOrder);
        }

        #region Map Configuration
        /// <summary>
        /// Calls the two GUI Map configurations
        /// </summary>
        private void ConfigureGUIMap()
        {
            ConfigureAllies();
            ConfigureEnemies();
        }
        /// <summary>
        /// Sets up the GUICombatTiles for the players side of the map
        /// </summary>
        private void ConfigureAllies()
        {
            int cols = LiteCombatManager.MAX_COL / 2;
            _arrAllies = new GUICombatTile[LiteCombatManager.MAX_ROW, cols];
            for (int row = 0; row < LiteCombatManager.MAX_ROW; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    _arrAllies[row, col] = new GUICombatTile(LiteCombatManager.GetMapTile(row, col));
                    if (row == 0 && col == 0) { _arrAllies[row, col].AnchorToScreen(SideEnum.Left, 100); }
                    else if (col == 0) { _arrAllies[row, col].AnchorAndAlignToObject(_arrAllies[row - 1, col], SideEnum.Bottom, SideEnum.Left); }
                    else { _arrAllies[row, col].AnchorAndAlignToObject(_arrAllies[row, col - 1], SideEnum.Right, SideEnum.Bottom); }
                    AddControl(_arrAllies[row, col]);
                }
            }
        }
        /// <summary>
        /// Sets up the GUICombatTiles for the enemy's side of the map
        /// </summary>
        private void ConfigureEnemies()
        {
            int cols = LiteCombatManager.MAX_COL / 2;
            _arrEnemies = new GUICombatTile[LiteCombatManager.MAX_ROW, cols];
            for (int row = 0; row < LiteCombatManager.MAX_ROW; row++)
            {
                for (int col = cols - 1; col >= 0; col--)
                {
                    _arrEnemies[row, col] = new GUICombatTile(LiteCombatManager.GetMapTile(row, col + 4));
                    if (row == 0 && col == cols - 1) { _arrEnemies[row, col].AnchorToScreen(SideEnum.Right, 100); }
                    else if (col == cols - 1) { _arrEnemies[row, col].AnchorAndAlignToObject(_arrEnemies[row - 1, col], SideEnum.Bottom, SideEnum.Right); }
                    else { _arrEnemies[row, col].AnchorAndAlignToObject(_arrEnemies[row, col + 1], SideEnum.Left, SideEnum.Bottom); }
                    AddControl(_arrEnemies[row, col]);
                }
            }
        }
        #endregion

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            //If the current Phase is skill selection, allow the user to pick a skill for the currentCharacter
            switch (LiteCombatManager.CurrentPhase)
            {
                case LiteCombatManager.PhaseEnum.SelectSkill:
                    rv = _gActionSelect.ProcessLeftButtonClick(mouse);
                    break;

                case LiteCombatManager.PhaseEnum.ChooseTarget:
                    LiteCombatManager.SelectedAction.SetSkillTarget();
                    break;
                case LiteCombatManager.PhaseEnum.DisplayVictory:
                    rv = _gPostScreen.ProcessLeftButtonClick(mouse);
                    break;
                case LiteCombatManager.PhaseEnum.Defeat:
                    GUIManager.BeginFadeOut(true);
                    GoToHUDScreen();
                    MapManager.CurrentMap = MapManager.Maps["mapHospital"];
                    PlayerManager.CurrentMap = "mapHospital";
                    PlayerManager.World.Position = Util.SnapToGrid(MapManager.CurrentMap.DictionaryCharacterLayer["playerSpawn"]);

                    foreach (LitePartyMember c in PlayerManager.GetLiteParty())
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
            if (LiteCombatManager.CanCancel())
            {
                CancelAction();
            }
            return rv;
        }

        internal void CancelAction()
        {
            LiteCombatManager.ClearSelectedTile();
            LiteCombatManager.SelectedAction = null;
            _gActionSelect.CancelAction();

            if (LiteCombatManager.CurrentPhase == LiteCombatManager.PhaseEnum.ChooseTarget) { LiteCombatManager.CurrentPhase = LiteCombatManager.PhaseEnum.SelectSkill; }
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;
            rv = _sdStamina.ProcessHover(mouse);

            if (!rv) { rv = _gActionSelect.ProcessHover(mouse); }
            if (!rv) { rv = _gTurnOrder.ProcessHover(mouse); }
            if (LiteCombatManager.PhaseChooseTarget())
            {
                rv = HandleHoverTargeting();
            }
            else
            {
                bool loop = true;
                GUICombatTile[,] array = _arrAllies;
                while (loop)
                {
                    foreach (GUICombatTile t in array)
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

            if (_gPostScreen != null)
            {
                _gPostScreen.ProcessHover(mouse);
            }
            Exit:

            return rv;
        }

        internal bool HandleHoverTargeting()
        {
            bool rv = false;

            if (LiteCombatManager.SelectedAction.TargetsEnemy())
            {
                rv = HoverTargetHelper(_arrEnemies);
            }
            else if (LiteCombatManager.SelectedAction.TargetsAlly())
            {
                rv = HoverTargetHelper(_arrAllies);
            }

            return rv;
        }

        internal bool HoverTargetHelper(GUICombatTile[,] array)
        {
            bool rv = false;
            foreach (GUICombatTile p in array)
            {
                if (rv) { break; }
                rv = p.CheckForTarget(GUICursor.Position.ToPoint());
            }

            return rv;
        }

        //First, call the update for the CombatManager
        public override void Update(GameTime gameTime)
        {
            LiteCombatManager.Update(gameTime);
            base.Update(gameTime);

            switch (LiteCombatManager.CurrentPhase)
            {
                case LiteCombatManager.PhaseEnum.EnemyTurn:
                    _gTurnOrder.CalculateTurnOrder();
                    LiteCombatManager.SelectedAction.SetSkillTarget();
                    break;

                case LiteCombatManager.PhaseEnum.NewTurn:
                    if (!LiteCombatManager.ActiveCharacter.IsActorType(ActorEnum.Monster))
                    {
                        _gActionSelect.SetCharacter(LiteCombatManager.ActiveCharacter);
                        _gActionSelect.AnchorToScreen(SideEnum.Bottom);
                    }
                    _gTurnOrder.CalculateTurnOrder();
                    LiteCombatManager.SelectedAction = null;
                    LiteCombatManager.CurrentPhase = LiteCombatManager.PhaseEnum.SelectSkill;
                    break;

                case LiteCombatManager.PhaseEnum.ChooseTarget:
                    LiteCombatManager.HandleKeyboardTargetting();

                    //Cancel out of selections made if escape is hit
                    if (InputManager.CheckPressedKey(Keys.Escape))
                    {
                        CancelAction();
                    }
                    break;

                case LiteCombatManager.PhaseEnum.DisplayAttack:
                    _gActionSelect.SetCharacter(null);
                    LiteCombatManager.CurrentPhase = LiteCombatManager.PhaseEnum.PerformAction;
                    if (!string.IsNullOrEmpty(LiteCombatManager.Text))
                    {
                        //if (_gActionTextWindow == null)
                        //{
                        //    _gActionTextWindow = new GUITextWindow(LiteCombatManager.Text, 0.5);
                        //    _gActionTextWindow.CenterOnScreen();
                        //    AddControl(_gActionTextWindow);
                        //}
                        //else
                        //{
                        //    _gActionTextWindow.Update(gameTime);
                        //    if (_gActionTextWindow.Duration <= 0)
                        //    {
                        //        RemoveControl(_gActionTextWindow);
                        //        _gActionTextWindow = null;
                        //        LiteCombatManager.CurrentPhase = LiteCombatManager.PhaseEnum.PerformAction;
                        //    }
                        //}
                    }
                    break;

                case LiteCombatManager.PhaseEnum.PerformAction:
                    if (LiteCombatManager.SelectedAction != null)
                    {
                        LiteCombatManager.SelectedAction.PerformAction(gameTime);
                    }

                    break;
                case LiteCombatManager.PhaseEnum.DisplayVictory:
                    if (_gPostScreen == null)
                    {
                        InventoryManager.InitMobInventory(1, 5);
                        _gPostScreen = new GUIPostCombatDisplay(ClosePostCombatDisplay);
                        _gPostScreen.CenterOnScreen();
                    }

                    break;
                case LiteCombatManager.PhaseEnum.Defeat:
                    //GUITextWindow window = new GUITextWindow("Defeated");
                    //window.CenterOnScreen();
                    //AddControl(window);
                    break;
            }

            List<LiteSummon> summons = new List<LiteSummon>();
            foreach (LiteCombatActor act in LiteCombatManager.Party)
            {
                if (act.LinkedSummon != null)
                {
                    summons.Add(act.LinkedSummon);
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            //Characters need to be drawn after the tiles because they move and need to be drawn on top of them
            foreach (GUICombatTile tile in _arrAllies) { tile.DrawCharacters(spriteBatch); }
            foreach (GUICombatTile tile in _arrEnemies) { tile.DrawCharacters(spriteBatch); }

            if (LiteCombatManager.SelectedAction != null) { LiteCombatManager.SelectedAction.Draw(spriteBatch); }

            //Draw here instead of leaving it to the controls because the
            //characters will get drawnon top of it otherwise.
            if (_gPostScreen != null)
            {
                _gPostScreen.Draw(spriteBatch);
            }
        }

        private void ClosePostCombatDisplay()
        {
            _gPostScreen = null;
            LiteCombatManager.EndCombatVictory();
        }

        private class ActionSelectObject : GUIObject
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

            public override bool ProcessHover(Point mouse)
            {
                bool rv = false;

                if (LiteCombatManager.CurrentPhase != LiteCombatManager.PhaseEnum.ChooseTarget && _gActionBar.ProcessHover(mouse))
                {
                    rv = true;
                    SyncText();
                }

                return rv;
            }

            public void SetCharacter(LiteCombatActor activeCharacter)
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
                LiteMenuAction selectedAction = _gActionBar.SelectedAction;
                Item selectedItem = _gActionBar.SelectedItem;

                string actionName = string.Empty;

                if (selectedAction != null)
                {
                    actionName = selectedAction.Name;
                  //  if (!selectedAction.IsMenu() && ((LiteCombatAction)selectedAction).MPCost > 0)
                    {
                  //      actionName = actionName + "  " + ((LiteCombatAction)selectedAction).MPCost.ToString() + " MP";
                    }
                }
                else if (selectedItem != null)
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

            private class ActionBar : GUIObject
            {
                ActionMenu _actionMenu;
                LiteCombatActor _actor;
                ActionButton _gSelectedAction;
                ActionButton _gSelectedMenu;
                List<ActionButton> _liActionButtons;

                public LiteMenuAction SelectedAction => _gSelectedAction != null ? _gSelectedAction.Action : null;
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
                                LiteMenuAction a = _actionMenu.SelectedAction.Action;
                                if (LiteCombatManager.ActiveCharacter.CanCast(((LiteCombatAction)a).MPCost))
                                {
                                    LiteCombatManager.ProcessActionChoice((LiteCombatAction)a);
                                }
                            }
                            else if (_actionMenu.ShowItems())
                            {
                                LiteCombatManager.ProcessItemChoice((Consumable)_actionMenu.SelectedAction.Item);
                            }
                        }
                    }

                    for (int i = 0; i < _liActionButtons.Count; i++)
                    {
                        ActionButton ab = _liActionButtons[i];
                        if (ab.ProcessLeftButtonClick(mouse))
                        {
                            rv = true;
                            LiteMenuAction a = ab.Action;
                            if (a.Compare(ActionEnum.Action) || a.Compare(ActionEnum.Spell))
                            {
                                if (LiteCombatManager.ActiveCharacter.CanCast(((LiteCombatAction)a).MPCost))
                                {
                                    LiteCombatManager.ProcessActionChoice((LiteCombatAction)a);
                                }
                            }
                            else if (a.Compare(ActionEnum.Move))
                            {
                                LiteCombatManager.ProcessActionChoice((LiteCombatAction)a);
                            }
                            else if (a.Compare(ActionEnum.EndTurn))
                            {
                                LiteCombatManager.EndCombatEscape();
                            }
                            else
                            {
                                if (a.Compare(ActionEnum.MenuSpell) && !LiteCombatManager.ActiveCharacter.Silenced())
                                {
                                    if (LiteCombatManager.ActiveCharacter.SpecialActions.Count > 0)
                                    {
                                        _gSelectedMenu = ab;
                                        _actionMenu = new ActionMenu(LiteCombatManager.ActiveCharacter.SpecialActions);
                                        _actionMenu.AnchorAndAlignToObject(this, SideEnum.Top, SideEnum.CenterX, 10);
                                    }
                                }
                                else if (a.Compare(ActionEnum.MenuItem) && InventoryManager.GetPlayerCombatItems().Count > 0)
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

                public override bool ProcessHover(Point mouse)
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
                        if (LiteCombatManager.CurrentPhase == LiteCombatManager.PhaseEnum.ChooseTarget)
                        {
                            _gSelectedAction = (_gSelectedMenu != null) ? _gSelectedMenu : _liActionButtons[0];
                        }
                        else
                        {
                            _actionMenu = null;
                            _gSelectedAction = (_gSelectedMenu != null) ? _gSelectedMenu : _liActionButtons[0];
                        }

                        ProcessHover(GUICursor.Position.ToPoint());
                    }
                }

                public void SetCharacter(LiteCombatActor activeCharacter)
                {
                    _actionMenu = null;
                    _gSelectedMenu = null;
                    _gSelectedAction = null;

                    _actor = activeCharacter;
                    _liActionButtons.Clear();

                    if (_actor != null)
                    {
                        foreach (LiteMenuAction ca in _actor.AbilityList)
                        {
                            ActionButton ab = new ActionButton(ca);
                            _liActionButtons.Add(ab);

                            if (ab.Action.IsMenu() && ab.Action.Compare(ActionEnum.MenuSpell) && LiteCombatManager.ActiveCharacter.Silenced())
                            {
                                ab.Enable(false);
                            }
                            if (ab.Action.IsMenu() && ab.Action.Compare(ActionEnum.MenuItem) && InventoryManager.GetPlayerCombatItems().Count == 0)
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

                    public ActionMenu(List<LiteCombatAction> specialsList)
                    {
                        _display = DisplayEnum.Spells;
                        _liActions = new List<ActionButton>();

                        for (int i = 0; i < specialsList.Count; i++)
                        {
                            _liActions.Add(new ActionButton(specialsList[i]));
                            if (specialsList[i].MPCost > LiteCombatManager.ActiveCharacter.CurrentMP)
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

                    public ActionMenu(List<Consumable> itemList)
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

                    public override bool ProcessHover(Point mouse)
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
                    LiteMenuAction _action;
                    public LiteMenuAction Action => _action;

                    Item _item;
                    public Item Item => _item;

                    GUIImage _gItem;

                    public ActionButton(LiteMenuAction action) : base(new Rectangle((int)action.IconGrid.X * TILE_SIZE, (int)action.IconGrid.Y * TILE_SIZE, TILE_SIZE, TILE_SIZE), TILE_SIZE, TILE_SIZE, @"Textures\texCmbtActions")
                    {
                        _action = action;
                        SetScale(LiteCombatManager.CombatScale);
                    }

                    public ActionButton(Item i) : base(new Rectangle(288, 32, 32, 32), 16, 16, @"Textures\Dialog")
                    {
                        _item = i;
                        _gItem = new GUIImage(_item.SourceRectangle, Width, Height, _item.Texture);
                        SetScale(LiteCombatManager.CombatScale);
                    }

                    public override void Draw(SpriteBatch spriteBatch)
                    {
                        spriteBatch.Draw(_texture, _drawRect, _sourceRect, EnabledColor * Alpha());
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
                        SetScale(LiteCombatManager.CombatScale + 1);

                        int diffWidth = Width - firstWidth;
                        int diffHeight = Height - firstHeight;

                        MoveBy(new Vector2(-diffWidth / 2, -diffHeight / 2));
                    }
                    public void Unselect()
                    {
                        int firstWidth = Width;
                        int firstHeight = Height;
                        SetScale(LiteCombatManager.CombatScale);

                        int diffWidth = firstWidth - Width;
                        int diffHeight = firstHeight - Height;

                        MoveBy(new Vector2(diffWidth / 2, diffHeight / 2));
                    }

                    public override void Position(Vector2 value)
                    {
                        base.Position(value);
                        if (_gItem != null) { _gItem.Position(value); }
                    }

                    public override void SetScale(double x, bool anchorToPos = true)
                    {
                        base.SetScale(x);
                        if (_gItem != null) { _gItem.SetScale(x); }
                    }
                }
            }
        }

        private class GUIPostCombatDisplay : GUIObject
        {
            GUIButton _btnClose;
            GUIWindow _gWin;
            GUIStatDisplay _gXPToGive;
            GUIStatDisplay[] _arrCharXP;

            bool _bDisplayItems;

            public GUIPostCombatDisplay(BtnClickDelegate closeDelegate)
            {
                _bDisplayItems = false;
                _arrCharXP = new GUIStatDisplay[4];
                _gWin = new GUIWindow(GUIWindow.Window_1, GUIManager.MAIN_COMPONENT_WIDTH, GUIManager.MAIN_COMPONENT_HEIGHT);
                _gXPToGive = new GUIStatDisplay(LiteCombatManager.CurrentMob.GetXP, Color.Yellow);
                _gXPToGive.CenterOnObject(_gWin);
                _gXPToGive.AnchorToInnerSide(_gWin, SideEnum.Top);

                for (int i = 0; i < PlayerManager.GetLiteParty().Count; i++)
                {
                    LitePartyMember adv = PlayerManager.GetLiteParty()[i];
                    _arrCharXP[i] = new GUIStatDisplay(adv.GetXP, Color.Yellow);

                    if (i == 0) { _arrCharXP[i].AnchorToInnerSide(_gWin, SideEnum.BottomLeft); }
                    else { _arrCharXP[i].AnchorAndAlignToObject(_arrCharXP[i - 1], SideEnum.Right, SideEnum.Bottom); }

                    _gWin.AddControl(_arrCharXP[i]);
                }

                _btnClose = new GUIButton("Close", closeDelegate);

                Width = _gWin.Width;
                Height = _gWin.Height;
                AddControl(_gWin);
            }

            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = false;
                if (!_bDisplayItems)
                {
                    _bDisplayItems = true;
                    RemoveControl(_gWin);

                    //_gItemManager = new GUIInventoriesDisplay();
                    //_btnClose.AnchorAndAlignToObject(_gItemManager, SideEnum.Right, SideEnum.Bottom);
                    //AddControl(_gItemManager);
                    _btnClose.CenterOnScreen();
                    AddControl(_btnClose);
                }
                else
                {
                    //rv = _gItemManager.ProcessLeftButtonClick(mouse);
                    if (!rv)
                    {
                        rv = _btnClose.ProcessLeftButtonClick(mouse);
                    }
                }

                return rv;
            }

        }
    }
}
