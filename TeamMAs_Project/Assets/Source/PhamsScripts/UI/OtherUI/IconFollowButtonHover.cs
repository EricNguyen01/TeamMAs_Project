// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
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

        private Dictionary<GameObject, Button> gameObjectButtonDict = new Dictionary<GameObject, Button>();

        private void Awake()
        {
            if (!UIImageAsFollowingIcon)
            {
                Debug.LogWarning("UI Image As Following Icon is missing. Disabling Script...");

                enabled = false;

                return;
            }

            if (UIImageAsFollowingIcon) iconRectTransform = UIImageAsFollowingIcon.rectTransform;

            if(gameObjectButtonDict.Count > 0) gameObjectButtonDict.Clear();
        }

        private void OnEnable()
        {
            if (!EventSystem.current)
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
            if (!enabled) return;

            if (EventSystem.current)
            {
                pointerEventData = new PointerEventData(EventSystem.current);
            }

            buttonForOffsetDebug = null;

            SetIconToFirstButtonInListOrFirstFound();
        }

        private void Update()
        {
            if (!enabled) return;

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

        //This function gradually tweens the follow icon to the current valid UI button being hovered on
        private void InterpolateToCurrentHoveredButtonInList()
        {
            if (!EventSystem.current || pointerEventData == null) return;

            pointerEventData.position = Input.mousePosition;

            EventSystem.current.RaycastAll(pointerEventData, pointerRaycastResults);

            //if mouse is not on any UI obj or element -> do nothing and exit func
            if (pointerRaycastResults == null || pointerRaycastResults.Count <= 0) return;

            //if mouse is on 1 or more UI elements -> iterate through the UI objects to check if any of them are buttons
            for (int i = 0; i < pointerRaycastResults.Count; i++)
            {
                Button button;
                
                //this if checks if the currently in-check UI game object is an existing key in the dict
                //if yes, skip the chunk under this if.
                if (!gameObjectButtonDict.TryGetValue(pointerRaycastResults[i].gameObject, out button))
                {
                    //key (UI game object that mouse is on) doesnt exist in dict -> check this game object for a button component

                    //because this UI game object has never been visited before, this is the first and only time get component will be used to check through its comps
                    //after this, this game object will exist in the dict as key along with its button pair value for quick extraction of data (no need to use get component next time).
                    pointerRaycastResults[i].gameObject.TryGetComponent<Button>(out button);

                    //add this just checked game object to dict no matter if it has or has not a button component
                    //this is so that the game object exists in the dict so that we don't have to look through its components to find a button comp (above if is used next time)
                    //(if it doesn't have a button, the next time the pair value is extracted, this loop will continue (see above if logic))
                    gameObjectButtonDict.Add(pointerRaycastResults[i].gameObject, button);
                }

                //after checking the UI obj, if it turns out the UI obj doesn't have a button comp -> continue iteration and skip the below chunk
                if (!button) continue;

                //if a button is found and a list of valid buttons is set, check if the founded button is in the list
                //if yes, it is a valid button and if not, it is invalid and iteration is exit.
                if (buttonsToFollowOnHovered.Count > 0)
                {
                    if (!buttonsToFollowOnHovered.Contains(button)) break;
                }

                //if the founded button is determined to be valid, set it as the current button to move to
                if (!currentButtonToMoveTo || currentButtonToMoveTo != button) currentButtonToMoveTo = button;

                break;//exit iteration once current button to move to is successfully set
            }

            //if above iteration has finished and no valid button is received -> do nothing and exit function
            if (!currentButtonToMoveTo || !currentButtonToMoveTo.image) return;

            //else if a valid button is received and it is a new one (in case the user leaves their mouse on the same valid button and it is checked every frame)
            //if it is a new one -> interpolate the follow icon to that button.
            if(!previousButton || previousButton != currentButtonToMoveTo)
            {
                iconRectTransform.DOAnchorMin(currentButtonToMoveTo.image.rectTransform.anchorMin + iconOffset,
                                              followTweenTime).SetEase(followEaseMode).SetUpdate(true);

                iconRectTransform.DOAnchorMax(currentButtonToMoveTo.image.rectTransform.anchorMax + iconOffset,
                                              followTweenTime).SetEase(followEaseMode).SetUpdate(true);

                previousButton = currentButtonToMoveTo;
            }
        }
    }
}
