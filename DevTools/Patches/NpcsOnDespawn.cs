using HarmonyLib;
using System;
using static UnityEngine.Object;

namespace DevTools.Patches;

[HarmonyPatch]
internal class NPCOnDespawn // THANK U, PixelGuy!
{
    [HarmonyPatch(typeof(NPC), "Despawn")]
    [HarmonyPrefix]
    private static void Despawn(NPC __instance)
    {
        var type = __instance.GetType();
        if (type == typeof(Beans)) // beans
        {
            var gum = ((Beans)__instance).gum;
            gum.Reset(); // Removes any move mod
            Destroy(gum.gameObject);
            return;
        }
        if (type == typeof(Cumulo))
        {
            ((Cumulo)__instance).audMan.FlushQueue(true);
            Destroy(((Cumulo)__instance).windManager.gameObject);
            return;
        }
        if (type == typeof(LookAtGuy))
        {
            __instance.ec.RemoveFog(((LookAtGuy)__instance).fog);
            ((LookAtGuy)__instance).FreezeNPCs(false);
            return;
        }
        if (type == typeof(NoLateTeacher))
        {
            ((NoLateTeacher)__instance).mapIcon?.gameObject.SetActive(false);
            ((NoLateTeacher)__instance).targetedPlayer?.Am.moveMods.Remove(((NoLateTeacher)__instance).moveMod);
            return;
        }
        if (type == typeof(Playtime))
        {
            var rope = ((Playtime)__instance).currentJumprope;
            if (rope)
                rope.Destroy();
            return;
        }

    }

    [HarmonyPatch(typeof(Chalkboard), "Update")]
    [HarmonyPrefix]
    private static bool DestroyIfRequired(Chalkboard __instance, ref RoomController ___room, ChalkFace ___chalkFace)
    {
        if (___chalkFace == null) // If it doesn't exist
        {
            ___room.functions.functions.Remove(__instance);
            Destroy(__instance.gameObject);
            return false;
        }
        return true;
    }
    [HarmonyPatch(typeof(FirstPrize_Active), "Update")]
    [HarmonyPrefix]
    private static bool WhenGetDestroyed(FirstPrize_Active __instance, FirstPrize ___firstPrize, ref PlayerManager ___currentPlayer, MoveModsManager ___moveModsMan)
    {
        if (___firstPrize == null) // if first prize just gone.
        {
            ___moveModsMan.RemoveAll();
            if (___currentPlayer != null)
            {
                PlayerManager playerManager = ___currentPlayer;
                playerManager.onPlayerTeleport = (PlayerManager.PlayerTeleportedFunction)Delegate.Remove(playerManager.onPlayerTeleport, new PlayerManager.PlayerTeleportedFunction(__instance.PlayerTeleported));
                ___currentPlayer = null;
            }
            return false;
        }

        return true;
    }
}
