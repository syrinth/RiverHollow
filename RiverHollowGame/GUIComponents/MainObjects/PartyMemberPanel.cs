using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using RiverHollow.Characters.Lite;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Items;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.GUIComponents.GUIObjects.GUIItemBox;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.MainObjects
{
    public class PartyMemberPanel : GUIObject
    {
        ClassedCombatant _partyMember;

        GUISprite _actorSprite;
        GUIText _gName;
        GUIText _gClassName;
        GUIText _gClassLevel;
        GUIText _gHPValue;

        public PartyMemberPanel(ClassedCombatant actor)
        {
            GUIImage panel = new GUIImage(new Rectangle(0, 160, 155, 64), DataManager.COMBAT_TEXTURE);
            AddControl(panel);

            _partyMember = actor;

            _actorSprite = new GUISprite(_partyMember.BodySprite, true);
            _actorSprite.PlayAnimation(actor.IsCritical() ? AnimationEnum.Critical : AnimationEnum.Idle);
            _actorSprite.Position(Position() + new Vector2(ScaleIt(4), ScaleIt(4)));
            AddControl(_actorSprite);

            _gName = new GUIText(_partyMember.Name());
            _gName.Position(Position() + new Vector2(ScaleIt(41), ScaleIt(2)));
            AddControl(_gName);

            GUIImage hpIcon = DataManager.GetIcon(GameIconEnum.MaxHealth);
            hpIcon.Position(Position() + new Vector2(ScaleIt(42), ScaleIt(18)));
            AddControl(hpIcon);

            _gHPValue = new GUIText(_partyMember.CurrentHP + @"/" + _partyMember.MaxHP, DataManager.GetBitMapFont(DataManager.FONT_STAT_DISPLAY));
            _gHPValue.Position(Position() + new Vector2(ScaleIt(53), ScaleIt(19)));
            AddControl(_gHPValue);

            GUIText xpValue = new GUIText(_partyMember.CurrentXP + @"/" + ClassedCombatant.LevelRange[_partyMember.ClassLevel], DataManager.GetBitMapFont(DataManager.FONT_STAT_DISPLAY));
            xpValue.Position(Position() + new Vector2(ScaleIt(53), ScaleIt(19)));
            xpValue.AnchorAndAlignToObject(panel, SideEnum.Right, SideEnum.Top);
            xpValue.ScaledMoveBy(-7, 19);
            xpValue.MoveBy(-xpValue.Width, 0);
            AddControl(xpValue);

            GUIImage xpIcon = DataManager.GetIcon(GameIconEnum.Experience);
            xpIcon.AnchorAndAlignToObject(xpValue, SideEnum.Left, SideEnum.Bottom);
            xpIcon.ScaledMoveBy(-1, 0);
            AddControl(xpIcon);

            GUIImage mainAttribute = DataManager.GetIcon(GameManager.GetGameIconFromAttribute(actor.KeyAttribute));
            mainAttribute.ScaledMoveBy(5, 47);
            AddControl(mainAttribute);

            GUIText attr = new GUIText(actor.Attribute(actor.KeyAttribute).ToString(), DataManager.GetBitMapFont(DataManager.FONT_STAT_DISPLAY));
            attr.AnchorAndAlignToObject(mainAttribute, SideEnum.Right, SideEnum.CenterY);
            attr.ScaledMoveBy(1, 0);
            AddControl(attr);

            Width = panel.Width;
            Height = panel.Height;
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            foreach (GUIObject o in Controls)
            {
                rv = o.ProcessLeftButtonClick(mouse);
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
            //if (//_charBox != null)
            {
           //     rv = _charBox.ProcessRightButtonClick(mouse);
            }
            return rv;
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = true;

           // if (_charBox != null)
            {
           //    rv = _charBox.ProcessHover(mouse);
            }

            return rv;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);
        }

        /// <summary>
        /// Sets the selected character for the CharacterDetailBox and loads
        /// the relevant data.
        /// </summary>
        /// <param name="selectedCharacter"></param>
        public void SetSelectedCharacter(ClassedCombatant selectedCharacter)
        {
           // _charBox?.SetAdventurer(selectedCharacter);
        }

        private class PositionMap : GUIWindow
        {
            ClassedCombatant _currentCharacter;
            StartPosition _currPosition;
            StartPosition[,] _arrStartPositions;

            public delegate void ClickDelegate(ClassedCombatant selectedCharacter);
            private ClickDelegate _delAction;

            public PositionMap(ClickDelegate del) : base(Window_2, 16, 16)
            {
                _delAction = del;

                //Actual entries will be one higher since we go to 0 inclusive
                int maxColIndex = 2;
                int maxRowIndex = 2;

                int spacing = 10;
                _arrStartPositions = new StartPosition[maxColIndex + 1, maxRowIndex + 1]; //increment by one as stated above
                for (int cols = maxColIndex; cols >= 0; cols--)
                {
                    for (int rows = maxRowIndex; rows >= 0; rows--)
                    {
                        StartPosition pos = new StartPosition(cols, rows);
                        _arrStartPositions[cols, rows] = pos;
                        if (cols == maxColIndex && rows == maxRowIndex)
                        {
                            pos.AnchorToInnerSide(this, SideEnum.TopLeft, spacing);
                        }
                        else if (cols == maxColIndex)
                        {
                            pos.AnchorAndAlignToObject(_arrStartPositions[maxColIndex, rows + 1], SideEnum.Bottom, SideEnum.Left, spacing);
                        }
                        else
                        {
                            pos.AnchorAndAlignToObject(_arrStartPositions[cols + 1, rows], SideEnum.Right, SideEnum.Bottom, spacing);
                        }
                    }
                }

                PopulatePositionMap();

                this.Resize();
            }

            public override void Update(GameTime gTime)
            {
                base.Update(gTime);
            }

            /// <summary>
            /// Populates the PositionMap with the initial starting positions of the party
            /// </summary>
            public void PopulatePositionMap()
            {
                //_currentCharacter = PlayerManager.World;

                ////Iterate over each member of the party and retrieve their starting position.
                ////Assigns the character to the starting position and assigns the current position
                ////to the Player Character's
                //foreach (ClassedCombatant c in PlayerManager.GetTacticalParty())
                //{
                //    Vector2 vec = c.StartPosition;
                //    _arrStartPositions[(int)vec.X, (int)vec.Y].SetCharacter(c, (c == _currentCharacter));
                //    if (c == _currentCharacter)
                //    {
                //        _currPosition = _arrStartPositions[(int)vec.X, (int)vec.Y];
                //    }
                //}
            }

            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = false;

                if (Contains(mouse))
                {
                    rv = true;
                }

                foreach (StartPosition sp in _arrStartPositions)
                {
                    //If we have clicked on a StartPosition
                    if (sp.Contains(mouse))
                    {
                        //If the StartPosition is not occupied set the currentPosition to null
                        //then set it to the clicked StartPosition and assign the current Character.
                        //Finally, reset the characters internal start position vector.
                        if (!sp.Occupied())
                        {
                            rv = true;
                            _currPosition.SetCharacter(null);
                            _currPosition = sp;
                            //_currPosition.SetCharacter(_currentCharacter, true);
                            _currentCharacter.SetStartPosition(new Vector2(_currPosition.Col, _currPosition.Row));
                        }
                        else
                        {
                            _currPosition?.PlayAnimation(VerbEnum.Idle, DirectionEnum.Down);
                            _currPosition = sp;
                            //Set the currentCharacter to the selected character.
                            //Call up to the parent object to redisplay data.
                            _currentCharacter = sp.Character;
                            //_delAction(_currentCharacter);
                            _currPosition?.PlayAnimation(VerbEnum.Walk, DirectionEnum.Down);
                        }

                        break;
                    }
                }

                return rv;
            }

            private class StartPosition : GUIImage
            {
                ClassedCombatant _character;
                public ClassedCombatant Character => _character;
                int _iCol;
                int _iRow;
                public int Col => _iCol;
                public int Row => _iRow;

                private GUICharacterSprite _sprite;
                public StartPosition(int col, int row) : base(new Rectangle(0, 112, 16, 16), TILE_SIZE, TILE_SIZE, DataManager.FILE_WORLDOBJECTS)
                {
                    _iCol = col;
                    _iRow = row;

                    SetScale(CurrentScale);
                }

                public override void Draw(SpriteBatch spriteBatch)
                {
                    base.Draw(spriteBatch);
                    if (_sprite != null)
                    {
                        _sprite.Draw(spriteBatch);
                    }
                }

                /// <summary>
                /// Assigns the character that the StartPosition is referring to.
                /// 
                /// Configures the Sprite and Adds it to the Controls ifthere is a character.
                /// Removes it if not.
                /// </summary>
                /// <param name="c">The Character to assign to the StartPosition</param>
                /// <param name="currentCharacter">Whether the character is the current character and should walk</param>
                public void SetCharacter(ClassedCombatant c, bool currentCharacter = false)
                {
                    //_character = c;
                    if (c != null)
                    {
                        if (c == PlayerManager.PlayerCombatant) { _sprite = new GUICharacterSprite(true); }
                        else { _sprite = new GUICharacterSprite(c.BodySprite, true); }

                        _sprite.SetScale(2);
                        _sprite.CenterOnObject(this);
                        _sprite.MoveBy(new Vector2(0, -(this.Width / 4)));

                        if (currentCharacter) { _sprite.PlayAnimation(VerbEnum.Walk, DirectionEnum.Down); }
                        else { _sprite.PlayAnimation(VerbEnum.Idle, DirectionEnum.Down); }
                        AddControl(_sprite);
                    }
                    else
                    {
                        RemoveControl(_sprite);
                        _sprite = null;
                    }
                }

                public bool Occupied() { return _character != null; }

                /// <summary>
                /// Wrapper for the PositionMap to call down to the Sprite directly
                /// </summary>
                /// <typeparam name="TEnum">Template for any enum type</typeparam>
                /// <param name="animation">The animation enum to play</param>
                public void PlayAnimation(VerbEnum verb, DirectionEnum dir)
                {
                    _sprite.PlayAnimation(verb, dir);
                }
            }
        }

        public class CharacterDetailObject : GUIObject
        {
            const int SPACING = 10;
            EquipWindow _equipWindow;

            ClassedCombatant _character;
            BitmapFont _font;

            List<SpecializedBox> _liGearBoxes;

            GUIWindow _winName;
            public GUIWindow WinDisplay;
            //GUIWindow _winClothes;

            SpecializedBox _sBoxArmor;
            SpecializedBox _sBoxHead;
            SpecializedBox _sBoxWeapon;
            SpecializedBox _sBoxAccessory;
            SpecializedBox _sBoxShirt;
            SpecializedBox _sBoxHat;
            GUIButton _btnSwap;

            GUIText _gName, _gClass, _gLvl, _gStr, _gDef, _gMagic, _gRes, _gSpd;

            public CharacterDetailObject(ClassedCombatant c)
            {
                _winName = new GUIWindow(GUIWindow.Window_1, (GUIManager.MAIN_COMPONENT_WIDTH) - (GUIWindow.Window_1.Edge * 2), 10);
                WinDisplay = new GUIWindow(GUIWindow.Window_1, (GUIManager.MAIN_COMPONENT_WIDTH) - (GUIWindow.Window_1.Edge * 2), (GUIManager.MAIN_COMPONENT_HEIGHT / 4) - (GUIWindow.Window_1.Edge * 2));
                WinDisplay.AnchorAndAlignToObject(_winName, SideEnum.Bottom, SideEnum.Left);
                //_winClothes = new GUIWindow(GUIWindow.RedWin, 10, 10);
                //_winClothes.AnchorAndAlignToObject(WinDisplay, SideEnum.Bottom, SideEnum.Left);

                _character = c;
                _font = DataManager.GetBitMapFont(DataManager.FONT_NEW);

                _liGearBoxes = new List<SpecializedBox>();
                Load();

                _winName.Resize();
                _winName.Height += SPACING;

                WinDisplay.Resize();
                WinDisplay.Height += SPACING;

                //_winClothes.Resize();
                //_winClothes.Height += SPACING;
                //_winClothes.Width += SPACING;

                WinDisplay.AnchorAndAlignToObject(_winName, SideEnum.Bottom, SideEnum.Left, ScaleIt(1));
                //_winClothes.AnchorAndAlignToObject(WinDisplay, SideEnum.Bottom, SideEnum.Left);

                AddControl(_winName);
                //AddControl(_winClothes);
                AddControl(WinDisplay);

                Width = _winName.Width;
                Height = WinDisplay.Bottom - _winName.Top;
            }

            private void Load()
            {
                //_winClothes.Controls.Clear();
                _winName.Controls.Clear();
                WinDisplay.Controls.Clear();

                _liGearBoxes.Clear();

                string nameLen = "";
                for (int i = 0; i < GameManager.MAX_NAME_LEN; i++) { nameLen += "X"; }

                _gName = new GUIText(nameLen);
                _gName.AnchorToInnerSide(_winName, SideEnum.TopLeft, GUIManager.STANDARD_MARGIN);
                _gClass = new GUIText("XXXXXXXX");
                _gClass.AnchorAndAlignToObject(_gName, SideEnum.Right, SideEnum.Bottom, GUIManager.STANDARD_MARGIN);

                _sBoxHead = new SpecializedBox(_character.CharacterClass.ArmorType, _character.GetEquipment(GearTypeEnum.Head), FindMatchingItems);
                _sBoxArmor = new SpecializedBox(_character.CharacterClass.ArmorType, _character.GetEquipment(GearTypeEnum.Chest), FindMatchingItems);
                _sBoxWeapon = new SpecializedBox(_character.CharacterClass.WeaponType, _character.GetEquipment(GearTypeEnum.Weapon), FindMatchingItems);
                _sBoxAccessory = new SpecializedBox(_character.CharacterClass.ArmorType, _character.GetEquipment(GearTypeEnum.Accessory), FindMatchingItems);

                _sBoxArmor.AnchorToInnerSide(WinDisplay, SideEnum.TopRight, SPACING);
                _sBoxHead.AnchorAndAlignToObject(_sBoxArmor, SideEnum.Left, SideEnum.Top, SPACING);
                _sBoxAccessory.AnchorAndAlignToObject(_sBoxArmor, SideEnum.Bottom, SideEnum.Right, SPACING);
                _sBoxWeapon.AnchorAndAlignToObject(_sBoxAccessory, SideEnum.Left, SideEnum.Top, SPACING);

                _liGearBoxes.Add(_sBoxHead);
                _liGearBoxes.Add(_sBoxArmor);
                _liGearBoxes.Add(_sBoxWeapon);
                _liGearBoxes.Add(_sBoxAccessory);

                int barWidth = _sBoxArmor.DrawRectangle.Right - _sBoxHead.DrawRectangle.Left;
               // _gBarXP = new GUIOldStatDisplay(_character.GetXP, Color.Yellow, barWidth);
               // _gBarXP.AnchorToInnerSide(_winName, SideEnum.Right, SPACING);
                //_gBarXP.AlignToObject(_gName, SideEnum.CenterY);

                _gLvl = new GUIText("LV. X");
                //_gLvl.AnchorAndAlignToObject(_gBarXP, SideEnum.Left, SideEnum.CenterY, SPACING);
                _gLvl.SetText("LV. " + _character.ClassLevel);

               // _gBarHP = new GUIOldStatDisplay(_character.GetHP, Color.Green, barWidth);
               // _gBarHP.AnchorAndAlignToObject(_sBoxHead, SideEnum.Left, SideEnum.Top, SPACING);

                if (_character == PlayerManager.PlayerCombatant)
                {
                    _sBoxHat = new SpecializedBox(ClothingEnum.Hat, PlayerManager.PlayerActor.Hat, FindMatchingItems);
                    _sBoxShirt = new SpecializedBox(ClothingEnum.Chest, PlayerManager.PlayerActor.Chest, FindMatchingItems);

                    //_sBoxHat.AnchorToInnerSide(_winClothes, SideEnum.TopLeft, SPACING);
                    _sBoxShirt.AnchorAndAlignToObject(_sBoxHat, SideEnum.Right, SideEnum.Top, SPACING);

                    _liGearBoxes.Add(_sBoxHat);
                    _liGearBoxes.Add(_sBoxShirt);
                }

                _gStr = new GUIText("Str: 99");
                _gDef = new GUIText("Def: 99");
                _gMagic = new GUIText("Mag: 99");
                _gRes = new GUIText("Res: 999");
                _gSpd = new GUIText("Spd: 999");
                _gStr.AnchorToInnerSide(WinDisplay, SideEnum.TopLeft, SPACING);
                _gDef.AnchorAndAlignToObject(_gStr, SideEnum.Bottom, SideEnum.Left, SPACING);
                _gMagic.AnchorAndAlignToObject(_gDef, SideEnum.Bottom, SideEnum.Left, SPACING);
                _gRes.AnchorAndAlignToObject(_gMagic, SideEnum.Bottom, SideEnum.Left, SPACING);
                _gSpd.AnchorAndAlignToObject(_gRes, SideEnum.Bottom, SideEnum.Left, SPACING);

                _gName.SetText(_character.Name());
                _gClass.SetText(_character.CharacterClass.Name());

                DisplayStatText();

                _equipWindow = new EquipWindow();
                WinDisplay.AddControl(_equipWindow);
                //_winClothes.AddControl(_equipWindow);
            }

            /// <summary>
            /// Delegate for hovering over equipment to equip. Updates the characters stats as apporpriate
            /// </summary>
            /// <param name="tempGear"></param>
            public void DisplayStatText(Equipment tempGear = null)
            {
                bool compareTemp = true;
                if (tempGear != null)
                {
                    if (tempGear.WeaponType != WeaponEnum.None) { _character.EquipComparator(tempGear); }
                    else if (tempGear.ArmorType != ArmorTypeEnum.None) { _character.EquipComparator(tempGear); }
                    else
                    {
                        compareTemp = false;
                    }
                }
                else
                {
                    compareTemp = false;
                    _character.ClearEquipmentCompare();
                }

                AssignStatText(_gStr, "Str", _character.Attribute(AttributeEnum.Strength), _character.TempAttribute(AttributeEnum.Strength), compareTemp);
                AssignStatText(_gDef, "Def", _character.Attribute(AttributeEnum.Defence), _character.TempAttribute(AttributeEnum.Defence), compareTemp);
                AssignStatText(_gMagic, "Mag", _character.Attribute(AttributeEnum.Magic), _character.TempAttribute(AttributeEnum.Magic), compareTemp);
                AssignStatText(_gRes, "Res", _character.Attribute(AttributeEnum.Resistance), _character.TempAttribute(AttributeEnum.Resistance), compareTemp);
                AssignStatText(_gSpd, "Spd", _character.Attribute(AttributeEnum.Speed), _character.TempAttribute(AttributeEnum.Speed), compareTemp);
            }

            private void AssignStatText(GUIText txtStat, string statString, int startStat, int tempStat, bool compareTemp)
            {
                txtStat.SetText(statString + ": " + (compareTemp ? tempStat : startStat));
                if (!compareTemp) { txtStat.SetColor(Color.White); }
                else
                {
                    if (startStat < tempStat) { txtStat.SetColor(Color.Green); }
                    else if (startStat > tempStat) { txtStat.SetColor(Color.Red); }
                    else { txtStat.SetColor(Color.White); }
                }
            }

            public override void Draw(SpriteBatch spriteBatch)
            {
                if (_character != null)
                {
                    _winName.Draw(spriteBatch);
                    WinDisplay.Draw(spriteBatch);
                    //if (_character == PlayerManager.World) { _winClothes.Draw(spriteBatch); }

                    if (_equipWindow.HasEntries())
                    {
                        _equipWindow.Draw(spriteBatch);
                    }
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
                            AssignEquipment((Equipment)_equipWindow.SelectedItem);
                        }
                        else if (_equipWindow.Box.ItemType.Equals(ItemEnum.Clothing))
                        {
                            PlayerManager.PlayerActor.SetClothes((Clothing)_equipWindow.SelectedItem);

                        }

                        InventoryManager.RemoveItemFromInventory(_equipWindow.SelectedItem);
                        if (olditem != null) { InventoryManager.AddToInventory(olditem); }

                        DisplayStatText();
                        GUIManager.CloseHoverWindow();
                        _equipWindow.Clear();
                    }
                    else
                    {
                        foreach (GUIObject c in WinDisplay.Controls)
                        {
                            rv = c.ProcessLeftButtonClick(mouse);
                            if (rv)
                            {
                                GUIManager.CloseHoverWindow();
                                break;
                            }
                        }

                        if (!rv && _character == PlayerManager.PlayerCombatant)
                        {
                            //foreach (GUIObject c in _winClothes.Controls)
                            //{
                            //    rv = c.ProcessLeftButtonClick(mouse);
                            //    if (rv)
                            //    {
                            //        GUIManager.CloseHoverWindow();
                            //        break;
                            //    }
                            //}
                        }
                    }
                }
                return rv;
            }

            public override bool ProcessRightButtonClick(Point mouse)
            {
                bool rv = false;

                if (_equipWindow.HasEntries())
                {
                    _equipWindow.Clear();
                }
                else
                {
                    foreach (SpecializedBox box in _liGearBoxes)
                    {
                        if (box.Contains(mouse) && box.BoxItem != null)
                        {
                            if (!box.WeaponType.Equals(WeaponEnum.None)) { _character.Unequip(GearTypeEnum.Weapon); }
                            else if (!box.ArmorType.Equals(ArmorTypeEnum.None)) { _character.Unequip(GearTypeEnum.Chest); }
                            else if (!box.ClothingType.Equals(ClothingEnum.None))
                            {
                                PlayerManager.PlayerActor.RemoveClothes(((Clothing)box.BoxItem).ClothesType);
                            }

                            InventoryManager.AddToInventory(box.BoxItem);
                            box.SetItem(null);
                            rv = true;
                        }
                    }
                }

                return rv;
            }

            public override bool ProcessHover(Point mouse)
            {
                bool rv = false;
                if (_equipWindow.HasEntries())
                {
                    rv = _equipWindow.ProcessHover(mouse);
                }
                else
                {
                    foreach (SpecializedBox box in _liGearBoxes)
                    {
                        if (box.ProcessHover(mouse))
                        {
                            rv = true;
                        }
                    }

                 //   _gBarXP.ProcessHover(mouse);
                 //   _gBarHP.ProcessHover(mouse);
                }
                return rv;
            }

            public void SetAdventurer(ClassedCombatant c)
            {
                _character = c;
                Load();
            }

            /// <summary>
            /// Delegate method asssigned to the SpecializedItemBoxes
            /// When clicked, the itembox will find matching items int he players inventory.
            /// </summary>
            /// <param name="boxMatch"></param>
            private void FindMatchingItems(SpecializedBox boxMatch)
            {
                GUIManager.CloseHoverWindow();

                List<Item> liItems = new List<Item>();
                foreach (Item i in InventoryManager.PlayerInventory)
                {
                    if (i != null && i.ItemType.Equals(boxMatch.ItemType))
                    {
                        if (boxMatch.ItemType.Equals(ItemEnum.Equipment) && i.CompareType(ItemEnum.Equipment))
                        {
                            if (boxMatch.ArmorType != ArmorTypeEnum.None && ((Equipment)i).ArmorType == boxMatch.ArmorType)
                            {
                                liItems.Add(i);
                            }
                            if (boxMatch.WeaponType != WeaponEnum.None && ((Equipment)i).WeaponType == boxMatch.WeaponType)
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

                _equipWindow.Load(boxMatch, liItems, DisplayStatText);
                _equipWindow.AnchorAndAlignToObject(boxMatch, SideEnum.Right, SideEnum.CenterY);
            }

            private void AssignEquipment(Equipment item)
            {
                _character.Equip(item);
            }
        }

        private class EquipWindow : GUIWindow
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
}
