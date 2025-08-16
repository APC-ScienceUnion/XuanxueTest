using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System;

namespace XuanZhiShiLian
{
    /// <summary>
    /// 专门用于导出答题记录的工具类
    /// 提供多种格式的答题记录导出功能
    /// </summary>
    public static class AnswerRecordExporter
    {
        /// <summary>
        /// 导出完整的答题记录为JSON格式
        /// </summary>
        public static void ExportAnswersToJson()
        {
            try
            {
                if (QuestionSystem.Instance == null)
                {
                    return;
                }

                var exportData = new
                {
                    playerName = GameManager.Instance?.playerName ?? "未知玩家",
                    exportTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    totalQuestions = QuestionSystem.Instance.GetAllQuestions().Count,
                    answeredQuestions = QuestionSystem.Instance.answerRecords.Count,
                    answers = QuestionSystem.Instance.answerRecords
                };

                string jsonData = JsonUtility.ToJson(exportData, true);
                string fileName = $"答题记录_{exportData.playerName}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                string filePath = Path.Combine(Application.persistentDataPath, fileName);

                File.WriteAllText(filePath, jsonData, Encoding.UTF8);
            }
            catch (Exception e)
            {
            }
        }

        /// <summary>
        /// 导出Stage5选择记录和答题记录的组合JSON
        /// </summary>
        public static void ExportCompleteGameRecord()
        {
            try
            {
                List<AnswerRecord> answers = QuestionSystem.Instance?.answerRecords ?? new List<AnswerRecord>();
                List<Stage5ChoiceRecord> choices = GameManager.Instance?.saveSystem?.GetStage5Choices() ?? new List<Stage5ChoiceRecord>();
                var saveData = GameManager.Instance?.saveSystem?.GetSaveData();
                List<StageInventoryEntry> stageInventories = saveData?.stageInventories ?? new List<StageInventoryEntry>();

                var completeRecord = new
                {
                    playerName = GameManager.Instance?.playerName ?? "未知玩家",
                    exportTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    gameVersion = "1.0.0",
                    currentStage = GameManager.Instance?.currentStage ?? 0,
                    
                    // 答题统计
                    answerSummary = new
                    {
                        totalAnswered = answers.Count,
                        answerDetails = answers
                    },
                    
                    // Stage5选择统计
                    stage5Summary = new
                    {
                        totalChoices = choices.Count,
                        choiceDetails = choices
                    },
                    
                    // 多Stage背包（包含Stage2~Stage5等，按存档为准）
                    inventoriesByStage = stageInventories,
                    
                    // 详细记录
                    answerRecords = answers,
                    stage5Choices = choices
                };

                string jsonData = JsonUtility.ToJson(completeRecord, true);
                string fileName = $"完整游戏记录_{completeRecord.playerName}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                string filePath = Path.Combine(Application.persistentDataPath, fileName);

                File.WriteAllText(filePath, jsonData, Encoding.UTF8);
            }
            catch (Exception e)
            {
            }
        }

        /// <summary>
        /// 导出简化版答题记录（仅包含题目ID和答案）
        /// </summary>
        public static void ExportSimpleAnswerRecord()
        {
            try
            {
                if (QuestionSystem.Instance == null)
                {
                    return;
                }

                var simpleAnswers = new List<object>();
                foreach (var record in QuestionSystem.Instance.answerRecords)
                {
                    simpleAnswers.Add(new
                    {
                        questionId = record.questionId,
                        answer = record.answer
                    });
                }

                var exportData = new
                {
                    playerName = GameManager.Instance?.playerName ?? "未知玩家",
                    exportTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    simpleAnswers = simpleAnswers
                };

                string jsonData = JsonUtility.ToJson(exportData, true);
                string fileName = $"简化答题记录_{exportData.playerName}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                string filePath = Path.Combine(Application.persistentDataPath, fileName);

                File.WriteAllText(filePath, jsonData, Encoding.UTF8);
            }
            catch (Exception e)
            {
            }
        }

        /// <summary>
        /// 创建答题记录的CSV格式导出
        /// </summary>
        public static void ExportAnswersToCSV()
        {
            try
            {
                if (QuestionSystem.Instance == null)
                {
                    return;
                }

                StringBuilder csv = new StringBuilder();
                csv.AppendLine("题目ID,答案,提交时间");

                foreach (var record in QuestionSystem.Instance.answerRecords)
                {
                    // 处理答案中的逗号和换行符
                    string cleanAnswer = record.answer.Replace(",", "，").Replace("\n", " ").Replace("\r", "");
                    csv.AppendLine($"{record.questionId},\"{cleanAnswer}\",{record.submitTime}");
                }

                string fileName = $"答题记录_{GameManager.Instance?.playerName ?? "未知"}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                string filePath = Path.Combine(Application.persistentDataPath, fileName);

                File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);
            }
            catch (Exception e)
            {
            }
        }

        /// <summary>
        /// 获取存档文件保存路径
        /// </summary>
        public static string GetSaveDataPath()
        {
            return Application.persistentDataPath;
        }

        /// <summary>
        /// 在文件资源管理器中打开存档文件夹
        /// </summary>
        public static void OpenSaveDataFolder()
        {
            string path = Application.persistentDataPath;
            
            try
            {
                #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                System.Diagnostics.Process.Start("explorer.exe", path);
                #elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                System.Diagnostics.Process.Start("open", path);
                #elif UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
                System.Diagnostics.Process.Start("xdg-open", path);
                #endif
                
            }
            catch (Exception e)
            {
            }
        }
    }
}

