using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.Characters.CombatStuff;
using System.Collections.Generic;
using RiverHollow.GUIObjects;
using RiverHollow.Items;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class PartyScreen : GUIScreen
    {
        List<CharacterBox> _partyList;
        public PartyScreen()
        {
            _partyList = new List<CharacterBox>();
            int i = 0;
            foreach(CombatAdventurer c in PlayerManager.GetParty())
            {
                _partyList.Add(new CharacterBox(c, new Vector2(128, 32+(i++*100))));
            }
            foreach(CharacterBox c in _partyList)
            {
                Controls.Add(c);
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
        int _drawnStat;
        Vector2 _size;
        GUIItemBox _weapon;
        GUIItemBox _armor;
        GUIButton _remove;
        public bool ClearThis;

        public CharacterBox(CombatAdventurer c, Vector2 position)
        {
            _window = new GUIWindow(position, new Vector2(0, 0), 32, RiverHollow.ScreenWidth - 100, 100);
            Vector2 start = _window.Rectangle().Location.ToVector2();
            _font = GameContentManager.GetFont(@"Fonts\Font");
            _character = c;
            _size = _font.MeasureString("XXXXXXXX");
            _weapon = new GUIItemBox(start + new Vector2(400, 0), new Rectangle(288, 32, 32, 32), 32, 32, @"Textures\Dialog", _character.Weapon);
            _armor = new GUIItemBox(start + new Vector2(450, 0), new Rectangle(288, 32, 32, 32), 32, 32, @"Textures\Dialog", _character.Armor);
            if (_character != PlayerManager.Combat)
            {
                _remove = new GUIButton(start + new Vector2(800, 64), new Rectangle(64, 192, 64, 32), 128, 64, @"Textures\Dialog", true);
            }

            ClearThis = false;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_character != null)
            {
                _window.Draw(spriteBatch);
                _drawnStat = 0;
                spriteBatch.DrawString(_font, _character.Name, _window.Rectangle().Location.ToVector2(), Color.White);
                spriteBatch.DrawString(_font, _character.CharacterClass.Name, _window.Rectangle().Location.ToVector2() + new Vector2(200, 0), Color.White);
                spriteBatch.DrawString(_font, _character.XP + "/" + CombatAdventurer.LevelRange[_character.ClassLevel], _window.Rectangle().Location.ToVector2() + new Vector2(800, 0), Color.White);
                DrawStat(spriteBatch, "Mag");
                DrawStat(spriteBatch, "Def");
                DrawStat(spriteBatch, "Dmg");
                DrawStat(spriteBatch, "HP");
                DrawStat(spriteBatch, "Spd");

                _weapon.Draw(spriteBatch);
                _armor.Draw(spriteBatch);

                _weapon.DrawDescription(spriteBatch);
                _armor.DrawDescription(spriteBatch);

                if (_remove != null) { _remove.Draw(spriteBatch); }
            }
        }

        private void DrawStat(SpriteBatch spriteBatch, string text)
        {
            // Template IE: MAG: 999
            Vector2 boxSpace = new Vector2(_window.Rectangle().Bottom, _window.Rectangle().Left - _size.Y);
            string statLine = string.Empty;
            switch (text)
            {
                case "Mag":
                    statLine = "Mag: " + _character.StatMagic.ToString(); ;
                    break;
                case "Def":
                    statLine = "Def: " + _character.StatDef.ToString();
                    break;
                case "Dmg":
                    statLine = "Dmg: " + _character.StatDmg.ToString();
                    break;
                case "HP":
                    statLine = "HP: " + _character.StatHP.ToString();
                    break;
                case "Spd":
                    statLine = "Spd: " + _character.StatSpd.ToString();
                    break;
            }
            Vector2 position = boxSpace + (new Vector2(_size.X, 0) * _drawnStat++);
            spriteBatch.DrawString(_font, statLine, position, Color.White);
        }

        public bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (_character != null)
            {
                if (_remove != null && _remove.Contains(mouse))
                {
                    PlayerManager.RemoveFromParty(_character);
                    _character.World.DrawIt = true;
                    _character = null;
                    rv = true;
                }
                else if (_weapon.Contains(mouse))
                {
                    rv = ItemSwap(_weapon, Equipment.EquipmentType.Weapon);
                }
                else if (_armor.Contains(mouse))
                {
                    rv = ItemSwap(_armor, Equipment.EquipmentType.Armor);
                }
            }
            return rv;
        }

        public bool ProcessHover(Point mouse)
        {
            bool rv = false;
            if (_weapon.ProcessHover(mouse))
            {
                rv = true;
            }
            else if (_armor.ProcessHover(mouse))
            {
                rv = true;
            }
            return rv;
        }
        public override bool Contains(Point mouse)
        {
            return _window.Contains(mouse);
        }

        private bool ItemSwap(GUIItemBox box, Equipment.EquipmentType match)
        {
            bool rv = false;
            if (GraphicCursor.HeldItem != null)
            {
                if (GraphicCursor.HeldItem.Type == Item.ItemType.Equipment && ((Equipment)GraphicCursor.HeldItem).EquipType == match)
                {
                    if (box.Item != null)
                    {
                        Equipment temp = (Equipment)GraphicCursor.HeldItem;
                        GraphicCursor.GrabItem(box.Item);
                        box.Item = temp;
                        _character.Weapon = temp;
                    }
                    else
                    {
                        box.Item = GraphicCursor.HeldItem;
                        _character.Weapon = (Equipment)GraphicCursor.HeldItem;
                        GraphicCursor.DropItem();
                        rv = true;
                    }
                }
            }
            else
            {
                rv = GraphicCursor.GrabItem(box.Item);
                box.Item = null;
                _character.Weapon = null;
            }

            return rv;
        }

        public bool EquipItem(Item i)
        {
            bool rv = false;

            if (i.Type == Item.ItemType.Equipment)
            {
                if (((Equipment)i).EquipType == Equipment.EquipmentType.Armor)
                {
                    rv = ItemSwap(_armor, Equipment.EquipmentType.Armor);
                }
                else
                {
                    rv = ItemSwap(_weapon, Equipment.EquipmentType.Weapon);
                }
            }

            return rv;
        }

        public void AssignNewCharacter(CombatAdventurer c)
        {
            _character = c;
            _weapon.Item = _character.Weapon;
            _armor.Item = _character.Armor;
        }
    }
}
