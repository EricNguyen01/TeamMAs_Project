using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHighlight : MonoBehaviour, IPointerExitHandler, IPointerDownHandler
{
    [SerializeField] private Button buttonToHighlight;

    [SerializeField]
    [Tooltip("If FALSE (Default): Use the HighlightedColor built-in field in the provided Button component.\n" +
    "If TRUE: Use a new custom color set below.")]
    private bool useCustomButtonHighlightColor = false;

    [SerializeField] private Color customHighlightColor = Color.grey;

    private void Awake()
    {
        if (!buttonToHighlight)
        {
            TryGetComponent<Button>(out buttonToHighlight);
        }

        if (!buttonToHighlight)
        {
            enabled = false;

            return;
        }

        if (!EventSystem.current)
        {
            enabled = false;

            return;
        }

        if (buttonToHighlight && useCustomButtonHighlightColor)
        {
            customHighlightColor.a = 255.0f;

            ColorBlock colorBlock = buttonToHighlight.colors;

            colorBlock.highlightedColor = customHighlightColor;

            buttonToHighlight.colors = colorBlock;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!enabled) return;

        if(EventSystem.current.currentSelectedGameObject == buttonToHighlight.gameObject)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!enabled) return;

        if (EventSystem.current.currentSelectedGameObject == buttonToHighlight.gameObject)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}