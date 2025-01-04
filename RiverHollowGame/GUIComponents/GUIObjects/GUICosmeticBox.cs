using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Misc;

using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.GUIObjects
{
    internal class GUICosmeticBox : GUIImage
    {
        protected GUIImage _gSlotImg;
        public CosmeticSlotEnum CosmeticSlot { get; }
        public Cosmetic Cosmetic { get; private set; }
        GUISprite _cosmeticSprite;
        public bool DisplaySlot;
        public bool Unlocked { get; }

        public delegate void ClickDelegate(GUICosmeticBox q);
        private readonly ClickDelegate _delAction;

        public GUICosmeticBox(CosmeticSlotEnum e, Cosmetic c, ClickDelegate del, bool displaySlot, bool unlocked = true) : base(GUIUtils.ITEM_BOX)
        {
            _delAction = del;
            DisplaySlot = displaySlot;
            Unlocked = unlocked;
            CosmeticSlot = e;
            SetCosmetic(c);

            if(!Unlocked)
            {
                _cosmeticSprite?.SetColor(Color.Black);
            }
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (Unlocked && Contains(mouse))
            {
                _delAction(this);
            }

            return rv;
        }

        public virtual void SetCosmeticColor(Color c)
        {
            _cosmeticSprite?.SetColor(c);
        }
        public virtual void SetCosmetic(Cosmetic c)
        {
            RemoveControl(_cosmeticSprite);

            if (c == null)
            {
                Cosmetic = null;
                _cosmeticSprite = null;

                if (_gSlotImg == null)
                {
                    if (!DisplaySlot)
                    {
                        _gSlotImg = new GUIImage(GUIUtils.INVENTORY_ICON_CANCEL);
                    }
                    else
                    {
                        switch (CosmeticSlot)
                        {
                            case CosmeticSlotEnum.Head:
                                _gSlotImg = new GUIImage(GUIUtils.INVENTORY_ICON_HAT);
                                break;
                            case CosmeticSlotEnum.Body:
                                _gSlotImg = new GUIImage(GUIUtils.INVENTORY_ICON_SHIRT);
                                break;
                            case CosmeticSlotEnum.Legs:
                                _gSlotImg = new GUIImage(GUIUtils.INVENTORY_ICON_PANTS);
                                break;
                            case CosmeticSlotEnum.Feet:
                                _gSlotImg = new GUIImage(GUIUtils.INVENTORY_ICON_FEET);
                                break;
                            case CosmeticSlotEnum.Hair:
                                _gSlotImg = new GUIImage(GUIUtils.INVENTORY_ICON_HAIR);
                                break;
                        }
                    }
                    _gSlotImg.CenterOnObject(this);

                    AddControl(_gSlotImg);
                }
            }
            else if (c != null)
            {
                Cosmetic = c;

                _cosmeticSprite = new GUISprite(c.GetSprite());
                _cosmeticSprite.PlayAnimation(AnimationEnum.ObjectIdle);
                _cosmeticSprite.CenterOnObject(this);

                var color = PlayerManager.PlayerActor.GetAppliedCosmetic(c.CosmeticSlot).CosmeticColor;
                _cosmeticSprite.SetColor(color);

                if (_gSlotImg != null)
                {
                    RemoveControl(_gSlotImg);
                    _gSlotImg = null;
                }
            }
        }
    }
}
