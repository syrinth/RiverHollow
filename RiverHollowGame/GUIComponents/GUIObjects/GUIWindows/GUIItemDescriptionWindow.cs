using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.Misc;
using RiverHollow.WorldObjects;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.GUIComponents.GUIObjects.GUIWindows
{
    public class GUIItemDescriptionWindow : GUITextWindow
    {
        GUIImage _gBar;
        public GUIItemDescriptionWindow(Item it, Point position) : base(new TextEntry(it.Description()), position)
        {
            _gBar = new GUIImage(GUIUtils.HUD_DIVIDER, _gText.Width, ScaleIt(1));
            _gBar.AnchorAndAlignWithSpacing(_gText, SideEnum.Bottom, SideEnum.Left, 2);
            AddControl(_gBar);

            Resize(false, 1);
        }

        public GUIItemDescriptionWindow(WorldObject it, Point position) : base(new TextEntry(it.Description()), position)
        {
            _gBar = new GUIImage(GUIUtils.HUD_DIVIDER, _gText.Width, ScaleIt(1));
            _gBar.AnchorAndAlignWithSpacing(_gText, SideEnum.Bottom, SideEnum.Left, 2);
            AddControl(_gBar);

            Resize(false, 1);
        }
    }
}
