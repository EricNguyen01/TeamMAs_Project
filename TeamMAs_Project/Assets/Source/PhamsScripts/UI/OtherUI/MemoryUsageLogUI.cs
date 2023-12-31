using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class MemoryUsageLogUI : MonoBehaviour
    {
        [SerializeField] private KeyCode toggleLogUIKey = KeyCode.F12;

        [Header("UI Components")]

        [SerializeField] private TextMeshProUGUI totalReservedMemoryTextUI;

        [SerializeField] private TextMeshProUGUI gcUsedMemoryTextUI;

        [SerializeField] private TextMeshProUGUI systemUsedMemoryTextUI;

        private CanvasGroup toggleCanvasGroup;
    }
}
