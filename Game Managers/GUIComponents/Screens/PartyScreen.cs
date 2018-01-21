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
        public static int WIDTH = RiverHollow.ScreenWidth / 3;
        public static int HEIGHT = RiverHollow.ScreenHeight / 3;
        List<CharacterBox> _partyList;
        GUIWindow _partyWindow;
        public PartyScreen()
        {
            _partyList = new List<CharacterBox>();
            _partyWindow = new GUIWindow(new Vector2(WIDTH, HEIGHT), GUIWindow.RedDialog, GUIWindow.RedDialogEdge, WIDTH, HEIGHT);
            int i = 0;
            foreach(CombatAdventurer c in PlayerManager.GetParty())
            {
                _partyList.Add(new CharacterBox(c, _partyWindow, ref i));
            }
            Controls.Add(_partyWindow);
            foreach (CharacterBox c in _partyList)
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
            _window = new GUIWindow(position, GUIWindow.RedDialog, GUIWindow.RedDialogEdge, RiverHollow.ScreenWidth - 100, 100);
            Vector2 start = _window.UsableRectangle().Location.ToVector2();
            _font = GameContentManager.GetFont(@"Fonts\Font");
            _character = c;
            _size = _font.MeasureString("XXXXXXXX");
            _weapon = new GUIItemBox(start + new Vector2(400, 0), new Rectangle(288, 32, 32, 32), 32, 32, @"Textures\Dialog", _character.Weapon);
            _armor = new GUIItemBox(start + new Vector2(450, 0), new Rectangle(288, 32, 32, 32), 32, 32, @"Textures\Dialog", _character.Armor);
            if (_character != PlayerManager.Combat)
            {
                _remove = new GUIButton(start + new Vector2(800, 64), new Rectangle(0, 128, 64, 32), 128, 64, "Remove", @"Textures\Dialog", true);
            }

            ClearThis = false;
        }

        public CharacterBox(CombatAdventurer c, GUIWindow win, ref int i)
        {
            int boxHeight = (QuestScreen.HEIGHT / 4) - (win.EdgeSize * 2);
            int boxWidth = (QuestScreen.WIDTH) - (win.EdgeSize * 2);

            Vector2 boxPoint = new Vector2(win.Corner().X + win.EdgeSize, win.Corner().Y + win.EdgeSize + (i++ * (boxHeight + (win.EdgeSize * 2))));
            _window = new GUIWindow(boxPoint, GUIWindow.RedDialog, GUIWindow.RedDialogEdge, boxWidth, boxHeight);

            _font = GameContentManager.GetFont(@"Fonts\Font");

            Rectangle rect = _window.UsableRectangle();
            _font = GameContentManager.GetFont(@"Fonts\Font");
            _character = c;
            _size = _font.MeasureString("XXXXXXXX");
            Vector2 start = new Vector2(rect.Right, rect.Top);
            start.X -= 32;
            _armor = new GUIItemBox(start, new Rectangle(288, 32, 32, 32), 32, 32, @"Textures\Dialog", _character.Armor);
            start.X -= 34;
            _weapon = new GUIItemBox(start, new Rectangle(288, 32, 32, 32), 32, 32, @"Textures\Dialog", _character.Weapon);
           // if (_character != PlayerManager.Combat)
            {
                _remove = new GUIButton(start + new Vector2(800, 64), new Rectangle(0, 128, 64, 32), 128, 64, "Remove", @"Textures\Dialog", true);
            }

            ClearThis = false;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_character != null)
            {
                Vector2 start = _window.Corner();
                _window.Draw(spriteBatch);
                _drawnStat = 0;
                spriteBatch.DrawString(_font, _character.Name, start, Color.White);
                spriteBatch.DrawString(_font, _character.CharacterClass.Name, start += new Vector2(_font.MeasureString(_character.Name).X+10, 0), Color.White);
                spriteBatch.DrawString(_font, _character.XP + "/" + CombatAdventurer.LevelRange[_character.ClassLevel], start += new Vector2(_font.MeasureString(_character.CharacterClass.Name).X + 10, 0), Color.White);
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
            Rectangle rect = _window.UsableRectangle();
            Vector2 start = new Vector2(rect.Left, rect.Bottom);// + new Vector2(0, _font.MeasureString("X").Y);
            start -= new Vector2(0, _font.MeasureString("X").Y);
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
            Vector2 position = start + (new Vector2(_size.X, 0) * _drawnStat++);
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
            if (_armor.ProcessHover(mouse))
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
