using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.U2D.Animation;

namespace TeamMAsTD
{
    public class PlantRangeCircle : MonoBehaviour
    {
        [Header("Required Components")]

        [SerializeField] private PlantUnitSO plantUnitSO;

        [SerializeField] private Sprite rangeCircleSprite;

        [SerializeField] private bool disableRangeCircleOnAwake = true;

        private SpriteRenderer rangeCircleSpriteRenderer;

        private void Awake()
        {
            rangeCircleSpriteRenderer = GetComponent<SpriteRenderer>();

            if(rangeCircleSpriteRenderer == null)
            {
                rangeCircleSpriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            rangeCircleSpriteRenderer.sprite = rangeCircleSprite;

            if(disableRangeCircleOnAwake) DisplayPlantRangeCircle(false);
        }

        public void InitializePlantRangeCircle(PlantUnitSO plantSO, bool hasTileParent)
        {
            if (plantSO != null)
            {
                float maxRange = 0.0f;

                TDGrid grid = FindObjectOfType<TDGrid>();

                float tileSize = 1.0f;

                if (grid != null) tileSize = grid.tileSize;
                
                maxRange = (tileSize * plantSO.attackRangeInTiles) + (tileSize / 2.0f);

                //for some reasons, we need to do the below calculation to get the circle at the right scale
                //the *2.0f multiplier is beyond me idk why but it has to be *2.0f
                //the /tileSize is to account for the diff between the tile parent obj's scale (if there's 1) and this local scale
                if (hasTileParent) maxRange = (maxRange * 2.0f) / tileSize;
                else maxRange *= 2.0f;

                SetCircleScale(maxRange);
            }
            else//set default scale
            {
                SetCircleScale();
            }
        }

        public void InitializePlantRangeCircle(PlantUnit plantUnit)
        {
            if (plantUnit != null)
            {
                float tileSize = 1.0f;
                
                if(plantUnit.tilePlacedOn != null && plantUnit.tilePlacedOn.gridParent != null)
                {
                    tileSize = plantUnit.tilePlacedOn.gridParent.tileSize;
                }

                //for some reasons, we need to do the below calculation to get the circle at the right scale
                //the *2.0f multiplier is beyond me idk why but it has to be *2.0f
                //the /tileSize is to account for the diff between the tile parent obj's scale and this local scale
                float maxRange = (plantUnit.plantMaxAttackRange * 2.0f) / tileSize;

                SetCircleScale(maxRange);
            }
            else//set default scale
            {
                SetCircleScale();
            }
        }

        public void DisplayPlantRangeCircle(bool shouldDisplay)
        {
            if (shouldDisplay)
            {
                if(!gameObject.activeInHierarchy) gameObject.SetActive(true);

                return;
            }

            if (gameObject.activeInHierarchy) gameObject.SetActive(false);
        }

        private void SetCircleScale(float scale = 1.0f)
        {
            transform.localScale = new Vector3(scale, scale, scale);
        }
    }
}
