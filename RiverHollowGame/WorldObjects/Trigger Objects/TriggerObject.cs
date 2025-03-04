﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.WorldObjects
{
    public abstract class TriggerObject : WorldObject
    {
        readonly TriggerObjectEnum _eSubType;
        protected string _sOutTrigger;   //What trigger response is sent
        protected string _sMatchTrigger; //What, if anything, the object responds to
        protected int _iTriggerNumber;
        protected int _iTriggersLeft;
        protected bool _bVisible = true;
        protected int _iItemKeyID = -1;
        protected bool _bHasBeenTriggered = false;
         
        protected TriggerObject(int id, Dictionary<string, string> stringData) : base(id)
        {
            _eObjectType = ObjectTypeEnum.DungeonObject;
            _eSubType = GetEnumByIDKey<TriggerObjectEnum>("Subtype");

            if (stringData != null)
            {
                Util.AssignValue(ref _sOutTrigger, "OutTrigger", stringData);

                if (stringData.ContainsKey("Reset"))
                {
                    Reset = true;
                }
 
                _iTriggerNumber = Util.AssignValue("TriggerNumber", stringData, 1);
                _iItemKeyID = Util.AssignValue("ItemKeyID", stringData);

                if(_iItemKeyID != -1)
                {
                    _sMatchTrigger = Constants.TRIGGER_ITEM_OPEN;
                }
                else
                {
                    Util.AssignValue(ref _sMatchTrigger, "MatchTrigger", stringData);
                }
            }
            
            _iTriggersLeft = _iTriggerNumber;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_bVisible)
            {
                base.Draw(spriteBatch);
            }
        }

        public override bool PlaceOnMap(Point pos, RHMap map, bool ignoreActors = false)
        {
            bool rv = base.PlaceOnMap(pos, map);
            if (rv)
            {
                if (map.IsDungeon) { DungeonManager.AddTriggerObject(map.DungeonName, this); }
                else { GameManager.AddTriggerObject(this); }
            }

            return rv;
        }

        /// <summary>
        /// This method is called when something attempts to trigger it
        /// </summary>
        /// <param name="name">The name of the trigger</param>
        public virtual void AttemptToTrigger(string name) { }

        /// <summary>
        /// Call to see if the object will be triggered by the sent trigger
        /// </summary>
        /// <param name="triggerName">The trigger name to match against the response trigger</param>
        /// <returns>True if theo object can trigger</returns>
        protected bool TriggerMatches(string triggerName)
        {
            bool rv = false;

            if (triggerName == _sMatchTrigger)
            {
                rv = UpdateTriggerNumber();
            }

            return rv;
        }

        /// <summary>
        /// This method is called to trigger the object and make it send its trigger
        /// </summary>
        public virtual void FireTrigger() { }

        /// <summary>
        /// Call this to reset the DungeonObject to its original state.
        /// </summary>
        public virtual void ResetTrigger() { }

        /// <summary>
        /// Given an item type, check it against the key for the DungeonObject
        /// </summary>
        /// <param name="item">The Item to check against</param>
        /// <returns>True if the item is the key</returns>
        public bool CheckForKey(Item item)
        {
            bool rv = false;
            if (_iItemKeyID == item.ID)
            {
                rv = true;
                item.Remove(1);
            }

            return rv;
        }

        /// <summary>
        /// This checks whether or not the object should trigger
        /// </summary>
        /// <returns>Returns true if the object can trigger</returns>
        protected bool UpdateTriggerNumber()
        {
            if (_iTriggersLeft > 0)
            {
                _iTriggersLeft--;
            }

            return _iTriggersLeft == 0;
        }

        public override WorldObjectData SaveData()
        {
            var data = base.SaveData();
            data.stringData = string.Format("{0}/{1}/{2}/{3}/{4}/{5}", _bHasBeenTriggered, _sOutTrigger, _sMatchTrigger, _iTriggerNumber, _iItemKeyID, _iTriggersLeft);

            return data;
        }

        public override void LoadData(WorldObjectData data)
        {
            base.LoadData(data);
            var strData = Util.FindParams(data.stringData);
            _bHasBeenTriggered = bool.Parse(strData[0]);
            _sOutTrigger = strData[1];
            _sMatchTrigger = strData[2];
            _iTriggerNumber = int.Parse(strData[3]);
            _iItemKeyID = int.Parse(strData[4]);
            _iTriggersLeft = int.Parse(strData[5]);
        }
    }
}
