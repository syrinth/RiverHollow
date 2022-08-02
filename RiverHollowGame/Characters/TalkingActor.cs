using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.Misc;
using RiverHollow.SpriteAnimations;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.TravelManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Characters
{
    public abstract class TalkingActor : WorldActor
    {
        protected ActorFaceEnum _eFaceEnum;
        protected List<ActorFaceEnum> _liActorFaceQueue;
        protected string _sPortrait;
        public string Portrait => _sPortrait;

        protected Dictionary<string, TextEntry> _diDialogue;

        public static List<int> FriendRange = new List<int> { 0, 10, 40, 100, 200, 600, 800, 1200, 1600, 2000 };
        public int FriendshipPoints = 1600;

        protected bool _bHasTalked;
        protected RHTask _assignedTask;

        public bool CanGiveGift = true;

        protected List<string> _liSpokenKeys;

        public TalkingActor(int id) : base(id)
        {
            _bCanTalk = true;
            _liActorFaceQueue = new List<ActorFaceEnum>();
            _liSpokenKeys = new List<string>();
        }

        public TalkingActor() : base()
        {
            _bCanTalk = true;
            _liActorFaceQueue = new List<ActorFaceEnum>();
            _liSpokenKeys = new List<string>();
        }

        public override void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            base.Draw(spriteBatch, useLayerDepth);
            if (_bOnTheMap && _assignedTask?.TaskState == TaskStateEnum.Assigned)
            {
                spriteBatch.Draw(DataManager.GetTexture(DataManager.DIALOGUE_TEXTURE), new Rectangle((int)Position.X, (int)Position.Y - 32, 16, 16), new Rectangle(224, 16, 16, 16), Color.White, 0, Vector2.Zero, SpriteEffects.None, Constants.MAX_LAYER_DEPTH);
            }
        }

        public bool AssignTask(RHTask task)
        {
            bool rv = false;

            if (_assignedTask == null)
            {
                rv = true;
                _assignedTask = task;
            }

            return rv;
        }

        public virtual TextEntry Gift(Item item) { return null; }
        public virtual TextEntry JoinParty() { return null; }
        public virtual void OpenShop() { }
        public virtual TextEntry OpenRequests() { return null; }

        public virtual void StopTalking() {
            GameManager.SetCurrentNPC(null);
            ResetActorFace();

            if(_assignedTask?.TaskState == TaskStateEnum.Talking)
            {
                _assignedTask.AddTaskToLog();
                _assignedTask = null;
            }
        }

        public override void ProcessRightButtonClick()
        {
            StartConversation();
        }

        /// <summary>
        ///  Retrieves any opening text, processes it, then opens a text window
        /// </summary>
        /// <param name="facePlayer">Whether the NPC should face the player. Mainly used to avoid messing up a cutscene</param>
        public virtual void StartConversation(bool facePlayer = true)
        {
            FacePlayer(true);
            GUIManager.OpenTextWindow(GetOpeningText(), this, true, true);
        }

        /// <summary>
        /// Used when already talking to an NPC, gets the next dialog tag in the conversation
        /// and opens a new window for it.
        /// </summary>
        /// <param name="dialogTag">The dialog tag to talk with</param>
        public void ContinueConversation(string dialogTag)
        {
            TextEntry text = null;
            if (_diDialogue.ContainsKey(dialogTag))
            {
                text = _diDialogue[dialogTag];
            }

            GUIManager.OpenTextWindow(text, this, true, true);
        }

        public void TalkCutscene(TextEntry cutsceneEntry)
        {
            GUIManager.OpenTextWindow(cutsceneEntry, this, true, true);
        }

        /// <summary>
        /// Stand-in Virtual method to be overrriden. Should never get called.
        /// </summary>
        public virtual TextEntry GetOpeningText() { return new TextEntry(); }

        /// <summary>
        /// Base method to get a line of dialog from the dialog dictionary.
        /// 
        /// Mostly used for the "Talk" parameter or if the TalkingActor has no other options.
        /// </summary>
        /// <returns>The dialog string for the entry.</returns>
        public TextEntry GetDailyDialogue()
        {
            _bHasTalked = true;
            PriorityQueue<TextEntry> keyPool = new PriorityQueue<TextEntry>();
            foreach (TextEntry entry in _diDialogue.Values)
            {
                if (entry.Validate(this))
                {
                    keyPool.Enqueue(entry, entry.Priority);
                }
            }

            List<TextEntry> possibles = keyPool.DequeueAllLowest();

            return possibles[RHRandom.Instance().Next(0, possibles.Count - 1)];
        }

        /// <summary>
        /// Retrieves the specified entry from the _diDictionaryand calls Util.ProcessTexton it.
        /// </summary>
        /// <param name="entry">The key of the entry to get from the Dictionary</param>
        /// <returns>The processed string text for the entry </returns>
        public virtual TextEntry GetDialogEntry(string entry)
        {
            TextEntry rv = new TextEntry(entry);

            if (_diDialogue.ContainsKey(entry))
            {
                rv = _diDialogue[entry];
            }

            return rv;
        }

        /// <summary>
        /// Determines the level of Friendship based off of how many Friendship points the Actor has.
        /// </summary>
        /// <returns></returns>
        public int GetFriendshipLevel()
        {
            int rv = 0;
            for (int i = 0; i < FriendRange.Count; i++)
            {
                if (FriendshipPoints >= FriendRange[i])
                {
                    rv = i;
                }
            }

            return rv;
        }

        /// <summary>
        /// Creates the queue of faces the TalkingActor will progress through as they talk.
        /// </summary>
        /// <param name="value">A '-' delimited list of facial expressions corresponding to ActoEnumFaces</param>
        public void QueueActorFace(string value)
        {
            foreach (string s in value.Split('-'))
            {
                _liActorFaceQueue.Add(Util.ParseEnum<ActorFaceEnum>(s));
            }
        }

        /// <summary>
        /// Removes the first entry from list of queued faces and sets it as the current face.
        /// Calling this again on an empty list resets the face to default
        /// </summary>
        public void DeQueueActorFace()
        {
            if (_liActorFaceQueue.Count == 0) { ResetActorFace(); }
            else
            {
                _eFaceEnum = _liActorFaceQueue[0];
                _liActorFaceQueue.RemoveAt(0);
            }
        }

        /// <summary>
        /// Clears the FaceQueue and resets the face to Default
        /// </summary>
        public void ResetActorFace()
        {
            _liActorFaceQueue.Clear();
            _eFaceEnum = ActorFaceEnum.Default;
        }

        public Rectangle GetPortraitRectangle()
        {
            int startX = 0;
            int width = 48;
            startX += (int)_eFaceEnum * width;
            return new Rectangle(startX, 0, width, 60);
        }

        public void AddSpokenKey(string key)
        {
            _liSpokenKeys.Add(key);
        }
    }
}
