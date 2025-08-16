using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace XuanZhiShiLian
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        
        [Header("游戏状态")]
        public string playerName = "";
        public int currentStage = 0;
        public float totalProgress = 0f;
        public bool isGameStarted = false;
        
        [Header("系统组件")]
        public UISystem uiSystem;
        public SaveSystem saveSystem;
        
        // 注意：QuestionSystem 和 DialogueSystem 现在是单例，不需要在这里引用
        
        [Header("场景名称")]
        public string[] sceneNames = {
            "Stage0", "Stage1", "Stage2", "Stage3", 
            "Stage4", "Stage5", "Stage6", "Stage7"
        };
        
        [Header("存档恢复")]
        public bool isLoadingFromSave = false;
        public PlayerPositionData savedPlayerPosition;
        public CameraPositionData savedCameraPosition;
        public Dictionary<int, List<SerializableItem>> savedStageInventories;
        public StageSpecificData savedStageData;
        
        private void Awake()
        {
            // 单例模式
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeGame();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializeGame()
        {
            
            // 检查和初始化QuestionSystem
            if (GetComponent<QuestionSystem>() == null)
            {
                gameObject.AddComponent<QuestionSystem>();
            }
            
            // 检查和初始化DialogueSystem
            if (GetComponent<DialogueSystem>() == null)
            {
                gameObject.AddComponent<DialogueSystem>();
            }
            
            // 初始化其他系统
            if (uiSystem == null)
                uiSystem = GetComponent<UISystem>();
            if (saveSystem == null)
                saveSystem = GetComponent<SaveSystem>();
                
            // 加载存档
            LoadGameData();
        }
        
        public void SetPlayerName(string name)
        {
            playerName = name;
            SaveGameData();
        }
        
        public void StartNewGame()
        {
            isGameStarted = true;
            currentStage = 0;
            playerName = "";
            
            // 重置所有题目状态 - 通过单例访问
            if (QuestionSystem.Instance != null)
            {
                foreach (Question question in QuestionSystem.Instance.GetAllQuestions())
                {
                    question.playerAnswer = "";
                    question.isAnswered = false;
                }
                
                // 清空答题记录
                QuestionSystem.Instance.answerSheet.Clear();
                QuestionSystem.Instance.answerRecords.Clear();
            }
        }
        
        public void ContinueGame()
        {
            if (saveSystem != null && saveSystem.HasSaveFile())
            {
                LoadGameData();
                
                // 加载对应的场景
                string sceneName = GetSceneName(currentStage);
                LoadScene(sceneName);
            }
        }
        
        public void CompleteStage(int stageNumber)
        {
            if (stageNumber > currentStage)
            {
                currentStage = stageNumber;
            }
            
            // 更新进度
            UpdateProgress();
            
            // 保存游戏数据
            SaveGameData();
        }
        
        public void UpdateProgress(float progress = -1f)
        {
            if (progress >= 0f)
            {
                totalProgress = progress;
            }
            else
            {
                // 自动计算进度
                if (QuestionSystem.Instance != null)
                {
                    int answeredQuestions = 0;
                    int totalQuestions = QuestionSystem.Instance.GetAllQuestions().Count;
                    
                    foreach (Question question in QuestionSystem.Instance.GetAllQuestions())
                    {
                        if (question.isAnswered)
                            answeredQuestions++;
                    }
                    
                    totalProgress = totalQuestions > 0 ? (float)answeredQuestions / totalQuestions : 0f;
                }
            }
        }
        
        public void LoadScene(string sceneName)
        {
            if (!string.IsNullOrEmpty(sceneName))
            {
                // 简单消息提示（可选）
                if (uiSystem != null)
                {
                    uiSystem.ShowMessage($"正在切换到 {sceneName}...");
                }
                
                // 在切换前先保存当前场景的背包数据（确保离开前Stage的背包被记录）
                SaveGameData();
                
                StartCoroutine(LoadSceneAsync(sceneName));
            }
        }
        
        private IEnumerator LoadSceneAsync(string sceneName)
        {
            // 简单的延迟（可选）
            yield return new WaitForSeconds(0.2f);
            
            // 异步加载场景
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            
            // 等待场景完全加载后再恢复位置
            yield return new WaitForSeconds(0.1f);
            
            // 如果是加载存档，恢复玩家和相机位置
            if (isLoadingFromSave)
            {
                RestorePlayerAndCameraPosition();
                isLoadingFromSave = false; // 重置标志
            }
            else
            {
                // 安全兜底：非读档场景切换后，强制将玩家放置到当前关卡的出生点
                ForcePlayerToSceneSpawn();
            }
            
            // 保存当前场景信息
            SaveGameData();
        }

        /// <summary>
        /// 非读档切换场景时，将玩家放置到该场景控制器定义的出生点
        /// </summary>
        private void ForcePlayerToSceneSpawn()
        {
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player == null)
            {
                return;
            }

            // 依次尝试各关卡控制器的出生点
            var s5 = FindObjectOfType<Stage5Controller>();
            if (s5 != null && s5.playerSpawnPoint != null)
            {
                player.transform.position = s5.playerSpawnPoint.position;
                player.RefreshInteractionDetection();
                return;
            }

            var s4 = FindObjectOfType<Stage4Controller>();
            if (s4 != null && s4.playerSpawnPoint != null)
            {
                player.transform.position = s4.playerSpawnPoint.position;
                player.RefreshInteractionDetection();
                return;
            }

            var s3 = FindObjectOfType<Stage3Controller>();
            if (s3 != null && s3.playerSpawnPoint != null)
            {
                player.transform.position = s3.playerSpawnPoint.position;
                player.RefreshInteractionDetection();
                return;
            }

            var s2 = FindObjectOfType<Stage2Controller>();
            if (s2 != null && s2.playerSpawnPoint != null)
            {
                player.transform.position = s2.playerSpawnPoint.position;
                player.RefreshInteractionDetection();
                return;
            }

            // Stage1无固定出生点：忽略
        }
        
        private string GetSceneName(int stageNumber)
        {
            if (stageNumber >= 0 && stageNumber < sceneNames.Length)
            {
                return sceneNames[stageNumber];
            }
            return "Stage0";
        }
        
        public void SaveGameData()
        {
            if (saveSystem != null)
            {
                // 检查当前场景，SampleScene和Stage0不自动存档
                string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                if (currentScene == "SampleScene" || currentScene == "Stage0")
                {
                    return;
                }
                
                saveSystem.SaveGame();
            }
        }
        
        public void LoadGameData()
        {
            if (saveSystem != null)
            {
                saveSystem.LoadGame();
            }
        }
        
        /// <summary>
        /// 设置加载存档标志和数据（供SaveSystem调用）
        /// </summary>
        public void SetLoadFromSave(PlayerPositionData playerPos, CameraPositionData cameraPos, Dictionary<int, List<SerializableItem>> stageInventories)
        {
            isLoadingFromSave = true;
            savedPlayerPosition = playerPos;
            savedCameraPosition = cameraPos;
            savedStageInventories = stageInventories;
        }
        
        /// <summary>
        /// 恢复玩家和相机位置及背包数据
        /// </summary>
        private void RestorePlayerAndCameraPosition()
        {
            // 恢复玩家位置
            if (savedPlayerPosition != null)
            {
                PlayerController player = FindObjectOfType<PlayerController>();
                if (player != null)
                {
                    player.transform.position = savedPlayerPosition.ToVector3();
                }
            }
            
            // 恢复相机位置
            if (savedCameraPosition != null)
            {
                Camera mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    Vector3 cameraPos = savedCameraPosition.ToPosition();
                    // 修复相机z坐标问题，确保z坐标为-10
                    if (cameraPos.z == 0)
                    {
                        cameraPos.z = -10f;
                    }
                    mainCamera.transform.position = cameraPos;
                    mainCamera.transform.rotation = savedCameraPosition.ToRotation();
                }
            }
            
            // 恢复当前Stage的背包数据
            if (savedStageInventories != null && savedStageInventories.ContainsKey(currentStage))
            {
                RestoreInventoryForCurrentStage();
            }
            
            // 恢复Stage5数据
            if (savedStageData != null && currentStage == 5)
            {
                RestoreStage5Data();
            }
        }
        
        /// <summary>
        /// 恢复当前Stage的背包数据
        /// </summary>
        private void RestoreInventoryForCurrentStage()
        {
            if (InventorySystem.Instance == null)
            {
                return;
            }
            
            if (savedStageInventories == null || !savedStageInventories.ContainsKey(currentStage))
            {
                return;
            }
            
            List<SerializableItem> stageItems = savedStageInventories[currentStage];
            Item[] itemsToLoad = new Item[stageItems.Count];
            
            for (int i = 0; i < stageItems.Count; i++)
            {
                itemsToLoad[i] = stageItems[i].ToItem();
            }
            
            InventorySystem.Instance.SetAllItems(itemsToLoad);
        }
        
        /// <summary>
        /// 恢复Stage5数据
        /// </summary>
        private void RestoreStage5Data()
        {
            Stage5Controller stage5 = FindObjectOfType<Stage5Controller>();
            if (stage5 != null && savedStageData != null)
            {
                // 恢复属性值
                stage5.LoadAttributes(savedStageData.goodEvilValue, savedStageData.truthValue, 
                                    savedStageData.loveDesireValue, savedStageData.money);
                
                // 恢复布尔状态
                stage5.canExitScene = savedStageData.canExitScene;
                stage5.hasJi = savedStageData.hasJi;
                stage5.hasMie = savedStageData.hasMie;
                stage5.hasKu = savedStageData.hasKu;
                stage5.canUseMJXW = savedStageData.canUseMJXW;
                stage5.canTalkWithQika = savedStageData.canTalkWithQika;
                stage5.isCallingPrincess = savedStageData.isCallingPrincess;
                // 恢复是否第一次进入标记，避免重复触发首次对话
                stage5.SetIsFirstTimeEntry(savedStageData.isFirstTimeEntry);
                
                // 恢复计数器和特殊状态
                stage5.SetMJXWUsageCount(savedStageData.mjxwUsageCount);
                stage5.SetHasReceivedFreePickaxe(savedStageData.hasReceivedFreePickaxe);
                
                // 恢复NPC交互状态（从列表转换为字典）
                var interactionDict = new Dictionary<string, bool>();
                if (savedStageData.npcInteractionStatus != null)
                {
                    foreach (var entry in savedStageData.npcInteractionStatus)
                    {
                        if (entry != null && !string.IsNullOrEmpty(entry.npcId))
                        {
                            interactionDict[entry.npcId] = entry.hasInteracted;
                        }
                    }
                }
                stage5.SetNPCInteractionStatus(interactionDict);

                // 恢复NPC名称变更（从列表转换为字典）
                var nameChangeDict = new Dictionary<string, string>();
                if (savedStageData.npcNameChanges != null)
                {
                    foreach (var entry in savedStageData.npcNameChanges)
                    {
                        if (entry != null && !string.IsNullOrEmpty(entry.originalName))
                        {
                            nameChangeDict[entry.originalName] = entry.newName ?? string.Empty;
                        }
                    }
                }
                stage5.SetNPCNameChanges(nameChangeDict);
            }
        }
        
        public void ExportAnswerSheet()
        {
            if (QuestionSystem.Instance != null)
            {
                QuestionSystem.Instance.ExportAnswerSheet();
            }
        }
        
        /// <summary>
        /// 导出Markdown格式的完整存档（供Stage0Controller使用）
        /// </summary>
        /// <returns>导出结果信息</returns>
        public ExportResult ExportMarkdownSave()
        {
            if (saveSystem != null)
            {
                ExportResult result = saveSystem.ExportToMarkdown();
                return result;
            }
            else
            {
                return new ExportResult
                {
                    Success = false,
                    FilePath = "",
                    FileName = "",
                    Message = "SaveSystem未找到，无法导出存档"
                };
            }
        }
        
        public void ExitGame()
        {
            // 保存游戏数据
            SaveGameData();
            
            // 退出游戏
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
        
        // 获取当前Stage的完成状态
        public bool IsStageCompleted(int stageNumber)
        {
            if (QuestionSystem.Instance != null)
            {
                return QuestionSystem.Instance.IsStageCompleted(stageNumber);
            }
            return false;
        }
        
        // 获取总体游戏进度
        public float GetOverallProgress()
        {
            if (QuestionSystem.Instance != null)
            {
                int answeredQuestions = 0;
                foreach (Question question in QuestionSystem.Instance.GetAllQuestions())
                {
                    if (question.isAnswered)
                        answeredQuestions++;
                }
                return (float)answeredQuestions / QuestionSystem.Instance.GetAllQuestions().Count;
            }
            return 0f;
        }
        
        public void ResetGame()
        {
            // 重置游戏状态
            isGameStarted = false;
            currentStage = 0;
            totalProgress = 0f;
            playerName = "";
            
            // 重置题目状态
            if (QuestionSystem.Instance != null)
            {
                foreach (Question question in QuestionSystem.Instance.GetAllQuestions())
                {
                    question.playerAnswer = "";
                    question.isAnswered = false;
                }
                QuestionSystem.Instance.answerSheet.Clear();
                QuestionSystem.Instance.answerRecords.Clear();
            }
        }
        
        private void Update()
        {
            // 保留基本的帮助快捷键
            if (Input.GetKeyDown(KeyCode.F1))
            {
                // 显示帮助信息
                ShowHelpInfo();
            }
        }
        
        private void ShowHelpInfo()
        {
            string helpText = "=== 玄之试炼 操作说明 ===\n" +
                             "WASD: 移动\n" +
                             "E: 交互\n" +
                             "Enter: 确认对话\n" +
                             "ESC: 返回主菜单\n" +
                             "Tab: 显示进度\n" +
                             "F1: 显示帮助\n" +
                             "Ctrl+S: 保存创作(Stage6)";
            
        }
        
        /// <summary>
        /// 处理ESC键确认返回主菜单
        /// </summary>
        public void HandleEscapeKey()
        {
            // 检查是否有对话正在进行，如果有则不处理ESC
            if (DialogueSystem.Instance != null && DialogueSystem.Instance.isDialogueActive)
            {
                return;
            }
            // 额外保护：如有Stage5过场动画在播放，也不弹确认
            var stage5 = FindObjectOfType<XuanZhiShiLian.Stage5Controller>();
            if (stage5 != null && stage5.transitionAnimator != null)
            {
                var st = stage5.transitionAnimator.GetCurrentAnimatorStateInfo(0);
                if (st.length > 0f && st.normalizedTime < 1f)
                {
                    return;
                }
            }
            
            // 创建确认选项
            List<DialogueOption> exitOptions = new List<DialogueOption>
            {
                DialogueSystem.CreateOption("是", () => ConfirmReturnToTitle()),
                DialogueSystem.CreateOption("否", () => CancelReturnToTitle())
            };
            
            // 创建确认对话
            List<DialogueData> confirmationDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateChoiceDialogue("", "确定要返回到标题界面吗？当前进度将会被保存。", exitOptions)
            };
            
            // 显示确认对话
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(confirmationDialogue);
            }
        }
        
        /// <summary>
        /// 确认返回标题界面
        /// </summary>
        private void ConfirmReturnToTitle()
        {
            // 保存游戏数据
            SaveGameData();
            // 返回标题界面
            LoadScene("Stage0");
        }
        
        /// <summary>
        /// 取消返回标题界面
        /// </summary>
        private void CancelReturnToTitle()
        {
            // 什么都不做，对话结束后游戏继续
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // 游戏暂停时自动保存
                SaveGameData();
            }
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                // 失去焦点时自动保存
                SaveGameData();
            }
        }
        
        private void OnDestroy()
        {
            // 销毁时保存游戏数据
            if (Instance == this)
            {
                SaveGameData();
            }
        }
    }
}
