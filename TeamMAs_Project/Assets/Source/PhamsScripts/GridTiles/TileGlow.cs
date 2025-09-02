// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using UnityEngine;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteGlow.SpriteGlowEffect))]
    public class TileGlow : MonoBehaviour
    {
        [Header("Tile Glow Config")]

        [SerializeField] private Color glowColorPositive = Color.green;

        private Color glowColorPositiveRuntimeDefault;

        [SerializeField] private Color glowColorNegative = Color.red;

        private Color glowColorNegativeRuntimeDefault;

        [SerializeField][Min(0.0f)] private float glowAlphaFrom = 50.0f;

        private float glowAlphaFromRuntimeDefault = 0.0f;

        [SerializeField][Min(0.0f)] private float glowAlphaTo = 100.0f;

        private float glowAlphaToRuntimeDefault = 0.0f;

        [SerializeField][Min(0.0f)] private float colorBrightnessFrom = 0.15f;

        private float colorBrightnessFromRuntimeDefault = 0.0f;

        [SerializeField][Min(0.0f)] private float colorBrightnessTo = 0.2f;

        private float colorBrightnessToRuntimeDefault = 0.0f;

        [SerializeField][Min(0.0f)] private float glowCycleFrequency = 1.3f;

        private float glowCycleFrequencyRuntimeDefault = 0.0f;

        public enum TileGlowMode { PositiveGlow, NegativeGlow }

        public SpriteGlow.SpriteGlowEffect spriteGlowEffectComp { get; private set; }

        public bool isTileGlowing { get; private set; } = false;

        private float glowCycleTime = 0.0f;

        private void Awake()
        {
            spriteGlowEffectComp = GetComponent<SpriteGlow.SpriteGlowEffect>();

            if(spriteGlowEffectComp == null)
            {
                spriteGlowEffectComp = gameObject.AddComponent<SpriteGlow.SpriteGlowEffect>();
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private void Start()
        {
            if (spriteGlowEffectComp.enabled) spriteGlowEffectComp.enabled = false;

            glowColorPositiveRuntimeDefault = glowColorPositive;

            glowColorNegativeRuntimeDefault = glowColorNegative;

            colorBrightnessFromRuntimeDefault = colorBrightnessFrom;

            colorBrightnessToRuntimeDefault = colorBrightnessTo;

            glowAlphaFromRuntimeDefault = glowAlphaFrom;

            glowAlphaToRuntimeDefault = glowAlphaTo;

            glowCycleFrequencyRuntimeDefault = glowCycleFrequency;
        }

        public void OverrideAllTileGlowConfig(Color positiveGlowColor, Color negativeGlowColor,
                                              float alphaFrom, float alphaTo,
                                              float brightnessFrom, float brightnessTo,
                                              float newGlowCycleFrequency)
        {
            OverrideTileGlowEffectColor(positiveGlowColor, negativeGlowColor);

            OverrideTileGlowEffectAlphaFromTo(alphaFrom, alphaTo);

            OverrideTileGlowEffectBrightnessFromTo(brightnessFrom, brightnessTo);

            OverrideTileGlowEffectGlowTime(newGlowCycleFrequency);
        }

        public void OverrideTileGlowEffectColor(Color positiveGlowColor, Color negativeGlowColor)
        {
            glowColorPositive = positiveGlowColor;
            glowColorNegative = negativeGlowColor;
        }

        public void OverrideTileGlowEffectAlphaFromTo(float alphaFrom, float alphaTo)
        {
            glowAlphaFrom = alphaFrom;
            glowAlphaTo = alphaTo;  
        }

        public void OverrideTileGlowEffectBrightnessFromTo(float brightnessFrom, float brightnessTo)
        {
            colorBrightnessFrom = brightnessFrom;
            colorBrightnessTo = brightnessTo;
        }

        public void OverrideTileGlowEffectGlowTime(float newGlowCycleFrequency)
        {
            glowCycleFrequency = newGlowCycleFrequency;
        }

        public void SetDefaultRuntimeValues()
        {
            OverrideTileGlowEffectColor(glowColorPositiveRuntimeDefault, glowColorNegativeRuntimeDefault);

            OverrideTileGlowEffectAlphaFromTo(glowAlphaFromRuntimeDefault, glowAlphaToRuntimeDefault);

            OverrideTileGlowEffectBrightnessFromTo(colorBrightnessFromRuntimeDefault, colorBrightnessToRuntimeDefault);

            OverrideTileGlowEffectGlowTime(glowCycleFrequencyRuntimeDefault);
        }

        private void EnableTileGlowEffect(TileGlowMode tileGlowMode)
        {
            //always disable first before enable
            //in case tile glow is already enabled and we want to reset it for any reason (e.g changing glow properties)
            //in disable function, isTileGlowing is set to false and all coroutines are stopped.
            DisableTileGlowEffect();

            if (tileGlowMode == TileGlowMode.PositiveGlow)
            {
                spriteGlowEffectComp.GlowColor = glowColorPositive;
            }
            else 
            { 
                spriteGlowEffectComp.GlowColor = glowColorNegative;
            }

            spriteGlowEffectComp.enabled = true;

            isTileGlowing = true;
            
            StartCoroutine(TileGlowProcessCoroutine());
        }

        private IEnumerator TileGlowProcessCoroutine()
        {
            //in case tile glow is already disabled -> stop this coroutine and all its recursions
            if (!isTileGlowing || !spriteGlowEffectComp.enabled) yield break;

            //if there's no proper glow cycle frequency provided -> dont execute coroutine and disable
            if(glowCycleFrequency <= 0.0f)
            {
                DisableTileGlowEffect();

                yield break;
            }

            Color glowColor = spriteGlowEffectComp.GlowColor;

            float normalizedTime = 0.0f;

            while (isTileGlowing)
            {
                //glow forward "from -> to"
                if (glowCycleTime == 0.0f)
                {
                    while (glowCycleTime < glowCycleFrequency)
                    {
                        glowCycleTime += Time.fixedDeltaTime;

                        normalizedTime = glowCycleTime / glowCycleFrequency;

                        //Debug.Log("NormalizedTime: " + normalizedTime);

                        glowColor.a = Mathf.Lerp(glowAlphaFrom, glowAlphaTo, normalizedTime);

                        spriteGlowEffectComp.GlowColor = glowColor;

                        spriteGlowEffectComp.GlowBrightness = Mathf.Lerp(colorBrightnessFrom, colorBrightnessTo, normalizedTime);

                        yield return new WaitForFixedUpdate();
                    }

                    glowCycleTime = glowCycleFrequency;
                }
                //glow reversed "to -> from"
                else if (glowCycleTime == glowCycleFrequency)
                {
                    while (glowCycleTime > 0.0f)
                    {
                        glowCycleTime -= Time.fixedDeltaTime;

                        normalizedTime = glowCycleTime / glowCycleFrequency;

                        glowColor.a = Mathf.Lerp(glowAlphaFrom, glowAlphaTo, normalizedTime);

                        spriteGlowEffectComp.GlowColor = glowColor;

                        spriteGlowEffectComp.GlowBrightness = Mathf.Lerp(colorBrightnessFrom, colorBrightnessTo, normalizedTime);

                        yield return new WaitForFixedUpdate();
                    }

                    glowCycleTime = 0.0f;
                }
            }

            glowCycleTime = 0.0f;

            yield break;
        }

        public void EnableTileGlowEffect(TileGlowMode tileGlowMode, float enableStartDelay = 0.0f)
        {
            if(enableStartDelay <= 0.0f)
            {
                EnableTileGlowEffect(tileGlowMode);

                return;
            }

            StartCoroutine(EnableTileGlowEffectDelayCoroutine(tileGlowMode, enableStartDelay));
        }

        public void EnableTileGlowEffect(TileGlowMode tileGlowMode, float startDelay, float stopDelay)
        {
            if (stopDelay <= startDelay) stopDelay = startDelay + 0.5f;

            StartCoroutine(TileGlowEffectStartStopWithDelay(tileGlowMode, startDelay, stopDelay));
        }

        private void DisableTileGlowEffect()
        {
            isTileGlowing = false;

            glowCycleTime = 0.0f;

            spriteGlowEffectComp.enabled = false;

            //after disable tile glow effect
            StopAllCoroutines();
        }

        public void DisableTileGlowEffect(float disableStartDelay = 0.0f)
        {
            if(disableStartDelay <= 0.0f)
            {
                DisableTileGlowEffect();

                return;
            }

            StartCoroutine(DisableTileGlowEffectDelayCoroutine(disableStartDelay));
        }

        private IEnumerator EnableTileGlowEffectDelayCoroutine(TileGlowMode tileGlowMode, float delay)
        {
            if (delay <= 0.0f)
            {
                EnableTileGlowEffect(tileGlowMode);

                yield break;
            }

            yield return new WaitForSeconds(delay);

            EnableTileGlowEffect(tileGlowMode);

            yield break;
        }

        private IEnumerator DisableTileGlowEffectDelayCoroutine(float delay)
        {
            if (delay <= 0.0f)
            {
                DisableTileGlowEffect();

                yield break;
            }

            yield return new WaitForSeconds(delay);

            DisableTileGlowEffect();

            yield break;
        }

        private IEnumerator TileGlowEffectStartStopWithDelay(TileGlowMode tileGlowMode, float startDelay, float stopDelay)
        {
            yield return StartCoroutine(EnableTileGlowEffectDelayCoroutine(tileGlowMode, startDelay));

            yield return StartCoroutine(DisableTileGlowEffectDelayCoroutine(stopDelay));

            yield break;
        }
    }
}
