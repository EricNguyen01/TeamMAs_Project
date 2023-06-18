using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

namespace TeamMAsTD
{
    public class UIRotate : UITweenBase
    {
        [Header("UI Rotate General Settings")]

        [SerializeField] private float rotateDegrees;

        public override void RunTweenInternal()
        {
            
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {

        }

        public override void OnPointerExit(PointerEventData eventData)
        {

        }
    }
}
