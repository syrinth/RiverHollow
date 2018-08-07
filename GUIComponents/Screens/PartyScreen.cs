using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.Characters.CombatStuff;
using RiverHollow.GUIObjects;
using RiverHollow.WorldObjects;
using static RiverHollow.WorldObjects.Clothes;
using RiverHollow.GUIComponents.GUIObjects;
using static RiverHollow.GUIObjects.GUIObject;
using static RiverHollow.Game_Managers.GameManager;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Characters.NPCs;
using RiverHollow.Characters;
using static RiverHollow.Game_Managers.GUIObjects.PartyScreen.NPCDisplayBox;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class PartyScreen : GUIScreen
    {
        public static int WIDTH = RiverHollow.ScreenWidth / 3;
        public static int HEIGHT = RiverHollow.ScreenHeight / 3;

        CharacterBox _charBox;
        NPCDisplayBox _selectedBox;
        GUIButton _btnMap;
        PositionMap _map;

        NPCDisplayBox[] _arrDisplayBoxes;

        public PartyScreen()
        {
            _charBox = new CharacterBox(PlayerManager.Combat);
            _charBox.CenterOnScreen();
            AddControl(_charBox);

            _btnMap = new GUIButton("Map", SwitchModes);
            _btnMap.AnchorAndAlignToObject(_charBox, SideEnum.Right, SideEnum.Bottom);

            int partySize = PlayerManager.GetParty().Count;
            _arrDisplayBoxes = new NPCDisplayBox[partySize];

            for(int i = 0; i < partySize; i++)
            {
                if(PlayerManager.GetParty()[i] == PlayerManager.Combat)
                {
                    _arrDisplayBoxes[i] = new PlayerDisplayBox(ChangeSelectedCharacter);
                }
                else
                {
                    CombatAdventurer c = PlayerManager.GetParty()[i];
                    if (c.EligibleNPC != null)
                    {
                        _arrDisplayBoxes[i] = new CharacterDisplayBox(c.EligibleNPC, ChangeSelectedCharacter);
                    }
                    else
                    {
                        _arrDisplayBoxes[i] = new CharacterDisplayBox(c.WorldAdv, ChangeSelectedCharacter);
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
            bool rv = true;
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
                if(box.Character == selectedCharacter)
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
                _map = new PositionMap(_selectedBox.Character, ChangeSelectedCharacter);
                _map.AnchorAndAlignToObject(_arrDisplayBoxes[0], SideEnum.Bottom, SideEnum.Left);
                AddControl(_map);
            }
            else if (_map != null)
            {
                RemoveControl(_map);
                _map = null;
                _charBox = new CharacterBox(_selectedBox.Character);
                _charBox.AnchorAndAlignToObject(_arrDisplayBoxes[0], SideEnum.Bottom, SideEnum.Left);
                AddControl(_charBox);
            }
        }

        private class PositionMap : GUIWindow
        {
            CombatAdventurer _currentCharacter;
            StartPosition _currPosition;
            StartPosition[,] _liStartPositions;

            public delegate void ClickDelegate(CombatAdventurer selectedCharacter);
            private ClickDelegate _delAction;

            public PositionMap(CombatAdventurer adv, ClickDelegate del) : base(BrownWin, (QuestScreen.WIDTH) - (BrownWin.Edge * 2), 16)
            {
                _delAction = del;
                _currentCharacter = adv;
                
                int maxCols = 4;
                int maxRows = 3;

                int size = this.MidWidth() / 4;

                _liStartPositions = new StartPosition[maxCols, maxRows];
                for (int cols = 0; cols < maxCols; cols++)
                {
                    for (int rows = 0; rows < maxRows; rows++)
                    {
                        StartPosition pos = new StartPosition(cols, rows, size);
                        _liStartPositions[cols, rows] = pos;
                        if (cols == 0 && rows == 0)
                        {
                            pos.AnchorToInnerSide(this, SideEnum.TopLeft);
                        }
                        else if (cols == 0)
                        {
                            pos.AnchorAndAlignToObject(_liStartPositions[0, rows - 1], SideEnum.Bottom, SideEnum.Left);
                        }
                        else
                        {
                            pos.AnchorAndAlignToObject(_liStartPositions[cols - 1, rows], SideEnum.Right, SideEnum.Bottom);
                        }
                    }
                }

                SetOccupancy(_currentCharacter);

                this.CenterOnScreen();
                this.Resize();
            }

            public void SetOccupancy(CombatAdventurer currentCharacter)
            {
                foreach (CombatAdventurer c in PlayerManager.GetParty())
                {
                    Vector2 vec = c.StartPos;
                    bool current = (c == currentCharacter);
                    _liStartPositions[(int)vec.X, (int)vec.Y].SetCharacter(c, current);
                    if (current)
                    {
                        _currPosition = _liStartPositions[(int)vec.X, (int)vec.Y];
                    }
                }
            }

            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = false;
                bool charChanged = false;

                if (Contains(mouse))
                {
                    rv = true;
                }

                foreach (StartPosition sp in _liStartPositions)
                {
                    if (sp.Contains(mouse))
                    {
                        if (!sp.Occupied())
                        {
                            rv = true;
                            _currPosition.SetCharacter(null);
                            _currPosition = sp;
                            _currPosition.SetCharacter(_currentCharacter, true);
                            _currentCharacter.SetStartPosition(new Vector2(_currPosition.Col, _currPosition.Row));
                        }
                        else
                        {
                            _currentCharacter = sp.Character;
                            _delAction(_currentCharacter);
                            charChanged = true;
                        }

                        break;
                    }
                }

                if (charChanged)
                {
                    foreach (StartPosition sp in _liStartPositions)
                    {
                        if (sp.Occupied())
                        {
                            if (sp.Character == _currentCharacter)
                            {
                                sp.SetCurrent();
                            }
                            else { sp.SetOccupied(); }
                        }
                    }
                }

                return rv;
            }

            private class StartPosition : GUIImage
            {
                #region Const Rectangles
                Rectangle RECT_OCCUPIED = new Rectangle(32, 80, 16, 16);
                Rectangle RECT_EMPTY = new Rectangle(0, 80, 16, 16);
                Rectangle RECT_CURRENT = new Rectangle(16, 80, 16, 16);
                #endregion

                CombatAdventurer _character;
                public CombatAdventurer Character => _character;
                int _iCol;
                int _iRow;
                public int Col => _iCol;
                public int Row => _iRow;

                public StartPosition(int col, int row, int size) : base(Vector2.Zero, new Rectangle(0, 80, 16, 16), size, size, @"Textures\Dialog")
                {
                    _iCol = col;
                    _iRow = row;
                }

                public void SetCharacter(CombatAdventurer c, bool current = false)
                {
                    _character = c;
                    _sourceRect = (_character != null) ? (current ? RECT_CURRENT : RECT_OCCUPIED) : RECT_EMPTY;
                }

                public void SetCurrent() { _sourceRect = RECT_CURRENT; }
                public void SetOccupied() { _sourceRect = RECT_OCCUPIED; }

                public bool Occupied() { return _character != null; }
            }
        }


        public class NPCDisplayBox : GUIWindow
        {
            public delegate void ClickDelegate(CombatAdventurer selectedCharacter);
            private ClickDelegate _delAction;

            CombatAdventurer _character;
            public CombatAdventurer Character => _character;

            public NPCDisplayBox(ClickDelegate action)
            {
                _winData = GUIWindow.GreyWin;
                _delAction = action;
            }

            public class CharacterDisplayBox : NPCDisplayBox
            {
                GUISprite _sprite;
                public GUISprite Sprite => _sprite;
 
                public CharacterDisplayBox(WorldAdventurer w, ClickDelegate del) : base(del)
                {
                    _character = w.Combat;
                    _sprite = new GUISprite(w.BodySprite);
                    Setup();
                }

                public CharacterDisplayBox(EligibleNPC n, ClickDelegate del) : base(del)
                {
                    _character = n.Combat;
                    _sprite = new GUISprite(n.BodySprite);
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

                public void PlayAnimation(string animation)
                {
                    _sprite.PlayAnimation(animation);
                }

                public override bool ProcessLeftButtonClick(Point mouse)
                {
                    bool rv = false;
                    if (Contains(mouse) && _delAction != null)
                    {
                        _delAction(_character);
                        rv = true;
                    }
                    return rv;
                }
            }

            public class PlayerDisplayBox : NPCDisplayBox
            {
                GUISprite _bodySprite;
                public GUISprite BodySprite => _bodySprite;
                GUISprite _eyeSprite;
                public GUISprite EyeSprite => _eyeSprite;
                GUISprite _hairSprite;
                public GUISprite HairSprite => _hairSprite;
                GUISprite _armSprite;
                public GUISprite ArmSprite => _armSprite;
                GUISprite _hatSprite;
                public GUISprite HatSprite => _hatSprite;
                GUISprite _shirtSprite;
                public GUISprite ShirtSprite => _shirtSprite;

                public PlayerDisplayBox(ClickDelegate action) : base(action)
                {
                    _character = PlayerManager.Combat;
                    Configure();

                    PositionSprites();
                }

                public override void Update(GameTime gameTime)
                {
                    _bodySprite.Update(gameTime);
                    _eyeSprite.Update(gameTime);
                    _hairSprite.Update(gameTime);
                    _armSprite.Update(gameTime);
                    if (_hatSprite != null) { _hatSprite.Update(gameTime); }
                    if (_shirtSprite != null) { _shirtSprite.Update(gameTime); }
                }

                public override bool ProcessLeftButtonClick(Point mouse)
                {
                    bool rv = false;

                    if (Contains(mouse))
                    {
                        _delAction(PlayerManager.Combat);
                        rv = true;
                    }

                    return rv;
                }

                private void Configure()
                {
                    _bodySprite = new GUISprite(PlayerManager.World.BodySprite);
                    _eyeSprite = new GUISprite(PlayerManager.World.EyeSprite);
                    _hairSprite = new GUISprite(PlayerManager.World.HairSprite);
                    _armSprite = new GUISprite(PlayerManager.World.ArmSprite);
                    if (PlayerManager.World.Hat != null) { _hatSprite = new GUISprite(PlayerManager.World.Hat.Sprite); }
                    if (PlayerManager.World.Chest != null) { _shirtSprite = new GUISprite(PlayerManager.World.Chest.Sprite); }

                    _bodySprite.SetScale((int)GameManager.Scale);
                    _eyeSprite.SetScale((int)GameManager.Scale);
                    _hairSprite.SetScale((int)GameManager.Scale);
                    _armSprite.SetScale((int)GameManager.Scale);
                    if (_hatSprite != null) { _hatSprite.SetScale((int)GameManager.Scale); }
                    if (_shirtSprite != null) { _shirtSprite.SetScale((int)GameManager.Scale); }

                    PlayAnimation("IdleDown");
                }

                public override void Position(Vector2 value)
                {
                    base.Position(value);
                    PositionSprites();
                }

                public void PlayAnimation(string animation)
                {
                    _bodySprite.PlayAnimation(animation);
                    _eyeSprite.PlayAnimation(animation);
                    _hairSprite.PlayAnimation(animation);
                    _armSprite.PlayAnimation(animation);
                }

                public void PositionSprites()
                {
                    if (_bodySprite != null)
                    {
                        _bodySprite.CenterOnWindow(this);
                        _bodySprite.AnchorToInnerSide(this, SideEnum.Bottom);

                        Width = _bodySprite.Width + _bodySprite.Width / 3;
                        Height = _bodySprite.Height + (_winData.Edge * 2);
                    }
                    PositionSprite(_eyeSprite);
                    PositionSprite(_hairSprite);
                    PositionSprite(_armSprite);
                    PositionSprite(_eyeSprite);
                    PositionSprite(_hatSprite);
                    PositionSprite(_shirtSprite);

                }
                private void PositionSprite(GUISprite sprite)
                {
                    if (sprite != null)
                    {
                        sprite.CenterOnWindow(this);
                        sprite.AnchorToInnerSide(this, SideEnum.Bottom);
                    }
                }
            }
        }
    }

    public class CharacterBox : GUIObject
    {
        GUIWindow _window;
        CombatAdventurer _character;
        SpriteFont _font;
        Vector2 _size;

        GUIItemBox _chestBox;
        GUIItemBox _hatBox;
        GUIItemBox _weaponBox;
        GUIItemBox _armorBox;

        GUIText _gName, _gClass, _gXP, _gMagic, _gDef, _gDmg, _gHP, _gSpd;
        GUIButton _btnPosition;

        //GUIButton _remove;

        public CharacterBox(CombatAdventurer c, Vector2 position)
        {
            _character = c;
            _font = GameContentManager.GetFont(@"Fonts\Font");
            _window = new GUIWindow(position, GUIWindow.RedWin, RiverHollow.ScreenWidth - 100, 100);

            Load();
        }

        public CharacterBox(CombatAdventurer c)
        {
            _character = c;
            _font = GameContentManager.GetFont(@"Fonts\Font");
            int boxHeight = (QuestScreen.HEIGHT / 4) - (GUIWindow.RedWin.Edge * 2);
            int boxWidth = (QuestScreen.WIDTH) - (GUIWindow.RedWin.Edge * 2);
            _window = new GUIWindow(Vector2.Zero, GUIWindow.RedWin, boxWidth, boxHeight);

            Load();
        }

        private void Load()
        {
            _window.Controls.Clear();
            Width = _window.Width;
            Height = _window.Height;

            string nameLen = "";
            for (int i = 0; i < GameManager.MAX_NAME_LEN; i++) { nameLen += "X"; }

            _gName = new GUIText(nameLen);
            _gName.AnchorToInnerSide(_window, SideEnum.TopLeft);
            _gClass = new GUIText("XXXXXXXX");
            _gClass.AnchorAndAlignToObject(_gName, SideEnum.Right, SideEnum.Bottom, 10);

            _gXP = new GUIText(@"9999/9999");//new GUIText(_character.XP + @"/" + CombatAdventurer.LevelRange[_character.ClassLevel]);
            _gXP.AnchorAndAlignToObject(_gClass, SideEnum.Right, SideEnum.Top, 10);

            _weaponBox = new GUIItemBox(new Rectangle(288, 32, 32, 32), 32, 32, @"Textures\Dialog", _character.Weapon, EquipmentSwap, EquipmentEnum.Weapon);
            _armorBox = new GUIItemBox(new Rectangle(288, 32, 32, 32), 32, 32, @"Textures\Dialog", _character.Armor, EquipmentSwap, EquipmentEnum.Armor);

            _weaponBox.AnchorAndAlignToObject(_gXP, SideEnum.Right, SideEnum.Top, 10);
            _armorBox.AnchorAndAlignToObject(_weaponBox, SideEnum.Right, SideEnum.Bottom, 10);

            if (_character == PlayerManager.Combat)
            {
                _hatBox = new GUIItemBox(new Rectangle(288, 32, 32, 32), 32, 32, @"Textures\Dialog", PlayerManager.World.Hat, ClothesSwap, ClothesEnum.Hat);
                _chestBox = new GUIItemBox(new Rectangle(288, 32, 32, 32), 32, 32, @"Textures\Dialog", PlayerManager.World.Chest, ClothesSwap, ClothesEnum.Chest);

                _hatBox.AnchorAndAlignToObject(_armorBox, SideEnum.Right, SideEnum.Bottom, 30);
                _chestBox.AnchorAndAlignToObject(_hatBox, SideEnum.Right, SideEnum.Bottom, 10);
            }

            int statSpacing = 10;
            _gMagic = new GUIText("Mag: 999");
            _gDef = new GUIText("Def: 999");
            _gDmg = new GUIText("Dmg: 999");
            _gHP = new GUIText("HP: 999");
            _gSpd = new GUIText("Spd: 999");
            _gMagic.AnchorToInnerSide(_window, SideEnum.BottomLeft);
            _gDef.AnchorAndAlignToObject(_gMagic, SideEnum.Right, SideEnum.Bottom, statSpacing);
            _gDmg.AnchorAndAlignToObject(_gDef, SideEnum.Right, SideEnum.Bottom, statSpacing);
            _gHP.AnchorAndAlignToObject(_gDmg, SideEnum.Right, SideEnum.Bottom, statSpacing);
            _gSpd.AnchorAndAlignToObject(_gHP, SideEnum.Right, SideEnum.Bottom, statSpacing);

            _gName.SetText(_character.Name);
            _gClass.SetText(_character.CharacterClass.Name);
            _gXP.SetText(_character.XP + @"/" + CombatAdventurer.LevelRange[_character.ClassLevel]);

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
                _window.Draw(spriteBatch);

                _weaponBox.DrawDescription(spriteBatch);
                _armorBox.DrawDescription(spriteBatch);
            }
        }

        public override void Position(Vector2 value)
        {
            base.Position(value);
            _window.Position(value);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (_character != null)
            {
                foreach(GUIObject c in _window.Controls)
                {
                    rv = c.ProcessLeftButtonClick(mouse);
                    if (rv) { break; }
                }

                //if (_remove != null && _remove.Contains(mouse))
                //{
                //    PlayerManager.RemoveFromParty(_character);
                //    _character.World.DrawIt = true;
                //    _character = null;
                //    rv = true;
                //}
            }
            return rv;
        }

        public bool ProcessHover(Point mouse)
        {
            bool rv = false;
            if (_weaponBox.ProcessHover(mouse))
            {
                rv = true;
            }
            if (_armorBox.ProcessHover(mouse))
            {
                rv = true;
            }
            return rv;
        }
        public override bool Contains(Point mouse)
        {
            return _window.Contains(mouse);
        }

        public void SetAdventurer(CombatAdventurer c)
        {
            _character = c;
            Load();
        }

        //MAR
        private bool EquipmentSwap(GUIItemBox box)
        {
            bool rv = false;
            if (GraphicCursor.HeldItem != null)
            {
                if (GraphicCursor.HeldItem.IsEquipment()) {
                    Equipment equip = (Equipment)GraphicCursor.HeldItem;
                    if (CheckValid(equip, box)) {
                        if (box.Item != null)
                        {
                            Equipment temp = (Equipment)GraphicCursor.HeldItem;
                            GraphicCursor.GrabItem(box.Item);
                            box.SetItem(temp);
                            AssignEquipment(box, temp);
                            rv = true;
                        }
                        else
                        {
                            box.SetItem(GraphicCursor.HeldItem);
                            AssignEquipment(box, (Equipment)GraphicCursor.HeldItem);
                            GraphicCursor.DropItem();
                            rv = true;
                        }
                    }
                }
            }
            else
            {
                rv = GraphicCursor.GrabItem(box.Item);
                box.SetItem(null);
                AssignEquipment(box, null);
            }

            return rv;
        }
        private bool ClothesSwap(GUIItemBox box)
        {
            bool rv = false;
            if (GraphicCursor.HeldItem != null)
            {
                if (GraphicCursor.HeldItem.IsClothes())
                {
                    Clothes theClothes = (Clothes)GraphicCursor.HeldItem;
                    if (theClothes.ClothesType == box.ClothesType)
                    {
                        if (box.Item != null)
                        {
                            Clothes temp = (Clothes)GraphicCursor.HeldItem;
                            GraphicCursor.GrabItem(box.Item);
                            box.SetItem(temp);
                            PlayerManager.World.SetClothes(temp);
                        }
                        else
                        {
                            box.SetItem(GraphicCursor.HeldItem);
                            PlayerManager.World.SetClothes((Clothes)GraphicCursor.HeldItem);
                            GraphicCursor.DropItem();
                        }
                        rv = true;
                    }
                }
            }
            else
            {
                rv = GraphicCursor.GrabItem(box.Item);
                box.SetItem(null);
                PlayerManager.World.RemoveClothes((Clothes)GraphicCursor.HeldItem);
            }

            return rv;
        }

        private void AssignEquipment(GUIItemBox box, Equipment item)
        {
            if (box.EquipType == EquipmentEnum.Weapon) { _character.Weapon = item; }
            else if (box.EquipType == EquipmentEnum.Armor) { _character.Armor = item; }
        }

        private bool CheckValid(Equipment equip, GUIItemBox box)
        {
            bool rv = false;
            if (equip.EquipType == box.EquipType)
            {
                if (box.EquipType == EquipmentEnum.Armor)
                {
                    rv = equip.ArmorType == _character.CharacterClass.ArmorType;
                }
                else if (box.EquipType == EquipmentEnum.Weapon)
                {
                    rv = equip.WeaponType == _character.CharacterClass.WeaponType;
                }
            }

            return rv;
        }

        public bool EquipItem(Item i)
        {
            bool rv = false;
            GUIItemBox gBox = null;

            if (i.IsEquipment())
            {
                Equipment equip = (Equipment)i;
                switch (equip.EquipType) {
                    case EquipmentEnum.Armor:
                        gBox = _armorBox;
                        break;

                    case EquipmentEnum.Weapon:
                        gBox = _weaponBox;
                        break;
                }

                if (gBox != null) {
                    rv = EquipmentSwap(gBox);
                }
            }
            else if (i.IsClothes())
            {
                Clothes equip = (Clothes)i;
                switch (equip.ClothesType)
                {
                    case ClothesEnum.Hat:
                        gBox = _hatBox;
                        break;

                    case ClothesEnum.Chest:
                        gBox = _chestBox;
                        break;
                }

                if (gBox != null) {
                    rv = ClothesSwap(gBox);
                }
            }

            return rv;
        }

        public void AssignNewCharacter(CombatAdventurer c)
        {
            _character = c;
            Load();
        }
    }
}
