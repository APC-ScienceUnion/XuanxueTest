using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace XuanZhiShiLian
{
    public class QuestionPanelController : MonoBehaviour
    {
        [Header("UI组件")]
        public TextMeshProUGUI questionText;
        public TextMeshProUGUI questionInfoText; // 显示题目编号和分数
        
        [Header("输入控件")]
        public TMP_InputField answerInputField;
        public Button[] choiceButtons;
        public Toggle[] choiceToggles; // 用于多选题
        
        [Header("图片输入控件")]
        [Tooltip("题目42：猪舍密码图片+输入框")]
        public UnityEngine.UI.Image imageInputImage42;
        public TMP_InputField imageInputField42;
        
        [Tooltip("题目46：棋谱图片+输入框")]
        public UnityEngine.UI.Image imageInputImage46;
        public TMP_InputField imageInputField46;
        
        [Tooltip("题目4713：塞尔达图片选择按钮（图片写死在预制件中）")]
        public Button[] imageChoiceButtons4713 = new Button[4];
        
        [Header("控制按钮")]
        public Button closeButton;        // 关闭按钮
        public Button submitButton;       // 提交答案
        public Button previousButton;     // 上一题
        public Button nextButton;         // 下一题
        
        private Question currentQuestion;
        private List<Question> questionGroup;  // 当前题目组
        private int currentQuestionIndex;      // 在题目组中的索引
        private List<int> selectedChoices = new List<int>();
        private System.Action<Question, string> onAnswerSubmitted;
        private System.Action onGroupCompleted;  // 题目组完成回调
        private System.Action onClosed;          // 关闭回调
        private PlayerController nearbyPlayer;   // 附近的玩家引用
        
        private void Start()
        {
            InitializeUI();
            SetupEventListeners();
        }
        
        private void InitializeUI()
        {
            // 不要在初始化时隐藏面板，让QuestionSystem控制显示/隐藏
            // gameObject.SetActive(false); // 注释掉这行
                
            // 初始化时不要隐藏所有控件，让ShowCurrentQuestion来控制
            // HideAllInputControls(); // 注释掉这行
        }
        
        private void SetupEventListeners()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(OnClose);
            if (submitButton != null)
                submitButton.onClick.AddListener(OnSubmitAnswer);
            if (previousButton != null)
                previousButton.onClick.AddListener(OnPreviousQuestion);
            if (nextButton != null)
                nextButton.onClick.AddListener(OnNextQuestion);
        }
        
        /// <summary>
        /// 显示单个题目（无导航按钮，无关闭按钮）
        /// </summary>
        public void ShowQuestion(Question question, System.Action<Question, string> onSubmit = null, PlayerController player = null)
        {
            // 单题目模式
            questionGroup = new List<Question> { question };
            currentQuestionIndex = 0;
            onAnswerSubmitted = onSubmit;
            onGroupCompleted = null;
            onClosed = null;
            nearbyPlayer = player;  // 设置玩家引用
            
            ShowCurrentQuestion();
            UpdateButtonStates();
        }
        
        /// <summary>
        /// 显示题目组（支持导航，有关闭按钮）
        /// </summary>
        public void ShowQuestionGroup(List<Question> questions, System.Action onCompleted = null, System.Action onClose = null, PlayerController player = null)
        {
            // 展开所有嵌套的子题目为平铺列表
            List<Question> flattenedQuestions = new List<Question>();
            foreach (Question question in questions)
            {
                FlattenQuestions(question, flattenedQuestions);
            }
            // 题目组模式
            questionGroup = flattenedQuestions;
            currentQuestionIndex = 0;
            onAnswerSubmitted = null;
            onGroupCompleted = onCompleted;
            onClosed = onClose;
            nearbyPlayer = player;  // 设置玩家引用
            
            ShowCurrentQuestion();
            UpdateButtonStates();
        }

        private void RecordAnswerForQuestion(Question q, string answer)
        {
            if (QuestionSystem.Instance == null || q == null) return;
            q.playerAnswer = answer;
            q.isAnswered = true;
            // 同步到系统记录（确保子题也被加入answerRecords）
            QuestionSystem.Instance.AnswerQuestion(q.id, answer);
        }
        
        /// <summary>
        /// 递归展开嵌套的子题目为平铺列表
        /// </summary>
        private void FlattenQuestions(Question question, List<Question> flattenedList)
        {
            if (question.hasSubQuestions)
            {
                // 如果有子题目，递归展开子题目
                foreach (Question subQuestion in question.subQuestions)
                {
                    FlattenQuestions(subQuestion, flattenedList);
                }
            }
            else
            {
                // 如果没有子题目，直接添加到平铺列表
                flattenedList.Add(question);
            }
        }
        
        private void ShowCurrentQuestion()
        {
            if (questionGroup == null || currentQuestionIndex < 0 || currentQuestionIndex >= questionGroup.Count)
            {
                return;
            }
                
            currentQuestion = questionGroup[currentQuestionIndex];
            
            // 禁用玩家移动（确保玩家在答题时无法移动）
            if (nearbyPlayer != null)
            {
                nearbyPlayer.SetCanMove(false);
            }
            
            // 强制激活当前GameObject（QuestionPanel预制件自身）
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
            
            // 检查CanvasGroup（如果存在）
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
            DisplayQuestion();
        }
        
        public void HideQuestion()
        {
            // 恢复玩家移动（在隐藏问题面板时）
            if (nearbyPlayer != null)
            {
                nearbyPlayer.SetCanMove(true);
                Debug.Log("✅ HideQuestion - 玩家移动已恢复");
            }
            
            gameObject.SetActive(false);
            Debug.Log("QuestionPanel已隐藏");
                
            currentQuestion = null;
            selectedChoices.Clear();
        }
        
        private void DisplayQuestion()
        {
            if (currentQuestion == null) return;
            
            Debug.Log($"🔍 DisplayQuestion - 题目ID: {currentQuestion.id}, 类型: {currentQuestion.type}");
            
            // 显示题目文本（特殊题目定制）
            if (questionText != null)
            {
                if (currentQuestion.id == 323)
                {
                    // 题323：覆盖标题
                    questionText.text = "这个密码似乎和《如来神掌》有关";
                }
                else
                {
                    questionText.text = currentQuestion.questionText;
                }
            }
            else
                Debug.LogWarning("questionText为null，请检查预制件配置");
                
            // 显示题目信息
            if (questionInfoText != null)
            {
                string typeText = GetQuestionTypeText(currentQuestion.type);
                string displayId = FormatQuestionIdForDisplay(currentQuestion.id);
                string infoText;

                if (currentQuestion.id == 0)
                {
                    // 题0（动态挖矿题）：信息行固定为“开始挖矿”
                    infoText = "开始挖矿";
                }
                else if (currentQuestion.id == 323)
                {
                    // 题323：不展示题号
                    infoText = $"({typeText}, {currentQuestion.score}分)";
                }
                else if (currentQuestion.type == QuestionType.MultipleChoice)
                {
                    // 多选题说明
                    infoText = $"题目{displayId} ({typeText}，请选择1-4个选项, {currentQuestion.score}分)";
                }
                else
                {
                    infoText = $"题目{displayId} ({typeText}, {currentQuestion.score}分)";
                }

                questionInfoText.text = infoText;
            }
            else
            {
                Debug.LogWarning("questionInfoText为null，请检查预制件配置");
            }
            
            // 隐藏所有输入控件
            HideAllInputControls();
            
            Debug.Log($"🎯 准备根据题目类型显示UI: {currentQuestion.type}");
            
            // 根据题目类型显示相应的输入控件
            switch (currentQuestion.type)
            {
                case QuestionType.ShortAnswer:
                case QuestionType.FillInBlank:
                case QuestionType.Essay:
                case QuestionType.Puzzle:
                    ShowInputField();
                    break;
                case QuestionType.ImagePuzzle:
                    ShowImageInput42();
                    break;
                case QuestionType.ImageInput:
                    ShowImageInput46();
                    break;
                case QuestionType.SingleChoice:
                case QuestionType.TrueFalse:
                    ShowChoiceButtons(false);
                    break;
                case QuestionType.ImageChoice:
                    ShowImageChoice4713();
                    break;
                case QuestionType.MultipleChoice:
                    ShowChoiceToggles();
                    break;
                default:
                    Debug.LogError($"未知的题目类型: {currentQuestion.type}");
                    break;
            }
            
            // 显示已有答案
            if (currentQuestion.isAnswered)
            {
                DisplayExistingAnswer();
            }
        }

        /// <summary>
        /// 将题目ID格式化为展示用编号。
        /// 规则：
        /// - 对于子题（四位及以上，且存在对应父题ID的情况），格式化为 题XX-YY[-Z]
        ///   例如：4611 -> 46-1-1（若存在46父题及其子题）
        /// - 其他情况保持原ID
        /// - 对于题323的隐藏题号逻辑在调用处处理
        /// </summary>
        private string FormatQuestionIdForDisplay(int id)
        {
            // 仅在存在父题（如46或47）时，才对四位/五位子题进行格式化
            // 当前关卡中已知的父题：46、47
            bool hasParent46 = QuestionSystem.Instance?.GetQuestionById(46) != null;
            bool hasParent47 = QuestionSystem.Instance?.GetQuestionById(47) != null;

            // 四位子题（常见：4611、4713）
            if (id >= 1000 && id <= 9999)
            {
                int parent = id / 100;      // 取前两位，如 4611 -> 46
                int rest = id % 100;        // 余下两位，如 4611 -> 11
                int subA = rest / 10;       // 第一层子题序号，如 1
                int subB = rest % 10;       // 第二层子题序号，如 1

                if ((parent == 46 && hasParent46) || (parent == 47 && hasParent47))
                {
                    return $"{parent}-{subA}-{subB}";
                }
            }

            // 三位子题（如 473 -> 47-3，仅当存在47父题时）
            if (id >= 100 && id <= 999)
            {
                int parent = id / 10;       // 取前两位，如 473 -> 47
                int sub = id % 10;          // 取最后一位，如 3

                if ((parent == 46 && hasParent46) || (parent == 47 && hasParent47))
                {
                    return $"{parent}-{sub}";
                }
            }

            // 默认：原样返回
            return id.ToString();
        }
        
        private void HideAllInputControls()
        {
            // 如果answerInputField为null，先尝试查找
            if (answerInputField == null)
            {
                answerInputField = GetComponentInChildren<TMP_InputField>();
            }
            
            if (answerInputField != null)
            {
                answerInputField.gameObject.SetActive(false);
            }
                
            if (choiceButtons != null)
            {
                foreach (Button button in choiceButtons)
                {
                    if (button != null)
                        button.gameObject.SetActive(false);
                }
            }
            
            if (choiceToggles != null)
            {
                foreach (Toggle toggle in choiceToggles)
                {
                    if (toggle != null)
                        toggle.gameObject.SetActive(false);
                }
            }
            
            // 隐藏图片输入控件
            if (imageInputImage42 != null)
                imageInputImage42.gameObject.SetActive(false);
            if (imageInputField42 != null)
                imageInputField42.gameObject.SetActive(false);
                
            if (imageInputImage46 != null)
                imageInputImage46.gameObject.SetActive(false);
            if (imageInputField46 != null)
                imageInputField46.gameObject.SetActive(false);
                
            if (imageChoiceButtons4713 != null)
            {
                foreach (var btn in imageChoiceButtons4713)
                {
                    if (btn != null)
                        btn.gameObject.SetActive(false);
                }
            }
        }
        
        private void ShowInputField()
        {
            // 如果预制件中的answerInputField没有设置，尝试自动查找
            if (answerInputField == null)
            {
                answerInputField = GetComponentInChildren<TMP_InputField>();
                
                if (answerInputField == null)
                {
                    Debug.LogError("❌ 预制件中没有找到TMP_InputField组件！");
                    return;
                }
            }
            
            // 配置InputField属性以支持多行和滚动
            ConfigureInputFieldForQuestion();
            
            // 激活并清空输入框
            answerInputField.gameObject.SetActive(true);
            answerInputField.text = "";
            answerInputField.Select();
            
            Debug.Log("✅ InputField已显示并可用");
        }
        
        private void ConfigureInputFieldForQuestion()
        {
            if (answerInputField == null || currentQuestion == null) return;
            
            // 根据题目类型配置InputField
            switch (currentQuestion.type)
            {
                case QuestionType.Essay:
                    // 论述题：多行，支持滚动
                    answerInputField.lineType = TMP_InputField.LineType.MultiLineNewline;
                    answerInputField.characterLimit = 5000; // 增加字符限制
                    Debug.Log("✅ Essay题目：已配置为多行模式");
                    break;
                    
                case QuestionType.ShortAnswer:
                    // 简答题：多行，支持滚动（内容可能较长）
                    answerInputField.lineType = TMP_InputField.LineType.MultiLineNewline;
                    answerInputField.characterLimit = 1000;
                    Debug.Log("✅ 简答题：已配置为多行模式");
                    break;
                    
                case QuestionType.FillInBlank:
                case QuestionType.Puzzle:
                    // 填空题和谜题：根据题目ID或答案长度判断
                    if (currentQuestion.id == 80 || (currentQuestion.correctAnswer != null && currentQuestion.correctAnswer.Length > 50))
                    {
                        answerInputField.lineType = TMP_InputField.LineType.MultiLineNewline;
                        Debug.Log("✅ 长答案题目：已配置为多行模式");
                    }
                    else
                    {
                        answerInputField.lineType = TMP_InputField.LineType.SingleLine;
                        Debug.Log("✅ 短答案题目：已配置为单行模式");
                    }
                    answerInputField.characterLimit = 500;
                    break;
                    
                default:
                    answerInputField.lineType = TMP_InputField.LineType.SingleLine;
                    answerInputField.characterLimit = 200;
                    break;
            }
            
            // 对于多行模式，确保文本组件支持换行和滚动
            if (answerInputField.lineType == TMP_InputField.LineType.MultiLineNewline)
            {
                SetupScrollableInputField();
            }
            else
            {
                // 单行模式，恢复正常设置
                if (answerInputField.textComponent != null)
                {
                    answerInputField.textComponent.enableWordWrapping = false;
                    answerInputField.textComponent.overflowMode = TextOverflowModes.Ellipsis;
                }
            }
        }
        
        private void SetupScrollableInputField()
        {
            if (answerInputField == null) return;
            
            // 方法1：确保InputField的Viewport Area有正确的Mask
            RectTransform viewportRect = null;
            
            // 查找TextMeshPro InputField的Viewport结构
            Transform viewportTransform = answerInputField.transform.Find("Text Area/Viewport");
            if (viewportTransform != null)
            {
                viewportRect = viewportTransform.GetComponent<RectTransform>();
                
                // 确保Viewport有Mask组件
                Mask viewportMask = viewportTransform.GetComponent<Mask>();
                if (viewportMask == null)
                {
                    viewportMask = viewportTransform.gameObject.AddComponent<Mask>();
                    viewportMask.showMaskGraphic = false;
                }
                
                // 确保Viewport有Image组件（Mask需要）
                Image viewportImage = viewportTransform.GetComponent<Image>();
                if (viewportImage == null)
                {
                    viewportImage = viewportTransform.gameObject.AddComponent<Image>();
                    viewportImage.color = Color.clear; // 完全透明
                }
                
                Debug.Log("✅ 使用TextMeshPro的Viewport结构配置遮罩");
            }
            else
            {
                // 如果没找到标准的Viewport结构，在InputField本身添加Mask
                Mask inputMask = answerInputField.GetComponent<Mask>();
                if (inputMask == null)
                {
                    inputMask = answerInputField.gameObject.AddComponent<Mask>();
                    inputMask.showMaskGraphic = false;
                }
                
                // 确保InputField有Image组件
                Image inputImage = answerInputField.GetComponent<Image>();
                if (inputImage == null)
                {
                    inputImage = answerInputField.gameObject.AddComponent<Image>();
                    inputImage.color = new Color(1, 1, 1, 0.01f); // 几乎透明
                }
                
                Debug.Log("✅ 在InputField本身添加遮罩");
            }
            
            // 配置文本组件
            if (answerInputField.textComponent != null)
            {
                answerInputField.textComponent.enableWordWrapping = true;
                answerInputField.textComponent.overflowMode = TextOverflowModes.Overflow;
                
                // 确保文本组件在正确的父对象下
                RectTransform textRect = answerInputField.textComponent.GetComponent<RectTransform>();
                if (textRect != null)
                {
                    // 让文本填满其容器
                    textRect.anchorMin = Vector2.zero;
                    textRect.anchorMax = Vector2.one;
                    textRect.offsetMin = new Vector2(5, 5); // 添加一点边距
                    textRect.offsetMax = new Vector2(-5, -5);
                }
            }
            
            // 配置placeholder文本
            if (answerInputField.placeholder != null)
            {
                RectTransform placeholderRect = answerInputField.placeholder.GetComponent<RectTransform>();
                if (placeholderRect != null)
                {
                    placeholderRect.anchorMin = Vector2.zero;
                    placeholderRect.anchorMax = Vector2.one;
                    placeholderRect.offsetMin = new Vector2(5, 5);
                    placeholderRect.offsetMax = new Vector2(-5, -5);
                }
            }
            
            // 设置InputField的滚动属性
            answerInputField.scrollSensitivity = 20f;
            
            Debug.Log("✅ InputField滚动和遮罩已配置");
        }
        
        private void ShowChoiceButtons(bool multipleChoice)
        {
            if (choiceButtons == null || currentQuestion.options == null)
            {
                Debug.LogError($"choiceButtons={choiceButtons!=null} 或 options={currentQuestion.options!=null}，无法显示选择按钮");
                return;
            }
            
            Debug.Log($"显示{currentQuestion.options.Length}个选择按钮");
            
            // 先清空选择状态，稍后在DisplayExistingAnswer中恢复
            selectedChoices.Clear();
            
            for (int i = 0; i < choiceButtons.Length && i < currentQuestion.options.Length; i++)
            {
                if (choiceButtons[i] != null)
                {
                    choiceButtons[i].gameObject.SetActive(true);
                    
                    TextMeshProUGUI buttonText = choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                        buttonText.text = currentQuestion.options[i];
                    
                    // 设置按钮点击事件
                    int optionIndex = i;
                    choiceButtons[i].onClick.RemoveAllListeners();
                    choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(optionIndex));
                    
                    // 初始化按钮状态为默认状态（不直接重置颜色，让DisplayExistingAnswer处理）
                    if (!currentQuestion.isAnswered)
                    {
                        ColorBlock colors = choiceButtons[i].colors;
                        colors.normalColor = Color.white;
                        colors.highlightedColor = Color.white * 1.2f;
                        colors.pressedColor = Color.white * 0.8f;
                        colors.selectedColor = Color.white;
                        choiceButtons[i].colors = colors;
                    }
                    
                    Debug.Log($"按钮{i}: {currentQuestion.options[i]}");
                }
                else
                {
                    Debug.LogWarning($"choiceButtons[{i}]为null");
                }
            }
            
            // 隐藏多余的按钮
            for (int i = currentQuestion.options.Length; i < choiceButtons.Length; i++)
            {
                if (choiceButtons[i] != null)
                {
                    choiceButtons[i].gameObject.SetActive(false);
                }
            }
        }
        
        private void ShowChoiceToggles()
        {
            if (choiceToggles == null || currentQuestion.options == null)
            {
                Debug.LogError($"choiceToggles={choiceToggles!=null} 或 options={currentQuestion.options!=null}，无法显示多选按钮");
                return;
            }
            
            Debug.Log($"显示{currentQuestion.options.Length}个多选切换按钮");
            
            // 先清空选择状态，避免与DisplayExistingAnswer冲突
            selectedChoices.Clear();
            
            for (int i = 0; i < choiceToggles.Length && i < currentQuestion.options.Length; i++)
            {
                if (choiceToggles[i] != null)
                {
                    choiceToggles[i].gameObject.SetActive(true);
                    
                    TextMeshProUGUI toggleText = choiceToggles[i].GetComponentInChildren<TextMeshProUGUI>();
                    if (toggleText != null)
                        toggleText.text = currentQuestion.options[i];
                    
                    // 先移除事件监听器，避免在设置isOn时触发事件
                    choiceToggles[i].onValueChanged.RemoveAllListeners();
                    
                    // 重置toggle状态为false
                    choiceToggles[i].isOn = false;
                    
                    // 设置toggle事件
                    int optionIndex = i;
                    choiceToggles[i].onValueChanged.AddListener((isOn) => OnToggleChanged(optionIndex, isOn));
                    
                    Debug.Log($"切换按钮{i}: {currentQuestion.options[i]}");
                }
                else
                {
                    Debug.LogWarning($"choiceToggles[{i}]为null");
                }
            }
            
            // 隐藏多余的toggle按钮
            for (int i = currentQuestion.options.Length; i < choiceToggles.Length; i++)
            {
                if (choiceToggles[i] != null)
                {
                    choiceToggles[i].gameObject.SetActive(false);
                }
            }
        }
        
        private void DisplayExistingAnswer()
        {
            if (currentQuestion.type == QuestionType.ShortAnswer || 
                currentQuestion.type == QuestionType.FillInBlank || 
                currentQuestion.type == QuestionType.Essay)
            {
                if (answerInputField != null)
                    answerInputField.text = currentQuestion.playerAnswer;
            }
            else if (currentQuestion.type == QuestionType.SingleChoice || 
                     currentQuestion.type == QuestionType.TrueFalse)
            {
                // 高亮选中的选项并同步内部状态
                if (int.TryParse(currentQuestion.playerAnswer, out int selectedIndex))
                {
                    // 同步内部状态
                    selectedChoices.Clear();
                    selectedChoices.Add(selectedIndex);
                    
                    // 高亮按钮
                    HighlightChoiceButton(selectedIndex);
                    
                    Debug.Log($"恢复单选/判断题选择状态: 选项{selectedIndex}");
                }
            }
            else if (currentQuestion.type == QuestionType.ImageChoice)
            {
                // 高亮选中的图片选项并同步内部状态
                if (int.TryParse(currentQuestion.playerAnswer, out int selectedIndex))
                {
                    // 同步内部状态
                    selectedChoices.Clear();
                    selectedChoices.Add(selectedIndex);
                    
                    // 高亮图片按钮
                    HighlightImageChoiceButton(selectedIndex);
                    
                    Debug.Log($"恢复图片选择题选择状态: 选项{selectedIndex}");
                }
            }
            else if (currentQuestion.type == QuestionType.MultipleChoice)
            {
                // 清空之前的选择状态
                selectedChoices.Clear();
                
                // 设置多选题的选中状态
                if (!string.IsNullOrEmpty(currentQuestion.playerAnswer))
                {
                    string[] selectedOptions = currentQuestion.playerAnswer.Split(',');
                    foreach (string option in selectedOptions)
                    {
                        if (int.TryParse(option, out int index) && index < choiceToggles.Length)
                        {
                            if (choiceToggles[index] != null)
                            {
                                choiceToggles[index].isOn = true;
                                selectedChoices.Add(index);
                            }
                        }
                    }
                    Debug.Log($"恢复多选题选择状态: {string.Join(",", selectedChoices)}");
                }
            }
        }
        
        private void OnChoiceSelected(int optionIndex)
        {
            selectedChoices.Clear();
            selectedChoices.Add(optionIndex);
            
            Debug.Log($"选择了选项{optionIndex}");
            
            // 高亮选中的按钮
            HighlightChoiceButton(optionIndex);
            
            // 立即保存选择状态到当前题目
            SaveCurrentAnswer();
            
            // 延迟一帧后再次刷新确保状态正确
            StartCoroutine(RefreshButtonStateNextFrame(optionIndex));
        }
        
        private void OnImageChoiceSelected(int optionIndex)
        {
            selectedChoices.Clear();
            selectedChoices.Add(optionIndex);
            
            Debug.Log($"选择了图片选项{optionIndex}");
            
            // 高亮选中的图片按钮
            HighlightImageChoiceButton(optionIndex);
            
            // 立即保存选择状态到当前题目
            SaveCurrentAnswer();
        }
        
        private System.Collections.IEnumerator RefreshButtonStateNextFrame(int selectedIndex)
        {
            yield return null; // 等待一帧
            
            // 再次确保按钮状态正确
            HighlightChoiceButton(selectedIndex);
            
            Debug.Log($"延迟刷新完成 - 确保选项{selectedIndex}正确高亮");
        }
        
        private void HighlightChoiceButton(int selectedIndex)
        {
            if (choiceButtons == null) 
            {
                Debug.LogWarning("choiceButtons为null，无法高亮按钮");
                return;
            }
            
            Debug.Log($"高亮按钮{selectedIndex}，总按钮数: {choiceButtons.Length}");
            
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                if (choiceButtons[i] != null)
                {
                    ColorBlock colors = choiceButtons[i].colors;
                    
                    if (i == selectedIndex)
                    {
                        // 选中状态：设置为绿色
                        colors.normalColor = Color.green;
                        colors.highlightedColor = Color.green * 1.2f;
                        colors.pressedColor = Color.green * 0.8f;
                        colors.selectedColor = Color.green;
                        Debug.Log($"按钮{i}设置为选中状态（绿色）");
                    }
                    else
                    {
                        // 未选中状态：设置为白色
                        colors.normalColor = Color.white;
                        colors.highlightedColor = Color.white * 1.2f;
                        colors.pressedColor = Color.white * 0.8f;
                        colors.selectedColor = Color.white;
                    }
                    
                    choiceButtons[i].colors = colors;
                    
                    // 强制刷新按钮状态
                    choiceButtons[i].targetGraphic?.SetAllDirty();
                }
                else
                {
                    Debug.LogWarning($"choiceButtons[{i}]为null");
                }
            }
            
            // 强制刷新Canvas
            Canvas.ForceUpdateCanvases();
        }
        
        private void HighlightImageChoiceButton(int selectedIndex)
        {
            if (imageChoiceButtons4713 == null) 
            {
                Debug.LogWarning("imageChoiceButtons4713为null，无法高亮图片按钮");
                return;
            }
            
            Debug.Log($"高亮图片按钮{selectedIndex}，总按钮数: {imageChoiceButtons4713.Length}");
            
            for (int i = 0; i < imageChoiceButtons4713.Length; i++)
            {
                if (imageChoiceButtons4713[i] != null)
                {
                    ColorBlock colors = imageChoiceButtons4713[i].colors;
                    
                    if (i == selectedIndex)
                    {
                        // 选中状态：设置为绿色
                        colors.normalColor = Color.green;
                        colors.highlightedColor = Color.green * 1.2f;
                        colors.pressedColor = Color.green * 0.8f;
                        colors.selectedColor = Color.green;
                        Debug.Log($"图片按钮{i}设置为选中状态（绿色）");
                    }
                    else
                    {
                        // 未选中状态：设置为白色
                        colors.normalColor = Color.white;
                        colors.highlightedColor = Color.white * 1.2f;
                        colors.pressedColor = Color.white * 0.8f;
                        colors.selectedColor = Color.white;
                    }
                    
                    imageChoiceButtons4713[i].colors = colors;
                    
                    // 强制刷新按钮状态
                    imageChoiceButtons4713[i].targetGraphic?.SetAllDirty();
                }
                else
                {
                    Debug.LogWarning($"imageChoiceButtons4713[{i}]为null");
                }
            }
            
            // 强制刷新Canvas
            Canvas.ForceUpdateCanvases();
        }
        
        private void OnToggleChanged(int optionIndex, bool isOn)
        {
            if (isOn)
            {
                if (!selectedChoices.Contains(optionIndex))
                    selectedChoices.Add(optionIndex);
            }
            else
            {
                selectedChoices.Remove(optionIndex);
            }
        }
        
        private void OnSubmitAnswer()
        {
            
            string answer = GetCurrentAnswer();
            
            string errorMessage = GetValidationErrorMessage(answer);
            
            // 保存当前答案，并确保当前题目的答案记录到系统
            SaveCurrentAnswer();
            if (!string.IsNullOrEmpty(currentQuestion.playerAnswer))
            {
                QuestionSystem.Instance.AnswerQuestion(currentQuestion.id, currentQuestion.playerAnswer);
            }
            
            if (questionGroup.Count == 1)
            {
                Debug.Log("单题目模式");
                
                // 单题目模式：直接提交
                if (onAnswerSubmitted != null)
                {
                    Debug.Log($"调用onAnswerSubmitted回调: 题目ID={currentQuestion.id}, 答案='{answer}'");
                    onAnswerSubmitted.Invoke(currentQuestion, answer);
                }
                
                HideQuestion();
            }
            else
            {
                Debug.Log("题目组模式：提交所有题目");
                
                // 题目组模式：提交所有题目（包含多层子题）
                foreach (Question question in questionGroup)
                {
                    if (question != null && !string.IsNullOrEmpty(question.playerAnswer))
                    {
                        QuestionSystem.Instance.AnswerQuestion(question.id, question.playerAnswer);
                    }
                }
                
                HideQuestion();
                onGroupCompleted?.Invoke();
            }
        }
        
        private string GetCurrentAnswer()
        {
            switch (currentQuestion.type)
            {
                case QuestionType.ShortAnswer:
                case QuestionType.FillInBlank:
                case QuestionType.Essay:
                case QuestionType.Puzzle:
                    return answerInputField != null ? answerInputField.text.Trim() : "";
                    
                case QuestionType.ImagePuzzle:
                    return imageInputField42 != null ? imageInputField42.text.Trim() : "";
                    
                case QuestionType.ImageInput:
                    return imageInputField46 != null ? imageInputField46.text.Trim() : "";
                    
                case QuestionType.SingleChoice:
                case QuestionType.TrueFalse:
                case QuestionType.ImageChoice:
                    return selectedChoices.Count > 0 ? selectedChoices[0].ToString() : "";
                    
                case QuestionType.MultipleChoice:
                    return string.Join(",", selectedChoices);
                    
                default:
                    return "";
            }
        }
        
        private string GetValidationErrorMessage(string answer)
        {
            if (currentQuestion == null) return "题目信息错误";
            
            switch (currentQuestion.type)
            {
                case QuestionType.ShortAnswer:
                case QuestionType.FillInBlank:
                case QuestionType.Essay:
                case QuestionType.Puzzle:
                case QuestionType.ImagePuzzle:
                case QuestionType.ImageInput:
                    if (string.IsNullOrEmpty(answer.Trim()))
                        return "请输入答案";
                    // 添加答案正确性验证（如果有标准答案）
                    if (!string.IsNullOrEmpty(currentQuestion.correctAnswer))
                    {
                        // 去除所有空格和换行符进行比较，与Stage4Controller保持一致
                        string playerAnswer = answer?.Replace(" ", "").Replace("\r", "").Replace("\n", "").Trim();
                        string correctAnswer = currentQuestion.correctAnswer?.Replace(" ", "").Replace("\r", "").Replace("\n", "").Trim();
                        if (!string.Equals(playerAnswer, correctAnswer, System.StringComparison.OrdinalIgnoreCase))
                            return "答案不正确，请重新输入";
                    }
                    break;
                    
                case QuestionType.SingleChoice:
                case QuestionType.TrueFalse:
                case QuestionType.ImageChoice:
                    if (string.IsNullOrEmpty(answer))
                        return "请选择一个选项";
                    // 添加答案正确性验证
                    if (!string.IsNullOrEmpty(currentQuestion.correctAnswer))
                    {
                        // 去除所有空格和换行符进行比较，与Stage4Controller保持一致
                        string playerAnswer = answer?.Replace(" ", "").Replace("\r", "").Replace("\n", "").Trim();
                        string correctAnswer = currentQuestion.correctAnswer?.Replace(" ", "").Replace("\r", "").Replace("\n", "").Trim();
                        if (!string.Equals(playerAnswer, correctAnswer, System.StringComparison.OrdinalIgnoreCase))
                            return "答案不正确，请重新选择";
                    }
                    break;
                    
                case QuestionType.MultipleChoice:
                    if (selectedChoices.Count == 0)
                        return "请至少选择一个选项";
                    if (selectedChoices.Count < 2)
                        return "请选择至少2个选项";
                    if (selectedChoices.Count > 4)
                        return "最多只能选择4个选项";
                    // 添加答案正确性验证
                    if (currentQuestion.correctAnswerIndices != null && currentQuestion.correctAnswerIndices.Length > 0)
                    {
                        var sortedSelected = new List<int>(selectedChoices);
                        var sortedCorrect = new List<int>(currentQuestion.correctAnswerIndices);
                        sortedSelected.Sort();
                        sortedCorrect.Sort();
                        
                        if (sortedSelected.Count != sortedCorrect.Count)
                            return "选择的选项数量不正确";
                        
                        for (int i = 0; i < sortedSelected.Count; i++)
                        {
                            if (sortedSelected[i] != sortedCorrect[i])
                                return "答案不正确，请重新选择";
                        }
                    }
                    break;
            }
            
            return ""; // 验证通过
        }
        
        private bool ValidateAnswer(string answer)
        {
            return string.IsNullOrEmpty(GetValidationErrorMessage(answer));
        }
        
        private void OnPreviousQuestion()
        {
            if (questionGroup != null && currentQuestionIndex > 0)
            {
                // 保存当前题目答案
                SaveCurrentAnswer();
                
                // 切换到上一题
                currentQuestionIndex--;
                ShowCurrentQuestion();
                UpdateButtonStates();
            }
        }
        
        private void OnNextQuestion()
        {
            if (questionGroup != null && currentQuestionIndex < questionGroup.Count - 1)
            {
                // 保存当前题目答案
                SaveCurrentAnswer();
                
                // 切换到下一题
                currentQuestionIndex++;
                ShowCurrentQuestion();
                UpdateButtonStates();
            }
        }
        
        private void OnClose()
        {
            // 恢复玩家移动（无论单题目还是题目组模式）
            if (nearbyPlayer != null)
            {
                nearbyPlayer.SetCanMove(true);
                Debug.Log("✅ OnClose - 玩家移动已恢复");
            }
            
            HideQuestion();
            
            // 根据模式调用不同的回调
            bool isSingleQuestion = questionGroup != null && questionGroup.Count == 1;
            if (isSingleQuestion)
            {
                // 单题目模式：直接隐藏，不调用任何回调
                Debug.Log("📝 单题目模式关闭 - 直接隐藏面板");
            }
            else
            {
                // 题目组模式：调用关闭回调
                Debug.Log("📋 题目组模式关闭 - 调用onClosed回调");
                onClosed?.Invoke();
            }
        }
        
        private void SaveCurrentAnswer()
        {
            if (currentQuestion == null) return;
            
            string answer = GetCurrentAnswer();
            if (!string.IsNullOrEmpty(answer))
            {
                // 临时保存答案到题目对象中，但不提交到QuestionSystem
                currentQuestion.playerAnswer = answer;
                currentQuestion.isAnswered = true;
            }
        }
        
        private string GetQuestionTypeText(QuestionType type)
        {
            switch (type)
            {
                case QuestionType.ShortAnswer:
                    return "简答题";
                case QuestionType.SingleChoice:
                    return "单选题";
                case QuestionType.MultipleChoice:
                    return "多选题";
                case QuestionType.TrueFalse:
                    return "判断题";
                case QuestionType.FillInBlank:
                    return "填空题";
                case QuestionType.Essay:
                    return "论述题";
                case QuestionType.Puzzle:
                    return "谜题";
                case QuestionType.ImagePuzzle:
                    return "图片谜题";
                case QuestionType.ImageInput:
                    return "图片题";
                case QuestionType.ImageChoice:
                    return "图片选择题";
                default:
                    return "未知题型";
            }
        }
        
        private void UpdateButtonStates()
        {
            bool isSingleQuestion = questionGroup.Count == 1;
            bool isFirstQuestion = currentQuestionIndex == 0;
            bool isLastQuestion = currentQuestionIndex == questionGroup.Count - 1;
            
            Debug.Log($"🔘 UpdateButtonStates - 单题模式: {isSingleQuestion}, 第一题: {isFirstQuestion}, 最后一题: {isLastQuestion}");
            
            // 关闭按钮：总是显示，但单题目模式和题目组模式的行为不同
            if (closeButton != null)
            {
                closeButton.gameObject.SetActive(true);  // 总是显示关闭按钮
                Debug.Log($"关闭按钮显示: true");
            }
            else
            {
                Debug.LogWarning("closeButton为null");
            }
            
            // 上一题按钮：不是第一题且在题目组模式时显示
            if (previousButton != null)
            {
                previousButton.gameObject.SetActive(!isSingleQuestion && !isFirstQuestion);
                Debug.Log($"上一题按钮显示: {!isSingleQuestion && !isFirstQuestion}");
            }
            else
            {
                Debug.LogWarning("previousButton为null");
            }
            
            // 下一题按钮：不是最后一题且在题目组模式时显示
            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(!isSingleQuestion && !isLastQuestion);
                Debug.Log($"下一题按钮显示: {!isSingleQuestion && !isLastQuestion}");
            }
            else
            {
                Debug.LogWarning("nextButton为null");
            }
            
            // 提交按钮：单题目模式总是显示，题目组模式只在最后一题显示
            if (submitButton != null)
            {
                if (isSingleQuestion)
                {
                    submitButton.gameObject.SetActive(true);
                    TextMeshProUGUI buttonText = submitButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        buttonText.text = "提交答案";
                        Debug.Log("✅ 单题模式：提交按钮已激活，文本设置为'提交答案'");
                    }
                    else
                    {
                        Debug.LogWarning("⚠️ submitButton上没有找到TextMeshProUGUI组件");
                    }
                }
                else
                {
                    submitButton.gameObject.SetActive(isLastQuestion);
                    if (isLastQuestion)
                    {
                        TextMeshProUGUI buttonText = submitButton.GetComponentInChildren<TextMeshProUGUI>();
                        if (buttonText != null)
                            buttonText.text = "提交题目组";
                        Debug.Log("✅ 题目组模式：最后一题，提交按钮已激活");
                    }
                    else
                    {
                        Debug.Log("题目组模式：非最后一题，提交按钮已隐藏");
                    }
                }
            }
            else
            {
                Debug.LogError("❌ submitButton为null！这是关键问题！");
            }
        }
        
        private void Update()
        {
            // 移除危险的回车键提交功能，用户应该使用提交按钮
            // 特别是对于多行文本输入，回车键应该用于换行
        }

        private void ShowImageInput42()
        {
            if (imageInputImage42 != null)
            {
                // 题目42图片已写死在预制件中，只需激活即可
                imageInputImage42.gameObject.SetActive(true);
                Debug.Log("✅ Image-42已激活（图片写死在预制件中）");
            }
            else
            {
                Debug.LogError("❌ imageInputImage42引用为null，请检查预制件配置");
            }
            
            if (imageInputField42 != null)
            {
                imageInputField42.gameObject.SetActive(true);
                imageInputField42.text = "";
                imageInputField42.Select();
                imageInputField42.ActivateInputField();
                Debug.Log("✅ InputField-42已激活");
            }
            else
            {
                Debug.LogError("❌ imageInputField42引用为null，请检查预制件配置");
            }
            
            Debug.Log("显示题目42图片输入控件（图片已写死）");
        }
        
        private void ShowImageInput46()
        {
            if (imageInputImage46 != null)
            {
                imageInputImage46.gameObject.SetActive(true);
                if (!string.IsNullOrEmpty(currentQuestion.imagePath))
                {
                    LoadAndDisplayImage(currentQuestion.imagePath, imageInputImage46);
                }
            }
            
            if (imageInputField46 != null)
            {
                imageInputField46.gameObject.SetActive(true);
                imageInputField46.text = "";
                imageInputField46.Select();
                imageInputField46.ActivateInputField();
            }
            
            Debug.Log("显示题目46图片输入控件");
        }
        
        private void ShowImageChoice4713()
        {
            if (imageChoiceButtons4713 != null)
            {
                for (int i = 0; i < imageChoiceButtons4713.Length; i++)
                {
                    if (imageChoiceButtons4713[i] != null)
                    {
                        imageChoiceButtons4713[i].gameObject.SetActive(true);
                        
                        // 设置按钮点击事件 - 使用专门的ImageChoice选择方法
                        int optionIndex = i;
                        imageChoiceButtons4713[i].onClick.RemoveAllListeners();
                        imageChoiceButtons4713[i].onClick.AddListener(() => OnImageChoiceSelected(optionIndex));
                        
                        // 初始化按钮状态为默认状态
                        if (!currentQuestion.isAnswered)
                        {
                            ColorBlock colors = imageChoiceButtons4713[i].colors;
                            colors.normalColor = Color.white;
                            colors.highlightedColor = Color.white * 1.2f;
                            colors.pressedColor = Color.white * 0.8f;
                            colors.selectedColor = Color.white;
                            imageChoiceButtons4713[i].colors = colors;
                        }
                    }
                }
            }
            
            Debug.Log("显示题目4713图片选择控件");
        }
        
        private void LoadAndDisplayImage(string imagePath, UnityEngine.UI.Image targetImage)
        {
            if (targetImage == null)
            {
                Debug.LogWarning($"目标图片组件为null，无法显示图片: {imagePath}");
                return;
            }
            
            if (string.IsNullOrEmpty(imagePath))
            {
                Debug.LogWarning("图片路径为空");
                targetImage.gameObject.SetActive(false);
                return;
            }
            
            Debug.Log($"尝试加载Sprite: Resources/{imagePath}");
            
            // 直接从Resources加载Sprite
            Sprite sprite = Resources.Load<Sprite>(imagePath);
            
            if (sprite != null)
            {
                targetImage.sprite = sprite;
                targetImage.gameObject.SetActive(true);
                Debug.Log($"✅ 成功加载并显示图片: {imagePath}");
            }
            else
            {
                // 显示占位符
                targetImage.sprite = null;
                targetImage.gameObject.SetActive(true);
                targetImage.color = new Color(0.8f, 0.8f, 0.8f, 1f); // 灰色占位符
            }
        }
    }
} 

