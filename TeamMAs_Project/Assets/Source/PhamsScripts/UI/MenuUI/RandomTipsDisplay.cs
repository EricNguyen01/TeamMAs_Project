using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RandomTipsDisplay : MonoBehaviour
{
    [SerializeField]
    [Min(1.0f)]
    [Tooltip("The time in seconds until the next tip in the tips list is displayed. Default is 2s and cannot goes below 1.0f")]
    private float timeTillNextTip = 2.0f;

    [SerializeField]
    [Tooltip("Does the tip starts with the word \"Tips\" in the beginning?")]
    private bool hasTheWordTipsInFront = true;

    [SerializeField]
    private TextMeshProUGUI tipTextMeshProComponent;

    [SerializeField] private bool displayingRandomTipCycleOnEnable = true;

    [SerializeField]
    [TextArea]
    private List<string> tipsList = new List<string>();

    private string previousTip = string.Empty;

    private string currentTip = string.Empty;

    private bool isDisplayingTipCycle = false;

    private void Awake()
    {
        if (!tipTextMeshProComponent)
        {
            TryGetComponent<TextMeshProUGUI>(out tipTextMeshProComponent);
        }

        if (!tipTextMeshProComponent)
        {
            tipTextMeshProComponent = gameObject.AddComponent<TextMeshProUGUI>();

            tipTextMeshProComponent.raycastTarget = false;
        }

        tipTextMeshProComponent.text = string.Empty;

        if (tipsList == null || tipsList.Count == 0) enabled = false;

        for(int i = 0; i < tipsList.Count; i++)
        {
            if (tipsList[i] == string.Empty ||
                string.IsNullOrEmpty(tipsList[i]) ||
                string.IsNullOrWhiteSpace(tipsList[i]) ||
                tipsList[i] == "" || tipsList[i] == null)
            {
                tipsList.RemoveAt(i);

                if (i > 0) i--;
            }
        }

        if (tipsList == null || tipsList.Count == 0) enabled = false;
    }

    private void OnEnable()
    {
        if (!enabled) return;

        if (displayingRandomTipCycleOnEnable) DisplayRandomTipCycleWithDelay();
    }

    private void OnDisable()
    {
        StopDisplayingTips();
    }

    private string ChooseRandomTip()
    {
        if (!enabled) return string.Empty;

        if (tipsList == null || tipsList.Count == 0) return string.Empty;

        if(tipsList.Count == 1)
        {
            currentTip = tipsList[0];

            previousTip = currentTip;

            return currentTip;
        }

        if(previousTip == string.Empty ||
           string.IsNullOrEmpty(previousTip) ||
           string.IsNullOrWhiteSpace(previousTip) ||
           previousTip == "" || previousTip == null)
        {
            currentTip = tipsList[Random.Range(0, tipsList.Count)];

            previousTip = currentTip;

            tipsList.Remove(previousTip);

            return currentTip;
        }

        currentTip = tipsList[Random.Range(0, tipsList.Count)];

        tipsList.Remove(currentTip);

        tipsList.Add(previousTip);

        previousTip = currentTip;

        return currentTip;
    }

    private void DisplayChosenTip(string choseTip)
    {
        if (!enabled) return;

        if (tipsList == null || tipsList.Count == 0) return;

        string tipToDisplay = "";

        if (hasTheWordTipsInFront)
        {
            choseTip.Substring(0, 1).ToLower();

            tipToDisplay = "Tips: " + choseTip;
        }
        else
        {
            choseTip.Substring(0, 1).ToUpper();

            tipToDisplay = choseTip;
        }

        tipTextMeshProComponent.text = tipToDisplay;
    }

    private IEnumerator DisplayTipsDelay(float delay)
    {
        if (!enabled)
        {
            yield return null; yield break;
        }

        while (isDisplayingTipCycle)
        {
            DisplayChosenTip(ChooseRandomTip());

            yield return new WaitForSeconds(timeTillNextTip);
        }

        yield break;
    }

    public void StopDisplayingTips()
    {
        StopCoroutine(DisplayTipsDelay(timeTillNextTip));

        isDisplayingTipCycle = false;

        tipTextMeshProComponent.enabled = false;
    }

    public void DisplayRandomTipCycleWithDelay()
    {
        if (!enabled) return;

        tipTextMeshProComponent.enabled = true;

        isDisplayingTipCycle = true;

        StartCoroutine(DisplayTipsDelay(timeTillNextTip));
    }

    public void DisplayRandomTipOnce()
    {
        if (!enabled) return;

        StopDisplayingTips();

        tipTextMeshProComponent.enabled = true;

        DisplayChosenTip(ChooseRandomTip());
    }
}
