// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public class DragSelectionUI : MonoBehaviour, IPointerEnterHandler
    {
        [SerializeField] private Image dragSelectionBoxImage;

        //INTERNALS....................................................................

        private Canvas dragSelectionCanvas;

        private CanvasGroup dragSelectionCanvasGroup;

        private CanvasScaler dragSelectionCanvasScaler;

        private Vector2 startSelectionMousePos = Vector2.zero;

        private float selectionWidth = 0.0f;

        private float selectionHeight = 0.0f;   

        private bool hasStartedDragging = false;

        private void Awake()
        {
            TryGetComponent<Canvas>(out dragSelectionCanvas);

            if (!dragSelectionCanvas)
            {
                Debug.LogWarning("Drag Selection UI Script Component Is Not Currently Being Attached To A UI Canvas Object. Disabling Script!");

                return;
            }

            if(!TryGetComponent<CanvasScaler>(out dragSelectionCanvasScaler))
            {
                dragSelectionCanvasScaler = gameObject.AddComponent<CanvasScaler>();
            }

            dragSelectionCanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;

            dragSelectionCanvasScaler.scaleFactor = 1f;

            dragSelectionCanvasScaler.referencePixelsPerUnit = 100.0f;

            if(!TryGetComponent<CanvasGroup>(out dragSelectionCanvasGroup))
            {
                dragSelectionCanvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            dragSelectionCanvasGroup.interactable = false;

            dragSelectionCanvasGroup.blocksRaycasts = false;

            if (!dragSelectionBoxImage)
            {
                dragSelectionBoxImage = GetComponentInChildren<Image>();
            }

            if (!dragSelectionBoxImage)
            {
                GameObject boxImgObj = new GameObject("DragSelectionMaskImage");

                boxImgObj.transform.SetParent(transform);

                boxImgObj.transform.localPosition = Vector3.zero;

                boxImgObj.AddComponent<RectTransform>();

                Image img = boxImgObj.AddComponent<Image>();

                dragSelectionBoxImage = img;
            }
        }

        private void OnEnable()
        {
            if (dragSelectionBoxImage)
            {
                dragSelectionBoxImage.rectTransform.anchorMin = Vector3.zero;

                dragSelectionBoxImage.rectTransform.anchorMax = Vector3.zero;

                dragSelectionBoxImage.rectTransform.pivot = new Vector2(0.0f, 1.0f);

                dragSelectionBoxImage.rectTransform.localScale = Vector3.one;

                dragSelectionBoxImage.raycastTarget = false;
            }

            if(dragSelectionCanvasGroup) dragSelectionCanvasGroup.alpha = 0.0f;

            if (EventSystem.current == null)
            {
                enabled = false;

                return;
            }
        }

        private void Update()
        {
            if (!enabled) return;
            
            if (Input.GetButtonDown("Fire1"))
            {
                BeginDrag();
            }
            else if (Input.GetButton("Fire1"))
            {
                OnDrag();
            }
            else if (Input.GetButtonUp("Fire1"))
            {
                EndDrag();
            }
        }

        public void BeginDrag()
        {
            if (!enabled) return;

            hasStartedDragging = true;

            startSelectionMousePos = Input.mousePosition;

            selectionWidth = 0.0f;

            selectionHeight = 0.0f;

            dragSelectionBoxImage.rectTransform.localScale = Vector3.one;

            dragSelectionCanvasGroup.alpha = 1.0f;

            //selection box has size of 0 on begin drag
            dragSelectionBoxImage.rectTransform.sizeDelta = Vector3.zero;

            dragSelectionBoxImage.rectTransform.anchoredPosition = Input.mousePosition;
        }

        public void OnDrag()
        {
            if (!enabled) return;

            if(!hasStartedDragging) return;

            Vector3 localScale = dragSelectionBoxImage.rectTransform.localScale;

            selectionWidth = Input.mousePosition.x - startSelectionMousePos.x;

            //if selection box's width < 0 -> flip X scale to -1
            if(selectionWidth < 0.0f && localScale.x > 0.0f) localScale.x *= -1.0f;

            //if width >= 0 -> flip X scale to 1
            else if(selectionWidth >= 0.0f && localScale.x < 0.0f) localScale *= -1.0f;

            selectionHeight = startSelectionMousePos.y - Input.mousePosition.y;

            //if selection box's height < 0 -> flip Y scale to -1
            if (selectionHeight < 0.0f && localScale.y > 0.0f) localScale.y *= -1.0f;

            //if width >= 0 -> flip Y scale to 1
            else if(selectionHeight >= 0.0f && localScale.y < 0.0f) localScale.y *= -1.0f;

            dragSelectionBoxImage.rectTransform.localScale = new Vector3(localScale.x, localScale.y, localScale.z);

            //adjusts selection box's size during drag
            dragSelectionBoxImage.rectTransform.sizeDelta = new Vector2(Mathf.Abs(selectionWidth), Mathf.Abs(selectionHeight));
        }

        public void EndDrag()
        {
            if (!enabled) return;

            if(!hasStartedDragging) return;

            hasStartedDragging = false;

            dragSelectionCanvasGroup.alpha = 0.0f;

            dragSelectionBoxImage.rectTransform.localScale = Vector3.one;

            //selection box has size of 0 on end drag
            dragSelectionBoxImage.rectTransform.sizeDelta = Vector3.zero;
        }

        //EventSystems UI Interface Implementation............................................................

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!enabled) return;

            if(!hasStartedDragging) return; 

            if (!eventData.pointerEnter) return;

            if(eventData.pointerEnter.layer == LayerMask.NameToLayer("UI"))
            {
                EndDrag();//On pointer enters a UI elements -> end drag selection if already dragging
            }
        }
    }
}
