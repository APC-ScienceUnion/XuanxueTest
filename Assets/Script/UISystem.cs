using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace XuanZhiShiLian
{
    /// <summary>
    /// 简化的UI系统 - 只负责真正需要的通用UI功能
    /// </summary>
    public class UISystem : MonoBehaviour
    {
        [Header("预制件引用")]
        public GameObject confirmationDialogPrefab;
        
        [Header("动态创建的UI")]
        private GameObject confirmationDialog;
        private System.Action onConfirmCallback;
        private System.Action onCancelCallback;
        
        /// <summary>
        /// 显示确认对话框
        /// </summary>
        public void ShowConfirmation(string message, System.Action onConfirm, System.Action onCancel = null)
        {
            // 如果没有预制件，则动态创建简单的确认框
            if (confirmationDialogPrefab == null)
            {
                CreateSimpleConfirmationDialog(message, onConfirm, onCancel);
            }
            else
            {
                // 使用预制件创建确认框
                CreateConfirmationFromPrefab(message, onConfirm, onCancel);
            }
        }
        
        /// <summary>
        /// 动态创建简单确认对话框
        /// </summary>
        private void CreateSimpleConfirmationDialog(string message, System.Action onConfirm, System.Action onCancel)
        {
            if (confirmationDialog != null)
            {
                Destroy(confirmationDialog);
            }
            
            // 找到Canvas
            GameObject canvasGameObject = GameObject.FindWithTag("Canvas");
            Canvas canvas = canvasGameObject != null ? canvasGameObject.GetComponent<Canvas>() : null;
            if (canvas == null)
            {
                return;
            }
            
            // 创建对话框
            confirmationDialog = new GameObject("ConfirmationDialog");
            confirmationDialog.transform.SetParent(canvas.transform, false);
            
            // 添加背景
            Image bg = confirmationDialog.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.7f);
            
            RectTransform bgRect = confirmationDialog.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            
            // 创建内容面板
            GameObject contentPanel = new GameObject("ContentPanel");
            contentPanel.transform.SetParent(confirmationDialog.transform, false);
            
            Image contentBg = contentPanel.AddComponent<Image>();
            contentBg.color = Color.white;
            
            RectTransform contentRect = contentPanel.GetComponent<RectTransform>();
            contentRect.sizeDelta = new Vector2(400, 200);
            
            // 创建文本
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(contentPanel.transform, false);
            
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = message;
            text.color = Color.black;
            text.alignment = TextAlignmentOptions.Center;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.1f, 0.5f);
            textRect.anchorMax = new Vector2(0.9f, 0.9f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            // 创建按钮容器
            GameObject buttonContainer = new GameObject("ButtonContainer");
            buttonContainer.transform.SetParent(contentPanel.transform, false);
            
            HorizontalLayoutGroup layout = buttonContainer.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 20;
            layout.childAlignment = TextAnchor.MiddleCenter;
            
            RectTransform buttonRect = buttonContainer.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.1f, 0.1f);
            buttonRect.anchorMax = new Vector2(0.9f, 0.4f);
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;
            
            // 创建确认按钮
            CreateButton("确认", buttonContainer.transform, () => {
                onConfirm?.Invoke();
                CloseConfirmation();
            });
            
            // 创建取消按钮（如果需要）
            if (onCancel != null)
            {
                CreateButton("取消", buttonContainer.transform, () => {
                    onCancel?.Invoke();
                    CloseConfirmation();
                });
            }
        }
        
        private void CreateButton(string text, Transform parent, System.Action onClick)
        {
            GameObject buttonObj = new GameObject(text + "Button");
            buttonObj.transform.SetParent(parent, false);
            
            Image buttonBg = buttonObj.AddComponent<Image>();
            buttonBg.color = new Color(0.8f, 0.8f, 0.8f);
            
            Button button = buttonObj.AddComponent<Button>();
            button.onClick.AddListener(() => onClick?.Invoke());
            
            GameObject buttonTextObj = new GameObject("Text");
            buttonTextObj.transform.SetParent(buttonObj.transform, false);
            
            TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = text;
            buttonText.color = Color.black;
            buttonText.alignment = TextAlignmentOptions.Center;
            
            RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.offsetMin = Vector2.zero;
            buttonTextRect.offsetMax = Vector2.zero;
            
            LayoutElement layoutElement = buttonObj.AddComponent<LayoutElement>();
            layoutElement.minWidth = 80;
            layoutElement.minHeight = 40;
        }
        
        private void CreateConfirmationFromPrefab(string message, System.Action onConfirm, System.Action onCancel)
        {
            // 使用预制件创建确认框的逻辑
            // 如果需要更复杂的UI可以实现这个方法
            CreateSimpleConfirmationDialog(message, onConfirm, onCancel);
        }
        
        private void CloseConfirmation()
        {
            if (confirmationDialog != null)
            {
                Destroy(confirmationDialog);
                confirmationDialog = null;
            }
            onConfirmCallback = null;
            onCancelCallback = null;
        }
        
        /// <summary>
        /// 显示简单消息（替代复杂的通知系统）
        /// </summary>
        public void ShowMessage(string message)
        {
            // 可以在这里添加简单的UI显示逻辑
        }
    }
} 