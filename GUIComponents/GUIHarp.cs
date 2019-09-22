using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.GUIComponents
{
    class GUIHarp : GUIWindow
    {
        GUISprite _gSprite;
        List<GUIMusicNote> _liGNotes;
        public GUIHarp() : base(GUIWindow.BrownWin, GUIManager.MAIN_COMPONENT_WIDTH, GUIManager.MAIN_COMPONENT_HEIGHT)
        {
            _gSprite = new GUISprite(HarpManager.SongSpirit.BodySprite);
            _gSprite.SetScale(Scale);
            _liGNotes = new List<GUIMusicNote>();

            _gSprite.CenterOnWindow(this);
            _gSprite.AnchorToInnerSide(this, SideEnum.Top);
            AddControl(_gSprite);
            CenterOnScreen();
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
                    SoundManager.PlayEffect("c3");
                    break;
                case "B":
                    gNote = new GUIMusicNote();
                    gNote.AnchorAndAlignToObject(_gSprite, SideEnum.Bottom, SideEnum.CenterX);
                    SoundManager.PlayEffect("d2");
                    break;
                case "C":
                    gNote = new GUIMusicNote();
                    gNote.AnchorToInnerSide(this, SideEnum.Right);
                    gNote.AlignToObject(_gSprite, SideEnum.Bottom);
                    SoundManager.PlayEffect("e2");
                    break;
            }

            if (gNote != null) { _liGNotes.Add(gNote); }
        }

        public class GUIMusicNote : GUIImage
        {
            public GUIMusicNote() : base(new Rectangle(0, 0, TileSize, TileSize), GameManager.ScaledTileSize, GameManager.ScaledTileSize, @"Textures\Dialog")
            {

            }

            public override void Update(GameTime gTime)
            {
                PositionAdd(new Vector2(0, 2));
            }
        }
    }
}
