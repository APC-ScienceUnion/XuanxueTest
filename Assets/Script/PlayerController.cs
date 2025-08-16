using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XuanZhiShiLian
{
    public class PlayerController : MonoBehaviour
    {
        [Header("移动设置")]
        public float moveSpeed = 5f;
        public bool canMove = true;
        
        [Header("UI引用")]
        public TMPro.TextMeshProUGUI interactionPromptText; // 简单的交互提示文本
        
        // 私有变量
        private Rigidbody2D rb2D;
        private Vector2 movement;
        private Interactable currentInteractable;
        
        private void Start()
        {
            rb2D = GetComponent<Rigidbody2D>();
            
            // 初始化交互提示为隐藏状态
            if (interactionPromptText != null)
            {
                interactionPromptText.gameObject.SetActive(false);
            }
        }
        
        private void Update()
        {
            if (canMove)
            {
                HandleInput();
                
                // 处理交互输入
                if (Input.GetKeyDown(KeyCode.E))
                {
                    // 检查对话是否被锁定，如果被锁定则不允许新的交互
                    if (!DialogueSystem.IsDialogueLocked)
                    {
                        Debug.Log($"[PlayerController] 按下E，canMove={canMove}, 锁定=false, 交互体={(currentInteractable!=null?currentInteractable.name:"null")}");
                        Interact();
                    }
                    else
                    {
                        Debug.LogWarning("[PlayerController] 按下E，但对话处于锁定状态，交互被拦截");
                    }
                }
            }
        }
        
        private void FixedUpdate()
        {
            if (canMove && rb2D != null)
            {
                // 使用Rigidbody2D移动
                rb2D.velocity = movement * moveSpeed;
            }
        }
        
        private void HandleInput()
        {
            // 只使用WASD键，不使用箭头键
            float horizontalInput = 0f;
            float verticalInput = 0f;
            
            if (Input.GetKey(KeyCode.A))
                horizontalInput = -1f;
            else if (Input.GetKey(KeyCode.D))
                horizontalInput = 1f;
                
            if (Input.GetKey(KeyCode.S))
                verticalInput = -1f;
            else if (Input.GetKey(KeyCode.W))
                verticalInput = 1f;
            
            movement = new Vector2(horizontalInput, verticalInput).normalized;
        }
        
        // 新的方法：设置当前交互对象（供Trigger系统使用）
        public void SetCurrentInteractable(Interactable interactable)
        {
            if (currentInteractable != interactable)
            {
                if (currentInteractable != null)
                {
                    currentInteractable.OnPlayerExit();
                }
                
                currentInteractable = interactable;
                
                if (currentInteractable != null)
                {
                    currentInteractable.OnPlayerEnter();
                }
                
                UpdateInteractionPrompt();
            }
        }
        
        private void UpdateInteractionPrompt()
        {
            // 使用简单的UI Text显示交互提示
            if (interactionPromptText != null)
            {
                if (currentInteractable != null)
                {
                    interactionPromptText.gameObject.SetActive(true);
                    interactionPromptText.text = currentInteractable.interactionText;
                }
                else
                {
                    interactionPromptText.gameObject.SetActive(false);
                }
            }
        }
        
        private void Interact()
        {
            if (currentInteractable != null)
            {
                Debug.Log($"[PlayerController] 调用交互: {currentInteractable.GetType().Name} - {currentInteractable.name}");
                currentInteractable.Interact();
            }
            else
            {
                Debug.Log("[PlayerController] 调用交互时没有当前交互体");
            }
        }
        
        public void SetCanMove(bool canMove)
        {
            this.canMove = canMove;
            if (!canMove)
            {
                // 确保rb2D已经初始化
                if (rb2D == null)
                    rb2D = GetComponent<Rigidbody2D>();
                    
                if (rb2D != null)
                {
                    rb2D.velocity = Vector2.zero;
                }
                movement = Vector2.zero;
            }
        }
        
        public void TeleportTo(Vector3 position)
        {
            transform.position = position;
            
            // 确保rb2D已经初始化
            if (rb2D == null)
                rb2D = GetComponent<Rigidbody2D>();
                
            if (rb2D != null)
            {
                rb2D.velocity = Vector2.zero;
            }
            movement = Vector2.zero;
            
            // 传送后刷新交互检测，防止之前的交互对象仍然激活
            RefreshInteractionDetection();
        }
        
        // 获取当前交互对象（供其他系统查询）
        public Interactable GetCurrentInteractable()
        {
            return currentInteractable;
        }
        
        // 强制刷新交互检测（用于场景切换后）
        public void RefreshInteractionDetection()
        {
            currentInteractable = null;
        }
        
        // 设置交互提示文本引用（供Stage控制器调用）
        public void SetInteractionPromptText(TMPro.TextMeshProUGUI promptText)
        {
            interactionPromptText = promptText;
            if (interactionPromptText != null)
            {
                interactionPromptText.gameObject.SetActive(false);
            }
        }
    }
} 