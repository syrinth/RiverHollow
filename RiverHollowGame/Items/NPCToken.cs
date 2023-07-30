using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Items
{
    public class NPCToken : Item
    {
        private NPCTokenTypeEnum TokenType => GetEnumByIDKey<NPCTokenTypeEnum>("Subtype");
        private int NPCID => GetIntByIDKey("NPC_ID");

        public NPCToken(int id, Dictionary<string, string> stringData) : base(id, stringData, 1)
        {
            _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "Tokens");
        }

        public override bool ItemBeingUsed()
        {
            if (TokenType == NPCTokenTypeEnum.Mount)
            {
                Mount act = DataManager.CreateMount(NPCID);
                PlayerManager.AddMount(act);
                act.SpawnInHome();
            }
            else if (TokenType == NPCTokenTypeEnum.Pet)
            {
                Pet act = DataManager.CreatePet(NPCID);
                PlayerManager.AddPet(act);
                act.SpawnNearPlayer();
                if (PlayerManager.PlayerActor.ActivePet == null)
                {
                    PlayerManager.PlayerActor.SetPet(act);
                }
            }

            Remove(1);

            return true;
        }
    }
}
