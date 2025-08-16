using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace XuanZhiShiLian
{
    /// <summary>
    /// 海子NPC专用交互脚本
    /// 支持条件选项显示和物品移除功能
    /// </summary>
    public class HaiziNPCInteractable : DialogueInteractable
    {
        [Header("海子NPC设置")]
        public string npcId = "海子";
        
        [Header("初次对话内容")]
        [TextArea(3, 10)]
        public string[] initialDialogueLines = {
            "海子：我是海之子！",
            "海子：你们要见国王就要在我这里打一个漂亮的水漂！"
        };
        
        [Header("动画设置")]
        public string animationName = "Stage5-5-3";
        
        [Header("物品ID设置")]
        public int xuanwuItemId = 6001; // 魔将玄武
        public int lalaiyeItemId = 8001; // 拉莱耶碎砖
        
        [Header("后续对话内容")]
        [TextArea(2, 5)]
        public string[] commonFollowUpDialogue = {
            "海子：不过就算如此，你也不能去见国王，我们都是活的死物，不可入大殿的。",
            "魔将玄武：要是我非要进去怎么办？",
            "海子：大概会变成真的死物吧？",
            "魔将玄武：那……还是你们两进去吧。",
            "魔将玄武：这个地方我哪怕自由移动应该也不会显得奇怪。",
            "海子：国王大人等你们很久了。"
        };
        
        private Stage5Controller stage5Controller;
        private bool hasInteractedOnce = false;
        
        protected override void Start()
        {
            base.Start();
            stage5Controller = FindObjectOfType<Stage5Controller>();
            
            // 设置初始对话内容
            if (initialDialogueLines != null && initialDialogueLines.Length > 0)
            {
                dialogueLines = initialDialogueLines;
            }
        }
        
        public override void Interact()
        {
            if (!playerInRange || hasInteractedOnce) return;
            
            // 暂停玩家移动
            if (nearbyPlayer != null)
            {
                nearbyPlayer.SetCanMove(false);
            }
            
            // 显示初始对话带选项
            ShowInitialDialogueWithOptions();
        }
        
        /// <summary>
        /// 显示带选项的初始对话
        /// </summary>
        private void ShowInitialDialogueWithOptions()
        {
            // 构建对话数据
            List<DialogueData> dialogueData = BuildDialogueData(initialDialogueLines);
            
            // 添加选项到最后一条对话
            if (dialogueData.Count > 0)
            {
                AddOptionsToLastDialogue(dialogueData);
            }
            
            // 显示对话
            if (DialogueSystem.Instance != null && dialogueData.Count > 0)
            {
                DialogueSystem.Instance.StartDialogue(dialogueData, OnInitialDialogueComplete);
            }
            else
            {
                OnInitialDialogueComplete();
            }
        }
        
        /// <summary>
        /// 构建对话数据
        /// </summary>
        private List<DialogueData> BuildDialogueData(string[] lines)
        {
            List<DialogueData> dialogueData = new List<DialogueData>();
            
            foreach (string line in lines)
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
            
            return dialogueData;
        }
        
        /// <summary>
        /// 添加选项到最后一条对话
        /// </summary>
        private void AddOptionsToLastDialogue(List<DialogueData> dialogueData)
        {
            if (dialogueData.Count == 0) return;
            
            // 获取最后一条对话
            DialogueData lastDialogue = dialogueData[dialogueData.Count - 1];
            
            // 创建选项列表
            List<DialogueOption> options = new List<DialogueOption>();
            
            // 选项1：使用魔将玄武（总是显示）
            options.Add(new DialogueOption(
                "使用魔将玄武",
                () => OnXuanwuOptionSelected()
            ));
            
            // 选项2：拉莱耶碎砖（只有持有时才显示）
            if (HasItemInInventory(lalaiyeItemId))
            {
                options.Add(new DialogueOption(
                    "拉莱耶碎砖",
                    () => OnLalaiyeOptionSelected()
                ));
            }
            
            // 添加选项到对话中
            lastDialogue.options = options;
        }
        
        /// <summary>
        /// 检查背包中是否有指定物品
        /// </summary>
        private bool HasItemInInventory(int itemId)
        {
            if (InventorySystem.Instance != null)
            {
                for (int i = 0; i < InventorySystem.Instance.maxSlots; i++)
                {
                    Item item = InventorySystem.Instance.GetItem(i);
                    if (!item.IsEmpty && item.id == itemId)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        
        /// <summary>
        /// 从背包中移除指定物品
        /// </summary>
        private bool RemoveItemFromInventory(int itemId)
        {
            if (InventorySystem.Instance != null)
            {
                for (int i = 0; i < InventorySystem.Instance.maxSlots; i++)
                {
                    Item item = InventorySystem.Instance.GetItem(i);
                    if (!item.IsEmpty && item.id == itemId)
                    {
                        InventorySystem.Instance.RemoveItem(i);
                        return true;
                    }
                }
            }
            return false;
        }
        
        /// <summary>
        /// 选择魔将玄武选项
        /// </summary>
        private void OnXuanwuOptionSelected()
        {
            
            // 移除魔将玄武
            RemoveItemFromInventory(xuanwuItemId);
            
            // 显示魔将玄武的台词，然后直接完成交互
            string[] xuanwuDialogue = { "魔将玄武：啊，不要啊！我还不会游泳！" };
            
            ShowFollowUpDialogue(xuanwuDialogue, () => {
                // 魔将玄武台词结束后，直接播放动画完成交互
                PlayFinalAnimation();
            });
        }
        
        /// <summary>
        /// 选择拉莱耶碎砖选项
        /// </summary>
        private void OnLalaiyeOptionSelected()
        {
            
            // 移除拉莱耶碎砖
            RemoveItemFromInventory(lalaiyeItemId);
            
            // 显示魔将玄武的台词，然后继续共同后续对话
            string[] xuanwuDialogue = { "魔将玄武：我就知道你们是不会把我丢掉的。" };
            
            ShowFollowUpDialogue(xuanwuDialogue, () => {
                // 魔将玄武台词结束后，显示共同后续对话
                ShowCommonFollowUpDialogue();
            });
        }
        
        /// <summary>
        /// 显示共同的后续对话（仅拉莱耶碎砖选项使用）
        /// </summary>
        private void ShowCommonFollowUpDialogue()
        {
            ShowFollowUpDialogue(commonFollowUpDialogue, () => {
                // 共同后续对话结束后，移除魔将玄武并播放动画
                RemoveItemFromInventory(xuanwuItemId);
                PlayFinalAnimation();
            });
        }
        
        /// <summary>
        /// 显示后续对话
        /// </summary>
        private void ShowFollowUpDialogue(string[] followUpLines, System.Action onComplete = null)
        {
            List<DialogueData> followUpData = BuildDialogueData(followUpLines);
            
            if (DialogueSystem.Instance != null && followUpData.Count > 0)
            {
                DialogueSystem.Instance.StartDialogue(followUpData, onComplete);
            }
            else
            {
                onComplete?.Invoke();
            }
        }
        
        /// <summary>
        /// 播放最终动画
        /// </summary>
        private void PlayFinalAnimation()
        {
            if (!string.IsNullOrEmpty(animationName))
            {
                StartCoroutine(PlayAnimationSequence());
            }
            else
            {
                OnInteractionComplete();
            }
        }
        
        /// <summary>
        /// 播放动画序列
        /// </summary>
        private IEnumerator PlayAnimationSequence()
        {
            
            // 通过Stage5Controller播放动画
            if (stage5Controller != null && stage5Controller.transitionAnimator != null)
            {
                stage5Controller.transitionAnimator.Play(animationName);
                
                // 等待动画播放完成
                yield return StartCoroutine(WaitForAnimationComplete());
            }
            else
            {
                // 等待3秒作为默认动画时长
                yield return new WaitForSeconds(3f);
            }
            
            OnInteractionComplete();
        }
        
        /// <summary>
        /// 等待指定动画播放完成
        /// </summary>
        private IEnumerator WaitForAnimationComplete()
        {
            if (stage5Controller == null || stage5Controller.transitionAnimator == null)
                yield break;
                
            Animator animator = stage5Controller.transitionAnimator;
            
            // 等待动画开始播放
            yield return new WaitForEndOfFrame();
            
            // 获取动画长度
            float animationTime = 3f; // 默认时长
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName(animationName))
            {
                animationTime = stateInfo.length;
            }
            
            
            // 等待动画播放完成
            float elapsedTime = 0f;
            while (elapsedTime < animationTime)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // 确保动画完全播放完成
            yield return new WaitForEndOfFrame();
            
        }
        
        /// <summary>
        /// 初始对话完成回调
        /// </summary>
        private void OnInitialDialogueComplete()
        {
            // 如果没有选择任何选项就结束了对话，恢复玩家移动
            if (nearbyPlayer != null)
            {
                nearbyPlayer.SetCanMove(true);
            }
        }
        
        /// <summary>
        /// 交互完成
        /// </summary>
        private void OnInteractionComplete()
        {
            // 恢复玩家移动
            if (nearbyPlayer != null)
            {
                nearbyPlayer.SetCanMove(true);
            }
            
            // 标记为已交互
            hasInteractedOnce = true;
            
            // 通知Stage5Controller
            if (stage5Controller != null)
            {
                stage5Controller.OnNPCInteractionComplete(npcId);
            }
            
            // 处理removeAfterTalk逻辑
            if (removeAfterTalk)
            {
                isInteractable = false;
                SetHighlight(false);
                Invoke(nameof(HideObject), 1f);
            }
            
        }
        
        /// <summary>
        /// 隐藏物体（延迟调用）
        /// </summary>
        private void HideObject()
        {
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// 重置NPC状态（用于测试或重新开始）
        /// </summary>
        public void ResetNPCState()
        {
            hasInteractedOnce = false;
            isInteractable = true;
            gameObject.SetActive(true);
        }
        
        /// <summary>
        /// 检查是否已交互
        /// </summary>
        public bool HasInteracted()
        {
            return hasInteractedOnce;
        }
    }
}