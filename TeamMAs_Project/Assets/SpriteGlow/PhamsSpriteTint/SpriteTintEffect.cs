using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    public class SpriteTintEffect : MonoBehaviour
    {
        public Color spriteTintColor { get; private set; } = Color.clear;

        private Color defaultSpriteTintColor = Color.clear;

        public float spriteTintAlpha { get; private set; } = 0.0f;

        private float defaultSpriteTintAlpha = 0.0f;

        [field: SerializeField] public bool disableTintEffect { get; set; } = false;

        public Shader spriteTintShader { get; private set; }

        private Material spriteTintMat;

        private SpriteRenderer spriteRenderer;

        private Sprite mainSpriteTexture;

        private void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            if(spriteRenderer == null)
            {
                Debug.LogError("Could not find a sprite renderer component on: " + name + " " +
                    "that has SpriteTintEffect script component attached! Disabling script...!");

                enabled = false;

                return;
            }

            mainSpriteTexture = spriteRenderer.sprite;

            spriteTintShader = Shader.Find("Shader Graphs/SpriteTintSG");

            if(spriteTintShader == null)
            {
                Debug.LogError("Could not find Shader Graphs/SpriteTintSG shader for SpriteTintEffect script component! Disabling  script...!");

                enabled = false;

                return;
            }

            AssignSpriteTintShaderMatIfNotAlready();

            if(spriteTintMat == null)
            {
                Debug.LogError("Could not create and set SpriteTintMaterial from SpriteTintEffect script component! Disabling  script...!");

                enabled = false;

                return;
            }

            SetDefaultValues();
        }

        private void AssignSpriteTintShaderMatIfNotAlready()
        {
            List<Material> mats = new List<Material>();

            spriteRenderer.GetSharedMaterials(mats);

            for(int i = 0; i < mats.Count; i++)
            {
                if (mats[i] == null) continue; 

                if (mats[i].shader.name == spriteTintShader.name)
                {
                    spriteTintMat = mats[i];

                    return;
                }
            }

            if(spriteTintMat == null && spriteTintShader != null)
            {
                spriteTintMat = new Material(spriteTintShader);

                if(!mats.Contains(spriteTintMat)) mats.Add(spriteTintMat);

                spriteRenderer.materials = mats.ToArray();
            }
        }

        private void SetDefaultValues()
        {
            if (spriteTintMat == null) return;

            spriteTintMat.SetTexture("_MainSpriteText", mainSpriteTexture.texture);

            spriteTintMat.SetColor("_SpriteTint", spriteTintColor);

            defaultSpriteTintColor = spriteTintColor;

            spriteTintAlpha = spriteTintColor.a;

            defaultSpriteTintAlpha = spriteTintAlpha;
        }

        public void SetSpriteTintColor(Color colorToSet, float alpha)
        {
            if (disableTintEffect) return;

            if (spriteTintMat == null) return;

            colorToSet.a = alpha;

            spriteTintColor = colorToSet;

            spriteTintAlpha = alpha;

            spriteTintMat.SetColor("_SpriteTint", spriteTintColor);
        }

        /*public void SetSpriteTintAlphaValue(float alphaValue)
        {
            if (disableTintEffect) return;

            if (spriteTintMat == null) return;

            spriteTintAlpha = alphaValue;

            Color color = spriteTintMat.GetColor("_SpriteTint");

            color.a = spriteTintAlpha;

            SetSpriteTintColor(color);
        }*/

        public void ResetSpriteTintEffectValuesToDefault()
        {
            SetSpriteTintColor(defaultSpriteTintColor, defaultSpriteTintAlpha);

            //SetSpriteTintAlphaValue(defaultSpriteTintAlpha);
        }
    }
}
