using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;
using RiverHollow.GUIComponents.Screens;

namespace RiverHollow.WorldObjects
{
    public class Trigger : TriggerObject
    {
        string _sSoundEffect;
        Item _item;
        public Trigger(int id, Dictionary<string, string> stringData) : base(id, stringData)
        {
            _item = DataManager.GetItem(_iItemKeyID);

            Util.AssignValue(ref _sSoundEffect, "SoundEffect", stringData);

            if (_iItemKeyID == -1)
            {
                Sprite.AddAnimation(AnimationEnum.Action1, _pImagePos.X + Width, _pImagePos.Y, _pSize);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            if (_item != null)
            {
                float visibility = _bHasBeenTriggered ? 1f : 0.25f;
                _item.Draw(spriteBatch, new Rectangle((int)(MapPosition.X), (int)(MapPosition.Y - 6), Constants.TILE_SIZE, Constants.TILE_SIZE), true, Sprite.LayerDepth + 1, visibility);
            }
        }

        /// <summary>
        /// Called when the player interacts with the object.
        /// 
        /// If it's already triggered, do nothing.
        /// </summary>
        public override bool ProcessRightClick()
        {
            bool rv = false;
            GameManager.SetSelectedWorldObject(this);

            if (!_bHasBeenTriggered)
            {
                //If there's an itemKeyID, display appropriate text
                if (_iItemKeyID != -1)
                {
                    rv = true;
                    GUIManager.OpenMainObject(new HUDInventoryDisplay());
                }
                else
                {
                    rv = true;
                    SoundManager.PlayEffectAtLoc(_sSoundEffect, MapName, MapPosition, this);
                    FireTrigger();
                }
            }

            return rv;
        }

        public override void AttemptToTrigger(string name)
        {
            if (TriggerMatches(name))
            {
                FireTrigger();
            }
        }

        public override void FireTrigger()
        {
            if (!_bHasBeenTriggered)
            {
                Activate();
            }
            else
            {
                Reset();
            }
            

            if (CurrentMap.IsDungeon) { DungeonManager.ActivateTrigger(_sOutTrigger); }
            else { GameManager.ActivateTriggers(_sOutTrigger); }
        }

        protected void Activate()
        {
            _bHasBeenTriggered = true;
            Sprite.PlayAnimation(AnimationEnum.Action1);
        }

        public override void Reset()
        {
            _bHasBeenTriggered = false;
            Sprite.PlayAnimation(AnimationEnum.ObjectIdle);
        }
    }
}
