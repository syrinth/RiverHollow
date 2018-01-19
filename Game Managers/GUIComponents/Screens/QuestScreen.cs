using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.Characters.CombatStuff;
using System.Collections.Generic;
using RiverHollow.GUIObjects;
using RiverHollow.Items;
using RiverHollow.Misc;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class QuestScreen : GUIScreen
    {
        List<QuestBox> _questList;
        public QuestScreen()
        {
            _questList = new List<QuestBox>();
            int i = 0;
            foreach (Quest q in PlayerManager.QuestLog)
            {
                _questList.Add(new QuestBox(q, new Vector2(128, 32 + (i++ * 100))));
            }
            foreach (QuestBox q in _questList)
            {
                Controls.Add(q);
            }
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            foreach (QuestBox c in _questList)
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
            foreach (QuestBox c in _questList)
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

    public class QuestBox : GUIObject
    {
        GUIWindow _window;
        Quest _quest;
        SpriteFont _font;
        Vector2 _size;
        public bool ClearThis;

        public QuestBox(Quest q, Vector2 position)
        {
            _window = new GUIWindow(position, new Vector2(0, 0), 32, RiverHollow.ScreenWidth - 100, 100);
            Vector2 start = _window.Rectangle().Location.ToVector2();
            _font = GameContentManager.GetFont(@"Fonts\Font");
            _quest = q;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _window.Draw(spriteBatch);
            spriteBatch.DrawString(_font, _quest.Name, _window.Rectangle().Location.ToVector2(), Color.White);
            spriteBatch.DrawString(_font, _quest.Accomplished + @"/" + _quest.TargetGoal, _window.Rectangle().Location.ToVector2() + new Vector2(200, 0), Color.White);
        }

        public bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            return rv;
        }
        public bool ProcessHover(Point mouse)
        {
            bool rv = false;
            return rv;
        }
        public override bool Contains(Point mouse)
        {
            return _window.Contains(mouse);
        }
    }
}
