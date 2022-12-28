using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Characters;
using RiverHollow.Characters.Lite;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.Combat.Lite;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.Items;
using RiverHollow.Misc;
using RiverHollow.SpriteAnimations;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.GUIComponents.GUIObjects.GUIObject;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class CombatScreen : GUIScreen
    {
        GUICombatTile[,] _arrAllies;
        GUICombatTile[,] _arrEnemies;
        GUITextWindow _gActionTextWindow;
        GUIImage _gActionEffect;

        PostCombatDisplay _gPostCombatDisplay;

        GUIImage _gBackgroundImage;
        GUIButton _btnEscape;

        GUISprite _gActiveIndicator;
        TurnOrder _gTurnOrder;
        InfoPanel _gActiveCharacterInfo;
        InfoPanel _gHoverCharacterInfo;

        ActionPanel _gActionPanel;

        EmptyDelegate _switch;

        public CombatScreen(EmptyDelegate combatSwitch)
        {
            GameManager.CurrentScreen = GameScreenEnum.Combat;

            _switch = combatSwitch;
            _gBackgroundImage = new GUIImage(new Rectangle(0, 0, 480, 270), DataManager.GUI_COMPONENTS + @"\Combat_Background_Forest");
            AddControl(_gBackgroundImage);

            _btnEscape = new GUIButton(new Rectangle(112, 24, 24, 26), DataManager.COMBAT_TEXTURE, CombatManager.EndCombatEscape);
            _btnEscape.ScaledMoveBy(3, 3);
            AddControl(_btnEscape);

            _gActionPanel = new ActionPanel();
            AddControl(_gActionPanel);

            DisplayActionPanel();

            ConfigureGUIMap();

            CombatManager.AssignPositions(ref _arrAllies);

            _gTurnOrder = new TurnOrder(CombatManager.CurrentRoundOrder);
            AddControl(_gTurnOrder);

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
            int cols = CombatManager.MAX_COLUMN / 2;
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
            int cols = CombatManager.MAX_COLUMN / 2;
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

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);

            if (!CombatManager.CombatStarted) { _switch(); }

            if (CombatManager.CurrentRoundOrder.Count != _gTurnOrder.CurrentActors)
            {
                _gTurnOrder.DisplayNewTurn(CombatManager.CurrentRoundOrder);
            }

            switch (CombatManager.CurrentPhase)
            {
                case CombatManager.PhaseEnum.NewTurn:
                    _gTurnOrder.DisplayNewTurn(CombatManager.CurrentRoundOrder);
                    _gActiveIndicator.CenterOnObject(CombatManager.ActiveCharacter.Tile.GUITile);
                    ShowHUDObjects(true);
                    _gActiveCharacterInfo.SetActor(CombatManager.ActiveCharacter);
                    if (!CombatManager.PlayerTurn) { _gHoverCharacterInfo.Show(false); }
                    break;
                case CombatManager.PhaseEnum.Upkeep:
                case CombatManager.PhaseEnum.ChooseAction:
                    break;
                case CombatManager.PhaseEnum.ChooseTarget:
                    //Cancel out of selections made if escape is hit
                    if (InputManager.CheckPressedKey(Keys.Escape))
                    {
                        CancelAction();
                    }
                    break;
                case CombatManager.PhaseEnum.DisplayAttack:
                    //_gActionSelect.SetCharacter(null);

                    //if (!string.IsNullOrEmpty(CombatManager.Text))
                    //{
                    //    //if (_gActionTextWindow == null)
                    //    //{
                    //    //    _gActionTextWindow = new GUITextWindow(LiteCombatManager.Text, 0.5);
                    //    //    _gActionTextWindow.CenterOnScreen();
                    //    //    AddControl(_gActionTextWindow);
                    //    //}
                    //    //else
                    //    //{
                    //    //    _gActionTextWindow.Update(gameTime);
                    //    //    if (_gActionTextWindow.Duration <= 0)
                    //    //    {
                    //    //        RemoveControl(_gActionTextWindow);
                    //    //        _gActionTextWindow = null;
                    //    //        LiteCombatManager.CurrentPhase = LiteCombatManager.PhaseEnum.PerformAction;
                    //    //    }
                    //    //}
                    //}
                    break;
                case CombatManager.PhaseEnum.PerformAction:
                    break;
                case CombatManager.PhaseEnum.EnemyTurn:
                    _gTurnOrder.DisplayNewTurn(CombatManager.CurrentRoundOrder);
                    ShowHUDObjects(false);
                    _gActiveIndicator.CenterOnObject(CombatManager.ActiveCharacter.Tile.GUITile);
                    break;
                case CombatManager.PhaseEnum.EndCombat:
                case CombatManager.PhaseEnum.Defeat:
                    //GUITextWindow window = new GUITextWindow("Defeated");
                    //window.CenterOnScreen();
                    //AddControl(window);
                    break;
                case CombatManager.PhaseEnum.DisplayVictory:
                    _gActiveIndicator.Show(false);
                    if (_gPostCombatDisplay == null)
                    {
                        _gPostCombatDisplay = new PostCombatDisplay();
                        AddControl(_gPostCombatDisplay);
                    }
                    _gPostCombatDisplay?.Update(gTime);
                    break;
            }

            List<Summon> summons = new List<Summon>();
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

            _gPostCombatDisplay?.Draw(spriteBatch);

            _guiHoverWindow?.Draw(spriteBatch);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            if (CombatManager.Party.Contains(CombatManager.ActiveCharacter))
            {
                if (_btnEscape.ProcessLeftButtonClick(mouse))
                {
                    return true;
                }

                //If the current Phase is skill selection, allow the user to pick a skill for the currentCharacter
                switch (CombatManager.CurrentPhase)
                {
                    case CombatManager.PhaseEnum.ChooseAction:
                        rv = _gActionPanel.ProcessLeftButtonClick(mouse);
                        break;

                    case CombatManager.PhaseEnum.ChooseTarget:
                        _gActionPanel.ClearHoverTarget();
                        CombatManager.SelectedAction.SetSkillTarget();
                        break;
                    case CombatManager.PhaseEnum.DisplayVictory:
                        if (_gPostCombatDisplay != null) {
                            rv = _gPostCombatDisplay.ProcessLeftButtonClick(mouse);
                        }
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
            }

            return rv;
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = true;
            if (CombatManager.CurrentPhase == CombatManager.PhaseEnum.DisplayVictory && _gPostCombatDisplay != null)
            {
                rv = _gPostCombatDisplay.ProcessRightButtonClick(mouse);

            }

            if (CombatManager.CanCancel())
            {
                CancelAction();
            }
            return rv;
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;

            if (_gPostCombatDisplay != null)
            {
                rv = _gPostCombatDisplay.ProcessHover(mouse);
            }

            if (!CombatManager.PlayerTurn) { return false; }

            bool showHoverCharacterInfo = false;
            if (_gActionPanel.Enabled)
            {
                rv = _gActionPanel.ProcessHover(mouse);
            }

            if (!rv && CombatManager.PhaseChooseTarget())
            {
                rv = HandleHoverTargeting();
            }

            if (!rv)
            {
                CombatActor hoverTarget = _gTurnOrder.GetHoverActor(mouse);
                if (hoverTarget != null)
                {
                    rv = true;
                    showHoverCharacterInfo = true;
                    _gHoverCharacterInfo.SetActor(hoverTarget);
                }
            }

            bool loop = true;
            GUICombatTile[,] array = _arrAllies;
            while (loop)
            {
                foreach (GUICombatTile t in array)
                {
                    if (t.Contains(mouse) && t.Occupied())
                    {
                        showHoverCharacterInfo = true;
                        if (_gHoverCharacterInfo.ShouldRefresh(t.MapTile.Character))
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

            _gHoverCharacterInfo.Show(showHoverCharacterInfo);

            return rv;
        }

        internal void CancelAction()
        {
            CombatManager.ClearSelectedTile();
            CombatManager.SelectedAction = null;
            _gActionPanel.CancelAction();

            if (CombatManager.CurrentPhase == CombatManager.PhaseEnum.ChooseTarget)
            {
                CombatManager.CurrentPhase = CombatManager.PhaseEnum.ChooseAction;
            }
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

                if (rv)
                {
                    _gActionPanel.DamageUpdate(p.MapTile.Character);
                }
            }

            return rv;
        }

        private void ShowHUDObjects(bool value)
        {
            _gActiveCharacterInfo.Show(value);
            _btnEscape.Show(value);
            DisplayActionPanel();
        }
        private void DisplayActionPanel()
        {
            _gActionPanel.Show(CombatManager.PlayerTurn);

            if (CombatManager.PlayerTurn)
            {
                _gActionPanel.PopulateActions();
            }
        }

        private class PostCombatDisplay : GUIObject
        {
            public enum DisplayStageEnum { ShowXP, ItemLootAll, ItemInventory };
            DisplayStageEnum _eCurrentStage = DisplayStageEnum.ShowXP;

            GUITextWindow _gCombatText;
            GUIObject _gLoot;

            public PostCombatDisplay()
            {
                TextEntry entry = DataManager.GetGameTextEntry("Combat_Label_XP");
                entry.FormatText(CombatManager.CurrentMob.XP.ToString());
                _gCombatText = new GUITextWindow(entry, Vector2.Zero);
                _gCombatText.CenterOnScreen();
                AddControl(_gCombatText);
            }

            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = false;

                switch (_eCurrentStage)
                {
                    case DisplayStageEnum.ShowXP:
                        rv = true;
                        RemoveControl(_gCombatText);
                        CreateLootWindow();
                        break;
                    case DisplayStageEnum.ItemLootAll:
                        rv = true;
                        AssignItemsToInventory();
                        CombatManager.EndCombatVictory();
                        break;
                    case DisplayStageEnum.ItemInventory:
                        rv = _gLoot.ProcessLeftButtonClick(mouse);
                        break;
                }

                return rv;
            }

            public override bool ProcessRightButtonClick(Point mouse)
            {
                bool rv = false;

                switch (_eCurrentStage)
                {
                    case DisplayStageEnum.ShowXP:
                        rv = true;
                        RemoveControl(_gCombatText);
                        CreateLootWindow();
                        break;
                    case DisplayStageEnum.ItemLootAll:
                        rv = true;
                        AssignItemsToInventory();
                        CombatManager.EndCombatVictory();
                        break;
                    case DisplayStageEnum.ItemInventory:
                        rv = _gLoot.ProcessRightButtonClick(mouse);
                        if (!rv)
                        {
                            AssignItemsToInventory();
                            CombatManager.EndCombatVictory();
                            rv = true;
                        }
                        break;
                }

                return rv;
            }

            private void CreateLootWindow()
            {
                Item[,] loot = CombatManager.CurrentMob.GetLoot();

                bool canFit = true;
                foreach (Item i in loot)
                {
                    if (!InventoryManager.HasSpaceInInventory(i.ID, i.Number))
                    {
                        canFit = false;
                        break;
                    }
                }

                if (canFit)
                {
                    _eCurrentStage = DisplayStageEnum.ItemLootAll;
                    InventoryManager.InitExtraInventory(loot);
                    _gLoot = new GUIInventory(false);
                }
                else
                {
                    _eCurrentStage = DisplayStageEnum.ItemInventory;
                    _gLoot = new HUDInventoryDisplay(loot, DisplayTypeEnum.Inventory);
                }
                _gLoot.CenterOnScreen();
                AddControl(_gLoot);

                _gCombatText = new GUITextWindow(DataManager.GetGameTextEntry("Combat_Label_Items"), Vector2.Zero);
                _gCombatText.AnchorAndAlignToObject(_gLoot, SideEnum.Top, SideEnum.CenterX, ScaleIt(1));
                AddControl(_gCombatText);
            }

            private void AssignItemsToInventory()
            {
                foreach (Item i in InventoryManager.ExtraInventory)
                {
                    InventoryManager.AddToInventory(i);
                }
            }
        }
    }
}
