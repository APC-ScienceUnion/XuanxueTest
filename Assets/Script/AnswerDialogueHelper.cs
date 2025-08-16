using System.Collections.Generic;
using UnityEngine;

namespace XuanZhiShiLian
{
    /// <summary>
    /// 在对话中展示指定题目的玩家作答内容的助手组件。
    /// 将本组件挂到需要使用的 NPC（例如“情报屋”）上，
    /// 然后在 Stage5NPCInteractable 的某个选项的 onFollowUpDialogueComplete 里绑定 ShowPlayerAnswer()。
    /// </summary>
    public class AnswerDialogueHelper : MonoBehaviour
    {
        [Header("显示配置")]
        [Tooltip("对话中的说话者名，不填则使用该物体上可能存在的 Stage5NPCInteractable.speakerName 或物体名")] 
        public string speakerNameOverride = "";

        [Tooltip("要展示的题目ID")] 
        public int questionId = 1;

        [TextArea(2, 4)]
        [Tooltip("当玩家尚未作答该题时的提示")] 
        public string notAnsweredText = "你还没有回答第{0}题。";

        [TextArea(2, 4)]
        [Tooltip("展示答案时的格式化文本。{0}=题目ID，{1}=玩家答案")] 
        public string showAnswerFormat = "你在第{0}题的作答是：{1}";

        /// <summary>
        /// 供 Inspector 的 UnityEvent(无参) 调用。
        /// 会读取 QuestionSystem 中的作答，并通过 DialogueSystem 弹出一段对话。
        /// </summary>
        public void ShowPlayerAnswer()
        {
            string speaker = ResolveSpeakerName();

            string playerAnswer = GetPlayerAnswer(questionId);
            string text = string.IsNullOrEmpty(playerAnswer)
                ? string.Format(notAnsweredText, questionId)
                : string.Format(showAnswerFormat, questionId, playerAnswer);

            var dialogues = new List<DialogueData>
            {
                DialogueSystem.CreateSubtitle(speaker, text)
            };

            // 为避免与前一段对话同帧竞争，延迟到下一帧再启动
            StartCoroutine(StartDialogueNextFrame(dialogues));
        }

        private System.Collections.IEnumerator StartDialogueNextFrame(List<DialogueData> dialogues)
        {
            yield return null;
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(dialogues);
            }
        }

        private string ResolveSpeakerName()
        {
            if (!string.IsNullOrEmpty(speakerNameOverride))
            {
                return speakerNameOverride;
            }

            // 优先尝试同物体上的 Stage5NPCInteractable.speakerName
            var npc = GetComponent<Stage5NPCInteractable>();
            if (npc != null && !string.IsNullOrEmpty(npc.speakerName))
            {
                return npc.speakerName;
            }

            // 回退到物体名
            return gameObject.name;
        }

        private string GetPlayerAnswer(int id)
        {
            // 优先从 QuestionSystem 的 answerSheet 取值
            if (QuestionSystem.Instance != null)
            {
                if (QuestionSystem.Instance.answerSheet != null && QuestionSystem.Instance.answerSheet.TryGetValue(id, out string ans))
                {
                    return ans;
                }

                // 回退：从题目对象上取 playerAnswer
                Question q = QuestionSystem.Instance.GetQuestionById(id);
                if (q != null)
                {
                    return q.playerAnswer;
                }
            }
            return string.Empty;
        }
    }
}

