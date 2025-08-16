using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace XuanZhiShiLian
{
    // Stage5专用的NPC交互类，支持复杂对话和属性修改
    public class Stage5NPCInteractable : DialogueInteractable
    {
        [Header("Stage5 NPC设置")]
        public string npcId = ""; // NPC唯一标识
        public bool hasInitialDialogue = true; // 是否有初次对话
        public bool hasRepeatDialogue = true; // 是否有重复对话
        public bool alwaysShowOptions = false; // 是否每次都显示选项（用于船夫等需要重复选择的NPC）

        
        [Header("条件检查设置")]
        public bool requiresSpecificItem = false; // 是否需要特定物品才能对话
        public int requiredItemId = 0; // 需要的物品ID
        public string conditionFailMessage = ""; // 条件不满足时的提示信息
        

        
        [Header("初次对话设置")]
        [TextArea(3, 10)]
        public string[] initialDialogueLines; // 初次对话内容
        
        [Header("重复对话设置")]
        [TextArea(2, 5)]
        public string[] repeatDialogueLines; // 重复对话内容
        

        
        [Header("选项设置")]
        public DialogueOptionData[] dialogueOptions; // 对话选项
        
        [Header("属性奖励设置")]
        public AttributeReward[] attributeRewards; // 属性奖励
        
        private Stage5Controller stage5Controller;
        private bool hasInteractedOnce = false;
        
        // 已购买的选项追踪（使用选项文本作为标识）
        private HashSet<string> purchasedOptions = new HashSet<string>();
        // 被禁用（不再显示）的选项
        private HashSet<string> disabledOptions = new HashSet<string>();
        
        [System.Serializable]
        public class DialogueOptionData
        {
            public string optionText;
            public int goodEvilChange;
            public int truthChange;
            public int loveDesireChange;
            public int moneyChange;
            [TextArea(2, 5)]
            public string[] followUpDialogue; // 选择后的后续对话
            
            [Header("购买功能（可选）")]
            public bool isPurchaseOption = false; // 是否是购买选项
            public bool isService = false; // 是否是服务（不产生实际物品）
            public int itemId = 0; // 物品ID（如果isService为true则忽略此字段）
            public int price = 0; // 价格（0表示免费）
            
            [Header("动画播放功能（可选）")]
            public bool shouldPlayAnimation = false; // 是否播放动画
            public string animationName = ""; // 动画名称
            
            [Header("特殊功能（可选）")]
            public bool isMiningOption = false; // 是否是挖矿选项
            
            [Header("问题触发功能（可选）")]
            public bool isQuestionOption = false; // 是否是问题触发选项
            public int questionId = 0; // 要触发的问题ID
            public int rewardItemId = 0; // 答对后获得的道具ID
            public string[] correctAnswerFollowUp; // 答对后的后续对话
            public string[] wrongAnswerFollowUp; // 答错后的后续对话
            
            [Header("自定义回调功能（可选）")]
            public UnityEvent onFollowUpDialogueComplete; // 后续对话完成时的回调事件
        }
        
        [System.Serializable]
        public class AttributeReward
        {
            public string attributeName; // "goodevil", "truth", "lovedesire", "money"
            public int value;
            public bool onFirstInteraction = true; // 是否只在首次交互时给予
        }
        
        protected override void Start()
        {
            base.Start();
            stage5Controller = FindObjectOfType<Stage5Controller>();
            
            // 如果有初次对话，将其设置为默认对话
            if (hasInitialDialogue && initialDialogueLines != null && initialDialogueLines.Length > 0)
            {
                dialogueLines = initialDialogueLines;
            }
        }
        
        public override void Interact()
        {
            if (!playerInRange) return;
            
            // 条件检查：是否需要特定物品
            if (requiresSpecificItem)
            {
                if (!CheckRequiredItemCondition())
                {
                    ShowConditionFailDialogue();
                    return;
                }
            }
            

            

            
            // 如果设置了总是显示选项，则每次都显示初次对话
            if (alwaysShowOptions && hasInitialDialogue)
            {
                ShowInitialDialogue();
            }
            // 根据是否已交互过来选择对话内容
            else if (!hasInteractedOnce && hasInitialDialogue)
            {
                ShowInitialDialogue();
            }
            else if (hasRepeatDialogue && repeatDialogueLines != null && repeatDialogueLines.Length > 0)
            {
                ShowRepeatDialogue();
            }
            else
            {
                // 使用默认的对话处理
                base.Interact();
            }
        }
        
        private void ShowInitialDialogue()
        {
            if (initialDialogueLines == null || initialDialogueLines.Length == 0)
            {
                return;
            }
            
            // 暂停玩家移动
            if (nearbyPlayer != null)
            {
                nearbyPlayer.SetCanMove(false);
            }
            
            // 构建对话数据
            List<DialogueData> dialogueData = BuildDialogueData(initialDialogueLines);
            
            // 如果有选项，添加到最后一条对话中
            if (dialogueOptions != null && dialogueOptions.Length > 0)
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
        
        private void ShowRepeatDialogue()
        {
            // 暂停玩家移动
            if (nearbyPlayer != null)
            {
                nearbyPlayer.SetCanMove(false);
            }
            
            // 构建重复对话数据
            List<DialogueData> dialogueData = BuildDialogueData(repeatDialogueLines);
            
            // 显示对话
            if (DialogueSystem.Instance != null && dialogueData.Count > 0)
            {
                DialogueSystem.Instance.StartDialogue(dialogueData, OnRepeatDialogueComplete);
            }
            else
            {
                OnRepeatDialogueComplete();
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
        
        private void AddOptionsToLastDialogue(List<DialogueData> dialogueData)
        {
            if (dialogueData.Count == 0) return;
            
            // 获取最后一条对话
            DialogueData lastDialogue = dialogueData[dialogueData.Count - 1];
            
            // 创建选项列表，只显示未购买的选项
            List<DialogueOption> options = new List<DialogueOption>();
            
            for (int i = 0; i < dialogueOptions.Length; i++)
            {
                int optionIndex = i; // 闭包变量
                DialogueOptionData optionData = dialogueOptions[i];
                
                // 如果该选项被禁用（例如某题正确回答后不再显示），跳过
                if (disabledOptions.Contains(optionData.optionText))
                {
                    continue;
                }

                // 如果是购买选项且已购买，则跳过
                if (optionData.isPurchaseOption && purchasedOptions.Contains(optionData.optionText))
                {
                    continue;
                }
                
                // 如果是问题选项且为第81题，且已经正确作答，则不再显示该选项
                if (optionData.isQuestionOption && optionData.questionId == 81 && QuestionSystem.Instance != null)
                {
                    var question81 = QuestionSystem.Instance.GetQuestionById(optionData.questionId);
                    if (question81 != null && question81.isAnswered)
                    {
                        bool answeredCorrectly = QuestionSystem.Instance.IsAnswerCorrect(question81.playerAnswer, question81.correctAnswer);
                        if (answeredCorrectly)
                        {
                            continue;
                        }
                    }
                }
                
                DialogueOption option = new DialogueOption(
                    optionData.optionText,
                    () => OnOptionSelected(optionIndex)
                );
                
                options.Add(option);
            }
            
            // 只有在有可用选项时才添加到对话中
            if (options.Count > 0)
            {
                lastDialogue.options = options;
            }
        }
        
        private void OnOptionSelected(int optionIndex)
        {
            if (optionIndex < 0 || optionIndex >= dialogueOptions.Length) return;
            
            DialogueOptionData selectedOption = dialogueOptions[optionIndex];
            
            // 记录Stage5选择到存档系统
            SaveSystem.RecordStage5Choice(
                npcId, 
                selectedOption.optionText, 
                optionIndex,
                selectedOption.goodEvilChange,
                selectedOption.truthChange,
                selectedOption.loveDesireChange,
                selectedOption.moneyChange
            );
            
            // 修改属性
            if (stage5Controller != null)
            {
                if (selectedOption.goodEvilChange != 0)
                    stage5Controller.ModifyAttribute("goodevil", selectedOption.goodEvilChange);
                    
                if (selectedOption.truthChange != 0)
                    stage5Controller.ModifyAttribute("truth", selectedOption.truthChange);
                    
                if (selectedOption.loveDesireChange != 0)
                    stage5Controller.ModifyAttribute("lovedesire", selectedOption.loveDesireChange);
                    
                if (selectedOption.moneyChange != 0)
                    stage5Controller.ModifyAttribute("money", selectedOption.moneyChange);
            }
            
            // 处理选项的后续逻辑
            HandleOptionFollowUp(selectedOption);
        }
        
        /// <summary>
        /// 获取物品名称（使用ItemDatabase）
        /// </summary>
        private string GetItemName(DialogueOptionData option)
        {
            // 兜底：数据库可用且物品存在时返回正式名称；
            // 否则回退到选项文本或“未知物品”。
            if (option == null)
            {
                return "未知物品";
            }
            if (ItemDatabase.Instance != null && ItemDatabase.Instance.ItemExists(option.itemId))
            {
                return ItemDatabase.Instance.GetItemName(option.itemId);
            }
            if (!string.IsNullOrEmpty(option.optionText))
            {
                return option.optionText;
            }
            return "未知物品";
        }
        
        /// <summary>
        /// 获取物品价格（使用ItemDatabase）
        /// </summary>
        private int GetItemPrice(DialogueOptionData option)
        {
            // 兜底：数据库可用且物品存在时取数据库价格；否则采用配置的price；并强制非负。
            if (option == null)
            {
                return 0;
            }
            int resolvedPrice = 0;
            if (ItemDatabase.Instance != null && ItemDatabase.Instance.ItemExists(option.itemId))
            {
                resolvedPrice = ItemDatabase.Instance.GetItemPrice(option.itemId);
            }
            else
            {
                resolvedPrice = option.price;
            }
            return Mathf.Max(0, resolvedPrice);
        }
        
        /// <summary>
        /// 处理选项选择的后续逻辑
        /// </summary>
        private void HandleOptionFollowUp(DialogueOptionData selectedOption)
        {
            // 处理购买逻辑（如果是购买选项）
            if (selectedOption.isPurchaseOption)
            {
                HandlePurchaseInFollowUp(selectedOption);
                // 购买选项的后续逻辑（包括动画播放）都在HandlePurchaseInFollowUp中处理
                // 不应该在这里再次处理动画播放
                return;
            }
            
            // 处理挖矿逻辑（如果是挖矿选项）
            if (selectedOption.isMiningOption)
            {
                HandleMiningOption();
                return;
            }
            
            // 处理问题触发逻辑（如果是问题选项）
            if (selectedOption.isQuestionOption)
            {
                HandleQuestionOption(selectedOption);
                return;
            }
            
            // 处理动画播放（如果需要播放动画）
            if (selectedOption.shouldPlayAnimation && !string.IsNullOrEmpty(selectedOption.animationName))
            {
                PlayAnimationAfterDialogue(selectedOption);
                return;
            }
            
            // 延迟显示后续对话，避免与DialogueSystem的ShowNextDialogue冲突
            if (selectedOption.followUpDialogue != null && selectedOption.followUpDialogue.Length > 0)
            {
                StartCoroutine(DelayedFollowUpDialogue(selectedOption));
            }
            else
            {
                // 没有后续对话，延迟执行回调
                StartCoroutine(DelayedCallback(selectedOption));
            }
            
            // 特殊处理：如果是魔王女儿的称呼选择
            if (npcId == "魔王女儿" && selectedOption.optionText.Contains("称呼"))
            {
                // 修改说话者名称为"魔族公主"
                speakerName = "魔族公主";
                if (stage5Controller != null)
                {
                    stage5Controller.SetNPCName("魔王女儿", "魔族公主");
                }
            }
        }
        
        private void HandlePurchaseInFollowUp(DialogueOptionData purchaseOption)
        {
            // 获取名称和价格
            string itemName;
            int itemPrice;
            
            if (purchaseOption.isService)
            {
                // 服务：使用选项文本作为服务名称，使用配置的价格
                itemName = purchaseOption.optionText;
                itemPrice = purchaseOption.price;
            }
            else
            {
                // 物品：使用ItemDatabase，如果没有则使用配置的价格
                itemName = GetItemName(purchaseOption);
                itemPrice = GetItemPrice(purchaseOption);
            }
            // 检查金钱是否足够
            int playerMoney = stage5Controller != null ? stage5Controller.money : 0;
            
            if (playerMoney < itemPrice)
            {
                // 金钱不足，显示提示对话（下一帧再开启，避免与当前对话同帧竞争）
                List<DialogueData> insufficientFundsDialogue = new List<DialogueData>
                {
                    DialogueSystem.CreateSubtitle(speakerName, $"你的钱不够买{itemName}，需要{itemPrice}G。")
                };
                StartCoroutine(StartDialogueNextFrame(insufficientFundsDialogue, OnFollowUpDialogueComplete));
                return;
            }
            
            // 金钱足够，显示确认购买对话
            if (purchaseOption.followUpDialogue != null && purchaseOption.followUpDialogue.Length > 0)
            {
                // 下一帧开始后续对话，避免与当前对话同帧竞争
                StartCoroutine(DeferredFollowUpThenPurchase(purchaseOption));
            }
            else
            {
                // 下一帧显示确认购买对话
                StartCoroutine(DeferredShowDefaultPurchaseConfirmation(purchaseOption));
            }
        }
        
        private void ShowDefaultPurchaseConfirmation(DialogueOptionData purchaseOption)
        {
            string itemName;
            if (purchaseOption.isService)
            {
                // 服务：使用选项文本作为服务名称，使用配置的价格
                itemName = purchaseOption.optionText;
            }
            else
            {
                // 物品：使用ItemDatabase，如果没有则使用配置的价格
                itemName = GetItemName(purchaseOption);
            }
            
            // 创建确认选项
            List<DialogueOption> confirmOptions = new List<DialogueOption>
            {
                DialogueSystem.CreateOption("确定", () => {
                    // 延迟执行购买逻辑，确保当前对话完全结束
                    StartCoroutine(DelayedPurchaseAction(purchaseOption, true));
                }),
                DialogueSystem.CreateOption("算了", () => {
                    // 延迟执行取消逻辑，确保当前对话完全结束
                    StartCoroutine(DelayedPurchaseAction(purchaseOption, false));
                })
            };
            
            // 直接创建带选项的确认对话
            DialogueData confirmOptionDialogue = DialogueSystem.CreateDialogueWithOptions(
                speakerName,
                $"确定要获取【{itemName}】吗？",
                confirmOptions
            );
            
            List<DialogueData> confirmDialogue = new List<DialogueData> { confirmOptionDialogue };
            
            if (DialogueSystem.Instance != null)
            {
                // 不设置完成回调，让选项自己处理
                DialogueSystem.Instance.StartDialogue(confirmDialogue);
            }
            else
            {
                OnFollowUpDialogueComplete();
            }
        }
        
        private IEnumerator DelayedPurchaseAction(DialogueOptionData purchaseOption, bool confirm)
        {
            
            // 等待一帧确保对话系统完全处理完选项
            yield return null;
            
            if (confirm)
            {
                CompletePurchase(purchaseOption);
                
                // 检查是否需要播放动画
                if (purchaseOption.shouldPlayAnimation && !string.IsNullOrEmpty(purchaseOption.animationName))
                {
                    // 购买成功后播放动画，不显示购买成功对话
                    yield return StartCoroutine(PlayAnimationSequence(purchaseOption.animationName, () => {
                        purchaseOption.onFollowUpDialogueComplete?.Invoke();
                    }));
                }
                else
                {
                    // 没有动画，显示购买成功对话，然后执行回调
                    ShowPurchaseSuccessDialogue(purchaseOption, () => {
                        purchaseOption.onFollowUpDialogueComplete?.Invoke();
                    });
                }
            }
            else
            {
                ShowPurchaseCancelDialogue();
            }
        }

        // 工具：下一帧再启动对话，避免与当前对话同帧结束/启动冲突
        private IEnumerator StartDialogueNextFrame(List<DialogueData> dialogues, System.Action onComplete = null)
        {
            yield return null;
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(dialogues, onComplete);
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        // 工具：下一帧显示默认确认购买对话
        private IEnumerator DeferredShowDefaultPurchaseConfirmation(DialogueOptionData purchaseOption)
        {
            yield return null;
            ShowDefaultPurchaseConfirmation(purchaseOption);
        }

        // 工具：下一帧开始自定义后续对话并在完成后购买
        private IEnumerator DeferredFollowUpThenPurchase(DialogueOptionData purchaseOption)
        {
            yield return null;
            ShowFollowUpDialogue(purchaseOption.followUpDialogue, () => {
                CompletePurchase(purchaseOption);
                if (purchaseOption.shouldPlayAnimation && !string.IsNullOrEmpty(purchaseOption.animationName))
                {
                    StartCoroutine(PlayAnimationSequence(purchaseOption.animationName, () => {
                        purchaseOption.onFollowUpDialogueComplete?.Invoke();
                    }));
                }
                else
                {
                    ShowPurchaseSuccessDialogue(purchaseOption, () => {
                        purchaseOption.onFollowUpDialogueComplete?.Invoke();
                    });
                }
            });
        }
        
        private void CompletePurchase(DialogueOptionData purchaseOption)
        {
            // 获取价格和名称
            int itemPrice;
            string finalItemName;
            
            if (purchaseOption.isService)
            {
                // 服务：使用配置的价格和选项文本作为名称
                itemPrice = purchaseOption.price;
                finalItemName = purchaseOption.optionText;
            }
            else
            {
                // 物品：使用ItemDatabase或配置的价格
                itemPrice = GetItemPrice(purchaseOption);
                finalItemName = GetItemName(purchaseOption);
            }
            
            // 二次余额校验：跨帧后可能余额已变化，需再次验证
            int currentMoney = stage5Controller != null ? stage5Controller.money : 0;
            if (currentMoney < itemPrice)
            {
                List<DialogueData> insufficientFundsDialogue = new List<DialogueData>
                {
                    DialogueSystem.CreateSubtitle(speakerName, $"你的钱不够买{finalItemName}，需要{itemPrice}G。")
                };
                if (DialogueSystem.Instance != null)
                {
                    DialogueSystem.Instance.StartDialogue(insufficientFundsDialogue, OnFollowUpDialogueComplete);
                }
                else
                {
                    OnFollowUpDialogueComplete();
                }
                return;
            }

            // 购买逻辑：扣钱
            if (stage5Controller != null)
            {
                stage5Controller.ModifyAttribute("money", -itemPrice);
            }
            
            bool added = true; // 服务默认成功
            
            // 只有非服务的物品才需要添加到背包
            if (!purchaseOption.isService && purchaseOption.itemId > 0)
            {
                // 使用ItemDatabase
                if (ItemDatabase.Instance != null && ItemDatabase.Instance.ItemExists(purchaseOption.itemId))
                {
                    added = InventorySystem.AddItemToInventory(purchaseOption.itemId);
                }
                else
                {
                    added = false;
                }
            }
            
            if (!added && !purchaseOption.isService)
            {
                // 只有物品购买失败时才退钱
                if (stage5Controller != null)
                {
                    stage5Controller.ModifyAttribute("money", itemPrice);
                }
                
                // 显示背包满的提示
                List<DialogueData> inventoryFullDialogue = new List<DialogueData>
                {
                    DialogueSystem.CreateSubtitle(speakerName, "你的背包满了，先清理一下吧。")
                };
                
                if (DialogueSystem.Instance != null)
                {
                    DialogueSystem.Instance.StartDialogue(inventoryFullDialogue, OnFollowUpDialogueComplete);
                }
                else
                {
                    OnFollowUpDialogueComplete();
                }
            }
            else
            {
                string purchaseType = purchaseOption.isService ? "服务" : "物品";
                
                // 标记选项为已购买
                purchasedOptions.Add(purchaseOption.optionText);
                
                // 特殊处理：如果购买了"见女王的方法"，设置canTalkWithQika为true
                if (purchaseOption.optionText.Contains("见女王的方法") && stage5Controller != null)
                {
                    stage5Controller.canTalkWithQika = true;
                }
            }
        }
        
        private void ShowPurchaseSuccessDialogue(DialogueOptionData purchaseOption, System.Action onComplete = null)
        {
            string itemName = GetItemName(purchaseOption);
            string successMessage;
            if (itemName == "《如来神掌》")
            {
                // 特殊物品的特殊对话
                List<DialogueData> specialDialogue = new List<DialogueData>
                {
                    DialogueSystem.CreateSubtitle("无名勇者", "这是什么？为什么这个这么便宜？"),
                    DialogueSystem.CreateSubtitle(speakerName, "因为这是我捡到的，一本破书想着没人会要。"),
                    DialogueSystem.CreateSubtitle(speakerName, "而且上面的内容完全和开玩笑一样，恶作剧道具而已。"),
                    DialogueSystem.CreateSubtitle(speakerName, "如果你今天不买的话我就扔掉了。")
                };
                
                System.Action completionCallback = onComplete != null ? 
                    () => { onComplete.Invoke(); OnFollowUpDialogueComplete(); } : 
                    OnFollowUpDialogueComplete;
                
                if (DialogueSystem.Instance != null)
                {
                    DialogueSystem.Instance.StartDialogue(specialDialogue, completionCallback);
                }
                else
                {
                    completionCallback?.Invoke();
                }
                return;
            }
        }
        
        private void ShowPurchaseCancelDialogue()
        {
            List<DialogueData> cancelDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle(speakerName, "随时再来~")
            };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(cancelDialogue, OnFollowUpDialogueComplete);
            }
            else
            {
                OnFollowUpDialogueComplete();
            }
        }
        
        private void ShowFollowUpDialogue(string[] followUpLines, System.Action onComplete = null)
        {
            List<DialogueData> followUpData = BuildDialogueData(followUpLines);
            
            // 🚫 禁止通过对话内容推测行为意图！所有场景切换必须通过SceneTransitionInteractable处理
            // 删除了通过对话内容判断场景切换的错误逻辑
            
            System.Action completionCallback = null;
            if (onComplete != null)
            {
                completionCallback = () => {
                    onComplete.Invoke();
                    OnFollowUpDialogueComplete();
                };
            }
            else
            {
                completionCallback = OnFollowUpDialogueComplete;
            }
            
            if (DialogueSystem.Instance != null && followUpData.Count > 0)
            {
                DialogueSystem.Instance.StartDialogue(followUpData, completionCallback);
            }
            else
            {
                completionCallback?.Invoke();
            }
        }
        
        private void OnInitialDialogueComplete()
        {
            // 给予首次交互属性奖励（只在真正的首次交互时给予）
            if (!hasInteractedOnce && attributeRewards != null && stage5Controller != null)
            {
                foreach (var reward in attributeRewards)
                {
                    if (reward.onFirstInteraction)
                    {
                        stage5Controller.ModifyAttribute(reward.attributeName, reward.value);
                    }
                }
            }
            
            // 恢复玩家移动
            if (nearbyPlayer != null)
            {
                nearbyPlayer.SetCanMove(true);
            }
            
            // 处理removeAfterTalk逻辑
            HandleRemoveAfterTalk();
            
            // 通知Stage5控制器
            if (stage5Controller != null)
            {
                if (!alwaysShowOptions || !hasInteractedOnce)
                {
                    // 普通NPC或alwaysShowOptions NPC的首次交互
                    stage5Controller.OnNPCInteractionComplete(npcId);
                }
                // alwaysShowOptions NPC的重复交互不通知，除非通过OnTransitionDialogueComplete调用
            }
            
            // 标记为已交互
            if (!alwaysShowOptions)
            {
                hasInteractedOnce = true;
            }
            else if (!hasInteractedOnce)
            {
                hasInteractedOnce = true; // alwaysShowOptions NPC只在首次标记
            }
        }
        
        private void OnRepeatDialogueComplete()
        {
            // 恢复玩家移动
            if (nearbyPlayer != null)
            {
                nearbyPlayer.SetCanMove(true);
            }
            
            // 处理removeAfterTalk逻辑
            HandleRemoveAfterTalk();
        }
        
        private void OnFollowUpDialogueComplete()
        {
            // 普通后续对话完成，恢复玩家移动
            if (nearbyPlayer != null)
            {
                nearbyPlayer.SetCanMove(true);
            }
            
            // 处理removeAfterTalk逻辑
            HandleRemoveAfterTalk();
            
            // 通知Stage5Controller（用于触发特殊逻辑，如解锁门等）
            if (stage5Controller != null && !alwaysShowOptions)
            {
                // 特殊处理：魔王女儿无论如何都要通知Controller以解锁门
                if (npcId == "魔王女儿" || !hasInteractedOnce)
                {
                    if (!hasInteractedOnce)
                    {
                        hasInteractedOnce = true; // 标记为已交互
                    }
                    stage5Controller.OnNPCInteractionComplete(npcId);
                }
            }
        }
        
        private void PlayAnimationAfterDialogue(DialogueOptionData animationOption)
        {
            print(animationOption.animationName);   
            // 先显示后续对话（如果有的话），然后播放动画
            if (animationOption.followUpDialogue != null && animationOption.followUpDialogue.Length > 0)
            {
                ShowFollowUpDialogue(animationOption.followUpDialogue, () => {
                    // 对话结束后播放动画
                    StartCoroutine(PlayAnimationSequence(animationOption.animationName, () => {
                        // 动画播放完成后执行回调
                        animationOption.onFollowUpDialogueComplete?.Invoke();
                    }));
                });
            }
            else
            {
                // 没有后续对话，直接播放动画
                StartCoroutine(PlayAnimationSequence(animationOption.animationName, () => {
                    // 动画播放完成后执行回调
                    animationOption.onFollowUpDialogueComplete?.Invoke();
                }));
            }
        }
        
        private IEnumerator PlayAnimationSequence(string animationName, System.Action onComplete = null)
        {
            // 暂停玩家移动
            if (nearbyPlayer != null)
            {
                nearbyPlayer.SetCanMove(false);
            }
            
            // 通过Stage5Controller播放动画
            if (stage5Controller != null && stage5Controller.transitionAnimator != null)
            {
                // 直接播放指定动画，避免修改transitionAnimationName
                stage5Controller.transitionAnimator.Play(animationName);
                
                // 等待动画播放完成
                yield return StartCoroutine(WaitForAnimationComplete(animationName));
            }
            
            // 恢复玩家移动
            if (nearbyPlayer != null)
            {
                nearbyPlayer.SetCanMove(true);
            }
            
            // 执行自定义回调
            onComplete?.Invoke();
            
            // 通知Stage5Controller NPC交互完成
            if (stage5Controller != null)
            {
                stage5Controller.OnNPCInteractionComplete(npcId);
            }
        }
        
        /// <summary>
        /// 等待指定动画播放完成
        /// </summary>
        private IEnumerator WaitForAnimationComplete(string animationName)
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
        
        // 公共方法：重置NPC状态
        public void ResetNPCState()
        {
            hasInteractedOnce = false;
            isInteractable = true;
            
            // 重置已购买选项
            purchasedOptions.Clear();
            
            // 重置对话内容
            if (hasInitialDialogue && initialDialogueLines != null && initialDialogueLines.Length > 0)
            {
                dialogueLines = initialDialogueLines;
            }
        }
        
        // 公共方法：检查是否已交互
        public bool HasInteracted()
        {
            return hasInteractedOnce;
        }
        
        // 公共方法：设置NPC名称
        public void SetNPCName(string newName)
        {
            speakerName = newName;
        }
        
        // 公共方法：检查指定选项是否已购买
        public bool HasPurchasedOption(string optionText)
        {
            return purchasedOptions.Contains(optionText);
        }
        
        // 公共方法：手动标记选项为已购买（用于特殊情况）
        public void MarkOptionAsPurchased(string optionText)
        {
            purchasedOptions.Add(optionText);
        }

        /// <summary>
        /// 停止后续再显示选项：
        /// - 关闭 alwaysShowOptions
        /// - 将当前所有选项加入禁用列表
        /// 建议在“第一个选项”的 onFollowUpDialogueComplete 里绑定该方法
        /// </summary>
        public void StopShowingOptions()
        {
            alwaysShowOptions = false;
            if (dialogueOptions != null)
            {
                for (int i = 0; i < dialogueOptions.Length; i++)
                {
                    var opt = dialogueOptions[i];
                    if (opt != null && !string.IsNullOrEmpty(opt.optionText))
                    {
                        disabledOptions.Add(opt.optionText);
                    }
                }
            }
        }
        
        /// <summary>
        /// 处理挖矿选项（当在对话选项中选择挖矿时调用）
        /// </summary>
        private void HandleMiningOption()
        {
            if (stage5Controller != null)
            {
                stage5Controller.StartMiningQuestion();
            }
        }
        
        /// <summary>
        /// 处理问题触发选项
        /// </summary>
        private void HandleQuestionOption(DialogueOptionData questionOption)
        {
            if (QuestionSystem.Instance == null)
            {
                OnFollowUpDialogueComplete();
                return;
            }
            
            // 获取问题
            Question question = QuestionSystem.Instance.GetQuestionById(questionOption.questionId);
            if (question == null)
            {
                OnFollowUpDialogueComplete();
                return;
            }
            
            // 暂停玩家移动
            if (nearbyPlayer != null)
            {
                nearbyPlayer.SetCanMove(false);
            }
            
            // 显示问题，并设置回调
            QuestionSystem.Instance.ShowQuestion(question, (answeredQuestion, playerAnswer) => {
                OnQuestionAnswered(questionOption, answeredQuestion, playerAnswer);
            }, nearbyPlayer);
        }
        
        /// <summary>
        /// 问题回答完成回调
        /// </summary>
        private void OnQuestionAnswered(DialogueOptionData questionOption, Question answeredQuestion, string playerAnswer)
        {
            // ⚠️ 重要：记录答案到QuestionSystem中（QuestionSystem会自动保存存档）
            if (QuestionSystem.Instance != null)
            {
                QuestionSystem.Instance.AnswerQuestion(answeredQuestion.id, playerAnswer);
            }
            
            // 判断答案是否正确
            bool isCorrect = false;
            if (!string.IsNullOrEmpty(answeredQuestion.correctAnswer))
            {
                isCorrect = playerAnswer.Trim().Equals(answeredQuestion.correctAnswer.Trim(), System.StringComparison.OrdinalIgnoreCase);
            }
            
            if (isCorrect)
            {
                HandleCorrectAnswer(questionOption);
            }
            else
            {
                HandleWrongAnswer(questionOption);
            }
        }
        
        /// <summary>
        /// 处理正确答案
        /// </summary>
        private void HandleCorrectAnswer(DialogueOptionData questionOption)
        {
            // 给予奖励道具
            if (questionOption.rewardItemId > 0)
            {
                bool itemAdded = InventorySystem.AddItemToInventory(questionOption.rewardItemId);
                if (itemAdded)
                {
                    
                }
                else
                {
                    
                }
            }

            // 特殊需求：题目ID为323且回答正确后，移除该对话（禁用该选项）
            if (questionOption.questionId == 323)
            {
                disabledOptions.Add(questionOption.optionText);
            }
            
            // 组合附加对话：题目323，且玩家已拥有 苦(7003)、集(7001)、灭(7002) 与 《如来神掌》(104)
            List<string> mergedLines = new List<string>();
            bool shouldAppendFourTruths = false;
            if (questionOption.questionId == 323)
            {
                bool hasKu = InventorySystem.HasItemInInventory(7003);
                bool hasJi = InventorySystem.HasItemInInventory(7001);
                bool hasMie = InventorySystem.HasItemInInventory(7002);
                bool hasBook = InventorySystem.HasItemInInventory(104);
                shouldAppendFourTruths = hasKu && hasJi && hasMie && hasBook;
            }

            if (shouldAppendFourTruths)
            {
                mergedLines.Add("这是四圣谛，苦、集、灭……还剩下一个道！");
                mergedLines.Add("原来最后一个道，一直在我身上啊。");
                mergedLines.Add("习得技能：如来神掌！");
            }

            // 叠加原有正确后续对话或默认对话
            if (questionOption.correctAnswerFollowUp != null && questionOption.correctAnswerFollowUp.Length > 0)
            {
                mergedLines.AddRange(questionOption.correctAnswerFollowUp);
            }
            else
            {
                mergedLines.Add($"{speakerName}：很好，你通过了测试！");
            }

            // 一次性播放合并后的对话，结束后执行回调
            ShowFollowUpDialogue(mergedLines.ToArray(), () => {
                questionOption.onFollowUpDialogueComplete?.Invoke();
            });
        }
        
        /// <summary>
        /// 处理错误答案
        /// </summary>
        private void HandleWrongAnswer(DialogueOptionData questionOption)
        {
            // 显示错误答案的后续对话
            if (questionOption.wrongAnswerFollowUp != null && questionOption.wrongAnswerFollowUp.Length > 0)
            {
                ShowFollowUpDialogue(questionOption.wrongAnswerFollowUp, () => {
                    // 执行自定义回调
                    questionOption.onFollowUpDialogueComplete?.Invoke();
                });
            }
            else
            {
                // 默认的错误答案对话
                string[] defaultWrongDialogue = { $"{speakerName}：不对，再想想吧。" };
                ShowFollowUpDialogue(defaultWrongDialogue, () => {
                    // 执行自定义回调
                    questionOption.onFollowUpDialogueComplete?.Invoke();
                });
            }
        }
        
        /// <summary>
        /// 检查所需物品条件是否满足
        /// </summary>
        private bool CheckRequiredItemCondition()
        {
            if (requiredItemId <= 0) return true;
            
            if (InventorySystem.Instance != null)
            {
                // 检查背包中是否有指定ID的物品
                for (int i = 0; i < InventorySystem.Instance.maxSlots; i++)
                {
                    Item item = InventorySystem.Instance.GetItem(i);
                    if (!item.IsEmpty && item.id == requiredItemId)
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 显示条件不满足时的对话
        /// </summary>
        private void ShowConditionFailDialogue()
        {
            string message = !string.IsNullOrEmpty(conditionFailMessage) 
                ? conditionFailMessage 
                : "似乎还有什么事情没有完成...";
                
            List<DialogueData> conditionFailDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle(speakerName, message)
            };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(conditionFailDialogue);
            }
        }
        
        /// <summary>
        /// 处理removeAfterTalk逻辑（从父类DialogueInteractable的OnDialogueComplete方法中提取）
        /// </summary>
        private void HandleRemoveAfterTalk()
        {
            // 如果设置为对话后移除，则禁用交互
            if (removeAfterTalk)
            {
                isInteractable = false;
                SetHighlight(false);
                
                // 延迟隐藏物体
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
        /// 延迟显示后续对话，避免与DialogueSystem的ShowNextDialogue冲突
        /// </summary>
        private IEnumerator DelayedFollowUpDialogue(DialogueOptionData selectedOption)
        {
            // 等待一帧，让DialogueSystem完成当前的选项处理
            yield return null;
            
            ShowFollowUpDialogue(selectedOption.followUpDialogue, () => {
                // 执行自定义回调
                selectedOption.onFollowUpDialogueComplete?.Invoke();
            });
        }
        
        /// <summary>
        /// 延迟执行回调，避免与DialogueSystem的ShowNextDialogue冲突
        /// </summary>
        private IEnumerator DelayedCallback(DialogueOptionData selectedOption)
        {
            // 等待一帧，让DialogueSystem完成当前的选项处理
            yield return null;
            
            // 没有后续对话，直接执行回调
            selectedOption.onFollowUpDialogueComplete?.Invoke();
            OnFollowUpDialogueComplete();
        }
        
        /// <summary>
        /// 强制交互方法（供外部调用，无需玩家在范围内）
        /// </summary>
        public void ForceInteract()
        {
            if (!isInteractable)
            {
                return;
            }
            
            // 临时设置玩家为附近状态
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                nearbyPlayer = player;
            }
            
            // 根据是否已交互过来选择对话内容
            if (!hasInteractedOnce && hasInitialDialogue)
            {
                ShowInitialDialogue();
            }
            else if (hasRepeatDialogue && repeatDialogueLines != null && repeatDialogueLines.Length > 0)
            {
                ShowRepeatDialogue();
            }
            else if (initialDialogueLines != null && initialDialogueLines.Length > 0)
            {
                // 没有重复对话时，使用初次对话
                ShowInitialDialogue();
            }
        }

        // ==========================
        // 实用方法：在 Inspector 里复用
        // ==========================
        /// <summary>
        /// 在当前对话流程结束后隐藏该 NPC（常用于某个选项后不再出现的角色）
        /// 可直接将某个选项的 onFollowUpDialogueComplete 事件绑定到该方法。
        /// </summary>
        public void HideAfterFollowUp()
        {
            // 设为对话后移除，并立即走一次移除流程
            removeAfterTalk = true;
            HandleRemoveAfterTalk();
        }

    }
} 