using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DisplaySliderPercentageAsText : MonoBehaviour
{
    [SerializeField] private Slider sliderToDisplayPercentage;

    [SerializeField] private TextMeshProUGUI sliderPercentageText;

    [SerializeField] private bool keepPresetText = true;

    [SerializeField] private bool hasSpaceBeforePercentage = true;

    [SerializeField] private bool addPercentageSymbolSuffix = false;

    [SerializeField] private bool disablePercentageTextDisplay = false;

    //presetText is any other texts that go before the percentage text in the slider text mesh pro text component
    private string presetText;

    //percentageText (the portion after presetText in the slider text)
    //includes a space after presetText and before percentage text (if true) +
    //the actual percentage as text +
    //the "%" suffix (if true)
    private string percentageText;

    private bool previouslyKeptPresetTextOn;
    private void OnEnable()
    {
        CheckAndGetRequiredComponents();

        if (sliderToDisplayPercentage)
        {
            if(sliderToDisplayPercentage.onValueChanged.GetPersistentEventCount() == 0)
            {
                sliderToDisplayPercentage.onValueChanged.AddListener((sliderVal) => UpdateSliderPercentageAsText(sliderVal));

                return;
            }

            for(int i = 0; i < sliderToDisplayPercentage.onValueChanged.GetPersistentEventCount(); i++)
            {
                if (sliderToDisplayPercentage.onValueChanged.GetPersistentMethodName(i) == "UpdateSliderPercentageAsText") return;
            }

            sliderToDisplayPercentage.onValueChanged.AddListener((sliderVal) => UpdateSliderPercentageAsText(sliderVal));
        }

        if (disablePercentageTextDisplay)
        {
            DisablePercentageTextDisplay();

            return;
        }

        if (sliderToDisplayPercentage && sliderPercentageText) presetText = sliderPercentageText.text;

        if (keepPresetText) previouslyKeptPresetTextOn = true;
        else previouslyKeptPresetTextOn = false;

        UpdateSliderPercentageAsText(sliderToDisplayPercentage ? sliderToDisplayPercentage.value : 0.0f);
    }

    private void OnDisable()
    {
        if (sliderToDisplayPercentage)
        {
            sliderToDisplayPercentage.onValueChanged.RemoveListener((sliderVal) => UpdateSliderPercentageAsText(sliderVal));
        }
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        if (!Application.isPlaying) return;

        if (disablePercentageTextDisplay)
        {
            DisablePercentageTextDisplay();

            return;
        }

        UpdateSliderPercentageAsText(sliderToDisplayPercentage? sliderToDisplayPercentage.value : 0.0f);
    }

#endif
    private void CheckAndGetRequiredComponents()
    {
        if (!sliderToDisplayPercentage)
        {
            TryGetComponent<Slider>(out sliderToDisplayPercentage);
        }

        if (!sliderToDisplayPercentage)
        {
            enabled = false;

            return;
        }

        if (sliderToDisplayPercentage && !sliderPercentageText)
        {
            sliderPercentageText = GetComponentInChildren<TextMeshProUGUI>(true);
        }

        if (!sliderPercentageText)
        {
            enabled = false;

            return;
        }
    }

    //If this method name is changed, unity event registrations in OnEnable and OnDisable need to be updated with the new method name
    private void UpdateSliderPercentageAsText(float currentSliderVal)
    {
        if (!enabled || !sliderToDisplayPercentage || disablePercentageTextDisplay) return;
        
        //remove the old percentage text portion (including space and suffix) of the whole slider text (e.g: "abc: 90%" will now be "abc:")
        //before updating and re-adding this text portion with the new slider value below.
        RemoveOnlyPercentageTextPortionFromSliderText();

        //current slider percentage to text
        string updatedPercentageToText = Mathf.RoundToInt((currentSliderVal / sliderToDisplayPercentage.maxValue) * 100.0f).ToString();

        //if do not keep any preset text before percentage text:
        //set slider text to display only slider value percentage as text
        if (!keepPresetText)
        {
            if (previouslyKeptPresetTextOn)
            {
                presetText = sliderPercentageText.text;

                previouslyKeptPresetTextOn = false;
            }

            percentageText = updatedPercentageToText + (addPercentageSymbolSuffix ? "%" : string.Empty);

            sliderPercentageText.text = percentageText;

            return;
        }

        //else, add both preset text and percentage text together as the final slider text

        if (!previouslyKeptPresetTextOn)
        {
            //if previously preset text was disabled which means that only percentageText was displayed
            //yet after removing percentageText portion, slider text length is > 0 (should be equal to 0)
            //this means that the whole slider text including percentageText has been overriden externally with some other texts.
            //In this case, set new presetText string value.
            if(sliderPercentageText.text.Length > 0) presetText = sliderPercentageText.text;

            previouslyKeptPresetTextOn = true;
        }

        percentageText = (hasSpaceBeforePercentage?" ":string.Empty) +
                         updatedPercentageToText + 
                         (addPercentageSymbolSuffix ? "%" : string.Empty);

        sliderPercentageText.text = presetText + percentageText;
    }

    private void RemoveOnlyPercentageTextPortionFromSliderText()
    {
        if (!enabled || !sliderToDisplayPercentage || !sliderPercentageText) return;

        if (percentageText == null ||
            string.IsNullOrEmpty(percentageText) ||
            string.IsNullOrWhiteSpace(percentageText) ||
            percentageText.Length == 0) return;

        //if any cond in this if occurs, the text in textMeshPro comp has been overriden externally and no longer displaying percentage text
        //if this is the case, no need to remove percentage text portion, set new presetText, and exit function.
        if (sliderPercentageText.text == null ||
           string.IsNullOrEmpty(sliderPercentageText.text) ||
           string.IsNullOrWhiteSpace(sliderPercentageText.text) ||
           sliderPercentageText.text.Length == 0 ||
           sliderPercentageText.text.Length < percentageText.Length ||
           !sliderPercentageText.text.Contains(percentageText))
        {
            presetText = sliderPercentageText.text;

            return;
        }

        //remove the percentageText portion (including space and suffix) from the whole slider text (e.g: "abc: 90%" will now be "abc:")
        sliderPercentageText.text = sliderPercentageText.text.Remove(sliderPercentageText.text.Length - percentageText.Length);
    }

    private void DisablePercentageTextDisplay()
    {
        if (!enabled || !sliderToDisplayPercentage) return;

        RemoveOnlyPercentageTextPortionFromSliderText();

        percentageText = string.Empty;
    }
}
