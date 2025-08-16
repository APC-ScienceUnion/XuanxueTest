using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace XuanZhiShiLian
{
    [System.Serializable]
    public class DialogueOption
    {
        public string optionText;
        public System.Action onSelected;
        
        public DialogueOption(string text, System.Action callback)
        {
            optionText = text;
            onSelected = callback;
        }
    }
    
    [System.Serializable]
    public class DialogueData
    {
        public string speaker;
        public string text;
        public DialogueType type;
        public float displayTime;
        public bool requiresInput;
        
        // 新增：选项支持
        public List<DialogueOption> options;
        public bool hasOptions => options != null && options.Count > 0;
        
        // 构造函数
        public DialogueData()
        {
            options = new List<DialogueOption>();
        }
    }
    
    public enum DialogueType
    {
        Subtitle,       // 字幕 ""
        SceneDescription, // 画面描述 【】
        InteractionHint  // 交互描述 （）
    }
    
    public class DialogueSystem : MonoBehaviour
    {
        // 单例实例
        public static DialogueSystem Instance { get; private set; }
        
        [Header("UI预制件配置")]
        public string dialoguePrefabPath = "Prefab/DialoguePanel";
        
        [Header("对话配置")]
        public float typewriterSpeed = 0.05f;
        public bool autoAdvance = false;
        public float autoAdvanceDelay = 2f;
        
        [Header("样式配置")]
        public Color subtitleColor = Color.white;
        public Color sceneDescriptionColor = Color.yellow;
        public Color interactionHintColor = Color.green;
        
        // UI组件引用（从预制件中获取）
        private GameObject dialoguePanel;
        private TextMeshProUGUI dialogueText;
        private TextMeshProUGUI speakerText;
        private Button nextButton;
        private Image dialogueBackground;
        
        // 新增：选项相关UI组件
        private Transform optionsContainer;
        private Button[] optionButtons = new Button[5]; // 固定5个按钮
        
        private Queue<DialogueData> dialogueQueue = new Queue<DialogueData>();
        private bool isTyping = false;
        public bool isDialogueActive = false;
        private Coroutine typingCoroutine;
        
        // 新增：对话锁定机制，防止其他交互系统打断对话
        private static bool isDialogueLocked = false;
        public static bool IsDialogueLocked => isDialogueLocked;
        
        // 回调支持
        private System.Action onDialogueComplete;
        
        // 新增：当前对话数据缓存，用于检查是否有选项
        private DialogueData currentDialogue;
        
        // 新增：玩家控制器引用，用于控制玩家移动
        private PlayerController playerController;
        
        // 新增：对话会话版本号，用于检测选项回调期间是否重启对话
        private int dialogueSessionId = 0;
        
        private void Awake()
        {
            // 单例模式初始化
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            // 不在Start中初始化UI，而是在需要时动态加载
            // 获取玩家控制器引用
            FindPlayerController();
        }
        
        private void FindPlayerController()
        {
            if (playerController == null)
            {
                GameObject playerObject = GameObject.FindWithTag("Player");
                if (playerObject != null)
                {
                    playerController = playerObject.GetComponent<PlayerController>();
                    if (playerController != null)
                    {
                    }
                    else
                    {
                    }
                }
                else
                {
                }
            }
        }
        
        private void Update()
        {
            if (isDialogueActive && Input.GetKeyDown(KeyCode.Return))
            {
                // 只有在文字完全显示完毕且没有选项显示时才能按回车继续
                // 修复：确保在有选项的对话中完全禁用Enter键
                if (!isTyping && !HasVisibleOptions() && !IsCurrentDialogueHasOptions())
                {
                    ShowNextDialogue();
                }
                // 移除了打字时按Enter立即完成的功能
            }

            // 强化：当对话激活时，每帧确保禁用玩家移动（涵盖动画事件触发等所有入口）
            if (isDialogueActive)
            {
                if (playerController == null)
                {
                    FindPlayerController();
                }
                if (playerController != null)
                {
                    playerController.SetCanMove(false);
                }
            }
        }
        
        private void InitializeUI()
        {
            // 如果UI已经存在，直接返回
            if (dialoguePanel != null) return;
            
            // 从Resources文件夹加载预制件
            GameObject prefab = Resources.Load<GameObject>(dialoguePrefabPath);
            if (prefab != null)
            {
                // 实例化预制件到Canvas上
                GameObject canvasGameObject = GameObject.FindWithTag("Canvas");
                Canvas canvas = canvasGameObject != null ? canvasGameObject.GetComponent<Canvas>() : null;
                if (canvas != null)
                {
                    dialoguePanel = Instantiate(prefab, canvas.transform);
                }
                else
                {
                    dialoguePanel = Instantiate(prefab);
                }
                
                // 从实例化的预制件中获取组件引用
                GetUIComponentsFromPrefab();
                
                // 初始状态隐藏面板
                dialoguePanel.SetActive(false);
                
                // 设置按钮事件
                if (nextButton != null)
                {
                    nextButton.onClick.RemoveAllListeners();
                    nextButton.onClick.AddListener(ShowNextDialogue);
                }
            }
        }
        
        private void GetUIComponentsFromPrefab()
        {
            if (dialoguePanel == null) return;
            
            // 通过名称查找子组件
            Transform dialogueTextTransform = dialoguePanel.transform.Find("DialogueText");
            if (dialogueTextTransform != null)
                dialogueText = dialogueTextTransform.GetComponent<TextMeshProUGUI>();
                
            Transform speakerTextTransform = dialoguePanel.transform.Find("SpeakerText");
            if (speakerTextTransform != null)
                speakerText = speakerTextTransform.GetComponent<TextMeshProUGUI>();
                
            Transform nextButtonTransform = dialoguePanel.transform.Find("NextButton");
            if (nextButtonTransform != null)
                nextButton = nextButtonTransform.GetComponent<Button>();
                
            Transform backgroundTransform = dialoguePanel.transform.Find("Background");
            if (backgroundTransform != null)
                dialogueBackground = backgroundTransform.GetComponent<Image>();
                
            // 新增：查找选项容器和按钮
            Transform optionsContainerTransform = dialoguePanel.transform.Find("OptionsContainer");
            if (optionsContainerTransform != null)
            {
                optionsContainer = optionsContainerTransform;
                FindOptionButtons();
            }
                
            // 如果直接查找失败，尝试递归查找
            if (dialogueText == null)
                dialogueText = dialoguePanel.GetComponentInChildren<TextMeshProUGUI>();
            if (nextButton == null)
                nextButton = dialoguePanel.GetComponentInChildren<Button>();
            if (dialogueBackground == null)
                dialogueBackground = dialoguePanel.GetComponentInChildren<Image>();
        }
        
        private void FindOptionButtons()
        {
            if (optionsContainer == null)
            {
                return;
            }
            
            // 查找Button1到Button5
            for (int i = 1; i <= 5; i++)
            {
                string buttonName = $"Button{i}";
                Transform buttonTransform = optionsContainer.Find(buttonName);
                
                if (buttonTransform != null)
                {
                    optionButtons[i-1] = buttonTransform.GetComponent<Button>();
                    if (optionButtons[i-1] != null)
                    {
                        // 初始隐藏按钮
                        optionButtons[i-1].gameObject.SetActive(false);
                    }
                }
            }
        }
        
        public void StartDialogue(List<DialogueData> dialogues, System.Action onComplete = null)
        {
            // 确保UI组件已初始化
            if (dialoguePanel == null)
            {
                InitializeUI();
            }
            
            // 确保已找到PlayerController
            if (playerController == null)
            {
                FindPlayerController();
            }

            // 会话版本号自增：标记一次全新的对话启动
            dialogueSessionId++;

            dialogueQueue.Clear();
            foreach (DialogueData dialogue in dialogues)
            {
                dialogueQueue.Enqueue(dialogue);
            }

            // 设置完成回调
            onDialogueComplete = onComplete;

            isDialogueActive = true;
            isDialogueLocked = true; // 锁定对话，防止被其他交互系统打断
            
            // 禁用玩家移动
            if (playerController != null)
            {
                playerController.SetCanMove(false);
            }
            
            if (dialoguePanel != null)
                dialoguePanel.SetActive(true);
                
            ShowNextDialogue();
        }
        
        public void ShowNextDialogue()
        {
            if (dialogueQueue.Count == 0)
            {
                EndDialogue();
                return;
            }
            
            DialogueData currentDialogue = dialogueQueue.Dequeue();
            DisplayDialogue(currentDialogue);
        }
        
        private void DisplayDialogue(DialogueData dialogue)
        {
            // 缓存当前对话数据
            currentDialogue = dialogue;
            
            // 设置说话者
            if (speakerText != null)
            {
                speakerText.text = dialogue.speaker;
            }
            
            // 设置文本颜色
            SetTextColor(dialogue.type);
            
            // 清除之前的选项
            HideAllOptionButtons();
            
            // 开始打字机效果
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            
            typingCoroutine = StartCoroutine(TypeText(dialogue.text, dialogue));
            
            // 如果有选项，隐藏下一步按钮
            if (dialogue.hasOptions)
            {
                if (nextButton != null)
                    nextButton.gameObject.SetActive(false);
            }
            else
            {
                if (nextButton != null)
                    nextButton.gameObject.SetActive(true);
                    
                // 自动推进
                if (autoAdvance && !dialogue.requiresInput)
                {
                    StartCoroutine(AutoAdvance(dialogue.displayTime > 0 ? dialogue.displayTime : autoAdvanceDelay));
                }
            }
        }
        
        private void SetTextColor(DialogueType type)
        {
            if (dialogueText == null) return;
            
            switch (type)
            {
                case DialogueType.Subtitle:
                    dialogueText.color = subtitleColor;
                    break;
                case DialogueType.SceneDescription:
                    dialogueText.color = sceneDescriptionColor;
                    break;
                case DialogueType.InteractionHint:
                    dialogueText.color = interactionHintColor;
                    break;
            }
        }
        
        private IEnumerator TypeText(string text, DialogueData dialogue)
        {
            isTyping = true;
            dialogueText.text = "";
            
            foreach (char letter in text.ToCharArray())
            {
                dialogueText.text += letter;
                yield return new WaitForSeconds(typewriterSpeed);
            }
            
            isTyping = false;
            
            // 文本打字完成后，如果有选项则显示选项
            if (dialogue.hasOptions)
            {
                ShowOptions(dialogue.options);
            }
        }
        
        private void ShowOptions(List<DialogueOption> options)
        {
            if (optionsContainer == null || options == null || options.Count == 0)
            {
                return;
            }
            
            
            // 隐藏所有按钮
            HideAllOptionButtons();
            
            // 显示需要的按钮并设置内容
            int optionsToShow = Mathf.Min(options.Count, 5);
            for (int i = 0; i < optionsToShow; i++)
            {
                SetupOptionButton(optionButtons[i], options[i], i);
            }
            
        }
        
        private void SetupOptionButton(Button button, DialogueOption option, int index)
        {
            if (button == null)
            {
                return;
            }
            
            // 显示按钮
            button.gameObject.SetActive(true);
            
            // 禁用按钮导航，防止鼠标悬停时Enter键触发
            Navigation nav = button.navigation;
            nav.mode = Navigation.Mode.None;
            button.navigation = nav;
            
            // 设置按钮文本
            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = option.optionText;
            }
            
            // 设置按钮点击事件
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnOptionSelected(option));
            
        }
        
        private void OnOptionSelected(DialogueOption option)
        {
            
            // 隐藏所有选项按钮
            HideAllOptionButtons();
            
            // 显示下一步按钮
            if (nextButton != null)
                nextButton.gameObject.SetActive(true);
            
            // 执行选项回调
            int sessionBefore = dialogueSessionId;
            try
            {
                option.onSelected?.Invoke();
            }
            catch (System.Exception)
            {
                // 忽略回调异常，保持后续逻辑一致
            }
            
            // 仅当回调期间未重启对话时，才推进到下一条
            if (sessionBefore == dialogueSessionId)
            {
                ShowNextDialogue();
            }
        }
        
        private void HideAllOptionButtons()
        {
            for (int i = 0; i < optionButtons.Length; i++)
            {
                if (optionButtons[i] != null)
                {
                    // 恢复按钮导航功能（以免影响其他地方的使用）
                    Navigation nav = optionButtons[i].navigation;
                    nav.mode = Navigation.Mode.Automatic;
                    optionButtons[i].navigation = nav;
                    
                    optionButtons[i].gameObject.SetActive(false);
                }
            }
        }
        
        private bool HasVisibleOptions()
        {
            for (int i = 0; i < optionButtons.Length; i++)
            {
                if (optionButtons[i] != null && optionButtons[i].gameObject.activeSelf)
                {
                    return true;
                }
            }
            return false;
        }
        
        // 新增：检查当前对话是否有选项（防止Enter键误触发）
        private bool IsCurrentDialogueHasOptions()
        {
            return currentDialogue != null && currentDialogue.hasOptions;
        }
        
        private IEnumerator AutoAdvance(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (!isTyping && !HasVisibleOptions())
            {
                ShowNextDialogue();
            }
        }
        
        private void StopTyping()
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                isTyping = false;
            }
        }
        
        public void EndDialogue()
        {
            isDialogueActive = false;
            isDialogueLocked = false; // 解锁对话，允许其他交互系统工作
            currentDialogue = null; // 清除当前对话缓存
            
            // 恢复玩家移动
            if (playerController != null)
            {
                playerController.SetCanMove(true);
            }
            
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);
                
            dialogueQueue.Clear();
            HideAllOptionButtons();
            
            // 调用完成回调
            if (onDialogueComplete != null)
            {
                System.Action callback = onDialogueComplete;
                onDialogueComplete = null; // 清除回调避免重复调用
                callback.Invoke();
            }
        }
        

        
        // 快速创建对话的辅助方法
        public static DialogueData CreateSubtitle(string speaker, string text, bool requiresInput = true)
        {
            return new DialogueData
            {
                speaker = speaker,
                text = text,
                type = DialogueType.Subtitle,
                displayTime = 0,
                requiresInput = requiresInput
            };
        }
        
        public static DialogueData CreateSceneDescription(string text, float displayTime = 3f)
        {
            return new DialogueData
            {
                speaker = "",
                text = text,
                type = DialogueType.SceneDescription,
                displayTime = displayTime,
                requiresInput = false
            };
        }
        
        public static DialogueData CreateInteractionHint(string text, float displayTime = 2f)
        {
            return new DialogueData
            {
                speaker = "",
                text = text,
                type = DialogueType.InteractionHint,
                displayTime = displayTime,
                requiresInput = false
            };
        }
        
        // 新增：创建带选项的对话
        public static DialogueData CreateDialogueWithOptions(string speaker, string text, List<DialogueOption> options)
        {
            DialogueData dialogue = new DialogueData
            {
                speaker = speaker,
                text = text,
                type = DialogueType.Subtitle,
                displayTime = 0,
                requiresInput = true,
                options = options ?? new List<DialogueOption>()
            };
            
            return dialogue;
        }
        
        // 新增：创建单个选项的便捷方法
        public static DialogueOption CreateOption(string text, System.Action callback)
        {
            return new DialogueOption(text, callback);
        }
        
        // 新增：创建带选项的对话
        public static DialogueData CreateChoiceDialogue(string speaker, string text, List<DialogueOption> options)
        {
            DialogueData dialogue = new DialogueData
            {
                speaker = speaker,
                text = text,
                type = DialogueType.Subtitle,
                displayTime = 0,
                requiresInput = true,
                options = options
            };
            return dialogue;
        }
        
        // 解析需求文档中的字幕格式
        public static List<DialogueData> ParseDialogueText(string rawText)
        {
            List<DialogueData> dialogues = new List<DialogueData>();
            string[] lines = rawText.Split('\n');
            
            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine)) continue;
                
                // 解析字幕 ""
                if (trimmedLine.StartsWith("\"") && trimmedLine.EndsWith("\""))
                {
                    string text = trimmedLine.Substring(1, trimmedLine.Length - 2);
                    dialogues.Add(CreateSubtitle("", text));
                }
                // 解析画面描述 【】
                else if (trimmedLine.StartsWith("【") && trimmedLine.EndsWith("】"))
                {
                    string text = trimmedLine.Substring(1, trimmedLine.Length - 2);
                    dialogues.Add(CreateSceneDescription(text));
                }
                // 解析交互描述 （）
                else if (trimmedLine.StartsWith("（") && trimmedLine.EndsWith("）"))
                {
                    string text = trimmedLine.Substring(1, trimmedLine.Length - 2);
                    dialogues.Add(CreateInteractionHint(text));
                }
                // 解析说话者对话
                else if (trimmedLine.Contains("："))
                {
                    string[] parts = trimmedLine.Split('：');
                    if (parts.Length >= 2)
                    {
                        string speaker = parts[0];
                        string text = parts[1];
                        if (text.StartsWith("\"") && text.EndsWith("\""))
                        {
                            text = text.Substring(1, text.Length - 2);
                        }
                        dialogues.Add(CreateSubtitle(speaker, text));
                    }
                }
            }
            
            return dialogues;
        }
    }
} 
