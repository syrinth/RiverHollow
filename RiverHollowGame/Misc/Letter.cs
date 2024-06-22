using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using System;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Misc
{
    public class Letter
    {
        public int ID { get; set; }
        public int NPCID { get; set; }
        public TextEntry Text { get; set; }

        public bool LetterRead { get; private set; }
        public bool Repeatable => Text.HasTag("Repeatable");
        public LetterItemStateEnum ItemState { get; private set; }
        public bool ItemWaiting => ItemState == LetterItemStateEnum.Waiting;

        public Tuple<int, int> ItemData { get; set; } = new Tuple<int, int>(-1, -1);

        public Letter(int id)
        {
            ID = id;
            Text = new TextEntry(id.ToString(), Util.DictionaryFromTaggedString(DataManager.LetterData[ID]));

            var strID = Text.GetTagValue("NPCID");
            if (int.TryParse(strID, out int npcID))
            {
                NPCID = npcID;
            }
            else
            {
                NPCID = -1;
            }

            if (Text.HasTag("ItemID") || Text.HasTag("RandomItemID"))
            {
                ItemState = LetterItemStateEnum.Waiting;
            }
            else
            {
                ItemState = LetterItemStateEnum.None;
            }

            LetterRead = false;

            SetItemData();
        }

        public Letter(Letter l)
        {
            ID = l.ID;
            Text = l.Text;
            NPCID = l.NPCID;
            ItemState = l.ItemState;
            LetterRead = false;

            SetItemData();
        }

        private void SetItemData()
        {
            if (Text.HasTag("RandomItemID"))
            {
                var randomIDs = Text.GetTagValue("RandomItemID");
                var randomSplit = Util.FindParams(randomIDs);

                if (randomSplit.Length > 0)
                {
                    var itemStrData = Util.GetRandomItem(randomSplit);
                    var itemData = Util.FindIntArguments(itemStrData);
                    ItemData = new Tuple<int, int>(itemData[0], itemData.Length > 1 ? itemData[1] : 1);
                }
            }
            else if (Text.HasTag("ItemID"))
            {
                var itemStrData = Text.GetTagValue("ItemID");
                var itemData = Util.FindIntArguments(itemStrData);
                ItemData = new Tuple<int, int>(itemData[0], itemData.Length > 1 ? itemData[1] : 1);
            }
        }

        public void ReadLetter()
        {
            LetterRead = true;
        }

        public void ChangeItemState(LetterItemStateEnum state)
        {
            ItemState = state;
        }

        public LetterData Save()
        {
            return new LetterData()
            {
                LetterID = ID,
                ItemState = (int)ItemState,
                Read = LetterRead,
                ItemID = ItemData.Item1,
                ItemNum = ItemData.Item2
            };
        }

        public void LoadData(LetterData data)
        {
            ID = data.LetterID;
            ItemState = (LetterItemStateEnum)data.ItemState;
            LetterRead = data.Read;
            ItemData = new Tuple<int, int>(data.ItemID, data.ItemNum);
        }
    }
}
