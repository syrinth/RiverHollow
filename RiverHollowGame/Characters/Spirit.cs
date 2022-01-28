using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Misc;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.Characters
{
    public class Spirit : TalkingActor
    {
        public override Rectangle HoverBox => new Rectangle((int)Position.X, (int)Position.Y, Width, Height);

        const float MIN_VISIBILITY = 0.05f;
        float _fVisibility;
        int _iID;
        string _sCondition;
        string _sText;
        public int SongID { get; } = 1;
        private string _sAwakenTrigger;

        private bool _bAwoken = false;
        public bool Triggered = false;

        public Spirit(Dictionary<string, string> stringData) : base()
        {
            _eActorType = WorldActorTypeEnum.Spirit;
            _fVisibility = MIN_VISIBILITY;

            Util.AssignValue(ref _sName, "Name", stringData);
            Util.AssignValue(ref _iID, "SpiritID", stringData);
            Util.AssignValue(ref _sText, "Text", stringData);
            Util.AssignValue(ref _sCondition, "Condition", stringData);
            Util.AssignValue(ref _sAwakenTrigger, "AwakenTrigger", stringData);

            //_sPortrait = Util.GetPortraitLocation(DataManager.PORTRAIT_FOLDER, "Spirit", stringData["Key"]);

            _bOnTheMap = false;

            _iBodyWidth = TILE_SIZE;
            _iBodyHeight = TILE_SIZE + 2;
            List<AnimationData> liData = new List<AnimationData>();
            Util.AddToAnimationsList(ref liData, stringData, VerbEnum.Idle);
            LoadSpriteAnimations(ref _sprBody, liData, DataManager.NPC_FOLDER + "Spirit_" + _iID);
        }

        public override void Update(GameTime gTime)
        {
            if (_bOnTheMap && _bAwoken)
            {
                _sprBody.Update(gTime);
                //if (_bActive)
                //{
                //    base.Update(gTime);
                //    if (!Triggered)
                //    {
                //        int max = TILE_SIZE * 13;
                //        int dist = 0;
                //        if (PlayerManager.CurrentMap == CurrentMapName && PlayerManager.PlayerInRangeGetDist(_spriteBody.Center.ToPoint(), max, ref dist))
                //        {
                //            float fMax = max;
                //            float fDist = dist;
                //            float percentage = (Math.Abs(dist - fMax)) / fMax;
                //            percentage = Math.Max(percentage, MIN_VISIBILITY);
                //            _fVisibility = 0.4f * percentage;
                //        }
                //    }
                //}
            }
        }

        public override void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            if (_bOnTheMap && _bAwoken)
            {
                _sprBody.Draw(spriteBatch, useLayerDepth, _fVisibility);
            }
        }

        public void AttemptToAwaken(string triggerName)
        {
            if (_sAwakenTrigger.Equals(triggerName))
            {
                _bAwoken = true;
            }
        }
        public void CheckCondition()
        {
            bool active = false;
            string[] splitCondition = _sCondition.Split('/');
            foreach (string s in splitCondition)
            {
                if (s.Equals("Raining"))
                {
                    active = EnvironmentManager.IsRaining();
                }
                else if (s.Contains("Day"))
                {
                    active = !GameCalendar.IsNight();//s.Equals(GameCalendar.GetDayOfWeek());
                }
                else if (s.Equals("Night"))
                {
                    active = GameCalendar.IsNight();
                }

                if (!active) { break; }
            }

            _bOnTheMap = active;
            Triggered = false;
        }
        public override TextEntry GetOpeningText()
        {
            TextEntry rv = null;
            if (_bOnTheMap)
            {
                Triggered = true;
                _fVisibility = 1.0f;

                //string[] loot = DataManager.DiSpiritInfo[_sType].Split('/');
                //int arrayID = RHRandom.Instance().Next(0, loot.Length - 1);
                //InventoryManager.AddToInventory(int.Parse(loot[arrayID]));

                //_sText = Util.ProcessText(_sText.Replace("*", "*" + loot[arrayID] + "*"));
                // GUIManager.OpenTextWindow(_sText, this, true);
            }
            return rv;
        }
    }
}
