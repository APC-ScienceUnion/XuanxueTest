using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace XuanZhiShiLian
{
    /// <summary>
    /// 玩家位置数据
    /// </summary>
    [System.Serializable]
    public class PlayerPositionData
    {
        public float x, y, z;
        public string sceneName;
        public string subsceneName; // 子场景名称（如Stage5的不同子场景）
        
        public PlayerPositionData()
        {
            x = y = z = 0f;
            sceneName = "";
            subsceneName = "";
        }
        
        public PlayerPositionData(Vector3 position, string scene, string subscene = "")
        {
            x = position.x;
            y = position.y;
            z = position.z;
            sceneName = scene;
            subsceneName = subscene;
        }
        
        public Vector3 ToVector3() => new Vector3(x, y, z);
    }

    /// <summary>
    /// 相机位置数据
    /// </summary>
    [System.Serializable]
    public class CameraPositionData
    {
        public float x, y, z;
        public float rotX, rotY, rotZ, rotW; // 四元数旋转
        
        public CameraPositionData()
        {
            x = y = z = 0f;
            rotX = rotY = rotZ = 0f;
            rotW = 1f;
        }
        
        public CameraPositionData(Vector3 position, Quaternion rotation)
        {
            x = position.x;
            y = position.y;
            z = position.z;
            rotX = rotation.x;
            rotY = rotation.y;
            rotZ = rotation.z;
            rotW = rotation.w;
        }
        
        public Vector3 ToPosition() => new Vector3(x, y, z);
        public Quaternion ToRotation() => new Quaternion(rotX, rotY, rotZ, rotW);
    }

    /// <summary>
    /// 导出结果信息
    /// </summary>
    [System.Serializable]
    public class ExportResult
    {
        public bool Success;
        public string FilePath;
        public string FileName;
        public string Message;
    }

    // 用于对外导出的精简结构（不包含仅用于场景加载的Stage1/2/3/4/6数据）
    [System.Serializable]
    public class Stage5ExportData
    {
        public int goodEvilValue;
        public int truthValue;
        public int loveDesireValue;
        public int money;
    }
    [System.Serializable]
    public class SaveExportData
    {
        // 基本信息
        public string playerName;
        public int currentStage;
        public float totalProgress;
        public string saveTime;
        public string gameVersion;

        // 位置
        public PlayerPositionData playerPosition;
        public CameraPositionData cameraPosition;

        // 作答记录（对外必需）
        public List<AnswerRecord> answerRecords;

        // 仅导出Stage5四个属性
        public Stage5ExportData stage5;

        // 按Stage背包
        public List<StageInventoryEntry> stageInventories;
    }

    /// <summary>
    /// 可序列化的物品数据（用于存档）
    /// </summary>
    [System.Serializable]
    public class SerializableItem
    {
        public int id;
        public string name;
        public string description;
        public string iconPath; // 图标路径，而不是Sprite对象
        
        public SerializableItem()
        {
            id = 0;
            name = "";
            description = "";
            iconPath = "";
        }
        
        public SerializableItem(Item item)
        {
            id = item.id;
            name = item.name;
            description = item.description;
            iconPath = ""; // 图标路径需要单独处理
        }
        
        public Item ToItem()
        {
            // 从物品数据创建Item对象，图标暂时为null
            return new Item(id, name, description, null);
        }
        
        public bool IsEmpty => id == 0;
    }

    /// <summary>
    /// Stage5选择记录
    /// </summary>
    [System.Serializable]
    public class Stage5ChoiceRecord
    {
        public string npcId;           // NPC标识
        public string choiceText;      // 选择的具体内容
        public int choiceIndex;        // 选择的索引
        public string timestamp;       // 选择时间
        public int goodEvilChange;     // 善恶值变化
        public int truthChange;        // 真理值变化
        public int loveDesireChange;   // 爱欲值变化
        public int moneyChange;        // 金钱变化
        
        public Stage5ChoiceRecord()
        {
            npcId = "";
            choiceText = "";
            choiceIndex = 0;
            timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            goodEvilChange = truthChange = loveDesireChange = moneyChange = 0;
        }
        
        public Stage5ChoiceRecord(string npc, string choice, int index, int goodEvil = 0, int truth = 0, int love = 0, int money = 0)
        {
            npcId = npc;
            choiceText = choice;
            choiceIndex = index;
            timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            goodEvilChange = goodEvil;
            truthChange = truth;
            loveDesireChange = love;
            moneyChange = money;
        }
    }

    /// <summary>
    /// NPC交互状态条目
    /// </summary>
    [System.Serializable]
    public class NPCInteractionEntry
    {
        public string npcId;
        public bool hasInteracted;
        
        public NPCInteractionEntry() { }
        
        public NPCInteractionEntry(string id, bool interacted)
        {
            npcId = id;
            hasInteracted = interacted;
        }
    }
    
    /// <summary>
    /// NPC名称变更条目
    /// </summary>
    [System.Serializable]
    public class NPCNameChangeEntry
    {
        public string originalName;
        public string newName;
        
        public NPCNameChangeEntry() { }
        
        public NPCNameChangeEntry(string original, string changed)
        {
            originalName = original;
            newName = changed;
        }
    }
    
    /// <summary>
    /// 各Stage特有属性数据
    /// </summary>
    [System.Serializable]
    public class StageSpecificData
    {
        // Stage1 数据
        [System.Serializable]
        public class Stage1Data
        {
            public bool isQuestionCompleted = false; // Stage1唯一题是否完成
        }

        // Stage2 数据
        [System.Serializable]
        public class Stage2Data
        {
            public bool isInHome = true; // 当前在家/墓园
            public int completedQuestions = 0; // 完成题目数（2-8）
        }

        // Stage3 数据
        [System.Serializable]
        public class Stage3Data
        {
            public bool isInCorridor = true;
            public int completedQuestions = 0; // 完成题目数（9-39）
            public bool isExamStarted = false;
            public bool hasPaper = false;
            public bool isExamSubmitted = false; // 题40是否已回答
        }

        // Stage4 数据
        [System.Serializable]
        public class Stage4Data
        {
            // 六把钥匙状态
            public bool right_stair_key = false;
            public bool left_stair_key = false;
            public bool right_floor2_key = false;
            public bool left_floor2_key = false;
            public bool right_floor3_key = false;
            public bool left_floor3_key = false;
            public bool hasAnsweredFillInBlank = false; // 题41
        }

        // Stage6 数据
        [System.Serializable]
        public class Stage6Data
        {
            public bool isQuestionCompleted = false; // 题80
        }

        // ===== 各Stage数据聚合 =====
        public Stage1Data stage1 = new Stage1Data();
        public Stage2Data stage2 = new Stage2Data();
        public Stage3Data stage3 = new Stage3Data();
        public Stage4Data stage4 = new Stage4Data();
        public Stage6Data stage6 = new Stage6Data();

        // Stage5数据
        public int goodEvilValue = 0;        // 善恶值
        public int truthValue = 0;           // 真理值
        public int loveDesireValue = 0;      // 爱欲值
        public int money = 0;                // 金钱
        public bool canExitScene = false;    // 是否可以离开场景
        public bool hasJi = false;           // 如来神掌-寂
        public bool hasMie = false;          // 如来神掌-灭
        public bool hasKu = false;           // 如来神掌-苦
        public bool canUseMJXW = false;      // 是否可以使用魔将玄武占卜
        public int mjxwUsageCount = 0;       // 魔将玄武使用次数
        public bool hasReceivedFreePickaxe = false; // 是否已获得免费石镐
        public bool canTalkWithQika = false; // 是否可以与奇卡女王对话
        public bool isCallingPrincess = true;// 是否称呼为公主
            public bool isFirstTimeEntry = true; // 是否第一次进入Stage5（决定是否触发魔王对话）
        
        // NPC交互状态 - 使用可序列化的格式
        public List<NPCInteractionEntry> npcInteractionStatus = new List<NPCInteractionEntry>();
        public List<NPCNameChangeEntry> npcNameChanges = new List<NPCNameChangeEntry>();
        
        // Stage5选择记录
        public List<Stage5ChoiceRecord> stage5Choices = new List<Stage5ChoiceRecord>();
        
        // 其他Stage的数据可以在这里扩展
        // public Stage1Data stage1Data;
        // public Stage2Data stage2Data;
        // ...
    }

    /// <summary>
    /// 可序列化的Stage背包数据容器
    /// </summary>
    [System.Serializable]
    public class StageInventoryEntry
    {
        public int stageNumber;
        public List<SerializableItem> items;
        
        public StageInventoryEntry() { }
        
        public StageInventoryEntry(int stage, List<SerializableItem> itemList)
        {
            stageNumber = stage;
            items = itemList ?? new List<SerializableItem>();
        }
    }
    
    /// <summary>
    /// 完整的存档数据结构
    /// </summary>
    [System.Serializable]
    public class SaveData
    {
        // 基本信息
        public string playerName;
        public int currentStage;
        public float totalProgress;
        public string saveTime;
        public string gameVersion = "1.0.0"; // 游戏版本号
        
        // 位置信息
        public PlayerPositionData playerPosition;
        public CameraPositionData cameraPosition;
        
        // 答题记录
        public List<AnswerRecord> answerRecords;
        
        // 各Stage特有数据
        public StageSpecificData stageData;
        
        // 背包数据（按Stage分组存储）- 使用List代替Dictionary以支持JSON序列化
        public List<StageInventoryEntry> stageInventories = new List<StageInventoryEntry>();
        
        // 游戏设置
        public float masterVolume = 1.0f;
        public float musicVolume = 1.0f;
        public float sfxVolume = 1.0f;
        
        public SaveData()
        {
            playerName = "";
            currentStage = 0;
            totalProgress = 0f;
            saveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            gameVersion = "1.0.0";
            
            playerPosition = new PlayerPositionData();
            cameraPosition = new CameraPositionData();
            answerRecords = new List<AnswerRecord>();
            stageData = new StageSpecificData();
            stageInventories = new List<StageInventoryEntry>();
        }
    }
    
    public class SaveSystem : MonoBehaviour
    {
        [Header("存档配置")]
        public string saveFileName = "xuanzhi_save.json"; // 改为JSON格式
        
        private string savePath;
        
        private void Awake()
        {
            // 确保存档路径在任何方法调用前已初始化（Awake 早于其他组件的Start/调用）
            if (string.IsNullOrEmpty(saveFileName))
            {
                saveFileName = "xuanzhi_save.json";
            }
            savePath = System.IO.Path.Combine(Application.persistentDataPath, saveFileName);
        }
        
        private void Start()
        {
            savePath = Path.Combine(Application.persistentDataPath, saveFileName);
        }
        
        public void SaveGame()
        {
            try
            {
                if (string.IsNullOrEmpty(savePath))
                {
                    savePath = Path.Combine(Application.persistentDataPath, saveFileName);
                }
                SaveData saveData = CreateSaveData();
                string jsonData = JsonUtility.ToJson(saveData, true);
                File.WriteAllText(savePath, jsonData);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"保存游戏失败: {e.Message}");
            }
        }

        /// <summary>
        /// 直接用传入的SaveData覆盖当前存档（用于在记录Stage5选择后先即时持久化该列表）
        /// </summary>
        public void OverwriteSaveData(SaveData data)
        {
            try
            {
                if (data == null)
                {
                    return;
                }
                if (string.IsNullOrEmpty(savePath))
                {
                    savePath = Path.Combine(Application.persistentDataPath, saveFileName);
                }
                string jsonData = JsonUtility.ToJson(data, true);
                File.WriteAllText(savePath, jsonData);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"覆盖存档失败: {e.Message}");
            }
        }
        
        public void LoadGame()
        {
            try
            {
                if (string.IsNullOrEmpty(savePath))
                {
                    savePath = Path.Combine(Application.persistentDataPath, saveFileName);
                }
                if (File.Exists(savePath))
                {
                    string jsonData = File.ReadAllText(savePath);
                    SaveData saveData = JsonUtility.FromJson<SaveData>(jsonData);
                    ApplySaveData(saveData);
                }
                else
                {
                    CreateNewSave();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"加载游戏失败: {e.Message}");
                CreateNewSave();
            }
        }
        
        public bool HasSaveFile()
        {
            return File.Exists(savePath);
        }
        
        public void DeleteSave()
        {
            try
            {
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"删除存档失败: {e.Message}");
            }
        }
        
        public static void DeleteSaveFile()
        {
            string savePath = Path.Combine(Application.persistentDataPath, "xuanzhi_save.json");
            try
            {
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"删除存档失败: {e.Message}");
            }
        }
        
        private SaveData CreateSaveData()
        {
            SaveData saveData = new SaveData();
            // 合并已有存档中的 Stage5 数据与各Stage背包，确保不会在保存时丢失
            try
            {
                SaveData existing = GetSaveData();
                // 先完整拷贝已有的Stage5数据（包括isFirstTimeEntry等布尔位与列表），避免被默认值覆盖
                if (existing != null && existing.stageData != null)
                {
                    // 基础数值
                    saveData.stageData.goodEvilValue = existing.stageData.goodEvilValue;
                    saveData.stageData.truthValue = existing.stageData.truthValue;
                    saveData.stageData.loveDesireValue = existing.stageData.loveDesireValue;
                    saveData.stageData.money = existing.stageData.money;
                    saveData.stageData.canExitScene = existing.stageData.canExitScene;
                    saveData.stageData.hasJi = existing.stageData.hasJi;
                    saveData.stageData.hasMie = existing.stageData.hasMie;
                    saveData.stageData.hasKu = existing.stageData.hasKu;
                    saveData.stageData.canUseMJXW = existing.stageData.canUseMJXW;
                    saveData.stageData.mjxwUsageCount = existing.stageData.mjxwUsageCount;
                    saveData.stageData.hasReceivedFreePickaxe = existing.stageData.hasReceivedFreePickaxe;
                    saveData.stageData.canTalkWithQika = existing.stageData.canTalkWithQika;
                    saveData.stageData.isCallingPrincess = existing.stageData.isCallingPrincess;
                    saveData.stageData.isFirstTimeEntry = existing.stageData.isFirstTimeEntry;
                    
                    // 列表深拷贝
                    saveData.stageData.npcInteractionStatus = new List<NPCInteractionEntry>();
                    if (existing.stageData.npcInteractionStatus != null)
                    {
                        foreach (var entry in existing.stageData.npcInteractionStatus)
                        {
                            if (entry != null)
                            {
                                saveData.stageData.npcInteractionStatus.Add(new NPCInteractionEntry(entry.npcId, entry.hasInteracted));
                            }
                        }
                    }
                    saveData.stageData.npcNameChanges = new List<NPCNameChangeEntry>();
                    if (existing.stageData.npcNameChanges != null)
                    {
                        foreach (var entry in existing.stageData.npcNameChanges)
                        {
                            if (entry != null)
                            {
                                saveData.stageData.npcNameChanges.Add(new NPCNameChangeEntry(entry.originalName, entry.newName));
                            }
                        }
                    }
                    saveData.stageData.stage5Choices = new List<Stage5ChoiceRecord>();
                    if (existing.stageData.stage5Choices != null)
                    {
                        foreach (var choice in existing.stageData.stage5Choices)
                        {
                            if (choice != null)
                            {
                                // 逐项复制
                                saveData.stageData.stage5Choices.Add(new Stage5ChoiceRecord
                                {
                                    npcId = choice.npcId,
                                    choiceText = choice.choiceText,
                                    choiceIndex = choice.choiceIndex,
                                    timestamp = choice.timestamp,
                                    goodEvilChange = choice.goodEvilChange,
                                    truthChange = choice.truthChange,
                                    loveDesireChange = choice.loveDesireChange,
                                    moneyChange = choice.moneyChange
                                });
                            }
                        }
                    }
                }
                // 合并各Stage背包数据（深拷贝）
                if (existing != null && existing.stageInventories != null && existing.stageInventories.Count > 0)
                {
                    saveData.stageInventories = new List<StageInventoryEntry>();
                    foreach (var entry in existing.stageInventories)
                    {
                        var copied = new StageInventoryEntry(entry.stageNumber, new List<SerializableItem>());
                        if (entry.items != null)
                        {
                            foreach (var it in entry.items)
                            {
                                // 逐项复制，避免引用共享
                                copied.items.Add(new SerializableItem { id = it.id, name = it.name, description = it.description, iconPath = it.iconPath });
                            }
                        }
                        saveData.stageInventories.Add(copied);
                    }
                }
                // 合并答题记录（以已有存档为基线，运行时记录增量覆盖，不整体替换）
                if (existing != null && existing.answerRecords != null && existing.answerRecords.Count > 0)
                {
                    saveData.answerRecords = new List<AnswerRecord>(existing.answerRecords);
                }

                // 合并各Stage显式数据（Stage1/2/3/4/6）作为基线
                if (existing != null && existing.stageData != null)
                {
                    // Stage1
                    if (existing.stageData.stage1 != null)
                    {
                        saveData.stageData.stage1.isQuestionCompleted = existing.stageData.stage1.isQuestionCompleted;
                    }
                    // Stage2
                    if (existing.stageData.stage2 != null)
                    {
                        saveData.stageData.stage2.isInHome = existing.stageData.stage2.isInHome;
                        saveData.stageData.stage2.completedQuestions = existing.stageData.stage2.completedQuestions;
                    }
                    // Stage3
                    if (existing.stageData.stage3 != null)
                    {
                        saveData.stageData.stage3.isInCorridor = existing.stageData.stage3.isInCorridor;
                        saveData.stageData.stage3.completedQuestions = existing.stageData.stage3.completedQuestions;
                        saveData.stageData.stage3.isExamStarted = existing.stageData.stage3.isExamStarted;
                        saveData.stageData.stage3.hasPaper = existing.stageData.stage3.hasPaper;
                        saveData.stageData.stage3.isExamSubmitted = existing.stageData.stage3.isExamSubmitted;
                    }
                    // Stage4
                    if (existing.stageData.stage4 != null)
                    {
                        saveData.stageData.stage4.right_stair_key = existing.stageData.stage4.right_stair_key;
                        saveData.stageData.stage4.left_stair_key = existing.stageData.stage4.left_stair_key;
                        saveData.stageData.stage4.right_floor2_key = existing.stageData.stage4.right_floor2_key;
                        saveData.stageData.stage4.left_floor2_key = existing.stageData.stage4.left_floor2_key;
                        saveData.stageData.stage4.right_floor3_key = existing.stageData.stage4.right_floor3_key;
                        saveData.stageData.stage4.left_floor3_key = existing.stageData.stage4.left_floor3_key;
                        saveData.stageData.stage4.hasAnsweredFillInBlank = existing.stageData.stage4.hasAnsweredFillInBlank;
                    }
                    // Stage6
                    if (existing.stageData.stage6 != null)
                    {
                        saveData.stageData.stage6.isQuestionCompleted = existing.stageData.stage6.isQuestionCompleted;
                    }
                }
            }
            catch (System.Exception mergeEx)
            {
                Debug.LogWarning($"合并已有Stage5选择记录失败: {mergeEx.Message}");
            }
            GameManager gameManager = GameManager.Instance;
            
            if (gameManager != null)
            {
                // 先更新总体进度，确保写入最新进度值
                gameManager.UpdateProgress();
                // 基本信息
                saveData.playerName = gameManager.playerName;
                saveData.currentStage = gameManager.currentStage;
                saveData.totalProgress = gameManager.totalProgress;
                
                // 保存答题记录（将运行时的记录合并进基线：同questionId以运行时为准，未出现的保留基线）
                if (QuestionSystem.Instance != null &&
                    QuestionSystem.Instance.answerRecords != null &&
                    QuestionSystem.Instance.answerRecords.Count > 0)
                {
                    // 基线：已有answerRecords（可能来自existing）
                    Dictionary<int, AnswerRecord> merged = new Dictionary<int, AnswerRecord>();
                    if (saveData.answerRecords != null)
                    {
                        foreach (var r in saveData.answerRecords)
                        {
                            if (r != null)
                            {
                                merged[r.questionId] = new AnswerRecord(r.questionId, r.answer) { submitTime = r.submitTime };
                            }
                        }
                    }
                    // 运行时：覆盖/新增
                    foreach (var r in QuestionSystem.Instance.answerRecords)
                    {
                        if (r != null)
                        {
                            merged[r.questionId] = new AnswerRecord(r.questionId, r.answer) { submitTime = r.submitTime };
                        }
                    }
                    saveData.answerRecords = new List<AnswerRecord>(merged.Values);
                }
                
                // 保存玩家位置
                PlayerController player = FindObjectOfType<PlayerController>();
                if (player != null)
                {
                    string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                    saveData.playerPosition = new PlayerPositionData(player.transform.position, currentScene);
                }
                
                // 保存相机位置
                Camera mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    saveData.cameraPosition = new CameraPositionData(mainCamera.transform.position, mainCamera.transform.rotation);
                }
                
                // 保存Stage5特有数据
                Stage5Controller stage5 = FindObjectOfType<Stage5Controller>();
                if (stage5 != null)
                {
                    saveData.stageData.goodEvilValue = stage5.goodEvilValue;
                    saveData.stageData.truthValue = stage5.truthValue;
                    saveData.stageData.loveDesireValue = stage5.loveDesireValue;
                    saveData.stageData.money = stage5.money;
                    saveData.stageData.canExitScene = stage5.canExitScene;
                    saveData.stageData.hasJi = stage5.hasJi;
                    saveData.stageData.hasMie = stage5.hasMie;
                    saveData.stageData.hasKu = stage5.hasKu;
                    saveData.stageData.canUseMJXW = stage5.canUseMJXW;
                    saveData.stageData.mjxwUsageCount = stage5.GetMJXWUsageCount();
                    saveData.stageData.hasReceivedFreePickaxe = stage5.GetHasReceivedFreePickaxe();
                    saveData.stageData.canTalkWithQika = stage5.canTalkWithQika;
                    saveData.stageData.isCallingPrincess = stage5.isCallingPrincess;
                    // 持久化：是否第一次进入Stage5
                    saveData.stageData.isFirstTimeEntry = stage5.GetIsFirstTimeEntry();
                    
                    // 保存NPC交互状态 - 转换为可序列化格式
                    Dictionary<string, bool> npcInteractions = stage5.GetNPCInteractionStatus();
                    saveData.stageData.npcInteractionStatus.Clear();
                    foreach (var kvp in npcInteractions)
                    {
                        saveData.stageData.npcInteractionStatus.Add(new NPCInteractionEntry(kvp.Key, kvp.Value));
                    }
                    
                    Dictionary<string, string> npcNames = stage5.GetNPCNameChanges();
                    saveData.stageData.npcNameChanges.Clear();
                    foreach (var kvp in npcNames)
                    {
                        saveData.stageData.npcNameChanges.Add(new NPCNameChangeEntry(kvp.Key, kvp.Value));
                    }
                }
                
                // 保存各Stage显式数据
                // Stage1
                Stage1Controller stage1 = FindObjectOfType<Stage1Controller>();
                if (stage1 != null)
                {
                    // 通过题目1状态推断
                    var q1 = QuestionSystem.Instance != null ? QuestionSystem.Instance.GetQuestionById(1) : null;
                    saveData.stageData.stage1.isQuestionCompleted = q1 != null && q1.isAnswered;
                }
                // Stage2
                Stage2Controller stage2 = FindObjectOfType<Stage2Controller>();
                if (stage2 != null)
                {
                    // 通过题目2-8统计与位置推断（位置变量未公开，这里以完成数为主）
                    int completed2to8 = 0;
                    if (QuestionSystem.Instance != null)
                    {
                        for (int i = 2; i <= 8; i++)
                        {
                            var q = QuestionSystem.Instance.GetQuestionById(i);
                            if (q != null && q.isAnswered) completed2to8++;
                        }
                    }
                    saveData.stageData.stage2.completedQuestions = completed2to8;
                    // 根据玩家当前位置与两个锚点的距离推断是否在家
                    try
                    {
                        var playerForS2 = FindObjectOfType<PlayerController>();
                        if (playerForS2 != null)
                        {
                            float dHome = stage2.homePlayerPosition != null ? Vector3.Distance(playerForS2.transform.position, stage2.homePlayerPosition.position) : float.MaxValue;
                            float dGrave = stage2.graveyardPlayerPosition != null ? Vector3.Distance(playerForS2.transform.position, stage2.graveyardPlayerPosition.position) : float.MaxValue;
                            saveData.stageData.stage2.isInHome = dHome <= dGrave;
                        }
                    }
                    catch {}
                }
                // Stage3
                Stage3Controller stage3 = FindObjectOfType<Stage3Controller>();
                if (stage3 != null)
                {
                    int completed9to39 = 0;
                    bool isSubmitted = false;
                    if (QuestionSystem.Instance != null)
                    {
                        for (int i = 9; i <= 39; i++)
                        {
                            var q = QuestionSystem.Instance.GetQuestionById(i);
                            if (q != null && q.isAnswered) completed9to39++;
                        }
                        var q40 = QuestionSystem.Instance.GetQuestionById(40);
                        isSubmitted = q40 != null && q40.isAnswered;
                    }
                    saveData.stageData.stage3.completedQuestions = completed9to39;
                    saveData.stageData.stage3.isExamStarted = completed9to39 >= 31;
                    saveData.stageData.stage3.hasPaper = completed9to39 >= 31; // 逻辑与控制器一致
                    saveData.stageData.stage3.isExamSubmitted = isSubmitted;
                    // 根据玩家当前位置与两个锚点的距离推断是否在走廊
                    try
                    {
                        var playerForS3 = FindObjectOfType<PlayerController>();
                        if (playerForS3 != null)
                        {
                            float dCorr = stage3.corridorPlayerPosition != null ? Vector3.Distance(playerForS3.transform.position, stage3.corridorPlayerPosition.position) : float.MaxValue;
                            float dClass = stage3.classroomPlayerPosition != null ? Vector3.Distance(playerForS3.transform.position, stage3.classroomPlayerPosition.position) : float.MaxValue;
                            saveData.stageData.stage3.isInCorridor = dCorr <= dClass;
                        }
                    }
                    catch {}
                }
                // Stage4
                Stage4Controller stage4 = FindObjectOfType<Stage4Controller>();
                if (stage4 != null)
                {
                    // 直接从运行时控制器读取真实钥匙状态，避免推断错误
                    saveData.stageData.stage4.right_stair_key = stage4.HasKey("right_stair_key");
                    saveData.stageData.stage4.left_stair_key = stage4.HasKey("left_stair_key");
                    saveData.stageData.stage4.right_floor2_key = stage4.HasKey("right_floor2_key");
                    saveData.stageData.stage4.left_floor2_key = stage4.HasKey("left_floor2_key");
                    saveData.stageData.stage4.right_floor3_key = stage4.HasKey("right_floor3_key");
                    saveData.stageData.stage4.left_floor3_key = stage4.HasKey("left_floor3_key");
                    // 题41状态从题库读取
                    if (QuestionSystem.Instance != null)
                    {
                        var q41 = QuestionSystem.Instance.GetQuestionById(41);
                        saveData.stageData.stage4.hasAnsweredFillInBlank = q41 != null && q41.isAnswered;
                    }
                }
                // Stage6
                Stage6Controller stage6 = FindObjectOfType<Stage6Controller>();
                if (stage6 != null)
                {
                    // 通过题目80状态推断
                    var q80 = QuestionSystem.Instance != null ? QuestionSystem.Instance.GetQuestionById(80) : null;
                    saveData.stageData.stage6.isQuestionCompleted = q80 != null && q80.isAnswered;
                }
                
                // 保存当前Scene的背包数据
                if (InventorySystem.Instance != null)
                {
                    SaveCurrentInventoryData(saveData, gameManager.currentStage);
                }
            }
            
            saveData.saveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            return saveData;
        }

        // ===== 辅助：Stage4正确性与钥匙名映射（与Stage4Controller一致） =====
        private bool CheckStage4AnswerCorrect(int questionId)
        {
            // 复用Stage4Controller的判定逻辑不可见，这里做保守处理：
            // 若题目已作答，则认为交互达成；实际是否正确由Stage4进入时再校验并刷新
            // 为保证一致性，如需严格校验可在此复制Stage4Controller.CheckAnswerCorrectness的判断
            return true;
        }
        private string GetStage4KeyName(int questionId)
        {
            switch (questionId)
            {
                case 42: return "left_stair_key";
                case 43: return "right_stair_key";
                case 44: return "left_floor2_key";
                case 45: return "right_floor2_key";
                case 46: return "left_floor3_key";
                case 47: return "right_floor3_key";
                default: return string.Empty;
            }
        }
        private void SetStage4Key(StageSpecificData.Stage4Data s4, string key, bool value)
        {
            if (s4 == null) return;
            if (key == "right_stair_key") s4.right_stair_key = value;
            else if (key == "left_stair_key") s4.left_stair_key = value;
            else if (key == "right_floor2_key") s4.right_floor2_key = value;
            else if (key == "left_floor2_key") s4.left_floor2_key = value;
            else if (key == "right_floor3_key") s4.right_floor3_key = value;
            else if (key == "left_floor3_key") s4.left_floor3_key = value;
        }
        
        /// <summary>
        /// 保存当前场景的背包数据
        /// </summary>
        private void SaveCurrentInventoryData(SaveData saveData, int currentStage)
        {
            if (InventorySystem.Instance == null)
                return;
                
            // 获取当前背包中的所有物品
            Item[] currentItems = InventorySystem.Instance.GetAllItems();
            
            // 转换为可序列化的格式
            List<SerializableItem> serializableItems = new List<SerializableItem>();
            foreach (Item item in currentItems)
            {
                serializableItems.Add(new SerializableItem(item));
            }
            
            // 查找现有的Stage数据或创建新的
            StageInventoryEntry existingEntry = saveData.stageInventories.Find(entry => entry.stageNumber == currentStage);
            if (existingEntry != null)
            {
                existingEntry.items = serializableItems;
            }
            else
            {
                saveData.stageInventories.Add(new StageInventoryEntry(currentStage, serializableItems));
            }
        }
        
        /// <summary>
        /// 将新格式的背包数据转换为Dictionary（用于兼容现有代码）
        /// </summary>
        private Dictionary<int, List<SerializableItem>> ConvertInventoryListToDictionary(List<StageInventoryEntry> inventoryList)
        {
            Dictionary<int, List<SerializableItem>> inventoryDict = new Dictionary<int, List<SerializableItem>>();
            
            if (inventoryList != null)
            {
                foreach (var entry in inventoryList)
                {
                    inventoryDict[entry.stageNumber] = entry.items ?? new List<SerializableItem>();
                }
            }
            
            return inventoryDict;
        }
        
        private void ApplySaveData(SaveData saveData)
        {
            GameManager gameManager = GameManager.Instance;
            
            if (gameManager != null)
            {
                // 恢复基本信息
                gameManager.playerName = saveData.playerName;
                gameManager.currentStage = saveData.currentStage;
                gameManager.totalProgress = saveData.totalProgress;
                
                // 恢复答题记录（仅设置内存状态与缓存，不触发再次保存）
                if (QuestionSystem.Instance != null && saveData.answerRecords != null)
                {
                    QuestionSystem.Instance.answerRecords = new List<AnswerRecord>(saveData.answerRecords);
                    
                    foreach (var answer in saveData.answerRecords)
                    {
                        Question question = QuestionSystem.Instance.GetQuestionById(answer.questionId);
                        if (question != null)
                        {
                            question.playerAnswer = answer.answer;
                            question.isAnswered = true;
                            // 同步答题缓存表
                            QuestionSystem.Instance.answerSheet[answer.questionId] = answer.answer;
                        }
                    }
                }
                
                // 设置位置和背包恢复标志（延迟到场景切换后执行）
                if (saveData.playerPosition != null || saveData.cameraPosition != null || saveData.stageInventories != null)
                {
                    // 将新格式的背包数据转换为Dictionary
                    Dictionary<int, List<SerializableItem>> inventoryDict = ConvertInventoryListToDictionary(saveData.stageInventories);
                    gameManager.SetLoadFromSave(saveData.playerPosition, saveData.cameraPosition, inventoryDict);
                }
                
                // 恢复Stage5属性（延迟到场景加载后执行）
                if (saveData.stageData != null)
                {
                    // 保存Stage5数据到GameManager，等待场景加载后应用
                    gameManager.savedStageData = saveData.stageData;
                }
                
                // 恢复背包物品（如果需要）
                // if (saveData.inventoryItems != null && InventorySystem.Instance != null)
                // {
                //     InventorySystem.Instance.LoadItems(saveData.inventoryItems);
                // }
            }
        }
        
        private void CreateNewSave()
        {
            if (string.IsNullOrEmpty(savePath))
            {
                savePath = Path.Combine(Application.persistentDataPath, saveFileName);
            }
            SaveData newSave = new SaveData();
            string jsonData = JsonUtility.ToJson(newSave, true);
            File.WriteAllText(savePath, jsonData);
        }
        
        public SaveData GetSaveData()
        {
            if (string.IsNullOrEmpty(savePath))
            {
                savePath = Path.Combine(Application.persistentDataPath, saveFileName);
            }
            if (File.Exists(savePath))
            {
                try
                {
                    string jsonData = File.ReadAllText(savePath);
                    return JsonUtility.FromJson<SaveData>(jsonData);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"读取存档数据失败: {e.Message}");
                }
            }
            return new SaveData();
        }
        
        public void ExportSaveData()
        {
            // 使用当前内存生成的完整存档快照，确保包含答题记录与Stage5四属性
            SaveData saveData = CreateSaveData();
            string exportPath = Path.Combine(Application.persistentDataPath, $"存档导出_{System.DateTime.Now:yyyyMMdd_HHmmss}.json");
            
            try
            {
                // 构造对外导出数据：不包含Stage1/2/3/4/6内部状态
                SaveExportData export = new SaveExportData
                {
                    playerName = saveData.playerName,
                    currentStage = saveData.currentStage,
                    totalProgress = saveData.totalProgress,
                    saveTime = saveData.saveTime,
                    gameVersion = saveData.gameVersion,
                    playerPosition = saveData.playerPosition,
                    cameraPosition = saveData.cameraPosition,
                    answerRecords = saveData.answerRecords,
                    stageInventories = saveData.stageInventories,
                    stage5 = new Stage5ExportData
                    {
                        goodEvilValue = saveData.stageData.goodEvilValue,
                        truthValue = saveData.stageData.truthValue,
                        loveDesireValue = saveData.stageData.loveDesireValue,
                        money = saveData.stageData.money
                    }
                };
                string jsonData = JsonUtility.ToJson(export, true);
                File.WriteAllText(exportPath, jsonData);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"导出存档失败: {e.Message}");
            }
        }
        
        public void ImportSaveData(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string jsonData = File.ReadAllText(filePath);
                    SaveData importedData = JsonUtility.FromJson<SaveData>(jsonData);
                    
                    // 验证数据有效性
                    if (ValidateSaveData(importedData))
                    {
                        ApplySaveData(importedData);
                        SaveGame(); // 保存导入的数据
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"导入存档失败: {e.Message}");
            }
        }
        
        private bool ValidateSaveData(SaveData saveData)
        {
            if (saveData == null) return false;
            if (saveData.currentStage < 0 || saveData.currentStage > 7) return false;
            if (saveData.totalProgress < 0f || saveData.totalProgress > 1f) return false;
            
            return true;
        }
        
        // 更新存档信息用于UI显示
        public string GetSaveInfo()
        {
            if (HasSaveFile())
            {
                SaveData saveData = GetSaveData();
                return $"玩家: {saveData.playerName}\n" +
                       $"当前阶段: Stage{saveData.currentStage}\n" +
                       $"完成进度: {saveData.totalProgress:P0}\n" +
                       $"保存时间: {saveData.saveTime}";
            }
            return "没有存档";
        }

        /// <summary>
        /// 导出存档为Markdown格式
        /// </summary>
        /// <returns>导出结果信息</returns>
        public ExportResult ExportToMarkdown()
        {
                        SaveData saveData = CreateSaveData();
                        string markdownContent = GenerateMarkdownContent(saveData);
                        
                        string fileName = $"玄之试炼_存档_{saveData.playerName}_{System.DateTime.Now:yyyyMMdd_HHmmss}.md";
                        
                        // 获取用户下载目录
                        string downloadsPath = GetDownloadsPath();
                        string filePath = Path.Combine(downloadsPath, fileName);
                        
                        File.WriteAllText(filePath, markdownContent, System.Text.Encoding.UTF8);
                        
                        return new ExportResult
                        {
                            Success = true,
                            FilePath = filePath,
                            FileName = fileName,
                            Message = $"存档已成功导出到下载文件夹"
                        };
            }
        
        /// <summary>
        /// 获取用户下载目录路径
        /// </summary>
        private string GetDownloadsPath()
        {
            string downloadsPath = "";
            
            try
            {
                // Windows系统下载目录
                if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
                {
                    string userProfile = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
                    downloadsPath = Path.Combine(userProfile, "Downloads");
                }
                
                
                // 如果无法获取下载目录，使用桌面
                if (string.IsNullOrEmpty(downloadsPath) || !Directory.Exists(downloadsPath))
                {
                    downloadsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"获取下载目录失败: {e.Message}，使用桌面目录");
                downloadsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            }
            
            // 最后的备用方案
            if (string.IsNullOrEmpty(downloadsPath) || !Directory.Exists(downloadsPath))
            {
                downloadsPath = Application.persistentDataPath;
            }
            
            return downloadsPath;
        }
        
        /// <summary>
        /// 记录Stage5选择
        /// </summary>
        public static void RecordStage5Choice(string npcId, string choiceText, int choiceIndex, int goodEvil = 0, int truth = 0, int love = 0, int money = 0)
        {
            if (GameManager.Instance?.saveSystem != null)
            {
                // 创建选择记录
                Stage5ChoiceRecord choice = new Stage5ChoiceRecord(npcId, choiceText, choiceIndex, goodEvil, truth, love, money);
                
                // 获取当前存档数据
                SaveData currentSave = GameManager.Instance.saveSystem.GetSaveData();
                if (currentSave.stageData == null)
                {
                    currentSave.stageData = new StageSpecificData();
                }
                
                // 添加选择记录
                currentSave.stageData.stage5Choices.Add(choice);
                
                // 立即覆盖保存，确保追加逻辑不会被后续CreateSaveData覆盖
                GameManager.Instance.saveSystem.OverwriteSaveData(currentSave);
            }
        }
        
        /// <summary>
        /// 获取Stage5选择记录
        /// </summary>
        public List<Stage5ChoiceRecord> GetStage5Choices()
        {
            SaveData saveData = GetSaveData();
            return saveData.stageData?.stage5Choices ?? new List<Stage5ChoiceRecord>();
        }
        
        /// <summary>
        /// 导出Stage5选择记录为单独的JSON文件
        /// </summary>
        public void ExportStage5Choices()
        {
            try
            {
                List<Stage5ChoiceRecord> choices = GetStage5Choices();
                if (choices.Count == 0)
                {
                    return;
                }
                
                string jsonData = JsonUtility.ToJson(new { stage5Choices = choices }, true);
                string fileName = $"Stage5选择记录_{System.DateTime.Now:yyyyMMdd_HHmmss}.json";
                string filePath = Path.Combine(Application.persistentDataPath, fileName);
                
                File.WriteAllText(filePath, jsonData, System.Text.Encoding.UTF8);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"导出Stage5选择记录失败: {e.Message}");
            }
        }
        
        private string GenerateMarkdownContent(SaveData saveData)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            
            sb.AppendLine("# 玄之试炼 - 游戏存档");
            sb.AppendLine();
            sb.AppendLine($"**玩家姓名：** {saveData.playerName}");
            sb.AppendLine($"**当前阶段：** Stage{saveData.currentStage}");
            sb.AppendLine($"**游戏进度：** {saveData.totalProgress:P1}");
            sb.AppendLine($"**游戏版本：** {saveData.gameVersion}");
            sb.AppendLine();

            // 导出所有Stage的背包数据
            if (saveData.stageInventories != null && saveData.stageInventories.Count > 0)
            {
                sb.AppendLine("## 背包数据（按Stage）");
                sb.AppendLine();
                // 以stage编号排序
                saveData.stageInventories.Sort((a, b) => a.stageNumber.CompareTo(b.stageNumber));
                foreach (var entry in saveData.stageInventories)
                {
                    sb.AppendLine($"### Stage{entry.stageNumber} 背包");
                    if (entry.items == null || entry.items.Count == 0)
                    {
                        sb.AppendLine("- （空）");
                    }
                    else
                    {
                        int index = 1;
                        foreach (var item in entry.items)
                        {
                            if (item != null && !item.IsEmpty)
                            {
                                sb.AppendLine($"- [{index}] {item.name} (ID: {item.id}) - {item.description}");
                            }
                            index++;
                        }
                    }
                    sb.AppendLine();
                }
            }
            
            // Stage5属性数据（仅当当前在Stage5时显示属性摘要）
            if (saveData.stageData != null && saveData.currentStage == 5)
            {
                sb.AppendLine("## Stage5 属性数据");
                sb.AppendLine();
                sb.AppendLine($"**善恶值：** {saveData.stageData.goodEvilValue}");
                sb.AppendLine($"**真理值：** {saveData.stageData.truthValue}");
                sb.AppendLine($"**爱欲值：** {saveData.stageData.loveDesireValue}");
                sb.AppendLine($"**金钱：** {saveData.stageData.money}G");
                sb.AppendLine();
                sb.AppendLine($"**特殊状态：**");
                sb.AppendLine($"- 可离开场景: {(saveData.stageData.canExitScene ? "是" : "否")}");
                sb.AppendLine($"- 如来神掌-寂: {(saveData.stageData.hasJi ? "已获得" : "未获得")}");
                sb.AppendLine($"- 如来神掌-灭: {(saveData.stageData.hasMie ? "已获得" : "未获得")}");
                sb.AppendLine($"- 如来神掌-苦: {(saveData.stageData.hasKu ? "已获得" : "未获得")}");
                sb.AppendLine($"- 魔将玄武占卜: {(saveData.stageData.canUseMJXW ? "可用" : "不可用")}");
                sb.AppendLine($"- 与奇卡女王对话: {(saveData.stageData.canTalkWithQika ? "可以" : "不可以")}");
                sb.AppendLine($"- 称呼方式: {(saveData.stageData.isCallingPrincess ? "公主" : "魔王女儿")}");
                sb.AppendLine();
            }

            // Stage5 选择记录（不受当前Stage限制，只要有数据就导出）
            if (saveData.stageData != null && saveData.stageData.stage5Choices != null && saveData.stageData.stage5Choices.Count > 0)
            {
                sb.AppendLine("## Stage5 选择记录");
                sb.AppendLine();
                foreach (var choice in saveData.stageData.stage5Choices)
                {
                    sb.AppendLine($"**{choice.timestamp}** - 与 **{choice.npcId}** 的对话");
                    sb.AppendLine($"- 选择: {choice.choiceText}");
                    if (choice.goodEvilChange != 0 || choice.truthChange != 0 || choice.loveDesireChange != 0 || choice.moneyChange != 0)
                    {
                        sb.AppendLine($"- 属性变化: 善恶{choice.goodEvilChange:+0;-#;0} 真理{choice.truthChange:+0;-#;0} 爱欲{choice.loveDesireChange:+0;-#;0} 金钱{choice.moneyChange:+0;-#;0}");
                    }
                    sb.AppendLine();
                }
            }
            
            // 答题记录
            if (saveData.answerRecords != null && saveData.answerRecords.Count > 0)
            {
                sb.AppendLine("## 答题记录");
                sb.AppendLine();
                
                foreach (var answer in saveData.answerRecords)
                {
                    sb.AppendLine($"### 题目 {answer.questionId}");
                    sb.AppendLine($"**答案：** {answer.answer}");
                    sb.AppendLine($"**提交时间：** {answer.submitTime}");
                    sb.AppendLine();
                }
            }
            
            sb.AppendLine("---");
            sb.AppendLine($"*导出时间：{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}*");
            
            return sb.ToString();
        }
        
        private string GenerateTextContent(SaveData saveData)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            
            sb.AppendLine("=== 玄之试炼 - 游戏存档 ===");
            sb.AppendLine();
            sb.AppendLine($"玩家姓名：{saveData.playerName}");
            sb.AppendLine($"当前阶段：Stage{saveData.currentStage}");
            sb.AppendLine($"游戏进度：{saveData.totalProgress:P1}");
            sb.AppendLine($"游戏版本：{saveData.gameVersion}");
            sb.AppendLine($"保存时间：{saveData.saveTime}");
            sb.AppendLine();
            
            // 位置信息
            if (saveData.playerPosition != null)
            {
                sb.AppendLine("=== 位置信息 ===");
                sb.AppendLine($"玩家位置：({saveData.playerPosition.x:F2}, {saveData.playerPosition.y:F2}, {saveData.playerPosition.z:F2})");
                sb.AppendLine($"所在场景：{saveData.playerPosition.sceneName}");
                if (!string.IsNullOrEmpty(saveData.playerPosition.subsceneName))
                {
                    sb.AppendLine($"子场景：{saveData.playerPosition.subsceneName}");
                }
                if (saveData.cameraPosition != null)
                {
                    sb.AppendLine($"相机位置：({saveData.cameraPosition.x:F2}, {saveData.cameraPosition.y:F2}, {saveData.cameraPosition.z:F2})");
                }
                sb.AppendLine();
            }
            
            // Stage5数据
            if (saveData.stageData != null && saveData.currentStage == 5)
            {
                sb.AppendLine("=== Stage5 属性数据 ===");
                sb.AppendLine($"善恶值：{saveData.stageData.goodEvilValue}");
                sb.AppendLine($"真理值：{saveData.stageData.truthValue}");
                sb.AppendLine($"爱欲值：{saveData.stageData.loveDesireValue}");
                sb.AppendLine($"金钱：{saveData.stageData.money}G");
                sb.AppendLine();
                sb.AppendLine("特殊状态：");
                sb.AppendLine($"  可离开场景: {(saveData.stageData.canExitScene ? "是" : "否")}");
                sb.AppendLine($"  如来神掌-寂: {(saveData.stageData.hasJi ? "已获得" : "未获得")}");
                sb.AppendLine($"  如来神掌-灭: {(saveData.stageData.hasMie ? "已获得" : "未获得")}");
                sb.AppendLine($"  如来神掌-苦: {(saveData.stageData.hasKu ? "已获得" : "未获得")}");
                sb.AppendLine($"  魔将玄武占卜: {(saveData.stageData.canUseMJXW ? "可用" : "不可用")}");
                sb.AppendLine($"  与奇卡女王对话: {(saveData.stageData.canTalkWithQika ? "可以" : "不可以")}");
                sb.AppendLine($"  称呼方式: {(saveData.stageData.isCallingPrincess ? "公主" : "魔王女儿")}");
                sb.AppendLine();
                
                // Stage5选择记录
                if (saveData.stageData.stage5Choices != null && saveData.stageData.stage5Choices.Count > 0)
                {
                    sb.AppendLine("=== Stage5 选择记录 ===");
                    foreach (var choice in saveData.stageData.stage5Choices)
                    {
                        sb.AppendLine($"[{choice.timestamp}] {choice.npcId} -> {choice.choiceText}");
                        if (choice.goodEvilChange != 0 || choice.truthChange != 0 || choice.loveDesireChange != 0 || choice.moneyChange != 0)
                        {
                            sb.AppendLine($"  属性变化: 善恶{choice.goodEvilChange:+0;-#;0} 真理{choice.truthChange:+0;-#;0} 爱欲{choice.loveDesireChange:+0;-#;0} 金钱{choice.moneyChange:+0;-#;0}");
                        }
                    }
                    sb.AppendLine();
                }
            }
            
            // 答题记录
            if (saveData.answerRecords != null && saveData.answerRecords.Count > 0)
            {
                sb.AppendLine("=== 答题记录 ===");
                sb.AppendLine();
                
                foreach (var answer in saveData.answerRecords)
                {
                    sb.AppendLine($"题目 {answer.questionId}：{answer.answer} (提交时间: {answer.submitTime})");
                }
                sb.AppendLine();
            }
            
            sb.AppendLine("========================");
            sb.AppendLine($"导出时间：{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            
            return sb.ToString();
        }
    }
} 