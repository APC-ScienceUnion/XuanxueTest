using UnityEngine;

namespace XuanZhiShiLian
{
    // 可交互对象的基类
    public abstract class Interactable : MonoBehaviour
    {
        [Header("交互设置")]
        public string interactionText = "按E交互";
        public bool isInteractable = true;
        public bool highlightOnHover = true;
        
        [Header("高亮设置")]
        public Color highlightColor = Color.yellow;
        public float highlightIntensity = 1.5f;
        
        protected SpriteRenderer spriteRenderer;
        protected Color originalColor;
        protected bool isHighlighted = false;
        
        protected virtual void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
        }
        
        public virtual bool CanInteract()
        {
            return isInteractable;
        }
        
        public virtual void OnPlayerEnter()
        {
            if (highlightOnHover && !isHighlighted)
            {
                SetHighlight(true);
            }
        }
        
        public virtual void OnPlayerExit()
        {
            if (isHighlighted)
            {
                SetHighlight(false);
            }
        }
        
        protected virtual void SetHighlight(bool highlight)
        {
            if (spriteRenderer != null)
            {
                isHighlighted = highlight;
                spriteRenderer.color = highlight ? highlightColor * highlightIntensity : originalColor;
            }
        }
        
        public abstract void Interact();
    }
} 