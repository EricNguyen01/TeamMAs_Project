using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteGlow.SpriteGlowEffect))]
    public class TileGlow : MonoBehaviour
    {
        [Header("Tile Glow Config")]
        [SerializeField] private Color glowColorPositive = Color.green;

        [SerializeField] private Color glowColorNegative = Color.red;

        [SerializeField][Min(0.0f)] private float glowAlphaFrom = 50.0f;

        [SerializeField][Min(0.0f)] private float glowAlphaTo = 100.0f;

        [SerializeField][Min(0.0f)] private float colorBrightnessFrom = 0.15f;

        [SerializeField][Min(0.0f)] private float colorBrightnessTo = 0.2f;

        [SerializeField][Min(0.0f)] private float glowCycleFrequency = 1.3f;

        private SpriteGlow.SpriteGlowEffect spriteGlowEffectComp;

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
        }

        public void OverrideAllTileGlowConfig(Color positiveGlowColor, Color negativeGlowColor,
                                              float alphaFrom, float alphaTo,
                                              float brightnessFrom, float brightnessTo,
                                              float newglowCycleFrequency)
        {
            OverrideTileGlowEffectColor(positiveGlowColor, negativeGlowColor);

            OverrideTileGlowEffectAlphaFromTo(alphaFrom, alphaTo);

            OverrideTileGlowEffectBrightnessFromTo(brightnessFrom, brightnessTo);

            OverrideTileGlowEffectGlowTime(newglowCycleFrequency);
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

        public void OverrideTileGlowEffectGlowTime(float newglowCycleFrequency)
        {
            glowCycleFrequency = newglowCycleFrequency;
        }

        private void EnableTileGlowEffect(bool isPositiveGlow)
        {
            //always disable first before enable
            //in case tile glow is already enabled and we want to reset it for any reason (e.g changing glow properties)
            //in disable function, isTileGlowing is set to false and all coroutines are stopped.
            DisableTileGlowEffect();

            if (isPositiveGlow) spriteGlowEffectComp.GlowColor = glowColorPositive;
            else spriteGlowEffectComp.GlowColor = glowColorNegative;

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

            //Color spriteRendererColor = spriteGlowEffectComp.Renderer.color;

            Color glowColor = spriteGlowEffectComp.GlowColor;

            while (isTileGlowing)
            {
                //glow forward "from -> to"
                if (glowCycleTime == 0.0f)
                {
                    while (glowCycleTime < glowCycleFrequency)
                    {
                        glowCycleTime += Time.fixedDeltaTime;

                        //spriteRendererColor.a = Mathf.Lerp(glowAlphaFrom, glowAlphaTo, glowCycleTime);

                        glowColor.a = Mathf.Lerp(glowAlphaFrom, glowAlphaTo, glowCycleTime);

                        //spriteGlowEffectComp.Renderer.color = spriteRendererColor;

                        spriteGlowEffectComp.GlowColor = glowColor;

                        spriteGlowEffectComp.GlowBrightness = Mathf.Lerp(colorBrightnessFrom, colorBrightnessTo, glowCycleTime);

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

                        //spriteRendererColor.a = Mathf.Lerp(glowAlphaFrom, glowAlphaTo, glowCycleTime);

                        glowColor.a = Mathf.Lerp(glowAlphaFrom, glowAlphaTo, glowCycleTime);

                        //spriteGlowEffectComp.Renderer.color = spriteRendererColor;

                        spriteGlowEffectComp.GlowColor = glowColor;

                        spriteGlowEffectComp.GlowBrightness = Mathf.Lerp(colorBrightnessFrom, colorBrightnessTo, glowCycleTime);

                        yield return new WaitForFixedUpdate();
                    }

                    glowCycleTime = 0.0f;
                }
            }

            glowCycleTime = 0.0f;

            yield break;
        }

        public void EnableTileGlowEffect(bool isPositiveGlow, float enableStartDelay = 0.0f)
        {
            if(enableStartDelay <= 0.0f)
            {
                EnableTileGlowEffect(isPositiveGlow);

                return;
            }

            EnableTileGlowEffectDelay(isPositiveGlow, enableStartDelay);
        }

        public void EnableTileGlowEffect(bool isPositiveGlow, float enableStartDelay, float enableStopDelay)
        {
            if (enableStopDelay <= enableStartDelay) enableStopDelay = enableStartDelay + 0.5f;

            EnableTileGlowEffect(isPositiveGlow, enableStartDelay);

            DisableTileGlowEffect(enableStopDelay);
        }

        private void EnableTileGlowEffectDelay(bool isPositiveGlow, float delay)
        {
            StartCoroutine(EnableTileGlowEffectDelayCoroutine(isPositiveGlow, delay));
        }

        private IEnumerator EnableTileGlowEffectDelayCoroutine(bool isPositiveGlow, float delay)
        {
            yield return new WaitForSeconds(delay);

            EnableTileGlowEffect(isPositiveGlow);

            yield break;
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

            DisableTileGlowEffectDelay(disableStartDelay);
        }

        private void DisableTileGlowEffectDelay(float delay)
        {
            StartCoroutine(DisableTileGlowEffectDelayCoroutine(delay));
        }

        private IEnumerator DisableTileGlowEffectDelayCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);

            DisableTileGlowEffect();

            yield break;
        }
    }
}
