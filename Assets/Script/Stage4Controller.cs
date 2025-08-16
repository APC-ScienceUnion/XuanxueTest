using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XuanZhiShiLian
{
    public class Stage4Controller : MonoBehaviour
    {
        [Header("场景设置")]
        public Transform playerSpawnPoint;
        
        [Header("玩家控制")]
        public GameObject playerPrefab;
        public PlayerController playerController;
        
        [Header("谜题交互对象")]
        [Tooltip("左1层楼梯谜题（题目42）")]
        public QuestionInteractable leftStair1Puzzle;
        [Tooltip("右1层楼梯谜题（题目43）")]
        public QuestionInteractable rightStair1Puzzle;
        [Tooltip("左2层谜题组合（题目44）")]
        public QuestionInteractable leftFloor2Puzzle;
        [Tooltip("右2层谜题组合（题目45）")]
        public QuestionInteractable rightFloor2Puzzle;
        [Tooltip("左3层最终谜题（题目46）")]
        public QuestionInteractable leftFloor3Puzzle;
        [Tooltip("右3层最终谜题（题目47）")]
        public QuestionInteractable rightFloor3Puzzle;
        
        [Header("NPC交互对象")]
        [Tooltip("左1层门卫")]
        public DialogueInteractable leftFloor1Guard;
        [Tooltip("右1层保安")]
        public DialogueInteractable rightFloor1Security;
        [Tooltip("左2层教授")]
        public DialogueInteractable leftFloor2Professor;
        [Tooltip("右2层总工")]
        public DialogueInteractable rightFloor2Engineer;
        [Tooltip("左3层校长")]
        public DialogueInteractable leftFloor3Principal;
        [Tooltip("右3层老板")]
        public DialogueInteractable rightFloor3Boss;
        
        [Header("楼梯切换")]
        [Tooltip("左侧1层到2层楼梯")]
        public SceneTransitionInteractable leftStair1To2;
        [Tooltip("右侧1层到2层楼梯")]
        public SceneTransitionInteractable rightStair1To2;
        [Tooltip("左侧2层到3层楼梯")]
        public SceneTransitionInteractable leftStair2To3;
        [Tooltip("右侧2层到3层楼梯")]
        public SceneTransitionInteractable rightStair2To3;
        [Tooltip("左侧3层到Stage5楼梯")]
        public SceneTransitionInteractable leftStair3ToStage5;
        [Tooltip("右侧3层到Stage5楼梯")]
        public SceneTransitionInteractable rightStair3ToStage5;
        
        [Header("洞口切换")]
        [Tooltip("2层洞口（跳到1层）")]
        public SceneTransitionInteractable floor2Hole;
        [Tooltip("3层洞口（跳到1层，但有特殊逻辑）")]
        public SceneTransitionInteractable floor3Hole;
        [Tooltip("3层洞口的隐藏出口逻辑")]
        public HiddenExitInteractable hiddenExit;
        
        // 三楼洞口交互计数
        private int floor3HoleInteractionCount = 0;
        
        // 钥匙系统
        private Dictionary<string, bool> keys = new Dictionary<string, bool>();
        private bool hasAnsweredFillInBlank = false; // 是否已回答填空题
        
        // 谜题完成状态跟踪
        private Dictionary<int, bool> puzzleCompleted = new Dictionary<int, bool>();
        private int completedQuestions = 0;
        private int totalQuestions = 7; // 题目41-47
        
        private void Start()
        {
            InitializeStage();
            StartStageSequence();
        }
        
        private void InitializeStage()
        {
            // 更新当前阶段
            if (GameManager.Instance != null)
                GameManager.Instance.currentStage = 4;
            
            // 初始化玩家
            InitializePlayer();
            
            // 初始化钥匙系统
            InitializeKeys();
            
            // 设置交互对象
            SetupInteractables();
            
            // 检查已完成的题目
            CheckCompletedQuestions();

            // 基于已作答或存档恢复钥匙与交互状态（避免从其他关卡返回后被重置）
            RestoreStage4State();
            
            // 确保背包系统已初始化并显示
            InitializeInventorySystem();

            // 防御性解锁：若对话被锁定而没有活动面板，则强制解锁，避免阻塞交互
            if (DialogueSystem.Instance != null && !DialogueSystem.Instance.isDialogueActive && DialogueSystem.IsDialogueLocked)
            {
                Debug.LogWarning("[Stage4] 检测到对话锁残留，执行解锁");
                DialogueSystem.Instance.EndDialogue();
            }
        }
        
        /// <summary>
        /// 初始化背包系统
        /// </summary>
        private void InitializeInventorySystem()
        {
            if (InventorySystem.Instance != null)
            {
                // Stage4需要显示背包
                InventorySystem.Instance.InitializeInventoryUI();
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
            
            // 设置玩家位置
            if (playerController != null && playerSpawnPoint != null)
            {
                playerController.transform.position = playerSpawnPoint.position;
                playerController.RefreshInteractionDetection();
            }
        }
        
        private void InitializeKeys()
        {
            // 初始化6把钥匙的状态
            keys["right_stair_key"] = false; // 右侧楼梯钥匙（题目42奖励）
            keys["left_stair_key"] = false;  // 左侧楼梯钥匙（题目43奖励）
            keys["right_floor2_key"] = false; // 右侧2层钥匙（题目44奖励）
            keys["left_floor2_key"] = false;  // 左侧2层钥匙（题目45奖励）
            keys["right_floor3_key"] = false; // 右侧3层钥匙（题目46奖励）
            keys["left_floor3_key"] = false;  // 左侧3层钥匙（题目47奖励）
        }
        
        private void SetupInteractables()
        {
            // 设置谜题交互对象
            if (leftStair1Puzzle != null) SetupQuestionInteractable(leftStair1Puzzle, 42, "按E解谜 - 猪舍密码");
            if (rightStair1Puzzle != null) SetupQuestionInteractable(rightStair1Puzzle, 43, "按E解谜 - base64加密");
            if (leftFloor2Puzzle != null) SetupQuestionInteractable(leftFloor2Puzzle, 44, "按E解谜 - 左2层谜题组合");
            if (rightFloor2Puzzle != null) SetupQuestionInteractable(rightFloor2Puzzle, 45, "按E解谜 - 右2层谜题组合");
            if (leftFloor3Puzzle != null) SetupQuestionInteractable(leftFloor3Puzzle, 46, "按E解谜 - 左3层最终谜题");
            if (rightFloor3Puzzle != null) SetupQuestionInteractable(rightFloor3Puzzle, 47, "按E解谜 - 右3层最终谜题");
            
            // 设置NPC对话交互对象
            if (leftFloor1Guard != null) SetupNPCInteractable(leftFloor1Guard, "左1层门卫");
            if (rightFloor1Security != null) SetupNPCInteractable(rightFloor1Security, "右1层保安");
            if (leftFloor2Professor != null) SetupNPCInteractable(leftFloor2Professor, "左2层教授");
            if (rightFloor2Engineer != null) SetupNPCInteractable(rightFloor2Engineer, "右2层总工");
            if (leftFloor3Principal != null) SetupNPCInteractable(leftFloor3Principal, "左3层校长");
            if (rightFloor3Boss != null) SetupNPCInteractable(rightFloor3Boss, "右3层老板");
            
            // 设置楼层切换交互对象（楼梯、洞口等）
            // SceneTransitionInteractable会根据GameObject名称自动判断钥匙要求
            if (leftStair1To2 != null)
            {
                leftStair1To2.targetSceneName = ""; // 场景内切换
            }
            if (rightStair1To2 != null)
            {
                rightStair1To2.targetSceneName = ""; // 场景内切换
            }
            if (leftStair2To3 != null)
            {
                leftStair2To3.targetSceneName = ""; // 场景内切换
            }
            if (rightStair2To3 != null)
            {
                rightStair2To3.targetSceneName = ""; // 场景内切换
            }
            if (leftStair3ToStage5 != null)
            {
                leftStair3ToStage5.targetSceneName = "Stage5"; // 跳转到Stage5
            }
            if (rightStair3ToStage5 != null)
            {
                rightStair3ToStage5.targetSceneName = "Stage5"; // 跳转到Stage5
            }
            
            // 设置2层洞口
            if (floor2Hole != null)
            {
                floor2Hole.targetSceneName = ""; // 场景内切换
                floor2Hole.interactionText = "按E跳下洞口";
            }
            
            // 设置3层洞口
            if (floor3Hole != null)
            {
                floor3Hole.targetSceneName = ""; // 场景内切换  
                floor3Hole.interactionText = "按E与洞口交互";
            }
            
            // 设置隐藏出口（3层洞口）
            if (hiddenExit != null)
            {
                hiddenExit.stage4Controller = this;
                // 3层洞口始终存在，不需要隐藏
                hiddenExit.gameObject.SetActive(true);
            }
            
            // 初始化楼梯的交互文本
            RefreshStairInteractionTexts();
        }

        /// <summary>
        /// 依据已作答且正确的题目恢复钥匙与相关交互体状态
        /// 避免从Stage6等场景返回Stage4后钥匙被重置
        /// </summary>
        private void RestoreStage4State()
        {
            // 优先从存档恢复
            var save = GameManager.Instance != null ? GameManager.Instance.saveSystem?.GetSaveData() : null;
            bool restoredFromSave = false;
            if (save != null && save.stageData != null && save.stageData.stage4 != null)
            {
                hasAnsweredFillInBlank = save.stageData.stage4.hasAnsweredFillInBlank;
                keys["right_stair_key"] = save.stageData.stage4.right_stair_key;
                keys["left_stair_key"] = save.stageData.stage4.left_stair_key;
                keys["right_floor2_key"] = save.stageData.stage4.right_floor2_key;
                keys["left_floor2_key"] = save.stageData.stage4.left_floor2_key;
                keys["right_floor3_key"] = save.stageData.stage4.right_floor3_key;
                keys["left_floor3_key"] = save.stageData.stage4.left_floor3_key;
                restoredFromSave = true;
            }

            // 若存档缺失，则回退到依据作答记录恢复
            if (!restoredFromSave && QuestionSystem.Instance != null)
            {
                // 恢复题目41（填空题）状态
                Question q41 = QuestionSystem.Instance.GetQuestionById(41);
                hasAnsweredFillInBlank = q41 != null && q41.isAnswered;

                // 恢复题目42-47对应钥匙（仅在答案正确时给予）
                for (int questionId = 42; questionId <= 47; questionId++)
                {
                    Question q = QuestionSystem.Instance.GetQuestionById(questionId);
                    if (q != null && q.isAnswered && CheckAnswerCorrectness(questionId))
                    {
                        string keyName = GetKeyReward(questionId);
                        if (!string.IsNullOrEmpty(keyName) && keys.ContainsKey(keyName))
                        {
                            keys[keyName] = true;
                            puzzleCompleted[questionId] = true;
                            // 刷新交互体隐藏/禁用状态
                            RefreshQuestionInteractableState(questionId);
                        }
                    }
                }
            }

            // 刷新楼梯交互文本（显示“前往Stage5”或默认提示）
            RefreshStairInteractionTexts();
        }
        
        private void SetupQuestionInteractable(QuestionInteractable interactable, int questionId, string interactionText)
        {
            interactable.questionId = questionId;
            interactable.interactionText = interactionText;
            interactable.RefreshQuestionState();
        }
        
        private void SetupNPCInteractable(DialogueInteractable interactable, string npcName)
        {
            interactable.interactionText = $"按E与{npcName}对话";
            
            // 设置NPC的名字
            string[] nameParts = npcName.Split(new char[]{'层'}, System.StringSplitOptions.RemoveEmptyEntries);
            if (nameParts.Length >= 2)
            {
                interactable.speakerName = nameParts[1]; // 例如："左1层门卫" -> "门卫"
            }
            else
            {
                interactable.speakerName = npcName;
            }
            
            // 根据NPC设置对话内容（从requirement.md中的内容）
            SetupNPCDialogue(interactable, npcName);
            
            // NPC对话内容应该在DialogueInteractable中配置
            interactable.RefreshInteractionState();
        }
        
        private void SetupNPCDialogue(DialogueInteractable interactable, string npcName)
        {
            // 根据requirement.md设置各NPC的对话内容
            switch (npcName)
            {
                case "左1层门卫":
                    interactable.dialogueLines = new string[]
                    {
                        "录取通知书只是流程，毕业也是，用纸换纸，以钱换钱。"
                    };
                    break;
                    
                case "右1层保安":
                    interactable.dialogueLines = new string[]
                    {
                        "无论如何，物质建设是精神建设的基础，世界是物质的。"
                    };
                    break;
                    
                case "左2层教授":
                    interactable.dialogueLines = new string[]
                    {
                        "读了书，但书是什么呢？知识本身就是智慧吗？我想不是。"
                    };
                    break;
                    
                case "右2层总工":
                    interactable.dialogueLines = new string[]
                    {
                        "站得高，望的远，但越是高，越是危险。终归要一技傍身。"
                    };
                    break;
                    
                case "左3层校长":
                    interactable.dialogueLines = new string[]
                    {
                        "从此门上去，不要和由右门上去一样，那样你就白走左边了。"
                    };
                    break;
                    
                case "右3层老板":
                    interactable.dialogueLines = new string[]
                    {
                        "从此门上去，无论从哪边上来都一样，你往上走就待不了这。"
                    };
                    break;
                    
                default:
                    interactable.dialogueLines = new string[]
                    {
                        "你好，我是" + interactable.speakerName + "。"
                    };
                    break;
            }
        }
        
        private void CheckCompletedQuestions()
        {
            completedQuestions = 0;
            if (QuestionSystem.Instance != null)
            {
                // 检查题目41的完成状态（填空题）
                Question fillInBlankQuestion = QuestionSystem.Instance.GetQuestionById(41);
                if (fillInBlankQuestion != null && fillInBlankQuestion.isAnswered)
                {
                    hasAnsweredFillInBlank = true;
                    completedQuestions++;
                }
                
                // 检查题目42-47的完成状态
                for (int i = 42; i <= 47; i++)
                {
                    Question question = QuestionSystem.Instance.GetQuestionById(i);
                    if (question != null && question.isAnswered)
                    {
                        completedQuestions++;
                        puzzleCompleted[i] = true;
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
            
            // 先播放开场对话
            if (DialogueSystem.Instance != null)
            {
                List<DialogueData> introDialogue = new List<DialogueData>
                {
                    DialogueSystem.CreateSceneDescription("这是一栋三层高的楼，左半侧是建好的建筑，右半侧是工地"),
                    DialogueSystem.CreateSceneDescription("除第一层外每层中间都有一个洞，左右两侧则是可以上楼的通道"),
                    DialogueSystem.CreateInteractionHint("解开谜题获得钥匙，用钥匙开启通道", 3f),
                };
                
                DialogueSystem.Instance.StartDialogue(introDialogue);
                yield return new WaitUntil(() => !DialogueSystem.Instance.isDialogueActive);
            }
            
            // 开场对话结束后，如果还没回答填空题，则显示填空题
            if (!hasAnsweredFillInBlank)
            {
                yield return StartCoroutine(ShowFillInBlankQuestion());
            }
        }
        
        private IEnumerator ShowFillInBlankQuestion()
        {
            // 显示题目41（填空题）
            if (QuestionSystem.Instance != null)
            {
                Question fillInBlankQuestion = QuestionSystem.Instance.GetQuestionById(41);
                if (fillInBlankQuestion != null && !fillInBlankQuestion.isAnswered)
                {
                    QuestionSystem.Instance.ShowQuestion(fillInBlankQuestion, OnFillInBlankAnswered);
                    yield return new WaitUntil(() => hasAnsweredFillInBlank);
                }
            }
        }
        
        private void OnFillInBlankAnswered(Question question, string answer)
        {
            if (QuestionSystem.Instance != null)
            {
                QuestionSystem.Instance.AnswerQuestion(question.id, answer);
            }
            
            hasAnsweredFillInBlank = true;
        }
        
        public void OnQuestionAnswered(int questionId)
        {
            // 对于题目46和47，强制检查并同步嵌套子题目状态
            if (questionId == 46 || questionId == 47)
            {
                SyncNestedQuestionStates(questionId);
            }
            
            CheckCompletedQuestions();
            
            // 检查答案是否正确
            bool isCorrect = CheckAnswerCorrectness(questionId);
            
            if (isCorrect)
            {
                // 答案正确才给予钥匙奖励
                string keyReward = GetKeyReward(questionId);
                if (!string.IsNullOrEmpty(keyReward))
                {
                    GiveKey(keyReward);
                    
                    // 刷新对应楼梯的交互文本
                    RefreshStairInteractionTexts();
                }
                
                // 答案正确才给予物品奖励
                HandlePuzzleHints(questionId);
                
                // 刷新对应QuestionInteractable的状态
                RefreshQuestionInteractableState(questionId);
                
                // 显示答题正确提示
                ShowAnswerFeedback(questionId, true);
            }
            else
            {
                // 答案错误时的处理
                ShowAnswerFeedback(questionId, false);
            }
            
            // 检查是否可以解锁隐藏出口
            CheckHiddenExit();
        }
        
        /// <summary>
        /// 检查指定题目的答案是否正确
        /// </summary>
        private bool CheckAnswerCorrectness(int questionId)
        {
            if (QuestionSystem.Instance == null)
            {
                return false;
            }
            
            Question question = QuestionSystem.Instance.GetQuestionById(questionId);
            if (question == null)
            {
                return false;
            }
            
            // 对于组合题目，需要特殊处理isAnswered状态
            if (question.hasSubQuestions && question.subQuestions != null && question.subQuestions.Length > 0)
            {
                // 组合题目：递归检查是否所有子题目都已回答（包括嵌套的子题目）
                bool allSubAnswered = CheckAllSubQuestionsAnswered(question, questionId);
                
                if (!allSubAnswered)
                {
                    return false;
                }
                
                // 强制标记组合题目为已完成，以便后续正确性检查
                if (!question.isAnswered)
                {
                    QuestionSystem.Instance.AnswerQuestion(questionId, "");
                    question.isAnswered = true;
                }
            }
            else
            {
                // 单个题目：检查主题目是否已回答
                if (!question.isAnswered)
                {
                    return false;
                }
            }
            
            if (question.hasSubQuestions && question.subQuestions != null && question.subQuestions.Length > 0)
            {
                bool allSubQuestionsCorrect = true;
                
                for (int i = 0; i < question.subQuestions.Length; i++)
                {
                    Question subQuestion = question.subQuestions[i];
                    
                    if (!subQuestion.isAnswered)
                    {
                        allSubQuestionsCorrect = false;
                        continue; // 跳过未回答的子题目，但继续检查其他子题目
                    }
                    
                    // 递归检查子题目答案正确性（支持多层嵌套）
                    bool subCorrect;
                if (subQuestion.hasSubQuestions && subQuestion.subQuestions != null && subQuestion.subQuestions.Length > 0)
                {
                    // 先检查是否所有子题目都已完成，如果是则自动标记
                    bool allSubCompleted = CheckAllSubQuestionsAnswered(subQuestion, subQuestion.id);
                    if (allSubCompleted && !subQuestion.isAnswered)
                    {
                        QuestionSystem.Instance.AnswerQuestion(subQuestion.id, "");
                        subQuestion.isAnswered = true; // 立即更新本地状态
                    }
                    subCorrect = CheckAnswerCorrectness(subQuestion.id);
                }
                    else
                    {
                        // 子题目是单个题目
                        subCorrect = CheckSingleQuestionCorrectness(subQuestion);
                    }
                    
                    
                    if (!subCorrect)
                    {
                        allSubQuestionsCorrect = false;
                    }
                }
                return allSubQuestionsCorrect;
            }
            else
            {
                // 单个题目，使用原来的逻辑
                return CheckSingleQuestionCorrectness(question);
            }
        }
        
        /// <summary>
        /// 检查单个题目的答案是否正确
        /// </summary>
        /// <summary>
        /// 递归检查组合题目的所有子题目是否都已回答（包括多层嵌套）
        /// 如果所有子题目都完成，自动标记主题目为已完成
        /// </summary>
        private bool CheckAllSubQuestionsAnswered(Question parentQuestion, int parentId)
        {
            if (parentQuestion.subQuestions == null || parentQuestion.subQuestions.Length == 0)
            {
                return parentQuestion.isAnswered;
            }
            
            bool allSubCompleted = true;
            foreach (Question subQuestion in parentQuestion.subQuestions)
            {
                bool subAnswered;
                
                if (subQuestion.hasSubQuestions && subQuestion.subQuestions != null && subQuestion.subQuestions.Length > 0)
                {
                    // 子题目也是组合题目，递归检查
                    subAnswered = CheckAllSubQuestionsAnswered(subQuestion, subQuestion.id);
                    
                    // 如果子组合题目的所有子题目都完成，标记该子组合题目为已完成
                    if (subAnswered && !subQuestion.isAnswered)
                    {
                        QuestionSystem.Instance.AnswerQuestion(subQuestion.id, "");
                        subQuestion.isAnswered = true; // 立即更新本地状态
                    }
                }
                else
                {
                    // 叶子题目，检查isAnswered
                    subAnswered = subQuestion.isAnswered;
                }
                
                if (!subAnswered)
                {
                    allSubCompleted = false;
                }
            }
            
            // 如果所有子题目都完成，标记主题目为已完成
            if (allSubCompleted && !parentQuestion.isAnswered)
            {
                QuestionSystem.Instance.AnswerQuestion(parentId, "");
                parentQuestion.isAnswered = true; // 立即更新本地状态
            }
            
            return allSubCompleted;
        }

        private bool CheckSingleQuestionCorrectness(Question question)
        {
            
            // 去除所有空格和换行符进行比较
            string playerAnswer = question.playerAnswer?.Replace(" ", "").Replace("\r", "").Replace("\n", "").Trim();
            string correctAnswer = question.correctAnswer?.Replace(" ", "").Replace("\r", "").Replace("\n", "").Trim();
            
            // 处理不同题型的答案检查
            switch (question.type)
            {
                case QuestionType.SingleChoice:
                case QuestionType.TrueFalse:
                case QuestionType.ShortAnswer:
                case QuestionType.FillInBlank:
                case QuestionType.Puzzle:
                case QuestionType.ImagePuzzle:
                case QuestionType.ImageInput:
                    // 单答案题型：直接比较字符串（忽略大小写和空格）
                    bool isCorrect = string.Equals(playerAnswer, correctAnswer, StringComparison.OrdinalIgnoreCase);
                    return isCorrect;
                    
                case QuestionType.MultipleChoice:
                    // 多选题：需要检查correctAnswerIndices
                    if (question.correctAnswerIndices != null && question.correctAnswerIndices.Length > 0)
                    {
                        // 这里需要根据具体的多选题答案格式来实现
                        // 暂时使用字符串比较作为备用
                        bool multiCorrect = string.Equals(playerAnswer, correctAnswer, StringComparison.OrdinalIgnoreCase);
                        return multiCorrect;
                    }
                    else
                    {
                        bool multiCorrect2 = string.Equals(playerAnswer, correctAnswer, StringComparison.OrdinalIgnoreCase);
                        return multiCorrect2;
                    }
                    
                case QuestionType.Essay:
                    // 主观题：始终认为正确（需要人工评判）
                    return true;
                    
                default:
                    return true;
            }
        }
        
        /// <summary>
        /// 显示答题反馈信息
        /// </summary>
        private void ShowAnswerFeedback(int questionId, bool isCorrect)
        {
            if (DialogueSystem.Instance == null) return;
            
            List<DialogueData> feedbackDialogue = new List<DialogueData>();
            
            if (isCorrect)
            {
                // 根据题目给出专门的正确反馈
                string feedback = GetCorrectAnswerFeedback(questionId);
                feedbackDialogue.Add(DialogueSystem.CreateInteractionHint(feedback, 2f));
            }
            else
            {
                // 错误答案反馈
                string feedback = GetWrongAnswerFeedback(questionId);
                feedbackDialogue.Add(DialogueSystem.CreateInteractionHint(feedback, 2f));
            }
            
            DialogueSystem.Instance.StartDialogue(feedbackDialogue);
        }
        
        /// <summary>
        /// 获取正确答案的反馈信息
        /// </summary>
        private string GetCorrectAnswerFeedback(int questionId)
        {
            string keyReward = GetKeyReward(questionId);
            if (!string.IsNullOrEmpty(keyReward))
            {
                string keyDisplayName = GetKeyDisplayName(keyReward);
                return $"答案正确！获得了{keyDisplayName}！";
            }
            else
            {
                return "答案正确！";
            }
        }
        
        /// <summary>
        /// 获取错误答案的反馈信息
        /// </summary>
        private string GetWrongAnswerFeedback(int questionId)
        {
            return "回答错误";
        }
        
        private void RefreshStairInteractionTexts()
        {
            // 更新楼梯的交互文本
            if (leftStair1To2 != null)
            {
                leftStair1To2.interactionText = "按E交互" ;
            }
            if (rightStair1To2 != null)
            {
                rightStair1To2.interactionText = "按E交互" ;
            }
            if (leftStair2To3 != null)
            {
                leftStair2To3.interactionText = "按E交互" ;
            }
            if (rightStair2To3 != null)
            {
                rightStair2To3.interactionText = "按E交互" ;
            }
            if (leftStair3ToStage5 != null)
            {
                leftStair3ToStage5.interactionText = HasKey("left_floor3_key") ? "按E前往Stage5" : "按E交互" ;
            }
            if (rightStair3ToStage5 != null)
            {
                rightStair3ToStage5.interactionText = HasKey("right_floor3_key") ? "按E前往Stage5" : "按E交互" ;
            }
        }
        
        /// <summary>
        /// 同步嵌套题目状态（专门处理题目46和47的多层嵌套问题）
        /// </summary>
        private void SyncNestedQuestionStates(int questionId)
        {
            if (QuestionSystem.Instance == null) return;
            
            Question question = QuestionSystem.Instance.GetQuestionById(questionId);
            if (question == null) return;
            
            // 递归同步所有子题目状态
            SyncQuestionStateRecursively(question, questionId);
        }
        
        /// <summary>
        /// 递归同步题目状态
        /// </summary>
        private void SyncQuestionStateRecursively(Question question, int questionId)
        {
            if (question.hasSubQuestions && question.subQuestions != null && question.subQuestions.Length > 0)
            {
                bool allSubCompleted = true;
                
                // 先递归检查所有子题目
                foreach (Question subQuestion in question.subQuestions)
                {
                    SyncQuestionStateRecursively(subQuestion, subQuestion.id);
                    
                    if (!subQuestion.isAnswered)
                    {
                        allSubCompleted = false;
                    }
                }
                
                // 如果所有子题目都完成了，但当前题目未标记为完成，则标记它
                if (allSubCompleted && !question.isAnswered)
                {
                    QuestionSystem.Instance.AnswerQuestion(questionId, "");
                    question.isAnswered = true;
                }
            }
        }
        
        /// <summary>
        /// 刷新指定题目对应的QuestionInteractable状态
        /// </summary>
        private void RefreshQuestionInteractableState(int questionId)
        {
            QuestionInteractable targetInteractable = null;
            
            // 根据题目ID找到对应的QuestionInteractable
            switch (questionId)
            {
                case 42:
                    targetInteractable = leftStair1Puzzle;
                    break;
                case 43:
                    targetInteractable = rightStair1Puzzle;
                    break;
                case 44:
                    targetInteractable = leftFloor2Puzzle;
                    break;
                case 45:
                    targetInteractable = rightFloor2Puzzle;
                    break;
                case 46:
                    targetInteractable = leftFloor3Puzzle;
                    break;
                case 47:
                    targetInteractable = rightFloor3Puzzle;
                    break;
                default:
                    return;
            }
            
            if (targetInteractable != null)
            {
                // 刷新题目状态
                targetInteractable.RefreshQuestionState();
                
                // 如果题目已完成且有对应钥匙，隐藏交互文本
                string keyReward = GetKeyReward(questionId);
                if (!string.IsNullOrEmpty(keyReward) && HasKey(keyReward))
                {
                    targetInteractable.interactionText = ""; // 隐藏交互文本
                    targetInteractable.isInteractable = false; // 禁用交互
                }
            }
        }
        
        private string GetKeyReward(int questionId)
        {
            // 根据requirement.md中的描述返回钥匙奖励
            switch (questionId)
            {
                case 42: return "left_stair_key";   // 左1层楼梯谜题给左侧钥匙
                case 43: return "right_stair_key";  // 右1层楼梯谜题给右侧钥匙
                case 44: return "left_floor2_key";  // 左2层谜题给左侧2层钥匙
                case 45: return "right_floor2_key"; // 右2层谜题给右侧2层钥匙
                case 46: return "left_floor3_key";  // 左3层谜题给左侧3层钥匙
                case 47: return "right_floor3_key"; // 右3层谜题给右侧3层钥匙
                default: return "";
            }
        }
        
        private void GiveKey(string keyName)
        {
            if (keys.ContainsKey(keyName))
            {
                keys[keyName] = true;
                string displayName = GetKeyDisplayName(keyName);
                
                
                if (DialogueSystem.Instance != null)
                {
                    List<DialogueData> keyDialogue = new List<DialogueData>
                    {
                        DialogueSystem.CreateInteractionHint($"获得了{displayName}！", 2f)
                    };
                    DialogueSystem.Instance.StartDialogue(keyDialogue);
                }
            }
        }
        
        private string GetKeyDisplayName(string keyName)
        {
            switch (keyName)
            {
                case "right_stair_key": return "右侧楼梯钥匙";
                case "left_stair_key": return "左侧楼梯钥匙";
                case "right_floor2_key": return "右侧2层钥匙";
                case "left_floor2_key": return "左侧2层钥匙";
                case "right_floor3_key": return "右侧3层钥匙";
                case "left_floor3_key": return "左侧3层钥匙";
                default: return "未知钥匙";
            }
        }
        
        private void HandlePuzzleHints(int questionId)
        {
            // 根据requirement.md实现左右题目间的提示关联
            // 完成题目后给予相应的提示物品到背包
            GiveItemRewards(questionId);
            
            // TODO: 实现题目44-45和46-47之间的互相提示机制
        }
        
        /// <summary>
        /// 根据完成的题目给予相应的物品奖励
        /// </summary>
        private void GiveItemRewards(int questionId)
        {
            // 确保InventorySystem已初始化
            if (InventorySystem.Instance == null)
            {
                return;
            }
            
            List<int> rewardItems = new List<int>();
            string rewardMessage = "";
            
            switch (questionId)
            {
                case 44: // 左2层谜题：设计碰字猜诗词 + 写整个诗句
                    rewardItems.Add(2); // 提示1
                    rewardItems.Add(3); // 提示2
                    rewardMessage = "获得了提示物品！";
                    break;
                    
                case 45: // 右2层谜题：顶天立地者何人 + 大肠包小肠
                    rewardItems.Add(4); // 提示3
                    rewardItems.Add(5); // 提示4
                    rewardMessage = "获得了提示物品！";
                    break;
                    
                case 46: // 左3层谜题：三盘棋题目 + 空洞数 + 三位数+150
                    rewardItems.Add(6); // 提示5
                    rewardItems.Add(7); // 提示6
                    rewardMessage = "获得了提示物品！";
                    break;
                    
                case 47: // 右3层谜题：三道题 + 质数 + 三位数-271
                    rewardItems.Add(8); // 提示7
                    rewardItems.Add(9); // 提示8
                    rewardMessage = "获得了提示物品！";
                    break;
                    
                default:
                    // 其他题目暂无物品奖励
                    return;
            }
            
            // 给予物品奖励
            bool allAdded = true;
            foreach (int itemId in rewardItems)
            {
                bool added = InventorySystem.AddItemToInventory(itemId);
                if (!added)
                {
                    allAdded = false;
                }
            }
            
            // 显示奖励提示
            if (allAdded && DialogueSystem.Instance != null)
            {
                List<DialogueData> rewardDialogue = new List<DialogueData>
                {
                    DialogueSystem.CreateInteractionHint(rewardMessage, 2f)
                };
                DialogueSystem.Instance.StartDialogue(rewardDialogue);
            }
            else if (!allAdded)
            {
                if (DialogueSystem.Instance != null)
                {
                    List<DialogueData> errorDialogue = new List<DialogueData>
                    {
                        DialogueSystem.CreateInteractionHint("背包已满，无法获得全部奖励！", 2f)
                    };
                    DialogueSystem.Instance.StartDialogue(errorDialogue);
                }
            }
        }
        
        private void CheckHiddenExit()
        {
            // 检查是否集齐了所有6把钥匙
            if (HasAllKeys())
            {
                if (DialogueSystem.Instance != null)
                {
                    List<DialogueData> unlockDialogue = new List<DialogueData>
                    {
                        DialogueSystem.CreateInteractionHint("已集齐所有钥匙！", 3f)
                    };
                    DialogueSystem.Instance.StartDialogue(unlockDialogue);
                }
            }
        }
        
        /// <summary>
        /// 检查是否拥有指定的钥匙
        /// </summary>
        public bool HasKey(string keyName)
        {
            return keys.ContainsKey(keyName) && keys[keyName];
        }
        
        /// <summary>
        /// 检查指定题目对应的钥匙是否已获得（供QuestionInteractable调用）
        /// </summary>
        public bool HasKeyForQuestion(int questionId)
        {
            string keyName = GetKeyReward(questionId);
            if (string.IsNullOrEmpty(keyName))
            {
                return false; // 如果题目不给钥匙，返回false
            }
            return HasKey(keyName);
        }
        
        /// <summary>
        /// 供SceneTransitionInteractable调用，检查是否可以使用指定的楼层切换
        /// 通过引用比较确定具体是哪个楼梯或洞口
        /// </summary>
        public bool CanUseTransition(SceneTransitionInteractable transition)
        {
            // 楼梯需要钥匙验证
            if (transition == leftStair1To2)
            {
                return HasKey("left_stair_key");
            }
            else if (transition == rightStair1To2)
            {
                return HasKey("right_stair_key");
            }
            else if (transition == leftStair2To3)
            {
                return HasKey("left_floor2_key");
            }
            else if (transition == rightStair2To3)
            {
                return HasKey("right_floor2_key");
            }
            else if (transition == leftStair3ToStage5)
            {
                return HasKey("left_floor3_key");
            }
            else if (transition == rightStair3ToStage5)
            {
                return HasKey("right_floor3_key");
            }
            // 洞口可以无条件使用（确认对话在Interact方法中处理）
            else if (transition == floor2Hole || transition == floor3Hole)
            {
                return true;
            }
            
            // 默认允许（可能是其他未识别的切换）
            return true;
        }
        
        /// <summary>
        /// 供SceneTransitionInteractable调用，处理洞口交互的确认对话
        /// </summary>
        public void HandleHoleInteraction(SceneTransitionInteractable hole)
        {
            if (hole == floor2Hole)
            {
                // 2层洞口：直接显示确认对话
                ShowHoleConfirmationDialogue(hole, "是否要跳下洞口回到一楼？", false);
            }
            else if (hole == floor3Hole)
            {
                // 3层洞口：特殊逻辑
                floor3HoleInteractionCount++;
                
                if (floor3HoleInteractionCount < 3)
                {
                    // 前两次：普通确认对话
                    ShowHoleConfirmationDialogue(hole, "是否要跳下洞口回到一楼？", false);
                }
                else
                {
                    // 第三次：特殊传送确认
                    ShowHoleConfirmationDialogue(hole, "洞口散发出神秘的光芒...要跳下去吗？", true);
                }
            }
        }
        
        private void ShowHoleConfirmationDialogue(SceneTransitionInteractable hole, string confirmText, bool isSpecial)
        {
            if (DialogueSystem.Instance != null)
            {
                // 创建选项
                List<DialogueOption> holeOptions = new List<DialogueOption>
                {
                    DialogueSystem.CreateOption("确认", () => ConfirmHoleJump(hole, isSpecial)),
                    DialogueSystem.CreateOption("取消", () => CancelHoleJump())
                };
                
                // 如果是特殊情况（3层洞口第三次交互），添加第三个选项
                if (isSpecial)
                {
                    holeOptions.Add(DialogueSystem.CreateOption("返回一楼", () => ReturnToFloor1(hole)));
                }
                
                // 创建确认对话
                List<DialogueData> confirmDialogue = new List<DialogueData>
                {
                    DialogueSystem.CreateChoiceDialogue("我", confirmText, holeOptions)
                };
                
                // 暂停玩家移动
                if (playerController != null)
                {
                    playerController.SetCanMove(false);
                }
                
                DialogueSystem.Instance.StartDialogue(confirmDialogue);
            }
        }
        
        /// <summary>
        /// 确认洞口跳跃
        /// </summary>
        private void ConfirmHoleJump(SceneTransitionInteractable hole, bool isSpecial)
        {
            // 恢复玩家移动
            if (playerController != null)
            {
                playerController.SetCanMove(true);
            }
            
            // 确认传送
            if (isSpecial)
            {
                // 特殊传送到Stage5
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.LoadScene("Stage5");
                }
            }
            else
            {
                // 普通传送：调用SceneTransitionInteractable的内部区域切换方法
                if (!string.IsNullOrEmpty(hole.targetSceneName))
                {
                    // 跨场景传送
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.LoadScene(hole.targetSceneName);
                    }
                }
                else
                {
                    // 场景内传送：直接调用HandleInternalAreaTransition的逻辑
                    if (hole.targetPlayerPosition != null && playerController != null)
                    {
                        playerController.TeleportTo(hole.targetPlayerPosition.position);
                    }
                    
                    // 移动相机
                    if (hole.targetCameraPosition != null)
                    {
                        Camera targetCamera = hole.sceneCamera != null ? hole.sceneCamera : Camera.main;
                        if (targetCamera != null)
                        {
                            Vector3 cameraPos = hole.targetCameraPosition.position;
                            cameraPos.z = targetCamera.transform.position.z; // 保持Z轴深度
                            targetCamera.transform.position = cameraPos;
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 取消洞口跳跃
        /// </summary>
        private void CancelHoleJump()
        {
            // 恢复玩家移动
            if (playerController != null)
            {
                playerController.SetCanMove(true);
            }
        }
        
        /// <summary>
        /// 返回一楼（普通传送，不激活特殊功能）
        /// </summary>
        private void ReturnToFloor1(SceneTransitionInteractable hole)
        {
            // 恢复玩家移动
            if (playerController != null)
            {
                playerController.SetCanMove(true);
            }
            
            // 执行普通的场景内传送到一楼
            if (hole.targetPlayerPosition != null && playerController != null)
            {
                playerController.TeleportTo(hole.targetPlayerPosition.position);
            }
            
            // 移动相机
            if (hole.targetCameraPosition != null)
            {
                Camera targetCamera = hole.sceneCamera != null ? hole.sceneCamera : Camera.main;
                if (targetCamera != null)
                {
                    Vector3 cameraPos = hole.targetCameraPosition.position;
                    cameraPos.z = targetCamera.transform.position.z; // 保持Z轴深度
                    targetCamera.transform.position = cameraPos;
                }
            }
            
            // 显示普通返回提示
            if (DialogueSystem.Instance != null)
            {
                List<DialogueData> returnDialogue = new List<DialogueData>
                {
                    DialogueSystem.CreateInteractionHint("已返回一楼", 1.5f)
                };
                DialogueSystem.Instance.StartDialogue(returnDialogue);
            }
        }
        
        private void Update()
        {
            // 按Tab显示进度
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ShowCurrentStatus();
            }
            
            // 按H显示帮助
            if (Input.GetKeyDown(KeyCode.H))
            {
                ShowHelp();
            }
            
            // 按ESC返回主菜单
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                GameManager.Instance.HandleEscapeKey();
            }
        }
        
        private void ShowCurrentStatus()
        {
            if (DialogueSystem.Instance != null)
            {
                int keyCount = 0;
                foreach (var key in keys.Values)
                {
                    if (key) keyCount++;
                }
                
                List<DialogueData> statusInfo = new List<DialogueData>
                {
                    DialogueSystem.CreateInteractionHint($"当前进度: {completedQuestions}/{totalQuestions}道题目已完成", 2f),
                    DialogueSystem.CreateInteractionHint($"钥匙收集: {keyCount}/6把钥匙", 2f)
                };
                
                DialogueSystem.Instance.StartDialogue(statusInfo);
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
                    DialogueSystem.CreateInteractionHint("Tab：查看状态", 1f),
                    DialogueSystem.CreateInteractionHint("ESC：返回主菜单", 2f)
                };
                
                DialogueSystem.Instance.StartDialogue(helpInfo);
            }
        }

        public bool HasAllKeys()
        {
            foreach (var key in keys.Values)
            {
                if (!key) return false;
            }
            return true;
        }
        
        // 检查是否有任意一个3层钥匙
        public bool HasAnyFloor3Key()
        {
            return (keys.ContainsKey("right_floor3_key") && keys["right_floor3_key"]) ||
                   (keys.ContainsKey("left_floor3_key") && keys["left_floor3_key"]);
        }
        
        // 供HiddenExitInteractable调用的公共方法
        public void OnHiddenExitTriggered()
        {
            StartCoroutine(PlaySpecialTransition());
        }
        
        private IEnumerator PlaySpecialTransition()
        {
            if (DialogueSystem.Instance != null)
            {
                List<DialogueData> specialDialogue = new List<DialogueData>
                {
                    DialogueSystem.CreateSceneDescription("【特殊转场动画】"),
                    DialogueSystem.CreateSubtitle("我", "完美的智慧开启了隐藏的道路..."),
                    DialogueSystem.CreateInteractionHint("获得特殊结局！", 3f)
                };
                
                DialogueSystem.Instance.StartDialogue(specialDialogue);
                yield return new WaitUntil(() => !DialogueSystem.Instance.isDialogueActive);
            }
            
            yield return new WaitForSeconds(2f);
            
            // 跳转到Stage5（或特殊场景）
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadScene("Stage5");
            }
        }
    }
} 