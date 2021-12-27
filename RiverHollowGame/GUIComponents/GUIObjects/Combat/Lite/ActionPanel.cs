using Microsoft.Xna.Framework;
using RiverHollow.Actors.CombatStuff;
using RiverHollow.CombatStuff;
using RiverHollow.Game_Managers;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.GUIComponents.GUIObjects.Combat.Lite
{
    public class ActionPanel : GUIImage
    {
        int _iSelectedAction = 0;

        GUIText _gActionName;
        GUIImage _gActionReticle;
        GUIImage[] _arrActionImages;

        List<GUIObject> _liActionIcons;

        public ActionPanel() : base(new Rectangle(0, 48, 112, 47), DataManager.COMBAT_TEXTURE)
        {
            _liActionIcons = new List<GUIObject>();
            _arrActionImages = new GUIImage[6];

            AnchorToScreen(SideEnum.Bottom);
            ScaledMoveBy(0, -14);
        }

        public void PopulateActions()
        {
            RemoveControl(_gActionReticle);
            foreach (GUIImage g in _arrActionImages)
            {
                RemoveControl(g);
            }

            _arrActionImages = new GUIImage[6];

            Vector2 iconPosition = new Vector2(4, 5);
            for (int i = 0; i < 4; i++)
            {
                LiteMenuAction action = CombatManager.ActiveCharacter.Actions[i];
                _arrActionImages[i] = new GUIImage(new Rectangle((int)action.IconGrid.X * GameManager.TILE_SIZE, (int)action.IconGrid.Y * GameManager.TILE_SIZE, 16, 16), DataManager.ACTION_ICONS);
                _arrActionImages[i].Position(Position() + ScaleIt(iconPosition));
                AddControl(_arrActionImages[i]);

                iconPosition.X += 22;
            }

            _arrActionImages[4] = new GUIImage(new Rectangle(0, 128, 16, 16), DataManager.ACTION_ICONS);
            _arrActionImages[4].Position(Position() + ScaleIt(new Vector2(92, 5)));
            AddControl(_arrActionImages[4]);

            _arrActionImages[5] = new GUIImage(new Rectangle(16, 128, 16, 16), DataManager.ACTION_ICONS);
            _arrActionImages[5].Position(Position() + ScaleIt(new Vector2(92, 26)));
            AddControl(_arrActionImages[5]);

            _gActionReticle = new GUIImage(new Rectangle(194, 112, 20, 20), DataManager.COMBAT_TEXTURE);
            AddControl(_gActionReticle);

            SetSelectedAction(_iSelectedAction);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            for (int i = 0; i < 6; i++)
            {
                if (_arrActionImages[i].Contains(mouse))
                {
                    rv = true;
                    CombatManager.SetChosenAction(i);
                    break;
                }
            }

            return rv;
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;
            for (int i = 0; i < 6; i++)
            {
                if (_arrActionImages[i].Contains(mouse))
                {
                    rv = true;
                    SetSelectedAction(i);
                    break;
                }
            }

            return rv;
        }

        private void SetSelectedAction(int i)
        {
            RemoveControl(_gActionName);
            foreach (GUIObject obj in _liActionIcons) { RemoveControl(obj); }

            _liActionIcons = new List<GUIObject>();

            _iSelectedAction = i;
            _gActionReticle.CenterOnObject(_arrActionImages[_iSelectedAction]);

            string textName = string.Empty;
            if (_iSelectedAction < 4) { textName = CombatManager.ActiveCharacter.Actions[_iSelectedAction].Name; }
            else if (_iSelectedAction == 4) { textName = "Item"; }
            else { textName = "Move"; }

            _gActionName = new GUIText(textName);
            _gActionName.AnchorAndAlignToObject(this, SideEnum.Top, SideEnum.CenterX);
            AddControl(_gActionName);

            //4 and 5 are Items and Move
            if (_iSelectedAction < 4)
            {
                GUIObject tempObj = null;
                CombatAction selectedAction = CombatManager.ActiveCharacter.Actions[_iSelectedAction];

                //Icon for Harm/Heal
                if (selectedAction.Harm) {
                    if (selectedAction.DamageType == DamageTypeEnum.Physical) { tempObj = DataManager.GetIcon(GameIconEnum.PhysicalDamage); }
                    else { tempObj = DataManager.GetIcon(GameIconEnum.MagicDamage); }
                }
                else if (selectedAction.Heal) { tempObj = DataManager.GetIcon(GameIconEnum.Heal); }

                tempObj.Position(Position() + ScaleIt(new Vector2(7, 29)));
                _liActionIcons.Add(tempObj);

                //Damage Prediction
                if (selectedAction.Potency > 0)
                {
                    CombatManager.ActiveCharacter.GetDamageRange(out int min, out int max, selectedAction);

                    tempObj = new GUIText(string.Format("{0}-{1}", min, max), DataManager.GetBitMapFont(DataManager.FONT_STAT_DISPLAY));
                    tempObj.AnchorAndAlignToObject(_liActionIcons[_liActionIcons.Count - 1], SideEnum.Right, SideEnum.CenterY);
                    tempObj.ScaledMoveBy(2, 0);
                    _liActionIcons.Add(tempObj);
                }


                //Icon for Range
                if (selectedAction.Range == RangeEnum.Melee) { tempObj = DataManager.GetIcon(GameIconEnum.Melee); }
                else if (selectedAction.Range == RangeEnum.Ranged) { tempObj = DataManager.GetIcon(GameIconEnum.Ranged); }

                tempObj.AnchorAndAlignToObject(_liActionIcons[_liActionIcons.Count-1], SideEnum.Right, SideEnum.CenterY);
                tempObj.ScaledMoveBy(2, 0);
                _liActionIcons.Add(tempObj);

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

                tempObj.AnchorAndAlignToObject(_liActionIcons[_liActionIcons.Count - 1], SideEnum.Right, SideEnum.CenterY);
                tempObj.ScaledMoveBy(2, 0);
                _liActionIcons.Add(tempObj);

                //Icon for User Movement
                if (selectedAction.UserMovement != MovementTypeEnum.None)
                {
                    if (selectedAction.UserMovement == MovementTypeEnum.Forward) { tempObj = DataManager.GetIcon(GameIconEnum.MoveRight); }
                    else if (selectedAction.UserMovement == MovementTypeEnum.Backward) { tempObj = DataManager.GetIcon(GameIconEnum.MoveLeft); }
                    tempObj.SetColor(Color.Green);

                    tempObj.AnchorAndAlignToObject(_liActionIcons[_liActionIcons.Count - 1], SideEnum.Right, SideEnum.CenterY);
                    tempObj.ScaledMoveBy(2, 0);
                    _liActionIcons.Add(tempObj);
                }

                //Icon for Target Movement
                if (selectedAction.TargetMovement != MovementTypeEnum.None)
                {
                    if (selectedAction.TargetMovement == MovementTypeEnum.Forward) { tempObj = DataManager.GetIcon(GameIconEnum.MoveLeft); }
                    else if (selectedAction.TargetMovement == MovementTypeEnum.Backward) { tempObj = DataManager.GetIcon(GameIconEnum.MoveRight); }
                    tempObj.SetColor(Color.Red);

                    tempObj.AnchorAndAlignToObject(_liActionIcons[_liActionIcons.Count - 1], SideEnum.Right, SideEnum.CenterY);
                    tempObj.ScaledMoveBy(2, 0);
                    _liActionIcons.Add(tempObj);
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
    }
}
