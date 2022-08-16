using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Map_Handling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiverHollow.WorldObjects.Trigger_Objects
{
    class Switch : WorldObject
    {
        protected readonly string _sOutTrigger;   //What trigger response is sent
        protected bool _bHasBeenTriggered = false;
        protected bool _bVisible = true;

        public Switch(int id, Dictionary<string, string> data) : base(id, data)
        {
           
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_bVisible)
            {
                base.Draw(spriteBatch);
            }
        }

        public override bool PlaceOnMap(Vector2 pos, RHMap map, bool ignoreActors = false)
        {
            bool rv = base.PlaceOnMap(pos, map);
            if (rv)
            {
                //if (map.IsDungeon) { DungeonManager.AddTriggerObject(map.DungeonName, this); }
                //else { GameManager.AddTriggerObject(this); }
            }

            return rv;
        }


        //public override void AttemptToTrigger(string name)
        //{
        //    if (TriggerMatches(name))
        //    {
        //        FireTrigger();
        //    }
        //}

        //public override void FireTrigger()
        //{
        //    if (CanTrigger())
        //    {
        //        _bHasBeenTriggered = true;
        //        _sprite.PlayAnimation(AnimationEnum.Action1);

        //        if (CurrentMap.IsDungeon) { DungeonManager.ActivateTrigger(_sOutTrigger); }
        //        else { GameManager.ActivateTriggers(_sOutTrigger); }
        //    }
        //}
    }
}
