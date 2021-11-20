using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Actors.CombatStuff;
using RiverHollow.Characters;
using RiverHollow.CombatStuff;
using RiverHollow.GUIComponents.GUIObjects;
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

        TurnOrderDisplay _gTurnOrder;

        public LiteCombatScreen()
        {
            _sdStamina = new GUIStatDisplay(PlayerManager.GetStamina, Color.Red);
            AddControl(_sdStamina);

            _gActionSelect = new ActionSelectObject();
            AddControl(_gActionSelect);

            ConfigureGUIMap();

            LiteCombatManager.AssignPositions(ref _arrAllies);

            _gTurnOrder = new TurnOrderDisplay();
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
                    BackToMain();
                    MapManager.CurrentMap = MapManager.Maps["mapHospital"];
                    PlayerManager.CurrentMap = "mapHospital";
                    PlayerManager.World.Position = Util.SnapToGrid(MapManager.CurrentMap.DictionaryCharacterLayer["playerSpawn"]);

                    foreach (CombatAdventurer c in PlayerManager.GetTacticalParty())
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
    }

    /// <summary>
    /// This class represents a combat actor, as well as the display information for them
    /// </summary>
    public class GUICombatActorInfo : GUIObject
    {
        GUICombatTile _gAssignedTile;     //This tile is justa reference
        LiteCombatActor _actor;
        GUIStatDisplay _gHP;
        GUIStatDisplay _gMP;
        GUILiteCombatActor _gLiteCombatActor;
        public GUISprite CharacterSprite => _gLiteCombatActor.CharacterSprite;
        public GUISprite CharacterWeaponSprite => _gLiteCombatActor.CharacterWeaponSprite;
        public GUICombatTile AssignedTile => _gAssignedTile;

        public GUICombatActorInfo(LiteCombatActor actor)
        {
            _actor = actor;
            _gLiteCombatActor = new GUILiteCombatActor(actor.BodySprite);
            AddControl(_gLiteCombatActor);

            SetWeapon();
            _gHP = new GUIStatDisplay(actor.GetHP, Color.Green, 100);
            _gHP.AnchorAndAlignToObject(_gLiteCombatActor, SideEnum.Bottom, SideEnum.CenterX);
            AddControl(_gHP);

            if (actor.MaxMP > 0)
            {
                _gMP = new GUIStatDisplay(actor.GetMP, Color.LightBlue, 100);
                _gMP.AnchorAndAlignToObject(_gHP, SideEnum.Bottom, SideEnum.Left);
                AddControl(_gMP);
            }

            Width = _gLiteCombatActor.Width;
            Height = _gMP.Bottom - _gLiteCombatActor.Top;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (LiteCombatManager.CurrentPhase == LiteCombatManager.PhaseEnum.PerformAction && LiteCombatManager.ActiveCharacter == _actor)
            {
                _gHP.Show(false);
                if (_gMP != null) { _gMP.Show(false); }
            }
            base.Draw(spriteBatch);

            _gHP.Show(true);
            if (_gMP != null) { _gMP.Show(true); }
        }

        public void SetWeapon()
        {
            if (_actor.IsCombatAdventurer())
            {
                CombatAdventurer adv = (CombatAdventurer)_actor;
                CharacterClass cClass = adv.CharacterClass;

                AnimatedSprite sprWeaponSprite = new AnimatedSprite(DataManager.FOLDER_ITEMS + "Combat\\Weapons\\" + cClass.WeaponType.ToString() + "\\" + adv.Weapon.GetItem().ItemID);

                int xCrawl = 0;
                RHSize frameSize = new RHSize(32, 32);
                sprWeaponSprite.AddAnimation(LiteCombatActionEnum.Idle, xCrawl, 0, frameSize, cClass.IdleFrames, cClass.IdleFramesLength);
                xCrawl += cClass.IdleFrames;
                sprWeaponSprite.AddAnimation(LiteCombatActionEnum.Cast, (xCrawl * frameSize.Width), 0, frameSize, cClass.CastFrames, cClass.CastFramesLength);
                xCrawl += cClass.CastFrames;
                sprWeaponSprite.AddAnimation(LiteCombatActionEnum.Hurt, (xCrawl * frameSize.Width), 0, frameSize, cClass.HitFrames, cClass.HitFramesLength);
                xCrawl += cClass.HitFrames;
                sprWeaponSprite.AddAnimation(LiteCombatActionEnum.Attack, (xCrawl * frameSize.Width), 0, frameSize, cClass.AttackFrames, cClass.AttackFramesLength);
                xCrawl += cClass.AttackFrames;
                sprWeaponSprite.AddAnimation(LiteCombatActionEnum.Critical, (xCrawl * frameSize.Width), 0, frameSize, cClass.CriticalFrames, cClass.CriticalFramesLength);
                xCrawl += cClass.CriticalFrames;
                sprWeaponSprite.AddAnimation(LiteCombatActionEnum.KO, (xCrawl * frameSize.Width), 0, frameSize, cClass.KOFrames, cClass.KOFramesLength);
                xCrawl += cClass.KOFrames;
                sprWeaponSprite.AddAnimation(LiteCombatActionEnum.Victory, (xCrawl * frameSize.Width), 0, frameSize, cClass.WinFrames, cClass.WinFramesLength);
                sprWeaponSprite.SetScale(LiteCombatManager.CombatScale);

                _gLiteCombatActor.SetWeapon(sprWeaponSprite);
            }
        }

        public void AssignTile(GUICombatTile tile)
        {
            _gAssignedTile = tile;
        }

        public void Reset()
        {
            _gLiteCombatActor.Reset();
        }

        public void PlayAnimation<TEnum>(TEnum animation)
        {
            _gLiteCombatActor.PlayAnimation(animation);
        }

        public class GUILiteCombatActor : GUIObject
        {
            GUISprite _gSprite;
            GUISprite _gSpriteWeapon;
            public GUISprite CharacterSprite => _gSprite;
            public GUISprite CharacterWeaponSprite => _gSpriteWeapon;

            public GUILiteCombatActor(AnimatedSprite sprite)
            {
                _gSprite = new GUISprite(sprite);
                AddControl(_gSprite);

                Width = _gSprite.Width;
                Height = _gSprite.Height;
            }

            public void SetWeapon(AnimatedSprite sprite)
            {
                _gSpriteWeapon = new GUISprite(sprite);
                AddControl(_gSpriteWeapon);
            }

            public void Reset()
            {
                _gSprite.Reset();
                if (_gSpriteWeapon != null) { _gSpriteWeapon.Reset(); }
            }

            public void PlayAnimation<TEnum>(TEnum animation)
            {
                _gSprite.PlayAnimation(animation);
                if (_gSpriteWeapon != null) { _gSpriteWeapon.PlayAnimation(animation); }
            }
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
                if (!selectedAction.IsMenu() && ((LiteCombatAction)selectedAction).MPCost > 0)
                {
                    actionName = actionName + "  " + ((LiteCombatAction)selectedAction).MPCost.ToString() + " MP";
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

        public class ActionBar : GUIObject
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
                        if (!a.IsMenu())
                        {
                            if (LiteCombatManager.ActiveCharacter.CanCast(((LiteCombatAction)a).MPCost))
                            {
                                LiteCombatManager.ProcessActionChoice((LiteCombatAction)a);
                            }
                        }
                        else
                        {
                            if (a.IsMenu() && a.IsSpecial() && !LiteCombatManager.ActiveCharacter.Silenced())
                            {
                                if (LiteCombatManager.ActiveCharacter.SpecialActions.Count > 0)
                                {
                                    _gSelectedMenu = ab;
                                    _actionMenu = new ActionMenu(LiteCombatManager.ActiveCharacter.SpecialActions);
                                    _actionMenu.AnchorAndAlignToObject(this, SideEnum.Top, SideEnum.CenterX, 10);
                                }
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

                        if (ab.Action.IsMenu() && ab.Action.IsSpecial() && LiteCombatManager.ActiveCharacter.Silenced())
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
        List<LiteCombatActor> _liNewTurnOrder;
        GUIWindow _gWindow;

        public TurnOrderDisplay()
        {
            _arrTurnDisplay = new TurnDisplay[MAX_SHOWN];
            _arrBarDisplay = new GUIImage[MAX_SHOWN];

            _liNewTurnOrder = LiteCombatManager.CalculateTurnOrder(MAX_SHOWN);

            for (int i = 0; i < MAX_SHOWN; i++)
            {
                _arrTurnDisplay[i] = new TurnDisplay(_liNewTurnOrder[i], _arrBarDisplay);
                _arrBarDisplay[i] = new GUIImage(new Rectangle(48, 58, 10, 2), 10, 2, @"Textures\Dialog");
                _arrBarDisplay[i].SetScale(LiteCombatManager.CombatScale);
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
                    if (!_bSyncing && _iCurrUpdate == MAX_SHOWN - 1 && _arrTurnDisplay[_iCurrUpdate].Finished && _arrTurnDisplay[_iCurrUpdate].Action == TurnDisplay.ActionEnum.Insert)
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

            if (_gWindow != null) { _gWindow.Draw(spriteBatch); }
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;
            LiteCombatActor a = null;
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
                _gWindow = new GUIWindow(GUIWindow.Window_2, 10, 10);
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
            if (_bTriggered)
            {
                List<LiteCombatActor> newList = LiteCombatManager.CalculateTurnOrder(MAX_SHOWN);

                bool change = false;
                //Assume that only one entry can be wrong for insertions
                for (int i = 0; i < MAX_SHOWN - 1; i++)
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
            LiteCombatActor _actor;
            public LiteCombatActor Actor => _actor;

            bool _bFadeIn;
            public bool FadeIn => _bFadeIn;
            bool _bFadeOut;
            public bool Finished;

            float _fFadeSpeed;
            int _iIndex;
            Vector2 _vMoveTo = new Vector2(0, 0);

            GUIImage[] _arrBarDisplay;

            public TurnDisplay(LiteCombatActor actor, GUIImage[] barDisplay)
            {
                _actor = actor;
                _bInParty = !actor.IsMonster();
                _gName = new GUIText(actor.Name.Substring(0, 1));
                _gImage = new GUIImage(new Rectangle(48, 48, 10, 10), 10, 10, @"Textures\Dialog");
                _gImage.SetScale(LiteCombatManager.CombatScale);

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
                if (Alpha() > 0)
                {
                    SetAlpha(Alpha() - _fFadeSpeed);
                }
                else
                {
                    _bFadeOut = false;
                    Finished = true;
                }
            }
            private void UpdateFadeIn(GameTime gameTime)
            {
                if (Alpha() < 1)
                {
                    SetAlpha(Alpha() + _fFadeSpeed);
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

            public void SetActor(LiteCombatActor c)
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

            public void SetAlpha(float alpha)
            {
                Alpha(alpha);
                _gImage.Alpha(alpha);
                _gName.Alpha(alpha);
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

    public class GUIPostCombatDisplay : GUIObject
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

            for (int i = 0; i < PlayerManager.GetTacticalParty().Count; i++)
            {
                CombatAdventurer adv = PlayerManager.GetTacticalParty()[i];
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
