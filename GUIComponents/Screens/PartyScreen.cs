using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.GUIObjects;
using RiverHollow.WorldObjects;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Actors;
using System.Collections.Generic;

using static RiverHollow.WorldObjects.Item;
using static RiverHollow.GUIObjects.GUIObject;
using static RiverHollow.WorldObjects.Clothes;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Game_Managers.GUIObjects.PartyScreen.NPCDisplayBox;
using static RiverHollow.Game_Managers.GUIComponents.GUIObjects.GUIItemBox;
namespace RiverHollow.Game_Managers.GUIObjects
{
    public class PartyScreen : GUIScreen
    {
        public static int WIDTH = RiverHollow.ScreenWidth / 3;
        public static int HEIGHT = RiverHollow.ScreenHeight / 3;

        CharacterDetailWindow _charBox;
        NPCDisplayBox _selectedBox;
        GUIButton _btnMap;
        PositionMap _map;

        NPCDisplayBox[] _arrDisplayBoxes;

        public PartyScreen()
        {
            _btnMap = new GUIButton("Map", SwitchModes);
            AddControl(_btnMap);

            _charBox = new CharacterDetailWindow(PlayerManager.Combat, SyncCharacter);
            _charBox.CenterOnScreen();
            AddControl(_charBox);

            _btnMap.AnchorAndAlignToObject(_charBox.WinDisplay, SideEnum.Right, SideEnum.Top);

            int partySize = PlayerManager.GetParty().Count;
            _arrDisplayBoxes = new NPCDisplayBox[partySize];

            for(int i = 0; i < partySize; i++)
            {
                if(PlayerManager.GetParty()[i] == PlayerManager.Combat)
                {
                    _arrDisplayBoxes[i] = new PlayerDisplayBox(true, ChangeSelectedCharacter);
                }
                else
                {
                    CombatAdventurer c = PlayerManager.GetParty()[i];
                    if (c.World != null)
                    {
                        _arrDisplayBoxes[i] = new CharacterDisplayBox(c.World, ChangeSelectedCharacter);
                    }
                }

                _arrDisplayBoxes[i].Enable(false);

                if (i == 0)
                {
                    _arrDisplayBoxes[i].AnchorAndAlignToObject(_charBox, SideEnum.Top, SideEnum.Left);
                }
                else
                {
                    _arrDisplayBoxes[i].AnchorAndAlignToObject(_arrDisplayBoxes[i- 1], SideEnum.Right, SideEnum.Bottom);
                }

            }
            _selectedBox = _arrDisplayBoxes[0];
            _selectedBox.Enable(true);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            foreach(GUIObject o in Controls)
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
            if(_charBox != null)
            {
                rv = _charBox.ProcessRightButtonClick(mouse);
            }
            return rv;
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = true;

            if (_charBox != null)
            {
                rv = _charBox.ProcessHover(mouse);
            }

            return rv;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public void UpdateCharacterBox(CombatAdventurer displayCharacter)
        {
            _charBox.SetAdventurer(displayCharacter);
        }

        public void ChangeSelectedCharacter(CombatAdventurer selectedCharacter)
        {
            _selectedBox.Enable(false);
            if (_charBox != null)
            {
                _charBox.SetAdventurer(selectedCharacter);
            }
            else if(_map != null)
            {
                _map.SetOccupancy(selectedCharacter);
            }

            foreach(NPCDisplayBox box in _arrDisplayBoxes)
            {
                if(box.Actor == selectedCharacter)
                {
                    _selectedBox = box;
                    break;
                }
            }

            _selectedBox.Enable(true);
        }

        public void SwitchModes()
        {
            if(_charBox != null)
            {
                RemoveControl(_charBox);
                _charBox = null;
                _map = new PositionMap(_selectedBox.Actor, ChangeSelectedCharacter);
                _map.AnchorAndAlignToObject(_arrDisplayBoxes[0], SideEnum.Bottom, SideEnum.Left);
                AddControl(_map);
            }
            else if (_map != null)
            {
                RemoveControl(_map);
                _map = null;
                _charBox = new CharacterDetailWindow(_selectedBox.Actor, SyncCharacter);
                _charBox.AnchorAndAlignToObject(_arrDisplayBoxes[0], SideEnum.Bottom, SideEnum.Left);
                AddControl(_charBox);
            }
        }

        public void SyncCharacter()
        {
            ((PlayerDisplayBox)_arrDisplayBoxes[0]).Configure();
            ((PlayerDisplayBox)_arrDisplayBoxes[0]).PositionSprites();
        }

        private class PositionMap : GUIWindow
        {
            CombatAdventurer _currentCharacter;
            StartPosition _currPosition;
            StartPosition[,] _arrStartPositions;

            public delegate void ClickDelegate(CombatAdventurer selectedCharacter);
            private ClickDelegate _delAction;

            public PositionMap(CombatAdventurer adv, ClickDelegate del) : base(BrownWin, 16, 16)
            {
                _delAction = del;
                _currentCharacter = adv;
                
                int maxCols = 4;
                int maxRows = 3;

                int spacing = 10;
                int totalSpaceCol = (maxCols + 1) * spacing;
                int totalSpaceRow = (maxRows + 1) * spacing;
                _arrStartPositions = new StartPosition[maxCols, maxRows];
                for (int cols = 0; cols < maxCols; cols++)
                {
                    for (int rows = 0; rows < maxRows; rows++)
                    {
                        StartPosition pos = new StartPosition(cols, rows);
                        _arrStartPositions[cols, rows] = pos;
                        if (cols == 0 && rows == 0)
                        {
                            pos.AnchorToInnerSide(this, SideEnum.TopLeft, spacing);
                        }
                        else if (cols == 0)
                        {
                            pos.AnchorAndAlignToObject(_arrStartPositions[0, rows - 1], SideEnum.Bottom, SideEnum.Left, spacing);
                        }
                        else
                        {
                            pos.AnchorAndAlignToObject(_arrStartPositions[cols - 1, rows], SideEnum.Right, SideEnum.Bottom, spacing);
                        }
                    }
                }

                SetOccupancy(_currentCharacter);

                this.Resize();
                this.IncreaseSizeTo((QuestScreen.WIDTH) - (BrownWin.Edge * 2), (QuestScreen.HEIGHT) - (BrownWin.Edge * 2));
            }

            public void SetOccupancy(CombatAdventurer currentCharacter)
            {
                _currentCharacter = currentCharacter;
                foreach (CombatAdventurer c in PlayerManager.GetParty())
                {
                    Vector2 vec = c.StartPos;
                    bool current = (c == currentCharacter);
                    _arrStartPositions[(int)vec.X, (int)vec.Y].SetCharacter(c);
                    if (current)
                    {
                        _currPosition = _arrStartPositions[(int)vec.X, (int)vec.Y];
                    }
                }
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
                    if (sp.Contains(mouse))
                    {
                        if (!sp.Occupied())
                        {
                            rv = true;
                            _currPosition.SetCharacter(null);
                            _currPosition = sp;
                            _currPosition.SetCharacter(_currentCharacter);
                            _currentCharacter.SetStartPosition(new Vector2(_currPosition.Col, _currPosition.Row));
                        }
                        else
                        {
                            _currentCharacter = sp.Character;
                            _delAction(_currentCharacter);
                        }

                        break;
                    }
                }

                return rv;
            }

            private class StartPosition : GUIImage
            {
                CombatAdventurer _character;
                public CombatAdventurer Character => _character;
                int _iCol;
                int _iRow;
                public int Col => _iCol;
                public int Row => _iRow;

                private GUICharacterSprite _sprite;

                public StartPosition(int col, int row) : base(new Rectangle(0, 80, 16, 16), TileSize,  TileSize, @"Textures\Dialog")
                {
                    _iCol = col;
                    _iRow = row;

                    SetScale(Scale);
                }

                public override void Draw(SpriteBatch spriteBatch)
                {
                    base.Draw(spriteBatch);
                    if(_sprite != null)
                    {
                        _sprite.Draw(spriteBatch);
                    }
                }

                public void SetCharacter(CombatAdventurer c)
                {
                    _character = c;
                    if (c != null)
                    {
                        if (c.World != null)
                        {
                            if(c == PlayerManager.Combat) { _sprite = new GUICharacterSprite(true); }
                            else { _sprite = new GUICharacterSprite(c.World.BodySprite, true); }
                            
                            _sprite.SetScale(2);
                            _sprite.PlayAnimation(WActorBaseAnim.IdleDown);
                            _sprite.CenterOnObject(this);
                            _sprite.MoveBy(new Vector2(0, -(this.Width/4)));
                        }         
                    }
                    else
                    {
                        _sprite = null;
                    }
                }

                public bool Occupied() { return _character != null; }

                public override void Position(Vector2 value)
                {
                    base.Position(value);
                    if (_sprite != null) {
                        _sprite.CenterOnObject(this);
                        _sprite.MoveBy(new Vector2(0, -(this.Width / 4)));
                    }
                }
            }
        }

        public class NPCDisplayBox : GUIWindow
        {
            public delegate void ClickDelegate(CombatAdventurer selectedCharacter);
            private ClickDelegate _delAction;

            CombatAdventurer _actor;
            public CombatAdventurer Actor => _actor;

            public NPCDisplayBox(ClickDelegate action = null)
            {
                _winData = GUIWindow.GreyWin;
                _delAction = action;
            }

            public virtual void PlayAnimation<TEnum>(TEnum animation)
            {

            }

            public class CharacterDisplayBox : NPCDisplayBox
            {
                public WorldAdventurer WorldAdv;
                GUISprite _sprite;
                public GUISprite Sprite => _sprite;

                public CharacterDisplayBox(WorldCombatant w, ClickDelegate del) : base(del)
                {
                    if (w != null)
                    {
                        _actor = w.Combat;
                        _sprite = new GUISprite(w.BodySprite, true);
                    }
                    Setup();
                }

                public CharacterDisplayBox(EligibleNPC n, ClickDelegate del) : base(del)
                {
                    if (n != null)
                    {
                        _actor = n.Combat;
                        _sprite = new GUISprite(n.BodySprite, true);
                    }
                    Setup();
                }

                public void AssignToBox(WorldAdventurer adv)
                {
                    if (adv != null)
                    {
                        WorldAdv = adv;
                        _actor = adv.Combat;
                        _sprite = new GUISprite(adv.BodySprite, true);

                        _sprite.SetScale((int)GameManager.Scale);
                        _sprite.CenterOnWindow(this);
                        _sprite.AnchorToInnerSide(this, SideEnum.Bottom);

                        PlayAnimation(WActorBaseAnim.IdleDown);
                    }
                    else
                    {
                        RemoveControl(_sprite);
                        WorldAdv = null;
                        _actor = null;
                        _sprite = null;
                    }
                }

                public void Setup()
                {
                    Width = ((int)Scale * TileSize) + ((int)Scale * TileSize) / 4;
                    Height = (int)Scale * ((TileSize * 2) + 2) + (_winData.Edge * 2);

                    if (_actor != null)
                    {
                        _sprite.SetScale((int)GameManager.Scale);
                        _sprite.CenterOnWindow(this);
                        _sprite.AnchorToInnerSide(this, SideEnum.Bottom);

                        PlayAnimation(WActorBaseAnim.IdleDown);
                    }
                }

                public override void Update(GameTime gameTime)
                {
                    if (_sprite != null)
                    {
                        _sprite.Update(gameTime);
                    }
                }

                public override void PlayAnimation<TEnum>(TEnum animation)
                {
                    _sprite.PlayAnimation(animation);
                }

                public override bool ProcessLeftButtonClick(Point mouse)
                {
                    bool rv = false;
                    if (Contains(mouse) && _delAction != null)
                    {
                        _delAction(_actor);
                        rv = true;
                    }
                    return rv;
                }

                public class ClassSelectionBox : CharacterDisplayBox
                {
                    private ClickDelegate _delAction;
                    public new delegate void ClickDelegate(ClassSelectionBox o);

                    private int _iClassID;
                    public int ClassID => _iClassID;

                    public ClassSelectionBox(WorldAdventurer w, ClickDelegate del) : base(w, null)
                    {
                        _iClassID = w.Combat.CharacterClass.ID;
                        _delAction = del;
                    }

                    public override bool ProcessLeftButtonClick(Point mouse)
                    {
                        bool rv = false;
                        if (Contains(mouse) && _delAction != null)
                        {
                            _delAction(this);
                            rv = true;
                        }
                        return rv;
                    }
                }
            }

            public class PlayerDisplayBox : NPCDisplayBox
            {
                GUICharacterSprite _playerSprite;
                public GUICharacterSprite PlayerSprite => _playerSprite;

                bool _bOverwrite = false;
                
                public PlayerDisplayBox(bool overwrite = false, ClickDelegate action = null) : base(action)
                {
                    _bOverwrite = overwrite;
                    _actor = PlayerManager.Combat;
                    Configure();
                }

                public override void Update(GameTime gameTime)
                {
                    _playerSprite.Update(gameTime);
                }

                public override bool ProcessLeftButtonClick(Point mouse)
                {
                    bool rv = false;

                    if (Contains(mouse) && _delAction != null)
                    {
                        _delAction(PlayerManager.Combat);
                        rv = true;
                    }

                    return rv;
                }

                public void Configure()
                {
                    Controls.Clear();
                    _playerSprite = new GUICharacterSprite(_bOverwrite);
                    _playerSprite.SetScale((int)GameManager.Scale);
                    _playerSprite.PlayAnimation(WActorBaseAnim.IdleDown);

                    PositionSprites();
                }

                public override void Position(Vector2 value)
                {
                    base.Position(value);
                    PositionSprites();
                }

                public void PositionSprites()
                {
                    if (_playerSprite != null)
                    {
                        _playerSprite.CenterOnWindow(this);
                        _playerSprite.AnchorToInnerSide(this, SideEnum.Bottom);

                        Width = _playerSprite.Width + _playerSprite.Width / 3;
                        Height = _playerSprite.Height + (_winData.Edge * 2);
                    }
                }
            }
        }
    }

    public class CharacterDetailWindow : GUIObject
    {
        const int SPACING = 10;
        EquipWindow _equipWindow;

        CombatAdventurer _character;
        SpriteFont _font;

        List<SpecializedBox> _liGearBoxes;

        GUIWindow _winName;
        public GUIWindow WinDisplay;
        GUIWindow _winClothes;

        SpecializedBox _sBoxArmor;
        SpecializedBox _sBoxHead;
        SpecializedBox _sBoxWeapon;
        SpecializedBox _sBoxWrist;
        SpecializedBox _sBoxShirt;
        SpecializedBox _sBoxHat;
        GUIButton _btnSwap;

        GUIText _gName, _gClass, _gLvl, _gStr, _gDef, _gMagic, _gRes, _gSpd;
        GUIStatDisplay _gBarXP, _gBarHP, _gBarMP;

        public delegate void SyncCharacter();
        private SyncCharacter _delSyncCharacter;

        public CharacterDetailWindow(CombatAdventurer c, SyncCharacter del = null)
        {
            _winName = new GUIWindow(GUIWindow.RedWin, (QuestScreen.WIDTH) - (GUIWindow.RedWin.Edge * 2), 10);
            WinDisplay = new GUIWindow(GUIWindow.RedWin, (QuestScreen.WIDTH) - (GUIWindow.RedWin.Edge * 2), (QuestScreen.HEIGHT / 4) - (GUIWindow.RedWin.Edge * 2));
            WinDisplay.AnchorAndAlignToObject(_winName, SideEnum.Bottom, SideEnum.Left);
            _winClothes = new GUIWindow(GUIWindow.RedWin, 10, 10);
            _winClothes.AnchorAndAlignToObject(WinDisplay, SideEnum.Bottom, SideEnum.Left);

            _delSyncCharacter = del;
            _character = c;
            _font = GameContentManager.GetFont(@"Fonts\Font");

            _liGearBoxes = new List<SpecializedBox>();
            Load();

            _winName.Resize();
            _winName.Height += SPACING;

            WinDisplay.Resize();
            WinDisplay.Height += SPACING;

            _winClothes.Resize();
            _winClothes.Height += SPACING;
            _winClothes.Width += SPACING;

            Width = _winName.Width;
            Height = _winName.Height + WinDisplay.Height;
        }

        private void Load()
        {
            _winClothes.Controls.Clear();
            _winName.Controls.Clear();
            WinDisplay.Controls.Clear();
            
            _liGearBoxes.Clear();

            string nameLen = "";
            for (int i = 0; i < GameManager.MAX_NAME_LEN; i++) { nameLen += "X"; }

            _gName = new GUIText(nameLen);
            _gName.AnchorToInnerSide(_winName, SideEnum.TopLeft, SPACING);
            _gClass = new GUIText("XXXXXXXX");
            _gClass.AnchorAndAlignToObject(_gName, SideEnum.Right, SideEnum.Bottom, 10);

            _sBoxHead = new SpecializedBox(_character.CharacterClass.ArmorType, _character.Head, FindMatchingItems);
            _sBoxArmor = new SpecializedBox(_character.CharacterClass.ArmorType, _character.Armor, FindMatchingItems);
            _sBoxWeapon = new SpecializedBox(_character.CharacterClass.WeaponType, _character.Weapon, FindMatchingItems);
            _sBoxWrist = new SpecializedBox(_character.CharacterClass.ArmorType, _character.Wrist, FindMatchingItems);

            _sBoxArmor.AnchorToInnerSide(WinDisplay, SideEnum.TopRight, SPACING);
            _sBoxHead.AnchorAndAlignToObject(_sBoxArmor, SideEnum.Left, SideEnum.Top, SPACING);
            _sBoxWrist.AnchorAndAlignToObject(_sBoxArmor, SideEnum.Bottom, SideEnum.Right, SPACING);
            _sBoxWeapon.AnchorAndAlignToObject(_sBoxWrist, SideEnum.Left, SideEnum.Top, SPACING);

            _liGearBoxes.Add(_sBoxHead);
            _liGearBoxes.Add(_sBoxArmor);
            _liGearBoxes.Add(_sBoxWeapon);
            _liGearBoxes.Add(_sBoxWrist);

            int barWidth = _sBoxArmor.DrawRectangle.Right - _sBoxHead.DrawRectangle.Left;
            _gBarXP = new GUIStatDisplay(GUIStatDisplay.DisplayEnum.XP, _character, barWidth);
            _gBarXP.AnchorToInnerSide(_winName, SideEnum.Right, SPACING);
            _gBarXP.AlignToObject(_gName, SideEnum.CenterY);

            _gLvl = new GUIText("LV. X");
            _gLvl.AnchorAndAlignToObject(_gBarXP, SideEnum.Left, SideEnum.CenterY, SPACING);
            _gLvl.SetText("LV. " + _character.ClassLevel);

            _gBarHP = new GUIStatDisplay(GUIStatDisplay.DisplayEnum.Health, _character, barWidth);
            _gBarHP.AnchorAndAlignToObject(_sBoxHead, SideEnum.Left, SideEnum.Top, SPACING);
            _gBarMP = new GUIStatDisplay(GUIStatDisplay.DisplayEnum.Mana, _character, barWidth);
            _gBarMP.AnchorAndAlignToObject(_gBarHP, SideEnum.Bottom, SideEnum.Right, SPACING);

            if (_character == PlayerManager.Combat)
            {
                _sBoxHat = new SpecializedBox(ClothesEnum.Hat, PlayerManager.World.Hat, FindMatchingItems);
                _sBoxShirt = new SpecializedBox(ClothesEnum.Chest, PlayerManager.World.Shirt, FindMatchingItems);

                _sBoxHat.AnchorToInnerSide(_winClothes, SideEnum.TopLeft, SPACING);
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

            _gName.SetText(_character.Name);
            _gClass.SetText(_character.CharacterClass.Name);

            DisplayStatText();

            _equipWindow = new EquipWindow();
            WinDisplay.AddControl(_equipWindow);
            _winClothes.AddControl(_equipWindow);
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
                if (tempGear.WeaponType != WeaponEnum.None) { _character.TempWeapon = tempGear; }
                else if (tempGear.ArmorType != ArmorEnum.None) { _character.TempArmor = tempGear; }
                else
                {
                    compareTemp = false;
                }
            }
            else
            {
                compareTemp = false;
                _character.TempWeapon = null;
                _character.TempArmor = null;
            }

            AssignStatText(_gStr, "Str", _character.StatStr, _character.TempStatStr, compareTemp);
            AssignStatText(_gDef, "Def", _character.StatDef, _character.TempStatDef, compareTemp);
            AssignStatText(_gMagic, "Mag", _character.StatMag, _character.TempStatMag, compareTemp);
            AssignStatText(_gRes, "Res", _character.StatRes, _character.TempStatRes, compareTemp);
            AssignStatText(_gSpd, "Spd", _character.StatSpd, _character.TempStatSpd, compareTemp);
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
                if (_character == PlayerManager.Combat) { _winClothes.Draw(spriteBatch); }

                if (_equipWindow.HasEntries())
                {
                    _equipWindow.Draw(spriteBatch);
                }
            }
        }

        public override void Position(Vector2 value)
        {
            base.Position(value);
            _winName.Position(value);
            WinDisplay.AnchorAndAlignToObject(_winName, SideEnum.Bottom, SideEnum.Left);
            _winClothes.AnchorAndAlignToObject(WinDisplay, SideEnum.Bottom, SideEnum.Left);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (_character != null)
            {
                if (_equipWindow.HasEntries() && _equipWindow.ProcessLeftButtonClick(mouse))
                {
                    Item olditem = _equipWindow.Box.Item;

                    _equipWindow.Box.SetItem(_equipWindow.SelectedItem);
                    if (_equipWindow.Box.ItemType.Equals(ItemEnum.Equipment))
                    {
                        AssignEquipment((Equipment)_equipWindow.SelectedItem);
                    }
                    else if (_equipWindow.Box.ItemType.Equals(ItemEnum.Clothes))
                    {
                        PlayerManager.World.SetClothes((Clothes)_equipWindow.SelectedItem);
                        _delSyncCharacter();
                    }

                    InventoryManager.RemoveItemFromInventory(_equipWindow.SelectedItem);
                    if (olditem != null) { InventoryManager.AddItemToInventory(olditem); }

                    DisplayStatText();
                    GUIManager.CloseHoverWindow();
                    _equipWindow.Clear();
                }
                else
                {
                    foreach (GUIObject c in WinDisplay.Controls)
                    {
                        rv = c.ProcessLeftButtonClick(mouse);
                        if (rv) {
                            GUIManager.CloseHoverWindow();
                            break;
                        }
                    }

                    if (!rv && _character == PlayerManager.Combat)
                    {
                        foreach (GUIObject c in _winClothes.Controls)
                        {
                            rv = c.ProcessLeftButtonClick(mouse);
                            if (rv) {
                                GUIManager.CloseHoverWindow();
                                break;
                            }
                        }
                    }
                }
            }
            return rv;
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = false;

            if (_equipWindow.HasEntries()) {
                _equipWindow.Clear();
            }
            else
            {
                foreach(SpecializedBox box in _liGearBoxes)
                {
                    if (box.Contains(mouse) && box.Item != null)
                    {
                        if (!box.WeaponType.Equals(WeaponEnum.None)) { _character.Weapon = null; }
                        else if (!box.ArmorType.Equals(ArmorEnum.None)) { _character.Armor = null; }
                        else if (!box.ClothingType.Equals(ClothesEnum.None))
                        {
                            PlayerManager.World.RemoveClothes(((Clothes)box.Item).ClothesType);
                            _delSyncCharacter();
                        }

                        InventoryManager.AddItemToInventory(box.Item);
                        box.SetItem(null);
                        rv = true;
                    }
                }
            }

            return rv;
        }

        public bool ProcessHover(Point mouse)
        {
            bool rv = false;
            if (_equipWindow.HasEntries()) {
                rv = _equipWindow.ProcessHover(mouse);
            }
            else
            {
                foreach(SpecializedBox box in _liGearBoxes)
                {
                    if (box.ProcessHover(mouse))
                    {
                        rv = true;
                    }
                }

                _gBarXP.ProcessHover(mouse);
                _gBarHP.ProcessHover(mouse);
                _gBarMP.ProcessHover(mouse);
            }
            return rv;
        }

        public void SetAdventurer(CombatAdventurer c)
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
                    if (boxMatch.ItemType.Equals(ItemEnum.Equipment) && i.IsEquipment())
                    {
                        if (boxMatch.ArmorType != ArmorEnum.None && ((Equipment)i).ArmorType == boxMatch.ArmorType)
                        {
                            liItems.Add(i);
                        }
                        if (boxMatch.WeaponType != WeaponEnum.None && ((Equipment)i).WeaponType == boxMatch.WeaponType)
                        {
                            liItems.Add(i);
                        }
                    }
                    else if (boxMatch.ItemType.Equals(ItemEnum.Clothes) && i.IsClothes())
                    {
                        if (boxMatch.ClothingType != ClothesEnum.None && ((Clothes)i).ClothesType == boxMatch.ClothingType)
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
            if (item.WeaponType != WeaponEnum.None) { _character.Weapon = item; }
            else if (item.ArmorType != ArmorEnum.None) { _character.Armor = item; }
        }
    }

    public class EquipWindow : GUIWindow
    {
        List<GUIItemBox> _gItemBoxes;
        Item _selectedItem;
        public Item SelectedItem => _selectedItem;

        ItemEnum _itemType;

        SpecializedBox _box;
        public SpecializedBox Box => _box;

        public delegate void DisplayEQ(Equipment test);
        private DisplayEQ _delDisplayEQ;

        public EquipWindow() : base(BrownWin, 20, 20)
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
            foreach(GUIItemBox g in _gItemBoxes)
            {
                if (g.Contains(mouse))
                {
                    _selectedItem = g.Item;
                    rv = true;
                    break;
                }
            }
            return rv;
        }
        public bool ProcessHover(Point mouse)
        {
            bool rv = false;

            Equipment temp = null;
            foreach (GUIItemBox box in _gItemBoxes)
            {
                rv = box.ProcessHover(mouse);
                if (rv && _itemType.Equals(ItemEnum.Equipment))
                {
                    temp = (Equipment)box.Item;
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
            if (HasEntries()) {
                base.Draw(spriteBatch);
            }
        }
        public bool HasEntries() { return _gItemBoxes.Count > 0; }

        public void Clear() {
            Controls.Clear();
            _gItemBoxes.Clear();
        }
    }
}
