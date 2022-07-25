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
using MonoGame.Extended.BitmapFonts;

namespace RiverHollow.GUIComponents.Screens.HUDScreens
{
    public class HUDParty : GUIMainObject
    {
        CharacterDetailObject[] _arrPartyMembers;
        public HUDParty()
        {
            _arrPartyMembers = new CharacterDetailObject[4];
            for (int i=0; i < 4; i++)
            {
                _arrPartyMembers[i] = new CharacterDetailObject(PlayerManager.GetParty()[i]);
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

        public class CharacterDetailObject : GUIObject
        {
            EquipWindow _equipWindow;

            ClassedCombatant _character;
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

            public CharacterDetailObject(ClassedCombatant c)
            {
                _diIcons = new Dictionary<AttributeEnum, GUIAttributeIcon>();

                GUIImage panel = new GUIImage(new Rectangle(0, 160, 155, 66), DataManager.COMBAT_TEXTURE);
                AddControl(panel);

                if (c != null)
                {
                    _character = c;

                    _actorSprite = new GUISprite(_character.BodySprite, true);
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
                    _diIcons[c.CharacterClass.KeyAttribute].ScaledMoveBy(5, 47);
                    AddControl(_diIcons[c.CharacterClass.KeyAttribute]);

                    _diIcons[AttributeEnum.Defence] = new GUIAttributeIcon(AttributeEnum.Defence, c.Attribute(AttributeEnum.Defence));
                    _diIcons[AttributeEnum.Defence].ScaledMoveBy(47, 32);
                    AddControl(_diIcons[AttributeEnum.Defence]);

                    _diIcons[AttributeEnum.Resistance] = new GUIAttributeIcon(AttributeEnum.Resistance, c.Attribute(AttributeEnum.Resistance));
                    _diIcons[AttributeEnum.Resistance].ScaledMoveBy(47, 41);
                    AddControl(_diIcons[AttributeEnum.Resistance]);

                    _diIcons[AttributeEnum.Evasion] = new GUIAttributeIcon(AttributeEnum.Evasion, c.Attribute(AttributeEnum.Evasion));
                    _diIcons[AttributeEnum.Evasion].ScaledMoveBy(47, 50);
                    AddControl(_diIcons[AttributeEnum.Evasion]);

                    _diIcons[AttributeEnum.Speed] = new GUIAttributeIcon(AttributeEnum.Speed, c.Attribute(AttributeEnum.Speed));
                    _diIcons[AttributeEnum.Speed].ScaledMoveBy(130, 52);
                    AddControl(_diIcons[AttributeEnum.Speed]);

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
                if (_character != null)
                {
                    if (_equipWindow.HasEntries() && _equipWindow.ProcessLeftButtonClick(mouse))
                    {
                        Item olditem = _equipWindow.Box.BoxItem;

                        _equipWindow.Box.SetItem(_equipWindow.SelectedItem);
                        if (_equipWindow.Box.ItemType.Equals(ItemEnum.Equipment))
                        {
                            _character.Equip((Equipment)_equipWindow.SelectedItem);
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

                                if (!box.WeaponType.Equals(WeaponEnum.None)) { _character.Unequip(GearTypeEnum.Weapon); }
                                else if (!box.GearType.Equals(GearTypeEnum.None)) { _character.Unequip(((Equipment)box.BoxItem).GearType); }
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

                _sBoxWeapon = new SpecializedBox(_character.CharacterClass.WeaponType, _character.GetEquipment(GearTypeEnum.Weapon), FindMatchingItems);
                _sBoxArmor = new SpecializedBox(_character.CharacterClass.ArmorType, GearTypeEnum.Chest, _character.GetEquipment(GearTypeEnum.Chest), FindMatchingItems);
                _sBoxHead = new SpecializedBox(_character.CharacterClass.ArmorType, GearTypeEnum.Head, _character.GetEquipment(GearTypeEnum.Head), FindMatchingItems);
                _sBoxAccessory = new SpecializedBox(ArmorTypeEnum.None, GearTypeEnum.Accessory, _character.GetEquipment(GearTypeEnum.Accessory), FindMatchingItems);

                _sBoxWeapon.ScaledMoveBy(67, 32);
                _sBoxArmor.AnchorAndAlignToObject(_sBoxWeapon, SideEnum.Right, SideEnum.Top, ScaleIt(1));
                _sBoxHead.AnchorAndAlignToObject(_sBoxArmor, SideEnum.Right, SideEnum.Top, ScaleIt(1));
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
                    if (tempGear.WeaponType != WeaponEnum.None) { _character.EquipComparator(tempGear); }
                    else { _character.EquipComparator(tempGear); }
                }
                else
                {
                    compareTemp = false;
                    _character.ClearEquipmentCompare();
                }

                AssignStatText(_diIcons[_character.KeyAttribute], _character.Attribute(_character.KeyAttribute), _character.AttributeTemp(_character.KeyAttribute), compareTemp);
                AssignStatText(_diIcons[AttributeEnum.Defence], _character.Attribute(AttributeEnum.Defence), _character.AttributeTemp(AttributeEnum.Defence), compareTemp);
                AssignStatText(_diIcons[AttributeEnum.Resistance], _character.Attribute(AttributeEnum.Resistance), _character.AttributeTemp(AttributeEnum.Resistance), compareTemp);
                AssignStatText(_diIcons[AttributeEnum.Evasion], _character.Attribute(AttributeEnum.Evasion), _character.AttributeTemp(AttributeEnum.Evasion), compareTemp);
                AssignStatText(_diIcons[AttributeEnum.Speed], _character.Attribute(AttributeEnum.Speed), _character.AttributeTemp(AttributeEnum.Speed), compareTemp);

                _gVitality.SetText(_character.CurrentHP + "/" + _character.MaxHP);
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

    //private class PositionMap : GUIWindow
    //{
    //    ClassedCombatant _currentCharacter;
    //    StartPosition _currPosition;
    //    StartPosition[,] _arrStartPositions;

    //    public delegate void ClickDelegate(ClassedCombatant selectedCharacter);
    //    private ClickDelegate _delAction;

    //    public PositionMap(ClickDelegate del) : base(Window_2, 16, 16)
    //    {
    //        _delAction = del;

    //        //Actual entries will be one higher since we go to 0 inclusive
    //        int maxColIndex = 2;
    //        int maxRowIndex = 2;

    //        int spacing = 10;
    //        _arrStartPositions = new StartPosition[maxColIndex + 1, maxRowIndex + 1]; //increment by one as stated above
    //        for (int cols = maxColIndex; cols >= 0; cols--)
    //        {
    //            for (int rows = maxRowIndex; rows >= 0; rows--)
    //            {
    //                StartPosition pos = new StartPosition(cols, rows);
    //                _arrStartPositions[cols, rows] = pos;
    //                if (cols == maxColIndex && rows == maxRowIndex)
    //                {
    //                    pos.AnchorToInnerSide(this, SideEnum.TopLeft, spacing);
    //                }
    //                else if (cols == maxColIndex)
    //                {
    //                    pos.AnchorAndAlignToObject(_arrStartPositions[maxColIndex, rows + 1], SideEnum.Bottom, SideEnum.Left, spacing);
    //                }
    //                else
    //                {
    //                    pos.AnchorAndAlignToObject(_arrStartPositions[cols + 1, rows], SideEnum.Right, SideEnum.Bottom, spacing);
    //                }
    //            }
    //        }

    //        PopulatePositionMap();

    //        this.Resize();
    //    }

    //    public override void Update(GameTime gTime)
    //    {
    //        base.Update(gTime);
    //    }

    //    /// <summary>
    //    /// Populates the PositionMap with the initial starting positions of the party
    //    /// </summary>
    //    public void PopulatePositionMap()
    //    {
    //        //_currentCharacter = PlayerManager.World;

    //        ////Iterate over each member of the party and retrieve their starting position.
    //        ////Assigns the character to the starting position and assigns the current position
    //        ////to the Player Character's
    //        //foreach (ClassedCombatant c in PlayerManager.GetTacticalParty())
    //        //{
    //        //    Vector2 vec = c.StartPosition;
    //        //    _arrStartPositions[(int)vec.X, (int)vec.Y].SetCharacter(c, (c == _currentCharacter));
    //        //    if (c == _currentCharacter)
    //        //    {
    //        //        _currPosition = _arrStartPositions[(int)vec.X, (int)vec.Y];
    //        //    }
    //        //}
    //    }

    //    public override bool ProcessLeftButtonClick(Point mouse)
    //    {
    //        bool rv = false;

    //        if (Contains(mouse))
    //        {
    //            rv = true;
    //        }

    //        foreach (StartPosition sp in _arrStartPositions)
    //        {
    //            //If we have clicked on a StartPosition
    //            if (sp.Contains(mouse))
    //            {
    //                //If the StartPosition is not occupied set the currentPosition to null
    //                //then set it to the clicked StartPosition and assign the current Character.
    //                //Finally, reset the characters internal start position vector.
    //                if (!sp.Occupied())
    //                {
    //                    rv = true;
    //                    _currPosition.SetCharacter(null);
    //                    _currPosition = sp;
    //                    //_currPosition.SetCharacter(_currentCharacter, true);
    //                    _currentCharacter.SetStartPosition(new Vector2(_currPosition.Col, _currPosition.Row));
    //                }
    //                else
    //                {
    //                    _currPosition?.PlayAnimation(VerbEnum.Idle, DirectionEnum.Down);
    //                    _currPosition = sp;
    //                    //Set the currentCharacter to the selected character.
    //                    //Call up to the parent object to redisplay data.
    //                    _currentCharacter = sp.Character;
    //                    //_delAction(_currentCharacter);
    //                    _currPosition?.PlayAnimation(VerbEnum.Walk, DirectionEnum.Down);
    //                }

    //                break;
    //            }
    //        }

    //        return rv;
    //    }

    //    private class StartPosition : GUIImage
    //    {
    //        ClassedCombatant _character;
    //        public ClassedCombatant Character => _character;
    //        int _iCol;
    //        int _iRow;
    //        public int Col => _iCol;
    //        public int Row => _iRow;

    //        private GUICharacterSprite _sprite;
    //        public StartPosition(int col, int row) : base(new Rectangle(0, 112, 16, 16), TILE_SIZE, TILE_SIZE, DataManager.FILE_WORLDOBJECTS)
    //        {
    //            _iCol = col;
    //            _iRow = row;

    //            SetScale(CurrentScale);
    //        }

    //        public override void Draw(SpriteBatch spriteBatch)
    //        {
    //            base.Draw(spriteBatch);
    //            if (_sprite != null)
    //            {
    //                _sprite.Draw(spriteBatch);
    //            }
    //        }

    //        /// <summary>
    //        /// Assigns the character that the StartPosition is referring to.
    //        /// 
    //        /// Configures the Sprite and Adds it to the Controls ifthere is a character.
    //        /// Removes it if not.
    //        /// </summary>
    //        /// <param name="c">The Character to assign to the StartPosition</param>
    //        /// <param name="currentCharacter">Whether the character is the current character and should walk</param>
    //        public void SetCharacter(ClassedCombatant c, bool currentCharacter = false)
    //        {
    //            //_character = c;
    //            if (c != null)
    //            {
    //                if (c == PlayerManager.PlayerCombatant) { _sprite = new GUICharacterSprite(true); }
    //                else { _sprite = new GUICharacterSprite(c.BodySprite, true); }

    //                _sprite.SetScale(2);
    //                _sprite.CenterOnObject(this);
    //                _sprite.MoveBy(new Vector2(0, -(this.Width / 4)));

    //                if (currentCharacter) { _sprite.PlayAnimation(VerbEnum.Walk, DirectionEnum.Down); }
    //                else { _sprite.PlayAnimation(VerbEnum.Idle, DirectionEnum.Down); }
    //                AddControl(_sprite);
    //            }
    //            else
    //            {
    //                RemoveControl(_sprite);
    //                _sprite = null;
    //            }
    //        }

    //        public bool Occupied() { return _character != null; }

    //        /// <summary>
    //        /// Wrapper for the PositionMap to call down to the Sprite directly
    //        /// </summary>
    //        /// <typeparam name="TEnum">Template for any enum type</typeparam>
    //        /// <param name="animation">The animation enum to play</param>
    //        public void PlayAnimation(VerbEnum verb, DirectionEnum dir)
    //        {
    //            _sprite.PlayAnimation(verb, dir);
    //        }
    //    }
    //}
}
