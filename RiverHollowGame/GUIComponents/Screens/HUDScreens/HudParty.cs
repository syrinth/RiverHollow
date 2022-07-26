using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters.Lite;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Items;
using System.Collections.Generic;
using static RiverHollow.GUIComponents.GUIObjects.GUIItemBox;
using static RiverHollow.Utilities.Enums;
using static RiverHollow.Game_Managers.GameManager;
using System;

namespace RiverHollow.GUIComponents.Screens.HUDScreens
{
    public class HUDParty : GUIMainObject
    {
        CharacterDetailObject[] _arrPartyMembers;
        public HUDParty()
        {
            _arrPartyMembers = new CharacterDetailObject[4];
            for (int i = 0; i < 4; i++)
            {
                _arrPartyMembers[i] = new CharacterDetailObject(PlayerManager.GetParty()[i], RefreshMaps);
                AddControl(_arrPartyMembers[i]);

                switch (i)
                {
                    case 1:
                        _arrPartyMembers[i].AnchorAndAlignToObject(_arrPartyMembers[0], SideEnum.Right, SideEnum.Bottom, ScaleIt(2));
                        break;
                    case 2:
                        _arrPartyMembers[i].AnchorAndAlignToObject(_arrPartyMembers[0], SideEnum.Bottom, SideEnum.Left, ScaleIt(2));
                        break;
                    case 3:
                        _arrPartyMembers[i].AnchorAndAlignToObject(_arrPartyMembers[2], SideEnum.Right, SideEnum.Bottom, ScaleIt(2));
                        break;
                }
            }
            Width = _arrPartyMembers[1].Right - _arrPartyMembers[0].Left;
            Height = _arrPartyMembers[2].Bottom - _arrPartyMembers[0].Top;

            CenterOnScreen();
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            for (int i = 0; i < 4; i++)
            {
                rv = _arrPartyMembers[i].ProcessLeftButtonClick(mouse);
                if (rv)
                {
                    break;
                }
            }

            return rv;
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = false;

            for (int i = 0; i < 4; i++)
            {
                rv = _arrPartyMembers[i].ProcessRightButtonClick(mouse);
                if (rv)
                {
                    break;
                }
            }

            return rv;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            for (int i = 0; i < 4; i++)
            {
                _arrPartyMembers[i].DrawEntries(spriteBatch);
            }
        }

        public void RefreshMaps()
        {
            for (int i = 0; i < 4; i++)
            {
                _arrPartyMembers[i]?.RefreshPositionMap();
            }
        }

        public class CharacterDetailObject : GUIObject
        {
            EmptyDelegate _delAction;
            EquipWindow _equipWindow;
            PositionMap _gPositionMap;

            ClassedCombatant _actor;
            GUISprite _actorSprite;

            List<SpecializedBox> _liGearBoxes;

            //GUIWindow _winClothes;

            SpecializedBox _sBoxArmor;
            SpecializedBox _sBoxHead;
            SpecializedBox _sBoxWeapon;
            SpecializedBox _sBoxAccessory;
            SpecializedBox _sBoxShirt;
            SpecializedBox _sBoxHat;

            GUIText _gVitality;
            Dictionary<AttributeEnum, GUIAttributeIcon> _diIcons;

            public CharacterDetailObject(ClassedCombatant c, EmptyDelegate action)
            {
                _delAction = action;
                _diIcons = new Dictionary<AttributeEnum, GUIAttributeIcon>();

                GUIImage panel = new GUIImage(new Rectangle(0, 160, 155, 80), DataManager.COMBAT_TEXTURE);
                AddControl(panel);

                if (c != null)
                {
                    _actor = c;

                    _actorSprite = new GUISprite(_actor.BodySprite, true);
                    _actorSprite.PlayAnimation(c.IsCritical() ? AnimationEnum.Critical : AnimationEnum.Idle);
                    _actorSprite.ScaledMoveBy(4, 4);
                    AddControl(_actorSprite);

                    GUIText gText = new GUIText(c.Name());
                    gText.ScaledMoveBy(41, 3);
                    AddControl(gText);

                    gText = new GUIText(c.CharacterClass.Name());
                    gText.ScaledMoveBy(109, 3);
                    AddControl(gText);

                    gText = new GUIText(c.ClassLevel.ToString(), true, DataManager.FONT_STAT_DISPLAY);
                    gText.ScaledMoveBy(144, 5);
                    AddControl(gText);

                    GUIImage temp = DataManager.GetIcon(GameIconEnum.MaxHealth);
                    temp.ScaledMoveBy(42, 18);
                    AddControl(temp);

                    _gVitality = new GUIText(c.CurrentHP + "/" + c.MaxHP, true, DataManager.FONT_STAT_DISPLAY);
                    _gVitality.AnchorAndAlignToObject(temp, SideEnum.Right, SideEnum.CenterY, ScaleIt(1));
                    AddControl(_gVitality);

                    temp = DataManager.GetIcon(GameIconEnum.Experience);
                    temp.ScaledMoveBy(116, 18);
                    AddControl(temp);

                    gText = new GUIText(c.CurrentXP + "/" + c.NextLevelAt(), true, DataManager.FONT_STAT_DISPLAY);
                    gText.AnchorAndAlignToObject(temp, SideEnum.Right, SideEnum.CenterY, ScaleIt(1));
                    AddControl(gText);

                    _diIcons[c.CharacterClass.KeyAttribute] = new GUIAttributeIcon(c.CharacterClass.KeyAttribute, c.Attribute(c.CharacterClass.KeyAttribute));
                    _diIcons[c.CharacterClass.KeyAttribute].ScaledMoveBy(46, 36);
                    AddControl(_diIcons[c.CharacterClass.KeyAttribute]);

                    _diIcons[AttributeEnum.Speed] = new GUIAttributeIcon(AttributeEnum.Speed, c.Attribute(AttributeEnum.Speed));
                    _diIcons[AttributeEnum.Speed].ScaledMoveBy(46, 48);
                    AddControl(_diIcons[AttributeEnum.Speed]);

                    _diIcons[AttributeEnum.Defence] = new GUIAttributeIcon(AttributeEnum.Defence, c.Attribute(AttributeEnum.Defence));
                    _diIcons[AttributeEnum.Defence].ScaledMoveBy(86, 36);
                    AddControl(_diIcons[AttributeEnum.Defence]);

                    _diIcons[AttributeEnum.Resistance] = new GUIAttributeIcon(AttributeEnum.Resistance, c.Attribute(AttributeEnum.Resistance));
                    _diIcons[AttributeEnum.Resistance].ScaledMoveBy(86, 49);
                    AddControl(_diIcons[AttributeEnum.Resistance]);

                    _diIcons[AttributeEnum.Evasion] = new GUIAttributeIcon(AttributeEnum.Evasion, c.Attribute(AttributeEnum.Evasion));
                    _diIcons[AttributeEnum.Evasion].ScaledMoveBy(86, 62);
                    AddControl(_diIcons[AttributeEnum.Evasion]);

                    _gPositionMap = new PositionMap(_actor, _delAction);
                    _gPositionMap.ScaledMoveBy(6, 46);
                    AddControl(_gPositionMap);

                    //_gLiteCombatActor.CharacterSprite.Reset();
                    //_gLiteCombatActor.CharacterWeaponSprite?.Reset();

                    _liGearBoxes = new List<SpecializedBox>();
                    Load();
                }                

                Width = panel.Width;
                Height = panel.Height;
            }

            public void DrawEntries(SpriteBatch spriteBatch)
            {
                if (_equipWindow != null && _equipWindow.HasEntries())
                {
                    _equipWindow.Draw(spriteBatch);
                }
            }

            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = false;
                if (_actor != null)
                {
                    if (_equipWindow.HasEntries() && _equipWindow.ProcessLeftButtonClick(mouse))
                    {
                        Item olditem = _equipWindow.Box.BoxItem;

                        _equipWindow.Box.SetItem(_equipWindow.SelectedItem);
                        if (_equipWindow.Box.ItemType.Equals(ItemEnum.Equipment))
                        {
                            _actor.Equip((Equipment)_equipWindow.SelectedItem);
                        }
                        else if (_equipWindow.Box.ItemType.Equals(ItemEnum.Clothing))
                        {
                            PlayerManager.PlayerActor.SetClothes((Clothing)_equipWindow.SelectedItem);
                        }

                        InventoryManager.RemoveItemFromInventory(_equipWindow.SelectedItem);
                        if (olditem != null) { InventoryManager.AddToInventory(olditem); }

                        DisplayAttributeText();
                        GUIManager.CloseHoverWindow();
                        _equipWindow.Clear();
                    }
                    else
                    {
                        foreach (SpecializedBox c in _liGearBoxes)
                        {
                            rv = c.ProcessLeftButtonClick(mouse);
                            if (rv)
                            {
                                GUIManager.CloseHoverWindow();
                                break;
                            }
                        }

                        rv = _gPositionMap.ProcessLeftButtonClick(mouse);
                    }
                }
                return rv;
            }

            public override bool ProcessRightButtonClick(Point mouse)
            {
                bool rv = false;

                if (_equipWindow != null && _equipWindow.HasEntries())
                {
                    GUIManager.CloseHoverWindow();
                    _equipWindow.Clear();
                    DisplayAttributeText();
                    rv = true;
                }
                else
                {
                    if (_liGearBoxes != null)
                    {
                        foreach (SpecializedBox box in _liGearBoxes)
                        {
                            if (box.Contains(mouse) && box.BoxItem != null)
                            {
                                GUIManager.CloseHoverWindow();

                                if (!box.WeaponType.Equals(WeaponEnum.None)) { _actor.Unequip(GearTypeEnum.Weapon); }
                                else if (!box.GearType.Equals(GearTypeEnum.None)) { _actor.Unequip(((Equipment)box.BoxItem).GearType); }
                                else if (!box.ClothingType.Equals(ClothingEnum.None))
                                {
                                    PlayerManager.PlayerActor.RemoveClothes(((Clothing)box.BoxItem).ClothesType);
                                }

                                DisplayAttributeText();

                                InventoryManager.AddToInventory(box.BoxItem);
                                box.SetItem(null);
                                rv = true;
                            }
                        }
                    }
                }

                return rv;
            }

            public override bool ProcessHover(Point mouse)
            {
                bool rv = false;
                if (_equipWindow != null && _equipWindow.HasEntries())
                {
                    rv = _equipWindow.ProcessHover(mouse);
                }
                else if (_liGearBoxes != null)
                {

                    foreach (SpecializedBox box in _liGearBoxes)
                    {
                        if (box.ProcessHover(mouse))
                        {
                            return true;
                        }
                    }

                    foreach(GUIAttributeIcon g in _diIcons.Values)
                    {
                        if (g.ProcessHover(mouse))
                        {
                            return true;
                        }
                    }

                    //_gBarXP.ProcessHover(mouse);
                    //_gBarHP.ProcessHover(mouse);
                }
                return rv;
            }

            private void Load()
            {
                _liGearBoxes.Clear();

                _sBoxWeapon = new SpecializedBox(_actor.CharacterClass.WeaponType, _actor.GetEquipment(GearTypeEnum.Weapon), FindMatchingItems);
                _sBoxArmor = new SpecializedBox(_actor.CharacterClass.ArmorType, GearTypeEnum.Chest, _actor.GetEquipment(GearTypeEnum.Chest), FindMatchingItems);
                _sBoxHead = new SpecializedBox(_actor.CharacterClass.ArmorType, GearTypeEnum.Head, _actor.GetEquipment(GearTypeEnum.Head), FindMatchingItems);
                _sBoxAccessory = new SpecializedBox(ArmorTypeEnum.None, GearTypeEnum.Accessory, _actor.GetEquipment(GearTypeEnum.Accessory), FindMatchingItems);

                _sBoxWeapon.ScaledMoveBy(109, 32);
                _sBoxArmor.AnchorAndAlignToObject(_sBoxWeapon, SideEnum.Right, SideEnum.Top, ScaleIt(1));
                _sBoxHead.AnchorAndAlignToObject(_sBoxWeapon, SideEnum.Bottom, SideEnum.Left, ScaleIt(1));
                _sBoxAccessory.AnchorAndAlignToObject(_sBoxHead, SideEnum.Right, SideEnum.Top, ScaleIt(1));

                _liGearBoxes.Add(_sBoxWeapon);
                _liGearBoxes.Add(_sBoxArmor);
                _liGearBoxes.Add(_sBoxHead);
                _liGearBoxes.Add(_sBoxAccessory);

                foreach (SpecializedBox b in _liGearBoxes) { AddControl(b); }

                //if (_character == PlayerManager.PlayerCombatant)
                //{
                //    _sBoxHat = new SpecializedBox(ClothingEnum.Hat, PlayerManager.PlayerActor.Hat, FindMatchingItems);
                //    _sBoxShirt = new SpecializedBox(ClothingEnum.Chest, PlayerManager.PlayerActor.Chest, FindMatchingItems);

                //    //_sBoxHat.AnchorToInnerSide(_winClothes, SideEnum.TopLeft, SPACING);
                //    _sBoxShirt.AnchorAndAlignToObject(_sBoxHat, SideEnum.Right, SideEnum.Top, SPACING);

                //    _liGearBoxes.Add(_sBoxHat);
                //    _liGearBoxes.Add(_sBoxShirt);
                //}

                DisplayAttributeText();

                _equipWindow = new EquipWindow();
                //_winClothes.AddControl(_equipWindow);
            }

            /// <summary>
            /// Delegate for hovering over equipment to equip. Updates the characters stats as apporpriate
            /// </summary>
            /// <param name="tempGear"></param>
            public void DisplayAttributeText(Equipment tempGear = null)
            {
                bool compareTemp = true;
                if (tempGear != null)
                {
                    if (tempGear.WeaponType != WeaponEnum.None) { _actor.EquipComparator(tempGear); }
                    else { _actor.EquipComparator(tempGear); }
                }
                else
                {
                    compareTemp = false;
                    _actor.ClearEquipmentCompare();
                }

                AssignStatText(_diIcons[_actor.KeyAttribute], _actor.Attribute(_actor.KeyAttribute), _actor.AttributeTemp(_actor.KeyAttribute), compareTemp);
                AssignStatText(_diIcons[AttributeEnum.Defence], _actor.Attribute(AttributeEnum.Defence), _actor.AttributeTemp(AttributeEnum.Defence), compareTemp);
                AssignStatText(_diIcons[AttributeEnum.Resistance], _actor.Attribute(AttributeEnum.Resistance), _actor.AttributeTemp(AttributeEnum.Resistance), compareTemp);
                AssignStatText(_diIcons[AttributeEnum.Evasion], _actor.Attribute(AttributeEnum.Evasion), _actor.AttributeTemp(AttributeEnum.Evasion), compareTemp);
                AssignStatText(_diIcons[AttributeEnum.Speed], _actor.Attribute(AttributeEnum.Speed), _actor.AttributeTemp(AttributeEnum.Speed), compareTemp);

                _gVitality.SetText(_actor.CurrentHP + "/" + _actor.MaxHP);
            }

            private void AssignStatText(GUIAttributeIcon attrIcon, int startStat, int tempStat, bool compareTemp)
            {
                attrIcon.SetText(compareTemp ? tempStat : startStat);
                if (!compareTemp) { attrIcon.SetColor(Color.White); }
                else
                {
                    if (startStat < tempStat) { attrIcon.SetColor(Color.Green); }
                    else if (startStat > tempStat) { attrIcon.SetColor(Color.Red); }
                    else { attrIcon.SetColor(Color.White); }
                }
            }

            /// <summary>
            /// Delegate method asssigned to the SpecializedItemBoxes
            /// When clicked, the itembox will find matching items int he players inventory.
            /// </summary>
            /// <param name="boxMatch"></param>
            private void FindMatchingItems(SpecializedBox boxMatch)
            {
                List<Item> liItems = new List<Item>();
                foreach (Item i in InventoryManager.PlayerInventory)
                {
                    if (i != null && i.ItemType.Equals(boxMatch.ItemType))
                    {
                        if (boxMatch.ItemType.Equals(ItemEnum.Equipment) && i.CompareType(ItemEnum.Equipment) && ((Equipment)i).GearType == boxMatch.GearType)
                        {
                            if (boxMatch.WeaponType != WeaponEnum.None && ((Equipment)i).WeaponType == boxMatch.WeaponType)
                            {
                                liItems.Add(i);
                            }
                            else if (((Equipment)i).ArmorType == boxMatch.ArmorType)
                            {
                                liItems.Add(i);
                            }
                        }
                        else if (boxMatch.ItemType.Equals(ItemEnum.Clothing) && i.CompareType(ItemEnum.Clothing))
                        {
                            if (boxMatch.ClothingType != ClothingEnum.None && ((Clothing)i).ClothesType == boxMatch.ClothingType)
                            {
                                liItems.Add(i);
                            }
                        }
                    }
                }

                if (liItems.Count > 0)
                {
                    GUIManager.CloseHoverWindow();

                    _equipWindow.Load(boxMatch, liItems, DisplayAttributeText);
                    _equipWindow.AnchorAndAlignToObject(boxMatch, SideEnum.Right, SideEnum.CenterY);
                }
            }

            public void RefreshPositionMap()
            {
                _gPositionMap?.RefreshMap();
            }
        }

        public class EquipWindow : GUIWindow
        {
            List<GUIItemBox> _gItemBoxes;
            public Item SelectedItem { get; private set; }

            ItemEnum _itemType;

            SpecializedBox _box;
            public SpecializedBox Box => _box;

            public delegate void DisplayEQ(Equipment test);
            private DisplayEQ _delDisplayEQ;

            public EquipWindow() : base(Window_2, 20, 20)
            {
                _gItemBoxes = new List<GUIItemBox>();
            }

            public void Load(SpecializedBox box, List<Item> items, DisplayEQ del)
            {
                _itemType = box.ItemType;
                _delDisplayEQ = del;
                _gItemBoxes = new List<GUIItemBox>();
                Controls.Clear();
                Width = 20;
                Height = 20;
                _box = box;

                for (int i = 0; i < items.Count; i++)
                {
                    GUIItemBox newBox = new GUIItemBox(items[i]);

                    if (i == 0) { newBox.AnchorToInnerSide(this, SideEnum.TopLeft); }
                    else { newBox.AnchorAndAlignToObject(_gItemBoxes[i - 1], SideEnum.Right, SideEnum.Bottom); }

                    _gItemBoxes.Add(newBox);
                }

                Resize();
            }

            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = false;
                foreach (GUIItemBox g in _gItemBoxes)
                {
                    if (g.Contains(mouse))
                    {
                        SelectedItem = g.BoxItem;
                        rv = true;
                        break;
                    }
                }
                return rv;
            }
            public override bool ProcessHover(Point mouse)
            {
                bool rv = false;

                Equipment temp = null;
                foreach (GUIItemBox box in _gItemBoxes)
                {
                    rv = box.ProcessHover(mouse);
                    if (rv && _itemType.Equals(ItemEnum.Equipment))
                    {
                        temp = (Equipment)box.BoxItem;
                    }
                }

                if (_itemType.Equals(ItemEnum.Equipment))
                {
                    _delDisplayEQ(temp);
                }

                return rv;
            }

            public override void Draw(SpriteBatch spriteBatch)
            {
                if (HasEntries())
                {
                    base.Draw(spriteBatch);
                }
            }
            public bool HasEntries() { return _gItemBoxes.Count > 0; }

            public void Clear()
            {
                Controls.Clear();
                _gItemBoxes.Clear();
            }
        }
    }

    public class PositionMap : GUIObject
    {
        EmptyDelegate _delAction;
        ClassedCombatant _actor;
        PositionObject[,] _arrPlayerTiles;

        public PositionMap(ClassedCombatant c, EmptyDelegate action)
        {
            _actor = c;
            _delAction = action;

            int rows = CombatManager.MAX_ROW;
            int cols = CombatManager.MAX_COLUMN / 2;
            _arrPlayerTiles = new PositionObject[rows, cols];

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    PositionObject obj = new PositionObject(row, col, _actor, _delAction);
                    _arrPlayerTiles[row, col] = obj;
                    AddControl(obj);

                    ColorizeMapObject(row, col, _arrPlayerTiles[row, col]);

                    if (col == 0 && row == 0)
                    {
                        continue;
                    }
                    if (col == 0 && row > 0)
                    {
                        obj.AnchorAndAlignToObject(_arrPlayerTiles[row - 1, col], SideEnum.Bottom, SideEnum.Left, ScaleIt(1));
                    }
                    else
                    {
                        obj.AnchorAndAlignToObject(_arrPlayerTiles[row, col - 1], SideEnum.Right, SideEnum.Top, ScaleIt(1));
                    }
                }
            }
        }

        public void RefreshMap()
        {
            int rows = CombatManager.MAX_ROW;
            int cols = CombatManager.MAX_COLUMN / 2;
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    ColorizeMapObject(row, col, _arrPlayerTiles[row, col]);
                }
            }
        }

        private void ColorizeMapObject(int row, int col, PositionObject obj)
        {
            for (int i = 0; i < PlayerManager.GetParty().Length; i++)
            {
                ClassedCombatant temp = PlayerManager.GetParty()[i];
                if (temp != null && temp.StartPosition.X == col && temp.StartPosition.Y == row)
                {
                    if (temp == _actor) { obj.SetColor(Color.Green); }
                    else { obj.SetColor(Color.Gray); }
                    break;
                }
                else
                {
                    obj.SetColor(Color.White);
                }
            }
        }

        class PositionObject : GUIImage
        {
            ClassedCombatant _actor;
            protected EmptyDelegate _delAction;
            public readonly int X;
            public readonly int Y;

            public PositionObject(int x, int y, ClassedCombatant actor, EmptyDelegate action) : base(new Rectangle(156, 181, 8, 8), DataManager.COMBAT_TEXTURE)
            {
                _actor = actor;
                _delAction = action;
                X = x;
                Y = y;
            }

            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = false;
                if (Contains(mouse))
                {
                    rv = true;

                    Vector2 start = _actor.StartPosition;
                    Vector2 myPosition = new Vector2(Y, X);
                    ClassedCombatant occupant = Array.Find(PlayerManager.GetParty(), x => x?.StartPosition == myPosition);
                    if(occupant != null)
                    {
                        occupant.SetStartPosition(start);
                    }
                    _actor.SetStartPosition(myPosition);

                    _delAction();
                }
                return rv;
            }
        }
    }
}
