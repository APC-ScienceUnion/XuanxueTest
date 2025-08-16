using UnityEngine;
using System.Collections.Generic;

namespace XuanZhiShiLian
{
    // 题目交互对象
    public class QuestionInteractable : Interactable
    {
        [Header("题目设置")]
        public int questionId;
        public bool removeAfterAnswered = true;
        
        private Question question;
        private bool playerInRange = false;
        private PlayerController nearbyPlayer;
        
        protected override void Start()
        {
            base.Start();
            
            // 确保Collider2D设置为Trigger（但不强制创建）
            Collider2D collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
            
            // 获取题目信息并设置交互文本
            LoadQuestionData();
        }
        
        private void LoadQuestionData()
        {
            if (QuestionSystem.Instance != null)
            {
                question = QuestionSystem.Instance.GetQuestionById(questionId);
                
                // 设置默认交互文本（如果用户没有自定义）
                if (string.IsNullOrEmpty(interactionText))
                {
                    if (question != null)
                    {
                        interactionText = $"按E答题 - 题目{questionId}";
                    }
                    else
                    {
                        interactionText = $"按E交互 - 题目{questionId}";
                    }
                }
                
                // 如果题目已经回答且设置为回答后移除，则禁用交互
                if (question != null && question.isAnswered && removeAfterAnswered)
                {
                    isInteractable = false;
                    SetHighlight(false);
                    gameObject.SetActive(false);
                }
            }
            else
            {
                // 设置备用文本
                if (string.IsNullOrEmpty(interactionText))
                {
                    interactionText = $"按E交互 - 题目{questionId}";
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
            if (!playerInRange)
            {
                Debug.LogWarning($"[QuestionInteractable] 玩家不在范围内，忽略交互 Q{questionId}");
                return;
            }
            
            // 特殊处理：questionId为负数时，通知Stage控制器进行自定义交互
            if (questionId < 0)
            {
                Debug.Log($"[QuestionInteractable] 负数题目ID，交给Stage控制器处理: {questionId}");
                NotifyStageController(questionId);
                return;
            }
            
            if (question == null)
            {
                // 尝试在交互当下重新获取题目（处理QuestionSystem初始化较晚的情况）
                if (QuestionSystem.Instance != null)
                {
                    question = QuestionSystem.Instance.GetQuestionById(questionId);
                }
                if (question == null)
                {
                    Debug.LogError($"[QuestionInteractable] 题目为空，无法交互 Q{questionId}");
                    return;
                }
            }
            
            // 检查Stage4中是否已经有对应的钥匙
            Stage4Controller stage4Controller = FindObjectOfType<Stage4Controller>();
            if (stage4Controller != null && stage4Controller.HasKeyForQuestion(questionId))
            {
                // 禁用QuestionInteractable组件
                this.enabled = false;
                Debug.Log($"[QuestionInteractable] 已有对应钥匙，禁用交互 Q{questionId}");
                return;
            }
            
            // 检查题目是否已答
            if (question.isAnswered && removeAfterAnswered)
            {
                ShowAlreadyAnsweredMessage();
                Debug.Log($"[QuestionInteractable] 题目已完成且配置为完成后移除，Q{questionId}");
                return;
            }
            
            // 显示题目
            ShowQuestion();
        }
        
        private void ShowQuestion()
        {
            // 使用QuestionSystem显示题目
            if (QuestionSystem.Instance != null)
            {
                Debug.Log($"[QuestionInteractable] 尝试显示题目 Q{questionId}, hasSub={question.hasSubQuestions}, subCount={(question.subQuestions!=null?question.subQuestions.Length:0)}");
                // 检查是否为组合题目（有子题目）
                if (question.hasSubQuestions)
                {
                    // 组合题目：若子题无效则回退为单题显示
                    if (question.subQuestions != null && question.subQuestions.Length > 0)
                    {
                        List<Question> questionGroup = new List<Question>(question.subQuestions);
                        QuestionSystem.Instance.ShowQuestionGroup(questionGroup, OnQuestionGroupCompleted, null, nearbyPlayer);
                    }
                    else
                    {
                        Debug.LogWarning($"[QuestionInteractable] 组合题子题为空，回退为单题显示 Q{questionId}");
                        QuestionSystem.Instance.ShowQuestion(question, OnQuestionAnswered, nearbyPlayer);
                    }
                }
                else
                {
                    // 单个题目：正常显示
                    QuestionSystem.Instance.ShowQuestion(question, OnQuestionAnswered, nearbyPlayer);
                }
                
                // 暂停玩家移动
                if (nearbyPlayer != null)
                {
                    nearbyPlayer.SetCanMove(false);
                }
            }
            else
            {
                Debug.LogError("[QuestionInteractable] QuestionSystem.Instance 为空，无法显示题目");
            }
        }
        
        private void OnQuestionGroupCompleted()
        {
            
            // 显示完成提示
            if (DialogueSystem.Instance != null)
            {
                var dialogue = new System.Collections.Generic.List<DialogueData>
                {
                    DialogueSystem.CreateInteractionHint($"组合题目{questionId}已完成！", 2f)
                };
                DialogueSystem.Instance.StartDialogue(dialogue);
            }
            
            // 如果设置为回答后移除，则禁用交互
            if (removeAfterAnswered)
            {
                isInteractable = false;
                SetHighlight(false);
                
                // 延迟隐藏物体，让玩家能看到完成效果
                Invoke(nameof(HideObject), 1f);
            }
            
            // 恢复玩家移动
            if (nearbyPlayer != null)
            {
                nearbyPlayer.SetCanMove(true);
            }
            
            // 通知Stage控制器（使用原题目ID）
            NotifyStageController(questionId);
        }
        
        private void OnQuestionAnswered(Question answeredQuestion, string answer)
        {
            // 提交答案
            if (QuestionSystem.Instance != null)
            {
                QuestionSystem.Instance.AnswerQuestion(answeredQuestion.id, answer);
            }
            
            // 显示完成提示
            ShowCompletionMessage(answer);
            
            // 如果设置为回答后移除，则禁用交互
            if (removeAfterAnswered)
            {
                isInteractable = false;
                SetHighlight(false);
                
                // 延迟隐藏物体，让玩家能看到完成效果
                Invoke(nameof(HideObject), 1f);
            }
            
            // 恢复玩家移动
            if (nearbyPlayer != null)
            {
                nearbyPlayer.SetCanMove(true);
            }
            
            // 通知Stage控制器
            NotifyStageController(answeredQuestion.id);
        }
        
        private void ShowAlreadyAnsweredMessage()
        {
            if (DialogueSystem.Instance != null)
            {
                var dialogue = new System.Collections.Generic.List<DialogueData>
                {
                    DialogueSystem.CreateInteractionHint($"题目{questionId}已经完成", 2f)
                };
                DialogueSystem.Instance.StartDialogue(dialogue);
            }
        }
        

        
        private void ShowCompletionMessage(string answer)
        {
            if (DialogueSystem.Instance != null)
            {
                var dialogue = new System.Collections.Generic.List<DialogueData>
                {
                    DialogueSystem.CreateInteractionHint($"题目{questionId}已完成！", 2f)
                };
                DialogueSystem.Instance.StartDialogue(dialogue);
            }
        }
        
        private void HideObject()
        {
            gameObject.SetActive(false);
        }
        
        private void NotifyStageController(int questionId)
        {
            // 根据当前Stage优先通知对应的控制器
            int currentStage = GameManager.Instance != null ? GameManager.Instance.currentStage : 0;
            
            // 优先查找当前Stage的控制器
            if (currentStage == 4)
            {
                Stage4Controller stage4Controller = FindObjectOfType<Stage4Controller>();
                if (stage4Controller != null)
                {
                    stage4Controller.OnQuestionAnswered(questionId);
                    return;
                }
            }
            else if (currentStage == 3)
            {
                Stage3Controller stage3Controller = FindObjectOfType<Stage3Controller>();
                if (stage3Controller != null)
                {
                    stage3Controller.OnQuestionAnswered(questionId);
                    return;
                }
            }
            else if (currentStage == 2)
            {
                Stage2Controller stage2Controller = FindObjectOfType<Stage2Controller>();
                if (stage2Controller != null)
                {
                    stage2Controller.OnQuestionAnswered(questionId);
                    return;
                }
            }
            
            Stage2Controller fallbackStage2Controller = FindObjectOfType<Stage2Controller>();
            if (fallbackStage2Controller != null)
            {
                fallbackStage2Controller.OnQuestionAnswered(questionId);
                return;
            }
            
            Stage3Controller fallbackStage3Controller = FindObjectOfType<Stage3Controller>();
            if (fallbackStage3Controller != null)
            {
                fallbackStage3Controller.OnQuestionAnswered(questionId);
                return;
            }
            
            Stage4Controller fallbackStage4Controller = FindObjectOfType<Stage4Controller>();
            if (fallbackStage4Controller != null)
            {
                fallbackStage4Controller.OnQuestionAnswered(questionId);
                return;
            }
        }
        
        // 公共方法：刷新题目状态（用于存档加载后）
        public void RefreshQuestionState()
        {
            LoadQuestionData();
        }
        
        // 调试信息
        private void OnDrawGizmosSelected()
        {
            // 绘制交互范围
            Collider2D collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                Gizmos.color = isInteractable ? Color.green : Color.red;
                Gizmos.matrix = transform.localToWorldMatrix;
                
                if (collider is BoxCollider2D boxCollider)
                {
                    Gizmos.DrawWireCube(boxCollider.offset, boxCollider.size);
                }
                else if (collider is CircleCollider2D circleCollider)
                {
                    Gizmos.DrawWireSphere(circleCollider.offset, circleCollider.radius);
                }
                else
                {
                    Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
                }
                
                Gizmos.matrix = Matrix4x4.identity;
            }
            
            // 显示题目ID
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, $"Q{questionId}");
            #endif
        }
    }
} 