using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.Misc;
using static RiverHollow.Utilities.Enums;
using static RiverHollow.Game_Managers.GameManager;
using System;
using System.Collections.Generic;

namespace RiverHollow.GUIComponents.GUIObjects.GUIWindows
{
    public class GUIItemDescriptionWindow : GUITextWindow
    {
        GUIImage _gBar;
        public GUIItemDescriptionWindow(Item it, Vector2 position) : base(new TextEntry(it.Description()), position)
        {
            _gBar = new GUIImage(new Rectangle(32, 158, 8, 1), _giText.Width, ScaleIt(1), DataManager.COMBAT_TEXTURE);
            _gBar.AnchorAndAlignToObject(_giText, SideEnum.Bottom, SideEnum.Left, ScaleIt(2));
            AddControl(_gBar);

            switch (it.ItemType)
            {
                case ItemEnum.Equipment:
                    AddEquipmentInfo(it);
                    break;
            }
            Resize(false, 1);
        }

        void AddEquipmentInfo(Item it)
        {
            Equipment eq = (Equipment)it;

            List<GUIObject> icons = new List<GUIObject>();
            foreach (AttributeEnum e in Enum.GetValues(typeof(AttributeEnum)))
            {
                if(eq.Attribute(e) == 0) { continue; }

                GUIAttributeIcon icon = new GUIAttributeIcon(e, eq.Attribute(e).ToString("00"));

                if (icons.Count == 0) { icon.AnchorAndAlignToObject(_gBar, SideEnum.Bottom, SideEnum.Left, ScaleIt(2)); }
                else { icon.AnchorAndAlignToObject(icons[icons.Count-1], SideEnum.Right, SideEnum.CenterY, ScaleIt(4)); }

                icons.Add(icon);

                AddControl(icon);
            }
        }
    }
}
