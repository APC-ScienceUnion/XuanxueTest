using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace XuanZhiShiLian
{
    public class InteractionPromptUI : MonoBehaviour
    {
        [Header("UI组件")]
        public GameObject promptPanel;
        public TextMeshProUGUI promptText;
        public Image promptBackground;
        
        [Header("显示设置")]
        public float fadeSpeed = 2f;
        public Vector3 worldOffset = Vector3.up * 1.5f;
        
        private Camera mainCamera;
        private CanvasGroup canvasGroup;
        private Transform targetTransform;
        private bool isVisible = false;
        
        private void Awake()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
                mainCamera = FindObjectOfType<Camera>();
                
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
                
            // 初始化为隐藏状态
            canvasGroup.alpha = 0f;
            if (promptPanel != null)
                promptPanel.SetActive(false);
        }
        
        public void ShowPrompt(string text, Transform target)
        {
            if (promptText != null)
                promptText.text = text;
                
            targetTransform = target;
            isVisible = true;
            
            if (promptPanel != null)
                promptPanel.SetActive(true);
        }
        
        public void HidePrompt()
        {
            isVisible = false;
            targetTransform = null;
        }
        
        private void Update()
        {
            // 更新透明度
            float targetAlpha = isVisible ? 1f : 0f;
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
            
            // 如果完全隐藏，禁用面板
            if (canvasGroup.alpha <= 0f && promptPanel != null)
            {
                promptPanel.SetActive(false);
            }
            
            // 更新位置跟随目标
            if (isVisible && targetTransform != null && mainCamera != null)
            {
                Vector3 worldPosition = targetTransform.position + worldOffset;
                Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
                
                // 确保在屏幕范围内
                if (screenPosition.z > 0)
                {
                    transform.position = screenPosition;
                }
            }
        }
    }
} 