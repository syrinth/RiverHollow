using RiverHollow.Characters;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.GUIComponents.GUIObjects
{
    /// <summary>
    /// This represents an info panel on the CombatScreen that will display info on characters
    /// </summary>
    public class GUITacticalCombatantInfo : GUIWindow
    {
        GUISprite _gSprite;
        TacticalCombatActor _actor;
        public TacticalCombatActor Character => _actor;

        public GUITacticalCombatantInfo(TacticalCombatActor actor) : base()
        {
            _actor = actor;
            _gSprite = new GUISprite(actor.BodySprite, true);
            _gSprite.SetScale(CurrentScale);
            _gSprite.AnchorToInnerSide(this, SideEnum.TopLeft);
            _gSprite.PlayAnimation(VerbEnum.Walk, DirectionEnum.Down);
            AddControl(_gSprite);

            GUIText gName = new GUIText("XXXXXXXXXXXXXXX");
            gName.AnchorAndAlignToObject(_gSprite, SideEnum.Right, SideEnum.Top);
            AddControl(gName);

            GUIText gHP = new GUIText(string.Format("HP: {0}/{1}", actor.CurrentHP, actor.MaxHP));
            gHP.AnchorAndAlignToObject(gName, SideEnum.Bottom, SideEnum.Left);
            AddControl(gHP);

            if (actor.MaxMP > 0)
            {
                GUIText gMP = new GUIText(string.Format("MP: {0}/{1}", actor.CurrentMP, actor.MaxMP));
                gMP.AnchorAndAlignToObject(gHP, SideEnum.Bottom, SideEnum.Left);
                AddControl(gMP);
            }

            Width = TILE_SIZE;
            Height = _gSprite.Height;
            Resize();
            gName.SetText(actor.Name);
        }

        public void Reset()
        {
            _gSprite.Reset();
        }
    }
}
