using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using RiverHollow.Characters;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.Misc;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using System.Linq;

using static RiverHollow.Utilities.Enums;
using static RiverHollow.Game_Managers.SaveManager;

namespace RiverHollow.Game_Managers
{
    static class CutsceneManager
    {
        static Cutscene _currentCutscene;
        static Dictionary<int, Cutscene> _diCutscenes;
        static Dictionary<int, Dictionary<string, TextEntry>> _diAllCutsceneDialogue;
        public static bool Playing;
        
        /// <summary>
        /// Loads the Cutscenesinto theManager
        /// </summary>
        /// <param name="Content">The Content pipeline</param>
        public static void LoadContent(ContentManager Content)
        {
            Playing = false;
            _diCutscenes = new Dictionary<int, Cutscene>();
            _diAllCutsceneDialogue = new Dictionary<int, Dictionary<string, TextEntry>>();
            
            foreach(string s in Directory.GetFiles(@"Content\Data\Text Files\Dialogue\Cutscenes"))
            {
                string fileName = s;
                Util.ParseContentFile(ref fileName);
                int fileID = int.Parse(Path.GetFileName(fileName).Replace("Cutscene_", "").Split('.')[0]);
                Dictionary<int, string> dss = Content.Load<Dictionary<int, string>>(fileName);

                Dictionary<string, TextEntry> entryDictionary = new Dictionary<string, TextEntry>();
                foreach (KeyValuePair<int, string> kvp in dss)
                {
                    entryDictionary[kvp.Key.ToString()] = new TextEntry(kvp.Key.ToString(), Util.DictionaryFromTaggedString(kvp.Value));
                }

                _diAllCutsceneDialogue[fileID] = entryDictionary;
            }

            Dictionary<int, List<string>> rawData = Content.Load<Dictionary<int, List<string>>>(@"Data\CutScenes");
            foreach (KeyValuePair<int, List<string>> kvp in rawData)
            {
                _diCutscenes.Add(kvp.Key, new Cutscene(kvp.Key, kvp.Value));
            }
        }

        /// <summary>
        /// When called, iterates over every CutScene in the list and checkto see
        /// if the coditions are right for it to have been called. If so, set it up and 
        /// confirm that we are playing a Cutscene.
        /// </summary>
        public static void CheckForTriggedCutscene(int id)
        {
            if (_diCutscenes[id].CanBeTriggered())
            {
                TriggerCutscene(id);
            }
        }

        public static void TriggerCutscene(int id, RHTask triggerTask = null)
        {
            GUICursor.ResetCursor();
            _currentCutscene = _diCutscenes[id];
            _currentCutscene.Setup(triggerTask);
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
        public static TextEntry GetDialogue(int cutsceneID, string stringID)
        {
            return _diAllCutsceneDialogue[cutsceneID][stringID];
        }

        public static void SkipCutscene()
        {
            _currentCutscene.Skip();
        }

        public static void UnsetCurrentCutscene()
        {
            _currentCutscene = null;
        }

        public static List<CutsceneData> SaveCutscenes()
        {
            List<CutsceneData> rv = new List<CutsceneData>();
            foreach (Cutscene c in _diCutscenes.Values.ToList())
            {
                rv.Add(c.SaveData());
            }

            return rv;
        }

        public static void LoadCutscenes(List<CutsceneData> dataList)
        {
            for (int i = 0; i < dataList.Count; i++)
            {
                _diCutscenes[i].LoadData(dataList[i]);
            }
        }
    }

    public class Cutscene
    {
        #region CutScene Commandinformation
        enum CutsceneCommandEnum { Activate, Background, BackgroundRemove, End, Face, GoTo, Introduce, ItemID, Move, MoveToTown, Sound, Speak, Speed, Task, Text, Wait };

        /// <summary>
        /// A class to hold the information for a CutSceneCommand step
        /// </summary>
        private class CutSceneCommand
        {
            public CutsceneCommandEnum Command;   //What is the command
            public string[] Data;           //What data does it have
            public bool ActionPerformed;    //Has it been performed yet.

            public CutSceneCommand(CutsceneCommandEnum command, string[] v = null)
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
        RHMap _cutsceneMap;
        RHMap _originalMap;
        Point _pOriginalPlayerPos;
        int _iTaskID = -1;
        int _iMinTime = -1;                                         //The earliest the cutscene can be triggered
        int _iMaxTime = -1;                                         //The latest the cutscene can be triggered
        int _iCurrentCommand;
        List<Actor> _liUsedNPCs;                                 //The list of NPCs that take part in the cutscene.
        List<KeyValuePair<int, int>> _liReqFriendship;              //The list of required Friendships to trigger the cutscene, key is NPC index
        List<CutSceneCommand> _liCommands;                          //The sequence of commands to follow for the cutscene
        List<string> _liSetupCommands;
        bool _bTriggered;
        bool _bWaitForMove;
        RHTask _triggerTask;

        Dictionary<Actor, DirectionEnum> _diMoving;
        List<Actor> _liToRemove;

        RHTimer _timer;

        /// <summary>
        /// Constructor for a Cutscene object
        /// </summary>
        /// <param name="id">The Cutscene's global ID</param>
        /// <param name="strData">The list of data associated with it</param>
        public Cutscene(int id, List<string> strData)
        {
            _iID = id;
            _bTriggered = false;
            _bWaitForMove = false;
            _iCurrentCommand = 0;

            _liUsedNPCs = new List<Actor>();
            _liCommands = new List<CutSceneCommand>();
            _liReqFriendship = new List<KeyValuePair<int, int>>();
            _liSetupCommands = new List<string>();
            _diMoving = new Dictionary<Actor, DirectionEnum>();
            _liToRemove = new List<Actor>();

            //Get the cutscene triggers
            string[] triggers = Util.FindTags(strData[0]);
            foreach (string s in triggers)
            {
                string[] tags = s.Split(':');
                //Parsing for important data
                if (tags[0].Equals("Time"))
                {
                    string[] time = Util.FindArguments(tags[1]);
                    _iMinTime = int.Parse(time[0]);
                    _iMaxTime = int.Parse(time[1]);
                }
                else if (tags[0].Equals("Friends"))
                {
                    string[] friend = Util.FindParams(tags[1]);
                    foreach (string f in friend)
                    {
                        string[] friendData = Util.FindArguments(f);
                        _liReqFriendship.Add(new KeyValuePair<int, int>(int.Parse(friendData[0]), int.Parse(friendData[1])));
                    }
                }
                else if (tags[0].Equals("TaskID"))
                {
                    _iTaskID = int.Parse(tags[1]);
                }
            }

            //Get the cutscene setup tags
            _liSetupCommands.AddRange(Util.FindTags(strData[1]));

            //Get the sequence of commands to run during the cutscene
            string[] commands = Util.FindTags(strData[2]);
            foreach (string s in commands)
            {
                string[] tags = s.Split(':');
                _liCommands.Add(new CutSceneCommand(Util.ParseEnum<CutsceneCommandEnum>(tags[0]), (tags.Length > 1 ? Util.FindParams(tags[1]) : null)));
            }
            _liCommands.Add(new CutSceneCommand(CutsceneCommandEnum.End));
        }

        /// <summary>
        /// Updates the current Cutscene so it runs.
        /// </summary>
        /// <param name="gTime">The GameTime object</param>
        public void Update(GameTime gTime)
        {
            //If the Wait command has been called, we need to count down to zero
            if(_timer == null || _timer.TickDown(gTime))
            {
                if (_timer != null)
                {
                    _timer = null;
                    _iCurrentCommand++;
                }
                //If someone is currently talking, do NOT process additional tags
                if (!GUIManager.IsTextWindowOpen())                 
                {
                    CutSceneCommand currentCommand = _liCommands[_iCurrentCommand];
                    if (!currentCommand.ActionPerformed)     //If we've already performed the action, do not do it again
                    {
                        bool goToNext = false;
                        foreach (string s in currentCommand.Data)   //Need to perform the action for each character
                        {
                            int npcID = -1;
                            string[] sCommandData = Util.FindArguments(s);   //split the data into segments
                            Actor actor;
                            switch (currentCommand.Command)
                            {
                                case CutsceneCommandEnum.Activate:
                                    goToNext = true;
                                    Activate(sCommandData);
                                    break;
                                case CutsceneCommandEnum.Speak:
                                    npcID = GetNPCData(sCommandData[0]);
                                    if (npcID != -1)    //Player should never be talking
                                    {
                                        TownManager.DIVillagers[npcID].TalkCutscene(CutsceneManager.GetDialogue(_iID, sCommandData[1]));
                                        goToNext = true;
                                    }
                                    break;
                                case CutsceneCommandEnum.Background:
                                    goToNext = true;
                                    GUIManager.AssignBackgroundImage(new GUIImage(DataManager.GetTexture(sCommandData[0])));
                                    break;
                                case CutsceneCommandEnum.BackgroundRemove:
                                    goToNext = true;
                                    GUIManager.ClearBackgroundImage();
                                    break;
                                case CutsceneCommandEnum.GoTo:
                                    _bWaitForMove = true;
                                    Actor c = GetActor(sCommandData[0]);
                                    DirectionEnum d = DirectionEnum.None;
                                    if (sCommandData.Length > 2) { d = Util.ParseEnum<DirectionEnum>(sCommandData[2]); }

                                    Point collisionCenter = c.CollisionCenter;
                                    var path = TravelManager.FindPathToLocation(ref collisionCenter, _cutsceneMap.DictionaryCharacterLayer[sCommandData[1]].Location);
                                    c.SetPath(path);
                                    c.SetMoveTo(path[0].Position);
                                    if (!_diMoving.ContainsKey(c))
                                    {
                                        _diMoving[c] = d;
                                    }
                                    break;
                                case CutsceneCommandEnum.ItemID:
                                    goToNext = true;
                                    GiveItem(sCommandData);
                                    break;
                                case CutsceneCommandEnum.Text:
                                    goToNext = true;
                                    GUIManager.OpenTextWindow(CutsceneManager.GetDialogue(_iID, sCommandData[0]));
                                    break;
                                case CutsceneCommandEnum.Move:
                                    _bWaitForMove = true;
                                    DirectionEnum faceDir = DirectionEnum.None;
                                    if(sCommandData.Length > 3) { faceDir = Util.ParseEnum<DirectionEnum>(sCommandData[3]); }
                                    AssignMovement(sCommandData[0], int.Parse(sCommandData[1]), Util.ParseEnum<DirectionEnum>(sCommandData[2]), faceDir);
                                    break;                     
                                case CutsceneCommandEnum.Wait:
                                    _timer = new RHTimer(double.Parse(sCommandData[0]));
                                    break;
                                case CutsceneCommandEnum.Task:
                                    goToNext = true;
                                    SetTask(sCommandData);
                                    break;
                                case CutsceneCommandEnum.Sound:
                                    goToNext = true;
                                    SoundManager.PlayEffect(Util.ParseEnum<SoundEffectEnum>(sCommandData[0]));
                                    break;
                                case CutsceneCommandEnum.Speed:
                                    goToNext = true;
                                    actor = GetActor(sCommandData[0]);
                                    if (sCommandData[1].Equals("P"))
                                    {
                                        actor.SpdMult = Constants.NORMAL_SPEED;
                                    }
                                    else if (sCommandData[1].Equals("N"))
                                    {
                                        actor.SpdMult = Constants.NPC_WALK_SPEED;
                                    }
                                    else
                                    {
                                        actor.SpdMult = float.Parse(sCommandData[1]);
                                    }
                                    break;
                                case CutsceneCommandEnum.Face:
                                    actor = GetActor(sCommandData[0]);
                                    actor.SetFacing(Util.ParseEnum<DirectionEnum>(sCommandData[1]));
                                    actor.PlayAnimationVerb(VerbEnum.Idle);

                                    if (sCommandData.Length == 3)
                                    {
                                        _timer = new RHTimer(double.Parse(sCommandData[2]));
                                    }
                                    else
                                    {
                                        goToNext = true;
                                    }
                                    break;
                                case CutsceneCommandEnum.MoveToTown:
                                    goToNext = true;
                                    MoveToTown(sCommandData);                                   
                                    break;
                                case CutsceneCommandEnum.Introduce:
                                    goToNext = true;
                                    IntroduceActor(sCommandData);
                                    break;
                                case CutsceneCommandEnum.End:
                                    EndCutscene();
                                    break;

                            }
                        }

                        //After all command tags have been processed, set the
                        //current commands actionPerformed to true so it's not processed again
                        currentCommand.ActionPerformed = true;
                        if (goToNext)
                        {
                            if (currentCommand.Command == CutsceneCommandEnum.ItemID && TownManager.DIVillagers[_liUsedNPCs[0].ID].HasHeldItems())
                            {
                                GUIManager.NewAlertIcon(DataManager.GetGameTextEntry("Alert_Inventory").GetFormattedText(), Color.Red);
                            }
                            _iCurrentCommand++;
                        }
                    }

                    //See if the moving characters have finished moving
                    foreach (var kvp in _diMoving)
                    {
                        CheckFinishedMovement(kvp.Key);
                    }

                    //Remove any characters that have finished moving from the list
                    foreach (Actor actor in _liToRemove)
                    {
                        _diMoving.Remove(actor);
                    }
                    _liToRemove.Clear();

                    //Now that everyone has finished moving, we can go to the next step
                    if (_bWaitForMove && _diMoving.Count == 0)
                    {
                        _bWaitForMove = false;
                        _iCurrentCommand++;
                    }
                }
            }

            //Update the Player character
            PlayerManager.Update(gTime);

            //Update the Clone map the cutscene is on
            _cutsceneMap.Update(gTime);
        }

        /// <summary>
        /// Retrieves the desired actor from the list of Actors usedin the Cutscene
        /// </summary>
        /// <param name="npcID">A string value representing the NPC ID</param>
        private Actor GetActor(string npcID) {
            int characterID = -1;
            if (!int.TryParse(npcID, out characterID))
            {
                //If the NPC ID could not be converted, effect the player. The string should be 'Player', but does not need to be
                characterID = -1;
            }

            return (characterID == -1 ? (Actor)PlayerManager.PlayerActor : _liUsedNPCs.Find(test => test.ID == characterID));
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
        /// Tells the WorldActor to movethe given number of tiles and adds them
        /// to thelist of moving NPCs.
        /// 0-Up 1-Down 2-Right 3-Left
        /// </summary>
        /// <param name="characterID">The Id of the NPC we're actingo n</param>
        /// <param name="numSquares">How many squares to move</param>
        /// <param name="dir">The directional to move in</param>
        private void AssignMovement(string characterID, int numSquares, DirectionEnum dir, DirectionEnum faceDir)
        {
            Actor c = GetActor(characterID);
            if (!c.HasMovement())
            {
                Point vec = Util.MultiplyPoint(Util.GetPointFromDirection(dir), numSquares * Constants.TILE_SIZE);
                
                c.SetMoveTo(c.CollisionBoxLocation + vec);
                if (!_diMoving.ContainsKey(c))
                {
                    _diMoving[c] = faceDir;
                }
            }
        }

        /// <summary>
        /// Checks whether the given WorldActor has arrived at their destination.
        /// If they have, make them Idle, and then add them to the ToRemove list.
        /// </summary>
        /// <param name="c">The WorldActor to check</param>
        private void CheckFinishedMovement(Actor c)
        {
            if (!c.HasMovement())
            {
                if (!_liToRemove.Contains(c))
                {
                    _liToRemove.Add(c);
                }
                c.SetMoveTo(Point.Zero);

                if (_diMoving[c] != DirectionEnum.None)
                {
                    c.SetFacing(_diMoving[c]);
                    c.PlayAnimationVerb(VerbEnum.Idle);
                }
            }
        }

        /// <summary>
        /// Sets up the Cutscene for playingbased off it's datas
        /// </summary>
        public void Setup(RHTask triggerTask = null)
        {
            _triggerTask = triggerTask;
            //Stop the player from moving
            PlayerManager.AllowMovement = false;

            GUIManager.AddSkipCutsceneButton();

            _originalMap = MapManager.CurrentMap;
            _pOriginalPlayerPos = PlayerManager.PlayerActor.CollisionBoxLocation;

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
                        PlayerManager.PlayerActor.SetPosition(Util.SnapToGrid(_cutsceneMap.GetCharacterSpawn(tags[1])));
                        PlayerManager.PlayerActor.PlayAnimation(VerbEnum.Idle);
                    }
                }
                else if (tags[0].Equals("Actors"))
                {
                    //Find all the NPCs that are going to be used in this Cutscene,
                    //and add them to the Clone map at the given positions.
                    string[] friend = Util.FindParams(tags[1]);
                    foreach (string f in friend)
                    {
                        string[] friendData = Util.FindArguments(f);
                        Actor act = null;
                        if (TownManager.DIVillagers.ContainsKey(int.Parse(friendData[0])))
                        {
                            int npcID = int.Parse(friendData[0]);
                            act = new Villager(npcID, DataManager.ActorData[npcID]);
                        }
                        else { act = DataManager.CreateNPCByIndex(int.Parse(friendData[0])); }
                        act.CurrentMapName = _cutsceneMap.Name;
                        act.SetPosition(Util.SnapToGrid(_cutsceneMap.GetCharacterSpawn(friendData[1])));
                        act.PlayAnimation(VerbEnum.Idle);

                        _cutsceneMap.AddActor(act);
                        _liUsedNPCs.Add(act);
                    }
                }
                else if (tags[0].Equals("Deactivate"))
                {
                    string[] IDs = Util.FindParams(tags[1]);
                    foreach (string str in IDs)
                    {
                        GetActor(str).Activate(false);
                    }
                }
                else if (tags[0].Equals("Face"))
                {
                    string[] IDs = Util.FindParams(tags[1]);
                    foreach (string str in IDs)
                    {
                        var args = Util.FindArguments(str);

                        var actor = GetActor(args[0]);
                        actor.SetFacing(Util.ParseEnum<DirectionEnum>(args[1]));
                        actor.PlayAnimation(VerbEnum.Idle);
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
            return !_bTriggered && OnTask() && InTimeWindow() && FriendshipReqs();
        }
        
        private bool OnTask()
        {
            if (_iTaskID == -1)
            {
                return true;
            }
            else {
                return false;
                //MAR
                //return PlayerManager.TaskLog.Contains(DITasks[_iTaskID]);
            }
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
                if (TownManager.DIVillagers[kvp.Key].FriendshipPoints < kvp.Value) { goto friendshipExit; }
            }

            rv = true;
            friendshipExit:

            return rv;
        }

        public void Skip()
        {
            CutsceneManager.UnsetCurrentCutscene();

            for(int i = _iCurrentCommand; i < _liCommands.Count; i++){
                CutSceneCommand currentCommand = _liCommands[i];
                if (!currentCommand.ActionPerformed)     //If we've already performed the action, do not do it again
                {
                    foreach (string s in currentCommand.Data)   //Need to perform the action for each character
                    {
                        string[] sCommandData = Util.FindArguments(s);   //split the data into segments
                        switch (currentCommand.Command)
                        {
                            case CutsceneCommandEnum.Activate:
                                Activate(sCommandData);
                                break;
                            case CutsceneCommandEnum.Introduce:
                                IntroduceActor(sCommandData);
                                break;
                            case CutsceneCommandEnum.ItemID:
                                GiveItem(sCommandData);
                                break;
                            case CutsceneCommandEnum.MoveToTown:
                                MoveToTown(sCommandData);
                                break;
                            case CutsceneCommandEnum.Task:
                                SetTask(sCommandData);
                                break;
                        }
                    }

                    if (currentCommand.Command == CutsceneCommandEnum.ItemID && TownManager.DIVillagers[_liUsedNPCs[0].ID].HasHeldItems())
                    {
                        GUIManager.NewAlertIcon(DataManager.GetGameTextEntry("Alert_Inventory").GetFormattedText(), Color.Red);
                    }

                    //After all command tags have been processed, set the
                    //current commands actionPerformed to true so it's not processed again
                    currentCommand.ActionPerformed = true;
                }
            }

            foreach(Actor act in _liUsedNPCs)
            {
                act.ClearPath();
                act.SetMoveTo(Point.Zero);
            }

            GUIManager.ClearBackgroundImage();

            EndCutscene();
        }

        private void Activate(string[] sCommandData)
        {
            var actor = GetActor(sCommandData[0]);
            actor?.Activate(true);
        }

        private void GiveItem(string[] sCommandData)
        {
            int id = int.Parse(sCommandData[0]);
            int number = sCommandData.Length == 1 ? 1 : int.Parse(sCommandData[1]);

            if (InventoryManager.HasSpaceInInventory(id, number))
            {
                InventoryManager.AddToInventory(id, number);
            }
            else
            {
                TownManager.DIVillagers[_liUsedNPCs[0].ID].AssignItemToNPC(int.Parse(sCommandData[0]), sCommandData.Length == 1 ? 1 : int.Parse(sCommandData[1]));
            }
        }
        private void IntroduceActor(string[] sCommandData)
        {
            int id = int.Parse(sCommandData[0]);
            if (TownManager.DIVillagers.ContainsKey(id))
            {
                TownManager.DIVillagers[id].Introduce();
            }
        }
        private void MoveToTown(string[] sCommandData)
        {
            if (int.TryParse(sCommandData[0], out int characterID))
            {
                TownManager.DIVillagers[characterID].TryToMoveIn();
                TownManager.DIVillagers[characterID].MoveToSpawn();
            }
        }

        private void SetTask(string[] sCommandData)
        {
            TaskManager.GetTaskByID(int.Parse(sCommandData[0]))?.AddTaskToLog(true);
        }

        private void EndCutscene()
        {
            _bTriggered = true;

            GUIManager.CloseTextWindow();

            _liUsedNPCs.ForEach(x => x.Activate(true));
            PlayerManager.PlayerActor.Activate(true);

            PlayerManager.PlayerActor.SpdMult = Constants.NORMAL_SPEED;
            PlayerManager.AllowMovement = true;
            CutsceneManager.Playing = false;
            PlayerManager.PlayerActor.SetMoveTo(Point.Zero);
            PlayerManager.PlayerActor.ClearPath();
            MapManager.Maps.Remove(_cutsceneMap.Name);
            MapManager.FadeToNewMap(_originalMap, _pOriginalPlayerPos, PlayerManager.PlayerActor.Facing, GameManager.CurrentBuilding);
            GUIManager.RemoveSkipCutsceneButton();

            _triggerTask?.EndTask();
        }

        public CutsceneData SaveData()
        {
            return new CutsceneData() { Played = _bTriggered };
        }

        public void LoadData(CutsceneData data)
        {
            _bTriggered = data.Played;
        }

    }
}