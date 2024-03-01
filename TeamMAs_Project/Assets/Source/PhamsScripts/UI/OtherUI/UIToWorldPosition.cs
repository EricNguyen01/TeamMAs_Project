// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using TeamMAsTD;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class UIToWorldPosition : MonoBehaviour
{
    [SerializeField] private bool controlThisUIObject = true;

    [SerializeField]
    [DisableIf("controlThisUIObject", true)]
    private RectTransform UIToRePosition;

    [SerializeField] private Vector3 worldPositionToMatch;

    [SerializeField] private bool positioningOnStart = false;

    private void OnEnable()
    {
        if (controlThisUIObject)
        {
            TryGetComponent<RectTransform>(out UIToRePosition);
        }
    }

    private void Start()
    {
        if(positioningOnStart) MatchUIRectPosToWorldPos(UIToRePosition, worldPositionToMatch);
    }

    public void MatchUIRectPosToWorldPos(Vector3 worldPos)
    {
        MatchUIRectPosToWorldPos(UIToRePosition, worldPos);
    }

    public void MatchUIRectPosToWorldPos(RectTransform UIObject, Vector3 worldPos)
    {
        if (!UIObject) return;

        UIToRePosition = UIObject;

        worldPositionToMatch = worldPos;

        HelperFunctions.MatchUIRectPosToWorldPos(UIObject, worldPos);
    }
}
