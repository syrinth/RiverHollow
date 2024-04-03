using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.Utilities;
using System.Collections.Generic;
using RiverHollow.GUIComponents.Screens;

using static RiverHollow.Utilities.Enums;
using static RiverHollow.Game_Managers.SaveManager;

namespace RiverHollow.WorldObjects
{
    public class Trigger : TriggerObject
    {
        Item _item;
        protected Point _pDisplayOffset;

        SoundEffectEnum TriggerSound => GetEnumByIDKey<SoundEffectEnum>("SoundEffect");
        public Trigger(int id, Dictionary<string, string> stringData) : base(id, stringData) 
        {
            _item = DataManager.GetItem(_iItemKeyID);
            _pDisplayOffset = GetPointByIDKey("DisplayOffset");

            if (_iItemKeyID == -1)
            {
                Sprite.AddAnimation(AnimationEnum.Action1, _pImagePos.X, _pImagePos.Y + Height, _pSize);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            if (_item != null)
            {
                float visibility = _bHasBeenTriggered ? 1f : 0.25f;
                _item.Draw(spriteBatch, new Rectangle(MapPosition.X + _pDisplayOffset.X, MapPosition.Y + _pDisplayOffset.Y, Constants.TILE_SIZE, Constants.TILE_SIZE), Sprite.LayerDepth + 1, visibility);
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
                SoundManager.PlayEffectAtLoc(TriggerSound, MapName, MapPosition, this);
                Activate();
            }
            else
            {
                ResetTrigger();
            }
            
            if (CurrentMap.IsDungeon) { DungeonManager.ActivateTrigger(_sOutTrigger); }
            else { GameManager.ActivateTriggers(_sOutTrigger); }
        }

        protected void Activate()
        {
            _bHasBeenTriggered = true;
            Sprite.PlayAnimation(AnimationEnum.Action1);
        }

        public override void ResetTrigger()
        {
            _bHasBeenTriggered = false;
            Sprite.PlayAnimation(AnimationEnum.ObjectIdle);
        }

        public override void LoadData(WorldObjectData data)
        {
            base.LoadData(data);
            _item = DataManager.GetItem(_iItemKeyID);
            if (_iItemKeyID != -1)
            {
                Sprite.RemoveAnimation(AnimationEnum.Action1);
            }
        }
    }
}
