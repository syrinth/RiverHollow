using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using RiverHollow.Characters;
using RiverHollow.Misc;
using RiverHollow.Tile_Engine;
using System;
using System.Collections.Generic;

namespace RiverHollow.Game_Managers
{
    static class CutSceneManager
    {
        static Cutscene _currentCutscene;
        static Dictionary<int, Cutscene> _diCutscenes;
        public static bool Playing;

        public static void LoadContent(ContentManager Content)
        {
            Playing = false;
            _diCutscenes = new Dictionary<int, Cutscene>();

            Dictionary<int, List<string>> rawData = Content.Load<Dictionary<int, List<string>>>(@"Data\CutScenes");
            foreach(KeyValuePair<int, List<string>> kvp in rawData){
                _diCutscenes.Add(kvp.Key, new Cutscene(kvp.Value));
            }
        }

        public static void CheckForTriggedCutscene()
        {
            foreach (KeyValuePair<int, Cutscene> kvp in _diCutscenes)
            {
                if (kvp.Value.Triggered())
                {
                    _currentCutscene = _diCutscenes[kvp.Key];
                    _currentCutscene.Setup();
                    Playing = true;
                    break;
                }
            }
        }

        public static void Update(GameTime gameTime)
        {
            _currentCutscene.Update(gameTime);
        }

        public static void Setup()
        {
            _currentCutscene.Setup();
        }
    }

    public class Cutscene
    {
        RHMap _cutsceneMap;
        string _sTriggerMap;
        int _iMinTime = -1;                                         //The earliest the cutscene can be triggered
        int _iMaxTime = -1;                                         //The latest the cutscene can be triggered
        int _iCurrentCommand;
        List<NPC> _liUsedNPCs;                                      //The list of NPCs that take part in the cutscene.
        List<KeyValuePair<int, int>> _liReqFriendship;              //The list of required Friendships to trigger the cutscene, key is NPC index
        List<string> _liCommands;                                   //The sequence of commands to follow for the cutscene
        List<string> _liSetupCommands;
        bool _bTriggered;

        public Cutscene(List<string> strData)
        {
            _bTriggered = false;
            _iCurrentCommand = 0;

            _liUsedNPCs = new List<NPC>();
            _liCommands = new List<string>();
            _liReqFriendship = new List<KeyValuePair<int, int>>();
            _liSetupCommands = new List<string>();

            //Get the cutscene triggers
            string[] triggers = strData[0].Split(new[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
            foreach(string s in triggers)
            {
                string[] tags = s.Split(':');
                //Parsing for important data
                if (tags[0].Equals("trigger"))
                {
                    _sTriggerMap = tags[1];
                }
                else if (tags[0].Equals("time"))
                {
                    string[] time = tags[1].Split(' ');
                    _iMinTime = int.Parse(time[0]);
                    _iMaxTime = int.Parse(time[1]);
                }
                else if (tags[0].Equals("friends"))
                {
                    string[] friend = tags[1].Split(' ');
                    foreach(string f in friend)
                    {
                        string[] friendData = f.Split('-');
                        _liReqFriendship.Add(new KeyValuePair<int,int>(int.Parse(friendData[0]), int.Parse(friendData[1])));
                    }
                }
            }

            //Get the cutscene setup tags
            _liSetupCommands.AddRange(strData[1].Split(new[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries));

            //Get the sequence of commands to run during the cutscene
            _liCommands.AddRange(strData[2].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            _liCommands.Add("End");
        }

        public void Update(GameTime gameTime)
        {
            if (GUIManager.CurrentGUIScreen != GUIManager.Screens.Text)                 //If someone is currently talking, do NOT process additional tags
            {
                string[] stringTest = _liCommands[_iCurrentCommand].Split('-');
                switch (stringTest[0])
                {
                    case "Speak":
                        if (stringTest.Length == 3)
                        {
                            NPC n = _liUsedNPCs.Find(test => test.ID == int.Parse(stringTest[1]));
                            n.Talk(stringTest[2]);
                            _iCurrentCommand++;
                        }
                        break;
                    case "MoveTo":
                        if (stringTest.Length == 3)
                        {
                            WorldCharacter c = stringTest[1].Equals("Player") ? PlayerManager.World : _liUsedNPCs.Find(test => test.ID == int.Parse(stringTest[1]));
                            if (c.MoveToObj == Vector2.Zero)
                            {
                                c.SetMoveObj(stringTest[2]);
                            }
                            else if (c.Position == c.MoveToObj) { _iCurrentCommand++; }
                        }
                        break;
                    case "End":
                        _bTriggered = true;
                        CutSceneManager.Playing = false;
                        MapManager.CurrentMap = MapManager.Maps[_cutsceneMap.Name];
                        break;
                }
            }
            _cutsceneMap.Update(gameTime);
        }

        public void Setup()
        {
            foreach (string s in _liSetupCommands)
            {
                string[] tags = s.Split(':');
                //Parsing for important data
                if (tags[0].Equals("map"))
                {
                    _cutsceneMap = new RHMap(MapManager.Maps[tags[1]]);
                }
                if (tags[0].Equals("player"))
                {
                    PlayerManager.World.Position = Utilities.Normalize(_cutsceneMap.GetCharacterSpawn(tags[1]));
                }
                else if (tags[0].Equals("actors"))
                {
                    string[] friend = tags[1].Split(' ');
                    foreach (string f in friend)
                    {
                        string[] friendData = f.Split('-');
                        NPC n = new NPC(CharacterManager.DiNPC[int.Parse(friendData[0])])
                        {
                            CurrentMapName = _cutsceneMap.Name,
                            Position = Utilities.Normalize(_cutsceneMap.GetCharacterSpawn(friendData[1]))
                        };
                        _cutsceneMap.AddCharacter(n);
                        _liUsedNPCs.Add(n);
                    }
                }
            }
            MapManager.CurrentMap = _cutsceneMap;
        }

        public bool Triggered()
        {
            return !_bTriggered && TimePassed() && TriggeredMap() && FriendshipReqs();
        }
        private bool TimePassed()
        {
            bool rv = false;
            if (_iMinTime != -1){
                if(_iMinTime > GameCalendar.CurrentHour) { goto timeExit;}
            }

            if (_iMaxTime != -1)
            {
                if (_iMaxTime < GameCalendar.CurrentHour) { goto timeExit; }
            }

            rv = true;

            timeExit:
            return rv;
        }
        private bool TriggeredMap()
        {
            return MapManager.CurrentMap.Name == _sTriggerMap;
        }
        private bool FriendshipReqs()
        {
            bool rv = false;
            foreach (KeyValuePair<int, int> kvp in _liReqFriendship)
            {
                if(CharacterManager.DiNPC[kvp.Key].Friendship < kvp.Value) { goto friendshipExit; }
            }

            rv = true;
            friendshipExit:

            return rv;
        }
    }
}


