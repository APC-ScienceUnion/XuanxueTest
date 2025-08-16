using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace XuanZhiShiLian
{
    /// <summary>
    /// 哨兵A专用交互类 - 支持手动交互（按E键）、动态选项、状态切换
    /// </summary>
    public class SentryGuardInteractable : DialogueInteractable
    {
        [Header("哨兵设置")]
        public string sentryId = "哨兵A";
        public int requiredFakePassportId = 1001; // 假护照的物品ID
        public int bribeAmount = 30; // 贿赂金额
        
        [Header("状态控制")]
        public bool hasBeenBribed = false; // 是否已被贿赂
        public bool blockPlayer = true; // 是否阻拦玩家
        
        private Stage5Controller stage5Controller;
        
        protected override void Start()
        {
            base.Start();
            stage5Controller = FindObjectOfType<Stage5Controller>();
        }
        

        
        /// <summary>
        /// 触发哨兵交互
        /// </summary>
        private void TriggerSentryInteraction()
        {
            if (hasBeenBribed) return;
            
            // 暂停玩家移动
            if (nearbyPlayer != null)
            {
                nearbyPlayer.SetCanMove(false);
            }
            
            // 显示哨兵的检查对话
            ShowPassportCheckDialogue();
        }
        
        /// <summary>
        /// 显示护照检查对话
        /// </summary>
        private void ShowPassportCheckDialogue()
        {
            List<DialogueData> checkDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle("哨兵A", "请出示护照。")
            };
            
            // 创建动态选项
            List<DialogueOption> options = CreateDynamicOptions();
            
            // 如果有选项，添加到对话中
            if (options.Count > 0)
            {
                DialogueData optionDialogue = DialogueSystem.CreateDialogueWithOptions(
                    "无名勇者",
                    "...", // 玩家思考
                    options
                );
                checkDialogue.Add(optionDialogue);
            }
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(checkDialogue, OnPassportCheckComplete);
            }
            else
            {
                OnPassportCheckComplete();
            }
        }
        
        /// <summary>
        /// 创建动态选项（根据是否持有假护照）
        /// </summary>
        private List<DialogueOption> CreateDynamicOptions()
        {
            List<DialogueOption> options = new List<DialogueOption>();
            
            // 检查是否持有假护照
            bool hasFakePassport = CheckHasFakePassport();
            
            // 如果有假护照，添加"出示假护照"选项
            if (hasFakePassport)
            {
                options.Add(DialogueSystem.CreateOption("出示假护照", () => {
                    OnFakePassportShown();
                }));
            }
            
            // 总是有"离开"选项
            options.Add(DialogueSystem.CreateOption("离开", () => {
                OnPlayerLeaves();
            }));
            
            return options;
        }
        
        /// <summary>
        /// 检查玩家是否持有假护照
        /// </summary>
        private bool CheckHasFakePassport()
        {
            if (InventorySystem.Instance != null)
            {
                for (int i = 0; i < InventorySystem.Instance.maxSlots; i++)
                {
                    Item item = InventorySystem.Instance.GetItem(i);
                    if (!item.IsEmpty && item.id == requiredFakePassportId)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        
        /// <summary>
        /// 出示假护照后的对话
        /// </summary>
        private void OnFakePassportShown()
        {
            List<DialogueData> fakePassportDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle("哨兵A", "（狐疑的眼神）远道而来的客人，请准备好足够的本国货币。"),
                DialogueSystem.CreateSubtitle("莉姆莉卡", "谢谢提醒，能够放我们过去了吗？"),
                DialogueSystem.CreateSubtitle("哨兵A", "（拦住）远道而来的客人，请准备好足够的本国货币。"),
                DialogueSystem.CreateSubtitle("哨兵A", "（伸出三根手指敲了敲护照）")
            };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(fakePassportDialogue, () => {
                    CheckBribeCondition();
                });
            }
            else
            {
                CheckBribeCondition();
            }
        }
        
        /// <summary>
        /// 检查贿赂条件（是否有足够金钱）
        /// </summary>
        private void CheckBribeCondition()
        {
            int playerMoney = stage5Controller != null ? stage5Controller.money : 0;
            
            if (playerMoney >= bribeAmount)
            {
                // 金钱足够，成功贿赂
                ShowSuccessfulBribe();
            }
            else
            {
                // 金钱不足，失败
                ShowInsufficientFunds();
            }
        }
        
        /// <summary>
        /// 贿赂成功的对话
        /// </summary>
        private void ShowSuccessfulBribe()
        {
            List<DialogueData> successDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle("无名勇者", "（偷偷塞了30G给哨兵）我们准备好了。"),
                DialogueSystem.CreateSubtitle("哨兵A", "欢迎您来到中央精灵领，远道而来的客人。")
            };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(successDialogue, OnBribeSuccessComplete);
            }
            else
            {
                OnBribeSuccessComplete();
            }
        }
        
        /// <summary>
        /// 贿赂成功后的处理
        /// </summary>
        private void OnBribeSuccessComplete()
        {
            // 扣除金钱
            if (stage5Controller != null)
            {
                stage5Controller.ModifyAttribute("money", -bribeAmount);
                
                // 一次性通过奖励：爱欲值+1 真理值+1
                stage5Controller.ModifyAttribute("lovedesire", 1);
                stage5Controller.ModifyAttribute("truth", 1);
            }
            
            // 改变状态：不再阻拦玩家
            hasBeenBribed = true;
            blockPlayer = false;
            
            // 恢复玩家移动
            if (nearbyPlayer != null)
            {
                nearbyPlayer.SetCanMove(true);
            }
            
            // 通知Stage5Controller
            if (stage5Controller != null)
            {
                stage5Controller.OnNPCInteractionComplete(sentryId);
            }
            
            // 隐藏哨兵的所有子物体
            HideSentryVisuals();
            
            // 禁用触发器，避免重复触发
            GetComponent<Collider>().enabled = false;
        }
        
        /// <summary>
        /// 金钱不足的对话
        /// </summary>
        private void ShowInsufficientFunds()
        {
            List<DialogueData> insufficientDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle("哨兵A", "请准备好之后再来。")
            };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(insufficientDialogue, OnInsufficientFundsComplete);
            }
            else
            {
                OnInsufficientFundsComplete();
            }
        }
        
        /// <summary>
        /// 金钱不足对话完成后的处理
        /// </summary>
        private void OnInsufficientFundsComplete()
        {
            // 恢复玩家移动
            if (nearbyPlayer != null)
            {
                nearbyPlayer.SetCanMove(true);
            }
            
            // 玩家需要重新准备金钱后再来
            // 保持触发器状态，允许重新触发
        }
        
        /// <summary>
        /// 玩家选择离开
        /// </summary>
        private void OnPlayerLeaves()
        {
            List<DialogueData> leaveDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle("无名勇者", "看来得想想办法")
            };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(leaveDialogue, OnPlayerLeaveComplete);
            }
            else
            {
                OnPlayerLeaveComplete();
            }
        }
        
        /// <summary>
        /// 玩家离开完成
        /// </summary>
        private void OnPlayerLeaveComplete()
        {
            // 恢复玩家移动
            if (nearbyPlayer != null)
            {
                nearbyPlayer.SetCanMove(true);
            }
        }
        
        /// <summary>
        /// 护照检查对话完成
        /// </summary>
        private void OnPassportCheckComplete()
        {
            // 如果没有进行其他选择，恢复玩家移动
            if (nearbyPlayer != null)
            {
                nearbyPlayer.SetCanMove(true);
            }
        }
        
        /// <summary>
        /// 重置哨兵状态（用于测试或重新开始）
        /// </summary>
        public void ResetSentryState()
        {
            hasBeenBribed = false;
            blockPlayer = true;
            GetComponent<Collider>().enabled = true;
            
            // 恢复哨兵的视觉显示
            ShowSentryVisuals();
        }
        
        /// <summary>
        /// 显示哨兵的所有子物体
        /// </summary>
        private void ShowSentryVisuals()
        {
            // 显示所有子物体
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                child.gameObject.SetActive(true);
            }
            
            // 恢复自身的渲染组件
            Renderer[] renderers = GetComponents<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = true;
            }
        }
        
        /// <summary>
        /// 检查哨兵是否已被贿赂
        /// </summary>
        public bool IsPassable()
        {
            return hasBeenBribed;
        }
        
        /// <summary>
        /// 隐藏哨兵的所有子物体
        /// </summary>
        private void HideSentryVisuals()
        {
            // 隐藏所有子物体
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                child.gameObject.SetActive(false);
            }
            
            // 也可以选择隐藏自身的渲染组件（如果有的话）
            Renderer[] renderers = GetComponents<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = false;
            }
        }
        
        // 重写基类的Interact方法，只支持手动交互
        public override void Interact()
        {
            // 如果已被贿赂，不再交互
            if (hasBeenBribed) return;
            
            // 使用基类的playerInRange检查
            if (!playerInRange) return;
            
            // 手动触发哨兵交互
            TriggerSentryInteraction();
        }
    }
}