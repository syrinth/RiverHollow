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

            _btnMap.AnchorAndAlignToObject(_charBox, SideEnum.Right, SideEnum.Bottom);

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
                GUISprite _sprite;
                public GUISprite Sprite => _sprite;
 
                public CharacterDisplayBox(WorldCombatant w, ClickDelegate del) : base(del)
                {
                    _actor = w.Combat;
                    _sprite = new GUISprite(w.BodySprite, true);
                    Setup();
                }

                public CharacterDisplayBox(EligibleNPC n, ClickDelegate del) : base(del)
                {
                    _actor = n.Combat;
                    _sprite = new GUISprite(n.BodySprite, true);
                    Setup();
                }

                public void Setup()
                {
                    _sprite.SetScale((int)GameManager.Scale);

                    Position(Vector2.Zero);
                    Width = _sprite.Width + _sprite.Width / 4;
                    Height = _sprite.Height + (_winData.Edge * 2);
                    _sprite.CenterOnWindow(this);
                    _sprite.AnchorToInnerSide(this, SideEnum.Bottom);

                    PlayAnimation(WActorAnimEnum.IdleDown);
                }

                public override void Update(GameTime gameTime)
                {
                    _sprite.Update(gameTime);
                }

                public override void Draw(SpriteBatch spriteBatch)
                {
                    base.Draw(spriteBatch);
                    _sprite.Draw(spriteBatch);
                }

                public override void Position(Vector2 value)
                {
                    base.Position(value);
                    if (_sprite != null)
                    {
                        _sprite.CenterOnWindow(this);
                        _sprite.AnchorToInnerSide(this, SideEnum.Bottom);
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
                    _playerSprite.PlayAnimation(WActorAnimEnum.IdleDown);

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

    public class CharacterDetailWindow : GUIWindow
    {
        EquipWindow _equipWindow;

        CombatAdventurer _character;
        SpriteFont _font;

        List<SpecializedBox> _liGearBoxes;

        SpecializedBox _chestBox;
        SpecializedBox _hatBox;
        SpecializedBox _weaponBox;
        SpecializedBox _armorBox;

        GUIText _gName, _gClass, _gXP, _gMagic, _gDef, _gDmg, _gHP, _gSpd;

        public delegate void SyncCharacter();
        private SyncCharacter _delSyncCharacter;

        public CharacterDetailWindow(CombatAdventurer c, SyncCharacter del = null) : base(GUIWindow.RedWin, (QuestScreen.WIDTH) - (GUIWindow.RedWin.Edge * 2), (QuestScreen.HEIGHT / 4) - (GUIWindow.RedWin.Edge * 2))
        {
            _delSyncCharacter = del;
            _character = c;
            _font = GameContentManager.GetFont(@"Fonts\Font");
            int boxHeight = (QuestScreen.HEIGHT / 4) - (GUIWindow.RedWin.Edge * 2);
            int boxWidth = (QuestScreen.WIDTH) - (GUIWindow.RedWin.Edge * 2);

            _liGearBoxes = new List<SpecializedBox>();
            Load();
        }

        private void Load()
        {
            Controls.Clear();
            _liGearBoxes.Clear();

            string nameLen = "";
            for (int i = 0; i < GameManager.MAX_NAME_LEN; i++) { nameLen += "X"; }

            _gName = new GUIText(nameLen);
            _gName.AnchorToInnerSide(this, SideEnum.TopLeft);
            _gClass = new GUIText("XXXXXXXX");
            _gClass.AnchorAndAlignToObject(_gName, SideEnum.Right, SideEnum.Bottom, 10);

            _gXP = new GUIText(@"9999/9999");
            _gXP.AnchorAndAlignToObject(_gClass, SideEnum.Right, SideEnum.Top, 10);

            _weaponBox = new SpecializedBox(_character.CharacterClass.WeaponType, _character.Weapon, FindMatchingItems);
            _armorBox = new SpecializedBox(_character.CharacterClass.ArmorType, _character.Armor, FindMatchingItems);

            _weaponBox.AnchorAndAlignToObject(_gXP, SideEnum.Right, SideEnum.Top, 10);
            _armorBox.AnchorAndAlignToObject(_weaponBox, SideEnum.Right, SideEnum.Bottom, 10);

            _liGearBoxes.Add(_weaponBox);
            _liGearBoxes.Add(_armorBox);

            if (_character == PlayerManager.Combat)
            {
                _hatBox = new SpecializedBox(ClothesEnum.Hat, PlayerManager.World.Hat, FindMatchingItems);
                _chestBox = new SpecializedBox(ClothesEnum.Chest, PlayerManager.World.Shirt, FindMatchingItems);

                _hatBox.AnchorAndAlignToObject(_armorBox, SideEnum.Right, SideEnum.Bottom, 30);
                _chestBox.AnchorAndAlignToObject(_hatBox, SideEnum.Right, SideEnum.Bottom, 10);

                _liGearBoxes.Add(_hatBox);
                _liGearBoxes.Add(_chestBox);
            }


            int statSpacing = 10;
            _gMagic = new GUIText("Mag: 999");
            _gDef = new GUIText("Def: 999");
            _gDmg = new GUIText("Dmg: 999");
            _gHP = new GUIText("HP: 999");
            _gSpd = new GUIText("Spd: 999");
            _gMagic.AnchorToInnerSide(this, SideEnum.BottomLeft);
            _gDef.AnchorAndAlignToObject(_gMagic, SideEnum.Right, SideEnum.Bottom, statSpacing);
            _gDmg.AnchorAndAlignToObject(_gDef, SideEnum.Right, SideEnum.Bottom, statSpacing);
            _gHP.AnchorAndAlignToObject(_gDmg, SideEnum.Right, SideEnum.Bottom, statSpacing);
            _gSpd.AnchorAndAlignToObject(_gHP, SideEnum.Right, SideEnum.Bottom, statSpacing);

            _gName.SetText(_character.Name);
            _gClass.SetText(_character.CharacterClass.Name);
            _gXP.SetText(_character.XP + @"/" + CombatAdventurer.LevelRange[_character.ClassLevel]);

            DisplayStatText();

            _equipWindow = new EquipWindow();
            AddControl(_equipWindow);
        }

        public void DisplayStatText(Equipment tempGear = null)
        {
            if (tempGear != null)
            {
                if (tempGear.WeaponType != WeaponEnum.None) { _character.TempWeapon = tempGear; }
                else if (tempGear.ArmorType != ArmorEnum.None) { _character.TempArmor = tempGear; }
            }
            else
            {
                _character.TempWeapon = null;
                _character.TempArmor = null;
            }

            _gDmg.SetText("Dmg: " + _character.StatStr);
            _gDef.SetText("Def: " + _character.StatDef);
            _gHP.SetText("Vit: " + _character.StatVit);
            _gMagic.SetText("Mag: " + _character.StatMag);
            _gSpd.SetText("Spd: " + _character.StatSpd);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_character != null)
            {
                base.Draw(spriteBatch);

                _weaponBox.DrawDescription(spriteBatch);
                _armorBox.DrawDescription(spriteBatch);

                if (_equipWindow.HasEntries())
                {
                    _equipWindow.Draw(spriteBatch);
                }
            }
        }

        public override void Position(Vector2 value)
        {
            base.Position(value);
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

                    _equipWindow.Clear();
                }
                else
                {
                    foreach (GUIObject c in Controls)
                    {
                        rv = c.ProcessLeftButtonClick(mouse);
                        if (rv) { break; }
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
            }
            return rv;
        }

        public void SetAdventurer(CombatAdventurer c)
        {
            _character = c;
            Load();
        }

        private void FindMatchingItems(SpecializedBox boxMatch)
        {
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
