using DevTools;
using HarmonyLib;
using MidiPlayerTK;
using NULL.Content;
using NULL.Manager;
using System.Collections.Generic;

namespace NULL.ModPatches;

[ConditionalPatchNULL]
[HarmonyPatch(typeof(MusicManager))]
internal class MusicManagerPatcher
{
   // internal static Dictionary<string, bool> MidisWasChanged = new() { { "BossIntro", false }, { "BossLoop", false } };
    [HarmonyPatch("MidiEvent")]
    static void Postfix(MusicManager __instance, List<MPTKEvent> midiEvents)
    {
        var b = BossManager.Instance;
        if (!b || b && b.health <= 0) return;

        for (int i = 0; i < midiEvents.Count; i++)
        {
            if (midiEvents[i].Command == MPTKCommand.MetaEvent && midiEvents[i].Meta == MPTKMeta.TextEvent)
            {
                if (!b.bossTransitionWaiting)
                {
                    if (midiEvents[i].Info == "Loop")
                    {
                        __instance.MidiPlayer.MPTK_TickCurrent = 0L;
                    }
                }
                else
                {
                    __instance.MidiPlayer.MPTK_TickCurrent = 4992L;
                    b.bossTransitionWaiting = false;
                    __instance.MidiPlayer.MPTK_Loop = false;
                }
            }
        }
    }
    /*   [HarmonyPatch("MidiEvent")]
       static void Prefix(MidiFilePlayer ___midiPlayer, List<MPTKEvent> midiEvents)
       {
           if (___midiPlayer.MPTK_MidiName == "custom_BossIntro" && !midiEvents.Contains(x => x.Info == "Loop"))
               MidisWasChanged["BossIntro"] = true;

           if (___midiPlayer.MPTK_MidiName == "custom_BossLoop" && !midiEvents.Contains(x => x.Command == MPTKCommand.MetaEvent && x.Meta == MPTKMeta.TextEvent))
               MidisWasChanged["BossLoop"] = true;
       }*/

    [HarmonyPatch(nameof(MusicManager.PlayMidi))]
    [HarmonyPrefix]
    static bool StopElevatorSong(string song)
    {
        if (song.Equals("Elevator") && ExtraVariables.PlayerInElevator && ModManager.NullStyle) return false;
        return true;
    }
}