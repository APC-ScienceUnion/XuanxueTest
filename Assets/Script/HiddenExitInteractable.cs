using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace XuanZhiShiLian
{
    // 3层洞口交互对象 - 处理隐藏出口和备用出口逻辑
    public class HiddenExitInteractable : Interactable
    {
        public Stage4Controller stage4Controller;
        public SceneTransitionInteractable normalHoleTransition; // 正常的洞口跳跃功能
        
        private int interactionCount = 0; // 交互次数计数
        
        public override void Interact()
        {
            if (stage4Controller != null)
            {
                // 检查是否有6把钥匙 - 隐藏出口
                if (stage4Controller.HasAllKeys())
                {
                    TriggerHiddenExit();
                }
                // 检查是否有3层钥匙 - 如果有钥匙，建议使用楼梯
                else if (stage4Controller.HasAnyFloor3Key())
                {
                    ShowStairSuggestion();
                }
                // 无钥匙情况 - 计数交互次数
                else
                {
                    HandleNoKeyInteraction();
                }
            }
        }
        
        private void TriggerHiddenExit()
        {
            if (DialogueSystem.Instance != null)
            {
                List<DialogueData> hiddenDialogue = new List<DialogueData>
                {
                    DialogueSystem.CreateSceneDescription("【你集齐了所有6把钥匙！】"),
                    DialogueSystem.CreateInteractionHint("隐藏的智慧之路为你开启！", 3f)
                };
                DialogueSystem.Instance.StartDialogue(hiddenDialogue, OnHiddenExitConfirmed);
            }
        }
        
        private void OnHiddenExitConfirmed()
        {
            if (stage4Controller != null)
            {
                stage4Controller.OnHiddenExitTriggered();
            }
        }
        
        private void ShowStairSuggestion()
        {
            if (DialogueSystem.Instance != null)
            {
                List<DialogueData> suggestionDialogue = new List<DialogueData>
                {
                    DialogueSystem.CreateSubtitle("我", "我有钥匙可以使用楼梯，为什么要跳下去呢？"),
                    DialogueSystem.CreateInteractionHint("建议使用楼梯前往下一关", 2f)
                };
                DialogueSystem.Instance.StartDialogue(suggestionDialogue, OnSuggestionComplete);
            }
        }
        
        private void OnSuggestionComplete()
        {
            // 如果玩家坚持要跳，执行正常跌落
            if (normalHoleTransition != null)
            {
                normalHoleTransition.Interact();
            }
        }
        
        private void HandleNoKeyInteraction()
        {
            interactionCount++;
            
            if (interactionCount >= 3)
            {
                // 第3次交互，触发备用出口
                TriggerBackupExit();
            }
            else
            {
                // 前两次交互，显示提示
                ShowNoKeyWarning();
            }
        }
        
        private void ShowNoKeyWarning()
        {
            if (DialogueSystem.Instance != null)
            {
                string message = "";
                if (interactionCount == 1)
                {
                    message = "我没有钥匙无法离开，跳下去只会回到一层...";
                }
                else if (interactionCount == 2)
                {
                    message = "再这样下去就要被困在这里了，要不要再试一次？";
                }
                
                List<DialogueData> warningDialogue = new List<DialogueData>
                {
                    DialogueSystem.CreateSubtitle("我", message),
                    DialogueSystem.CreateInteractionHint($"再交互{3 - interactionCount}次可以强制离开", 2f)
                };
                DialogueSystem.Instance.StartDialogue(warningDialogue, OnWarningComplete);
            }
        }
        
        private void OnWarningComplete()
        {
            // 执行正常跌落到1层
            if (normalHoleTransition != null)
            {
                normalHoleTransition.Interact();
            }
        }
        
        private void TriggerBackupExit()
        {
            if (DialogueSystem.Instance != null)
            {
                List<DialogueData> backupDialogue = new List<DialogueData>
                {
                    DialogueSystem.CreateSubtitle("我", "算了，不管怎样都要继续前进..."),
                    DialogueSystem.CreateSceneDescription("【强行突破，前往下一关】"),
                    DialogueSystem.CreateInteractionHint("触发备用出口", 2f)
                };
                DialogueSystem.Instance.StartDialogue(backupDialogue, OnBackupExitConfirmed);
            }
        }
        
        private void OnBackupExitConfirmed()
        {
            // 直接跳转到Stage5
            StartCoroutine(LoadNextStage());
        }
        
        private System.Collections.IEnumerator LoadNextStage()
        {
            yield return new WaitForSeconds(1f);
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadScene("Stage5");
            }
        }
        
        // 重置交互计数（例如当玩家离开3层后重新回来）
        public void ResetInteractionCount()
        {
            interactionCount = 0;
        }
    }
} 