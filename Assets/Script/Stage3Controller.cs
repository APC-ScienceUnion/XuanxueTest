using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XuanZhiShiLian
{
    public class Stage3Controller : MonoBehaviour
    {
        [Header("场景设置")]
        public Transform playerSpawnPoint;
        
        [Header("玩家位置点")]
        public Transform corridorPlayerPosition; // 玩家在走廊的位置
        public Transform classroomPlayerPosition; // 玩家在教室的位置
        
        [Header("玩家控制")]
        public GameObject playerPrefab;
        public PlayerController playerController;
        
        
        [Header("教室交互对象")]
        public DialogueInteractable teacherInteractable; // 老师（对话交互）
        public QuestionInteractable examDeskInteractable; // 考试桌子（特殊处理）
        
        [Header("场景切换")]
        [Tooltip("从走廊到教室的门（场景内切换）")]
        public SceneTransitionInteractable corridorToClassroomDoor;
        [Tooltip("从教室到走廊的门（场景内切换）")]
        public SceneTransitionInteractable classroomToCorridorDoor;
        [Tooltip("从走廊离开到Stage4的梯子（场景切换）")]
        public SceneTransitionInteractable exitLadder;
        
        // 公共访问器，供SceneTransitionInteractable使用
        public SceneTransitionInteractable CorridorToClassroomDoor => corridorToClassroomDoor;
        public SceneTransitionInteractable ClassroomToCorridorDoor => classroomToCorridorDoor;
        
        private bool isInCorridor = true;
        private int completedQuestions = 0;
        private int totalQuestions = 31; // 题目9-39
        private bool isExamStarted = false;
        private bool hasPaper = false;
        private bool isExamSubmitted = false;

        private bool hasShownInteractionHints = false; // 新增：是否已显示交互提示
        
        private void Start()
        {
            InitializeStage();
            StartStageSequence();
        }
        
        private void InitializeStage()
        {
            // 更新当前阶段
            if (GameManager.Instance != null)
                GameManager.Instance.currentStage = 3;
            
            // 初始化玩家
            InitializePlayer();
            
            // 设置交互对象
            SetupInteractables();
            
            // 检查已完成的题目
            CheckCompletedQuestions();
            
            // 确保背包系统已初始化并显示
            InitializeInventorySystem();

            // 从存档恢复走廊/教室位置与考试状态
            RestoreStage3FromSave();
        }

        private void RestoreStage3FromSave()
        {
            var save = GameManager.Instance != null ? GameManager.Instance.saveSystem?.GetSaveData() : null;
            if (save == null || save.stageData == null || save.stageData.stage3 == null)
            {
                return;
            }

            isInCorridor = save.stageData.stage3.isInCorridor;
            isExamStarted = save.stageData.stage3.isExamStarted;
            hasPaper = save.stageData.stage3.hasPaper;
            isExamSubmitted = save.stageData.stage3.isExamSubmitted;

            // 根据区域将玩家定位到对应锚点
            if (playerController != null)
            {
                Transform target = isInCorridor ? (corridorPlayerPosition ?? playerSpawnPoint) : (classroomPlayerPosition ?? playerSpawnPoint);
                if (target != null)
                {
                    playerController.transform.position = target.position;
                    playerController.RefreshInteractionDetection();
                }
            }

            // 根据状态更新老师对话/桌子提示
            UpdateTeacherDialogue();
        }
        
        /// <summary>
        /// 初始化背包系统
        /// </summary>
        private void InitializeInventorySystem()
        {
            if (InventorySystem.Instance != null)
            {
                // Stage3需要显示背包
                InventorySystem.Instance.InitializeInventoryUI();
                Debug.Log("✅ Stage3 背包系统已初始化");
            }
            else
            {
                Debug.LogWarning("❌ InventorySystem实例不存在，背包功能可能不可用");
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
            
            // 设置玩家位置（初始在走廊）
            if (playerController != null)
            {
                Vector3 startPosition = playerSpawnPoint != null ? playerSpawnPoint.position : 
                                      (corridorPlayerPosition != null ? corridorPlayerPosition.position : Vector3.zero);
                playerController.transform.position = startPosition;
                // 刷新交互检测
                playerController.RefreshInteractionDetection();
            }
        }
        
        private void CheckCompletedQuestions()
        {
            completedQuestions = 0;
            if (QuestionSystem.Instance != null)
            {
                // 检查题目9-39的完成状态
                for (int i = 9; i <= 39; i++)
                {
                    Question question = QuestionSystem.Instance.GetQuestionById(i);
                    if (question != null && question.isAnswered)
                    {
                        completedQuestions++;
                    }
                }
            }
            
            // 检查是否所有题目都已完成
            if (completedQuestions >= totalQuestions)
            {
                hasPaper = true;
                isExamStarted = true;
            }
            
            // 检查是否已经交卷（回答了题目40）
            if (QuestionSystem.Instance != null)
            {
                Question teacherQuestion = QuestionSystem.Instance.GetQuestionById(40);
                if (teacherQuestion != null && teacherQuestion.isAnswered)
                {
                    isExamSubmitted = true;
                    // 校正：若检测到已交卷但背包仍有试卷，自动移除以保持状态一致
                    if (InventorySystem.HasItemInInventory(1))
                    {
                        bool autoRemoved = InventorySystem.RemoveItemFromInventory(1);
                        Debug.Log(autoRemoved
                            ? "🛠 修正：已交卷但背包仍有试卷，已自动移除。"
                            : "⚠️ 修正失败：尝试移除背包中的试卷但未成功。");
                    }
                }
            }
            
            Debug.Log($"Stage3 已完成题目: {completedQuestions}/{totalQuestions}, 试卷状态: {(hasPaper ? "已完成" : "未完成")}, 交卷状态: {(isExamSubmitted ? "已交卷" : "未交卷")}");
        }
        
        private void SetupInteractables()
        {
            // 设置老师对话（根据当前状态动态更新对话内容）
            if (teacherInteractable != null)
            {
                UpdateTeacherDialogue();
                teacherInteractable.isRepeatable = true; // 可重复对话
                teacherInteractable.removeAfterTalk = false; // 不会消失
            }
            
            // 设置考试桌子交互（特殊处理，不是标准题目）
            if (examDeskInteractable != null)
            {
                examDeskInteractable.questionId = -1; // 标记为非标准题目
                examDeskInteractable.interactionText = "按E坐下开始考试";
                examDeskInteractable.removeAfterAnswered = false; // 桌子不会消失
            }
            
            // 设置场景内切换
            if (corridorToClassroomDoor != null)
            {
                corridorToClassroomDoor.targetSceneName = ""; // 空字符串表示场景内切换
                corridorToClassroomDoor.requiresAllQuestionsAnswered = false;
                corridorToClassroomDoor.interactionText = "按E进入教室";
            }
            
            if (classroomToCorridorDoor != null)
            {
                classroomToCorridorDoor.targetSceneName = ""; // 空字符串表示场景内切换
                classroomToCorridorDoor.requiresAllQuestionsAnswered = false;
                classroomToCorridorDoor.interactionText = "按E返回走廊";
            }
            
            // 设置场景切换（到Stage4）
            if (exitLadder != null)
            {
                exitLadder.targetSceneName = "Stage4";
                exitLadder.requiresAllQuestionsAnswered = false;
                exitLadder.interactionText = "按E前往下一层";
            }
        }
        
        /// <summary>
        /// 根据当前状态更新老师的对话内容
        /// </summary>
        private void UpdateTeacherDialogue()
        {
            if (teacherInteractable == null) return;
            
            CheckCompletedQuestions(); // 确保状态最新
            
            // 检查背包中是否有试卷
            bool hasPaperInInventory = InventorySystem.HasItemInInventory(1); // ID 1 是试卷
            
            if (!isExamSubmitted && (!hasPaper || !hasPaperInInventory))
            {
                // 没有试卷
                teacherInteractable.dialogueLines = new string[]
                {
                    "老师：今天是你的考试，带着写好的卷子来吧，我会帮你改的。"
                };
                teacherInteractable.interactionText = "按E与老师交谈";
            }
            else if (!isExamSubmitted && hasPaperInInventory)
            {
                // 有试卷但未交卷
                teacherInteractable.dialogueLines = new string[]
                {
                    "老师：我虽然能现在收下你的卷子，但是需要你先回答我的问题："
                };
                teacherInteractable.interactionText = "按E交卷";
            }
            else
            {
                // 已交卷
                teacherInteractable.dialogueLines = new string[]
                {
                    "老师：虽然交了卷，但你还可以再拿回去改一改。"
                };
                teacherInteractable.interactionText = "按E与老师交谈";
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
            
            // 开场对话
            if (DialogueSystem.Instance != null)
            {
                List<DialogueData> introDialogue = new List<DialogueData>
                {
                    DialogueSystem.CreateSceneDescription("从梯子爬到教室的走廊，有老师办公室和教室，尽头是去下一层的路"),
                    DialogueSystem.CreateSubtitle("我", "考试应该在教室里，要找到自己的座位坐下来考试。"),
                };
                
                DialogueSystem.Instance.StartDialogue(introDialogue);
                yield return new WaitUntil(() => !DialogueSystem.Instance.isDialogueActive);
            }
            
        }
        
        private void ShowInteractionHints()
        {
            if (DialogueSystem.Instance != null)
            {
                List<DialogueData> hints = new List<DialogueData>
                {
                    DialogueSystem.CreateInteractionHint("在教室与座椅交互开始考试", 3f),
                    DialogueSystem.CreateInteractionHint("完成考试后与老师交卷", 3f),
                    DialogueSystem.CreateInteractionHint("使用门在走廊和教室之间切换", 3f)
                };
                
                DialogueSystem.Instance.StartDialogue(hints);
            }
        }
        
        /// <summary>
        /// 由QuestionInteractable调用的回调方法
        /// </summary>
        public void OnQuestionAnswered(int questionId)
        {
            if (questionId == -1)
            {
                // 考试桌子的特殊交互
                HandleExamDeskInteraction();
            }
            else if (questionId == 40)
            {
                // 直接回答题目40（通过ShowQuestion显示的）
                isExamSubmitted = true;
                
                List<DialogueData> submittedDialogue = new List<DialogueData>
                {
                    DialogueSystem.CreateSubtitle("老师", "很好，试卷已收下。你可以继续你的试炼了。"),
                    // 新增：提示试卷被收走
                    DialogueSystem.CreateInteractionHint("试卷被收走了。", 2f)
                };
                
                if (DialogueSystem.Instance != null)
                {
                    DialogueSystem.Instance.StartDialogue(submittedDialogue);
                }
                
                // 确保从背包移除试卷
                bool removedFromInventory40 = InventorySystem.RemoveItemFromInventory(1); // ID 1 是试卷
                if (!removedFromInventory40)
                {
                    Debug.LogWarning("❌ 题目40回答后移除试卷失败（OnQuestionAnswered）");
                }

                // 更新老师对话状态
                UpdateTeacherDialogue();
                
                // 完成Stage3
                GameManager.Instance.CompleteStage(3);
            }
            else
            {
                // 其他题目完成（如果有的话）
                CheckCompletedQuestions();
                Debug.Log($"题目{questionId}已完成，当前进度: {completedQuestions}/{totalQuestions}");
            }
        }
        
        /// <summary>
        /// 由DialogueInteractable调用的对话完成回调
        /// </summary>
        public void OnDialogueCompleted(string interactableName)
        {
            Debug.Log($"🔔 OnDialogueCompleted被调用，interactableName: '{interactableName}'");
            
            // 检查背包中是否有试卷
            bool hasPaperInInventory = InventorySystem.HasItemInInventory(1); // ID 1 是试卷
            Debug.Log($"状态检查 - hasPaper: {hasPaper}, isExamSubmitted: {isExamSubmitted}, hasPaperInInventory: {hasPaperInInventory}");
            
            // 更宽松的教师检测逻辑
            bool isTeacherInteraction = interactableName.ToLower().Contains("teacher") || 
                                       interactableName.Contains("老师") ||
                                       interactableName.ToLower().Contains("npc") ||
                                       ReferenceEquals(FindObjectOfType<DialogueInteractable>(), teacherInteractable);
            
            if (isTeacherInteraction)
            {
                Debug.Log("✅ 检测到教师交互");
                // 老师对话完成后的处理 - 必须同时满足有试卷、未交卷、背包中有试卷三个条件
                if (hasPaper && !isExamSubmitted && hasPaperInInventory)
                {
                    Debug.Log("✅ 条件满足：有试卷且未交卷且背包中有试卷，准备显示题目40");
                    // 有试卷且未交卷且背包中有试卷，显示题目40
                    StartCoroutine(ShowTeacherQuestionAfterDelay());
                }
                else
                {
                    Debug.Log($"❌ 条件不满足：hasPaper={hasPaper}, isExamSubmitted={isExamSubmitted}, hasPaperInInventory={hasPaperInInventory}");
                    
                    // 额外的调试信息
                    if (!hasPaper)
                    {
                        Debug.Log("💡 提示：需要先完成考试获得试卷");
                    }
                    if (!hasPaperInInventory)
                    {
                        Debug.Log("💡 提示：背包中没有试卷");
                    }
                    if (isExamSubmitted)
                    {
                        Debug.Log("💡 提示：试卷已经提交过了");
                    }
                }
            }
            else
            {
                Debug.Log($"❌ 交互对象名称不匹配教师：'{interactableName}'");
                
                // 额外的调试：检查是否是正确的teacherInteractable
                if (teacherInteractable != null)
                {
                    Debug.Log($"💡 teacherInteractable对象名称: '{teacherInteractable.gameObject.name}'");
                }
            }
        }
        
        private IEnumerator ShowTeacherQuestionAfterDelay()
        {
            Debug.Log("⏰ ShowTeacherQuestionAfterDelay开始，等待1秒...");
            yield return new WaitForSeconds(1f);
            
            Debug.Log("📝 开始获取题目40...");
            Question teacherQuestion = QuestionSystem.Instance?.GetQuestionById(40);
            if (teacherQuestion != null)
            {
                Debug.Log($"✅ 成功找到题目40：{teacherQuestion.questionText}");
                Debug.Log("🎯 准备显示题目40...");
                // 防御：显示前再次确认未标记为已交卷，避免误判导致不弹题
                if (!isExamSubmitted)
                {
                    QuestionSystem.Instance.ShowQuestion(teacherQuestion, OnTeacherQuestionAnswered, playerController);
                    Debug.Log("✅ ShowQuestion调用完成");
                }
                else
                {
                    Debug.LogWarning("⚠️ 已交卷标记为true，跳过弹题40");
                }
            }
            else
            {
                Debug.LogError("❌ 未找到题目40");
                Debug.LogError($"QuestionSystem.Instance状态: {QuestionSystem.Instance != null}");
                if (QuestionSystem.Instance != null)
                {
                    var allQuestions = QuestionSystem.Instance.GetAllQuestions();
                    Debug.LogError($"总题目数量: {allQuestions.Count}");
                    foreach (var q in allQuestions)
                    {
                        if (q.id >= 35 && q.id <= 45) // 检查题目40附近的题目
                        {
                            Debug.LogError($"题目{q.id}: {q.questionText}");
                        }
                    }
                }
            }
        }
        
        private void OnTeacherQuestionAnswered(Question question, string answer)
        {
            if (QuestionSystem.Instance != null)
            {
                QuestionSystem.Instance.AnswerQuestion(question.id, answer);
            }
            
            // 题目40回答完，直接从背包移除试卷并完成交卷
            bool removedFromInventory = InventorySystem.RemoveItemFromInventory(1); // ID 1 是试卷
            isExamSubmitted = true;
            
            // 记录背包操作结果
            if (removedFromInventory)
            {
                Debug.Log("✅ 题目40已回答，试卷已从背包中移除，交卷完成");
            }
            else
            {
                Debug.LogWarning("❌ 试卷从背包移除失败");
            }
            
            // 新增：在对话里提示“试卷被收走了。”
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(new List<DialogueData>
                {
                    DialogueSystem.CreateInteractionHint("试卷被收走了。", 2f)
                });
            }

            // 更新老师对话状态
            UpdateTeacherDialogue();
            
            // 完成Stage3
            GameManager.Instance.CompleteStage(3);
        }
        
        private void HandleExamDeskInteraction()
        {
            if (!isExamStarted)
            {
                ShowExamStartDialog();
            }
            else if (hasPaper && !isExamSubmitted)
            {
                ShowExamCompleteDialog();
            }
            else if (isExamStarted && !hasPaper)
            {
                // 考试已开始但未完成，询问是否要修改作答
                ShowExamRetryDialog();
            }
            else if (isExamSubmitted)
            {
                // 已交卷，询问是否要重新修改
                ShowExamRetryDialog();
            }
            else
            {
                ShowExamProgressDialog();
            }
        }
        
        private void ShowExamStartDialog()
        {
            // 创建选项
            List<DialogueOption> options = new List<DialogueOption>
            {
                DialogueSystem.CreateOption("开始考试", OnConfirmStartExam),
                DialogueSystem.CreateOption("取消", OnCancelStartExam)
            };
            
            // 创建带选项的对话
            DialogueData choiceDialogue = DialogueSystem.CreateChoiceDialogue("我", "是否开始考试？", options);
            
            List<DialogueData> examStartDialogue = new List<DialogueData>
            {
                choiceDialogue
            };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(examStartDialogue);
            }
        }
        
        private void OnConfirmStartExam()
        {
            isExamStarted = true;
            
            // 开始考试，显示第一题（题目9）
            StartExamSequence();
        }
        
        private void OnCancelStartExam()
        {
            Debug.Log("用户取消开始考试");
            // 什么都不做，对话会自动结束
        }
        
        private void StartExamSequence()
        {
            if (QuestionSystem.Instance != null)
            {
                // 获取Stage3的题目
                List<Question> examQuestions = QuestionSystem.Instance.GetStageQuestions(3);
                if (examQuestions.Count > 0)
                {
                    // 显示题目组，从题目9开始
                    QuestionSystem.Instance.ShowQuestionGroup(examQuestions, OnExamCompleted, OnExamClosed);
                }
                else
                {
                    Debug.LogError("未找到Stage3的考试题目");
                }
            }
        }
        
        private void OnExamCompleted()
        {
            // 考试完成回调
            CheckCompletedQuestions();
            
            // 将试卷添加到背包中
            bool addedToInventory = InventorySystem.AddItemToInventory(1); // ID 1 是试卷
            
            List<DialogueData> completeDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateInteractionHint("考试完成！", 3f),
                DialogueSystem.CreateSubtitle("我", "应该把卷子交给老师")
            };
            
            // 记录背包操作结果，但不添加额外对话
            if (addedToInventory)
            {
                Debug.Log("✅ 试卷已成功添加到背包");
            }
            else
            {
                Debug.LogWarning("❌ 试卷添加到背包失败，可能背包已满");
            }
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(completeDialogue);
            }
            
            hasPaper = true;
            
            // 更新老师对话状态
            UpdateTeacherDialogue();
        }
        
        private void OnExamClosed()
        {
            // 考试面板关闭时的回调，保存进度
            CheckCompletedQuestions();
            
            // 更新老师对话状态
            UpdateTeacherDialogue();
        }
        
        private void ShowExamCompleteDialog()
        {
            List<DialogueData> submitDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle("我", "考试已完成，可以去找老师交卷了。")
            };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(submitDialogue);
            }
        }
        
        private void ShowExamProgressDialog()
        {
            List<DialogueData> progressInfo = new List<DialogueData>
            {
                DialogueSystem.CreateInteractionHint($"考试进度: {completedQuestions}/{totalQuestions}道题目已完成", 3f)
            };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(progressInfo);
            }
        }
        
        /// <summary>
        /// 显示询问是否修改作答的对话
        /// </summary>
        private void ShowExamRetryDialog()
        {
            // 创建选项
            List<DialogueOption> options = new List<DialogueOption>
            {
                DialogueSystem.CreateOption("继续答卷", OnContinueExam),
                DialogueSystem.CreateOption("取消", OnCancelExam)
            };
            
            // 创建带选项的对话
            DialogueData choiceDialogue = DialogueSystem.CreateChoiceDialogue("我", "是否要修改作答？", options);
            
            List<DialogueData> retryDialogue = new List<DialogueData>
            {
                choiceDialogue
            };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(retryDialogue);
            }
        }
        
        /// <summary>
        /// 选择继续答卷的回调
        /// </summary>
        private void OnContinueExam()
        {
            Debug.Log("用户选择继续答卷");
            // 重新开始考试序列
            StartExamSequence();
        }
        
        /// <summary>
        /// 选择取消的回调
        /// </summary>
        private void OnCancelExam()
        {
            Debug.Log("用户选择取消修改作答");
            // 什么都不做，对话会自动结束
        }
        
        /// <summary>
        /// 从走廊进入教室时调用
        /// </summary>
        public void OnEnterClassroom()
        {
            if (isInCorridor)
            {
                isInCorridor = false;
                // 可以添加进入教室的特殊逻辑
                if (!hasShownInteractionHints)
                {
                    hasShownInteractionHints = true;
                    ShowInteractionHints();
                }
            }
        }
        
        /// <summary>
        /// 从教室返回走廊时调用
        /// </summary>
        public void OnReturnCorridor()
        {
            if (!isInCorridor)
            {
                isInCorridor = true;
                // 可以添加返回走廊的特殊逻辑
            }
        }
        
        private void Update()
        {
            // 按Tab显示进度
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ShowProgress();
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
            // 直接切换场景
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadScene("Stage4");
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
    }
} 