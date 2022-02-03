using Microsoft.Xna.Framework;
using RiverHollow.Characters;
using RiverHollow.CombatStuff;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Items;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.GUIObjects.Combat.Lite
{
    public class ActionPanel : GUIImage
    {
        const int USE_ITEM = 4;
        int _iSelectedAction = 0;

        GUIText _gActionName;
        GUIImage _gActionReticle;
        GUIImage[] _arrActionImages;
        CombatInventoryPanel _gInventoryPanel;

        List<GUIObject> _liActionIcons;
        CombatActor _hoverTarget;

        public ActionPanel() : base(new Rectangle(0, 48, 112, 47), DataManager.COMBAT_TEXTURE)
        {
            _liActionIcons = new List<GUIObject>();
            _arrActionImages = new GUIImage[6];

            AnchorToScreen(SideEnum.Bottom);
            ScaledMoveBy(0, -14);
        }

        public void PopulateActions()
        {
            RemoveControl(_gInventoryPanel);
            _gInventoryPanel = null;

            RemoveControl(_gActionReticle);
            foreach (GUIImage g in _arrActionImages)
            {
                RemoveControl(g);
            }

            _arrActionImages = new GUIImage[6];

            Vector2 iconPosition = new Vector2(4, 5);
            for (int i = 0; i < 6; i++)
            {
                if(i == 5){ iconPosition.Y += 21; }
                CombatAction action = CombatManager.ActiveCharacter.Actions[i];
                _arrActionImages[i] = new GUIImage(new Rectangle((int)action.IconGrid.X * GameManager.TILE_SIZE, (int)action.IconGrid.Y * GameManager.TILE_SIZE, 16, 16), DataManager.ACTION_ICONS);
                _arrActionImages[i].Position(Position() + ScaleIt(iconPosition));
                AddControl(_arrActionImages[i]);

                if (i < USE_ITEM) { iconPosition.X += 22; }
            }

            _gActionReticle = new GUIImage(new Rectangle(194, 112, 20, 20), DataManager.COMBAT_TEXTURE);
            AddControl(_gActionReticle);

            SetSelectedActionInfo(_iSelectedAction);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (CombatManager.CurrentPhase == CombatManager.PhaseEnum.ChooseAction)
            {
                if (_gInventoryPanel == null)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        if (_arrActionImages[i].Contains(mouse))
                        {
                            _iSelectedAction = i;
                            if (_iSelectedAction == USE_ITEM)
                            {
                                _gActionName.SetText("");
                                _gInventoryPanel = new CombatInventoryPanel();
                                _gInventoryPanel.Position(Position());
                                AddControl(_gInventoryPanel);
                            }
                            else { CombatManager.SetChosenAction(_iSelectedAction); }
                            rv = true;
                            break;
                        }
                    }
                }
                else
                {
                    CombatManager.SetChosenItem(_gInventoryPanel.GetSelectedItem());
                }
            }

            return rv;
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;
            if (_gInventoryPanel == null)
            {
                if (CombatManager.CurrentPhase == CombatManager.PhaseEnum.ChooseAction)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        if (_arrActionImages[i].Contains(mouse))
                        {
                            rv = true;
                            SetSelectedActionInfo(i);
                            break;
                        }
                    }
                }
            }
            else
            {
                rv = _gInventoryPanel.ProcessHover(mouse);

                Consumable it = _gInventoryPanel.GetSelectedItem();
                if (it != null)
                {
                    SetSelectedName(it.Name);
                }
            }

            return rv;
        }

        private void SetSelectedName(string name)
        {
            RemoveControl(_gActionName);
            _gActionName = new GUIText(name);
            _gActionName.AnchorAndAlignToObject(this, SideEnum.Top, SideEnum.CenterX);
            AddControl(_gActionName);
        }

        private void SetSelectedActionInfo(int i)
        {
            foreach (GUIObject obj in _liActionIcons) { RemoveControl(obj); }

            _liActionIcons = new List<GUIObject>();

            _iSelectedAction = i;
            _gActionReticle.CenterOnObject(_arrActionImages[_iSelectedAction]);

            SetSelectedName(CombatManager.ActiveCharacter.Actions[_iSelectedAction].Name);

            if (_iSelectedAction < USE_ITEM)
            {
                GUIObject tempObj = null;
                CombatAction selectedAction = CombatManager.ActiveCharacter.Actions[_iSelectedAction];

                //Icon for Harm/Heal
                if (selectedAction.Harm) {
                    if (selectedAction.DamageType == DamageTypeEnum.Physical) { tempObj = DataManager.GetIcon(GameIconEnum.PhysicalDamage); }
                    else { tempObj = DataManager.GetIcon(GameIconEnum.MagicDamage); }
                }
                else if (selectedAction.Heal) { tempObj = DataManager.GetIcon(GameIconEnum.Heal); }

                PositionIcon(tempObj);

                //Damage Prediction
                if (selectedAction.Potency > 0)
                {
                    int min, max;
                    if (_hoverTarget == null) { CombatManager.ActiveCharacter.GetRawDamageRange(out min, out max, selectedAction.PowerAttribute, selectedAction.Potency); }
                    else { _hoverTarget.GetActualDamageRange(out min, out max, CombatManager.ActiveCharacter, selectedAction.PowerAttribute, selectedAction.Potency, selectedAction.Element); }

                    tempObj = new GUIText(string.Format("{0}-{1}", min, max), DataManager.GetBitMapFont(DataManager.FONT_STAT_DISPLAY));
                    PositionIcon(tempObj);
                    _liActionIcons.Add(tempObj);
                }

                //Icon for Range
                if (selectedAction.AreaType != AreaTypeEnum.All)
                {
                    if (selectedAction.Range == RangeEnum.Melee) { tempObj = DataManager.GetIcon(GameIconEnum.Melee); }
                    else if (selectedAction.Range == RangeEnum.Ranged) { tempObj = DataManager.GetIcon(GameIconEnum.Ranged); }
                }

                PositionIcon(tempObj);

                //Icon for Area of Effect
                switch (selectedAction.AreaType)
                {
                    case AreaTypeEnum.All:
                        tempObj = DataManager.GetIcon(GameIconEnum.AreaAll);
                        break;
                    case AreaTypeEnum.Column:
                        if (selectedAction.IsHelpful()) { tempObj = DataManager.GetIcon(GameIconEnum.AreaColumnAlly); }
                        else { tempObj = DataManager.GetIcon(GameIconEnum.AreaColumnEnemy); }
                        break;
                    case AreaTypeEnum.Row:
                        tempObj = DataManager.GetIcon(GameIconEnum.AreaRow);
                        break;
                    case AreaTypeEnum.Self:
                        tempObj = DataManager.GetIcon(GameIconEnum.AreaSelf);
                        break;
                    case AreaTypeEnum.Single:
                        tempObj = DataManager.GetIcon(GameIconEnum.AreaSingle);
                        break;
                    case AreaTypeEnum.Square:
                        tempObj = DataManager.GetIcon(GameIconEnum.AreaSquare);
                        break;
                }

                if (selectedAction.IsHelpful()) { tempObj.SetColor(Color.Green); }
                else { tempObj.SetColor(Color.Red); }

                PositionIcon(tempObj);

                //Icon for User Movement
                if (selectedAction.UserMovement != DirectionEnum.None)
                {
                    if (selectedAction.UserMovement == DirectionEnum.Right) { tempObj = DataManager.GetIcon(GameIconEnum.MoveRight); }
                    else if (selectedAction.UserMovement == DirectionEnum.Left) { tempObj = DataManager.GetIcon(GameIconEnum.MoveLeft); }
                    else if (selectedAction.UserMovement == DirectionEnum.Up) { tempObj = DataManager.GetIcon(GameIconEnum.MoveUp); }
                    else if (selectedAction.UserMovement == DirectionEnum.Down) { tempObj = DataManager.GetIcon(GameIconEnum.MoveDown); }
                    tempObj.SetColor(Color.Green);

                    PositionIcon(tempObj);
                }

                //Icon for Target Movement
                if (selectedAction.TargetMovement != DirectionEnum.None)
                {
                    if (selectedAction.TargetMovement == DirectionEnum.Left) { tempObj = DataManager.GetIcon(GameIconEnum.MoveLeft); }
                    else if (selectedAction.TargetMovement == DirectionEnum.Right) { tempObj = DataManager.GetIcon(GameIconEnum.MoveRight); }
                    else if (selectedAction.TargetMovement == DirectionEnum.Up) { tempObj = DataManager.GetIcon(GameIconEnum.MoveUp); }
                    else if (selectedAction.TargetMovement == DirectionEnum.Down) { tempObj = DataManager.GetIcon(GameIconEnum.MoveDown); }
                    tempObj.SetColor(Color.Red);

                    PositionIcon(tempObj);
                }
                
                //Display Status Effects
                if(selectedAction.StatusEffect != null)
                {
                    if (selectedAction.StatusEffect.EffectType == StatusTypeEnum.Buff || selectedAction.StatusEffect.EffectType == StatusTypeEnum.Debuff)
                    {
                        foreach (KeyValuePair<AttributeEnum, string> kvp in selectedAction.StatusEffect.AffectedAttributes)
                        {
                            tempObj = new GUIEffectDetailIcon(kvp.Key, selectedAction.StatusEffect.EffectType);
                            PositionIcon(tempObj);
                        }
                        tempObj = new GUIText(selectedAction.StatusEffect.Duration.ToString(), DataManager.GetBitMapFont(DataManager.FONT_STAT_DISPLAY));
                        PositionIcon(tempObj);

                        tempObj = DataManager.GetIcon(GameIconEnum.Timer);
                        PositionIcon(tempObj, 1);
                    }
                    else if (selectedAction.StatusEffect.EffectType == StatusTypeEnum.DoT)
                    {
                        tempObj = DataManager.GetIcon(GameIconEnum.MagicDamage);
                        PositionIcon(tempObj);

                        tempObj = new GUIText(selectedAction.StatusEffect.Duration.ToString(), DataManager.GetBitMapFont(DataManager.FONT_STAT_DISPLAY));
                        PositionIcon(tempObj);

                        tempObj = DataManager.GetIcon(GameIconEnum.Timer);
                        PositionIcon(tempObj, 1);
                    }
                }

                //Icon for Elemental Alignment
                if (selectedAction.Element != ElementEnum.None) {
                    switch (selectedAction.Element)
                    {
                        case ElementEnum.Fire:
                            tempObj = DataManager.GetIcon(GameIconEnum.ElementFire);
                            break;
                        case ElementEnum.Ice:
                            tempObj = DataManager.GetIcon(GameIconEnum.ElementIce);
                            break;
                        case ElementEnum.Lightning:
                            tempObj = DataManager.GetIcon(GameIconEnum.ElementLightning);
                            break;
                    }
                    tempObj.AnchorAndAlignToObject(_gActionName, SideEnum.Right, SideEnum.CenterY);
                    tempObj.ScaledMoveBy(2, 0);
                    _liActionIcons.Add(tempObj);
                }

                foreach (GUIObject obj in _liActionIcons) { AddControl(obj); }
            }
        }

        private void PositionIcon(GUIObject obj, int space = 2)
        {
            if (obj != null)
            {
                if (_liActionIcons.Count == 0)
                {
                    obj.Position(Position() + ScaleIt(new Vector2(7, 29)));
                }
                else
                {
                    obj.AnchorAndAlignToObject(_liActionIcons[_liActionIcons.Count - 1], SideEnum.Right, SideEnum.CenterY);
                    obj.ScaledMoveBy(space, 0);
                }
                _liActionIcons.Add(obj);
            }
        }

        public void DamageUpdate(CombatActor newTarget)
        {
            _hoverTarget = newTarget;
            SetSelectedActionInfo(_iSelectedAction);
        }

        public void CancelAction()
        {
            ClearHoverTarget();
            if (_gInventoryPanel != null)
            {
                RemoveControl(_gInventoryPanel);
                _gInventoryPanel = null;

                SetSelectedActionInfo(_iSelectedAction);
            }
        }

        public void ClearHoverTarget()
        {
            _hoverTarget = null;
        }

        private class CombatInventoryPanel : GUIObject
        {
            int _iSelectedItem = 0;

            GUIImage _gPanel;
            GUIImage _gActionReticle;
            List<Consumable> _liItems;
            List<GUIImage> _liImages;
            List<GUIText> _liNumbers;

            public CombatInventoryPanel()
            {
                _gPanel = new GUIImage(new Rectangle(0, 96, 112, 56), DataManager.COMBAT_TEXTURE);
                AddControl(_gPanel);

                _liItems = InventoryManager.GetConsumables();
                _liImages = new List<GUIImage>();
                _liNumbers = new List<GUIText>();
                for (int i = 0; i < _liItems.Count; i++)
                {
                    _liImages.Add(new GUIImage(_liItems[i].SourceRectangle, DataManager.FOLDER_ITEMS + "Consumables"));
                    _liNumbers.Add(new GUIText(_liItems[i].Number, true, DataManager.FONT_STAT_DISPLAY));
                }

                Vector2 pos = new Vector2(3, 18);
                for (int i = 0; i < 12 && i <_liImages.Count; i++)
                {
                    AddControl(_liImages[i]);
                    AddControl(_liNumbers[i]);
                    _liImages[i].Position(Position() + ScaleIt(pos));
                    _liNumbers[i].AlignToObject(_liImages[i], SideEnum.Bottom);
                    _liNumbers[i].AlignToObject(_liImages[i], SideEnum.Right);
                    _liNumbers[i].ScaledMoveBy(-1, -1);

                    pos.X += 18;

                    if ((i + 1) % 6 == 0)
                    {
                        pos.X = 0;
                        pos.Y += 18;
                    }
                }

                _gActionReticle = new GUIImage(new Rectangle(194, 112, 20, 20), DataManager.COMBAT_TEXTURE);
                _gActionReticle.CenterOnObject(_liImages[_iSelectedItem]);
                AddControl(_gActionReticle);

                Width = _gPanel.Width;
                Height = _gPanel.Height;
            }

            public override bool ProcessHover(Point mouse)
            {
                bool rv = false;
                for (int i = 0; i < 12 && i < _liImages.Count; i++)
                {
                    if (_liImages[i].Contains(mouse))
                    {
                        rv = true;
                        _iSelectedItem = i;
                        _gActionReticle.CenterOnObject(_liImages[_iSelectedItem]);
                        break;
                    }
                }

                return rv;
            }

            public Consumable GetSelectedItem()
            {
                return _liItems[_iSelectedItem];
            }
        }
    }
}
