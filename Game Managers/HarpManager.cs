using Microsoft.Xna.Framework;
using RiverHollow.Actors;
using RiverHollow.GUIComponents;
using RiverHollow.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiverHollow.Game_Managers
{
    class HarpManager
    {
        static bool _bPlayingMusic;
        static GUIHarp _gHarp;
        static Spirit _targetSpirit;
        static Song _currSong;

        public static Spirit SongSpirit=> _targetSpirit;
        public static bool PlayingMusic;
        public static void NewSong(Spirit target)
        {
            PlayingMusic = true;
            _targetSpirit = target;
            _currSong = new Song(_targetSpirit.SongID);
            _gHarp = new GUIHarp();

            GUIManager.OpenMainObject(_gHarp);
        }

        public static void Update(GameTime gTime)
        {
            _currSong.Update(gTime);
        }

        public static void SpawnNotes(string newNotes)
        {
            string[] notes = newNotes.Split('-');
            foreach(string note in notes)
            {
                _gHarp.SpawnGUINote(note);
            }
        }

        private class Song
        {
            int _iIndex;
            double _dTimer;
            List<KeyValuePair<double, string>> _liNotes;

            /// <summary>
            /// Class to hold the Songs that get played by the Harp. Songs are contained in the
            /// Songs.xml file and are stored as lists by ID.
            /// 
            /// Each song has a list with enties like the following: [X:A-B-C]
            /// X is the amount of seconds that have elapsed since the last note was played, and A-B-C
            /// are any notes that should be played when the given timer elapses.
            /// 
            /// Ex:
            /// [2:A]       Play an A note after two seconds
            /// [1:A-B]     Play an A and B note one second after the last note
            /// [1.4:C]     Play a C note 1.4 seconds after the A-B note
            /// 
            /// Due to update constraints, the songs may not play at the EXACT samemilisecond, but shouuld be close enough
            /// </summary>
            /// <param name="id"></param>
            public Song(int id)
            {
                _liNotes = new List<KeyValuePair<double, string>>();
                foreach (string s in GameContentManager.GetSong(id))
                {
                    string[] songTags = Util.FindTags(s);
                    songTags = songTags[0].Split(':');

                    //Store the song data in a list of KVPs where the key is the timer and the value is the notes
                    _liNotes.Add(new KeyValuePair<double, string>(double.Parse(songTags[0]), songTags[1]));
                }

                _iIndex = 0;
                _dTimer = _liNotes[_iIndex].Key;
            }

            public void Update(GameTime gTime)
            {
                _dTimer -= gTime.ElapsedGameTime.TotalSeconds;

                if(_dTimer <= 0)
                {
                    if (_iIndex < _liNotes.Count)
                    {
                        HarpManager.SpawnNotes(_liNotes[_iIndex++].Value);
                        if (_iIndex < _liNotes.Count)
                        {
                            _dTimer = _liNotes[_iIndex].Key;
                        }
                    }
                }
            }
        }
    }
}
