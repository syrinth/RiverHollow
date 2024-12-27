using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Misc;
using RiverHollow.SpriteAnimations;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;

using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Items
{
    public class Clothing : Item
    {
        public EquipmentEnum ClothingType => GetEnumByIDKey<EquipmentEnum>("Subtype");
        public bool GenderNeutral => GetBoolByIDKey("Neutral");

        public Clothing(int id) : base(id, 1)
        {
            _texTexture = DataManager.GetTexture(GetTextureName());
        }
        private string GetTextureName()
        {
            string rv = string.Empty;

            switch (ClothingType)
            {
                case EquipmentEnum.Hat:
                    rv = DataManager.FOLDER_ITEMS + "Hats";
                    break;
                case EquipmentEnum.Shirt:
                    rv = DataManager.FOLDER_ITEMS + "Shirts";
                    break;
                case EquipmentEnum.Pants:
                    rv = DataManager.FOLDER_ITEMS + "Pants";
                    break;
            }

            return rv;
        }

        public AnimatedSprite GetSprite()
        {
            var size = new Point(1, 1);
            int xCrawl = 0;
            AnimatedSprite sprite =  new AnimatedSprite(GetTextureName());

            if (ClothingType != EquipmentEnum.Pants)
            {
                foreach (DirectionEnum e in Enum.GetValues(typeof(DirectionEnum)))
                {
                    if (e == DirectionEnum.None) { continue; }

                    sprite.AddAnimation(e, ((_pSourcePos.X + 1) * Constants.TILE_SIZE) + xCrawl, (_pSourcePos.Y * Constants.TILE_SIZE), size);
                    xCrawl += Constants.TILE_SIZE;
                }
            }
            else
            {
                var data = DataManager.Config[17];
                List<AnimationData> dataList = Util.LoadPlayerAnimations(data);

                foreach (AnimationData d in dataList)
                {
                    xCrawl = 0;
                    d.ChangeLocation(new Point(Constants.TILE_SIZE, _pSourcePos.Y * Constants.TILE_SIZE));

                    if (d.Directional)
                    {
                        foreach (DirectionEnum e in Enum.GetValues(typeof(DirectionEnum)))
                        {
                            if (e == DirectionEnum.None) { continue; }
                            sprite.AddAnimation(d.Verb, e, d.XLocation + xCrawl, d.YLocation * Constants.TILE_SIZE, Constants.TILE_SIZE, Constants.TILE_SIZE, d.Frames, d.FrameSpeed, false, d.Verb == VerbEnum.Action1);
                            xCrawl += (d.Frames * Constants.TILE_SIZE);
                        }

                        if (d.BackToIdle)
                        {
                            foreach (DirectionEnum e in Enum.GetValues(typeof(DirectionEnum)))
                            {
                                if (e == DirectionEnum.None) { continue; }
                                sprite.SetNextAnimation(Util.GetActorString(d.Verb, e), Util.GetActorString(VerbEnum.Idle, e));
                            }
                        }
                    }
                    else
                    {
                        sprite.AddAnimation(d.Animation, d.XLocation, d.YLocation, Constants.TILE_SIZE, Constants.TILE_SIZE, d.Frames, d.FrameSpeed, d.PingPong, d.PlayOnce);
                    }
                }
            }
            return sprite;
        }
    }
}
