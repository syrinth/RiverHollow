using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Misc;
using RiverHollow.SpriteAnimations;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.Utilities.Enums;
using static RiverHollow.Game_Managers.GameManager;

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

            _rBase.Y = _uSize.Height - BaseHeight;
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);
            _alertSprite?.Update(gTime);
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            _alertSprite?.Draw(spriteBatch, GameManager.MAX_LAYER_DEPTH);
        }

        public override void ProcessRightClick()
        {
            TakeMessage();
        }

        public void SendMessage(string messageID)
        {
            _liSentMessages.Add(messageID);
        }

        public void TakeMessage()
        {
            if (_liCurrentMessages.Count > 0)
            {
                TextEntry tEntry = DataManager.GetMailboxMessage(_liCurrentMessages[0]);
                _liCurrentMessages.RemoveAt(0);

                if (_liCurrentMessages.Count == 0)
                {
                    _alertSprite = null;
                }

                GUIManager.OpenTextWindow(tEntry);
            }
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
                _alertSprite.AddAnimation(AnimationEnum.ObjectIdle, 64, 64, TILE_SIZE, TILE_SIZE, 3, 0.150f, true);
                _alertSprite.Position = new Vector2(_vMapPosition.X, _vMapPosition.Y - TILE_SIZE);
            }
        }

        public MailboxData SaveData()
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
