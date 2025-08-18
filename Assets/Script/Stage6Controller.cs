using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XuanZhiShiLian
{
    public class Stage6Controller : MonoBehaviour
    {
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
            GameManager.Instance.currentStage = 6;
        }
        
        private void StartStageSequence()
        {
            StartCoroutine(PlayStageSequence());
        }
        
        private IEnumerator PlayStageSequence()
        {
            // 开场对话
            List<DialogueData> introDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle("【我】", "那么，还剩最后一件事...")
            };
            
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(introDialogue);
                yield return new WaitUntil(() => !DialogueSystem.Instance.isDialogueActive);
            }
            
            yield return new WaitForSeconds(0.5f);
            
            // 显示题目
            ShowQuestion();
        }
        
        private void ShowQuestion()
        {
            // 获取题目80
            currentQuestion = QuestionSystem.Instance.GetQuestionById(80);
            if (currentQuestion != null)
            {
                // 使用QuestionSystem显示题目
                QuestionSystem.Instance.ShowQuestion(currentQuestion, OnQuestionAnswered);
            }
        }
        
        private void OnQuestionAnswered(Question question, string answer)
        {
            // 记录答案到QuestionSystem中
            if (QuestionSystem.Instance != null)
            {
                QuestionSystem.Instance.AnswerQuestion(question.id, answer);
            }
            
            // 标记题目完成
            isQuestionCompleted = true;
            
            // 显示完成提示并跳转
            ShowCompletionMessage();
        }
        
        private void ShowCompletionMessage()
        {
            List<DialogueData> completionDialogue = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle("", "感谢赏玩！")
            };
            
            if (DialogueSystem.Instance != null)
            {
                // 为对话添加完成回调，直接跳转到Stage0
                DialogueSystem.Instance.StartDialogue(completionDialogue, OnDialogueComplete);
            }
        }
        
        private void OnDialogueComplete()
        {
            OnComplete(); // 直接跳转，不需要按钮
        }
        
        private void OnComplete()
        {
            // 完成当前阶段
            GameManager.Instance.CompleteStage(6);
            GameManager.Instance.LoadScene("Stage0");
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