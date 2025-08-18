using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;
using System.Text;
using System;

namespace XuanZhiShiLian
{
    [System.Serializable]
    public class AnswerRecord
    {
        public int questionId;
        public string answer;
        public string submitTime;
        
        public AnswerRecord(int id, string ans)
        {
            questionId = id;
            answer = ans;
            submitTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
    
    [System.Serializable]
    public class Question
    {
        public int id;
        public string questionText;
        public QuestionType type;
        public string[] options; // 用于选择题
        public string correctAnswer; // 仅用于客观题的标准答案参考
        public int[] correctAnswerIndices; // 仅用于多选题的标准答案参考
        public int score; // 题目分值（仅用于显示和导出）
        public string playerAnswer;
        public bool isAnswered;
        // 新增：图片支持
        public string imagePath; // 题目配图路径
        public string[] imagePaths; // 多张图片路径（用于复杂题目）
        // 新增：子题目支持
        public Question[] subQuestions; // 子题目数组
        public bool hasSubQuestions => subQuestions != null && subQuestions.Length > 0;
        // 新增：钥匙和提示系统（用于Stage4）
        public string keyReward; // 完成题目后获得的钥匙
        public string[] hints; // 题目提示信息
        // 移除了 isCorrect 和 earnedScore 字段，因为不在游戏内评分
    }
    
    public enum QuestionType
    {
        ShortAnswer,    // 简答题
        SingleChoice,   // 单选题
        MultipleChoice, // 多选题
        TrueFalse,      // 判断题
        FillInBlank,    // 填空题
        Essay,          // 大创作题
        Puzzle,         // 谜题（用于Stage4复杂题目）
        ImagePuzzle,    // 图片谜题（题目42：猪舍密码）
        ImageInput,     // 图片+输入框（题目46的三道图片题）
        ImageChoice     // 图片选择题（题目4713：塞尔达选择）
    }
    
    [System.Serializable]
    public class QuestionData
    {
        public int id;
        public string questionText;
        public string type;
        public string[] options;
        public string correctAnswer;
        public int[] correctAnswerIndices;
        public int score;
        public int stageNumber;
        // 新增：图片支持
        public string imagePath; // 题目配图路径
        public string[] imagePaths; // 多张图片路径
        // 新增：子题目支持
        public QuestionData[] subQuestions; // 子题目数据
        // 新增：钥匙和提示系统
        public string keyReward; // 完成题目后获得的钥匙
        public string[] hints; // 题目提示信息
    }
    
    [System.Serializable]
    public class QuestionsContainer
    {
        public QuestionData[] questions;
    }
    
    public class QuestionSystem : MonoBehaviour
    {
        // 单例实例
        public static QuestionSystem Instance { get; private set; }
        
        [Header("UI预制件配置")]
        public string questionPanelPrefabPath = "Prefab/QuestionPanel";
        
        [Header("答题记录")]
        public Dictionary<int, string> answerSheet = new Dictionary<int, string>();
        public List<AnswerRecord> answerRecords = new List<AnswerRecord>();
        
        // 私有字段
        private List<Question> allQuestions = new List<Question>();
        private List<Question> currentStageQuestions = new List<Question>();
        
        // UI组件引用（从预制件中获取）
        private GameObject questionPanel;
        private QuestionPanelController questionPanelController;
        
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
            InitializeQuestions();
            InitializeUI();
        }
        
        private void InitializeUI()
        {
            // 如果UI已经存在，直接返回
            if (questionPanel != null) 
            {
                return;
            }
            
            // 从Resources文件夹加载预制件
            GameObject prefab = Resources.Load<GameObject>(questionPanelPrefabPath);
            if (prefab != null)
            {
                
                // 查找或创建Canvas
                GameObject canvasGameObject = GameObject.FindWithTag("Canvas");
                Canvas canvas = canvasGameObject != null ? canvasGameObject.GetComponent<Canvas>() : null;
                if (canvas == null)
                {
                    GameObject canvasGO = new GameObject("Canvas");
                    canvas = canvasGO.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvas.sortingOrder = 100; // 确保在最前面
                    
                    CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1920, 1080);
                    
                    canvasGO.AddComponent<GraphicRaycaster>();
                    
                    // 创建EventSystem（如果不存在）
                    if (FindObjectOfType<EventSystem>() == null)
                    {
                        Debug.LogWarning("场景中未找到EventSystem，正在创建...");
                        GameObject eventSystemGO = new GameObject("EventSystem");
                        eventSystemGO.AddComponent<EventSystem>();
                        eventSystemGO.AddComponent<StandaloneInputModule>();
                    }
                }
                
                // 实例化预制件到Canvas上
                questionPanel = Instantiate(prefab, canvas.transform);
                
                // 确保QuestionPanel正确设置为全屏显示
                RectTransform rectTransform = questionPanel.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    // 设置锚点为全屏
                    rectTransform.anchorMin = Vector2.zero;
                    rectTransform.anchorMax = Vector2.one;
                    rectTransform.offsetMin = Vector2.zero;
                    rectTransform.offsetMax = Vector2.zero;
                    rectTransform.anchoredPosition = Vector2.zero;
                }
                
                // 确保在Canvas的最前面
                questionPanel.transform.SetAsLastSibling();
                
                // 从实例化的预制件中获取控制器组件
                questionPanelController = questionPanel.GetComponent<QuestionPanelController>();
                
                if (questionPanelController != null)
                {
                    // 初始状态隐藏面板
                    questionPanel.SetActive(false);
                }
            }
        }
        
        /// <summary>
        /// 显示题目
        /// </summary>
        public void ShowQuestion(Question question, System.Action<Question, string> onAnswered = null, PlayerController player = null)
        {
            // 确保UI组件已初始化
            if (questionPanel == null)
            {
                InitializeUI();
            }
            
            if (questionPanelController != null)
            {
                questionPanelController.ShowQuestion(question, onAnswered, player);
            }
            else
            {
                // 尝试重新获取组件
                if (questionPanel != null)
                {
                    questionPanelController = questionPanel.GetComponent<QuestionPanelController>();
                    if (questionPanelController != null)
                    {
                        questionPanelController.ShowQuestion(question, onAnswered, player);
                    }
                }
            }
        }
        
        /// <summary>
        /// 显示题目组
        /// </summary>
        public void ShowQuestionGroup(List<Question> questions, System.Action onCompleted = null, System.Action onClosed = null, PlayerController player = null)
        {
            // 确保UI组件已初始化
            if (questionPanel == null)
            {
                InitializeUI();
            }
            
            if (questionPanelController != null)
            {
                questionPanelController.ShowQuestionGroup(questions, onCompleted, onClosed, player);
            }
        }
        
        /// <summary>
        /// 隐藏题目面板
        /// </summary>
        public void HideQuestion()
        {
            // Stage0 禁止关闭题目面板
            if (GameManager.Instance != null && GameManager.Instance.currentStage == 1)
            {
                return;
            }
            if (questionPanel != null)
            {
                questionPanel.SetActive(false);
            }
        }
        
        private void InitializeQuestions()
        {
            allQuestions.Clear();
            
            // 从JSON文件加载题目数据
            TextAsset jsonFile = Resources.Load<TextAsset>("Data/Questions");
            if (jsonFile != null)
            {
                try
                {
                    QuestionsContainer container = JsonUtility.FromJson<QuestionsContainer>(jsonFile.text);
                    
                    foreach (QuestionData questionData in container.questions)
                    {
                        Question question = CreateQuestionFromData(questionData);
                        allQuestions.Add(question);
                    }
                }
                catch (System.Exception e)
                {
                    CreateDefaultQuestions(); // 回退到默认题目
                }
            }
            else
            {
                CreateDefaultQuestions(); // 回退到默认题目
            }
        }
        
        private Question CreateQuestionFromData(QuestionData data)
        {
            Question question = new Question
            {
                id = data.id,
                questionText = data.questionText,
                type = ParseQuestionType(data.type),
                options = data.options,
                correctAnswer = data.correctAnswer,
                correctAnswerIndices = data.correctAnswerIndices,
                score = data.score,
                playerAnswer = "",
                isAnswered = false,
                // 新增字段
                imagePath = data.imagePath,
                imagePaths = data.imagePaths,
                keyReward = data.keyReward,
                hints = data.hints
            };
            
            // 处理子题目
            if (data.subQuestions != null && data.subQuestions.Length > 0)
            {
                question.subQuestions = new Question[data.subQuestions.Length];
                for (int i = 0; i < data.subQuestions.Length; i++)
                {
                    // 递归创建子题目
                    Question sub = CreateQuestionFromData(data.subQuestions[i]);
                    question.subQuestions[i] = sub;
                    // 确保子题目也被加入到全局题目列表，以便通过ID检索和记录答题
                    if (allQuestions.Find(q => q.id == sub.id) == null)
                    {
                        allQuestions.Add(sub);
                    }
                }
            }
            
            return question;
        }
        
        private QuestionType ParseQuestionType(string typeString)
        {
            switch (typeString)
            {
                case "ShortAnswer": return QuestionType.ShortAnswer;
                case "SingleChoice": return QuestionType.SingleChoice;
                case "MultipleChoice": return QuestionType.MultipleChoice;
                case "TrueFalse": return QuestionType.TrueFalse;
                case "FillInBlank": return QuestionType.FillInBlank;
                case "Essay": return QuestionType.Essay;
                case "Puzzle": return QuestionType.Puzzle;
                case "ImagePuzzle": return QuestionType.ImagePuzzle;
                case "ImageInput": return QuestionType.ImageInput;
                case "ImageChoice": return QuestionType.ImageChoice;
                default:
                    return QuestionType.ShortAnswer;
            }
        }
        
        private void CreateDefaultQuestions()
        {
            
            Question q1 = new Question
            {
                id = 1,
                questionText = "你是谁？",
                type = QuestionType.ShortAnswer,
                options = null,
                correctAnswer = "",
                correctAnswerIndices = null,
                score = 5,
                playerAnswer = "",
                isAnswered = false
            };
            allQuestions.Add(q1);
            
            Question q2 = new Question
            {
                id = 2,
                questionText = "我是我吗？",
                type = QuestionType.ShortAnswer,
                options = null,
                correctAnswer = "",
                correctAnswerIndices = null,
                score = 5,
                playerAnswer = "",
                isAnswered = false
            };
            allQuestions.Add(q2);
        }
        
        public List<Question> GetStageQuestions(int stageNumber)
        {
            List<Question> stageQuestions = new List<Question>();
            
            // 先尝试从JSON数据的stageNumber字段获取
            foreach (Question question in allQuestions)
            {
                // 从JSON数据重新加载以获取stageNumber
                int questionStage = GetStageFromQuestionData(question.id);
                if (questionStage == stageNumber)
                {
                    stageQuestions.Add(question);
                }
            }
            
            Debug.Log($"为Stage{stageNumber}找到 {stageQuestions.Count} 道题目");
            return stageQuestions;
        }
        
        private int GetStageFromQuestionData(int questionId)
        {
            // 从原始JSON数据中查找题目的stageNumber
            TextAsset jsonFile = Resources.Load<TextAsset>("Data/Questions");
            if (jsonFile != null)
            {
                try
                {
                    QuestionsContainer container = JsonUtility.FromJson<QuestionsContainer>(jsonFile.text);
                    foreach (QuestionData questionData in container.questions)
                    {
                        if (questionData.id == questionId)
                        {
                            return questionData.stageNumber;
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"读取题目stageNumber失败: {e.Message}");
                }
            }
            
            // 如果从JSON读取失败，回退到ID范围判断
            return GetStageByQuestionId(questionId);
        }
        
        private int GetStageByQuestionId(int questionId)
        {
            // 根据题目ID范围判断所属Stage（作为备用方法）
            if (questionId == 1) return 1;
            if (questionId >= 2 && questionId <= 8) return 2;
            if (questionId >= 9 && questionId <= 40) return 3;
            if (questionId >= 41 && questionId <= 47) return 4; // Stage4的题目范围
            if (questionId >= 48 && questionId <= 52) return 5; // Stage5的场景题目
            if (questionId == 80) return 6; // 大创作题
            return 0; // 未知
        }
        
        public Question GetQuestionById(int id)
        {
            return allQuestions.Find(q => q.id == id);
        }
        
        public List<Question> GetAllQuestions()
        {
            return allQuestions;
        }
        
        public void AnswerQuestion(int questionId, string answer)
        {
            Question question = GetQuestionById(questionId);
            if (question != null)
            {
                question.playerAnswer = answer;
                question.isAnswered = true;
                
                // 更新答题记录
                answerSheet[questionId] = answer;
                
                // 添加或更新答题记录
                AnswerRecord existingRecord = answerRecords.Find(r => r.questionId == questionId);
                if (existingRecord != null)
                {
                    existingRecord.answer = answer;
                    existingRecord.submitTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    answerRecords.Add(new AnswerRecord(questionId, answer));
                }
                
                // 立即保存存档
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SaveGameData();
                }
                
                // 更新进度（仅统计已答题数量）
                UpdateAnsweredCount();
            }
        }

        /// <summary>
        /// 检查答案是否正确（忽略标点与大小写，支持中文数字一到九映射为1到9）
        /// </summary>
        /// <param name="playerAnswer">玩家答案</param>
        /// <param name="correctAnswer">标准答案</param>
        /// <returns>是否匹配</returns>
        public bool IsAnswerCorrect(string playerAnswer, string correctAnswer)
        {
            if (string.IsNullOrEmpty(playerAnswer) || string.IsNullOrEmpty(correctAnswer))
            {
                return false;
            }

            string normalizedPlayer = NormalizeAnswer(playerAnswer);
            string normalizedCorrect = NormalizeAnswer(correctAnswer);

            return string.Equals(normalizedPlayer, normalizedCorrect, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 多选题答案比较（忽略顺序）
        /// </summary>
        public bool IsMultipleChoiceCorrect(int[] playerAnswerIndices, int[] correctAnswerIndices)
        {
            if (playerAnswerIndices == null || correctAnswerIndices == null)
            {
                return false;
            }
            if (playerAnswerIndices.Length != correctAnswerIndices.Length)
            {
                return false;
            }

            int[] a = new int[playerAnswerIndices.Length];
            int[] b = new int[correctAnswerIndices.Length];
            Array.Copy(playerAnswerIndices, a, a.Length);
            Array.Copy(correctAnswerIndices, b, b.Length);
            Array.Sort(a);
            Array.Sort(b);
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }

        /// <summary>
        /// 标准化答案：
        /// 1) 将中文数字一~九、零替换为阿拉伯数字0~9
        /// 2) 去除中英文标点（仅保留字母/数字/汉字/空白）
        /// 3) 转小写
        /// 4) 归一化空白
        /// </summary>
        private string NormalizeAnswer(string answer)
        {
            if (string.IsNullOrEmpty(answer)) return string.Empty;

            string text = ConvertChineseNumbersToArabic(answer);
            text = RemovePunctuation(text);
            text = text.ToLowerInvariant();
            text = System.Text.RegularExpressions.Regex.Replace(text, "\\s+", " ").Trim();
            return text;
        }

        /// <summary>
        /// 中文数字到阿拉伯数字的简单替换（支持：零/一/二/三/四/五/六/七/八/九）
        /// </summary>
        private string ConvertChineseNumbersToArabic(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            // 基本一位数字映射，按需求不处理十、百、千等复合数字
            Dictionary<char, char> map = new Dictionary<char, char>
            {
                {'零','0'}, {'〇','0'},
                {'一','1'}, {'二','2'}, {'三','3'}, {'四','4'}, {'五','5'},
                {'六','6'}, {'七','7'}, {'八','8'}, {'九','9'}
            };

            StringBuilder sb = new StringBuilder(text.Length);
            foreach (char c in text)
            {
                if (map.TryGetValue(c, out char d))
                {
                    sb.Append(d);
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 去除中英文标点，仅保留字母/数字/汉字/空白
        /// </summary>
        private string RemovePunctuation(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            StringBuilder sb = new StringBuilder(text.Length);
            foreach (char c in text)
            {
                if (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || IsChineseChar(c))
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 判断是否为常用汉字（基本区+扩展A）
        /// </summary>
        private bool IsChineseChar(char c)
        {
            return (c >= '\u4E00' && c <= '\u9FFF') ||
                   (c >= '\u3400' && c <= '\u4DBF');
        }
        
        private void UpdateAnsweredCount()
        {
            int answeredQuestions = 0;
            
            foreach (Question question in allQuestions)
            {
                if (question.isAnswered)
                {
                    answeredQuestions++;
                }
            }
            
            float progress = allQuestions.Count > 0 ? (float)answeredQuestions / allQuestions.Count : 0f;
            GameManager.Instance.UpdateProgress(progress);
            
            Debug.Log($"答题进度：{answeredQuestions}/{allQuestions.Count} ({progress:P0})");
        }
        
        public void ExportAnswerSheet()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# 玄之试炼 - 答题记录");
            sb.AppendLine($"考生姓名：{GameManager.Instance.playerName}");
            sb.AppendLine($"考试时间：{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();
            
            // 统计答题数量
            int totalAnswered = 0;
            int subjectiveCount = 0;
            int objectiveCount = 0;
            
            foreach (Question question in allQuestions)
            {
                if (question.isAnswered)
                {
                    totalAnswered++;
                    
                    if (IsSubjectiveQuestion(question.type))
                    {
                        subjectiveCount++;
                    }
                    else
                    {
                        objectiveCount++;
                    }
                }
            }
            
            sb.AppendLine("## 答题统计");
            sb.AppendLine($"- 已答题目：{totalAnswered}/{allQuestions.Count}");
            sb.AppendLine($"- 客观题：{objectiveCount}题");
            sb.AppendLine($"- 主观题：{subjectiveCount}题");
            sb.AppendLine();
            
            sb.AppendLine("## 详细答题记录");
            
            foreach (Question question in allQuestions)
            {
                if (question.isAnswered)
                {
                    sb.AppendLine($"### 题目{question.id} ({GetQuestionTypeText(question.type)}, {question.score}分)");
                    sb.AppendLine($"**题目：** {question.questionText}");
                    sb.AppendLine($"**答案：** {question.playerAnswer}");
                    
                    if (IsSubjectiveQuestion(question.type))
                    {
                        sb.AppendLine($"**状态：** 待人工阅卷");
                    }
                    else
                    {
                        sb.AppendLine($"**状态：** 客观题，待批改");
                        if (!string.IsNullOrEmpty(question.correctAnswer))
                        {
                            sb.AppendLine($"**参考答案：** {question.correctAnswer}");
                        }
                    }
                    sb.AppendLine();
                }
            }
            
            // 保存到文件
            string fileName = $"答题记录_{GameManager.Instance.playerName}_{System.DateTime.Now:yyyyMMdd_HHmmss}.md";
            string filePath = Path.Combine(Application.persistentDataPath, fileName);
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
            
            Debug.Log($"答题记录已导出到：{filePath}");
        }
        
        private bool IsSubjectiveQuestion(QuestionType type)
        {
            return type == QuestionType.ShortAnswer || 
                   type == QuestionType.FillInBlank || 
                   type == QuestionType.Essay;
        }
        
        private string GetQuestionTypeText(QuestionType type)
        {
            switch (type)
            {
                case QuestionType.ShortAnswer: return "简答题";
                case QuestionType.SingleChoice: return "单选题";
                case QuestionType.MultipleChoice: return "多选题";
                case QuestionType.TrueFalse: return "判断题";
                case QuestionType.FillInBlank: return "填空题";
                case QuestionType.Essay: return "论述题";
                default: return "未知题型";
            }
        }
        
        public bool IsStageCompleted(int stageNumber)
        {
            List<Question> stageQuestions = GetStageQuestions(stageNumber);
            foreach (Question question in stageQuestions)
            {
                if (!question.isAnswered)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 测试QuestionSystem状态的公共方法
        /// </summary>
        public void TestSystem()
        {
            Debug.Log("=== QuestionSystem 状态测试 ===");
            Debug.Log($"Instance存在: {Instance != null}");
            Debug.Log($"questionPanel存在: {questionPanel != null}");
            Debug.Log($"questionPanelController存在: {questionPanelController != null}");
            Debug.Log($"题目总数: {allQuestions.Count}");
            Debug.Log($"答题记录数: {answerRecords.Count}");
            
            if (questionPanel != null)
            {
                Debug.Log($"questionPanel激活状态: {questionPanel.activeSelf}");
                Debug.Log($"questionPanel名称: {questionPanel.name}");
            }
            
            // 测试预制件加载
            GameObject testPrefab = Resources.Load<GameObject>(questionPanelPrefabPath);
            Debug.Log($"预制件测试加载: {testPrefab != null}");
            if (testPrefab != null)
            {
                Debug.Log($"预制件名称: {testPrefab.name}");
            }
            
            Debug.Log("=== QuestionSystem 状态测试完成 ===");
        }
    }
} 