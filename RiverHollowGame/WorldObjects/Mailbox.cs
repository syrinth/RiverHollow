using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Misc;
using RiverHollow.SpriteAnimations;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.Utilities.Enums;
using static RiverHollow.Game_Managers.GameManager;
using RiverHollow.Utilities;

namespace RiverHollow.WorldObjects
{
    public class Mailbox : Buildable
    {
        private AnimatedSprite _alertSprite;
        private List<string> _liCurrentMessages;
        private List<string> _liSentMessages;

        public Mailbox(int id, Dictionary<string, string> stringData) : base(id, stringData)
        {
            _liCurrentMessages = new List<string>();
            _liSentMessages = new List<string>();
            PlayerManager.PlayerMailbox = this;

            _rBase.Y = _pSize.Y - BaseHeight;
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);
            _alertSprite?.Update(gTime);
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            _alertSprite?.Draw(spriteBatch, Constants.MAX_LAYER_DEPTH);
        }

        public override bool ProcessRightClick()
        {
            return TakeMessage();
        }

        public void SendMessage(string messageID)
        {
            _liSentMessages.Add(messageID);
        }

        public bool TakeMessage()
        {
            bool rv = false;

            if (_liCurrentMessages.Count > 0)
            {
                rv = true;
                TextEntry tEntry = DataManager.GetMailboxMessage(_liCurrentMessages[0]);
                _liCurrentMessages.RemoveAt(0);

                if (_liCurrentMessages.Count == 0)
                {
                    _alertSprite = null;
                }

                GUIManager.OpenTextWindow(tEntry);
            }

            return rv;
        }

        public override void Rollover()
        {
            foreach (string strID in _liSentMessages)
            {
                _liCurrentMessages.Add(strID);
            }
            _liSentMessages.Clear();

            if (_liCurrentMessages.Count > 0)
            {
                _alertSprite = new AnimatedSprite(DataManager.DIALOGUE_TEXTURE);
                _alertSprite.AddAnimation(AnimationEnum.ObjectIdle, 64, 64, Constants.TILE_SIZE, Constants.TILE_SIZE, 3, 0.150f, true);
                _alertSprite.Position = new Point(MapPosition.X, MapPosition.Y - Constants.TILE_SIZE);
            }
        }

        public new MailboxData SaveData()
        {
            MailboxData data = new MailboxData();
            data.MailboxMessages = new List<string>();
            foreach (string strID in _liCurrentMessages)
            {
                data.MailboxMessages.Add(strID);
            }

            return data;
        }
        public void LoadData(MailboxData data)
        {
            foreach (string strID in data.MailboxMessages)
            {
                _liSentMessages.Add(strID);
            }

            Rollover();
        }
    }
}
