﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;

using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.GUIComponents
{
    class GUIHarp : GUIMainObject
    {
        GUIWindow _gWindow;
        GUISprite _gSprite;
        List<GUIMusicNote> _liGNotes;
        public GUIHarp()
        {
            _gWindow = SetMainWindow();
            _gSprite = new GUISprite(HarpManager.SongSpirit.BodySprite);
            _gSprite.SetScale(Scale);
            _liGNotes = new List<GUIMusicNote>();

            _gSprite.CenterOnWindow(_gWindow);
            _gSprite.AnchorToInnerSide(_gWindow, SideEnum.Top);
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
