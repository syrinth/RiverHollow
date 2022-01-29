using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Characters;
using RiverHollow.Characters.Lite;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.Misc;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.GUIObjects.GUIWindows
{
    public class GUITextSelectionWindow : GUITextWindow
    {
        string _sStatement;
        protected Point _poiMouse = Point.Zero;
        protected GUIImage _giSelection;
        protected int _iKeySelection;

        protected Dictionary<int, SelectionData> _diOptions;

        public GUITextSelectionWindow(TextEntry selectionText, bool open = true)
        {
            _textEntry = selectionText;
            _textEntry.HandlePreWindowActions();
            _diOptions = new Dictionary<int, SelectionData>();
            ConfigureHeight();
            _iKeySelection = 0;
            ConstructSelectionMenu();

            PostParse();

            Setup(open);
        }

        public void PostParse()
        {
            SyncText(_sStatement, true);
            _giText.AnchorToInnerSide(this, SideEnum.TopLeft, GUIManager.STANDARD_MARGIN);
            _giSelection = new GUIImage(new Rectangle(288, 96, 8, 9), GameManager.ScaleIt(8), GameManager.ScaleIt(9), DataManager.DIALOGUE_TEXTURE);
            _giSelection.AnchorAndAlignToObject(_giText, SideEnum.Bottom, SideEnum.Left, GUIManager.STANDARD_MARGIN);
            AddControl(_giSelection);

            AssignToColumn();
        }

        #region Selection Menu Creation
        /// <summary>
        /// Constructs the appropriate Selection Menu based on the TextEntry's selection type
        /// </summary>
        private void ConstructSelectionMenu()
        {
            _sStatement = _textEntry.GetFormattedText();
            switch (_textEntry.SelectionType)
            {
                case TextEntrySelectionEnum.VillageTalk:
                    AddVillagerTalkOptions();
                    break;
                case TextEntrySelectionEnum.MerchantTalk:
                    AddMerchantTalkOptions();
                    break;
                case TextEntrySelectionEnum.YesNo:
                    AddYesNoOptions();
                    break;
                case TextEntrySelectionEnum.Shop:
                    AddShopOptions();
                    break;
                case TextEntrySelectionEnum.Party:
                    AddPartyOptions();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Adds the appropriate Villager talk options
        /// </summary>
        private void AddVillagerTalkOptions()
        {
            if (GameManager.CurrentNPC != null && CurrentNPC.IsActorType(WorldActorTypeEnum.Villager))
            {
                Villager v = (Villager)CurrentNPC;
                List<TextEntry> liCommands = new List<TextEntry> { DataManager.GetGameTextEntry("Selection_Talk") };

                if (v.CanGiveGift) { liCommands.Add(DataManager.GetGameTextEntry("Selection_Gift")); }
                if (v.Combatant && v.GetFriendshipLevel() >= 2) { liCommands.Add(DataManager.GetGameTextEntry("Selection_Party")); }
                if (_textEntry.HasTag("ShipGoods")) { liCommands.Add(DataManager.GetGameTextEntry("Selection_ShipGoods")); }
                if (v.CanBeMarried)
                {
                    if (v.AvailableToDate()) { liCommands.Add(DataManager.GetGameTextEntry("Selection_Date")); }
                    else if (v.AvailableToMarry() && InventoryManager.HasItemInPlayerInventory(int.Parse(DataManager.Config[16]["ItemID"]), 1)) { liCommands.Add(DataManager.GetGameTextEntry("Selection_Propose")); }
                }
                liCommands.Add(DataManager.GetGameTextEntry("Selection_NeverMind"));

                AddOptions(liCommands);
            }
        }

        /// <summary>
        /// Adds the appropriate Merchant talk options
        /// </summary>
        private void AddMerchantTalkOptions()
        {
            if (GameManager.CurrentNPC != null)
            {
                TalkingActor act = CurrentNPC;
                List<TextEntry> liCommands = new List<TextEntry> { DataManager.GetGameTextEntry("Selection_Talk") };
                liCommands.Add(DataManager.GetGameTextEntry("Selection_Buy"));
                liCommands.Add(DataManager.GetGameTextEntry("Selection_Requests"));
                liCommands.Add(DataManager.GetGameTextEntry("Selection_NeverMind"));

                AddOptions(liCommands);
            }
        }

        /// <summary>
        /// Adds the TextEntry options for a Shop
        /// </summary>
        private void AddShopOptions()
        {
            if (GameManager.CurrentNPC != null)
            {
                TalkingActor act = CurrentNPC;
                List<TextEntry> liCommands = new List<TextEntry> { DataManager.GetGameTextEntry("Selection_Talk") };

                if (act.CanGiveGift) { liCommands.Add(DataManager.GetGameTextEntry("Selection_Gift")); }
                liCommands.Add(DataManager.GetGameTextEntry("Selection_Buy"));
                liCommands.Add(DataManager.GetGameTextEntry("Selection_NeverMind"));

                AddOptions(liCommands);
            }
        }

        /// <summary>
        /// Constructs new TextEntry objects for each character in the party
        /// with an Option_# verb attached
        /// </summary>
        private void AddPartyOptions()
        {
            List<TextEntry> liCommands = new List<TextEntry>();

            int i = 0;
            foreach(ClassedCombatant act in PlayerManager.GetParty())
            {
                string verb = "Option_" + i++;
                Dictionary<string, string> stringData = new Dictionary<string, string> { ["Text"] = act.Name, ["TextVerb"] = verb };

                liCommands.Add(new TextEntry(verb, stringData));
            }

            AddOptions(liCommands);
        }

        /// <summary>
        /// Adds the Yes and No TextEntry objects
        /// </summary>
        private void AddYesNoOptions()
        {
            List<TextEntry> liCommands = new List<TextEntry> {
                DataManager.GetGameTextEntry("Selection_Yes"),
                DataManager.GetGameTextEntry("Selection_No")
            };

            AddOptions(liCommands);
        }

        /// <summary>
        /// Adds the given list of TextEntries to the _diOptions dictionary
        /// </summary>
        /// <param name="liCommands"></param>
        private void AddOptions(List<TextEntry> liCommands)
        {
            int key = 0;
            foreach (TextEntry s in liCommands)
            {
                _diOptions.Add(key++, new SelectionData(s));
            }
        }
        #endregion

        public override void Update(GameTime gTime)
        {
            if (_bOpening)
            {
                HandleOpening(gTime);
            }
            else
            {
                if (InputManager.CheckPressedKey(Keys.W) || InputManager.CheckPressedKey(Keys.Up))
                {
                    if (_iKeySelection - 1 >= 0)
                    {
                        _giSelection.AlignToObject(_diOptions[_iKeySelection - 1].GText, SideEnum.Bottom);
                        _iKeySelection--;
                    }
                }
                else if (InputManager.CheckPressedKey(Keys.S) || InputManager.CheckPressedKey(Keys.Down))
                {
                    if (_iKeySelection + 1 < _diOptions.Count)
                    {
                        _giSelection.AlignToObject(_diOptions[_iKeySelection + 1].GText, SideEnum.Bottom);
                        _iKeySelection++;
                    }
                }
                else
                {
                    //Until fixed for specific motion
                    if (_poiMouse != GUICursor.Position.ToPoint() && Contains(GUICursor.Position.ToPoint()))
                    {
                        _poiMouse = GUICursor.Position.ToPoint();
                        if (_iKeySelection - 1 >= 0 && GUICursor.Position.Y < _giSelection.Position().Y)
                        {
                            _giSelection.AlignToObject(_diOptions[_iKeySelection - 1].GText, SideEnum.Bottom);
                            _iKeySelection--;
                        }
                        else if (_iKeySelection + 1 < _diOptions.Count && GUICursor.Position.Y > _giSelection.Position().Y + _giSelection.Height)
                        {
                            _giSelection.AlignToObject(_diOptions[_iKeySelection + 1].GText, SideEnum.Bottom);
                            _iKeySelection++;
                        }
                    }
                }

                if (InputManager.CheckPressedKey(Keys.Enter))
                {
                    SelectAction();
                }
            }
        }

        /// <summary>
        /// Triggered when the user selects an option in the GUITextSelectionWindow
        /// 
        /// This method analyzes the chosen TextEntry object and determines what actions to take
        /// as a result of the chosenAction and the current TextEntry
        /// </summary>
        protected void SelectAction()
        {
            TextEntry chosenAction = _diOptions[_iKeySelection].SelectionEntry;
            TextEntry nextText = null;

            if(GameManager.CurrentNPC != null)
            {
                TalkingActor act = GameManager.CurrentNPC;
                if (chosenAction.TextVerb.Equals(TextEntryVerbEnum.Yes))
                {
                    if (_textEntry.GameTrigger.Equals(TextEntryTriggerEnum.ConfirmGift))
                    {
                        nextText = act.Gift(GameManager.CurrentItem);
                        GUIManager.CloseMainObject();
                    }
                    else if (_textEntry.GameTrigger.Equals(TextEntryTriggerEnum.Donate))
                    {
                        ((Villager)GameManager.CurrentNPC).FriendshipPoints += 40;
                    }
                    else if (_textEntry.GameTrigger.Equals(TextEntryTriggerEnum.PetFollow))
                    {
                        PlayerManager.PlayerActor.SetPet((Pet)GameManager.CurrentNPC);
                    }
                    else if (_textEntry.GameTrigger.Equals(TextEntryTriggerEnum.PetUnfollow))
                    {
                        PlayerManager.PlayerActor.SetPet(null);
                    }
                    else if (_textEntry.GameTrigger.Equals(TextEntryTriggerEnum.GetBaby))
                    {
                        PlayerManager.DetermineBabyAcquisition();
                    }
                }
                else if (chosenAction.TextVerb.Equals(TextEntryVerbEnum.No))
                {
                    if (_textEntry.GameTrigger.Equals(TextEntryTriggerEnum.Donate))
                    {
                        ((Villager)GameManager.CurrentNPC).FriendshipPoints += 1000;
                    }

                    //Just pop this here for now
                    if (act.IsActorType(WorldActorTypeEnum.ShippingGremlin)) { act.PlayAnimation(AnimationEnum.Action2); }
                }
                else if (chosenAction.TextVerb.Equals(TextEntryVerbEnum.Gift)) { GUIManager.OpenMainObject(new HUDInventoryDisplay(DisplayTypeEnum.Gift)); }
                else if (chosenAction.TextVerb.Equals(TextEntryVerbEnum.Party)) { nextText = act.JoinParty(); }
                else if (chosenAction.TextVerb.Equals(TextEntryVerbEnum.ShowRequests)) { nextText = act.OpenRequests(); }
                else if (chosenAction.TextVerb.Equals(TextEntryVerbEnum.Buy)) { act.OpenShop(); }
                else if (chosenAction.TextVerb.Equals(TextEntryVerbEnum.Talk)) { nextText = act.GetDailyDialogue(); }
                else if (chosenAction.TextVerb.Equals(TextEntryVerbEnum.ShipGoods)) { ((ShippingGremlin)GameManager.CurrentNPC).OpenShipping(); }
                else if (chosenAction.TextVerb.Equals(TextEntryVerbEnum.Propose))
                {
                    if (act.GetFriendshipLevel() >= 8)
                    {
                        Villager v = ((Villager)act);
                        v.Relationship = RelationShipStatusEnum.Engaged;

                        nextText = act.GetDialogEntry("MarriageYes");
                    }
                    else { nextText = act.GetDialogEntry("MarriageNo"); }
                }
                else if (chosenAction.TextVerb.Equals(TextEntryVerbEnum.Date))
                {
                    ((Villager)act).Relationship = RelationShipStatusEnum.Dating;
                    nextText = act.GetDialogEntry("DateYes");
                }
            }
            else if (GameManager.CurrentItem != null)
            {
                //Valid verbs to continue are "yes" or "option_#"
                if (!chosenAction.TextVerb.Equals(TextEntryVerbEnum.No))
                {
                    if (_textEntry.GameTrigger.Equals(TextEntryTriggerEnum.UseItem))
                    {
                        GameManager.CurrentItem.UseItem(chosenAction.TextVerb);
                    }
                }
            }
            else
            {
                if (chosenAction.TextVerb.Equals(TextEntryVerbEnum.Yes))
                {
                    if (_textEntry.GameTrigger.Equals(TextEntryTriggerEnum.Exit)) { }
                    else if (_textEntry.GameTrigger.Equals(TextEntryTriggerEnum.EndDay))
                    {
                        Vector2 pos = PlayerManager.PlayerActor.CollisionBox.Center.ToVector2();
                        PlayerManager.SetPath(TravelManager.FindPathToLocation(ref pos, MapManager.CurrentMap.DictionaryCharacterLayer["PlayerSpawn"]));
                        GUIManager.SetScreen(new DayEndScreen());
                    }
                }
            }

            if (nextText == null) { GUIManager.CloseTextWindow(); }
            else { GUIManager.SetWindowText(nextText, GameManager.CurrentNPC, true); }
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (Contains(mouse))
            {
                SelectAction();
                rv = true;
            }
            return rv;
        }

        internal void Clear()
        {
            _iKeySelection = 0;
            
            foreach (SelectionData g in _diOptions.Values)
            {
                if (Controls.Contains(g.GText))
                {
                    RemoveControl(g.GText);
                }
            }

            _diOptions.Clear();
        }

        protected void AssignToColumn()
        {
            for (int i = 0; i < _diOptions.Count; i++)
            {
                GUIText gText = _diOptions[i].GText;
                if (i == 0) { gText.AnchorAndAlignToObject(_giSelection, SideEnum.Right, SideEnum.Bottom); }
                else { gText.AnchorAndAlignToObject(_diOptions[i - 1].GText, SideEnum.Bottom, SideEnum.Left); }
                AddControl(gText);
            }
        }

        protected class SelectionData
        {
            GUIText _gText;
            public GUIText GText => _gText;

            public string Text => _gText.Text;

            public TextEntry SelectionEntry { get; private set; }

            public SelectionData(TextEntry textEntry, string fontName = DataManager.FONT_MAIN)
            {
                SelectionEntry = textEntry;
                _gText = new GUIText(textEntry.GetFormattedText(), true, fontName);
            }
        }

        public override bool IsSelectionBox() { return true; }
    }
}
