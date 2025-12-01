using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestData
{
    public string questID;
    [TextArea]
    public string questTitle;
    public string[] dialogueLines;
    public string nextQuestID;

    [HideInInspector] public System.Func<bool> completionCondition;
    [HideInInspector] public bool dialogueShown = false;
}

public enum QuestState
{
    NotStarted,
    InProgress,
    Completed
}

public class QuestManager : MonoBehaviour
{
    public static QuestManager instance;
    private Dictionary<string, QuestState> questStates = new Dictionary<string, QuestState>();
    public List<QuestData> questList = new List<QuestData>();
    public Transform doorApart;  // 인스펙터에서 연결
    public Transform doorServer;  // 인스펙터에서 연결
    // 이벤트 트리거용 bool
    [HideInInspector] public bool USBAcquired = false;
    [HideInInspector] public bool VIPSpoken = false;
    [HideInInspector] public bool FlaregunAcquired = false;
    [HideInInspector] public bool CarItemsAcquired = false;
    [HideInInspector] public bool BoatItemsAcquired = false;

    private void Awake()
    {
        instance = this;

        // 퀘스트 데이터 등록
        // 기존 코드 그대로
        questList = new List<QuestData>()
        {
// ============================
// 0. 게임 시작 인트로
// ============================
new QuestData {
    questID = "q00",
    questTitle = "임무 시작",
    dialogueLines = new string[]{
        "(암전. 파도 소리… 거친 통신 연결음이 섞인다.)",
        "주인공: 여기는 델타. 서버 관리자, 내 목소리 들리나? ...반복한다. 관리자, 응답하라.",
        "(뚝- 연결음이 끊기고 지직거리는 노이즈)",
        "본부(무전): 요원, 그만두게. 그 회선은 죽었어.",
        "본부(무전): 그 섬은 이미 바이러스로 붕괴된 지 오래다.",
        "본부(무전): 자네 임무는 '생존자 수색'이 아니다.",
        "본부(무전): 서버실의 '좀비 백신 데이터 USB' 회수. 그게 최우선이다.",
        "본부(무전): 감정에 휘둘리지 말고 데이터만 확보해. 이상.",
        "(화면이 밝아지며 게임 시작)"
    },
    nextQuestID = "q01"
},

// ============================
// 1. 전화 벨소리 이벤트
// ============================
new QuestData {
    questID = "q01",
    questTitle = "벨소리를 따라가라",
    dialogueLines = new string[]{
        "(어딘가에서 희미하게 울리는 벨소리…)",
        "주인공(독백): 이 벨소리는…?",
        "주인공(독백): 방금 보낸 호출 신호가 살아있던 건가?",
        "주인공(독백): 살아있든 죽었든, 단서는 반드시 있다. 소리를 따라가자."
    },
    nextQuestID = "q02"
},

// ============================
// 2. 서버관리자 좀비 발견
// ============================
new QuestData {
    questID = "q02",
    questTitle = "감염된 관리자를 발견했다",
    dialogueLines = new string[]{
        "(벨소리가 멈춘 곳… 좀비로 변한 관리자가 서성인다.)",
        "주인공(독백): 이미 감염됐군.",
        "주인공(독백): 미안하지만, 단서를 위해서라도 보내줘야 한다."
    },
    nextQuestID = "q03"
},

// ============================
// 3. 관리자를 처치하고 휴대전화 획득
// ============================
new QuestData {
    questID = "q03",
    questTitle = "관리자를 처치하고 단서를 찾아라",
    dialogueLines = new string[]{
        "좀비 관리자 처치 완료.",
        "시스템: 아이템 획득 - 서버관리자의 휴대전화",
        "주인공(독백): 아직 작동하네. 서버실 위치를 찾을 수 있겠어."
    },
    nextQuestID = "q04"
},

// ============================
// 4. 휴대전화 정보 확인 (지도/문자 퍼즐)
// ============================
new QuestData {
    questID = "q04",
    questTitle = "휴대전화 정보를 해독하라",
    dialogueLines = new string[]{
        "지도 앱 실행...",
        "주인공(독백): 백화점, 주차타워… 서버실 위치 확인.",
        "문자 기록 분석 중...",
        "문제: 숫자야구 퍼즐(159, 765, 576)을 조합해 주소를 찾아라.",
        "주인공(독백): 가능한 조합은 하나뿐이다. 답을 찾았다."
    },
    nextQuestID = "q05"
},

// ============================
// 5. 관리자의 집 도착
// ============================
new QuestData {
    questID = "q05",
    questTitle = "관리자의 집을 찾아라",
    dialogueLines = new string[]{
        "7동 756호 도착.",
        "문이 열려 있다."
    },
    nextQuestID = "q06"
},

// ============================
// 6. 서버실로 이동
// ============================
new QuestData {
    questID = "q06",
    questTitle = "서버실로 이동하라",
    dialogueLines = new string[]{
        "서버실 열쇠 획득 완료.",
        "주인공(독백): 장비 점검하고 서버실로 간다."
    },
    nextQuestID = "q07"
},

// ============================
// 7. 서버실 데이터 복사
// ============================
new QuestData {
    questID = "q07",
    questTitle = "데이터를 복사하라",
    dialogueLines = new string[]{
        "데이터 복사 중… 0% → 90% → 100%",
        "시스템: USB 데이터 확보 완료",
        "본부(무전): 백신 데이터 확보 확인. 잘했다, 요원.",
        "본부(무전): 추가 명령. VIP 코드명 ‘로열’의 신호가 포착됐다.",
        "(삐-삐-삐- 서버실 경보 발동)",
        "시스템: 경고 - 좀비들이 1차 강화 상태에 돌입했습니다!",
        "본부(무전): 위험하면 VIP는 포기해도… 데이터만이라도… 치직…"
    },
    nextQuestID = "q08"
},

// ============================
// 8. 경보 + 좀비 강화 + 선택 퀘스트 개방
// ============================
new QuestData {
    questID = "q08",
    questTitle = "탈출 방법을 찾아라",
    dialogueLines = new string[]{
        "경보가 울려 퍼진다!",
        "좀비가 1차 강화 상태에 돌입했다!",
        "본부: VIP 구조는 선택이다. 판단은 요원에게 맡긴다."
    },
    nextQuestID = "" // 여기서 선택지
},

// ----------------------------
// 선택 퀘스트: VIP 구출 루트
// ----------------------------
new QuestData {
    questID = "q08_save",
    questTitle = "VIP를 구출하라 (선택)",
    dialogueLines = new string[]{
        "VIP가 백화점 인근에서 발견됐다.",
        "주인공(독백): 위험하지만… 살릴 수 있다면 살려야 한다."
    },
    nextQuestID = "q08_save_escape"
},

new QuestData {
    questID = "q08_save_escape",
    questTitle = "VIP와 함께 탈출하라",
    dialogueLines = new string[]{
        "VIP 확보 완료.",
        "상태 이상: VIP 호위 - 이동속도 감소.",
        "주인공(독백): 조심해서 이동하자."
    },
    nextQuestID = "q08_save_end"
},

new QuestData {
    questID = "q08_save_end",
    questTitle = "탈출",
    dialogueLines = new string[]{
        "VIP와 함께 탈출에 성공했다.",
        "주인공은 영웅으로 기록될 것이다."
    },
    nextQuestID = ""
},

// ============================
// 헬기 탈출 루트
// ============================
new QuestData {
    questID = "q09_heli",
    questTitle = "헬기로 탈출하라",
    dialogueLines = new string[]{
        "플레어건 획득.",
        "주차타워 옥상에서 신호탄을 쏴야 한다."
    },
    nextQuestID = "q09_heli_defense"
},

new QuestData {
    questID = "q09_heli_defense",
    questTitle = "3분간 버텨라",
    dialogueLines = new string[]{
        "플레어건 발사!",
        "좀비 2차 강화 돌입!",
        "헬기 도착까지 3분 동안 버텨라!"
    },
    nextQuestID = "q09_heli_end"
},

new QuestData {
    questID = "q09_heli_end",
    questTitle = "헬기 탈출",
    dialogueLines = new string[]{
        "헬기가 도착했다!",
        "헬기에 탑승해 섬을 벗어났다."
    },
    nextQuestID = ""
},

// ============================
// 자동차 탈출 루트
// ============================
new QuestData {
    questID = "q10_car",
    questTitle = "자동차로 탈출하라",
    dialogueLines = new string[]{
        "차 키 + 폭발물 획득.",
        "터널만 뚫으면 육로로 나갈 수 있다."
    },
    nextQuestID = "q10_car_tunnel"
},

new QuestData {
    questID = "q10_car_tunnel",
    questTitle = "터널을 폭파하라",
    dialogueLines = new string[]{
        "폭발물 설치… 콰앙!",
        "좀비 2차 강화 상태 돌입!"
    },
    nextQuestID = "q10_car_end"
},

new QuestData {
    questID = "q10_car_end",
    questTitle = "탈출",
    dialogueLines = new string[]{
        "자동차로 터널을 빠져나왔다.",
        "섬을 벗어나는 데 성공했다."
    },
    nextQuestID = ""
},

// ============================
// 배 탈출 루트
// ============================
new QuestData {
    questID = "q11_boat",
    questTitle = "배로 탈출하라",
    dialogueLines = new string[]{
        "배 키 획득.",
        "부두로 이동하자."
    },
    nextQuestID = "q11_boat_end"
},

new QuestData {
    questID = "q11_boat_end",
    questTitle = "탈출",
    dialogueLines = new string[]{
        "배를 타고 섬을 떠났다.",
        "생존에 성공했다."
    },
    nextQuestID = ""
}
        };

        SetupTestConditions();
    }

    private void SetupTestConditions()
    {
        questList[0].completionCondition = () =>
        {
            if (!questList[0].dialogueShown) return false;
            // (!) 사운드 추가시 수정
            //if (!Sound.Instance.BellRang)
            //{
             //   Sound.Instance.PlayBell();
              //  Enemy.Instance.MoveZombiesToBell();
            //}
            return true;
        };

        questList[1].completionCondition = () =>
        {
            if (!questList[1].dialogueShown) return false;

            GameObject zombie = GameObject.Find("ZombieScientist1"); // (!) 좀비 프리팹명 변경 필요시 수정
            if (zombie == null) return false;

            Transform player = GameObject.FindGameObjectWithTag("Player").transform;
            if (player == null) return false;

            float distance = Vector3.Distance(zombie.transform.position, player.position);

            // 원하는 범위값 (예: 5f)
            return distance <= 5f;
        };


            questList[2].completionCondition = () =>
        {
            if (!questList[2].dialogueShown) return false;
            GameObject zombie = GameObject.Find("ZombieScientist1"); // (!) 좀비 프리팹명 변경 필요시 수정

            // (!) 완료시 제거 - 좀비 이상으로 바닥에서 떨어지는 현상으로 인해 임시 처리
            if (InventoryManager.instance.HasItem("Phone"))
            {
                Debug.Log("폰 주움");
                return true;

            }
            EnemyHealth health = zombie.GetComponent<EnemyHealth>();
            return health != null && health.GetCurrentHealth() <= 0 && InventoryManager.instance.HasItem("Phone");
        };

        questList[3].completionCondition = () =>
        {
            if (!questList[3].dialogueShown) return false;

            return questList[3].dialogueShown;
        };

        questList[4].completionCondition = () =>
        {
            if (!questList[4].dialogueShown) return false;
            if (doorApart == null) return false;

            Transform player = GameObject.FindGameObjectWithTag("Player").transform;
            if (player == null) return false;

            return Vector3.Distance(doorApart.position, player.position) <= 3f;
        };

        questList[5].completionCondition = () =>
        {
            if (!questList[5].dialogueShown) return false;
            if (InventoryManager.instance.HasItem("ServerRoomCard")) Debug.Log("카드가 있어요");
            return InventoryManager.instance.HasItem("ServerRoomCard");
        };

        questList[6].completionCondition = () =>
        {
            if (!questList[6].dialogueShown) return false;
            if (doorServer == null) return false;

            Transform player = GameObject.FindGameObjectWithTag("Player").transform;
            if (player == null) return false;

            return Vector3.Distance(doorServer.position, player.position) <= 3f;
        };

        questList[7].completionCondition = () =>
        {
            if (!questList[7].dialogueShown) return false;

            // USB 아이템이 없을 때만 true
            if (!InventoryManager.instance.HasItem("USB"))
            {
                Debug.Log("USB가 없습니다!");
                return true;
            }

            // USB가 있으면 false 반환
            return false;
        };

        questList[8].completionCondition = () =>
        {
            if (!questList[8].dialogueShown) return false;

            // (!) USB는 이미 q07에서 확보됐다고 가정 (임시로 주석처리함 제거해야함)
            //if (!questList[7].dialogueShown) return false;

            // 각 조건을 체크하고, 완료되면 다음 퀘스트를 시작
            if (VIPSpoken && !string.IsNullOrEmpty(questList[9].questID))
            {
                StartQuest(questList[9].questID);
            }

            // 필수 루트: 플레어건, 자동차, 배 중 하나라도 완료되면 q08 완료
            bool anyMandatoryCompleted = FlaregunAcquired || CarItemsAcquired || BoatItemsAcquired;

            // 해당 조건을 만족하면 후속 퀘스트 시작
            if (FlaregunAcquired && !string.IsNullOrEmpty(questList[12].questID))
            {
                StartQuest(questList[12].questID);
            }

            if (CarItemsAcquired && !string.IsNullOrEmpty(questList[15].questID))
            {
                Debug.Log("분기 성공");
                StartQuest(questList[15].questID);
            }

            if (BoatItemsAcquired && !string.IsNullOrEmpty(questList[18].questID))
            {
                StartQuest(questList[18].questID);
            }

            // 필수 루트 중 하나라도 완료되면 q08 자체 완료
            if (anyMandatoryCompleted)
            {
                Debug.Log("q08 완료!");
                return true;
            }

            return false;
        };

        // q08_save: VIP와 대화해야 시작
        questList[9].completionCondition = () =>
        {
            if (!questList[9].dialogueShown) return false;
            return VIPSpoken;
        };

        // q09_heli: 플레어건 획득해야 시작
        questList[12].completionCondition = () =>
        {
            if (!questList[12].dialogueShown) return false;
            return FlaregunAcquired;
        };

        // q10_car: 자동차 아이템 획득해야 시작
        questList[15].completionCondition = () =>
        {
            if (!questList[15].dialogueShown) return false;
            return CarItemsAcquired;
        };

        // q11_boat: 배 아이템 획득해야 시작
        questList[18].completionCondition = () =>
        {
            if (!questList[18].dialogueShown) return false;
            return BoatItemsAcquired;
        };


    }

    private void Update()
    {
        foreach (var quest in questList)
        {
            // 대사가 끝나고, 아직 완료되지 않은 퀘스트만 체크
            if (GetQuestState(quest.questID) == QuestState.InProgress &&
                quest.dialogueShown &&                // 대사가 끝난 경우만
                questStates[quest.questID] != QuestState.Completed &&
                quest.completionCondition != null &&
                quest.completionCondition())
            {
                QuestComplete(quest.questID);
                // 다음 퀘스트는 StartQuest에서 대사 출력 시작
                break; // 한 프레임에 한 퀘스트만 처리
            }
        }
    }


    public void StartQuest(string questID)
    {
        if (GetQuestState(questID) != QuestState.NotStarted) return;

        QuestData quest = questList.Find(q => q.questID == questID);
        if (quest == null) return;

        questStates[questID] = QuestState.InProgress;
        quest.dialogueShown = false;

        DialogueUI.instance.ShowDialogue(
            quest.dialogueLines,
            () =>
            {
                quest.dialogueShown = true;
            }
        );
    }

    public void QuestComplete(string questID)
    {
        QuestData quest = questList.Find(q => q.questID == questID);
        if (quest == null) return;
        if (GetQuestState(questID) == QuestState.Completed) return;

        questStates[questID] = QuestState.Completed;
        Debug.Log($"Quest completed: {questID}");

        if (!string.IsNullOrEmpty(quest.nextQuestID))
            StartQuest(quest.nextQuestID);
    }

    public QuestState GetQuestState(string questID)
    {
        if (questStates.ContainsKey(questID))
            return questStates[questID];
        return QuestState.NotStarted;
    }

    public QuestData GetCurrentQuest()
    {
        foreach (var quest in questList)
        {
            if (GetQuestState(quest.questID) == QuestState.InProgress)
                return quest;
        }
        return null;
    }

}
