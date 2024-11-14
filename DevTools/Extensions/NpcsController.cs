using System.Collections;
using System.Linq;
using UnityEngine;
using static NULL.Manager.ModManager;

namespace DevTools;
public static class NpcsController
{
    static NpcState prevState; // The previous state of the NPC
    static NavigationState prevNavState; // The previous state of the NPC navigation
    /// <summary>
    /// Adds a sprite to an NPC.
    /// </summary>
    /// <typeparam name="T">Type of the npc.</typeparam>
    /// <param name="npc">Target npc.</param>
    /// <param name="sprite">Name of sprite from asset manager.</param>
    /// <returns></returns>
    public static NPC AddSprite<T>(this NPC npc, string sprite) where T : NPC
    {
        npc.spriteRenderer[0].sprite = m.Get<Sprite>(sprite);
        return npc;
    }
    /// <summary>
    /// Teleport npc on random position.
    /// </summary>
    /// <param name="npc">Target npc.</param>
    public static void SetRandomPosition(this NPC npc)
    {
        var list = ExtraVariables.ec.AllTilesNoGarbage(false, false);
        npc.transform.position = list[UnityEngine.Random.Range(0, list.Count)].TileTransform.position + new Vector3(0f, 5f, 0f);
    }
    /// <summary>
    /// Spawn npc.
    /// </summary>
    /// <param name="npc">Npc to spawn.</param>
    /// <param name="pos">Position of npc to spawn.</param>
    public static void Spawn(this NPC npc, Vector3 pos)
    {
        var fixedPos = pos + new Vector3(0f, 5f);
        ExtraVariables.ec.SpawnNPC(npc, new IntVector2((int)(fixedPos.x / 10f), (int)(fixedPos.z / 10f)));
    }
    /// <summary>
    /// Spawn npc.
    /// </summary>
    /// <param name="npc">Npc to spawn.</param>
    /// <param name="pos">Cell to spawn.</param>
    public static void Spawn(this NPC npc, Cell cell) => npc.Spawn(cell.TileTransform.position);
    /// <summary>
    /// Despawn of all NPCs except those specified
    /// </summary>
    /// <param name="toSave">NPCs that will not be despawned.</param>
    public static void DespawnAllNpcs(params Character[] toSave) 
    {
        foreach (NPC npc in Singleton<BaseGameManager>.Instance.Ec.Npcs.ToList()) 
            if (!toSave.Contains(npc.Character)) npc.Despawn();
    }
    /// <summary>
    /// Save previous state of npc.
    /// </summary>
    /// <param name="npc">Target npc.</param>
    public static void SavePreviousState(this NPC npc)
    {
        if (!npc.behaviorStateMachine.currentState.ToString().Contains("Pause"))
             prevState = npc.behaviorStateMachine.currentState;
    }
    /// <summary>
    /// Save previous navigation state of npc.
    /// </summary>
    /// <param name="npc">Target npc.</param>
    public static void SavePreviousNavState(this NPC npc)
    {
        if (!npc.behaviorStateMachine.CurrentNavigationState.ToString().Contains("Nothing"))
            prevNavState = npc.behaviorStateMachine.CurrentNavigationState;
    }
    /// <summary>
    /// Stops the NPC for the specified time.
    /// </summary>
    /// <param name="npc">Target npc.</param>
    /// <param name="time">The pause time of the NPC. If not specified, float is used by default.MaxValue.</param>
    public static void Pause(this NPC npc, float time = float.MaxValue)
    {
          if (npc.behaviorStateMachine.currentState is NPC_Pause state)
           {
              try { checked { state.time += time; } }
              catch { state.time = float.MaxValue; }
              return;
          }
          npc.SavePreviousState();
          npc.SavePreviousNavState();
          npc.behaviorStateMachine.ChangeState(new NPC_Pause(npc, time));

    }
    /// <summary>
    /// Unpause npc.
    /// </summary>
    /// <param name="npc">Target npc.</param>
    public static void Unpause(this NPC npc) => npc.behaviorStateMachine.ChangeState(new NPC_Pause(npc, 0f));

    /// <summary>
    /// Returns true if the NPC is paused.
    /// </summary>
    /// <param name="npc">Target npc.</param>
    /// <returns></returns>
    public static bool IsPaused(this NPC npc) => npc.behaviorStateMachine.CurrentState.ToString().Contains("NPC_Pause");

    /// <summary>
    /// Infinite acceleration of Baldi.
    /// </summary>
    /// <param name="b">Target Baldi.</param>
    /// <param name="increaser">Speed of the acceleration.</param>
    /// <returns></returns>
    public static IEnumerator Rage(this Baldi b, float increaser = .1f)
    {
        if (increaser <= 0f)
            yield break;

        while (true)
        {
            b.GetAngry(increaser * b.TimeScale * Time.deltaTime);
            yield return null;
        }
    }

    public class NPC_Pause(NPC npc, float time = float.MaxValue) : NpcState(npc) // Pause state for any npc
    {
        public float time = time;
        public override void Enter()
        {
            base.Enter();
            npc.behaviorStateMachine.ChangeNavigationState(new NavigationState_DoNothing(npc, 0));
#if CHEAT
            Debug.Log("Npc is paused");
#endif
        }
        public override void OnStateTriggerStay(Collider other) { }
        public override void OnStateTriggerEnter(Collider other) { }
        public override void OnStateTriggerExit(Collider other) { }
        public override void Update()
        {
            base.Update();
            time -= Time.deltaTime;
            if (time <= 0f)
            {
                npc.behaviorStateMachine.ChangeState(prevState);
                npc.behaviorStateMachine.ChangeNavigationState(prevNavState);
            }
        }
    }
}

