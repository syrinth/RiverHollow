using RiverHollow.Actors;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Game_Managers.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.GUIObjects;

using static RiverHollow.WorldObjects.Door;
using RiverHollow.Misc;

namespace RiverHollow.Game_Managers.GUIComponents.Screens
{
    class TextScreen : GUIScreen
    {
        bool _bIsSelection;
        private GUITextWindow _window;

        private TextScreen()
        {
            GraphicCursor._CursorType = GraphicCursor.EnumCursorType.Normal;
            GameManager.Pause();
        }

        public TextScreen(string text, bool selection) : this()
        {
            if (selection)
            {
                _window = new GUITextSelectionWindow(text);
                _bIsSelection = true;
            }
            else
            {
                _window = new GUITextWindow(text);
                _bIsSelection = false;
            }
            AddControl(_window);
        }

        public TextScreen(KeyDoor door, string text) : this()
        {
            GameManager.gmDoor = door;
            _window = new GUITextSelectionWindow(text);
            AddControl(_window);
        }

        public TextScreen(TalkingActor talker, string text) : this()
        {
            if (text.Contains("["))
            {
                _bIsSelection = true;
                _window = new GUITextSelectionWindow(talker, text);
            }
            else
            {
                _bIsSelection = false;
                _window = new GUITextWindow(talker, text);
            }
            AddControl(_window);
        }

        public TextScreen(Spirit talker, string text) : this()
        {
            GameManager.gmSpirit = talker;
            _window = new GUITextWindow(text);

            AddControl(_window);
        }

        public override void Update(GameTime gameTime)
        {
            if (TextFinished())
            {
                if (DungeonManager.Maps.Count > 0)
                {
                    MapManager.EnterDungeon();
                }
                if (CutsceneManager.Playing) { GUIManager.ClearScreen(); }
                else { GameManager.BackToMain(); }
            }
            else
            {
                base.Update(gameTime);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _window.Draw(spriteBatch);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = true;

            if (_window.ProcessLeftButtonClick(mouse) && _bIsSelection)
            {
                string SelectAction = ((GUITextSelectionWindow)_window).SelectedAction;
                if (GameManager.gmNPC != null)
                {
                    string nextText = GameManager.gmNPC.GetDialogEntry(SelectAction);

                    if (SelectAction.StartsWith("Quest"))
                    {
                        Quest q = GameManager.DIQuests[int.Parse(SelectAction.Remove(0, "Quest".Length))];
                        PlayerManager.AddToQuestLog(q);
                        GUIManager.SetScreen(new TextScreen(GameManager.gmNPC, GameManager.gmNPC.GetDialogEntry("Quest" + q.QuestID)));
                    }
                    else if (SelectAction.StartsWith("Donate"))
                    {
                        ((Villager)GameManager.gmNPC).FriendshipPoints += 40;
                        GUIManager.SetScreen(new TextScreen(GameManager.gmNPC, nextText));
                    }
                    else if (SelectAction.StartsWith("NoDonate"))
                    {
                        ((Villager)GameManager.gmNPC).FriendshipPoints -= 1000;
                        GUIManager.SetScreen(new TextScreen(GameManager.gmNPC, nextText));
                    }
                    else if (!string.IsNullOrEmpty(nextText))
                    {
                        GUIManager.SetScreen(new TextScreen(GameManager.gmNPC, nextText));
                    }
                    else if (GUIManager.IsTextScreen())
                    {
                        GameManager.BackToMain();
                    }
                }
                else
                {
                    if (SelectAction.Equals("SleepNow"))
                    {
                        Vector2 pos = PlayerManager.World.Center;
                        PlayerManager.SetPath(TravelManager.FindPathToLocation(ref pos, MapManager.CurrentMap.DictionaryCharacterLayer["PlayerSpawn"], MapManager.CurrentMap.Name));
                        GameManager.BackToMain();
                    }
                    else if (SelectAction.Equals("OpenDoor"))
                    {
                        GUIManager.SetScreen(new InventoryScreen(GameManager.gmDoor));
                    }
                    else if (SelectAction.Contains("UseItem"))
                    {
                        GameManager.UseItem();
                    }
                    else if (SelectAction.Contains("SellContract") && GameManager.gmNPC != null)
                    {
                        if (GameManager.gmNPC.IsWorldAdventurer())
                        {
                            ((WorldAdventurer)GameManager.gmNPC).Building.RemoveWorker((WorldAdventurer)GameManager.gmNPC);
                            PlayerManager.AddMoney(1000);
                            GameManager.BackToMain();
                        }
                    }
                    else
                    {
                        GameManager.BackToMain();
                    }
                }
            }

            if (_window != null)
            {
                if (!_window.Paused)
                {
                    _window.PrintAll();
                }
                else
                {
                    _window.NextText();
                }
            }
            return rv;
        }

        public bool TextFinished()
        {
            return _window.Done() && !_window.Paused;
        }

        public override bool IsTextScreen() { return true; }
    }
}
