using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Utilities;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.GUIComponents
{
    class GUIHarp : GUIMainObject
    {
        GUISprite _gSprite;
        List<GUIMusicNote> _liGNotes;
        public GUIHarp()
        {
            _winMain = SetMainWindow();
            _gSprite = new GUISprite(HarpManager.SongSpirit.BodySprite);
            _gSprite.SetScale(CurrentScale);
            _liGNotes = new List<GUIMusicNote>();

            _gSprite.CenterOnWindow(_winMain);
            _gSprite.AnchorToInnerSide(_winMain, SideEnum.Top);
            AddControl(_gSprite);
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);

            //If the music note has fallen off thebottom of the window, remove it
            List<GUIMusicNote> ToRemove = new List<GUIMusicNote>();
            foreach (GUIMusicNote n in _liGNotes)
            {
                if (n.Bottom >= this.Bottom)
                {
                    ToRemove.Add(n);
                }
            }

            foreach (GUIMusicNote n in ToRemove)
            {
                RemoveControl(n);
                _liGNotes.Remove(n);
            }
            ToRemove.Clear();
        }

        public void SpawnGUINote(string note)
        {
            GUIMusicNote gNote = null;
            switch(note){
                case "A":
                    gNote = new GUIMusicNote();
                    gNote.AnchorToInnerSide(this, SideEnum.Left);
                    gNote.AlignToObject(_gSprite, SideEnum.Bottom);
                   // SoundManager.PlayEffect("c3");
                    break;
                case "B":
                    gNote = new GUIMusicNote();
                    gNote.AnchorAndAlign(_gSprite, SideEnum.Bottom, SideEnum.CenterX);
                   // SoundManager.PlayEffect("d2");
                    break;
                case "C":
                    gNote = new GUIMusicNote();
                    gNote.AnchorToInnerSide(this, SideEnum.Right);
                    gNote.AlignToObject(_gSprite, SideEnum.Bottom);
                   // SoundManager.PlayEffect("e2");
                    break;
            }

            if (gNote != null) { _liGNotes.Add(gNote); }
        }

        public class GUIMusicNote : GUIImage
        {
            public GUIMusicNote() : base(new Rectangle(0, 0, Constants.TILE_SIZE, Constants.TILE_SIZE))
            {

            }

            public override void Update(GameTime gTime)
            {
                PositionAdd(new Point(0, 2));
            }
        }
    }
}
