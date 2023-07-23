using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    public class IconFollowButtonHover : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("This UI object with an Image component will be moved to follow the button's anchored pos that is being hovered on.")]
        private Image UIImageAsFollowingIcon;

        [SerializeField]
        [Tooltip("Icon will smoothly interpolate to the anchored position of the button being hovered on. " +
        "Default icon start anchored position will always be the first button in list if any. " +
        "If no button is set in list, default icon start will be the first button found position " +
        "and if no button is found, the icon will stay where it is placed in the scene.")]
        private List<Button> buttonsToFollowOnHovered = new List<Button>();

        [SerializeField]
        [Tooltip("The icon will interpolate to the hovered button's center pos (anchored pos) plus this offset value if set.")]
        private Vector2 iconOffset = Vector2.zero;

        [SerializeField]
        private Ease followEaseMode = Ease.InOutElastic;

        [SerializeField]
        private float followTweenTime = 0.4f;

        [SerializeField]
        [Tooltip("Set a button to this field to see where the UI Icon will position itself to this button during runtime. " +
        "Remove debug button after use to return the icon to its default pos (1st button in list).")]
        private Button buttonForOffsetDebug;

        //INTERNALS.....................................................................................

        private Button currentButtonToMoveTo;

        private Button previousButton;

        private RectTransform iconRectTransform;

        private PointerEventData pointerEventData;

        private List<RaycastResult> pointerRaycastResults = new List<RaycastResult>();

        private Vector2 currentVel;

        private void Awake()
        {
            if (!Application.isPlaying) return;

            if (!UIImageAsFollowingIcon)
            {
                Debug.LogWarning("UI Image As Following Icon is missing. Disabling Script...");

                enabled = false;

                return;
            }

            if (UIImageAsFollowingIcon) iconRectTransform = UIImageAsFollowingIcon.rectTransform;
        }

        private void OnEnable()
        {
            if (!FindObjectOfType<EventSystem>())
            {
                Debug.LogWarning("UI Icon Follow Hovered Buttons Could Not Find An Event System. Disabling Script...");

                enabled = false;

                return;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying) return;

            if (!UIImageAsFollowingIcon)
            {
                UIImageAsFollowingIcon = GetComponent<Image>();
            }

            if (!UIImageAsFollowingIcon)
            {
                UIImageAsFollowingIcon = GetComponentInChildren<Image>();
            }

            if (UIImageAsFollowingIcon) iconRectTransform = UIImageAsFollowingIcon.rectTransform;

            SetIconToFirstButtonInListOrFirstFound();

            SetIconToDebugButton();
        }
#endif

        private void Start()
        {
            if (!Application.isPlaying || !enabled) return;

            if (EventSystem.current)
            {
                pointerEventData = new PointerEventData(EventSystem.current);
            }

            buttonForOffsetDebug = null;

            SetIconToFirstButtonInListOrFirstFound();
        }

        private void Update()
        {
            if (!Application.isPlaying || !enabled) return;

            InterpolateToCurrentHoveredButtonInList();
        }

        private void SetIconToFirstButtonInListOrFirstFound()
        {
            if (buttonForOffsetDebug) return;

            if (buttonsToFollowOnHovered == null || buttonsToFollowOnHovered.Count <= 0)
            {
                foreach(Button button in FindObjectsOfType<Button>())
                {
                    if (button)
                    {
                        SnapIconToButtonCenterWithOffset(button);

                        currentButtonToMoveTo = button;

                        break;
                    }
                }

                return;
            }

            SnapIconToButtonCenterWithOffset(buttonsToFollowOnHovered[0]);

            currentButtonToMoveTo = buttonsToFollowOnHovered[0];
        }

        private void SetIconToDebugButton()
        {
            if (!UIImageAsFollowingIcon || !buttonForOffsetDebug) return;

            SnapIconToButtonCenterWithOffset(buttonForOffsetDebug);
        }

        private void SnapIconToButtonCenterWithOffset(Button button)
        {
            if (!button || !UIImageAsFollowingIcon) return;

            UIImageAsFollowingIcon.rectTransform.anchorMin = button.image.rectTransform.anchorMin + iconOffset;

            UIImageAsFollowingIcon.rectTransform.anchorMax = button.image.rectTransform.anchorMax + iconOffset;
        }

        private void InterpolateToCurrentHoveredButtonInList()
        {
            if (!EventSystem.current || pointerEventData == null) return;

            pointerEventData.position = Input.mousePosition;

            EventSystem.current.RaycastAll(pointerEventData, pointerRaycastResults);

            if(pointerRaycastResults != null && pointerRaycastResults.Count > 0)
            {
                for (int i = 0; i < pointerRaycastResults.Count; i++)
                {
                    Button button = pointerRaycastResults[i].gameObject.GetComponent<Button>();

                    if (!button) continue;

                    if(buttonsToFollowOnHovered.Count > 0)
                    {
                        if (!buttonsToFollowOnHovered.Contains(button)) break;
                    }

                    if (!currentButtonToMoveTo || currentButtonToMoveTo != button) currentButtonToMoveTo = button;

                    break;
                }
            }

            if (!currentButtonToMoveTo || !currentButtonToMoveTo.image) return;

            if(!previousButton || previousButton != currentButtonToMoveTo)
            {
                iconRectTransform.DOAnchorMin(currentButtonToMoveTo.image.rectTransform.anchorMin + iconOffset,
                                              followTweenTime).SetEase(followEaseMode).SetUpdate(true);

                iconRectTransform.DOAnchorMax(currentButtonToMoveTo.image.rectTransform.anchorMax + iconOffset,
                                              followTweenTime).SetEase(followEaseMode).SetUpdate(true);

                previousButton = currentButtonToMoveTo;
            }

            //iconRectTransform.localPosition = currentButtonToMoveTo.image.rectTransform.localPosition + new Vector3(iconOffset.x, iconOffset.y, 0.0f);
        }
    }
}
