using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace XuanZhiShiLian
{
    public class Stage1Controller : MonoBehaviour
    {
        [Header("UI组件")]
        public Button nextStageButton;
        
        private Question currentQuestion;
        private bool isQuestionCompleted = false;
        
        private void Start()
        {
            InitializeStage();
            StartStageSequence();
        }
        
        private void InitializeStage()
        {
            // 更新当前阶段
            GameManager.Instance.currentStage = 1;
            
            // 隐藏下一阶段按钮
            if (nextStageButton != null)
                nextStageButton.gameObject.SetActive(false);
                
            // 设置事件监听
            if (nextStageButton != null)
                nextStageButton.onClick.AddListener(OnNextStage);
        }
        
        private void StartStageSequence()
        {
            StartCoroutine(PlayStageSequence());
        }
        
        private IEnumerator PlayStageSequence()
        {
            // 渐入黑屏效果
            List<DialogueData> blackScreenDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateInteractionHint("在游戏中，按Enter可以确认对话", 0f),
                DialogueSystem.CreateInteractionHint("按ESC可以返回标题界面", 0f),
                DialogueSystem.CreateSceneDescription("【Stage1】- 序章", 0f), // 0f表示不自动播放
                DialogueSystem.CreateSubtitle("", "你是谁？"),
                DialogueSystem.CreateSubtitle("", "你从哪来？"),
                DialogueSystem.CreateSubtitle("", "要到哪里去？")
            };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(blackScreenDialogue);
                yield return new WaitUntil(() => !DialogueSystem.Instance.isDialogueActive);
            }
            
            yield return new WaitForSeconds(0.5f);
            
            // 显示题目
            ShowQuestion();
        }
        
        private void ShowQuestion()
        {
            
            // 获取Stage1的题目
            currentQuestion = QuestionSystem.Instance.GetQuestionById(1);
            if (currentQuestion != null)
            {
                // 直接使用QuestionSystem显示题目
                QuestionSystem.Instance.ShowQuestion(currentQuestion, OnQuestionAnswered);
            }
        }
        
        private void OnQuestionAnswered(Question question, string answer)
        {
            // ⚠️ 重要：记录答案到QuestionSystem中（QuestionSystem会自动保存存档）
            if (QuestionSystem.Instance != null)
            {
                QuestionSystem.Instance.AnswerQuestion(question.id, answer);
            }
            
            // 标记题目完成
            isQuestionCompleted = true;
            
            // 显示完成提示
            ShowCompletionMessage();
        }
        
        private void ShowCompletionMessage()
        {
            List<DialogueData> completionDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateInteractionHint("答案已提交", 0f), // 不自动播放在对话中
                DialogueSystem.CreateSubtitle("", "Stage1 序章完成")
            };
            
            if (DialogueSystem.Instance != null)
            {
                // 为对话添加完成回调，直接跳转到Stage2
                DialogueSystem.Instance.StartDialogue(completionDialogue, OnDialogueComplete);
            }
        }
        
        private void OnDialogueComplete()
        {
            OnNextStage(); // 直接跳转，不需要按钮
        }
        
        private void OnNextStage()
        {
            // 完成当前阶段
            GameManager.Instance.CompleteStage(1);
            
            // 加载下一个场景
            GameManager.Instance.LoadScene("Stage2");
        }
        
        private void Update()
        {
            // 按ESC返回主菜单
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                GameManager.Instance.HandleEscapeKey();
            }
        }
    }
} 