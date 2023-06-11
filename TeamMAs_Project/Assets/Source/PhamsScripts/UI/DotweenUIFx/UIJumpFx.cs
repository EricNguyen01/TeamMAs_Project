using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TeamMAsTD
{
    public class UIJumpFx : MonoBehaviour
    {
        [Header("UI Jump FX Settings")]

        [SerializeField] private float jumpHeight = 5.0f;

        [SerializeField] private float jumpPower = 1.0f;

        [SerializeField] private float jumpDuration = 0.5f;

        //INTERNALS...............................................

        private RectTransform rectTransform;

        private Vector2 baseAnchoredPos;

        private bool alreadyJumped = false;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            if(!rectTransform) enabled = false;

            baseAnchoredPos = rectTransform.anchoredPosition;
        }

        public void ProcessUIJump()
        {
            if (!rectTransform || alreadyJumped) return;

            StartCoroutine(JumpCoroutine());
        }

        private IEnumerator JumpCoroutine()
        {
            alreadyJumped = true;

            rectTransform.DOJumpAnchorPos(new Vector2(baseAnchoredPos.x, baseAnchoredPos.y + jumpHeight), jumpPower, 2, jumpDuration);

            yield return new WaitForSeconds(jumpDuration);

            alreadyJumped = false;
        }

    }
}
