using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace XuanZhiShiLian
{
    /// <summary>
    /// 矿洞入口专用交互类，负责处理挖矿相关的交互逻辑
    /// </summary>
    public class MineEntranceInteractable : Interactable
    {
        [Header("矿洞入口设置")]
        [TextArea(2, 5)]
        public string entranceName = "矿洞入口"; // 入口名称
        
        [TextArea(2, 5)]
        public string noPickaxeMessage = "需要有镐子才能挖矿。"; // 没有镐子时的提示信息
        
        [TextArea(2, 5)]
        public string noPickaxeHint = "去矿场登记员那里领取免费石镐吧！"; // 没有镐子时的提示
        
        [TextArea(2, 5)]
        public string miningQuestionText = "要开始挖矿吗？"; // 挖矿确认问题
        
        [TextArea(2, 5)]
        public string miningConfirmText = "挖吧挖吧"; // 确认挖矿选项文本
        
        [TextArea(2, 5)]
        public string miningCancelText = "算了"; // 取消挖矿选项文本
        
        [TextArea(2, 5)]
        public string cancelMessage = "还是去别处先看看吧。"; // 取消挖矿时的消息
        
        private Stage5Controller stage5Controller;
        private PlayerController nearbyPlayer;
        private bool playerInRange = false;
        
        protected override void Start()
        {
            base.Start();
            stage5Controller = FindObjectOfType<Stage5Controller>();
            
            // 确保Collider2D设置为Trigger
            Collider2D collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
            
            // 设置默认交互文本
            if (string.IsNullOrEmpty(interactionText))
            {
                interactionText = "按E挖矿";
            }
        }
        
        public override void Interact()
        {
            if (!playerInRange) return;
            
            if (stage5Controller == null)
            {
                return;
            }
            
            // 检查玩家是否持有镐子
            bool hasPickaxe = stage5Controller.HasPickaxe();
            
            if (hasPickaxe)
            {
                // 有镐子，显示挖矿选择对话
                ShowMiningOptionsDialogue();
            }
            else
            {
                // 没有镐子，显示提示对话
                ShowNoPickaxeDialogue();
            }
        }
        
        /// <summary>
        /// 显示有镐子时的挖矿选择对话
        /// </summary>
        private void ShowMiningOptionsDialogue()
        {
            // 暂停玩家移动
            if (nearbyPlayer != null)
            {
                nearbyPlayer.SetCanMove(false);
            }
            
            List<DialogueOption> miningOptions = new List<DialogueOption>
            {
                DialogueSystem.CreateOption(miningConfirmText, () => {
                    // 开始挖矿
                    if (stage5Controller != null)
                    {
                        stage5Controller.StartMiningQuestion();
                    }
                    
                    // 恢复玩家移动
                    if (nearbyPlayer != null)
                    {
                        nearbyPlayer.SetCanMove(true);
                    }
                }),
                DialogueSystem.CreateOption(miningCancelText, () => {
                    // 取消挖矿
                    ShowCancelMiningDialogue();
                })
            };
            
            // 创建带选项的对话
            DialogueData miningQuestionDialogue = DialogueSystem.CreateDialogueWithOptions(
                "无名勇者",
                miningQuestionText,
                miningOptions
            );
            
            List<DialogueData> dialogues = new List<DialogueData> { miningQuestionDialogue };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(dialogues, () => {
                    // 对话结束时恢复玩家移动（如果还没有恢复的话）
                    if (nearbyPlayer != null)
                    {
                        nearbyPlayer.SetCanMove(true);
                    }
                });
            }
            else
            {
                // 如果DialogueSystem不可用，直接恢复玩家移动
                if (nearbyPlayer != null)
                {
                    nearbyPlayer.SetCanMove(true);
                }
            }
        }
        
        /// <summary>
        /// 显示取消挖矿的对话
        /// </summary>
        private void ShowCancelMiningDialogue()
        {
            List<DialogueData> cancelDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle("无名勇者", cancelMessage)
            };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(cancelDialogue, () => {
                    // 恢复玩家移动
                    if (nearbyPlayer != null)
                    {
                        nearbyPlayer.SetCanMove(true);
                    }
                });
            }
            else
            {
                // 如果DialogueSystem不可用，直接恢复玩家移动
                if (nearbyPlayer != null)
                {
                    nearbyPlayer.SetCanMove(true);
                }
            }
        }
        
        /// <summary>
        /// 显示没有镐子时的提示对话
        /// </summary>
        private void ShowNoPickaxeDialogue()
        {
            // 暂停玩家移动
            if (nearbyPlayer != null)
            {
                nearbyPlayer.SetCanMove(false);
            }
            
            List<DialogueData> noPickaxeDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle(entranceName, noPickaxeMessage),
                DialogueSystem.CreateInteractionHint(noPickaxeHint, 3f)
            };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(noPickaxeDialogue, () => {
                    // 恢复玩家移动
                    if (nearbyPlayer != null)
                    {
                        nearbyPlayer.SetCanMove(true);
                    }
                });
            }
            else
            {
                // 如果DialogueSystem不可用，直接恢复玩家移动
                if (nearbyPlayer != null)
                {
                    nearbyPlayer.SetCanMove(true);
                }
            }
        }
        
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
        
        /// <summary>
        /// 检查是否有镐子（通过Stage5Controller）
        /// </summary>
        public bool HasPickaxe()
        {
            return stage5Controller != null && stage5Controller.HasPickaxe();
        }
        
        /// <summary>
        /// 开始挖矿（通过Stage5Controller）
        /// </summary>
        public void StartMining()
        {
            if (stage5Controller != null)
            {
                stage5Controller.StartMiningQuestion();
            }
        }
    }
}