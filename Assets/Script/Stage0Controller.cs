using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace XuanZhiShiLian
{
    public class Stage0Controller : MonoBehaviour
    {
        [Header("主菜单UI")]
        public Slider progressSlider;
        public TextMeshProUGUI progressText;
        public Button startButton;
        public Button continueButton;
        public Button settingsButton;
        public Button exitButton;
        public Button exportButton;
        
        [Header("姓名输入面板")]
        public GameObject nameInputPanel;
        public TMP_InputField nameInputField;
        public Button confirmNameButton;
        
        [Header("设置面板")]
        public GameObject settingsPanel;
        public Slider volumeSlider;
        public Toggle fullscreenToggle;
        public Button closeSettingsButton;
        
        [Header("存档信息")]
        public TextMeshProUGUI saveInfoText;
        
        [Header("过场动画")]
        public GameObject transitionPanel;  // 过场动画面板
        public Animator transitionAnimator;  // 动画控制器
        public string transitionAnimationName = "TransitionIn";  // 动画名称
        
        private void Start()
        {
            InitializeUI();
            SetupEventListeners();
            
            // 延迟更新UI，确保GameManager完全初始化
            StartCoroutine(DelayedUIUpdate());
        }
        
        private IEnumerator DelayedUIUpdate()
        {
            // 等待一帧，确保所有组件都已初始化
            yield return null;
            
            // 等待GameManager初始化完成
            while (GameManager.Instance == null)
            {
                yield return null;
            }
            
            // 再等待一帧确保子系统初始化
            yield return null;
            
            UpdateUI();
        }
        
        private void InitializeUI()
        {
            // 隐藏面板
            if (nameInputPanel != null)
                nameInputPanel.SetActive(false);
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
            if (transitionPanel != null)
                transitionPanel.SetActive(false);

            // 进度条为只读展示：禁用交互与导航
            if (progressSlider != null)
            {
                progressSlider.interactable = false;
                var nav = progressSlider.navigation;
                nav.mode = UnityEngine.UI.Navigation.Mode.None;
                progressSlider.navigation = nav;
            }

            // 禁用主菜单按钮的键盘导航，避免WS切换高亮与Enter触发
            DisableButtonNavigation(startButton);
            DisableButtonNavigation(continueButton);
            DisableButtonNavigation(settingsButton);
            DisableButtonNavigation(exitButton);
            DisableButtonNavigation(exportButton);

            // 禁用EventSystem的导航事件以彻底屏蔽WS/Enter对UI的影响（不影响我们代码中手动检测的回车）
            if (EventSystem.current != null)
            {
                EventSystem.current.sendNavigationEvents = false;
                // 同时清空当前选中，防止已有选中项被回车触发
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        private void DisableButtonNavigation(Button btn)
        {
            if (btn == null) return;
            var nav = btn.navigation;
            nav.mode = Navigation.Mode.None;
            btn.navigation = nav;
        }
        
        private void SetupEventListeners()
        {
            // 主菜单按钮
            if (startButton != null)
                startButton.onClick.AddListener(OnStartGame);
            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinueGame);
            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnShowSettings);
            if (exitButton != null)
                exitButton.onClick.AddListener(OnExitGame);
            if (exportButton != null)
                exportButton.onClick.AddListener(OnExportAnswers);
                
            // 姓名输入
            if (confirmNameButton != null)
                confirmNameButton.onClick.AddListener(OnConfirmName);
                
            // 设置面板
            if (closeSettingsButton != null)
                closeSettingsButton.onClick.AddListener(OnCloseSettings);
            if (volumeSlider != null)
                volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            if (fullscreenToggle != null)
                fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggled);
        }
        
        private void UpdateUI()
        {
            // 检查GameManager是否已初始化
            if (GameManager.Instance == null)
            {
                return;
            }
            
            // 更新进度条
            if (progressSlider != null)
            {
                progressSlider.value = GameManager.Instance.totalProgress;
            }
            
            if (progressText != null)
            {
                progressText.text = $"答题进度: {GameManager.Instance.totalProgress:P0}";
            }
            
            // 更新继续按钮状态
            if (continueButton != null)
            {
                bool hasSave = GameManager.Instance.saveSystem != null && 
                              GameManager.Instance.saveSystem.HasSaveFile();
                continueButton.interactable = hasSave;
            }
            
            // 更新存档信息
            if (saveInfoText != null)
            {
                if (GameManager.Instance.saveSystem != null)
                {
                    saveInfoText.text = GameManager.Instance.saveSystem.GetSaveInfo();
                }
                else
                {
                    saveInfoText.text = "暂无存档信息";
                }
            }
            
            // 更新导出按钮状态（有存档数据就可以导出）
            if (exportButton != null)
            {
                bool hasSaveData = GameManager.Instance.saveSystem != null && 
                                  GameManager.Instance.saveSystem.HasSaveFile();
                exportButton.interactable = hasSaveData;
            }
        }
        
        /// <summary>
        /// 公共方法：刷新UI显示
        /// </summary>
        public void RefreshUI()
        {
            UpdateUI();
        }
        
        private void OnStartGame()
        {
            if (string.IsNullOrEmpty(GameManager.Instance.playerName))
            {
                // 显示姓名输入面板
                if (nameInputPanel != null)
                {
                    nameInputPanel.SetActive(true);
                    if (nameInputField != null)
                        nameInputField.Select();
                }
            }
            else
            {
                // 直接开始游戏
                StartNewGame();
            }
        }
        
        private void OnContinueGame()
        {
            if (GameManager.Instance != null && GameManager.Instance.saveSystem != null)
            {
                if (GameManager.Instance.saveSystem.HasSaveFile())
                {
                    // 先加载游戏数据
                    GameManager.Instance.LoadGameData();
                    
                    // 等待一帧确保数据完全加载
                    StartCoroutine(DelayedContinueGame());
                }
            }
        }
        
        private IEnumerator DelayedContinueGame()
        {
            yield return null; // 等待一帧
            
            // 加载对应的场景
            string targetScene = $"Stage{GameManager.Instance.currentStage}";
            GameManager.Instance.LoadScene(targetScene);
        }
        
        private void OnShowSettings()
        {
            // 显示功能未完成的对话
            ShowSettingsNotImplementedDialog();
        }
        
        /// <summary>
        /// 显示设置功能未完成的对话
        /// </summary>
        private void ShowSettingsNotImplementedDialog()
        {
            if (DialogueSystem.Instance != null)
            {
                List<DialogueData> settingsDialogue = new List<DialogueData>
                {
                    DialogueSystem.CreateSubtitle(" ", "这个功能还没做呢TT"),
                    DialogueSystem.CreateSubtitle(" ", "按Enter关闭对话")
                };
                
                DialogueSystem.Instance.StartDialogue(settingsDialogue);
            }
        }
        
        private void OnExitGame()
        {
            Application.Quit();
        }
        
        private void OnExportAnswers()
        {
            if (GameManager.Instance != null)
            {
                // 导出Markdown格式的完整存档
                ExportResult result = GameManager.Instance.ExportMarkdownSave();
                
                // 显示导出结果对话框
                ShowExportResultDialog(result);
            }
        }
        
        /// <summary>
        /// 显示导出结果对话框
        /// </summary>
        private void ShowExportResultDialog(ExportResult result)
        {
            if (DialogueSystem.Instance != null)
            {
                List<DialogueData> exportDialogue = new List<DialogueData>();
                
                if (result.Success)
                {
                    // 成功导出
                    string dialogueText = $"{result.Message}\n\n文件名：{result.FileName}\n路径：{result.FilePath}";
                    exportDialogue.Add(DialogueSystem.CreateSubtitle("系统", dialogueText));
                }
                else
                {
                    // 导出失败
                    exportDialogue.Add(DialogueSystem.CreateSubtitle("系统", $"导出失败：{result.Message}"));
                }
                
                DialogueSystem.Instance.StartDialogue(exportDialogue);
            }
            else
            {
                // 如果DialogueSystem不可用，使用UISystem显示简单消息
                if (GameManager.Instance?.uiSystem != null)
                {
                    string message = result.Success ? 
                        $"导出成功！文件：{result.FileName}" : 
                        $"导出失败：{result.Message}";
                    GameManager.Instance.uiSystem.ShowMessage(message);
                }
            }
        }
        
        private void OnConfirmName()
        {
            if (nameInputField != null && !string.IsNullOrEmpty(nameInputField.text.Trim()))
            {
                string playerName = nameInputField.text.Trim();
                GameManager.Instance.SetPlayerName(playerName);
                    
                // 刷新UI
                RefreshUI();
                
                // 开始游戏
                StartNewGame();
            }
        }
        
        private IEnumerator StartNewGameSequence()
        {
            // 重新开始：清空（或覆盖为空）存档文件，确保全新开局
            if (GameManager.Instance != null && GameManager.Instance.saveSystem != null)
            {
                // 使用实例方法删除当前配置路径下的存档
                GameManager.Instance.saveSystem.DeleteSave();
                // 无论删除是否成功，写入一份空白存档进行覆盖，防止文件被外部占用导致残留
                GameManager.Instance.saveSystem.OverwriteSaveData(new SaveData());
            }
            else
            {
                // 回退：删除默认文件名
                SaveSystem.DeleteSaveFile();
            }
            // 立即清空内存中的背包，避免切场景前的UI残留
            if (InventorySystem.Instance != null)
            {
                InventorySystem.Instance.ClearInventory();
            }
            if (GameManager.Instance != null)
            {
                // 清空内存中的保存片段，避免跨场景残留
                GameManager.Instance.savedStageData = null;
                GameManager.Instance.savedPlayerPosition = null;
                GameManager.Instance.savedCameraPosition = null;
                GameManager.Instance.savedStageInventories = null;
                GameManager.Instance.isLoadingFromSave = false;
            }
            // 简单的黑屏效果（可选）
            if (GameManager.Instance != null && GameManager.Instance.uiSystem != null)
            {
                GameManager.Instance.uiSystem.ShowMessage("正在开始试炼...");
            }
            
            yield return new WaitForSeconds(0.5f); // 短暂延迟
            
            // 播放过场动画
            yield return StartCoroutine(PlayTransitionAnimation());
            
            // 重置游戏状态
            if (GameManager.Instance != null)
            {
                GameManager.Instance.currentStage = 1;
                GameManager.Instance.totalProgress = 0f;
                
                // 清空答题记录
                if (QuestionSystem.Instance != null)
                {
                    QuestionSystem.Instance.answerSheet.Clear();
                    QuestionSystem.Instance.answerRecords.Clear();
                }
                
                // 不在Stage0保存（会被拦截），载入Stage1后会自动保存为全新进度
                
                // 加载Stage1
                GameManager.Instance.LoadScene("Stage1");
            }
        }
        
        /// <summary>
        /// 等待过场动画播放完成
        /// </summary>
        private IEnumerator WaitForTransitionAnimation()
        {
            float animationTime = 5.8f; // 动画总时长
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
        /// 播放过场动画
        /// </summary>
        public IEnumerator PlayTransitionAnimation()
        {
            if (transitionPanel != null && transitionAnimator != null)
            {
                // 禁用主UI交互
                SetMainUIInteractable(false);
                
                transitionPanel.SetActive(true);
                transitionAnimator.Play(transitionAnimationName);
                
                // 等待动画播放完成
                yield return StartCoroutine(WaitForTransitionAnimation());
                
                
                // 重新启用主UI交互
                SetMainUIInteractable(true);
            }
        }
        
        /// <summary>
        /// 设置主UI的交互状态
        /// </summary>
        private void SetMainUIInteractable(bool interactable)
        {
            if (startButton != null) startButton.interactable = interactable;
            if (continueButton != null) continueButton.interactable = interactable;
            if (settingsButton != null) settingsButton.interactable = interactable;
            if (exitButton != null) exitButton.interactable = interactable;
            if (exportButton != null) exportButton.interactable = interactable;
        }
        
        private void StartNewGame()
        {
            StartCoroutine(StartNewGameSequence());
        }
        
        private void OnCloseSettings()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
        }
        
        private void OnVolumeChanged(float value)
        {
            AudioListener.volume = value;
        }
        
        private void OnFullscreenToggled(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
        }
        
        private void Update()
        {
            // 按ESC关闭面板
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (nameInputPanel != null && nameInputPanel.activeSelf)
                {
                    nameInputPanel.SetActive(false);
                }
                else if (settingsPanel != null && settingsPanel.activeSelf)
                {
                    settingsPanel.SetActive(false);
                }
            }
            
            // 在姓名输入面板中按Enter确认
            if (nameInputPanel != null && nameInputPanel.activeSelf && Input.GetKeyDown(KeyCode.Return))
            {
                OnConfirmName();
            }
        }
    }
} 