using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace XuanZhiShiLian
{
    /// <summary>
    /// 条件分支对话交互组件
    /// 根据Stage5Controller的条件值显示不同的对话分支
    /// </summary>
    public class ConditionalDialogueInteractable : Interactable
    {
        [Header("基本设置")]
        public string npcId = ""; // NPC唯一标识
        public string speakerName = "NPC"; // 说话者名称
        
        [Header("初始对话")]
        [TextArea(3, 10)]
        public string[] initialDialogue; // 初始对话（条件判断前）
        
        [Header("条件设置")]
        public string stage5ConditionName = ""; // 条件名称（如 "canTalkWithQika"）
        public bool expectedConditionValue = true; // 期望的条件值
        
        [Header("条件为True时的对话")]
        [TextArea(5, 15)]
        public string[] conditionTrueDialogue; // 条件为true时的对话
        
        [Header("条件为False时的对话")]
        [TextArea(3, 10)]
        public string[] conditionFalseDialogue; // 条件为false时的对话
        
        [Header("动画设置")]
        public bool playAnimationAfterTrueBranch = false; // true分支完成后是否播放动画
        public string trueBranchAnimationName = ""; // true分支的动画名称
        
        [Header("交互设置")]
        public bool canRepeatInteraction = false; // 是否可以重复交互
        
        private Stage5Controller stage5Controller;
        private PlayerController nearbyPlayer;
        private bool hasInteractedOnce = false;
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
                interactionText = "按E对话";
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
        
        public override void Interact()
        {
            if (!playerInRange) return;
            
            // 检查是否可以重复交互
            if (hasInteractedOnce && !canRepeatInteraction)
            {
                return;
            }
            
            // 开始对话序列
            StartCoroutine(StartDialogueSequence());
        }
        
        private IEnumerator StartDialogueSequence()
        {
            // 暂停玩家移动
            if (nearbyPlayer != null)
            {
                nearbyPlayer.SetCanMove(false);
            }
            
            // 1. 显示初始对话
            if (initialDialogue != null && initialDialogue.Length > 0)
            {
                yield return StartCoroutine(ShowDialogue(initialDialogue, null));
            }
            
            // 2. 检查条件并显示对应分支
            bool conditionMet = CheckStage5Condition();
            bool shouldShowTrueBranch = (conditionMet == expectedConditionValue);
            
            if (shouldShowTrueBranch && conditionTrueDialogue != null && conditionTrueDialogue.Length > 0)
            {
                yield return StartCoroutine(ShowDialogue(conditionTrueDialogue, () => OnTrueBranchComplete()));
            }
            else if (!shouldShowTrueBranch && conditionFalseDialogue != null && conditionFalseDialogue.Length > 0)
            {
                yield return StartCoroutine(ShowDialogue(conditionFalseDialogue, () => OnFalseBranchComplete()));
            }
            else
            {
                // 没有匹配的分支，直接完成交互
                OnDialogueComplete();
            }
        }
        
        private IEnumerator ShowDialogue(string[] dialogueLines, System.Action onComplete)
        {
            List<DialogueData> dialogueData = BuildDialogueData(dialogueLines);
            
            bool dialogueFinished = false;
            System.Action completionCallback = () => {
                dialogueFinished = true;
                onComplete?.Invoke();
            };
            
            if (DialogueSystem.Instance != null && dialogueData.Count > 0)
            {
                DialogueSystem.Instance.StartDialogue(dialogueData, completionCallback);
                yield return new WaitUntil(() => dialogueFinished);
            }
            else
            {
                completionCallback?.Invoke();
            }
        }
        
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
                            string speaker = parts[0].Trim().Replace("【", "").Replace("】", "");
                            string text = parts[1].Trim();
                            dialogueData.Add(DialogueSystem.CreateSubtitle(speaker, text));
                        }
                    }
                    else
                    {
                        // 没有说话者标记，使用默认说话者
                        dialogueData.Add(DialogueSystem.CreateSubtitle(speakerName, trimmedLine));
                    }
                }
            }
            
            return dialogueData;
        }
        
        private bool CheckStage5Condition()
        {
            if (stage5Controller == null)
            {
                return false;
            }
            
            switch (stage5ConditionName.ToLower())
            {
                case "cantalkwithqika":
                    return stage5Controller.canTalkWithQika;
                case "money":
                    // 可以扩展其他条件，比如金钱检查
                    return stage5Controller.money > 0;
                case "goodevil":
                    return stage5Controller.goodEvilValue > 0;
                case "truth":
                    return stage5Controller.truthValue > 0;
                case "lovedesire":
                    return stage5Controller.loveDesireValue > 0;
                default:
                    return false;
            }
        }
        
        private void OnTrueBranchComplete()
        {         
            // 如果需要播放动画
            if (playAnimationAfterTrueBranch && !string.IsNullOrEmpty(trueBranchAnimationName))
            {
                StartCoroutine(PlayAnimationSequence());
            }
            else
            {
                OnDialogueComplete();
            }
        }
        
        private void OnFalseBranchComplete()
        {
            OnDialogueComplete();
        }
        
        private IEnumerator PlayAnimationSequence()
        {
            
            // 通过Stage5Controller播放动画
            if (stage5Controller != null && stage5Controller.transitionAnimator != null)
            {
                stage5Controller.transitionAnimator.Play(trueBranchAnimationName);
                
                // 等待动画播放完成
                yield return StartCoroutine(WaitForAnimationComplete());
            }
            
            OnDialogueComplete();
        }
        
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
            if (stateInfo.IsName(trueBranchAnimationName))
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
            
            yield return new WaitForEndOfFrame();
        }
        
        private void OnDialogueComplete()
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
            
        }
        
        // 公共方法：重置交互状态
        public void ResetInteractionState()
        {
            hasInteractedOnce = false;
            isInteractable = true;
        }
        
        // 公共方法：检查是否已交互
        public bool HasInteracted()
        {
            return hasInteractedOnce;
        }
    }
}