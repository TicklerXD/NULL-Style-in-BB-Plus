using DevTools;
using DevTools.Extensions;
using HarmonyLib;
using NULL.Content;
using NULL.CustomComponents;
using NULL.Manager;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static DevTools.ExtraVariables;
using static NULL.Manager.CompatibilityModule.Plugins;
using static NULL.Manager.ModManager;

namespace NULL.NPCs;

public class NullNPC : Baldi
{
    public bool slideMode;
    internal static new AnimationCurve slapCurve;
    internal static new AnimationCurve speedCurve;
    public static int attempts;
    public static float timeSinceExcitingThing;
    public Cell currentCell;
    public Cell previousCell;
    float flickerDelay, _distance;
    List<Cell> lightsToChange = [];
    SoundObject hitSound;
    public SpeechCheck Speaker { get; private set; }
    public bool Hidden { get => !spriteBase; set => spriteBase.SetActive(!value); }
    public bool isGlitch;
    public new AudioManager AudMan => GetComponent<AudioManager>();
    void SetupPrefab()
    {
        baseAnger = 0.1f;
        baseSpeed = 5f;

        audMan = GetComponent<AudioManager>();
        audMan.subtitleColor = Color.white;
        audMan.overrideSubtitleColor = false;
        audMan.ignoreListenerPause = true;

        navigator.Initialize(ec);
        spriteRenderer[0].transform.localScale += new Vector3(0.5f, 0.5f, 0.5f);
        if (isGlitch) spriteRenderer[0].transform.localScale *= 2;
        Speaker = new(this);
        gameObject.AddComponent<Flash>();
        hitSound = !isGlitch ? m.Get<SoundObject>("NullHit") : PixelInternalAPI.Extensions.GenericExtensions.FindResourceObjectByName<SoundObject>("Lose_Buzz");
        NullPlusManager.instance.nullNpc = this;
    }
    public override void Initialize()
    {
        SetupPrefab();
        behaviorStateMachine.ChangeState(new NullNPC_Chase(this, this));
        behaviorStateMachine.ChangeNavigationState(new NavigationState_WanderRandom(this, 0));

        foreach (var cell in ec.lights)
            if (cell.lightStrength > 0.5)
                lightsToChange.Add(cell);

#if CHEAT
        Debug.Log("NULL" + (IsGlitch ? "GLITCH" : string.Empty) + " is initialized!");
#endif
    }
    public void Hit(int val, bool pause = true)
    {
        if (val < 0) return;
        GetComponent<Flash>()?.SetFlash(1.5f);
        if (pause) this.Pause(1f);
        GetAngry(IsTimes ? 2.25f : 3.125f * val);
        audMan.FlushQueue(true);
        audMan.QueueAudio(hitSound);
        if (!BossManager.Instance.BossActive)
            audMan.QueueAudio(isGlitch ? "GlitchBossStart" : "Null_PreBoss_Start");
    }
    public override void Slap()
    {
        slapTotal = 0f;
        slapDistance = nextSlapDistance;
        nextSlapDistance = 0f;
        var speed = !slideMode ? slapDistance / (Delay * MovementPortion) : (anger + baseAnger + extraAnger) * 1.25f;
        navigator.SetSpeed(speed);
    }
    public void FixedUpdateSlapDistance() => nextSlapDistance += Speed * Time.deltaTime * TimeScale;
    public override void VirtualUpdate()
    {
        base.VirtualUpdate();
        FlickerLights(!Hidden && !OptionsManager.DarkAmbience);
    }
    public new float Delay => slapCurve.Evaluate(anger + extraAnger) + 0.4f;
    public new float Speed => (speedCurve.Evaluate(anger) + baseSpeed + extraAnger) * 1.25f;

    public void FlickerLights(bool enable)
    {
        if (!enable) return;

        flickerDelay -= Time.deltaTime * ec.EnvironmentTimeScale;
        foreach (var cell in lightsToChange)
        {
            _distance = Vector3.Distance(transform.position, cell.TileTransform.position);
            float num = (_distance - 30f) / 70f;
            if (behaviorStateMachine.currentState.ToString().Contains("Baldi_Attack")) break;

            if (!Singleton<PlayerFileManager>.Instance.reduceFlashing)
            {
                if (_distance <= 30f)
                {
                    if (cell.lightOn)
                        cell.SetLight(false);
                }
                else if (_distance <= 100f)
                {
                    if (flickerDelay <= 0f && Random.Range(0f, 1f) <= 0.1f)
                    {
                        if (!cell.lightOn)
                            if (Random.Range(0f, 1f) <= num)
                                cell.SetLight(true);

                            else if (Random.Range(0f, 1f) >= num)
                                cell.SetLight(false);
                    }
                }
                else if (!cell.lightOn) cell.SetLight(true);
            }
            else if (_distance <= 70f) cell.SetLight(false);
            else cell.SetLight(true);
        }
    }


    //       ******************** SPEECH CHECKER *********************
    public class SpeechCheck
    {
        NullNPC nullNpc;
        public Dictionary<string, bool> nullPhrases = [];
        float speechCheckTime = 10f;
        float doorCommentCool;
        float corneredCommentCool;
        public float gameTime;
        public float hadTargetTime;
        public SpeechCheck(NullNPC nullNpc)
        {
            this.nullNpc = nullNpc;
            string[] allPhrases = ["Bored", "Enough", "Haha", "Hide", "Nothing", "Scary", "Stop", "Wherever"];
            allPhrases.Do(x => nullPhrases.Add(x, false));
        }
        public void SpeechChecker(string phrase, float chance)
        {
            if (BossManager.Instance.BossActive || BossManager.Instance.bossTransitionWaiting) return;

            List<string> genericPhrases = ["Bored", "Scary","Stop", "Wherever"];
            var audMan = nullNpc.GetComponent<AudioManager>();

            void PlayPhrase(string name, bool generic = false)
            {
                if (nullNpc.isGlitch) return;

                if (Random.Range(0f, 1f) <= chance && !audMan.AnyAudioIsPlaying && (!nullPhrases[name] || (phrase.Equals("Generic") && generic)))
                {
                    if (!generic)
                    {
                        audMan.QueueAudio("Null_NPC_" + name);
                        nullPhrases[name] = true;
                        return;
                    }
                    genericPhrases.Remove(name);
                }
            }

            if (!phrase.Equals("Generic"))
            {
                PlayPhrase(phrase);
                return;
            }
            var list = new List<string>(genericPhrases);
            while (list.Count > 0)
            {
                var aud = list[Random.Range(0, list.Count)];
                if (aud != "Bored")
                {
                    switch (aud)
                    {
                        case "Scary":
                            if (gameTime >= 240f && !nullNpc.looker.PlayerInSight())
                            {
                                PlayPhrase(aud, true);
                                return;
                            }
                            break;
                        case "Stop":
                            if (attempts >= 5 && gameTime >= 60f)
                            {
                                PlayPhrase(aud, true);
                                return;
                            }
                            break;
                        case "Where":
                            if (hadTargetTime >= 30f && gameTime >= 60f)
                            {
                                PlayPhrase(aud, true);
                                return;
                            }
                            break;
                    }
                }
                else if (gameTime >= 300f && timeSinceExcitingThing >= 60f)
                {
                    PlayPhrase(aud, true);
                    return;
                }
                list.Remove(aud);
            }
        }

        public void Update()
        {
            if (nullPhrases.Count == 0 || nullNpc.currentCell is null || nullNpc.previousCell is null) return;

            speechCheckTime -= Time.deltaTime;
            if (nullNpc.currentCell.doorHere)
            {
                foreach (Door door in nullNpc.currentCell.doors)
                {
                    if (door.GetComponent<LockdownDoor>() == null)
                        door.OpenTimed(0.5f, false);
                }
                if (doorCommentCool <= 0f) SpeechChecker("Hide", 0.01f);
                doorCommentCool = 1f;
            }
            if (speechCheckTime <= 0f)
            {
                speechCheckTime = 10f;
                SpeechChecker("Generic", 0.25f);
                if (!Singleton<CoreGameManager>.Instance.GetPlayer(0).itm.HasItem() && Singleton<CoreGameManager>.Instance.GetPlayer(0).plm.stamina == 0f &&
                    Vector3.Distance(nullNpc.transform.position, Singleton<CoreGameManager>.Instance.GetPlayer(0).transform.position) <= 25f && nullNpc.anger >= 4f)
                    SpeechChecker("Nothing", 0.2f);
            }
            if (corneredCommentCool <= 0f && nullNpc.currentCell != null && nullNpc.previousCell != null && Singleton<BaseGameManager>.Instance.FoundNotebooks > 2)
            {
                int navBin = nullNpc.currentCell.NavBin;
                for (int i = 0; i < 4; i++)
                {
                    if ((navBin & 1 << i) == 0) 
                        nullNpc.currentCell.SilentBlock((Direction)i, true);
                }

                if (!ExtraVariables.ec.CheckPath(nullNpc.previousCell, ExtraVariables.ec.CellFromPosition(IntVector2.GetGridPosition(Singleton<CoreGameManager>.Instance.GetPlayer(0).transform.position)), PathType.Nav))
                    SpeechChecker("Enough", 0.04f);

                for (int j = 0; j < 4; j++)
                {
                    if ((navBin & 1 << j) == 0)
                        nullNpc.currentCell.SilentBlock((Direction)j, false);
                }
                corneredCommentCool = 1f;
            }
            hadTargetTime += Time.deltaTime;
            gameTime += Time.deltaTime;
            doorCommentCool -= Time.deltaTime;
            corneredCommentCool -= Time.deltaTime;
        }
    }
}


//       ********************NULL STATES*********************
public class NullNPC_Chase(NPC npc, NullNPC nullNpc) : Baldi_Chase(npc, nullNpc)
{
    private float delayTimer;
    protected NullNPC nullNpc = nullNpc;
    public override void Enter()
    {
        delayTimer = nullNpc.Delay;
        nullNpc.ResetSlapDistance();
    }
    public override void OnStateTriggerStay(Collider other)
    {

#if CHEAT
        if (Singleton<PlayerFileManager>.Instance.fileName == "Test") return;
#endif

        if (other.CompareTag("Player") && !nullNpc.Hidden)
        {
            bool flag;
            nullNpc.looker.Raycast(other.transform, Vector3.Magnitude(nullNpc.transform.position - other.transform.position), out flag);
            if (flag)
            {
                PlayerManager component = other.GetComponent<PlayerManager>();
                ItemManager itm = component.itm;
                if (!component.invincible)
                {
                   /* if (itm.Has(Items.Apple))
                    {
                        itm.Remove(Items.Apple);
                        nullNpc.Pause(Random.Range(6, 10));
                        return;
                    }*/
                    nullNpc.Speaker.SpeechChecker("Haha", 0.04f);
                    nullNpc.CaughtPlayer(component);
                }
            }
        }
    }
    public override void OnStateTriggerEnter(Collider other)
    {
        base.OnStateTriggerEnter(other);
        if (nullNpc.Navigator.passableObstacles.Contains(PassableObstacle.Window) && other.CompareTag("Window"))
        {
            other.GetComponent<Window>().Break(false);
            nullNpc.Speaker.SpeechChecker("Hide", 0.04f);
        }
    }
    public override void Update()
    {
        nullNpc.FixedUpdateSlapDistance();
        delayTimer -= Time.deltaTime * npc.TimeScale;
        if (delayTimer <= 0f || nullNpc.slideMode)
        {
            nullNpc.Slap();
            nullNpc.SlapRumble();
            delayTimer = nullNpc.Delay;
        }

        if (ec.CellFromPosition(IntVector2.GetGridPosition(nullNpc.transform.position)) != nullNpc.currentCell)
        {
            nullNpc.previousCell = nullNpc.currentCell;
            nullNpc.currentCell = ec.CellFromPosition(IntVector2.GetGridPosition(nullNpc.transform.position));
        }

        nullNpc.Speaker.Update();
    }
    public override void DestinationEmpty()
    {
        if (nullNpc.Navigator.passableObstacles.Contains(PassableObstacle.Window))
            nullNpc.Navigator.passableObstacles.Clear();

        base.DestinationEmpty();
    }
    public override void PlayerSighted(PlayerManager player)
    {
        base.PlayerSighted(player);
        if (!nullNpc.Navigator.passableObstacles.Contains(PassableObstacle.Window))
        {
            nullNpc.Navigator.passableObstacles.Add(PassableObstacle.Window);
            nullNpc.Navigator.CheckPath();
        }
    }
    public override void PlayerInSight(PlayerManager player) //Just in case, lol
    {
        base.PlayerInSight(player);
        if (!nullNpc.Navigator.passableObstacles.Contains(PassableObstacle.Window))
        {
            nullNpc.Navigator.passableObstacles.Add(PassableObstacle.Window);
            nullNpc.Navigator.CheckPath();
        }
    }
}

public class NullNPC_Preboss(NullNPC nullNpc, Elevator finalElevator) : NullNPC_Chase(nullNpc, nullNpc)
{
    protected Vector3 elevatorPos = finalElevator.transform.position - finalElevator.Door.direction.ToVector3() * 10f;
    protected EnvironmentController ec = nullNpc.ec;
    public override void Enter() // Null runs into the director's office
    {
        if (!IsTimes && ec.rooms.Any(x => x.category == RoomCategory.Office)) // ...but not in the BB Times!
        {
            base.Enter();
            nullNpc.slideMode = true;
            nullNpc.GetAngry(169f);
            nullNpc.Navigator.passableObstacles.Clear();
            nullNpc.behaviorStateMachine.ChangeNavigationState(new NavigationState_TargetPosition(nullNpc, 163, ec.RealRoomMid(ec.offices[0])));
        }
    }
    public override void Update()
    {
        base.Update();
        if (ec.NavigableDistance(Singleton<CoreGameManager>.Instance.GetPlayer(0).transform.position.ToCell(), elevatorPos.ToCell(), PathType.Nav) < 15)
            nullNpc.behaviorStateMachine.ChangeState(new NullNPC_Rushing(npc, nullNpc, elevatorPos));
    }
    public override void DestinationEmpty()
    {
        if (IsTimes) return;
        if (!nullNpc.Hidden)
        {
            nullNpc.Hidden = true;
        }
        nullNpc.behaviorStateMachine.ChangeNavigationState(new NavigationState_DoNothing(nullNpc, 0));
    }
    public override void PlayerInSight(PlayerManager player) { }
    public override void PlayerSighted(PlayerManager player) { }
    public override void OnStateTriggerEnter(Collider other) { }
    public override void OnStateTriggerStay(Collider other) { }
}

public class NullNPC_Rushing(NPC npc, NullNPC nullNPC, Vector3 finalElevatorPos) : NullNPC_Chase(npc, nullNPC)
{
    Vector3 finalElevatorPos = finalElevatorPos;
    public override void Enter()
    {
        base.Enter();
        nullNpc.Navigator.passableObstacles.Clear();
        nullNpc.slideMode = true;

        if (nullNpc.anger < 169f)
            nullNpc.GetAngry(169f);

        nullNpc.behaviorStateMachine.ChangeNavigationState(new NavigationState_TargetPosition(nullNpc, 0, finalElevatorPos + new Vector3(0f, 5f, 0f)));
        nullNpc.Hidden = false;
    }
    public override void OnStateTriggerStay(Collider other) { }
    public override void PlayerInSight(PlayerManager player) { }
    public override void DestinationEmpty() => BossManager.Instance.StartBossIntro();
}