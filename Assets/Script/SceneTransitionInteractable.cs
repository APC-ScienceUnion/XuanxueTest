using UnityEngine;
using System.Collections.Generic;

namespace XuanZhiShiLian
{
    // 场景切换交互对象
    public class SceneTransitionInteractable : Interactable
    {
        [Header("场景切换设置")]
        public string targetSceneName;
        public bool requiresAllQuestionsAnswered = false;
        
        [Header("场景内切换设置")]
        public Camera sceneCamera; // 手动附加的相机引用
        public Transform targetCameraPosition; // 相机目标位置
        public Transform targetPlayerPosition; // 玩家目标位置
        
        [Header("切换后回调设置")]
        public UnityEngine.Events.UnityEvent onTransitionComplete; // 切换完成后的回调事件
        
        private bool playerInRange = false;
        private PlayerController nearbyPlayer;
        
        protected override void Start()
        {
            base.Start();
            
            // 确保Collider2D设置为Trigger
            Collider2D collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
            
            // 设置默认交互文本
            if (string.IsNullOrEmpty(interactionText))
            {
                if (!string.IsNullOrEmpty(targetSceneName))
                {
                    interactionText = $"按E前往{targetSceneName}";
                }
                else
                {
                    interactionText = "按E切换区域";
                }
            }
        }
        
        // Trigger事件检测
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isInteractable) return;
            
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                playerInRange = true;
                nearbyPlayer = player;
                OnPlayerEnter();
                
                // 通知玩家控制器
                player.SetCurrentInteractable(this);
            }
        }
        
        private void OnTriggerExit2D(Collider2D other)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && player == nearbyPlayer)
            {
                playerInRange = false;
                nearbyPlayer = null;
                OnPlayerExit();
                
                // 通知玩家控制器
                player.SetCurrentInteractable(null);
            }
        }
        
        public override void Interact()
        {
            if (!playerInRange) return;
            
            // 检查是否是Stage2的退出梯子，需要确认对话
            if (GameManager.Instance != null && GameManager.Instance.currentStage == 2)
            {
                Stage2Controller stage2Controller = FindObjectOfType<Stage2Controller>();
                if (stage2Controller != null && targetSceneName == "Stage3")
                {
                    // Stage2退出梯子需要确认对话
                    stage2Controller.OnExitAttempt();
                    return;
                }
            }
            
            // 检查是否是Stage3的退出梯子，需要确认对话
            if (GameManager.Instance != null && GameManager.Instance.currentStage == 3)
            {
                Stage3Controller stage3Controller = FindObjectOfType<Stage3Controller>();
                if (stage3Controller != null && targetSceneName == "Stage4")
                {
                    // Stage3退出梯子需要确认对话
                    stage3Controller.OnExitAttempt();
                    return;
                }
            }
            
            // 检查是否是Stage4的洞口，需要特殊的确认对话处理
            if (GameManager.Instance != null && GameManager.Instance.currentStage == 4)
            {
                Stage4Controller stage4Controller = FindObjectOfType<Stage4Controller>();
                if (stage4Controller != null)
                {
                    // 检查是否是洞口（通过名称或引用判断）
                    if (gameObject.name.ToLower().Contains("hole") || 
                        this == stage4Controller.floor2Hole || 
                        this == stage4Controller.floor3Hole)
                    {
                        // 洞口需要确认对话，由Stage4Controller处理
                        stage4Controller.HandleHoleInteraction(this);
                        return;
                    }
                }
            }
            
            // 普通的场景切换逻辑
            // 检查是否满足切换条件
            if (CanTransition())
            {
                // 如果targetSceneName为空，说明是场景内区域切换
                if (string.IsNullOrEmpty(targetSceneName))
                {
                    ShowInternalTransitionConfirmation();
                }
                else
                {
                    // 场景间切换
                    ShowSceneTransitionConfirmation();
                }
            }
        }
        
        private bool CanTransition()
        {
            if (GameManager.Instance == null) return false;
            
            // 如果是内部区域切换（targetSceneName为空），需要检查Stage4的钥匙要求
            if (string.IsNullOrEmpty(targetSceneName))
            {
                // 检查是否在Stage4且需要钥匙
                if (GameManager.Instance.currentStage == 4)
                {
                    Stage4Controller stage4Controller = FindObjectOfType<Stage4Controller>();
                    if (stage4Controller != null)
                    {
                        // 使用引用检查钥匙要求
                        bool canUse = stage4Controller.CanUseTransition(this);
                        if (!canUse)
                        {
                            // 没有钥匙，触发对应的谜题
                            TriggerStairPuzzle(stage4Controller);
                            return false;
                        }
                    }
                }
                return true;
            }
            else
            {
                // 场景切换也可能需要钥匙（如3层楼梯到Stage5）
                if (GameManager.Instance.currentStage == 4)
                {
                    Stage4Controller stage4Controller = FindObjectOfType<Stage4Controller>();
                    if (stage4Controller != null)
                    {
                        if (!stage4Controller.CanUseTransition(this))
                        {
                            // 没有钥匙，触发对应的谜题
                            TriggerStairPuzzle(stage4Controller);
                            return false;
                        }
                    }
                }
                
                // 检查是否需要回答所有题目
                if (requiresAllQuestionsAnswered)
                {
                    if (QuestionSystem.Instance != null)
                    {
                        int currentStage = GameManager.Instance.currentStage;
                        List<Question> stageQuestions = QuestionSystem.Instance.GetStageQuestions(currentStage);
                        
                        foreach (Question question in stageQuestions)
                        {
                            if (!question.isAnswered)
                            {
                                return false;
                            }
                        }
                    }
                }
                
                return true;
            }
        }
        
        private void TriggerStairPuzzle(Stage4Controller stage4Controller)
        {
            // 根据当前楼梯确定要触发的谜题
            QuestionInteractable targetPuzzle = GetCorrespondingPuzzle(stage4Controller);
            
            if (targetPuzzle != null)
            {
                // 暂停玩家移动
                if (nearbyPlayer != null)
                {
                    nearbyPlayer.SetCanMove(false);
                }
                
                // 触发谜题
                targetPuzzle.Interact();
            }
            else
            {
                // 找不到对应谜题，显示普通的钥匙提示
                ShowKeyRequiredMessage();
            }
        }
        
        private QuestionInteractable GetCorrespondingPuzzle(Stage4Controller stage4Controller)
        {
            // 通过引用比较确定对应的谜题
            if (this == stage4Controller.leftStair1To2)
            {
                return stage4Controller.leftStair1Puzzle;
            }
            else if (this == stage4Controller.rightStair1To2)
            {
                return stage4Controller.rightStair1Puzzle;
            }
            else if (this == stage4Controller.leftStair2To3)
            {
                return stage4Controller.leftFloor2Puzzle;
            }
            else if (this == stage4Controller.rightStair2To3)
            {
                return stage4Controller.rightFloor2Puzzle;
            }
            else if (this == stage4Controller.leftStair3ToStage5)
            {
                return stage4Controller.leftFloor3Puzzle;
            }
            else if (this == stage4Controller.rightStair3ToStage5)
            {
                return stage4Controller.rightFloor3Puzzle;
            }
            
            return null; // 洞口或其他不需要谜题的切换
        }

        private void HandleInternalAreaTransition()
        {
            // 通用的场景内区域切换
            PlayerController player = FindObjectOfType<PlayerController>();
            
            // 移动玩家
            if (player != null && targetPlayerPosition != null)
            {
                player.TeleportTo(targetPlayerPosition.position);
            }
            
            // 移动相机
            Camera targetCamera = sceneCamera != null ? sceneCamera : Camera.main;
            if (targetCamera != null && targetCameraPosition != null)
            {
                Vector3 cameraPos = targetCameraPosition.position;
                cameraPos.z = targetCamera.transform.position.z; // 保持Z轴深度
                targetCamera.transform.position = cameraPos;
            }
            
            // Stage3的特殊回调处理
            if (GameManager.Instance != null && GameManager.Instance.currentStage == 3)
            {
                Stage3Controller stage3Controller = FindObjectOfType<Stage3Controller>();
                if (stage3Controller != null)
                {
                    // 检查是否是从教室返回走廊的门
                    if (this == stage3Controller.ClassroomToCorridorDoor)
                    {
                        stage3Controller.OnReturnCorridor();
                    }
                    // 检查是否是从走廊进入教室的门
                    else if (this == stage3Controller.CorridorToClassroomDoor)
                    {
                        stage3Controller.OnEnterClassroom();
                    }
                }
            }
            
            // 触发切换完成回调
            if (onTransitionComplete != null)
            {
                onTransitionComplete.Invoke();
            }
        }

        private void ShowKeyRequiredMessage()
        {
            string message = "需要相应的钥匙才能通过";
            
            if (DialogueSystem.Instance != null)
            {
                List<DialogueData> warningDialogue = new List<DialogueData>
                {
                    DialogueSystem.CreateInteractionHint(message, 3f)
                };
                DialogueSystem.Instance.StartDialogue(warningDialogue);
            }
        }
        
        /// <summary>
        /// 显示场景内区域切换的确认对话
        /// </summary>
        private void ShowInternalTransitionConfirmation()
        {
            if (DialogueSystem.Instance == null) return;
            
            List<DialogueOption> options = new List<DialogueOption>
            {
                DialogueSystem.CreateOption("是", () => {
                    HandleInternalAreaTransition();
                }),
                DialogueSystem.CreateOption("否", () => {
                    // 什么也不做，对话结束
                })
            };
            
            DialogueData confirmationDialogue = DialogueSystem.CreateDialogueWithOptions(
                "",
                "要离开这里吗？",
                options
            );
            
            List<DialogueData> dialogueData = new List<DialogueData> { confirmationDialogue };
            DialogueSystem.Instance.StartDialogue(dialogueData);
        }
        
        /// <summary>
        /// 显示场景间切换的确认对话
        /// </summary>
        private void ShowSceneTransitionConfirmation()
        {
            if (DialogueSystem.Instance == null) return;
            
            List<DialogueOption> options = new List<DialogueOption>
            {
                DialogueSystem.CreateOption("是", () => {
                    // 切换场景
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.LoadScene(targetSceneName);
                    }
                }),
                DialogueSystem.CreateOption("否", () => {
                    // 什么也不做，对话结束
                })
            };
            
            DialogueData confirmationDialogue = DialogueSystem.CreateDialogueWithOptions(
                "",
                "要切换Stage吗？",
                options
            );
            
            List<DialogueData> dialogueData = new List<DialogueData> { confirmationDialogue };
            DialogueSystem.Instance.StartDialogue(dialogueData);
        }
    }
} 