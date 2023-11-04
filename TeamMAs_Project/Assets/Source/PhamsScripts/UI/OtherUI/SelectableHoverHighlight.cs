// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class SelectableHoverHighlight : MonoBehaviour, IPointerExitHandler, IPointerDownHandler
    {
        [SerializeField] private Selectable selectableToHighlight;

        [SerializeField]
        [Tooltip("If FALSE (Default): Use the HighlightedColor built-in field in the provided Button component.\n" +
        "If TRUE: Use a new custom color set below.")]
        private bool useCustomButtonHighlightColor = false;

        [SerializeField] private Color customHighlightColor = Color.grey;

        private void Awake()
        {
            if (!selectableToHighlight)
            {
                TryGetComponent<Selectable>(out selectableToHighlight);
            }

            if (!selectableToHighlight)
            {
                enabled = false;

                return;
            }

            if (!EventSystem.current)
            {
                enabled = false;

                return;
            }

            if (selectableToHighlight && useCustomButtonHighlightColor)
            {
                customHighlightColor.a = 255.0f;

                ColorBlock colorBlock = selectableToHighlight.colors;

                colorBlock.highlightedColor = customHighlightColor;

                selectableToHighlight.colors = colorBlock;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!enabled) return;

            if (EventSystem.current.currentSelectedGameObject == selectableToHighlight.gameObject)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!enabled) return;

            if (EventSystem.current.currentSelectedGameObject == selectableToHighlight.gameObject)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }
}
