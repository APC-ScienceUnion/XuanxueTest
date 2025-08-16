using UnityEngine;
using System.Collections.Generic;

namespace XuanZhiShiLian
{
    // 纯对话交互对象
    public class DialogueInteractable : Interactable
    {
        [Header("对话设置")]
        [Tooltip("NPC的名字，会显示在对话中")]
        public string speakerName = "NPC"; // NPC名字
        [TextArea(3, 6)]
        public string[] dialogueLines; // 多行对话内容
        public bool isRepeatable = true; // 是否可重复对话
        public bool removeAfterTalk = false; // 对话后是否移除
        
        private bool hasInteracted = false;
        protected bool playerInRange = false;
        protected PlayerController nearbyPlayer;
        
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
                interactionText = "按E对话";
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
            
            // 检查是否已经对话过且不可重复
            if (hasInteracted && !isRepeatable)
            {
                ShowAlreadyTalkedMessage();
                return;
            }
            
            // 显示对话
            ShowDialogue();
        }
        
        private void ShowDialogue()
        {
            if (dialogueLines == null || dialogueLines.Length == 0)
            {
                return;
            }
            
            // 暂停玩家移动
            if (nearbyPlayer != null)
            {
                nearbyPlayer.SetCanMove(false);
            }
            
            // 构建对话数据
            List<DialogueData> dialogueData = new List<DialogueData>();
            
            foreach (string line in dialogueLines)
            {
                if (!string.IsNullOrEmpty(line.Trim()))
                {
                    string trimmedLine = line.Trim();
                    
                    // 解析 说话者：对话内容 格式
                    if (trimmedLine.Contains("："))
                    {
                        string[] parts = trimmedLine.Split('：');
                        if (parts.Length >= 2)
                        {
                            string speaker = parts[0].Trim();
                            string text = parts[1].Trim();
                            dialogueData.Add(DialogueSystem.CreateSubtitle(speaker, text));
                        }
                    }
                    else
                    {
                        // 没有说话者标记，使用NPC的名字作为默认说话者
                        dialogueData.Add(DialogueSystem.CreateSubtitle(speakerName, trimmedLine));
                    }
                }
            }
            
            // 使用DialogueSystem显示对话
            if (DialogueSystem.Instance != null && dialogueData.Count > 0)
            {
                DialogueSystem.Instance.StartDialogue(dialogueData, OnDialogueComplete);
            }
            else
            {
                // 没有对话内容或系统未找到，直接恢复玩家移动
                OnDialogueComplete();
            }
        }
        
        private void OnDialogueComplete()
        {
            // 标记已对话
            hasInteracted = true;
            
            // 恢复玩家移动
            if (nearbyPlayer != null)
            {
                nearbyPlayer.SetCanMove(true);
            }
            
            // 如果设置为对话后移除，则禁用交互
            if (removeAfterTalk)
            {
                isInteractable = false;
                SetHighlight(false);
            }
            
            // 通知Stage控制器
            NotifyStageController();
        }
        
        private void ShowAlreadyTalkedMessage()
        {
            if (DialogueSystem.Instance != null)
            {
                var dialogue = new List<DialogueData>
                {
                    DialogueSystem.CreateInteractionHint("你已经和这里对话过了", 2f)
                };
                DialogueSystem.Instance.StartDialogue(dialogue);
            }
        }
        
        private void HideObject()
        {
            gameObject.SetActive(false);
        }
        
        private void NotifyStageController()
        {
            // 通知当前场景的Stage控制器对话完成
            Stage2Controller stage2Controller = FindObjectOfType<Stage2Controller>();
            if (stage2Controller != null)
            {
                stage2Controller.OnDialogueCompleted(gameObject.name);
                return;
            }
            
            Stage3Controller stage3Controller = FindObjectOfType<Stage3Controller>();
            if (stage3Controller != null)
            {
                stage3Controller.OnDialogueCompleted(gameObject.name);
                return;
            }
            
            Stage5Controller stage5Controller = FindObjectOfType<Stage5Controller>();
            if (stage5Controller != null)
            {
                stage5Controller.OnNPCInteractionComplete(gameObject.name);
                return;
            }
            
            // 可以添加其他Stage控制器的通知
        }
        
        // 公共方法：重置对话状态
        public void ResetDialogue()
        {
            hasInteracted = false;
            isInteractable = true;
            gameObject.SetActive(true);
        }
        
        // 公共方法：刷新交互状态（供Stage控制器使用）
        public void RefreshInteractionState()
        {
            // 重新检查状态，暂时不需要特殊处理
            // 子类可以重写这个方法来处理特殊逻辑
        }
        
        // 调试信息
        private void OnDrawGizmosSelected()
        {
            // 绘制交互范围
            Collider2D collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                Gizmos.color = isInteractable ? Color.blue : Color.gray;
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
            
            // 显示对话标识
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, "💬");
            #endif
        }
    }
} 