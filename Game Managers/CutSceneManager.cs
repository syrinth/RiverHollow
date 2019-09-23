using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using RiverHollow.Actors;
using RiverHollow.Game_Managers.GUIObjects;
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

        /// <summary>
        /// Loads the Cutscenesinto theManager
        /// </summary>
        /// <param name="Content">The Content pipeline</param>
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

        /// <summary>
        /// When called, iterates over every CutScene in the list and checkto see
        /// if the coditions are right for it to have been called.If so, set it up and 
        /// confirm thatwe are playinga Cutscene.
        /// </summary>
        /// <param name="Content">The Content pipeline</param>
        public static void CheckForTriggedCutscene(int id)
        {
            if (_diCutscenes[id].CanBeTriggered())
            {
                TriggerCutscene(id);
            }
        }

        public static void TriggerCutscene(int id)
        {
            _currentCutscene = _diCutscenes[id];
            _currentCutscene.Setup();
            Playing = true;
        }

        /// <summary>
        /// Runs update on the current Cutscene
        /// </summary>
        /// <param name="gTime">The Gametime info for timing</param>
        public static void Update(GameTime gTime)
        {
            _currentCutscene.Update(gTime);
        }

        /// <summary>
        /// Dialogue is stored in the CutsceneManager, call this method
        /// to find the requested text entry
        /// </summary>
        /// <param name="cutsceneID">ID of the Cutscene</param>
        /// <param name="stringID">The string ID to query for</param>
        /// <returns></returns>
        public static string GetDialogue(int cutsceneID, string stringID)
        {
            return _diCutsceneDialogue[cutsceneID][stringID];
        }
    }

    public class Cutscene
    {
        #region CutScene Commandinformation
        enum EnumCSCommand { Activate, Speak, Move, Face, Wait, End, Quest, Speed, Text, Background, RemoveBackground };

        /// <summary>
        /// A class to hold the information for a CutSceneCommand step
        /// </summary>
        private class CutSceneCommand
        {
            public EnumCSCommand Command;   //What is the command
            public string[] Data;           //What data does it have
            public bool ActionPerformed;    //Has it been performed yet.

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
        #endregion

        int _iID;
        RHMap _originalMap;
        Vector2 _vOriginalPlayerPos;
        RHMap _cutsceneMap;
        int _iMinTime = -1;                                         //The earliest the cutscene can be triggered
        int _iMaxTime = -1;                                         //The latest the cutscene can be triggered
        int _iCurrentCommand;
        List<Villager> _liUsedNPCs;                                 //The list of NPCs that take part in the cutscene.
        List<KeyValuePair<int, int>> _liReqFriendship;              //The list of required Friendships to trigger the cutscene, key is NPC index
        List<CutSceneCommand> _liCommands;                          //The sequence of commands to follow for the cutscene
        List<string> _liSetupCommands;
        bool _bTriggered;
        bool _bWaitForMove;

        List<WorldActor> _liMoving;
        List<WorldActor> _liToRemove;

        double _dTimer;

        /// <summary>
        /// Constructor for a Cutsceneo bject
        /// </summary>
        /// <param name="id">The Cutscene's global ID</param>
        /// <param name="strData">The list of data associated with it</param>
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
                if (tags[0].Equals("Time"))
                {
                    string[] time = tags[1].Split(' ');
                    _iMinTime = int.Parse(time[0]);
                    _iMaxTime = int.Parse(time[1]);
                }
                else if (tags[0].Equals("Friends"))
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
                _liCommands.Add(new CutSceneCommand(Util.ParseEnum<EnumCSCommand>(tags[0]), (tags.Length > 1 ? tags[1].Split(' ') : null)));
            }
            _liCommands.Add(new CutSceneCommand(EnumCSCommand.End));
        }

        /// <summary>
        /// Updates the current Cutscene so it runs.
        /// </summary>
        /// <param name="gTime">The GameTime object</param>
        public void Update(GameTime gTime)
        {
            //If the Wait command has been called, we need to count down to zero
            if (_dTimer > 0)
            {
                _dTimer -= gTime.ElapsedGameTime.TotalSeconds;
                if (_dTimer <= 0) {
                    _iCurrentCommand++;
                    _dTimer = 0;
                }
            }
            else
            {
                //If someone is currently talking, do NOT process additional tags
                if (!GUIManager.IsTextWindowOpen())                 
                {
                    CutSceneCommand currentCommand = _liCommands[_iCurrentCommand];
                    if (!currentCommand.ActionPerformed)     //If we've already performed the action, do not do it again
                    {
                        bool bGoToNext = false;
                        foreach (string s in currentCommand.Data)   //Need to perform the action for each character
                        {
                            int npcID = -1;
                            string[] sCommandData = s.Split('-');   //split the data into segments
                            switch (currentCommand.Command)
                            {
                                case EnumCSCommand.Activate:
                                    npcID = GetNPCData(sCommandData[0]);
                                    Villager a = _liUsedNPCs.Find(test => test.ID == npcID);
                                    if(a != null)
                                    {
                                        a.Activate(true);
                                    }
                                    bGoToNext = true;
                                    break;
                                case EnumCSCommand.Speak:
                                    npcID = GetNPCData(sCommandData[0]);
                                    if (npcID != -1)    //Player should never be talking
                                    {
                                        Villager v = _liUsedNPCs.Find(test => test.ID == npcID);
                                        v.TalkCutscene(CutsceneManager.GetDialogue(_iID, sCommandData[1]));
                                        bGoToNext = true;
                                    }
                                    break;
                                case EnumCSCommand.Background:
                                    GUIManager.AssignBackgroundImage(new GUIImage(new Rectangle(0, 0, 448, 336), (int)(448 * Scale), (int)(336 * Scale), sCommandData[0]));
                                    bGoToNext = true;
                                    break;
                                case EnumCSCommand.RemoveBackground:
                                    GUIManager.ClearBackgroundImage();
                                    bGoToNext = true;
                                    break;
                                case EnumCSCommand.Text:
                                    GUIManager.OpenTextWindow(CutsceneManager.GetDialogue(_iID, sCommandData[0]));
                                    bGoToNext = true;
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
                                    n.SetWalkingDir((DirectionEnum)HandleDir(sCommandData[1]));
                                    n.Idle();
                                    bGoToNext = true;
                                    break;
                                case EnumCSCommand.End:
                                    _bTriggered = true;
                                    PlayerManager.AllowMovement = true;
                                    CutsceneManager.Playing = false;
                                    MapManager.Maps.Remove(_cutsceneMap.Name);
                                    MapManager.FadeToNewMap(_originalMap, _vOriginalPlayerPos);
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

                    //See if the moving characters have finished moving
                    foreach (WorldActor actor in _liMoving)
                    {
                        CheckFinishedMovement(actor);
                    }

                    //Remove any characters that have finished moving from the list
                    foreach (WorldActor actor in _liToRemove)
                    {
                        _liMoving.Remove(actor);
                    }
                    _liToRemove.Clear();

                    //Now that everyone has finished moving, we can go to the next step
                    if (_bWaitForMove && _liMoving.Count == 0)
                    {
                        _bWaitForMove = false;
                        _iCurrentCommand++;
                    }
                }
            }

            //Update the Player character
            PlayerManager.UpdateWorld(gTime);

            //Update the Clone map the cutscene is on
            _cutsceneMap.Update(gTime);
        }

        /// <summary>
        /// Retrieves the desired actor from the list of Actors usedin the Cutscene
        /// </summary>
        /// <param name="npcID">A string value representing the NPC ID</param>
        private WorldActor GetActor(string npcID) {
            int characterID = -1;
            if (!int.TryParse(npcID, out characterID))
            {
                //If the NPC ID could not be converted, effect the player. The string should be 'Player', but does not need to be
                characterID = -1;
            }

            return (characterID == -1 ? (WorldActor)PlayerManager.World : _liUsedNPCs.Find(test => test.ID == characterID));
        }

        /// <summary>
        /// Parses the given string to an int
        /// </summary>
        /// <param name="npcID">The NPcId to get, 'Player' if we are effecting the player.</param>
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

        /// <summary>
        /// Converts the string to an int directional value
        /// </summary>
        /// <param name="str">The directional string to convert</param>
        /// <returns>The int directional value</returns>
        private int HandleDir(string str)
        {
            int rv = -1;
            if (str.Contains("Up")) { rv = 0; }
            else if (str.Contains("Down")) { rv = 1; }
            else if (str.Contains("Right")) { rv = 2; }
            else if (str.Contains("Left")) { rv = 3; }

            return rv;
        }

        /// <summary>
        /// Tells the WorldActor to movethe given number of tiles and adds them
        /// to thelist of moving NPCs.
        /// 0-Up 1-Down 2-Right 3-Left
        /// </summary>
        /// <param name="characterID">The Id of the NPC we're actingo n</param>
        /// <param name="numSquares">How many squares to move</param>
        /// <param name="dir">The directional to move in</param>
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

        /// <summary>
        /// Checks whether the given WorldActor has arrived at their destination.
        /// If they have, make them Idle, and then add them to the ToRemove list.
        /// </summary>
        /// <param name="c">The WorldActor to check</param>
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

        /// <summary>
        /// Sets up the Cutscene for playingbased off it's datas
        /// </summary>
        public void Setup()
        {
            //Stop the player from moving
            PlayerManager.AllowMovement = false;

            _originalMap = MapManager.CurrentMap;
            _vOriginalPlayerPos = PlayerManager.World.Position;

            //Iterates over all of the setup commands
            foreach (string s in _liSetupCommands)
            {
                string[] tags = s.Split(':');

                //Clone the map and put the player on it and tell the MapManager
                //that the new clone map is the current one
                if (tags[0].Equals("Map"))
                {
                    string mapName = tags.Length == 1 ? PlayerManager.CurrentMap : tags[1];
                    _cutsceneMap = new RHMap(MapManager.Maps[mapName]);
                    MapManager.Maps.Add(_cutsceneMap.Name, _cutsceneMap);
                    MapManager.CurrentMap = _cutsceneMap;
                    PlayerManager.CurrentMap = _cutsceneMap.Name;
                }

                //Set thePlayer to the given position
                if (tags[0].Equals("Player"))
                {
                    if (tags.Length > 1)
                    {
                        PlayerManager.World.Position = Util.SnapToGrid(_cutsceneMap.GetCharacterSpawn(tags[1]));
                    }
                }
                else if (tags[0].Equals("Actors"))
                {
                    //Find all the NPCs that are going to be used in this Cutscene,
                    //and add them to the Clone map at the given positions.
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
                else if (tags[0].Equals("Deactivate"))
                {
                    //Find all the NPC IDs for the NPCs that will start deactivated
                    //and then deactivate them.
                    string[] friends = tags[1].Split(' ');
                    foreach (string npcIDs in friends)
                    {
                        foreach (Villager v in _liUsedNPCs)
                        {
                            if (v.ID == int.Parse(npcIDs))
                            {
                                v.Activate(false);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check the various map conditions to see if the Cutscene should be triggered
        /// </summary>
        /// <returns>True if the Cutscene should be triggered.</returns>
        public bool CanBeTriggered()
        {
            return !_bTriggered && InTimeWindow() && FriendshipReqs();
        }

        /// <summary>
        /// Confirms whether or not the current time is greater than the minimum start time
        /// and less than the maximum start time.
        /// </summary>
        /// <returns>True if in the activation window.</returns>
        private bool InTimeWindow()
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
        
        /// <summary>
        /// Confirms whether the Friendship level requirements have been met for the given NPCs
        /// </summary>
        /// <returns>True if met.</returns>
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