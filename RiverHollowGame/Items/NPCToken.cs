using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Items
{
    public class NPCToken : Item
    {
        private NPCTokenTypeEnum TokenType => GetEnumByIDKey<NPCTokenTypeEnum>("Subtype");
        private int NPCID => GetIntByIDKey("NPC_ID");

        public NPCToken(int id) : base(id, 1)
        {
            _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "Tokens");
        }

        public override bool ItemBeingUsed()
        {
            if (TokenType == NPCTokenTypeEnum.Mount)
            {
                Mount act = DataManager.CreateActor<Mount>(NPCID);
                PlayerManager.AddMount(act);
                act.SpawnInHome();
                Remove(1);
            }
            else if (TokenType == NPCTokenTypeEnum.Pet)
            {
                if (TownManager.PetCafe != null || PlayerManager.Pets.Count == 0)
                {
                    Pet p = DataManager.CreateActor<Pet>(NPCID);
                    PlayerManager.AddPet(p);

                    if (PlayerManager.PlayerActor.ActivePet == null)
                    {
                        PlayerManager.PlayerActor.SetPet(p);
                        p.SpawnNearPlayer();
                    }
                    else if (MapManager.CurrentMap == TownManager.PetCafe.InnerMap)
                    {
                        p.SetPosition(MapManager.CurrentMap.GetRandomPosition(MapManager.CurrentMap.GetCharacterObject("Destination")));
                        MapManager.CurrentMap.AddActor(p);
                    }

                    Remove(1);
                }
                else
                {
                    GUIManager.NewWarningAlertIcon(Constants.STR_ALERT_PET_CAFE);
                }
            }

            return true;
        }
    }
}
