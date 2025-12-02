using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class QuestData
{
    public string questID;
    [TextArea] public string questTitle;

    [Header("대사 설정")]
    public string[] dialogueLines;
    public AudioClip[] dialogueSounds;

    [Tooltip("0 = 끝까지 재생, 숫자 = 해당 시간(초)만큼 재생")]
    public float[] soundDurations;

    [Tooltip("체크하면 스페이스바를 눌러도 소리가 안 끊기고 계속 나옵니다.")]
    public bool[] keepSoundOnSkip;

    [Header("배경음악 (비워두면 이전 음악 유지)")]
    public AudioClip questBgm;

    public string nextQuestID;

    [HideInInspector] public System.Func<bool> completionCondition;
    [HideInInspector] public bool dialogueShown = false;
}

public enum QuestState { NotStarted, InProgress, Completed }

public class QuestManager : MonoBehaviour
{
    public static QuestManager instance;
    private Dictionary<string, QuestState> questStates = new Dictionary<string, QuestState>();
    public List<QuestData> questList = new List<QuestData>();

    [Header("오브젝트 연결")]
    public Transform doorApart;
    public Transform doorServer;
    public LaptopInteraction serverLaptop;
    public PlayerShooting playerShooting;

    [Header("UI 연결")]
    public CanvasGroup fadeScreen;

    [Header("사운드 설정")]
    public AudioSource audioSource;
    public AudioSource managerRingtoneSource;

    [Header("헬기 구조 스크립트 연결")]
    public FlareHelicopterRescueV2 heliRescueScript;

    // 내부 상태 변수들
    private bool boatEscapeTriggered = false;
    private bool isTunnelBlown = false;
    private bool isFlaregunDialoguePlayed = false;
    private bool isFlareFired = false;
    private bool isHeliBoarded = false;

    // 프로퍼티
    public bool USBAcquired => HasItemFlexible("usb");
    public bool FlaregunAcquired { get { if (playerShooting != null) return playerShooting.IsGunUnlocked("FlareGun"); return false; } }
    public bool CarItemsAcquired => HasItemFlexible("carkey");
    public bool BoatItemsAcquired => HasItemFlexible("boatkey");
    public bool HasFuel => HasItemFlexible("jerrycan");
    public bool IsVIPSaved => GetQuestState("q08_save_end") == QuestState.Completed;
    [HideInInspector] public bool VIPSpoken = false;

    private void Awake()
    {
        instance = this;
        if (playerShooting == null) playerShooting = FindObjectOfType<PlayerShooting>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (fadeScreen != null) { fadeScreen.alpha = 0f; fadeScreen.blocksRaycasts = false; }
        SetupTestConditions();
    }

    // [변경됨] 외부에서 호출할 수 있도록 public으로 변경
    public void PlayDialogueOnly(string questID)
    {
        QuestData quest = questList.Find(q => q.questID == questID);
        if (quest != null && !quest.dialogueShown)
        {
            if (DialogueUI.instance != null)
            {
                DialogueUI.instance.ShowDialogue(
                    quest.dialogueLines,
                    quest.dialogueSounds,
                    quest.soundDurations,
                    quest.keepSoundOnSkip,
                    () => {
                        quest.dialogueShown = true;
                        if (questID == "q01" && managerRingtoneSource != null)
                        {
                            if (!managerRingtoneSource.isPlaying) managerRingtoneSource.Play();
                        }
                    }
                );
            }
            else
            {
                quest.dialogueShown = true;
            }
        }
    }

    // 아이템 체크 함수들
    private bool HasItemFlexible(string targetName)
    {
        if (InventoryManager.instance == null) return false;
        foreach (var item in InventoryManager.instance.items)
        {
            if (item == null) continue;
            string myItemClean = item.itemName.Replace(" ", "").ToLower();
            string targetClean = targetName.Replace(" ", "").ToLower();
            if (myItemClean == targetClean) return true;
        }
        return false;
    }

    public void OnItemAdded(string rawItemName)
    {
        string name = rawItemName.Replace(" ", "").ToLower();
        if (name.Contains("flaregun")) { isFlaregunDialoguePlayed = true; PlayDialogueOnly("q09_heli"); }
        if (name.Contains("carkey") && CarItemsAcquired) PlayDialogueOnly("q10_car");
        if ((name.Contains("boatkey") || name.Contains("jerrycan")) && BoatItemsAcquired && !boatEscapeTriggered) PlayDialogueOnly("q11_boat");
    }

    public void ProcessVIPInteraction() { VIPSpoken = true; if (GetQuestState("q08_save") == QuestState.NotStarted) StartQuest("q08_save"); }
    public void ProcessBoatEscape() { if (GetQuestState("q08_save_escape") == QuestState.InProgress) boatEscapeTriggered = true; }
    public void ProcessTunnelExplosion() { isTunnelBlown = true; if (GetQuestState("q10_car") == QuestState.InProgress) QuestComplete("q10_car"); }
    public void ProcessCarInteraction() { if (!isTunnelBlown) { Debug.Log("터널 막힘"); return; } if (HasFuel) StartQuest("q10_car_end"); else ShowFuelMissingDialogue("자동차"); }
    public void ProcessPlayerBoatInteraction() { if (boatEscapeTriggered) return; if (HasFuel) StartQuest("q11_boat_end"); else ShowFuelMissingDialogue("배"); }

    public void ProcessFlareGunFired()
    {
        if (GetQuestState("q09_heli") == QuestState.InProgress)
        {
            isFlareFired = true;
            if (heliRescueScript != null)
            {
                GameObject p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) heliRescueScript.CallHelicopter(p.transform.position);
            }
        }
    }

    public void ProcessHeliBoarding()
    {
        if (GetQuestState("q09_heli_end") == QuestState.InProgress)
        {
            isHeliBoarded = true;
        }
    }

    private void ShowFuelMissingDialogue(string vehicleName) { if (DialogueUI.instance != null) DialogueUI.instance.ShowDialogue(new string[] { $"시동이 걸리지 않는다.", $"주인공: {vehicleName}에 연료가 없어! 어딘가에 기름통(JerryCan)이 있을 거야." }, null); }

    // 셋업
    private void SetupTestConditions()
    {
        if (questList == null || questList.Count == 0) return;
        if (questList.Count > 0) questList[0].completionCondition = () => { return questList[0].dialogueShown; };
        if (questList.Count > 1) questList[1].completionCondition = () => { return questList[1].dialogueShown && CheckDistance("ZombieScientist1", 10f); };
        if (questList.Count > 2) questList[2].completionCondition = () => { return questList[2].dialogueShown && HasItemFlexible("phone"); };
        if (questList.Count > 3) questList[3].completionCondition = () => { return questList[3].dialogueShown; };
        if (questList.Count > 4) questList[4].completionCondition = () => { return questList[4].dialogueShown && CheckDistance(doorApart, 3f); };
        if (questList.Count > 5) questList[5].completionCondition = () => { return questList[5].dialogueShown && HasItemFlexible("ServerRoomCard"); };
        if (questList.Count > 6) questList[6].completionCondition = () => { return questList[6].dialogueShown && CheckDistance(doorServer, 3f); };
        if (serverLaptop != null) serverLaptop.onDownloadComplete.AddListener(() => StartQuest("q07"));
        if (questList.Count > 7) questList[7].completionCondition = () => { return questList[7].dialogueShown; };
        if (questList.Count > 8) questList[8].completionCondition = () => { if (!questList[8].dialogueShown) return false; return FlaregunAcquired || CarItemsAcquired || BoatItemsAcquired; };
        if (questList.Count > 9) questList[9].completionCondition = () => { return questList[9].dialogueShown; };
        if (questList.Count > 10) questList[10].completionCondition = () => { return questList[10].dialogueShown && boatEscapeTriggered; };
        if (questList.Count > 11) questList[11].completionCondition = () => { return questList[11].dialogueShown; };
        if (questList.Count > 12) questList[12].completionCondition = () => { return isFlareFired; };
        if (questList.Count > 13) questList[13].completionCondition = () => { return questList[13].dialogueShown; };
        if (questList.Count > 14) questList[14].completionCondition = () => { return isHeliBoarded; };
        if (questList.Count > 15) questList[15].completionCondition = () => { return isTunnelBlown; };
        if (questList.Count > 16) questList[16].completionCondition = () => { return questList[16].dialogueShown; };
        if (questList.Count > 17) questList[17].completionCondition = () => { return questList[17].dialogueShown; };
        if (questList.Count > 18) questList[18].completionCondition = () => { return GetQuestState("q11_boat_end") == QuestState.InProgress; };
        if (questList.Count > 19) questList[19].completionCondition = () => { return questList[19].dialogueShown; };
    }

    private void Update()
    {
        foreach (var quest in questList)
        {
            if (GetQuestState(quest.questID) == QuestState.InProgress &&
                quest.dialogueShown &&
                questStates[quest.questID] != QuestState.Completed &&
                quest.completionCondition != null &&
                quest.completionCondition())
            {
                QuestComplete(quest.questID);
                break;
            }
        }
        if (!isFlaregunDialoguePlayed && FlaregunAcquired) { isFlaregunDialoguePlayed = true; PlayDialogueOnly("q09_heli"); }
        CheckStartEscapeQuests();
    }

    private void CheckStartEscapeQuests() { if (GetQuestState("q08") != QuestState.Completed) return; if (FlaregunAcquired && GetQuestState("q09_heli") == QuestState.NotStarted) StartQuest("q09_heli"); if (CarItemsAcquired && GetQuestState("q10_car") == QuestState.NotStarted) StartQuest("q10_car"); if (BoatItemsAcquired && GetQuestState("q11_boat") == QuestState.NotStarted && !boatEscapeTriggered) StartQuest("q11_boat"); }
    bool CheckDistance(string targetName, float dist) { GameObject t = GameObject.Find(targetName); GameObject p = GameObject.FindGameObjectWithTag("Player"); if (t == null || p == null) return false; return Vector3.Distance(t.transform.position, p.transform.position) <= dist; }
    bool CheckDistance(Transform target, float dist) { GameObject p = GameObject.FindGameObjectWithTag("Player"); if (target == null || p == null) return false; return Vector3.Distance(target.position, p.transform.position) <= dist; }

    public void StartQuest(string questID)
    {
        if (GetQuestState(questID) != QuestState.NotStarted) return;

        QuestData quest = questList.Find(q => q.questID == questID);
        if (quest == null) return;

        questStates[questID] = QuestState.InProgress;
        Debug.Log($"Quest Started: {questID}");

        if (quest.questBgm != null && SoundManager.instance != null)
        {
            SoundManager.instance.PlayBGM(quest.questBgm);
        }

        PlayDialogueOnly(questID);
    }

    public int GetItemCount(string targetName) { if (InventoryManager.instance == null) return 0; int count = 0; foreach (var item in InventoryManager.instance.items) { if (item == null) continue; string myItemClean = item.itemName.Replace(" ", "").ToLower(); string targetClean = targetName.Replace(" ", "").ToLower(); if (myItemClean.Contains(targetClean)) count++; } return count; }
    private string GetEndingSceneName(string escapeMethod) { bool vipSaved = IsVIPSaved; int dogTagCount = GetItemCount("dogtag"); string endingStatus = ""; if (!vipSaved) endingStatus = "Solo"; else { if (dogTagCount >= 5) endingStatus = "True"; else endingStatus = "Normal"; } string finalSceneName = $"Ending_{escapeMethod}_{endingStatus}"; Debug.Log($"[Ending Decision] 수단:{escapeMethod}, VIP:{vipSaved}, 군번줄:{dogTagCount}개 -> 이동할 씬: {finalSceneName}"); return finalSceneName; }

    IEnumerator WaitForHeliEscapeAndEnd(string sceneName, float delayTime)
    {
        Debug.Log($"[Ending] 헬기 이륙 중... {delayTime}초 대기");

        yield return new WaitForSeconds(delayTime);

        // 헬기만 4초 페이드 아웃
        StartCoroutine(LoadEndingScene(sceneName, null, 4.0f));
    }

    IEnumerator LoadEndingScene(string sceneName, AudioClip endingSound, float fadeDuration)
    {
        Debug.Log($"[Ending] 페이드 아웃 시작... {fadeDuration}초 동안");

        if (SoundManager.instance != null) SoundManager.instance.StopBGM();

        if (DialogueUI.instance != null) DialogueUI.instance.panel.SetActive(false);

        if (endingSound != null && audioSource != null)
        {
            audioSource.Stop();
            audioSource.PlayOneShot(endingSound);
        }

        if (fadeScreen != null)
        {
            fadeScreen.blocksRaycasts = true;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                fadeScreen.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                yield return null;
            }
            fadeScreen.alpha = 1f;
        }
        else
        {
            yield return new WaitForSeconds(fadeDuration);
        }

        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"[Error] '{sceneName}' 씬이 없습니다!");
        }
    }

    public void QuestComplete(string questID)
    {
        QuestData quest = questList.Find(q => q.questID == questID);
        if (quest == null || GetQuestState(questID) == QuestState.Completed) return;
        questStates[questID] = QuestState.Completed;
        Debug.Log($"Quest completed: {questID}");

        if (questID == "q01" && managerRingtoneSource != null) managerRingtoneSource.Stop();

        switch (questID)
        {
            case "q08_save_end": if (!string.IsNullOrEmpty(quest.nextQuestID)) StartQuest(quest.nextQuestID); break;

            case "q09_heli_end":
                // 헬기만 4초 페이드 아웃
                float waitTime = (quest.soundDurations.Length > 0 && quest.soundDurations[0] > 0) ? quest.soundDurations[0] : 4.0f;
                StartCoroutine(WaitForHeliEscapeAndEnd(GetEndingSceneName("Heli"), waitTime));
                break;

            case "q10_car_end":
                AudioClip cSound = (quest.dialogueSounds.Length > 0) ? quest.dialogueSounds[0] : null;
                // 자동차/배는 2초 페이드 아웃
                StartCoroutine(LoadEndingScene(GetEndingSceneName("Car"), cSound, 2.0f));
                break;
            case "q11_boat_end":
                AudioClip bSound = (quest.dialogueSounds.Length > 0) ? quest.dialogueSounds[0] : null;
                // 자동차/배는 2초 페이드 아웃
                StartCoroutine(LoadEndingScene(GetEndingSceneName("Boat"), bSound, 2.0f));
                break;

            default: if (!string.IsNullOrEmpty(quest.nextQuestID)) StartQuest(quest.nextQuestID); break;
        }
    }

    public QuestState GetQuestState(string questID) { if (questStates.ContainsKey(questID)) return questStates[questID]; return QuestState.NotStarted; }
}