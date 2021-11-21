using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters;
using RiverHollow.CombatStuff;
using RiverHollow.Game_Managers;
using RiverHollow.WorldObjects;
using System;
using System.Collections.Generic;

using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.GUIComponents.GUIObjects
{
    public class GUIActionSelectObject : GUIObject
    {
        GUIText _gText;
        ActionBar _gActionBar;

        public GUIActionSelectObject()
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

            if (TacticalCombatManager.AreWeSelectingAnAction() && _gActionBar.ProcessHover(mouse))
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
            TacticalMenuAction selectedAction = _gActionBar.SelectedAction;
            Item selectedItem = _gActionBar.SelectedItem;

            string actionName = string.Empty;

            if (selectedAction != null)
            {
                actionName = selectedAction.Name;
                if (selectedAction.IsSpell() && ((TacticalCombatAction)selectedAction).MPCost > 0)
                {
                    actionName = actionName + "  " + ((TacticalCombatAction)selectedAction).MPCost.ToString() + " MP";
                }
            }
            else if (selectedItem != null)
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
            TacticalCombatManager.ChangePhase(TacticalCombatManager.CmbtPhaseEnum.MainSelection);
            TacticalCombatManager.ClearAllTiles();
            SyncText();
        }

        public class ActionBar : GUIObject
        {
            ActionMenu _actionMenu;
            TacticalCombatActor _actor;
            ActionButton _gSelectedAction;
            ActionButton _gSelectedMenu;
            List<ActionButton> _liActionButtons;

            public TacticalMenuAction SelectedAction => _gSelectedAction != null ? _gSelectedAction.Action : null;
            public Item SelectedItem => (_actionMenu != null && _actionMenu.SelectedAction.Item != null) ? _actionMenu.SelectedAction.Item : null;

            public ActionBar()
            {
                _liActionButtons = new List<ActionButton>();

                _actionMenu = null;
                _gSelectedMenu = null;
                _gSelectedAction = null;

                _actor = TacticalCombatManager.ActiveCharacter; ;
                _liActionButtons.Clear();

                if (_actor != null)
                {
                    foreach (TacticalMenuAction ca in _actor.TacticalAbilityList)
                    {
                        ActionButton ab = new ActionButton(ca);
                        TacticalMenuAction action = ab.Action;
                        _liActionButtons.Add(ab);

                        if (TacticalCombatManager.CurrentTurnInfo.HasActed && !action.IsMove() && !action.IsEndTurn())
                        {
                            ab.Enable(false);
                        }
                        else if (TacticalCombatManager.CurrentTurnInfo.HasMoved && action.IsMove())
                        {
                            ab.Enable(false);
                        }
                        else if (action.IsSpellMenu() && TacticalCombatManager.ActiveCharacter.Silenced())
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
                if (Show())
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
                            TacticalMenuAction a = _actionMenu.SelectedAction.Action;
                            if (TacticalCombatManager.ActiveCharacter.CanCast(((TacticalCombatAction)a).MPCost))
                            {
                                TacticalCombatManager.ProcessActionChoice((TacticalCombatAction)a);
                            }
                        }
                        else if (_actionMenu.ShowItems())
                        {
                            TacticalCombatManager.ProcessActionChoice(new TacticalCombatAction((Consumable)_actionMenu.SelectedAction.Item));
                        }
                    }
                }

                for (int i = 0; i < _liActionButtons.Count; i++)
                {
                    ActionButton ab = _liActionButtons[i];
                    if (ab.ProcessLeftButtonClick(mouse))
                    {
                        rv = true;
                        TacticalMenuAction a = ab.Action;
                        if (ab.Enabled)
                        {
                            if (a.IsEndTurn())
                            {
                                if (TacticalCombatManager.ActiveCharacter.CurrentCharge == 100)
                                {
                                    TacticalCombatManager.ActiveCharacter.CurrentCharge -= 50;
                                }
                                TacticalCombatManager.EndTurn();
                            }
                            else if (a.IsMove())
                            {
                                TacticalCombatManager.ChangePhase(TacticalCombatManager.CmbtPhaseEnum.ChooseMoveTarget);
                                TacticalCombatManager.FindAndHighlightLegalTiles();
                            }
                            else if (a.IsAction() || a.IsSpell())
                            {
                                if (TacticalCombatManager.ActiveCharacter.CanCast(((TacticalCombatAction)a).MPCost))
                                {
                                    TacticalCombatManager.ProcessActionChoice((TacticalCombatAction)a);
                                }
                            }
                            else
                            {
                                if (a.IsSpellMenu())
                                {
                                    if (TacticalCombatManager.ActiveCharacter.GetCurrentSpecials().Count > 0)
                                    {
                                        _gSelectedMenu = ab;
                                        _actionMenu = new ActionMenu(TacticalCombatManager.ActiveCharacter.GetCurrentSpecials());
                                        _actionMenu.AnchorAndAlignToObject(this, SideEnum.Top, SideEnum.CenterX, 10);
                                        TacticalCombatManager.ChangePhase(TacticalCombatManager.CmbtPhaseEnum.ChooseAction);
                                    }
                                }
                                else if (a.IsUseItem())
                                {
                                    _gSelectedMenu = ab;
                                    _actionMenu = new ActionMenu(InventoryManager.GetPlayerCombatItems());
                                    _actionMenu.AnchorAndAlignToObject(this, SideEnum.Top, SideEnum.CenterX, 10);
                                    TacticalCombatManager.ChangePhase(TacticalCombatManager.CmbtPhaseEnum.ChooseAction);
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
                    if (TacticalCombatManager.CurrentPhase == TacticalCombatManager.CmbtPhaseEnum.ChooseMoveTarget)
                    {
                        _gSelectedAction = (_gSelectedMenu != null) ? _gSelectedMenu : _liActionButtons[0];
                    }
                    else if (TacticalCombatManager.CurrentPhase == TacticalCombatManager.CmbtPhaseEnum.ChooseActionTarget)
                    {
                        _actionMenu = null;
                        _gSelectedAction = (_gSelectedMenu != null) ? _gSelectedMenu : _liActionButtons[0];
                    }
                    else if (TacticalCombatManager.CurrentPhase == TacticalCombatManager.CmbtPhaseEnum.ChooseAction)
                    {
                        _actionMenu = null;
                        _gSelectedAction = _liActionButtons[0];
                    }

                    ProcessHover(GUICursor.Position.ToPoint());
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

                public ActionMenu(List<TacticalCombatAction> specialsList)
                {
                    _display = DisplayEnum.Spells;
                    _liActions = new List<ActionButton>();

                    for (int i = 0; i < specialsList.Count; i++)
                    {
                        _liActions.Add(new ActionButton(specialsList[i]));
                        if (specialsList[i].MPCost > TacticalCombatManager.ActiveCharacter.CurrentMP)
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
                TacticalMenuAction _action;
                public TacticalMenuAction Action => _action;

                Item _item;
                public Item Item => _item;

                GUIImage _gItem;

                public ActionButton(TacticalMenuAction action) : base(new Rectangle((int)action.IconGrid.X * TILE_SIZE, (int)action.IconGrid.Y * TILE_SIZE, TILE_SIZE, TILE_SIZE), TILE_SIZE, TILE_SIZE, @"Textures\texCmbtActions")
                {
                    _action = action;
                    SetScale(GameManager.CurrentScale);
                }

                public ActionButton(Item i) : base(new Rectangle(288, 32, 32, 32), 16, 16, DataManager.DIALOGUE_TEXTURE)
                {
                    _item = i;
                    _gItem = new GUIImage(_item.SourceRectangle, Width, Height, _item.Texture);
                    SetScale(GameManager.CurrentScale);
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
                    SetScale(GameManager.CurrentScale + 1);

                    int diffWidth = Width - firstWidth;
                    int diffHeight = Height - firstHeight;

                    MoveBy(new Vector2(-diffWidth / 2, -diffHeight / 2));
                }
                public void Unselect()
                {
                    int firstWidth = Width;
                    int firstHeight = Height;
                    SetScale(GameManager.CurrentScale);

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
}
