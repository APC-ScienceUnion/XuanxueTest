using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace XuanZhiShiLian
{
    public class Stage5Controller : MonoBehaviour
    {
        [Header("场景设置")]
        public Transform playerSpawnPoint;
        public Canvas canvas;
        
        [Header("场景3-子场景1")]
        public SceneTransitionInteractable caveDoor; // 山洞门（子场景1→子场景2）
        
        [Header("场景4-子场景1")]
        public SceneTransitionInteractable castleExitDoor; // 离开城堡回到破败城市的门（子场景1→子场景2）
        
        [Header("场景4-子场景2")]
        public SceneTransitionInteractable castleDoor; // 通往城堡内部的门（子场景2→子场景1）
        public SceneTransitionInteractable roadToNextSubScene; // 通往下一子场景的道路（子场景2→子场景3）
        
        [Header("场景4-子场景3")]
        public Stage5NPCInteractable fortressShipNPC; // 前往矮人租界的船夫
        
        [Header("过场动画设置")]
        public Animator transitionAnimator;  // 动画控制器
        public string transitionAnimationName = "Stage5-2-2";  // 动画名称
        
        [Header("玩家属性")]
        public int goodEvilValue = 0;    // 善恶值
        public int truthValue = 0;       // 真理值
        public int loveDesireValue = 0;  // 爱欲值
        public int money = 0;            // 金钱
        
        [Header("属性UI")]
        public GameObject attributePanel;
        public TextMeshProUGUI moneyText;
        
        [Header("场景标题UI")]
        public TextMeshProUGUI storyTitleText;
        
        [Header("场景进度")]
        public bool canExitScene = false; // 是否可以离开场景
        public bool isFirstTimeEntry = true; // 是否是真正的第一次进入（用于触发魔王对话）

        [Header("如来神掌进度")]
        public bool hasJi = false;
        public bool hasMie = false;
        public bool hasKu = false;
        
        [Header("魔将玄武占卜功能")]
        public bool canUseMJXW = false; // 是否可以使用魔将玄武占卜
        private int mjxwUsageCount = 0; // 魔将玄武使用次数计数器
        
        [Header("魔王NPC设置")]
        public Stage5NPCInteractable demonKingNPC; // 魔王NPC的引用
        
        // NPC交互状态
        private Dictionary<string, bool> npcInteractionStatus = new Dictionary<string, bool>();
        private Dictionary<string, string> npcNameChanges = new Dictionary<string, string>();
        
        // 矮人城镇相关数据
        private bool hasReceivedFreePickaxe = false; // 是否已获得免费石镐
        
        // 树屋D相关数据
        public bool canTalkWithQika = false; // 是否可以与奇卡女王对话
        // 是否称呼为公主
        public bool isCallingPrincess = true;// 影响5-2公主名字
        
        private void Start()
        {
            InitializeStage();
            InitializeUI();
            StartStageSequence();
        }
        
        private void InitializeStage()
        {
            // 更新当前阶段
            GameManager.Instance.currentStage = 5;
            
            // 初始化玩家属性
            ResetAttributes();
            
            // 初始化玩家位置
            if (playerSpawnPoint != null)
            {
                PlayerController player = FindObjectOfType<PlayerController>();
                if (player != null)
                {
                    player.transform.position = playerSpawnPoint.position;
                }
            }
            
            // 确保背包系统已初始化并显示
            InitializeInventorySystem();

            // 在加载存档进入时，优先同步是否第一次进入的标记，避免首次对话误触发
            if (GameManager.Instance != null && GameManager.Instance.savedStageData != null)
            {
                isFirstTimeEntry = GameManager.Instance.savedStageData.isFirstTimeEntry;
            }

            // 开局默认锁定离开城堡的门，直至与魔王女儿/公主完成对话后解锁
            if (castleExitDoor != null)
            {
                castleExitDoor.isInteractable = false;
                if (!string.IsNullOrEmpty(castleExitDoor.interactionText))
                {
                    castleExitDoor.interactionText = "和魔王女儿聊聊吧";
                }
            }
        }
        
        /// <summary>
        /// 初始化背包系统
        /// </summary>
        private void InitializeInventorySystem()
        {
            if (InventorySystem.Instance != null)
            {
                // Stage5需要显示背包
                InventorySystem.Instance.InitializeInventoryUI();
            }
        }
        
        private void InitializeUI()
        {
            CreateAttributeUI();
            UpdateAttributeUI();
        }
        
        private void CreateAttributeUI()
        {
            if (attributePanel == null)
            {
                // 创建面板
                GameObject panel = new GameObject("AttributePanel");
                panel.transform.SetParent(canvas.transform, false);
                
                RectTransform panelRect = panel.AddComponent<RectTransform>();
                panelRect.anchorMin = new Vector2(0f, 1f);
                panelRect.anchorMax = new Vector2(0f, 1f);
                panelRect.anchoredPosition = new Vector2(10f, -10f);
                panelRect.sizeDelta = new Vector2(200f, 120f);
                
                // 添加背景
                Image panelImage = panel.AddComponent<Image>();
                panelImage.color = new Color(0f, 0f, 0f, 0.7f);
                
                attributePanel = panel;
                
                // 创建属性文本
                CreateAttributeText("金钱: 0G", new Vector2(0f, -10f), out moneyText);
            }
        }
        
        private void CreateAttributeText(string text, Vector2 position, out TextMeshProUGUI textComponent)
        {
            GameObject textObj = new GameObject("AttributeText");
            textObj.transform.SetParent(attributePanel.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0f, 1f);
            textRect.anchorMax = new Vector2(1f, 1f);
            textRect.anchoredPosition = position;
            textRect.sizeDelta = new Vector2(-20f, 20f);
            
            textComponent = textObj.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = 14f;
            textComponent.color = Color.white;
        }
        
        public void SetHasJi(bool value)
        {
            hasJi = value;
        }
        
        public void SetHasMie(bool value)
        {
            hasMie = value;
        }
        
        public void SetHasKu(bool value)
        {
            hasKu = value;
        }
        public void SetCanTalkWithQika(bool value)
        {
            canTalkWithQika = value;
        }
        
        /// <summary>
        /// 设置是否可以使用魔将玄武占卜功能
        /// 在用户通过动画触发PlayRouteplanningDialogue后调用
        /// </summary>
        public void SetCanUseMJXW(bool value)
        {
            canUseMJXW = value;
        }
        
        private void StartStageSequence()
        {
            StartCoroutine(PlayOpeningSequence());
        }
        
        private IEnumerator PlayOpeningSequence()
        {
            // 开场动画
            List<DialogueData> openingDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSceneDescription("【Stage5】-爱与森林"),
                DialogueSystem.CreateSubtitle("", "请用心感受这个世界。"),
                DialogueSystem.CreateSubtitle("", "你的每一个选择都很重要。")
            };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(openingDialogue);
                yield return new WaitUntil(() => !DialogueSystem.Instance.isDialogueActive);
            }
            
            // 检查是否需要触发魔王对话（仅在真正第一次进入时）
            if (isFirstTimeEntry)
            {
                yield return StartCoroutine(TriggerDemonKingDialogue());
                // 标记已经不是第一次进入了
                isFirstTimeEntry = false;
            }
        }
        
        /// <summary>
        /// 触发魔王对话（仅在第一次进入时调用）
        /// </summary>
        private IEnumerator TriggerDemonKingDialogue()
        {
            if (demonKingNPC == null)
            {
                yield break;
            }
            
            // 等待一小段时间确保开场对话完全结束
            yield return new WaitForSeconds(0.5f);
            
            // 暂停玩家移动
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                player.SetCanMove(false);
            }
            
            // 直接调用魔王NPC的强制交互方法
            demonKingNPC.ForceInteract();
            
            // 等待魔王对话完成（检查对话系统状态）
            yield return new WaitUntil(() => !DialogueSystem.Instance.isDialogueActive);
            
            // 恢复玩家移动
            if (player != null)
            {
                player.SetCanMove(true);
            }
        }
        
        // 属性管理方法
        public void ModifyAttribute(string attributeName, int value)
        {
            switch (attributeName.ToLower())
            {
                case "goodevil":
                case "善恶":
                    goodEvilValue += value;
                    break;
                case "truth":
                case "真理":
                    truthValue += value;
                    break;
                case "lovedesire":
                case "爱欲":
                    loveDesireValue += value;
                    break;
                case "money":
                case "金钱":
                    {
                        int newMoney = money + value;
                        money = Mathf.Max(0, newMoney);
                    }
                    break;
                default:
                    return;
            }
            UpdateAttributeUI();
        }
        
        public void SetAttribute(string attributeName, int value)
        {
            switch (attributeName.ToLower())
            {
                case "goodevil":
                case "善恶":
                    goodEvilValue = value;
                    break;
                case "truth":
                case "真理":
                    truthValue = value;
                    break;
                case "lovedesire":
                case "爱欲":
                    loveDesireValue = value;
                    break;
                case "money":
                case "金钱":
                    money = Mathf.Max(0, value);
                    break;
                default:
                    return;
            }
            
            UpdateAttributeUI();
        }
        
        public void ResetAttributes()
        {
            goodEvilValue = 0;
            truthValue = 0;
            loveDesireValue = 0;
            money = 0; 
            UpdateAttributeUI();
        }
        
        public void LoadAttributes(int goodEvil, int truth, int loveDesire, int moneyValue)
        {
            goodEvilValue = goodEvil;
            truthValue = truth;
            loveDesireValue = loveDesire;
            money = moneyValue;
            UpdateAttributeUI();
        }
        
        /// <summary>
        /// 设置是否是第一次进入（供存档系统调用）
        /// </summary>
        public void SetIsFirstTimeEntry(bool value)
        {
            isFirstTimeEntry = value;
        }
        
        /// <summary>
        /// 获取是否是第一次进入（供存档系统调用）
        /// </summary>
        public bool GetIsFirstTimeEntry()
        {
            return isFirstTimeEntry;
        }
        
        private void UpdateAttributeUI()
        {
            if (moneyText != null)
                moneyText.text = $"金钱: {money}G";
        }

        public void SetIsCallingPrincess(bool value)
        {
            isCallingPrincess = value;
        }
        
        // NPC交互完成回调
        public void OnNPCInteractionComplete(string npcId)
        {
            npcInteractionStatus[npcId] = true;
            
            // 特殊处理：与魔王女儿对话后解锁城堡出口
            if (npcId == "魔王女儿" && castleExitDoor != null)
            {
                castleExitDoor.isInteractable = true;
                castleExitDoor.interactionText = "按E离开城堡";
            }
        }
        
        
        // NPC名称管理
        public void SetNPCName(string oldName, string newName)
        {
            npcNameChanges[oldName] = newName;
        }
        
        public string GetNPCName(string originalName)
        {
            return npcNameChanges.ContainsKey(originalName) ? npcNameChanges[originalName] : originalName;
        }
        
        // 存档系统需要的getter方法（补充缺失的方法）
        public bool GetHasReceivedFreePickaxe()
        {
            return hasReceivedFreePickaxe;
        }
        
        public Dictionary<string, bool> GetNPCInteractionStatus()
        {
            return new Dictionary<string, bool>(npcInteractionStatus);
        }
        
        public Dictionary<string, string> GetNPCNameChanges()
        {
            return new Dictionary<string, string>(npcNameChanges);
        }
        
        // 存档系统需要的setter方法（补充缺失的方法）
        public void SetMJXWUsageCount(int count)
        {
            mjxwUsageCount = count;
        }
        
        public void SetNPCInteractionStatus(Dictionary<string, bool> status)
        {
            if (status != null)
            {
                npcInteractionStatus = new Dictionary<string, bool>(status);
            }
        }
        
        public void SetNPCNameChanges(Dictionary<string, string> changes)
        {
            if (changes != null)
            {
                npcNameChanges = new Dictionary<string, string>(changes);
            }
        }
        
        // 故事标题管理
        public void SetStoryTitle(string title)
        {
            if (storyTitleText != null)
            {
                storyTitleText.text = title;
            }
        }
        
        private void Update()
        {
            // 按ESC返回主菜单
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // 在播放过场动画或对话时禁止ESC
                bool isPlayingTransition = transitionAnimator != null && transitionAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f && transitionAnimator.GetCurrentAnimatorStateInfo(0).length > 0f;
                if (isPlayingTransition)
                {
                    return;
                }
                if (DialogueSystem.Instance != null && DialogueSystem.Instance.isDialogueActive)
                {
                    return;
                }
                GameManager.Instance.HandleEscapeKey();
            }
        }
        
        // 场景完成处理
        public void CompleteCurrentScene()
        {
            GameManager.Instance.CompleteStage(5);
        }
        
        
        // 场景区域交互处理（为SceneAreaInteractable提供兼容性支持）
        public void OnSceneAreaInteraction(int sceneIndex)
        {
            // 场景切换现在由SceneTransitionInteractable和动画处理，这里只记录日志
        }
        
        // 进入魔族要塞时触发的自动对话
        public void OnEnterFortress()
        {
            
            // 检查是否已经播放过魔族要塞对话
            if (npcInteractionStatus.ContainsKey("魔族要塞对话") && npcInteractionStatus["魔族要塞对话"])
            {
                return; // 已经播放过，不重复播放
            }
            
            StartCoroutine(PlayFortressPaintingDialogue());
        }
        
        private IEnumerator PlayFortressPaintingDialogue()
        {
            // 等待一小段时间确保场景切换完成
            yield return new WaitForSeconds(0.5f);
            
            // 暂停玩家移动
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                player.SetCanMove(false);
            }
            
            // 获取当前的角色名称（可能已经被之前的交互修改过）
            string currentPlayerName = GetNPCName("我");
            string currentPrincessName = (isCallingPrincess) ? "魔族公主" : "魔王女儿";
            
            // 构建壁画对话
            List<DialogueData> paintingDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle(currentPlayerName, "这是什么？"),
                DialogueSystem.CreateSubtitle(currentPrincessName, "这是爷爷，魔王波旬的画像，他被人类的那个鸭舌帽勇者杀了。"),
                DialogueSystem.CreateSubtitle(currentPlayerName, "鸭舌帽？"),
                DialogueSystem.CreateSubtitle(currentPrincessName, "那个人永远带着他的鸭舌帽，所有人都这么叫他，没人记得他的名字。"),
                DialogueSystem.CreateSubtitle(currentPlayerName, "那我叫什么？")
            };
            
            // 创建选项对话
            List<DialogueOption> options = new List<DialogueOption>
            {
                DialogueSystem.CreateOption("我自无名姓，身心本一空", () => OnPaintingOptionA_Selected()),
                DialogueSystem.CreateOption("我以后能直接称呼你的名字吗？", () => OnPaintingOptionB_Selected())
            };
            
            DialogueData optionDialogue = DialogueSystem.CreateDialogueWithOptions(
                currentPrincessName, 
                "我不知道，但是我的名字叫莉姆莉卡。你也没说过你的名字。", 
                options
            );
            
            paintingDialogue.Add(optionDialogue);
            
            // 开始对话
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(paintingDialogue);
            }
            else
            {
                OnPaintingDialogueComplete();
            }
        }
        
        private void OnPaintingOptionA_Selected()
        {
            // 修改属性：真理值+1，爱欲值-1
            ModifyAttribute("truth", 1);
            ModifyAttribute("lovedesire", -1);
            
            // 继续对话到下一条
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.ShowNextDialogue();
            }
            
            // 选择A应该调用A的后续对话，添加延迟确保对话系统准备好
            StartCoroutine(DelayedShowPaintingOptionAFollowUp());
        }
        
        private void OnPaintingOptionB_Selected()
        {
            // 修改属性：善恶值+1，爱欲值+1
            ModifyAttribute("goodevil", 1);
            ModifyAttribute("lovedesire", 1);
            
            // 继续对话到下一条
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.ShowNextDialogue();
            }
            
            // 🔧 修复：选择B应该调用B的后续对话，添加延迟确保对话系统准备好
            StartCoroutine(DelayedShowPaintingOptionBFollowUp());
        }
        
        /// <summary>
        /// 延迟执行选项A的后续对话，确保对话系统完全准备好
        /// </summary>
        private IEnumerator DelayedShowPaintingOptionAFollowUp()
        {
            // 等待对话系统完全结束当前对话
            yield return new WaitUntil(() => !DialogueSystem.Instance.isDialogueActive);
            
            // 额外等待一小段时间确保UI状态稳定
            yield return new WaitForSeconds(0.1f);
            
            // 开始A选项的后续对话
            yield return StartCoroutine(ShowPaintingOptionAFollowUp());
        }
        
        /// <summary>
        /// 延迟执行选项B的后续对话，确保对话系统完全准备好
        /// </summary>
        private IEnumerator DelayedShowPaintingOptionBFollowUp()
        {
            // 等待对话系统完全结束当前对话
            yield return new WaitUntil(() => !DialogueSystem.Instance.isDialogueActive);
            
            // 额外等待一小段时间确保UI状态稳定
            yield return new WaitForSeconds(0.1f);
            
            // 开始B选项的后续对话
            yield return StartCoroutine(ShowPaintingOptionBFollowUp());
        }
        
        private IEnumerator ShowPaintingOptionAFollowUp()
        {
            // 第一段：魔族公主/魔王女儿的第一句话
            List<DialogueData> part1 = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle("魔族公主", "你果然是个奇怪的人，那既然如此，你就叫无名勇者吧。还不感谢如此伟大的我赐给你姓名？")
            };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(part1);
                yield return new WaitUntil(() => !DialogueSystem.Instance.isDialogueActive);
            }
            
            // 此时修改"我"称呼为"无名勇者"
            SetNPCName("我", "无名勇者");
            
            // 第二段：无名勇者的回应
            List<DialogueData> part2 = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle("无名勇者", "感谢莉姆莉卡殿下。")
            };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(part2);
                yield return new WaitUntil(() => !DialogueSystem.Instance.isDialogueActive);
            }
            
            // 此时修改"魔族公主/魔王女儿"称呼为"莉姆莉卡"
            SetNPCName("魔族公主", "莉姆莉卡");
            
            // 第三段：莉姆莉卡的后续对话
            List<DialogueData> part3 = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle("莉姆莉卡", "咿呀~如果你不是勇者的话，你这么僭越的称呼我的名字可是死刑哦~"),
                DialogueSystem.CreateSubtitle("莉姆莉卡", "算了，反正以后还要掩人耳目，你以后就这么叫吧，我原谅你了，感谢我吧。")
            };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(part3, OnPaintingDialogueComplete);
            }
            else
            {
                OnPaintingDialogueComplete();
            }
        }
        
        private IEnumerator ShowPaintingOptionBFollowUp()
        {
            // 第一段：魔族公主/魔王女儿的前两句话
            List<DialogueData> part1 = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle("魔族公主", "嗯……那就这样吧，公开场合为了掩人耳目你就叫我名字，问起来就说我是你妹妹。"),
                DialogueSystem.CreateSubtitle("魔族公主", "如何？额……所以，你叫什么名字？")
            };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(part1);
                yield return new WaitUntil(() => !DialogueSystem.Instance.isDialogueActive);
            }
            
            // 此时修改"魔族公主/魔王女儿"称呼为"莉姆莉卡"
            SetNPCName("魔族公主", "莉姆莉卡");
            
            // 第二段：我的回应
            List<DialogueData> part2 = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle("我", "我不知道……我应该没有名字")
            };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(part2);
                yield return new WaitUntil(() => !DialogueSystem.Instance.isDialogueActive);
            }
            
            // 第三段：莉姆莉卡的回应
            List<DialogueData> part3 = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle("莉姆莉卡", "那这样吧，既然你没有名字，你就叫无名勇者吧？还不快，感谢伟大的公主殿下赐给你名字？")
            };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(part3);
                yield return new WaitUntil(() => !DialogueSystem.Instance.isDialogueActive);
            }
            
            // 此时修改"我"称呼为"无名勇者"
            SetNPCName("我", "无名勇者");
            
            // 第四段：无名勇者的最终回应
            List<DialogueData> part4 = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle("无名勇者", "谢公主殿下赐名。")
            };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(part4, OnPaintingDialogueComplete);
            }
            else
            {
                OnPaintingDialogueComplete();
            }
        }
        
        private void OnPaintingDialogueComplete()
        {
            // 恢复玩家移动
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                player.SetCanMove(true);
            }
            
            // 标记魔族要塞对话已完成
            OnNPCInteractionComplete("魔族要塞对话");
        }
        
        // 处理船夫过场动画和场景切换
        private IEnumerator HandleShipTransition()
        {
            // 播放过场动画
            yield return StartCoroutine(PlayTransitionAnimation());
            
            // 完成Stage5
            CompleteCurrentScene();
            
            // 切换到Stage6
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadScene("Stage6");
            }
        }
        
        /// <summary>
        /// 播放过场动画（参考Stage0Controller实现）
        /// </summary>
        public IEnumerator PlayTransitionAnimation()
        {
            if (transitionAnimator != null)
            {
                // 播放动画
                transitionAnimator.Play(transitionAnimationName);
                
                // 等待动画播放完成
                yield return StartCoroutine(WaitForTransitionAnimation());
                
            }
        }
        
        /// <summary>
        /// 等待过场动画播放完成（参考Stage0Controller实现）
        /// </summary>
        private IEnumerator WaitForTransitionAnimation()
        {
            float animationTime = 3f; // 默认动画时长
            float elapsedTime = 0f;
            
            // 如果有Animator组件，尝试获取动画长度
            if (transitionAnimator != null)
            {
                AnimatorStateInfo stateInfo = transitionAnimator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.IsName(transitionAnimationName))
                {
                    animationTime = stateInfo.length;
                }
            }
            
            // 等待动画播放完成
            while (elapsedTime < animationTime)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // 确保动画完全播放完成
            yield return new WaitForEndOfFrame();
        }
        
        /// <summary>
        /// 播放Stage5-3-1过场动画（供caveDoor的onTransitionComplete回调使用）
        /// </summary>
        public void PlayStage5_3_1Animation()
        {
            StartCoroutine(PlayStage5_3_1AnimationSequence());
        }
        
        private IEnumerator PlayStage5_3_1AnimationSequence()
        {
            // 暂停玩家移动
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                player.SetCanMove(false);
            }
            
            // 播放Stage5-3-1动画
            if (transitionAnimator != null)
            {
                // 播放动画
                transitionAnimator.Play("Stage5-3-1");
                
                // 等待动画播放完成
                yield return StartCoroutine(WaitForSpecificAnimation("Stage5-3-1"));
            }
            
            // 恢复玩家移动
            if (player != null)
            {
                player.SetCanMove(true);
            }
        }
        
        private IEnumerator WaitForSpecificAnimation(string animName)
        {
            float animationTime = 3f; // 默认动画时长
            float elapsedTime = 0f;
            
            // 等待一帧确保动画开始播放
            yield return new WaitForEndOfFrame();
            
            // 如果有Animator组件，尝试获取动画长度
            if (transitionAnimator != null)
            {
                AnimatorStateInfo stateInfo = transitionAnimator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.IsName(animName))
                {
                    animationTime = stateInfo.length;
                }
            }
            
            // 等待动画播放完成
            while (elapsedTime < animationTime)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // 确保动画完全播放完成
            yield return new WaitForEndOfFrame();
        }
        
        // ================================
        // 矮人城镇交互方法 - 第一部分
        // ================================
        
        /// <summary>
        /// 检查玩家是否有足够金钱进行购买
        /// </summary>
        private bool CanAfford(int price)
        {
            return money >= price;
        }
        
        /// <summary>
        /// 购买物品（扣除金钱）
        /// </summary>
        private void Purchase(int price)
        {
            if (CanAfford(price))
            {
                ModifyAttribute("money", -price);
            }
        }
        
        /// <summary>
        /// 通用商店购买对话
        /// </summary>
        /// <param name="itemName">物品名称</param>
        /// <param name="price">价格，0表示免费</param>
        /// <param name="itemId">物品ID，0表示非真实物品（如服务）</param>
        /// <param name="onConfirm">确认后的回调</param>
        public void ShowPurchaseConfirmation(string itemName, int price, int itemId, System.Action onConfirm)
        {
            // 价格为0的情况：免费赠送
            if (price == 0)
            {
                HandleFreeGift(itemName, itemId, onConfirm);
                return;
            }
            
            // 有价格的购买确认对话
            List<DialogueData> confirmDialogue = new List<DialogueData>();
            
            List<DialogueOption> options = new List<DialogueOption>
            {
                DialogueSystem.CreateOption("确定", () => {
                    if (CanAfford(price))
                    {
                        Purchase(price);
                        onConfirm?.Invoke();
                        
                        // 根据itemId决定显示什么成功信息
                        if (itemId == 0)
                        {
                            ShowServicePurchaseSuccessDialogue(itemName);
                        }
                        else
                        {
                            ShowItemPurchaseSuccessDialogue(itemName);
                        }
                    }
                    else
                    {
                        ShowInsufficientFundsDialogue();
                    }
                }),
                DialogueSystem.CreateOption("算了", () => {
                    ShowPurchaseCancelDialogue();
                })
            };
            
            string dialogueText = itemId == 0 ? 
                $"要购买{itemName}吗？需要{price}G" : 
                $"要买{itemName}吗？需要{price}G";
                
            DialogueData confirmDialogueData = DialogueSystem.CreateDialogueWithOptions(
                " ", 
                dialogueText, 
                options
            );
        
            confirmDialogue.Add(confirmDialogueData);
        
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(confirmDialogue);
            }
        }
        
        /// <summary>
        /// 处理免费赠送物品
        /// </summary>
        private void HandleFreeGift(string itemName, int itemId, System.Action onConfirm)
        {
            // 执行获得物品的逻辑
            onConfirm?.Invoke();
            
            // 根据itemId决定显示什么信息
            if (itemId == 0)
            {
                // 非真实物品（如服务），显示简单确认
                List<DialogueData> serviceDialogue = new List<DialogueData>
                {
                    DialogueSystem.CreateInteractionHint($"{itemName}完成！", 2f)
                };
                
                if (DialogueSystem.Instance != null)
                {
                    DialogueSystem.Instance.StartDialogue(serviceDialogue);
                }
            }
            else
            {
                // 真实物品，显示获得物品提示
                List<DialogueData> giftDialogue = new List<DialogueData>
                {
                    DialogueSystem.CreateInteractionHint($"获得了{itemName}！", 2f)
                };
                
                if (DialogueSystem.Instance != null)
                {
                    DialogueSystem.Instance.StartDialogue(giftDialogue);
                }
            }
        }
        
        /// <summary>
        /// 购买成功对话（向后兼容性保留）
        /// </summary>
        private void ShowPurchaseSuccessDialogue()
        {
            ShowItemPurchaseSuccessDialogue("物品");
        }
        
        /// <summary>
        /// 物品购买成功对话
        /// </summary>
        private void ShowItemPurchaseSuccessDialogue(string itemName)
        {
            List<DialogueData> successDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle("小贩", "谢谢惠顾"),
                DialogueSystem.CreateInteractionHint($"获得了{itemName}！", 2f)
            };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(successDialogue);
            }
        }
        
        /// <summary>
        /// 服务购买成功对话
        /// </summary>
        private void ShowServicePurchaseSuccessDialogue(string serviceName)
        {
            List<DialogueData> successDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle("小贩", "谢谢惠顾"),
                DialogueSystem.CreateInteractionHint($"{serviceName}完成！", 2f)
            };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(successDialogue);
            }
        }
        
        /// <summary>
        /// 购买取消对话
        /// </summary>
        private void ShowPurchaseCancelDialogue()
        {
            List<DialogueData> cancelDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle("小贩", "欢迎再来")
            };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(cancelDialogue);
            }
        }
        
        /// <summary>
        /// 金钱不足对话
        /// </summary>
        private void ShowInsufficientFundsDialogue()
        {
            List<DialogueData> insufficientDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle("小贩", "您的金钱不足")
            };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(insufficientDialogue);
            }
        }
        
        // ================================
        // 矮人城镇辅助方法（供Stage5NPCInteractable使用）
        // ================================
        
        /// <summary>
        /// 访问私有字段的公共属性
        /// </summary>
        public bool HasReceivedFreePickaxe => hasReceivedFreePickaxe;
        
        /// <summary>
        /// 设置是否已获得免费石镐
        /// </summary>
        public void SetHasReceivedFreePickaxe(bool value)
        {
            hasReceivedFreePickaxe = value;
        }

        /// <summary>
        /// 赠送免费石镐方法
        /// </summary>
        public void GiftFreeGao()
        {
            // 设置已获得免费石镐的标记
            SetHasReceivedFreePickaxe(true);
            
            // 添加石镐道具（id：5001）到背包
            bool success = InventorySystem.AddItemToInventory(5001);
            
            if (success)
            {
                // 显示获得道具的提示对话
                List<DialogueData> giftDialogue = new List<DialogueData>
                {
                    DialogueSystem.CreateInteractionHint("获得了石镐！", 2f)
                };
                
                if (DialogueSystem.Instance != null)
                {
                    DialogueSystem.Instance.StartDialogue(giftDialogue);
                }
            }
            else
            {
                // 显示背包已满的提示
                List<DialogueData> fullInventoryDialogue = new List<DialogueData>
                {
                    DialogueSystem.CreateInteractionHint("背包已满，无法获得石镐！", 3f)
                };
                
                if (DialogueSystem.Instance != null)
                {
                    DialogueSystem.Instance.StartDialogue(fullInventoryDialogue);
                }
            }
        }        
        /// <summary>
        /// 检查玩家是否持有镐子
        /// </summary>
        public bool HasPickaxe()
        {
            if (InventorySystem.Instance != null)
            {
                for (int i = 0; i < InventorySystem.Instance.maxSlots; i++)
                {
                    Item item = InventorySystem.Instance.GetItem(i);
                    if (!item.IsEmpty)
                    {
                        if (item.name.Contains("镐"))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        
        /// <summary>
        /// 开始挖矿数学题（供Stage5NPCInteractable调用）
        /// </summary>
        public void StartMiningQuestion()
        {
            // 生成两个 1-999 的随机整数（包含 999）
            // 注意：Unity 的 Random.Range(int, int) 上限为开区间，因此使用 1000 作为上限
            int num1 = Random.Range(1, 1000);
            int num2 = Random.Range(1, 1000);
            int product = num1 * num2;
            
            // 计算各个位数相加之和
            int sum = CalculateDigitSum(product);
            bool isPrime = IsPrime(sum);
            
            // 创建数学题
            Question mathQuestion = new Question
            {
                // 更明确的说明：请玩家仅回答“是”或“不是”
                questionText = $"{num1} 和 {num2} 相乘得到的乘积的各位数字之和，是否为质数？请仅回答：是 / 不是",
                type = QuestionType.ShortAnswer,
                playerAnswer = "",
                isAnswered = false,
                // 注意：挖矿题目不设置correctAnswer，允许任何答案提交
                // correctAnswer = isPrime ? "是" : "不是"
            };
            
            // 暂停玩家移动
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                player.SetCanMove(false);
            }
            
            // 显示题目
            if (QuestionSystem.Instance != null)
            {
                QuestionSystem.Instance.ShowQuestion(mathQuestion, (question, answer) => {
                    OnMiningQuestionAnswered(question, answer, isPrime, sum, product);
                }, player);
            }
        }
        
        /// <summary>
        /// 计算数字各位数之和
        /// </summary>
        private int CalculateDigitSum(int number)
        {
            int sum = 0;
            while (number > 0)
            {
                sum += number % 10;
                number /= 10;
            }
            return sum;
        }
        
        /// <summary>
        /// 判断是否为质数
        /// </summary>
        private bool IsPrime(int number)
        {
            if (number < 2) return false;
            if (number == 2) return true;
            if (number % 2 == 0) return false;
            
            for (int i = 3; i * i <= number; i += 2)
            {
                if (number % i == 0)
                    return false;
            }
            return true;
        }
        
        /// <summary>
        /// 挖矿题目回答处理
        /// </summary>
        private void OnMiningQuestionAnswered(Question question, string answer, bool isPrime, int sum, int product)
        {
            // ⚠️ 重要：记录答案到QuestionSystem中
            if (QuestionSystem.Instance != null && question.id > 0)
            {
                QuestionSystem.Instance.AnswerQuestion(question.id, answer);
            }
            else if (question.id <= 0)
            {
                // 挖矿题目是动态生成的，没有固定ID，创建一个临时记录
                var tempRecord = new AnswerRecord(99999, $"挖矿题目: {question.questionText} -> {answer}");
                QuestionSystem.Instance.answerRecords.Add(tempRecord);
                
                // 手动保存存档（因为这不通过AnswerQuestion方法）
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SaveGameData();
                }
            }
            
            // 关闭作答面板
            QuestionSystem.Instance.HideQuestion();
            // 恢复玩家移动
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                player.SetCanMove(true);
            }
            
            // 判断答案正确性（更健壮：去除空格/全角空格，仅判断是否以“是”或“不是”开头）
            string normalized = (answer ?? string.Empty).Trim();
            // 去除常见空白变体
            normalized = normalized.Replace(" ", string.Empty).Replace("\u3000", string.Empty);
            bool answeredYes = normalized.StartsWith("是");
            bool answeredNo = normalized.StartsWith("不是") || normalized.StartsWith("不");
            bool isCorrect = (isPrime && answeredYes) || (!isPrime && answeredNo);
            
            // 根据答案给予奖励
            int reward = 1; // 默认1G
            if (isPrime && isCorrect)
            {
                reward = 10; // 是质数且答对：10G
            }
            else if (!isPrime && isCorrect)
            {
                reward = 4; // 不是质数且答对：4G
            }
            
            ModifyAttribute("money", reward);
            
            // 显示结果对话
            List<DialogueData> resultDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle("挖矿结果", $"乘积为 {product}，各位数之和为 {sum}"),
                DialogueSystem.CreateSubtitle("挖矿结果", isPrime ? $"{sum} 是质数" : $"{sum} 不是质数"),
                DialogueSystem.CreateSubtitle("挖矿结果", isCorrect ? "答案正确！" : "答案错误！"),
                DialogueSystem.CreateInteractionHint($"获得了 {reward}G！", 2f)
            };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(resultDialogue);
            }
        }
        
        // ================================
        // 魔将玄武占卜功能
        // ================================
        
        /// <summary>
        /// 运势类型枚举
        /// </summary>
        public enum FortuneType
        {
            DaJi,    // 大吉 - 出现比例1 - 奖励20G
            Ji,      // 吉 - 出现比例3 - 奖励10G  
            XiaoJi,  // 小吉 - 出现比例5 - 奖励5G
            Xiong    // 凶 - 出现比例1 - 奖励-5G
        }
        
        /// <summary>
        /// 魔将玄武占卜功能主入口
        /// 供InventorySlot在点击魔将玄武时调用
        /// </summary>
        public void UseMJXWDivination()
        {
            // 检查是否已启用占卜功能
            if (!canUseMJXW)
            {
                return;
            }
            
            // 检查对话系统是否空闲
            if (DialogueSystem.Instance != null && DialogueSystem.Instance.isDialogueActive)
            {
                return;
            }
            StartCoroutine(PlayMJXWDivinationSequence());
        }
        
        /// <summary>
        /// 魔将玄武占卜序列
        /// </summary>
        private IEnumerator PlayMJXWDivinationSequence()
        {
            // 暂停玩家移动
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                player.SetCanMove(false);
            }
            
            // 生成随机运势
            FortuneType fortune = GenerateRandomFortune();
            
            // 根据运势类型获取文本和奖励
            string fortuneText = GetFortuneText(fortune);
            int reward = GetFortuneReward(fortune);
            
            // 创建占卜对话
            List<DialogueData> divinationDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle("魔将玄武", $"让我等为您占卜。您今天的运势为：{fortuneText}")
            };
            
            // 开始对话
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(divinationDialogue);
                yield return new WaitUntil(() => !DialogueSystem.Instance.isDialogueActive);
            }
            
            // 应用运势效果
            ApplyFortuneEffect(reward);
            
            // 增加使用次数并检查善恶值扣减
            IncrementMJXWUsage();
            
            // 恢复玩家移动
            if (player != null)
            {
                player.SetCanMove(true);
            }
        }
        
        /// <summary>
        /// 根据比例生成随机运势
        /// 大吉:吉:小吉:凶 = 1:3:5:1
        /// </summary>
        private FortuneType GenerateRandomFortune()
        {
            // 总权重：1+3+5+1 = 10
            int randomValue = Random.Range(0, 10);
            
            if (randomValue < 1)
                return FortuneType.DaJi;    // 0 - 大吉(1/10)
            else if (randomValue < 4)
                return FortuneType.Ji;      // 1-3 - 吉(3/10)
            else if (randomValue < 9)
                return FortuneType.XiaoJi;  // 4-8 - 小吉(5/10)
            else
                return FortuneType.Xiong;   // 9 - 凶(1/10)
        }
        
        /// <summary>
        /// 获取运势对应的文本
        /// </summary>
        private string GetFortuneText(FortuneType fortune)
        {
            switch (fortune)
            {
                case FortuneType.DaJi:
                    return "大吉";
                case FortuneType.Ji:
                    return "吉";
                case FortuneType.XiaoJi:
                    return "小吉";
                case FortuneType.Xiong:
                    return "凶";
                default:
                    return "小吉";
            }
        }
        
        /// <summary>
        /// 获取运势对应的金钱奖励
        /// </summary>
        private int GetFortuneReward(FortuneType fortune)
        {
            switch (fortune)
            {
                case FortuneType.DaJi:
                    return 20;   // 大吉：+20G
                case FortuneType.Ji:
                    return 10;   // 吉：+10G
                case FortuneType.XiaoJi:
                    return 5;    // 小吉：+5G
                case FortuneType.Xiong:
                    return -5;   // 凶：-5G
                default:
                    return 5;
            }
        }
        
        /// <summary>
        /// 应用运势效果（修改金钱）
        /// </summary>
        private void ApplyFortuneEffect(int reward)
        {
            ModifyAttribute("money", reward);
            
            // 显示效果提示
            string effectText = reward > 0 ? $"获得了{reward}G！" : $"失去了{Mathf.Abs(reward)}G！";
            
            List<DialogueData> effectDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateInteractionHint(effectText, 2f)
            };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(effectDialogue);
            }
        }
        
        /// <summary>
        /// 增加魔将玄武使用次数，每使用十次善恶值-1
        /// </summary>
        private void IncrementMJXWUsage()
        {
            mjxwUsageCount++;
            
            // 每使用十次，善恶值-1
            if (mjxwUsageCount % 10 == 0)
            {
                ModifyAttribute("goodevil", -1);
            }
        }
        
        /// <summary>
        /// 获取魔将玄武使用次数（调试用）
        /// </summary>
        public int GetMJXWUsageCount()
        {
            return mjxwUsageCount;
        }
        
        /// <summary>
        /// 重置魔将玄武使用次数（调试用）
        /// </summary>
        public void ResetMJXWUsageCount()
        {
            mjxwUsageCount = 0;
        }
    }
} 