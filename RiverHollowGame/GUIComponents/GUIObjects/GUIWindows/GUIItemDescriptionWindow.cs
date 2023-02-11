using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.Misc;

using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.GUIComponents.GUIObjects.GUIWindows
{
    public class GUIItemDescriptionWindow : GUITextWindow
    {
        GUIImage _gBar;
        public GUIItemDescriptionWindow(Item it, Point position) : base(new TextEntry(it.Description()), position)
        {
            _gBar = new GUIImage(new Rectangle(106, 122, 8, 1), _giText.Width, ScaleIt(1), DataManager.HUD_COMPONENTS);
            _gBar.AnchorAndAlignToObject(_giText, SideEnum.Bottom, SideEnum.Left, ScaleIt(2));
            AddControl(_gBar);

            Resize(false, 1);
        }
    }
}
