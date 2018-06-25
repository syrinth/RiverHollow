using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.Characters.CombatStuff;
using System.Collections.Generic;
using RiverHollow.GUIObjects;
using RiverHollow.WorldObjects;
using static RiverHollow.WorldObjects.Equipment;
using static RiverHollow.WorldObjects.Clothes;
using RiverHollow.GUIComponents.GUIObjects;
using static RiverHollow.GUIObjects.GUIObject;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class PartyScreen : GUIScreen
    {
        public static int WIDTH = RiverHollow.ScreenWidth / 3;
        public static int HEIGHT = RiverHollow.ScreenHeight / 3;
        List<CharacterBox> _partyList;
        GUIWindow _partyWindow;
        public PartyScreen()
        {
            _partyList = new List<CharacterBox>();
            _partyWindow = new GUIWindow(new Vector2(WIDTH, HEIGHT), GUIWindow.RedWin, WIDTH, HEIGHT);
            AddControl(_partyWindow);
            for(int i =0; i < PlayerManager.GetParty().Count; i++)
            {
                CharacterBox cb = new CharacterBox(PlayerManager.GetParty()[i], _partyWindow);
                
                if (i == 0) { cb.AnchorToInnerSide(_partyWindow, SideEnum.TopLeft); }
                else { cb.AnchorAndAlignToObject(_partyList[i-1], SideEnum.Bottom, SideEnum.Left); }

                _partyList.Add(cb);
            }
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            foreach(CharacterBox c in _partyList)
            {
                rv = c.ProcessLeftButtonClick(mouse);
                if (rv) { break; }
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
            foreach(CharacterBox c in _partyList)
            {
                rv = c.ProcessHover(mouse);
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
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
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

        GUIButton _remove;

        public CharacterBox(CombatAdventurer c, Vector2 position)
        {
            _character = c;
            _font = GameContentManager.GetFont(@"Fonts\Font");
            _window = new GUIWindow(position, GUIWindow.RedWin, RiverHollow.ScreenWidth - 100, 100);

            Load();
        }

        public CharacterBox(CombatAdventurer c, GUIWindow win)
        {
            _character = c;
            _font = GameContentManager.GetFont(@"Fonts\Font");

            int boxHeight = (QuestScreen.HEIGHT / 4) - (win.EdgeSize * 2);
            int boxWidth = (QuestScreen.WIDTH) - (win.EdgeSize * 2);
            _window = new GUIWindow(Vector2.Zero, GUIWindow.RedWin, boxWidth, boxHeight);

            Load();
            
            //if (_character != PlayerManager.Combat)
            //{
            //    _remove = new GUIButton(start + new Vector2(800, 64), new Rectangle(0, 128, 64, 32), 128, 64, "Remove", @"Textures\Dialog", true);
            //}
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

            _gMagic.SetText("Mag: " + _character.StatMagic);
            _gDef.SetText("Def: " + _character.StatDef);
            _gDmg.SetText("Dmg: " + _character.StatDmg);
            _gHP.SetText("HP: " + _character.StatHP);
            _gSpd.SetText("Spd: " + _character.StatSpd);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_character != null)
            {
                _window.Draw(spriteBatch);

                _weaponBox.DrawDescription(spriteBatch);
                _armorBox.DrawDescription(spriteBatch);

                if (_remove != null) { _remove.Draw(spriteBatch); }
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
