using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using RiverHollow.Actors;
using RiverHollow.Actors.CombatStuff;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.GUIObjects;
using RiverHollow.Misc;
using RiverHollow.Tile_Engine;
using RiverHollow.WorldObjects;
using System;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.GUIObjects.GUIObject;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class CombatScreen : GUIScreen
    {
        ActionSelectObject _gActionSelect;
        TurnOrderDisplay _gTurnOrder;
        GUIActorInfoPanel _gActorInfoPanel;

        private Dictionary<FloatingText, bool> _diFTextQueue;
        private List<FloatingText> _liFloatingText;

        public CombatScreen()
        {
            //_sdStamina = new GUIStatDisplay(PlayerManager.GetStamina, Color.Red);
            //AddControl(_sdStamina);
            _diFTextQueue = new Dictionary<FloatingText, bool>();
            _liFloatingText = new List<FloatingText>();

            _gTurnOrder = new TurnOrderDisplay();
            _gTurnOrder.AnchorToScreen(SideEnum.Top);
            AddControl(_gTurnOrder);
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);

            foreach (KeyValuePair<FloatingText, bool> kvp in _diFTextQueue)
            {
                if (kvp.Value) { _liFloatingText.Add(kvp.Key); }
                else { _liFloatingText.Remove(kvp.Key); }
            }
            _diFTextQueue.Clear();
            foreach (FloatingText text in _liFloatingText)
            {
                text.Update(gTime);
            }

            switch (CombatManager.CurrentPhase)
            {
                case CombatManager.CmbtPhaseEnum.MainSelection:
                    _gActionSelect?.Update(gTime);
                    break;

                case CombatManager.CmbtPhaseEnum.DisplayDefeat:
                    GUITextWindow window = new GUITextWindow("Defeated");
                    window.CenterOnScreen();
                    AddControl(window);
                    break;
            }

            //switch (CombatManager.CurrentPhase)
            //{
            //    case CombatManager.PhaseEnum.EnemyTurn:
            //        _gTurnOrder.CalculateTurnOrder();
            //        CombatManager.SelectedAction.SetSkillTarget();
            //        break;

            //    case CombatManager.PhaseEnum.NewTurn:
            //        if (!CombatManager.ActiveCharacter.IsMonster())
            //        {
            //            _gActionSelect.SetCharacter(CombatManager.ActiveCharacter);
            //            _gActionSelect.AnchorToScreen(SideEnum.Bottom);
            //        }
            //        _gTurnOrder.CalculateTurnOrder();
            //        CombatManager.SelectedAction = null;
            //        CombatManager.ChangePhase(CombatManager.PhaseEnum.SelectSkill);
            //        break;

            //    case CombatManager.PhaseEnum.ChooseTarget:
            //        ;

            //    case CombatManager.PhaseEnum.DisplayAttack:
            //        _gActionSelect.SetCharacter(null);
            //        if (!string.IsNullOrEmpty(CombatManager.Text))
            //        {
            //            if (_gActionTextWindow == null)
            //            {
            //                _gActionTextWindow = new GUITextWindow(CombatManager.Text, 0.5);
            //                _gActionTextWindow.CenterOnScreen();
            //                AddControl(_gActionTextWindow);
            //            }
            //            else
            //            {
            //                _gActionTextWindow.Update(gTime);
            //                if (_gActionTextWindow.Duration <= 0)
            //                {
            //                    RemoveControl(_gActionTextWindow);
            //                    _gActionTextWindow = null;
            //                    CombatManager.ChangePhase(CombatManager.PhaseEnum.PerformAction);
            //                }
            //            }
            //        }
            //        break;

            List<Summon> summons = new List<Summon>();
            foreach (CombatActor act in CombatManager.Party)
            {
                if (act.LinkedSummon != null)
                {
                    summons.Add(act.LinkedSummon);
                }
            }
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            _guiTextWindow?.ProcessLeftButtonClick(mouse);

            if (CombatManager.AreWeSelectingAnAction())
            {
                if (_gActionSelect != null)
                {
                    rv = _gActionSelect.ProcessLeftButtonClick(mouse);
                }
            }
            else if (CombatManager.CombatPhaseCheck(CombatManager.CmbtPhaseEnum.ChooseMoveTarget))
            {
                CombatManager.SetMoveTarget();
            }
            else if (CombatManager.CombatPhaseCheck(CombatManager.CmbtPhaseEnum.ChooseActionTarget))
            {
                CombatManager.SelectedAction.UseSkillOnTarget();
            }

            //If the current Phase is skill selection, allow the user to pick a skill for the currentCharacter
            //switch (CombatManager.CurrentPhase)
            //{  
            //    case CombatManager.PhaseEnum.DisplayVictory:
            //        rv = _gPostScreen.ProcessLeftButtonClick(mouse);
            //        break;
            //    case CombatManager.PhaseEnum.Defeat:
            //        GUIManager.BeginFadeOut(true);
            //        BackToMain();
            //        MapManager.CurrentMap = MapManager.Maps["mapHospital"];
            //        PlayerManager.CurrentMap = "mapHospital";
            //        PlayerManager.World.Position = Util.SnapToGrid(MapManager.CurrentMap.DictionaryCharacterLayer["playerSpawn"]);
            //        GUIManager.OpenTextWindow(ObjectManager.DiNPC[7].GetDialogEntry("Healed"), ObjectManager.DiNPC[7]);

            //        foreach (ClassedCombatant c in PlayerManager.GetParty())
            //        {
            //            c.ClearConditions();
            //            c.ModifyHealth((int)(c.MaxHP * 0.10), false);
            //        }

            //        break;
            //}

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

        public void CancelAction()
        {
            _gActionSelect.CancelAction();
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;

            RHTile hoverTile = MapManager.CurrentMap.GetTileByPixelPosition(GraphicCursor.GetWorldMousePosition());
            if (hoverTile?.Character != null)
            {
                if(_gActorInfoPanel?.Character != hoverTile.Character)
                {
                    _gActorInfoPanel = new GUIActorInfoPanel(hoverTile.Character);
                    _gActorInfoPanel.AnchorToScreen(SideEnum.BottomRight);
                }
            }
            else
            {
                _gActorInfoPanel = null;
            }

            if (!rv && _gActionSelect != null) {
                rv = _gActionSelect.ProcessHover(mouse);
            }

            return rv;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            _gActorInfoPanel?.Draw(spriteBatch);
        }

        /// <summary>
        /// Draws the Combat objects that exist in WorldSpace
        /// </summary>
        /// <param name="spriteBatch"></param>
        public void DrawUpperCombatLayer(SpriteBatch spriteBatch)
        {
            CombatManager.SelectedAction?.Draw(spriteBatch);
            foreach (FloatingText text in _liFloatingText)
            {
                text.Draw(spriteBatch);
            }
        }

        #region CombatManager Controls
        public void OpenMainSelection()
        {
            if(_gActionSelect == null)
            {
                _gActionSelect = new ActionSelectObject();
                AddControl(_gActionSelect);
            }
        }
        public void CloseMainSelection()
        {
            if (_gActionSelect != null)
            {
                RemoveControl(_gActionSelect);
                _gActionSelect = null;
            }
        }
        #endregion

        #region FloatingText Handling
        /// <summary>
        /// Returns true if there are any Floating Text objects.
        /// </summary>
        /// <returns></returns>
        public bool AreThereFloatingText() { return _liFloatingText.Count > 0; }

        /// <summary>
        /// Adds the FloatingText object to the removal queue
        /// </summary>
        public void RemoveFloatingText(FloatingText fText)
        {
            _diFTextQueue[fText] = false;
        }

        /// <summary>
        /// Adds the FloatingText object to the queue to add
        /// </summary>
        public void AddFloatingText(FloatingText fText)
        {
            _diFTextQueue[fText] = true;
        }

        #endregion
    }

    public class FloatingText
    {
        Vector2 Position;
        protected BitmapFont _font;
        protected Color _cTextColor;
        const double VANISH_AFTER_DEF = 1.0;
        double _dVanishAfter;
        double _dCountDown = 0;
        protected string _sText;

        public FloatingText(Vector2 position, int spriteWidth, string text, Color c)
        {
            RHRandom rand = RHRandom.Instance;

            _font = DataManager.GetBitMapFont(@"Fonts\FontBattle");
            _sText = text;
            _cTextColor = c;

            Position = position;
            _dVanishAfter = VANISH_AFTER_DEF - (0.05 * (double)rand.Next(0, 5));

            Position.X += spriteWidth / 2; //get the center
            Position.X += rand.Next(-8, 8); //displace the x position
            Position.Y -= rand.Next(0, TileSize / 2); //Subtract this number to go 'up' the screen
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(_font, _sText, Position, _cTextColor, Position.Y, null);
        }

        public void Update(GameTime gTime)
        {
            Position += new Vector2(0, -0.5f);
            _dCountDown += gTime.ElapsedGameTime.TotalSeconds;
            if (_dCountDown >= _dVanishAfter)
            {
                CombatManager.RemoveFloatingText(this);
            }
        }
    }

    /// <summary>
    /// This represents an info panel on the CombatScreen that will display info on characters
    /// </summary>
    public class GUIActorInfoPanel : GUIWindow
    {
        GUISprite _gSprite;
        CombatActor _actor;
        public CombatActor Character => _actor;

        public GUIActorInfoPanel(CombatActor actor) : base()
        {
            _actor = actor;
            _gSprite = new GUISprite(actor.BodySprite, true);
            _gSprite.SetScale(Scale);
            _gSprite.AnchorToInnerSide(this, SideEnum.TopLeft);
            _gSprite.PlayAnimation(VerbEnum.Walk, DirectionEnum.Down);
            AddControl(_gSprite);

            GUIText gName = new GUIText("XXXXXXXXXXXXXXX");
            gName.AnchorAndAlignToObject(_gSprite, SideEnum.Right, SideEnum.Top);
            AddControl(gName);

            GUIText gHP = new GUIText(string.Format("HP: {0}/{1}", actor.CurrentHP, actor.MaxHP));
            gHP.AnchorAndAlignToObject(gName, SideEnum.Bottom, SideEnum.Left);
            AddControl(gHP);

            if (actor.MaxMP > 0)
            {
                GUIText gMP = new GUIText(string.Format("MP: {0}/{1}", actor.CurrentMP, actor.MaxMP));
                gMP.AnchorAndAlignToObject(gHP, SideEnum.Bottom, SideEnum.Left);
                AddControl(gMP);
            }

            Width = TileSize * 10;
            Height = _gSprite.Height;
            Resize();
            gName.SetText(actor.Name);    
        }

//SetWeapon();
            //_gHP = new GUIStatDisplay(actor.GetHP, Color.Green, 100);
            //_gHP.AnchorAndAlignToObject(_gSprite, SideEnum.Bottom, SideEnum.Left);
            //AddControl(_gHP);
            //if (actor.MaxMP > 0) { 
            //    _gMP = new GUIStatDisplay(actor.GetMP, Color.LightBlue, 100);
            //    _gMP.AnchorAndAlignToObject(_gHP, SideEnum.Bottom, SideEnum.Left);
            //    AddControl(_gMP);
            //}

        //public void SetWeapon()
        //{
        //    if (_actor.IsAdventurer())
        //    {
        //        ClassedCombatant adv = (ClassedCombatant)_actor;
        //        CharacterClass cClass = adv.CharacterClass;

        //        AnimatedSprite sprWeaponSprite = new AnimatedSprite(GameContentManager.FOLDER_ITEMS + "Combat\\Weapons\\" + cClass.WeaponType.ToString() + "\\" + adv.Weapon.GetItem().ItemID);

        //        int xCrawl = 0;
        //        int frameWidth = 32;
        //        int frameHeight = 32;
        //        sprWeaponSprite.AddAnimation(CActorAnimEnum.Idle, xCrawl, 0, 32, 32, cClass.IdleFrames, cClass.IdleFramesLength);
        //        xCrawl += cClass.IdleFrames;
        //        sprWeaponSprite.AddAnimation(CActorAnimEnum.Cast, (xCrawl * frameWidth), 0, frameWidth, frameHeight, cClass.CastFrames, cClass.CastFramesLength);
        //        xCrawl += cClass.CastFrames;
        //        sprWeaponSprite.AddAnimation(CActorAnimEnum.Hurt, (xCrawl * frameWidth), 0, frameWidth, frameHeight, cClass.HitFrames, cClass.HitFramesLength);
        //        xCrawl += cClass.HitFrames;
        //        sprWeaponSprite.AddAnimation(CActorAnimEnum.Attack, (xCrawl * frameWidth), 0, frameWidth, frameHeight, cClass.AttackFrames, cClass.AttackFramesLength);
        //        xCrawl += cClass.AttackFrames;
        //        sprWeaponSprite.AddAnimation(CActorAnimEnum.Critical, (xCrawl * frameWidth), 0, frameWidth, frameHeight, cClass.CriticalFrames, cClass.CriticalFramesLength);
        //        xCrawl += cClass.CriticalFrames;
        //        sprWeaponSprite.AddAnimation(CActorAnimEnum.KO, (xCrawl * frameWidth), 0, frameWidth, frameHeight, cClass.KOFrames, cClass.KOFramesLength);
        //        xCrawl += cClass.KOFrames;
        //        sprWeaponSprite.AddAnimation(CActorAnimEnum.Win, (xCrawl * frameWidth), 0, frameWidth, frameHeight, cClass.WinFrames, cClass.WinFramesLength);
        //        sprWeaponSprite.SetScale(CombatManager.CombatScale);

        //        _gSprite.SetWeapon(sprWeaponSprite);
        //    }
        //}

        public void Reset()
        {
            _gSprite.Reset();
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

            SyncText();

            Width = _gActionBar.Width;
            Height = _gActionBar.Height + _gText.Height;

            AnchorToScreen(SideEnum.Bottom);
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

            if (CombatManager.AreWeSelectingAnAction() && _gActionBar.ProcessHover(mouse))
            {
                rv = true;
                SyncText();
            }

            return rv;
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
                if (selectedAction.IsSpell() && ((CombatAction)selectedAction).MPCost > 0)
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

        /// <summary>
        /// Tell the ActionBar to Cancel whatever selection it had
        /// Backtrack the CurrentPhase to the appropriate step
        /// Clear any selected or highlited RHTiles
        /// </summary>
        public void CancelAction()
        {
            _gActionBar.CancelAction();
            CombatManager.ChangePhase(CombatManager.CmbtPhaseEnum.MainSelection);
            CombatManager.ClearAllTiles();
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

                _actionMenu = null;
                _gSelectedMenu = null;
                _gSelectedAction = null;

                _actor = CombatManager.ActiveCharacter; ;
                _liActionButtons.Clear();

                if (_actor != null)
                {
                    foreach (MenuAction ca in _actor.AbilityList)
                    {
                        ActionButton ab = new ActionButton(ca);
                        MenuAction action = ab.Action;
                        _liActionButtons.Add(ab);

                        if (CombatManager.CurrentTurnInfo.HasActed && !action.IsMove() && !action.IsEndTurn())
                        {
                            ab.Enable(false);
                        }
                        else if (CombatManager.CurrentTurnInfo.HasMoved && action.IsMove())
                        {
                            ab.Enable(false);
                        }
                        else if (action.IsSpellMenu() && CombatManager.ActiveCharacter.Silenced())
                        {
                            ab.Enable(false);
                        }
                        else if (action.IsUseItem() && InventoryManager.GetPlayerCombatItems().Count == 0)
                        {
                            ab.Enable(false);
                        }

                    }

                    _gSelectedAction = _liActionButtons[0];

                    Width = _liActionButtons.Count * _liActionButtons[0].Width;
                    Height = _liActionButtons[0].Height;
                }
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
                            CombatManager.ProcessActionChoice(new CombatAction((Consumable)_actionMenu.SelectedAction.Item));
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
                        if (ab.Enabled)
                        {
                            if (a.IsEndTurn())
                            {
                                if (CombatManager.ActiveCharacter.CurrentCharge == 100)
                                {
                                    CombatManager.ActiveCharacter.CurrentCharge -= 30;
                                }
                                CombatManager.EndTurn();
                            }
                            else if (a.IsMove())
                            {
                                CombatManager.ChangePhase(CombatManager.CmbtPhaseEnum.ChooseMoveTarget);
                                CombatManager.FindAndHighlightLegalTiles();
                            }
                            else if (a.IsAction() || a.IsSpell())
                            {
                                if (CombatManager.ActiveCharacter.CanCast(((CombatAction)a).MPCost))
                                {
                                    CombatManager.ProcessActionChoice((CombatAction)a);
                                }
                            }
                            else
                            {
                                if (a.IsSpellMenu())
                                {
                                    if (CombatManager.ActiveCharacter.GetCurrentSpecials().Count > 0)
                                    {
                                        _gSelectedMenu = ab;
                                        _actionMenu = new ActionMenu(CombatManager.ActiveCharacter.GetCurrentSpecials());
                                        _actionMenu.AnchorAndAlignToObject(this, SideEnum.Top, SideEnum.CenterX, 10);
                                        CombatManager.ChangePhase(CombatManager.CmbtPhaseEnum.ChooseAction);
                                    }
                                }
                                else if (a.IsUseItem())
                                {
                                    _gSelectedMenu = ab;
                                    _actionMenu = new ActionMenu(InventoryManager.GetPlayerCombatItems());
                                    _actionMenu.AnchorAndAlignToObject(this, SideEnum.Top, SideEnum.CenterX, 10);
                                    CombatManager.ChangePhase(CombatManager.CmbtPhaseEnum.ChooseAction);
                                }
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
                    if (CombatManager.CurrentPhase == CombatManager.CmbtPhaseEnum.ChooseMoveTarget)
                    {
                        _gSelectedAction = (_gSelectedMenu != null) ? _gSelectedMenu : _liActionButtons[0];
                    }
                    else if( CombatManager.CurrentPhase == CombatManager.CmbtPhaseEnum.ChooseActionTarget)
                    {
                        _actionMenu = null;
                        _gSelectedAction = (_gSelectedMenu != null) ? _gSelectedMenu : _liActionButtons[0];
                    }
                    else if (CombatManager.CurrentPhase == CombatManager.CmbtPhaseEnum.ChooseAction)
                    {
                        _actionMenu = null;
                        _gSelectedAction = _liActionButtons[0];
                    }

                    ProcessHover(GraphicCursor.Position.ToPoint());
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

                public ActionMenu(List<CombatAction> specialsList)
                {
                    _display = DisplayEnum.Spells;
                    _liActions = new List<ActionButton>();

                    for (int i = 0; i < specialsList.Count; i++)
                    {
                        _liActions.Add(new ActionButton(specialsList[i]));
                        if (specialsList[i].MPCost > CombatManager.ActiveCharacter.CurrentMP)
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
                            rv = _liActions[i].Contains(mouse) && _liActions[i].Enabled;
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
                MenuAction _action;
                public MenuAction Action => _action;

                Item _item;
                public Item Item => _item;

                GUIImage _gItem;

                public ActionButton(MenuAction action) : base(new Rectangle((int)action.IconGrid.X * TileSize, (int)action.IconGrid.Y * TileSize, TileSize, TileSize), TileSize, TileSize, @"Textures\texCmbtActions")
                {
                    _action = action;
                    SetScale(GameManager.Scale);
                }

                public ActionButton(Item i) : base(new Rectangle(288, 32, 32, 32), 16, 16, @"Textures\Dialog")
                {
                    _item = i;
                    _gItem = new GUIImage(_item.SourceRectangle, Width, Height, _item.Texture);
                    SetScale(GameManager.Scale);
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
                    SetScale(GameManager.Scale + 1);

                    int diffWidth = Width - firstWidth;
                    int diffHeight = Height - firstHeight;

                    MoveBy(new Vector2(-diffWidth / 2, -diffHeight / 2));
                }
                public void Unselect()
                {
                    int firstWidth = Width;
                    int firstHeight = Height;
                    SetScale(GameManager.Scale);

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
                _arrBarDisplay[i].SetScale(GameManager.Scale);
            }

            Width = MAX_SHOWN * _arrTurnDisplay[0].Width;
            Height = (2 * _arrTurnDisplay[0].Height) + _arrBarDisplay[0].Height;

            Position(Position());
        }
        
        public override void Update(GameTime gTime)
        {
            if (_bUpdate)
            {
                _dTimer -= gTime.ElapsedGameTime.TotalSeconds;

                if (_dTimer <= 0)
                {
                    _arrTurnDisplay[_iCurrUpdate].Update(gTime);
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

        public override bool ProcessHover(Point mouse)
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
                _gImage.SetScale(GameManager.Scale);

                _arrBarDisplay = barDisplay;
                Width = _gImage.Width;
                Height = _gImage.Height;
            }

            public override void Update(GameTime gTime)
            {
                if (_bFadeOut)
                {
                    UpdateFadeOut(gTime);
                }
                else if (_bFadeIn)
                {
                    UpdateFadeIn(gTime);
                }
                else if (_vMoveTo != Vector2.Zero)
                {
                    UpdateMove(gTime);
                }
            }
            private void UpdateFadeOut(GameTime gTime)
            {
                if (Alpha() > 0)
                {
                    Alpha(Alpha() - _fFadeSpeed);
                }
                else
                {
                    _bFadeOut = false;
                    Finished = true;
                }
            }
            private void UpdateFadeIn(GameTime gTime)
            {
                if (Alpha() < 1)
                {
                    Alpha(Alpha() + _fFadeSpeed);
                }
                else
                {
                    _bFadeIn = false;
                    Finished = true;
                }
            }
            private void UpdateMove(GameTime gTime)
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

            public override void Alpha(float alpha) {
                base.Alpha(alpha);
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
                    Alpha(0);
                    AnchorAndAlignToObject(_arrBarDisplay[_iIndex], IsInParty() ? SideEnum.Top : SideEnum.Bottom, SideEnum.CenterX);
                }
            }
        }
    }
}
