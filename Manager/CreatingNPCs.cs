using DevTools.DevAPI.Extensions;
using MTM101BaldAPI.ObjectCreation;
using NULL.NPCs;
using UnityEngine;

namespace NULL.Manager;

internal static partial class ModManager
{
    static void CreateNPCs()
    {
        // Creating NULL
        NPC npc;
        npc = new NPCBuilder<NullNPC>(plug.Info).AddLooker().AddSpawnableRoomCategories(RoomCategory.Hall).
            AddTrigger().SetMinMaxAudioDistance(250, 450).SetEnum(Character.Baldi).SetName("NULL").SetSprite("ull").BuildNPC(); // Yeah, even the new audio propagation system is powerless in front of his powerful speeches! 😎
      //  npc.spriteRenderer[0].transform.localScale += Vector3.one * .5f;
        m.Add("NULL", npc);

        // Creating Baldloon
        npc = new NPCBuilder<NullNPC>(plug.Info).AddLooker().AddSpawnableRoomCategories(RoomCategory.Hall).
            AddTrigger().SetMinMaxAudioDistance(250, 450).SetEnum(Character.Baldi).SetName("NULLGLITCH").SetSprite("BaldloonRed").BuildNPC();
        npc.GetComponent<NullNPC>().isGlitch = true;
       // npc.spriteRenderer[0].transform.localScale *= 2;
        m.Add("NULLGLITCH", npc);
    }
}
