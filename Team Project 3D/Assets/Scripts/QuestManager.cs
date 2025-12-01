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

    private void Awake()
    {
        instance = this;

        // 퀘스트 데이터 등록
        // 기존 코드 그대로
        questList = new List<QuestData>()
        {
new QuestData {
    questID = "q00",
    questTitle = "서버실의 이상한 기척",
    dialogueLines = new string[]{
        "서버실에서 이상한 소리가 들린다...",
        "전화벨소리가 울리고 있다. 확인해보자.",
        "뭔가 불길한 예감이 든다..."
    },
    nextQuestID = "q01"
},

// --------------------- 흐름도 시작 ---------------------
new QuestData {

    questID = "q01",
    questTitle = "좀비로 변한 서버 관리자",
    dialogueLines = new string[]{
        "서버 관리자가 좀비로 변했다!",
        "조심해서 처치해야 한다!"
    },
    nextQuestID = "q01_kill" // → 처치 퀘스트
},

// 서버 관리자 처치 퀘스트
new QuestData {
    questID = "q01_kill",
    questTitle = "서버 관리자를 처치하라",
    dialogueLines = new string[]{
        "좀비가 된 서버 관리자를 처치하자!"
    },
    nextQuestID = "q02"
},

// 휴대전화 획득
new QuestData {
    questID = "q02",
    questTitle = "휴대전화 획득",
    dialogueLines = new string[]{
        "휴대전화를 얻었다.",
        "이 휴대전화가 도움이 될 것 같다..."
    },
    nextQuestID = "q03"
},

// 서버실 찾아가기
new QuestData {
    questID = "q03",
    questTitle = "서버실로 향하라",
    dialogueLines = new string[]{
        "휴대전화 정보를 토대로 서버실 위치를 찾았다.",
        "서버실로 이동하자!"
    },
    nextQuestID = "q04"
},

// 서버실 도착 → USB 복사
new QuestData {
    questID = "q04",
    questTitle = "USB에 데이터를 복사하라",
    dialogueLines = new string[]{
        "서버실 컴퓨터에서 데이터를 복사해야 한다.",
        "복사가 완료될 때까지 주변을 경계하자."
    },
    nextQuestID = "q05"
},

// 데이터 복사 완료 → 경보 → 좀비 강화
new QuestData {
    questID = "q05",
    questTitle = "경보 발생!",
    dialogueLines = new string[]{
        "경보가 울렸다!",
        "좀비가 경보음에 반응하여 강화되었다!"
    },
    nextQuestID = "q06" // 다음: 동시 퀘스트 생성
},

// 동시 퀘스트 트리거 (선택+메인)
new QuestData {
    questID = "q06",
    questTitle = "탈출 방법을 찾아라",
    dialogueLines = new string[]{
        "이제 탈출 방법을 찾아야 한다."
    },
    nextQuestID = "" // → 여기서 추가로 q06_save(선택 퀘스트)도 실행해야 함
},

// 선택 퀘스트 : 주요 인물을 구출하라
new QuestData {
    questID = "q06_save",
    questTitle = "주요 인물을 구출하라 (선택)",
    dialogueLines = new string[]{
        "주요 인물이 아직 살아있다!",
        "그를 구출하자!"
    },
    nextQuestID = "q06_save_escape"
},

// 선택 퀘스트 : 주요 인물과 함께 탈출
new QuestData {
    questID = "q06_save_escape",
    questTitle = "주요 인물과 함께 탈출하라",
    dialogueLines = new string[]{
        "주요 인물을 데리고 탈출해야 한다.",
        "이동속도가 느려졌으니 조심하자..."
    },
    nextQuestID = "q06_save_end"
},

new QuestData {
    questID = "q06_save_end",
    questTitle = "탈출",
    dialogueLines = new string[]{
        "배에 도착했다!",
        "주요 인물을 태우고 배를 출발시켰다!",
        "당신은 주요 인물을 구출한 영웅이다..."
    },
    nextQuestID = ""
},

// 아이템 획득에 따라 분기되는 엔딩 퀘스트들

// 플레어건 → 헬기로 탈출
new QuestData {
    questID = "q07_heli",
    questTitle = "헬기로 탈출하라",
    dialogueLines = new string[]{
        "플레어건을 획득했다!",
        "주차타워 옥상으로 가서 구조 신호를 쏘자!"
    },
    nextQuestID = "q07_heli_defense"
},

new QuestData {
    questID = "q07_heli_defense",
    questTitle = "좀비를 막아라 (3분 버티기)",
    dialogueLines = new string[]{
        "플레어건으로 인해 좀비들이 몰려들기 시작했다",
        "헬기가 오는 동안 3분간 좀비를 막아야 한다!",
        "버텨라!!!"
    },
    nextQuestID = "q07_heli_end"
},

new QuestData {
    questID = "q07_heli_end",
    questTitle = "탈출",
    dialogueLines = new string[]{
        "헬기가 도착했다!",
        "헬기에 탑승하여 살아남았다!"
    },
    nextQuestID = ""
},

// 자동차 탈출
new QuestData {
    questID = "q08_car",
    questTitle = "자동차로 탈출하라",
    dialogueLines = new string[]{
        "차 키와 폭발물을 입수했다!",
        "차량이 있는 주차장으로 이동하자."
    },
    nextQuestID = "q08_car_tunnel"
},

new QuestData {
    questID = "q08_car_tunnel",
    questTitle = "터널을 폭파하라",
    dialogueLines = new string[]{
        "터널이 막혀 있다.",
        "폭발물을 사용해 길을 뚫어야 한다!"
    },
    nextQuestID = "q08_car_end"
},

new QuestData {
    questID = "q08_car_end",
    questTitle = "탈출",
    dialogueLines = new string[]{
        "자동차로 터널을 빠져나왔다!",
        "당신은 살아남았다!"
    },
    nextQuestID = ""
},

// 배로 탈출
new QuestData {
    questID = "q09_boat",
    questTitle = "배로 탈출하라",
    dialogueLines = new string[]{
        "배키를 획득했다!",
        "부두로 가자!"
    },
    nextQuestID = "q09_boat_end"
},

new QuestData {
    questID = "q09_boat_end",
    questTitle = "탈출",
    dialogueLines = new string[]{
        "배를 타고 도시를 빠져나왔다!",
        "당신은 살아남았다!"
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
            // 이벤트 한번만 실행
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
            return GameObject.Find("ZombieScientist1") != null;
        };

        questList[2].completionCondition = () =>
        {
            if (!questList[2].dialogueShown) return false;
            GameObject zombie = GameObject.Find("ZombieScientist1");
            if (zombie == null) return true;
            EnemyHealth health = zombie.GetComponent<EnemyHealth>();
            return health != null && health.GetCurrentHealth() <= 0;
        };

        questList[3].completionCondition = () =>
        {
            return questList[3].dialogueShown;
        };

        questList[4].completionCondition = () => Input.GetKeyDown(KeyCode.B);
        questList[5].completionCondition = () => true;
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
