using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XuanZhiShiLian
{
    public class Stage2Controller : MonoBehaviour
    {
        [Header("场景设置")]
        public Transform playerSpawnPoint;
        
        [Header("玩家位置点")]
        public Transform homePlayerPosition; // 玩家在家中的位置
        public Transform graveyardPlayerPosition; // 玩家在墓园的位置
        
        [Header("玩家控制")]
        public GameObject playerPrefab;
        public PlayerController playerController;
        
        [Header("交互对象")]
        public QuestionInteractable mirrorInteractable;
        public QuestionInteractable wardrobeInteractable;
        public QuestionInteractable fridgeInteractable;
        public QuestionInteractable parentsInteractable;
        
        [Header("墓园交互对象")]
        [Tooltip("有题目的坟墓（最多3个，对应题目6-8）")]
        public QuestionInteractable[] graveyardQuestionInteractables;
        [Tooltip("纯对话的坟墓")]
        public DialogueInteractable[] graveyardDialogueInteractables;
        
        [Header("场景切换")]
        [Tooltip("从家到墓园的梯子（场景内切换）")]
        public SceneTransitionInteractable homeToGraveyardLadder;
        [Tooltip("从墓园到家的梯子（场景内切换）")]
        public SceneTransitionInteractable graveyardToHomeLadder;
        [Tooltip("从墓园离开到Stage3的梯子（场景切换）")]
        public SceneTransitionInteractable exitLadder;
        
        // 公共访问器，供其他类使用
        public SceneTransitionInteractable HomeToGraveyardLadder => homeToGraveyardLadder;
        public SceneTransitionInteractable GraveyardToHomeLadder => graveyardToHomeLadder;
        
        private bool isInHome = true;
        private int completedQuestions = 0;
        private int totalQuestions = 7; // 题目2-8
        
        private void Start()
        {
            InitializeStage();
            StartStageSequence();
        }
        
        private void InitializeStage()
        {
            // 更新当前阶段
            if (GameManager.Instance != null)
                GameManager.Instance.currentStage = 2;
            
            // 初始化玩家
            InitializePlayer();
            
            // 设置交互对象
            SetupInteractables();
            
            // 检查已完成的题目
            CheckCompletedQuestions();

            // 从存档恢复区域状态（在家/墓园）并定位
            RestoreStage2RegionFromSave();
        }

        private void RestoreStage2RegionFromSave()
        {
            var save = GameManager.Instance != null ? GameManager.Instance.saveSystem?.GetSaveData() : null;
            if (save == null || save.stageData == null || save.stageData.stage2 == null)
            {
                return;
            }
            isInHome = save.stageData.stage2.isInHome;

            // 根据区域设置玩家位置到对应锚点
            if (playerController != null)
            {
                Transform target = isInHome ? (homePlayerPosition ?? playerSpawnPoint) : (graveyardPlayerPosition ?? playerSpawnPoint);
                if (target != null)
                {
                    playerController.transform.position = target.position;
                    playerController.RefreshInteractionDetection();
                }
            }
        }
        
        private void InitializePlayer()
        {
            // 查找现有的玩家控制器
            if (playerController == null)
            {
                playerController = FindObjectOfType<PlayerController>();
            }
            
            // 如果没有找到且有预制件，则创建新的
            if (playerController == null && playerPrefab != null && playerSpawnPoint != null)
            {
                GameObject playerGO = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity);
                playerController = playerGO.GetComponent<PlayerController>();
            }
            
            // 设置玩家位置（初始在家中）
            if (playerController != null)
            {
                Vector3 startPosition = playerSpawnPoint != null ? playerSpawnPoint.position : 
                                      (homePlayerPosition != null ? homePlayerPosition.position : Vector3.zero);
                playerController.transform.position = startPosition;
                // 刷新交互检测
                playerController.RefreshInteractionDetection();
            }
        }
        
        private void SetupInteractables()
        {
            // 设置家中的交互对象题目ID
            if (mirrorInteractable != null)
                SetupQuestionInteractable(mirrorInteractable, 2, "按E照镜子");
            if (wardrobeInteractable != null)
                SetupQuestionInteractable(wardrobeInteractable, 3, "按E查看衣柜");
            if (fridgeInteractable != null)
                SetupQuestionInteractable(fridgeInteractable, 4, "按E打开冰箱");
            if (parentsInteractable != null)
                SetupQuestionInteractable(parentsInteractable, 5, "按E检查影子");
                
            // 设置墓园的交互对象
            if (graveyardQuestionInteractables != null)
            {
                for (int i = 0; i < graveyardQuestionInteractables.Length && i < 3; i++)
                {
                    if (graveyardQuestionInteractables[i] != null)
                    {
                        int questionId = 6 + i; // 题目6-8
                        string[] graveyardHints = {
                            "按E检查坟墓",
                            "按E检查坟墓",
                            "按E检查坟墓"
                        };
                        SetupQuestionInteractable(graveyardQuestionInteractables[i], questionId, graveyardHints[i]);
                    }
                }
            }

            // 设置墓园的对话交互对象
            if (graveyardDialogueInteractables != null)
            {
                for (int i = 0; i < graveyardDialogueInteractables.Length; i++)
                {
                    if (graveyardDialogueInteractables[i] != null)
                    {
                        SetupDialogueInteractable(graveyardDialogueInteractables[i]);
                    }
                }
            }
            
            // 设置场景切换
            if (homeToGraveyardLadder != null)
            {
                // 这个梯子用于在家和墓园之间切换（场景内切换）
                homeToGraveyardLadder.targetSceneName = ""; // 不切换场景，只是区域切换
                homeToGraveyardLadder.requiresAllQuestionsAnswered = false;
                homeToGraveyardLadder.interactionText = "按E前往墓园";
            }
            
            if (graveyardToHomeLadder != null)
            {
                // 这个梯子用于在家和墓园之间切换（场景内切换）
                graveyardToHomeLadder.targetSceneName = ""; // 不切换场景，只是区域切换
                graveyardToHomeLadder.requiresAllQuestionsAnswered = false;
                graveyardToHomeLadder.interactionText = "按E返回家中";
            }
            
            if (exitLadder != null)
            {
                // 这个梯子用于离开Stage2，前往Stage3
                exitLadder.targetSceneName = "Stage3";
                exitLadder.requiresAllQuestionsAnswered = false; // 允许不答完就走
                exitLadder.interactionText = "按E离开此地，前往下一关";
            }
        }
        
        private void SetupQuestionInteractable(QuestionInteractable interactable, int questionId, string interactionText)
        {
            interactable.questionId = questionId;
            interactable.interactionText = interactionText;
            
            // 刷新题目状态
            interactable.RefreshQuestionState();
        }

        private void SetupDialogueInteractable(DialogueInteractable interactable)
        {
            interactable.interactionText = "按E与坟墓对话";
            interactable.RefreshInteractionState();
        }
        
        private void CheckCompletedQuestions()
        {
            completedQuestions = 0;
            if (QuestionSystem.Instance != null)
            {
                // 检查题目2-8的完成状态
                for (int i = 2; i <= 8; i++)
                {
                    Question question = QuestionSystem.Instance.GetQuestionById(i);
                    if (question != null && question.isAnswered)
                    {
                        completedQuestions++;
                    }
                }
            }
        }
        
        private void StartStageSequence()
        {
            StartCoroutine(PlayStageSequence());
        }
        
        private IEnumerator PlayStageSequence()
        {
            // 等待系统准备就绪
            yield return new WaitForSeconds(0.5f);
            
            // 家中的开场对话
            if (DialogueSystem.Instance != null)
            {
                List<DialogueData> homeDialogue = new List<DialogueData>
                {
                    DialogueSystem.CreateSceneDescription("场景：家"),
                    DialogueSystem.CreateSceneDescription("从床上醒来"),
                    DialogueSystem.CreateSubtitle("我", "早上好，我自己"),
                    DialogueSystem.CreateSubtitle("我", "今天我要参加一场重要的考试，该抓紧时间了。")
                };
                
                DialogueSystem.Instance.StartDialogue(homeDialogue);
                yield return new WaitUntil(() => !DialogueSystem.Instance.isDialogueActive);
            }
            
            // 显示交互提示
            ShowInteractionHints();
        }
        
        private void ShowInteractionHints()
        {
            if (DialogueSystem.Instance != null)
            {
                List<DialogueData> hints = new List<DialogueData>
                {
                    DialogueSystem.CreateInteractionHint("使用WASD移动，E键与物品交互", 3f),
                };
                
                DialogueSystem.Instance.StartDialogue(hints);
            }
        }
        
        public void OnQuestionAnswered(int questionId)
        {
            CheckCompletedQuestions();
        }
        
        /// <summary>
        /// 处理纯对话坟墓的对话完成事件
        /// </summary>
        public void OnDialogueCompleted(string interactableName)
        {
            
        }
        
        private bool IsHomeCompleted()
        {
            if (QuestionSystem.Instance == null) return false;
            
            for (int i = 2; i <= 5; i++)
            {
                Question question = QuestionSystem.Instance.GetQuestionById(i);
                if (question == null || !question.isAnswered)
                    return false;
            }
            return true;
        }
        
        public void OnEnterGraveyard()
        {
            if (isInHome)
            {
                isInHome = false;
                StartCoroutine(PlayGraveyardSequence());
            }
        }
        
        /// <summary>
        /// 从墓园返回家时调用
        /// </summary>
        public void OnReturnHome()
        {
            if (!isInHome)
            {
                isInHome = true;
            }
        }
        
        private IEnumerator PlayGraveyardSequence()
        {
            if (DialogueSystem.Instance != null)
            {
                List<DialogueData> graveyardDialogue = new List<DialogueData>
                {
                    DialogueSystem.CreateSceneDescription("出门从坟墓中爬出来，旁边有其他的坟墓，墓园门口同样是梯子"),
                    DialogueSystem.CreateSubtitle("我", "考试的话，应该去学校，但是我才活过来，应该看看那些还没活的邻居再走。")
                };
                
                DialogueSystem.Instance.StartDialogue(graveyardDialogue);
                yield return new WaitUntil(() => !DialogueSystem.Instance.isDialogueActive);
            }
            
            // 显示墓园交互提示
            ShowGraveyardHints();
        }
        
        private void ShowGraveyardHints()
        {
            if (DialogueSystem.Instance != null)
            {
                List<DialogueData> hints = new List<DialogueData>
                {
                    DialogueSystem.CreateInteractionHint("与各个坟墓交互可以答题", 3f),
                    DialogueSystem.CreateInteractionHint("完成题目后可以爬梯子离开墓园", 3f)
                };
                
                DialogueSystem.Instance.StartDialogue(hints);
            }
        }
        
        public void OnExitAttempt()
        {
            ShowExitConfirmationDialogue();
        }
        
        /// <summary>
        /// 显示退出确认对话
        /// </summary>
        private void ShowExitConfirmationDialogue()
        {
            if (DialogueSystem.Instance != null)
            {
                // 创建确认选项
                List<DialogueOption> exitOptions = new List<DialogueOption>
                {
                    DialogueSystem.CreateOption("是", () => ConfirmExit()),
                    DialogueSystem.CreateOption("否", () => CancelExit())
                };
                
                // 创建确认对话
                List<DialogueData> confirmationDialogue = new List<DialogueData>
                {
                    DialogueSystem.CreateChoiceDialogue("", "是否要前往下一层？", exitOptions)
                };
                
                DialogueSystem.Instance.StartDialogue(confirmationDialogue);
            }
        }
        
        /// <summary>
        /// 确认退出，前往下一层
        /// </summary>
        private void ConfirmExit()
        {
            if (DialogueSystem.Instance != null)
            {
                List<DialogueData> exitMessage = new List<DialogueData>();
                
                exitMessage.Add(DialogueSystem.CreateSceneDescription("爬上梯子，离开了温暖的家"));
                
                DialogueSystem.Instance.StartDialogue(exitMessage, () => 
                {
                    // 对话结束后切换场景
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.LoadScene("Stage3");
                    }
                });
            }
            else
            {
                // 如果对话系统不可用，直接切换场景
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.LoadScene("Stage3");
                }
            }
        }
        
        /// <summary>
        /// 取消退出，关闭对话框
        /// </summary>
        private void CancelExit()
        {
            // 关闭对话框，玩家可以继续探索
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.EndDialogue();
            }
        }
        
        private void Update()
        {
            // 按ESC返回主菜单
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                GameManager.Instance.HandleEscapeKey();
            }
        }
        
        private void ShowProgress()
        {
            if (DialogueSystem.Instance != null)
            {
                List<DialogueData> progressInfo = new List<DialogueData>
                {
                    DialogueSystem.CreateInteractionHint($"当前进度: {completedQuestions}/{totalQuestions}道题目已完成", 3f)
                };
                
                DialogueSystem.Instance.StartDialogue(progressInfo);
            }
        }
        
        private void ShowHelp()
        {
            if (DialogueSystem.Instance != null)
            {
                List<DialogueData> helpInfo = new List<DialogueData>
                {
                    DialogueSystem.CreateInteractionHint("WASD：移动", 1f),
                    DialogueSystem.CreateInteractionHint("E：交互", 1f),
                    DialogueSystem.CreateInteractionHint("Tab：查看进度", 1f),
                    DialogueSystem.CreateInteractionHint("ESC：返回主菜单", 2f)
                };
                
                DialogueSystem.Instance.StartDialogue(helpInfo);
            }
        }
    }
} 