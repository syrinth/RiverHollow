using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using RiverHollow.Actors;
using RiverHollow.Misc;
using RiverHollow.Tile_Engine;
using System;
using System.Collections.Generic;

using static RiverHollow.Game_Managers.GameManager;
namespace RiverHollow.Game_Managers
{
    static class CutsceneManager
    {
        static Cutscene _currentCutscene;
        static Dictionary<int, Cutscene> _diCutscenes;
        static Dictionary<int, Dictionary<string, string>> _diCutsceneDialogue;
        public static bool Playing;

        public static void LoadContent(ContentManager Content)
        {
            Playing = false;
            _diCutscenes = new Dictionary<int, Cutscene>();
            _diCutsceneDialogue = new Dictionary<int, Dictionary<string, string>>();

            //We need to do this bullshit because the god damn XML Importer can't have nested Dictionaries. WE MAKE OUR OWN!
            Dictionary<int, List<string>> dataList = Content.Load<Dictionary<int, List<string>>>(@"Data\Text Files\Dialogue\CutsceneDialogue");
            foreach (KeyValuePair<int, List<string>> kvp in dataList)
            {
                Dictionary<string, string> dss = new Dictionary<string, string>();
                foreach (string s in kvp.Value)
                {
                    string newVal = s.Split(new[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries)[0];
                    if (newVal.Contains(":"))
                    {
                        string[] tagSplit = newVal.Split(':');
                        dss[tagSplit[0]] = tagSplit[1];
                    }
                }
                _diCutsceneDialogue.Add(kvp.Key, dss);
            }

            Dictionary<int, List<string>> rawData = Content.Load<Dictionary<int, List<string>>>(@"Data\CutScenes");
            foreach (KeyValuePair<int, List<string>> kvp in rawData)
            {
                _diCutscenes.Add(kvp.Key, new Cutscene(kvp.Key, kvp.Value));
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

        public static string GetDialogue(int cutsceneID, string stringID)
        {
            return _diCutsceneDialogue[cutsceneID][stringID];
        }
    }

    public class Cutscene
    {
        enum EnumCSCommand { Speak, Move, Face, Wait, End, Quest, Speed };
        private class CutSceneCommand
        {
            public EnumCSCommand Command;
            public string[] Data;
            public bool ActionPerformed;

            public CutSceneCommand(EnumCSCommand command, string[] v = null)
            {
                ActionPerformed = false;
                Command = command;
                if (v == null)
                {
                    Data = new string[1];
                    Data[0] = "";
                }
                else
                {
                    Data = v;
                }
            }
        }

        int _iID;
        RHMap _cutsceneMap;
        string _sTriggerMap;
        int _iMinTime = -1;                                         //The earliest the cutscene can be triggered
        int _iMaxTime = -1;                                         //The latest the cutscene can be triggered
        int _iCurrentCommand;
        List<Villager> _liUsedNPCs;                                      //The list of NPCs that take part in the cutscene.
        List<KeyValuePair<int, int>> _liReqFriendship;              //The list of required Friendships to trigger the cutscene, key is NPC index
        List<CutSceneCommand> _liCommands;                                   //The sequence of commands to follow for the cutscene
        List<string> _liSetupCommands;
        bool _bTriggered;
        bool _bWaitForMove;

        List<WorldActor> _liMoving;
        List<WorldActor> _liToRemove;

        double _dTimer;

        public Cutscene(int id, List<string> strData)
        {
            _iID = id;
            _bTriggered = false;
            _bWaitForMove = false;
            _iCurrentCommand = 0;

            _liUsedNPCs = new List<Villager>();
            _liCommands = new List<CutSceneCommand>();
            _liReqFriendship = new List<KeyValuePair<int, int>>();
            _liSetupCommands = new List<string>();
            _liMoving = new List<WorldActor>();
            _liToRemove = new List<WorldActor>();

            //Get the cutscene triggers
            string[] triggers = Util.FindTags(strData[0]);
            foreach (string s in triggers)
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
                    foreach (string f in friend)
                    {
                        string[] friendData = f.Split('-');
                        _liReqFriendship.Add(new KeyValuePair<int, int>(int.Parse(friendData[0]), int.Parse(friendData[1])));
                    }
                }
            }

            //Get the cutscene setup tags
            _liSetupCommands.AddRange(Util.FindTags(strData[1]));

            //Get the sequence of commands to run during the cutscene
            string[] commands = Util.FindTags(strData[2]);
            foreach (string s in commands)
            {
                string[] tags = s.Split(':');
                _liCommands.Add(new CutSceneCommand(Util.ParseEnum<EnumCSCommand>(tags[0]), tags[1].Split(' ')));
            }
            _liCommands.Add(new CutSceneCommand(EnumCSCommand.End));
        }

        public void Update(GameTime gameTime)
        {
            if (_dTimer > 0)
            {
                _dTimer -= gameTime.ElapsedGameTime.TotalSeconds;
                if (_dTimer <= 0) {
                    _iCurrentCommand++;
                    _dTimer = 0;
                }
            }
            else
            {
                if (!GUIManager.IsTextWindowOpen())                 //If someone is currently talking, do NOT process additional tags
                {
                    CutSceneCommand currentCommand = _liCommands[_iCurrentCommand];
                    if (!currentCommand.ActionPerformed)
                    {
                        bool bGoToNext = false;
                        foreach (string s in currentCommand.Data)
                        {
                            string[] sCommandData = s.Split('-');
                            int npcID = -1;

                            switch (currentCommand.Command)
                            {
                                case EnumCSCommand.Speak:
                                    npcID = GetNPCData(sCommandData[0]);
                                    if (npcID != -1)
                                    {
                                        Villager v = _liUsedNPCs.Find(test => test.ID == npcID);
                                        v.TalkCutscene(CutsceneManager.GetDialogue(_iID, sCommandData[1]));
                                        bGoToNext = true;
                                    }
                                    break;
                                case EnumCSCommand.Move:
                                    _bWaitForMove = true;
                                    AssignMovement(sCommandData[0], int.Parse(sCommandData[1]), HandleDir(sCommandData[2]));
                                    break;
                                case EnumCSCommand.Wait:
                                    _dTimer = double.Parse(sCommandData[0]);
                                    break;
                                case EnumCSCommand.Quest:
                                    PlayerManager.AddToQuestLog(GameManager.DiQuests[int.Parse(sCommandData[0])]);
                                    bGoToNext = true;
                                    break;
                                case EnumCSCommand.Speed:
                                    WorldActor c = GetActor(sCommandData[0]);
                                    c.SpdMult = float.Parse(sCommandData[1]);
                                    bGoToNext = true;
                                    break;
                                case EnumCSCommand.Face:
                                    WorldActor n = GetActor(sCommandData[0]);
                                    n.SetWalkingDir((WorldActor.DirectionEnum)HandleDir(sCommandData[1]));
                                    n.Idle();
                                    bGoToNext = true;
                                    break;
                                case EnumCSCommand.End:
                                    _bTriggered = true;
                                    PlayerManager.AllowMovement = true;
                                    CutsceneManager.Playing = false;
                                    MapManager.CurrentMap = MapManager.Maps[_cutsceneMap.Name];
                                    GUIManager.SlowFadeOut();
                                    break;
                            }
                        }

                        //After all command tags have been processed, set the
                        //current commands actionPerformed to true so it's not processed again
                        currentCommand.ActionPerformed = true;
                        if (bGoToNext)
                        {
                            _iCurrentCommand++;
                        }
                    }

                    foreach (WorldActor actor in _liMoving)
                    {
                        CheckFinishedMovement(actor);
                    }
                    foreach (WorldActor actor in _liToRemove)
                    {
                        _liMoving.Remove(actor);
                    }
                    _liToRemove.Clear();

                    if (_bWaitForMove && _liMoving.Count == 0)
                    {
                        _bWaitForMove = false;
                        _iCurrentCommand++;
                    }
                }
            }
            PlayerManager.UpdateWorld(gameTime);
            _cutsceneMap.Update(gameTime);
        }

        private WorldActor GetActor(string npcID) {
            int characterID = -1;
            if (!int.TryParse(npcID, out characterID))
            {
                //If the NPC ID could not be converted, effect the player. The string should be 'Player'
                characterID = -1;
            }
            return (characterID == -1 ? (WorldActor)PlayerManager.World : _liUsedNPCs.Find(test => test.ID == characterID));
        }

        private int GetNPCData(string npcID)
        {
            int rv = -1;
            if (!int.TryParse(npcID, out rv))
            {
                //If the NPC ID could not be converted, effect the player. The string should be 'Player'
                rv = -1;
            }
            return rv;
        }

        private int HandleDir(string str)
        {
            int rv = -1;
            if (str.Contains("Up")) { rv = 0; }
            else if (str.Contains("Down")) { rv = 1; }
            else if (str.Contains("Right")) { rv = 2; }
            else if (str.Contains("Left")) { rv = 3; }

            return rv;
        }

        //0-Up 1-Down 2-Right 3-Left
        private void AssignMovement(string characterID, int numSquares, int dir)
        {
            WorldActor c = GetActor(characterID);
            if (c.MoveToLocation == Vector2.Zero)
            {
                Vector2 vec = Vector2.Zero;
                switch (dir)
                {
                    case 0:
                        vec = new Vector2(0, -numSquares * TileSize);
                        break;
                    case 1:
                        vec = new Vector2(0, numSquares * TileSize);
                        break;
                    case 2:
                        vec = new Vector2(numSquares * TileSize, 0);
                        break;
                    case 3:
                        vec = new Vector2(-numSquares * TileSize, 0);
                        break;

                }
                c.SetMoveObj(c.Position + vec);
                if (!_liMoving.Contains(c))
                {
                    _liMoving.Add(c);
                }
            }
        }

        private void CheckFinishedMovement(WorldActor c)
        {
            if (c.Position == c.MoveToLocation)
            {
                if (!_liToRemove.Contains(c))
                {
                    _liToRemove.Add(c);
                }
                c.SetMoveObj(Vector2.Zero);
                c.Idle();
            }
        }

        public void Setup()
        {
            PlayerManager.AllowMovement = false;
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
                    PlayerManager.World.Position = Util.SnapToGrid(_cutsceneMap.GetCharacterSpawn(tags[1]));
                }
                else if (tags[0].Equals("actors"))
                {
                    string[] friend = tags[1].Split(' ');
                    foreach (string f in friend)
                    {
                        string[] friendData = f.Split('-');
                        Villager n = new Villager(ObjectManager.DiNPC[int.Parse(friendData[0])])
                        {
                            CurrentMapName = _cutsceneMap.Name,
                            Position = Util.SnapToGrid(_cutsceneMap.GetCharacterSpawn(friendData[1]))
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
            if (_iMinTime != -1)
            {
                if (_iMinTime > GameCalendar.CurrentHour) { goto timeExit; }
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
                if (ObjectManager.DiNPC[kvp.Key].FriendshipPoints < kvp.Value) { goto friendshipExit; }
            }

            rv = true;
            friendshipExit:

            return rv;
        }
    }
}