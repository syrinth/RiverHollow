using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.WorldObjects
{
    public abstract class TriggerObject : WorldObject
    {
        readonly TriggerObjectEnum _eSubType;
        protected readonly string _sOutTrigger;   //What trigger response is sent
        protected string _sMatchTrigger; //What, if anything, the object responds to
        protected int _iTriggerNumber = 1;
        protected int _iTriggersLeft = 1;
        protected bool _bVisible = true;
        readonly protected int _iItemKeyID = -1;
        protected bool _bHasBeenTriggered = false;

        protected TriggerObject(int id, Dictionary<string, string> stringData) : base(id)
        {
            LoadDictionaryData(stringData);
            _eObjectType = ObjectTypeEnum.DungeonObject;
            _eSubType = Util.ParseEnum<TriggerObjectEnum>(stringData["Subtype"]);

            Util.AssignValue(ref _sOutTrigger, "OutTrigger", stringData);
            Util.AssignValue(ref _sMatchTrigger, "MatchTrigger", stringData);
            _iTriggerNumber = Util.AssignValue("TriggerNumber", stringData);
            _iItemKeyID = Util.AssignValue("ItemKeyID", stringData);

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
        public virtual void Reset() { }

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
    }
}
