using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.SpriteAnimations;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Misc
{
    public class Cosmetic
    {
        public int ID { get; private set; }
        public CosmeticSlotEnum CosmeticSlot { get; private set; }
        public bool DrawAbove => DataManager.GetBoolByIDKey(ID, "Above", DataType.Cosmetic);

        public Cosmetic(int id)
        {
            ID = id;
            CosmeticSlot = DataManager.GetEnumByIDKey<CosmeticSlotEnum>(id, "Type", DataType.Cosmetic);
        }

        private string GetTextureName()
        {
            string rv = DataManager.FOLDER_COSMETICS;

            switch (CosmeticSlot)
            {
                case CosmeticSlotEnum.Hair:
                    rv += "Hair";
                    break;
                case CosmeticSlotEnum.Eyes:
                    rv += "Eyes";
                    break;
                case CosmeticSlotEnum.Head:
                    rv += "Hats";
                    break;
                case CosmeticSlotEnum.Body:
                    rv += "Shirts";
                    break;
                case CosmeticSlotEnum.Legs:
                    rv += "Pants";
                    break;
                case CosmeticSlotEnum.Feet:
                    rv += "Feet";
                    break;
            }

            return rv;
        }

        public AnimatedSprite GetSprite()
        {
            string[] texIndices = Util.FindArguments(DataManager.GetStringByIDKey(ID, "Image", DataType.Cosmetic));
            var sourcePoint = new Point(int.Parse(texIndices[0]) * Constants.TILE_SIZE, int.Parse(texIndices[1]) * Constants.TILE_SIZE);

            var size = new Point(1, 1);
            int xCrawl = 0;
            AnimatedSprite sprite = new AnimatedSprite(GetTextureName());

            switch (CosmeticSlot)
            {
                case CosmeticSlotEnum.Legs:
                    var data = DataManager.Config[17];
                    List<AnimationData> dataList = Util.LoadPlayerAnimations(data);

                    foreach (AnimationData d in dataList)
                    {
                        xCrawl = 0;
                        d.ChangeLocation(new Point(Constants.TILE_SIZE, sourcePoint.Y));

                        if (d.Directional)
                        {
                            foreach (DirectionEnum e in Enum.GetValues(typeof(DirectionEnum)))
                            {
                                if (e == DirectionEnum.None) { continue; }
                                sprite.AddAnimation(d.Verb, e, d.XLocation + xCrawl, d.YLocation, Constants.TILE_SIZE, Constants.TILE_SIZE, d.Frames, d.FrameSpeed, false, d.Verb == VerbEnum.Action1);
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
                    break;
                case CosmeticSlotEnum.Hair:
                    foreach (DirectionEnum e in Enum.GetValues(typeof(DirectionEnum)))
                    {
                        if (e == DirectionEnum.None) { continue; }

                        sprite.AddAnimation(e, (sourcePoint.X) + xCrawl, (sourcePoint.Y), size);
                        xCrawl += Constants.TILE_SIZE;
                    }
                    break;
                case CosmeticSlotEnum.Eyes:
                    sprite.AddAnimation(DirectionEnum.Down, 0, 0, new Point(1, 1));
                    sprite.AddAnimation(DirectionEnum.Right, Constants.TILE_SIZE, 0, new Point(1, 1));
                    sprite.AddAnimation(DirectionEnum.Up, Constants.TILE_SIZE * 2, 0, new Point(1, 1));
                    sprite.AddAnimation(DirectionEnum.Left, Constants.TILE_SIZE, 0, new Point(1, 1));
                    sprite.GetFrameAnimation(Util.GetEnumString(DirectionEnum.Left)).Flip = true;
                    break;
                default:
                    //Offset for the item icon
                    xCrawl += Constants.TILE_SIZE;
                    foreach (DirectionEnum e in Enum.GetValues(typeof(DirectionEnum)))
                    {
                        if (e == DirectionEnum.None) { continue; }

                        sprite.AddAnimation(e, sourcePoint.X + xCrawl, sourcePoint.Y, size);
                        xCrawl += Constants.TILE_SIZE;
                    }
                    break;
            }

            return sprite;
        }
    }

    public class AppliedCosmetic
    {
        public Color CosmeticColor { get; private set; } = Color.White;
        public Cosmetic MyCosmetic { get; private set; } = null;
        public AnimatedSprite MySprite { get; private set; } = null;

        public AppliedCosmetic() { }

        public void SetCosmetic(Cosmetic c)
        {
            MyCosmetic = c;
            MySprite = c?.GetSprite();

            SetColor(CosmeticColor);
        }

        public void SetColor(Color c)
        {
            CosmeticColor = c;
            MySprite?.SetColor(CosmeticColor);
        }
    }
}
