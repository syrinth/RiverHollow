using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Characters;
using RiverHollow.Characters.Lite;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.Combat.Lite;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.SpriteAnimations;
using RiverHollow.Utilities;
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
        GUIImage _gActionEffect;

        GUIPostCombatDisplay _gPostScreen;

        GUIImage _gBackgroundImage;
        GUIButton _btnEscape;

        GUISprite _gActiveIndicator;
        TurnOrder _gTurnOrder;
        InfoPanel _gActiveCharacterInfo;
        InfoPanel _gHoverCharacterInfo;

        ActionPanel _gActionPanel;

        public LiteCombatScreen()
        {
            _gBackgroundImage = new GUIImage(new Rectangle(0, 0, 480, 270), DataManager.GUI_COMPONENTS + @"\Combat_Background_Forest");
            AddControl(_gBackgroundImage);

            _btnEscape = new GUIButton(new Rectangle(112, 24, 24, 26), DataManager.COMBAT_TEXTURE, CombatManager.EndCombatEscape);
            _btnEscape.ScaledMoveBy(3, 3);
            AddControl(_btnEscape);

            _gActionPanel = new ActionPanel();
            _gActionPanel.Show(CombatManager.Party.Contains(CombatManager.ActiveCharacter));

            if (CombatManager.Party.Contains(CombatManager.ActiveCharacter))
            {
                _gActionPanel.PopulateActions();
            }
            AddControl(_gActionPanel);

            ConfigureGUIMap();

            CombatManager.AssignPositions(ref _arrAllies);

            _gTurnOrder = new TurnOrder(CombatManager.CurrentRoundOrder);
            AddControl(_gTurnOrder);

            PlayerManager.PlayerCombatant.ApplyStatusEffect(DataManager.GetStatusEffectByIndex(0));

            //Create the Active Character Info Panel
            _gActiveCharacterInfo = new InfoPanel(CombatManager.ActiveCharacter);
            _gActiveCharacterInfo.AnchorToScreen(SideEnum.BottomLeft);
            _gActiveCharacterInfo.ScaledMoveBy(16, -15);
            AddControl(_gActiveCharacterInfo);

            //Create the Hover Info Panel
            _gHoverCharacterInfo = new InfoPanel(CombatManager.ActiveCharacter);
            _gHoverCharacterInfo.AnchorToScreen(SideEnum.BottomRight);
            _gHoverCharacterInfo.ScaledMoveBy(-16, -15);
            _gHoverCharacterInfo.Show(false);
            AddControl(_gHoverCharacterInfo);

            //Create the ActiveCharacter Indicator
            AnimatedSprite indicator = new AnimatedSprite(DataManager.COMBAT_TEXTURE);
            indicator.AddAnimation(AnimationEnum.PlayAnimation, 126, 112, 34, 34, 2, 0.25f);
            indicator.PlayAnimation(AnimationEnum.PlayAnimation);

            _gActiveIndicator = new GUISprite(indicator);
            _gActiveIndicator.SetScale(CurrentScale);
            _gActiveIndicator.CenterOnObject(CombatManager.ActiveCharacter.Tile.GUITile);
            _gActiveIndicator.Show(CombatManager.Party.Contains(CombatManager.ActiveCharacter));
            AddControl(_gActiveIndicator);
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
            int cols = CombatManager.MAX_COL / 2;
            _arrAllies = new GUICombatTile[CombatManager.MAX_ROW, cols];
            for (int row = 0; row < CombatManager.MAX_ROW; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    _arrAllies[row, col] = new GUICombatTile(CombatManager.GetMapTile(row, col));
                    if (row == 0 && col == 0) { _arrAllies[row, col].ScaledMoveBy(81, 97); }
                    else if (col == 0) {
                        _arrAllies[row, col].AnchorAndAlignToObject(_arrAllies[row - 1, col], SideEnum.Bottom, SideEnum.Left);
                        _arrAllies[row, col].ScaledMoveBy(-8, 1);
                    }
                    else { _arrAllies[row, col].AnchorAndAlignToObject(_arrAllies[row, col - 1], SideEnum.Right, SideEnum.Bottom, ScaleIt(2)); }
                    AddControl(_arrAllies[row, col]);
                }
            }
        }
        /// <summary>
        /// Sets up the GUICombatTiles for the enemy's side of the map
        /// </summary>
        private void ConfigureEnemies()
        {
            int cols = CombatManager.MAX_COL / 2;
            _arrEnemies = new GUICombatTile[CombatManager.MAX_ROW, cols];
            for (int row = 0; row < CombatManager.MAX_ROW; row++)
            {
                for (int col = cols - 1; col >= 0; col--)
                {
                    _arrEnemies[row, col] = new GUICombatTile(CombatManager.GetMapTile(row, col + 4));
                    if (row == 0 && col == cols - 1) { _arrEnemies[row, col].ScaledMoveBy(369, 97); }
                    else if (col == cols - 1) {
                        _arrEnemies[row, col].AnchorAndAlignToObject(_arrEnemies[row - 1, col], SideEnum.Bottom, SideEnum.Right);
                        _arrEnemies[row, col].ScaledMoveBy(8, 1);
                    }
                    else { _arrEnemies[row, col].AnchorAndAlignToObject(_arrEnemies[row, col + 1], SideEnum.Left, SideEnum.Bottom, ScaleIt(2)); }
                    AddControl(_arrEnemies[row, col]);
                }
            }
        }
        #endregion

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            _btnEscape.ProcessLeftButtonClick(mouse);

            //If the current Phase is skill selection, allow the user to pick a skill for the currentCharacter
            switch (CombatManager.CurrentPhase)
            {
                case CombatManager.PhaseEnum.SelectSkill:
                   // rv = _gActionSelect.ProcessLeftButtonClick(mouse);
                    break;

                case CombatManager.PhaseEnum.ChooseTarget:
                    CombatManager.SelectedAction.SetSkillTarget();
                    break;
                case CombatManager.PhaseEnum.DisplayVictory:
                    rv = _gPostScreen.ProcessLeftButtonClick(mouse);
                    break;
                case CombatManager.PhaseEnum.Defeat:
                    GUIManager.BeginFadeOut(true);
                    GoToHUDScreen();
                    MapManager.CurrentMap = MapManager.Maps["mapHospital"];
                    PlayerManager.CurrentMap = "mapHospital";
                    PlayerManager.PlayerActor.Position = Util.SnapToGrid(MapManager.CurrentMap.DictionaryCharacterLayer["playerSpawn"]);

                    foreach (ClassedCombatant c in PlayerManager.GetParty())
                    {
                        if (c.KnockedOut)
                        {
                            c.Recover();
                        }
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
            //_gActionSelect.CancelAction();

            if (CombatManager.CurrentPhase == CombatManager.PhaseEnum.ChooseTarget) { CombatManager.CurrentPhase = CombatManager.PhaseEnum.SelectSkill; }
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;

            if (!rv && _gActionPanel.Enabled) { rv = _gActionPanel.ProcessHover(mouse); }
            if (!rv) { rv = _gTurnOrder.ProcessHover(mouse); }
            if (CombatManager.PhaseChooseTarget())
            {
                rv = HandleHoverTargeting();
            }
            else
            {
                bool hoverPanel = false;
                bool loop = true;
                GUICombatTile[,] array = _arrAllies;
                while (loop)
                {
                    foreach (GUICombatTile t in array)
                    {
                        if (t.Contains(mouse) && t.Occupied())
                        {
                            hoverPanel = true;
                            if (!_gHoverCharacterInfo.DisplayingActor(t.MapTile.Character))
                            {
                                _gHoverCharacterInfo.SetActor(t.MapTile.Character);
                            }
                        }

                        rv = t.ProcessHover(mouse);
                        if (rv)
                        {
                            goto Exit;
                        }
                    }
                    if (array != _arrEnemies) { array = _arrEnemies; }
                    else { loop = false; }
                }
                Exit:

                _gHoverCharacterInfo.Show(hoverPanel);
            }

            if (_gPostScreen != null)
            {
                _gPostScreen.ProcessHover(mouse);
            }


            return rv;
        }

        internal bool HandleHoverTargeting()
        {
            bool rv = false;

            if (CombatManager.SelectedAction.TargetsEnemy())
            {
                rv = HoverTargetHelper(_arrEnemies);
            }
            else if (CombatManager.SelectedAction.TargetsAlly())
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
            CombatManager.Update(gameTime);
            base.Update(gameTime);

            switch (CombatManager.CurrentPhase)
            {
                case CombatManager.PhaseEnum.EnemyTurn:
                    //_gTurnOrder.CalculateTurnOrder();
                    CombatManager.SelectedAction.SetSkillTarget();
                    break;

                case CombatManager.PhaseEnum.NewTurn:
                    if (!CombatManager.ActiveCharacter.IsActorType(ActorEnum.Monster))
                    {
                        //_gActionSelect.SetCharacter(CombatManager.ActiveCharacter);
                        //_gActionSelect.AnchorToScreen(SideEnum.Bottom);
                    }
                    //_gTurnOrder.CalculateTurnOrder();
                    CombatManager.SelectedAction = null;
                    CombatManager.CurrentPhase = CombatManager.PhaseEnum.SelectSkill;
                    break;

                case CombatManager.PhaseEnum.ChooseTarget:
                    CombatManager.HandleKeyboardTargetting();

                    //Cancel out of selections made if escape is hit
                    if (InputManager.CheckPressedKey(Keys.Escape))
                    {
                        CancelAction();
                    }
                    break;

                case CombatManager.PhaseEnum.DisplayAttack:
                    //_gActionSelect.SetCharacter(null);
                    CombatManager.CurrentPhase = CombatManager.PhaseEnum.PerformAction;
                    if (!string.IsNullOrEmpty(CombatManager.Text))
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

                case CombatManager.PhaseEnum.PerformAction:
                    if (CombatManager.SelectedAction != null)
                    {
                        CombatManager.SelectedAction.PerformAction(gameTime);
                    }

                    break;
                case CombatManager.PhaseEnum.DisplayVictory:
                    if (_gPostScreen == null)
                    {
                        InventoryManager.InitMobInventory(1, 5);
                        _gPostScreen = new GUIPostCombatDisplay(ClosePostCombatDisplay);
                        _gPostScreen.CenterOnScreen();
                    }

                    break;
                case CombatManager.PhaseEnum.Defeat:
                    //GUITextWindow window = new GUITextWindow("Defeated");
                    //window.CenterOnScreen();
                    //AddControl(window);
                    break;
            }

            List<LiteSummon> summons = new List<LiteSummon>();
            foreach (CombatActor act in CombatManager.Party)
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

            if (CombatManager.SelectedAction != null) { CombatManager.SelectedAction.Draw(spriteBatch); }

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
            CombatManager.EndCombatVictory();
        }

        //private class ActionSelectObject : GUIObject
        //{
        //    GUIText _gText;
        //    ActionBar _gActionBar;

        //    public ActionSelectObject()
        //    {
        //        _gActionBar = new ActionBar();
        //        _gText = new GUIText();
        //    }

        //    public override void Draw(SpriteBatch spriteBatch)
        //    {
        //        _gActionBar.Draw(spriteBatch);
        //        _gText.Draw(spriteBatch);
        //    }

        //    public override bool ProcessLeftButtonClick(Point mouse)
        //    {
        //        return _gActionBar.ProcessLeftButtonClick(mouse);
        //    }

        //    public override bool ProcessHover(Point mouse)
        //    {
        //        bool rv = false;

        //        if (CombatManager.CurrentPhase != CombatManager.PhaseEnum.ChooseTarget && _gActionBar.ProcessHover(mouse))
        //        {
        //            rv = true;
        //            SyncText();
        //        }

        //        return rv;
        //    }

        //    public void SetCharacter(CombatActor activeCharacter)
        //    {
        //        _gActionBar.SetCharacter(activeCharacter);
        //        SyncText();

        //        Width = _gActionBar.Width;
        //        Height = _gActionBar.Height + _gText.Height;
        //    }

        //    public override void Position(Vector2 value)
        //    {
        //        base.Position(value);
        //        _gActionBar.Position(value);
        //        _gText.AnchorAndAlignToObject(_gActionBar, SideEnum.Bottom, SideEnum.CenterX);
        //    }

        //    private void SyncText()
        //    {
        //        LiteMenuAction selectedAction = _gActionBar.SelectedAction;
        //        Item selectedItem = _gActionBar.SelectedItem;

        //        string actionName = string.Empty;

        //        if (selectedAction != null)
        //        {
        //            actionName = selectedAction.Name;
        //          //  if (!selectedAction.IsMenu() && ((LiteCombatAction)selectedAction).MPCost > 0)
        //            {
        //          //      actionName = actionName + "  " + ((LiteCombatAction)selectedAction).MPCost.ToString() + " MP";
        //            }
        //        }
        //        else if (selectedItem != null)
        //        {
        //            actionName = selectedItem.Name + "  x" + selectedItem.Number;
        //        }

        //        _gText.SetText(actionName);
        //        _gText.AnchorAndAlignToObject(_gActionBar, SideEnum.Bottom, SideEnum.CenterX);
        //    }

        //    public void CancelAction()
        //    {
        //        _gActionBar.CancelAction();
        //        SyncText();
        //    }

        //    private class ActionBar : GUIObject
        //    {
        //        ActionMenu _actionMenu;
        //        CombatActor _actor;
        //        ActionButton _gSelectedAction;
        //        ActionButton _gSelectedMenu;
        //        List<ActionButton> _liActionButtons;

        //        public LiteMenuAction SelectedAction => _gSelectedAction?.Action;
        //        public Item SelectedItem => (_actionMenu != null && _actionMenu.SelectedAction.Item != null) ? _actionMenu.SelectedAction.Item : null;

        //        public ActionBar()
        //        {
        //            _liActionButtons = new List<ActionButton>();
        //        }

        //        public override void Draw(SpriteBatch spriteBatch)
        //        {
        //            foreach (ActionButton ab in _liActionButtons)
        //            {
        //                if (ab != _gSelectedAction && ab != _gSelectedMenu)
        //                {
        //                    ab.Draw(spriteBatch);
        //                }
        //            }

        //            if (_gSelectedMenu != null) { _gSelectedMenu.Draw(spriteBatch); }
        //            if (_gSelectedAction != null) { _gSelectedAction.Draw(spriteBatch); }

        //            if (_actionMenu != null) { _actionMenu.Draw(spriteBatch); }
        //        }

        //        public override bool ProcessLeftButtonClick(Point mouse)
        //        {
        //            bool rv = false;
        //            if (_actionMenu != null)
        //            {
        //                if (_actionMenu.ProcessLeftButtonClick(mouse))
        //                {
        //                    rv = true;
        //                    if (_actionMenu.ShowSpells())
        //                    {
        //                        LiteMenuAction a = _actionMenu.SelectedAction.Action;
        //                        CombatManager.ProcessActionChoice((LiteCombatAction)a);
        //                    }
        //                    else if (_actionMenu.ShowItems())
        //                    {
        //                        CombatManager.ProcessItemChoice((Consumable)_actionMenu.SelectedAction.Item);
        //                    }
        //                }
        //            }

        //            for (int i = 0; i < _liActionButtons.Count; i++)
        //            {
        //                ActionButton ab = _liActionButtons[i];
        //                if (ab.ProcessLeftButtonClick(mouse))
        //                {
        //                    rv = true;
        //                    LiteMenuAction a = ab.Action;
        //                    if (a.Compare(ActionEnum.Action) || a.Compare(ActionEnum.Spell))
        //                    {
        //                        CombatManager.ProcessActionChoice((LiteCombatAction)a);
        //                    }
        //                    else if (a.Compare(ActionEnum.Move))
        //                    {
        //                        CombatManager.ProcessActionChoice((LiteCombatAction)a);
        //                    }
        //                    else if (a.Compare(ActionEnum.EndTurn))
        //                    {
        //                        CombatManager.EndCombatEscape();
        //                    }
        //                    else
        //                    {
        //                        if (a.Compare(ActionEnum.MenuSpell))
        //                        {
        //                            if (CombatManager.ActiveCharacter.SpecialActions.Count > 0)
        //                            {
        //                                _gSelectedMenu = ab;
        //                                _actionMenu = new ActionMenu(CombatManager.ActiveCharacter.SpecialActions);
        //                                _actionMenu.AnchorAndAlignToObject(this, SideEnum.Top, SideEnum.CenterX, 10);
        //                            }
        //                        }
        //                        else if (a.Compare(ActionEnum.MenuItem) && InventoryManager.GetPlayerCombatItems().Count > 0)
        //                        {
        //                            _gSelectedMenu = ab;
        //                            _actionMenu = new ActionMenu(InventoryManager.GetPlayerCombatItems());
        //                            _actionMenu.AnchorAndAlignToObject(this, SideEnum.Top, SideEnum.CenterX, 10);
        //                        }
        //                    }

        //                    break;
        //                }
        //            }

        //            return rv;
        //        }

        //        public override bool ProcessHover(Point mouse)
        //        {
        //            bool rv = false;

        //            if (_actionMenu != null)
        //            {
        //                if (_actionMenu.ProcessHover(mouse))
        //                {
        //                    rv = true;
        //                    _gSelectedAction = _actionMenu.SelectedAction;
        //                }
        //            }
        //            else
        //            {
        //                for (int i = 0; i < _liActionButtons.Count; i++)
        //                {
        //                    ActionButton ab = _liActionButtons[i];
        //                    if (ab.Contains(mouse))
        //                    {
        //                        rv = true;
        //                        if (_gSelectedAction != null) { _gSelectedAction.Unselect(); }
        //                        _gSelectedAction = ab;
        //                        _gSelectedAction.Select();
        //                        break;
        //                    }
        //                }
        //            }

        //            return rv;
        //        }

        //        public void CancelAction()
        //        {
        //            if (_actionMenu != null)
        //            {
        //                if (CombatManager.CurrentPhase == CombatManager.PhaseEnum.ChooseTarget)
        //                {
        //                    _gSelectedAction = _gSelectedMenu ?? _liActionButtons[0];
        //                }
        //                else
        //                {
        //                    _actionMenu = null;
        //                    _gSelectedAction = _gSelectedMenu ?? _liActionButtons[0];
        //                }

        //                ProcessHover(GUICursor.Position.ToPoint());
        //            }
        //        }

        //        public void SetCharacter(CombatActor activeCharacter)
        //        {
        //            _actionMenu = null;
        //            _gSelectedMenu = null;
        //            _gSelectedAction = null;

        //            _actor = activeCharacter;
        //            _liActionButtons.Clear();

        //            if (_actor != null)
        //            {
        //                foreach (LiteMenuAction ca in _actor.AbilityList)
        //                {
        //                    ActionButton ab = new ActionButton(ca);
        //                    _liActionButtons.Add(ab);

        //                    if (ab.Action.IsMenu() && ab.Action.Compare(ActionEnum.MenuSpell))
        //                    {
        //                        ab.Enable(false);
        //                    }
        //                    if (ab.Action.IsMenu() && ab.Action.Compare(ActionEnum.MenuItem) && InventoryManager.GetPlayerCombatItems().Count == 0)
        //                    {
        //                        ab.Enable(false);
        //                    }
        //                }

        //                _gSelectedAction = _liActionButtons[0];

        //                Width = _liActionButtons.Count * _liActionButtons[0].Width;
        //                Height = _liActionButtons[0].Height;
        //            }
        //        }

        //        public override void Position(Vector2 value)
        //        {
        //            base.Position(value);
        //            _liActionButtons[0].Position(value);
        //            for (int i = 1; i < _liActionButtons.Count; i++)
        //            {
        //                _liActionButtons[i].AnchorAndAlignToObject(_liActionButtons[i - 1], SideEnum.Right, SideEnum.CenterY);
        //            }

        //            if (_actionMenu != null) { _actionMenu.AnchorAndAlignToObject(this, SideEnum.Top, SideEnum.CenterX, 10); }
        //        }

        //        private class ActionMenu : GUIObject
        //        {
        //            public enum DisplayEnum { Spells, Items };
        //            private DisplayEnum _display;

        //            List<ActionButton> _liActions;

        //            ActionButton _gSelectedAction;
        //            public ActionButton SelectedAction => _gSelectedAction;

        //            public ActionMenu(List<LiteCombatAction> specialsList)
        //            {
        //                _display = DisplayEnum.Spells;
        //                _liActions = new List<ActionButton>();

        //                for (int i = 0; i < specialsList.Count; i++)
        //                {
        //                    _liActions.Add(new ActionButton(specialsList[i]));
        //                }

        //                Width = Math.Min(_liActions.Count, 5) * _liActions[0].Width;
        //                int numRows = 0;
        //                int temp = _liActions.Count;
        //                do
        //                {
        //                    numRows++;
        //                    temp -= 5;
        //                } while (temp > 0);
        //                Height = numRows * _liActions[0].Height;

        //                Position(Position());
        //            }

        //            public ActionMenu(List<Consumable> itemList)
        //            {
        //                _display = DisplayEnum.Items;
        //                _liActions = new List<ActionButton>();

        //                for (int i = 0; i < itemList.Count; i++)
        //                {
        //                    _liActions.Add(new ActionButton(itemList[i]));
        //                }

        //                Width = Math.Min(_liActions.Count, 5) * _liActions[0].Width;
        //                int numRows = 0;
        //                int temp = _liActions.Count;
        //                do
        //                {
        //                    numRows++;
        //                    temp -= 5;
        //                } while (temp > 0);
        //                Height = numRows * _liActions[0].Height;

        //                Position(Position());
        //            }

        //            public override void Draw(SpriteBatch spriteBatch)
        //            {
        //                if (_liActions != null)
        //                {
        //                    foreach (ActionButton ab in _liActions)
        //                    {
        //                        if (ab != SelectedAction)
        //                        {
        //                            ab.Draw(spriteBatch);
        //                        }
        //                    }

        //                    if (SelectedAction != null) { SelectedAction.Draw(spriteBatch); }
        //                }
        //            }

        //            public override bool ProcessLeftButtonClick(Point mouse)
        //            {
        //                bool rv = false;
        //                if (_liActions != null)
        //                {
        //                    for (int i = 0; i < _liActions.Count; i++)
        //                    {
        //                        rv = _liActions[i].Contains(mouse);
        //                        if (rv) { break; }
        //                    }
        //                }

        //                return rv;
        //            }

        //            public override bool ProcessHover(Point mouse)
        //            {
        //                bool rv = false;

        //                if (_liActions != null)
        //                {
        //                    for (int i = 0; i < _liActions.Count; i++)
        //                    {
        //                        ActionButton ab = _liActions[i];
        //                        if (ab.Contains(mouse))
        //                        {
        //                            rv = true;
        //                            if (_gSelectedAction != null) { _gSelectedAction.Unselect(); }
        //                            _gSelectedAction = ab;
        //                            _gSelectedAction.Select();
        //                            break;
        //                        }
        //                    }
        //                }

        //                return rv;
        //            }

        //            public override void Position(Vector2 value)
        //            {
        //                base.Position(value);
        //                if (_liActions != null)
        //                {
        //                    _liActions[0].AnchorToInnerSide(this, SideEnum.BottomLeft);
        //                    for (int i = 1; i < _liActions.Count; i++)
        //                    {
        //                        if (i % 5 == 0)
        //                        {
        //                            _liActions[i].AnchorAndAlignToObject(_liActions[i - 5], SideEnum.Top, SideEnum.Left);
        //                        }
        //                        else
        //                        {
        //                            _liActions[i].AnchorAndAlignToObject(_liActions[i - 1], SideEnum.Right, SideEnum.Bottom);
        //                        }
        //                    }
        //                }
        //            }

        //            public bool ShowItems() { return _display == DisplayEnum.Items; }
        //            public bool ShowSpells() { return _display == DisplayEnum.Spells; }
        //        }

        //        private class ActionButton : GUIImage
        //        {
        //            LiteMenuAction _action;
        //            public LiteMenuAction Action => _action;

        //            Item _item;
        //            public Item Item => _item;

        //            GUIImage _gItem;

        //            public ActionButton(LiteMenuAction action) : base(new Rectangle((int)action.IconGrid.X * TILE_SIZE, (int)action.IconGrid.Y * TILE_SIZE, TILE_SIZE, TILE_SIZE), TILE_SIZE, TILE_SIZE, @"Textures\texCmbtActions")
        //            {
        //                _action = action;
        //                SetScale(GameManager.CurrentScale);
        //            }

        //            public ActionButton(Item i) : base(new Rectangle(288, 32, 32, 32), 16, 16, @"Textures\Dialog")
        //            {
        //                _item = i;
        //                _gItem = new GUIImage(_item.SourceRectangle, Width, Height, _item.Texture);
        //                SetScale(GameManager.CurrentScale);
        //            }

        //            public override void Draw(SpriteBatch spriteBatch)
        //            {
        //                spriteBatch.Draw(_texture, _drawRect, _sourceRect, EnabledColor * Alpha());
        //                if (_item != null) { _gItem.Draw(spriteBatch); }
        //            }

        //            public string GetName()
        //            {
        //                string rv = string.Empty;

        //                if (_action != null) { rv = _action.Name; }
        //                else if (_item != null) { rv = _item.Name; }

        //                return rv;
        //            }

        //            public void Select()
        //            {
        //                int firstWidth = Width;
        //                int firstHeight = Height;
        //                SetScale(GameManager.CurrentScale + 1);

        //                int diffWidth = Width - firstWidth;
        //                int diffHeight = Height - firstHeight;

        //                MoveBy(new Vector2(-diffWidth / 2, -diffHeight / 2));
        //            }
        //            public void Unselect()
        //            {
        //                int firstWidth = Width;
        //                int firstHeight = Height;
        //                SetScale(GameManager.CurrentScale);

        //                int diffWidth = firstWidth - Width;
        //                int diffHeight = firstHeight - Height;

        //                MoveBy(new Vector2(diffWidth / 2, diffHeight / 2));
        //            }

        //            public override void Position(Vector2 value)
        //            {
        //                base.Position(value);
        //                if (_gItem != null) { _gItem.Position(value); }
        //            }

        //            public override void SetScale(double x, bool anchorToPos = true)
        //            {
        //                base.SetScale(x);
        //                if (_gItem != null) { _gItem.SetScale(x); }
        //            }
        //        }
        //    }
        //}

        private class GUIPostCombatDisplay : GUIObject
        {
            GUIButton _btnClose;
            GUIWindow _gWin;
            GUIOldStatDisplay _gXPToGive;
            GUIOldStatDisplay[] _arrCharXP;

            bool _bDisplayItems;

            public GUIPostCombatDisplay(BtnClickDelegate closeDelegate)
            {
                _bDisplayItems = false;
                _arrCharXP = new GUIOldStatDisplay[4];
                _gWin = new GUIWindow(GUIWindow.Window_1, GUIManager.MAIN_COMPONENT_WIDTH, GUIManager.MAIN_COMPONENT_HEIGHT);
                _gXPToGive = new GUIOldStatDisplay(CombatManager.CurrentMob.GetXP, Color.Yellow);
                _gXPToGive.CenterOnObject(_gWin);
                _gXPToGive.AnchorToInnerSide(_gWin, SideEnum.Top);

                for (int i = 0; i < PlayerManager.GetParty().Count; i++)
                {
                    ClassedCombatant adv = PlayerManager.GetParty()[i];
                    _arrCharXP[i] = new GUIOldStatDisplay(adv.GetXP, Color.Yellow);

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
