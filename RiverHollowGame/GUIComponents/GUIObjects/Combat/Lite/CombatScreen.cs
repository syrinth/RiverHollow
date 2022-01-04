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
            AddControl(_gActionPanel);

            DisplayActionPanel();

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

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            if (CombatManager.Party.Contains(CombatManager.ActiveCharacter))
            {
                _btnEscape.ProcessLeftButtonClick(mouse);

                //If the current Phase is skill selection, allow the user to pick a skill for the currentCharacter
                switch (CombatManager.CurrentPhase)
                {
                    case CombatManager.PhaseEnum.ChooseAction:
                        rv = _gActionPanel.ProcessLeftButtonClick(mouse);
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
            }

            return rv;
        }

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
            _gActionPanel.CancelAction();

            if (CombatManager.CurrentPhase == CombatManager.PhaseEnum.ChooseTarget)
            {
                CombatManager.CurrentPhase = CombatManager.PhaseEnum.ChooseAction;
            }
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;

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

            if (CombatManager.CurrentRoundOrder.Count != _gTurnOrder.CurrentActors)
            {
                _gTurnOrder.DisplayNewTurn(CombatManager.CurrentRoundOrder);
            }

            switch (CombatManager.CurrentPhase)
            {
                case CombatManager.PhaseEnum.EnemyTurn:
                    _gTurnOrder.DisplayNewTurn(CombatManager.CurrentRoundOrder);
                    ShowHUDObjects(false);
                    _gActiveIndicator.CenterOnObject(CombatManager.ActiveCharacter.Tile.GUITile);
                    CombatManager.SelectedAction.SetSkillTarget();
                    break;

                case CombatManager.PhaseEnum.NewTurn:
                    _gTurnOrder.DisplayNewTurn(CombatManager.CurrentRoundOrder);
                    ShowHUDObjects(true);
                    _gActiveCharacterInfo.SetActor(CombatManager.ActiveCharacter);
                    _gActiveIndicator.CenterOnObject(CombatManager.ActiveCharacter.Tile.GUITile);

                    CombatManager.SelectedAction = null;
                    CombatManager.CurrentPhase = CombatManager.PhaseEnum.ChooseAction;
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
            if (_gPostScreen != null)
            {
                _gPostScreen.Draw(spriteBatch);
            }
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

        private void ClosePostCombatDisplay()
        {
            _gPostScreen = null;
            CombatManager.EndCombatVictory();
        }

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
