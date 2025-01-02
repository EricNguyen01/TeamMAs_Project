// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AbilityEffectReceivedInventory))]
    public class VisitorUnit : MonoBehaviour, IUnit, IDamageable
    {
        [field: ReadOnlyInspectorPlayMode]
        [field: SerializeField] 
        public VisitorUnitSO visitorUnitSO { get; private set; }

        [SerializeField] 
        private HeartEffect heartEffect;

        [SerializeField] 
        private GameResourceDropper visitorCoinsDropper;

        private AbilityEffectReceivedInventory abilityEffectReceivedInventory;

        public float currentVisitorHealth { get; private set; } = 0.0f;

        private Wave waveSpawnedThisVisitor;

        private VisitorPool poolContainsThisVisitor;

        private List<Path> visitorPathsList = new List<Path>();

        private Path chosenPath;

        private Vector2 lastTilePos;

        private int currentPathElement = 0;

        private Vector2 currentTileWaypointPos;

        private bool startFollowingPath = false;

        private bool isInvincible = false;

        private float baseAppeasementTime = 1.0f;

        private float currentAppeasementTime = 0.0f;

        private bool isProcessingAppeasement = false;

        private float baseDamageColorChangeTime = 0.25f;

        private float currentDamageColorChangeTime = 0.0f;

        private bool isProcessingDamageColorChange = false;

        private SpriteRenderer visitorSpriteRenderer;

        private Color visitorOriginalSpriteColor = Color.white;

        private Color visitorHitColor = Color.white;

        private UnitWorldUI visitorWorldUIComponent;

        private Collider2D visitorCollider2D;

        public ParticleSystem visitorAppeasedEffect { get; private set; }

        private bool isProcessingAppeasementEffect = false;

        private WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

        //Invoked on visitor appeased
        //PlantAimShootSystem.cs sub to this event to update its targets list
        public static event System.Action OnVisitorAppeased;

        [Serializable]
        private struct VisitorStatsDebug
        {
            public float visitorMoveSpeed;
        }

        private VisitorStatsDebug DEBUG_VisitorStats = new VisitorStatsDebug();

        private void Awake()
        {
            if(visitorUnitSO == null)
            {
                Debug.LogError("Visitor ScriptableObject data on visitor: " + name + " is missing! Visitor won't work. Destroying Visitor!");
                
                if (poolContainsThisVisitor != null)
                {
                    poolContainsThisVisitor.RemoveVisitorFromPool(this);
                    Destroy(gameObject);
                    return;
                }

                enabled = false;
                gameObject.SetActive(false);
                return;
            }

            visitorUnitSO = Instantiate(visitorUnitSO);

            UpdateVisitorStatsDebugData();

            abilityEffectReceivedInventory = GetComponent<AbilityEffectReceivedInventory>();

            if(abilityEffectReceivedInventory == null)
            {
                abilityEffectReceivedInventory = gameObject.AddComponent<AbilityEffectReceivedInventory>();
            }

            if (visitorUnitSO.visitorAppeasementTime <= 0.1f) baseAppeasementTime = 1.0f;
            else baseAppeasementTime = visitorUnitSO.visitorAppeasementTime;

            currentAppeasementTime = baseAppeasementTime;

            if (!heartEffect) heartEffect = GetComponentInChildren<HeartEffect>();

            GetVisitorPathsOnAwake();

            visitorSpriteRenderer = GetComponent<SpriteRenderer>();

            if (visitorSpriteRenderer != null) visitorOriginalSpriteColor = visitorSpriteRenderer.color;

            visitorWorldUIComponent = GetComponent<UnitWorldUI>();

            visitorCollider2D = GetComponent<Collider2D>();

            if(visitorCoinsDropper != null)
            {
                visitorCoinsDropper.SetDropAmountForResource(visitorUnitSO.coinResourceToDrop, visitorUnitSO.visitorsAppeasedCoinsDrop);

                visitorCoinsDropper.SetDropChanceForResource(visitorUnitSO.coinResourceToDrop,
                                                             0,
                                                             visitorUnitSO.chanceToDropCoins,
                                                             visitorUnitSO.chanceToNotDropCoins);
            }

            if(visitorUnitSO && visitorUnitSO.unitDestroyEffectPrefab)
            {
                GameObject visitorAppeasedEffectGO = visitorUnitSO.SpawnUnitEffectGameObject(visitorUnitSO.unitDestroyEffectPrefab,
                                                                                             transform,
                                                                                             true,
                                                                                             false);

                visitorAppeasedEffect = visitorAppeasedEffectGO.GetComponent<ParticleSystem>();

                if (visitorAppeasedEffect)
                {
                    var fxMain = visitorAppeasedEffect.main;

                    fxMain.loop = false;

                    fxMain.playOnAwake = false;
                }
            }
        }

        private void OnEnable()
        {
            ProcessVisitorBecomesActiveFromPool();
        }

        private void Update()
        {
            //stop updating visitor's behaviors if happiness(health) is below 0:
            if (currentVisitorHealth <= 0.0f) return;
            
            //if happiness(Health) is not below 0:
            WalkOnPath();
        }

        private void ProcessVisitorBecomesActiveFromPool()
        {
            //set visitor's pos to 1st tile's pos in chosen path
            SetVisitorToFirstTileOnPath(GetChosenPath());

            if (chosenPath == null)
            {
                ProcessVisitorDespawns();

                Debug.LogWarning("There appears to be no visitor paths in the scene for visitor: " + name + " to walk on.");

                return;
            }

            List<Tile> orderedPathTiles = chosenPath.GetOrderedPathTiles();

            if(orderedPathTiles == null || orderedPathTiles.Count == 0)
            {
                ProcessVisitorDespawns();

                Debug.LogWarning("Could not find any valid Path Tile on chosen Path game object for visitor to walk on. Visitor despawned!");

                return;
            }

            //get last tile's pos in path
            lastTilePos = (Vector2)orderedPathTiles[orderedPathTiles.Count - 1].transform.position;

            //reset to start tile
            currentPathElement = 0;

            currentTileWaypointPos = (Vector2)orderedPathTiles[currentPathElement].transform.position;

            //reset visitors health, various timers, collider, UI elements, and colors to their original values
            ResetVisitorStatsAndLooks();

            isInvincible = false;//reset invincibility just in case

            //start following path
            startFollowingPath = true;
        }

        private void GetVisitorPathsOnAwake()
        {
            Path[] paths = FindObjectsOfType<Path>();

            for (int i = 0; i < paths.Length; i++)
            {
                if (paths[i].GetOrderedPathTiles() == null || paths[i].GetOrderedPathTiles().Count == 0) continue;

                visitorPathsList.Add(paths[i]);
            }

            if(visitorPathsList == null || visitorPathsList.Count == 0)
            {
                Debug.LogWarning("There appears to be no visitor paths in the scene for visitor: " + name + " to walk on.");
            }
        }

        private Path GetChosenPath()
        {
            if (visitorPathsList.Count == 0)
            {
                chosenPath = null;
            }
            else chosenPath = visitorPathsList[UnityEngine.Random.Range(0, visitorPathsList.Count)];

            return chosenPath;
        }

        private void SetVisitorToFirstTileOnPath(Path chosenPath)
        {
            if (chosenPath == null) return;

            if (chosenPath.GetOrderedPathTiles() == null || chosenPath.GetOrderedPathTiles().Count == 0) return;

            transform.position = chosenPath.GetOrderedPathTiles()[0].transform.position;
        }

        private void WalkOnPath()
        {
            if (chosenPath == null) return;

            if (!startFollowingPath) return;

            //if reached last tile pos in path
            if(Vector2.Distance((Vector2)transform.position, lastTilePos) <= 0.05f)
            {
                //deals emotional damage
                VisitorDealsDissapointmentDamage();

                ProcessVisitorDespawns();

                startFollowingPath = false;

                return;
            }

            //if reached target tile's pos in chosen path -> set next tile element in chosen path as target waypoint
            if(Vector2.Distance((Vector2)transform.position, currentTileWaypointPos) <= 0.05f)
            {
                currentPathElement++;

                currentTileWaypointPos = (Vector2)chosenPath.GetOrderedPathTiles()[currentPathElement].transform.position;
            }

            transform.position = Vector2.MoveTowards(transform.position, currentTileWaypointPos, visitorUnitSO.moveSpeed * Time.deltaTime);
        }

        private void VisitorHealsPlayerEmotionalHealthOnAppeased()
        {
            if (GameResource.gameResourceInstance == null || GameResource.gameResourceInstance.emotionalHealthSOTypes == null) return;

            if (GameResource.gameResourceInstance.emotionalHealthSOTypes.Count == 0) return;

            if (visitorUnitSO == null) return;

            for (int i = 0; i < GameResource.gameResourceInstance.emotionalHealthSOTypes.Count; i++)
            {
                if (visitorUnitSO.visitorType != GameResource.gameResourceInstance.emotionalHealthSOTypes[i].visitorTypeAffectingThisHealth) continue;

                GameResource.gameResourceInstance.emotionalHealthSOTypes[i].AddResourceAmount(visitorUnitSO.appeasementHealAmount);
            }
        }

        private void VisitorDealsDissapointmentDamage()
        {
            if (GameResource.gameResourceInstance == null || GameResource.gameResourceInstance.emotionalHealthSOTypes == null) return;

            if(GameResource.gameResourceInstance.emotionalHealthSOTypes.Count == 0) return;

            if (visitorUnitSO == null) return;

            for(int i = 0; i < GameResource.gameResourceInstance.emotionalHealthSOTypes.Count; i++)
            {
                if (visitorUnitSO.visitorType != GameResource.gameResourceInstance.emotionalHealthSOTypes[i].visitorTypeAffectingThisHealth) continue;

                GameResource.gameResourceInstance.emotionalHealthSOTypes[i].RemoveResourceAmount(visitorUnitSO.dissapointmentDamage);
            }

            //GameResource.gameResourceInstance.emotionalHealthSO.RemoveResourceAmount(visitorUnitSO.emotionalAttackDamage);
        }

        //This function returns visitor to pool and deregister it from active visitor list in the wave that spawned it.
        private void ProcessVisitorDespawns()
        {
            //if either of these are null -> destroy visitor instead
            if(poolContainsThisVisitor == null || waveSpawnedThisVisitor == null)
            {
                Destroy(gameObject);
                return;
            }

            //else return to this visitor's correct pool
            poolContainsThisVisitor.ReturnVisitorToPool(this);

            //remove from active visitors list in wave that spawned this visitor
            waveSpawnedThisVisitor.RemoveInactiveVisitorsFromActiveList(this);
        }

        private void ResetVisitorStatsAndLooks()
        {
            //set starting HP
            currentVisitorHealth = visitorUnitSO.happinessAsHealth;

            currentAppeasementTime = baseAppeasementTime;

            currentDamageColorChangeTime = baseDamageColorChangeTime;

            visitorCollider2D.enabled = true;

            if (visitorWorldUIComponent != null)
            {
                visitorWorldUIComponent.EnableUnitNameTextUI(true);

                visitorWorldUIComponent.EnableUnitHealthBarSlider(true);

                visitorWorldUIComponent.SetHealthBarSliderValue(currentVisitorHealth, visitorUnitSO.happinessAsHealth, true);
            }

            if (visitorSpriteRenderer != null)
            {
                visitorOriginalSpriteColor.a = 255.0f;

                visitorSpriteRenderer.color = visitorOriginalSpriteColor;

                visitorHitColor = visitorUnitSO.visitorHitColor;
            }
        }

        private IEnumerator ProcessVisitorDamageColorChange()
        {
            if (isProcessingDamageColorChange || currentVisitorHealth <= 0.0f) yield break;

            isProcessingDamageColorChange = true;

            Color visitorDamageColor;

            //lerp visitor color to damage color (duration is currentDamageVisualTime)
            while(currentDamageColorChangeTime >= 0.0f)
            {
                if (currentVisitorHealth <= 0.0f) break;

                if (currentDamageColorChangeTime > 0.0f)
                    visitorDamageColor = Color.Lerp(visitorHitColor,
                                                    visitorOriginalSpriteColor,
                                                    currentDamageColorChangeTime / baseDamageColorChangeTime);

                else
                    visitorDamageColor = visitorHitColor;

                visitorDamageColor.a = 255.0f;

                if (visitorSpriteRenderer != null) visitorSpriteRenderer.color = visitorDamageColor;

                yield return waitForFixedUpdate;

                currentDamageColorChangeTime -= Time.fixedDeltaTime;
            }

            //once visitor's color to damage color lerp is complete -> reset color to base visitor color

            visitorOriginalSpriteColor.a = 255.0f;

            visitorSpriteRenderer.color = visitorOriginalSpriteColor;

            //reset variables

            isProcessingDamageColorChange = false;

            currentDamageColorChangeTime = baseDamageColorChangeTime;

            yield break;
        }

        private IEnumerator ProcessVisitorAppeased()
        {
            if (isProcessingAppeasement) yield break;

            isProcessingAppeasement = true;

            //hide name and happiness bar on being appeased
            if (visitorWorldUIComponent != null)
            {
                //hide name text UI
                visitorWorldUIComponent.EnableUnitNameTextUI(false);
                //hide happiness bar slider UI
                visitorWorldUIComponent.EnableUnitHealthBarSlider(false);
            }

            while (currentAppeasementTime > 0.0f)
            {
                //during appeasement time,
                //if there's an appease anim clip, it is triggered in TakeDamageFrom() function below...
                //also process appeasment visual iteratively here...
                VisitorAppeasementColorFade();

                yield return waitForFixedUpdate;

                currentAppeasementTime -= Time.fixedDeltaTime;
            }

            ProcessVisitorDespawns();

            currentAppeasementTime = 0.0f;

            //reset variables

            isProcessingAppeasement = false;

            yield break;
        }

        private void VisitorAppeasementColorFade()
        {
            //slowly fade visitor on being appeased
            if(visitorSpriteRenderer != null)
            {
                var color = visitorSpriteRenderer.color;

                if (currentAppeasementTime <= 0.0f) color.a = 0.0f;
                else color.a = currentAppeasementTime / baseAppeasementTime;

                visitorSpriteRenderer.color = color;
            }
        }

        private IEnumerator ProcessVisitorAppeasementEffect()
        {
            if (isProcessingAppeasementEffect) yield break;

            if(!visitorAppeasedEffect) yield break;

            isProcessingAppeasementEffect = true;

            visitorAppeasedEffect.transform.SetParent(null);

            if (visitorUnitSO)
            {
                visitorAppeasedEffect.transform.rotation = visitorUnitSO.unitDestroyEffectPrefab.transform.rotation;

                visitorAppeasedEffect.transform.localScale = visitorUnitSO.unitDestroyEffectPrefab.transform.localScale;
            }

            if (!visitorAppeasedEffect.gameObject.activeInHierarchy)
                visitorAppeasedEffect.gameObject.SetActive(true);

            visitorAppeasedEffect.Play();

            ParticleSystem[] appeasementFXs = visitorAppeasedEffect.GetComponentsInChildren<ParticleSystem>();

            if(appeasementFXs != null && appeasementFXs.Length > 0)
            {
                bool isEmitting = true;

                while(isEmitting)
                {
                    for (int i = 0; i < appeasementFXs.Length; i++)
                    {
                        if (appeasementFXs[i].isEmitting)
                        {
                            yield return new WaitForSeconds(0.1f);

                            break;
                        }

                        if(i == appeasementFXs.Length - 1)
                        {
                            if (!appeasementFXs[i].isEmitting) isEmitting = false;
                        }
                    }
                }
            }

            yield return new WaitForSeconds(0.1f);

            if (visitorAppeasedEffect.gameObject.activeInHierarchy)
                visitorAppeasedEffect.gameObject.SetActive(false);

            visitorAppeasedEffect.transform.SetParent(transform, true);

            visitorAppeasedEffect.transform.localPosition = Vector3.zero;

            isProcessingAppeasementEffect = false;

            yield break;
        }

        public void SetVisitorInvincible(bool shouldBeInvincible)
        {
            isInvincible = shouldBeInvincible;
        }

        public void SetVisitorFollowingPath(bool shouldFollowPath)
        {
            startFollowingPath = shouldFollowPath;
        }

        public void SetPoolContainsThisVisitor(VisitorPool visitorPool)
        {
            poolContainsThisVisitor = visitorPool;
        }

        public void SetWaveSpawnedThisVisitor(Wave wave)
        {
            waveSpawnedThisVisitor = wave;
        }

        public Vector2 GetVisitorUnitMoveDir(bool normalized)
        {
            if (normalized) 
            { 
                return (currentTileWaypointPos - (Vector2)transform.position).normalized; 
            }

            return currentTileWaypointPos - (Vector2)transform.position;
        }

        public void UpdateVisitorStatsDebugData()
        {
            DEBUG_VisitorStats.visitorMoveSpeed = visitorUnitSO.moveSpeed;
        }

        //IUnit Interface functions....................................................

        public UnitSO GetUnitScriptableObjectData()
        {
            return visitorUnitSO;
        }

        public object GetUnitObject()
        {
            return this;
        }

        public Tile GetTileUnitIsOn()
        {
            //cast a 2d ray backward
            Ray backRay = new Ray(transform.position, -transform.forward);

            RaycastHit2D hit2DBackward = Physics2D.GetRayIntersection(backRay, Mathf.Infinity, LayerMask.GetMask("Tile"));

            if(hit2DBackward.collider)
            {
                return hit2DBackward.collider.GetComponent<Tile>();
            }

            //cast a 2d ray foreward
            Ray forwardRay = new Ray(transform.position, transform.forward);

            RaycastHit2D hit2DForward = Physics2D.GetRayIntersection(forwardRay, Mathf.Infinity, LayerMask.GetMask("Tile"));

            if (hit2DForward.collider)
            {
                return hit2DForward.collider.GetComponent<Tile>();
            }

            return null;
        }

        public Transform GetUnitTransform()
        {
            return transform;
        }

        public LayerMask GetUnitLayerMask()
        {
            return gameObject.layer;
        }

        public AbilityEffectReceivedInventory GetAbilityEffectReceivedInventory()
        {
            return abilityEffectReceivedInventory;
        }

        public void UpdateUnitSOData(UnitSO replacementUnitSO)
        {

        }

        //IDamageable Interface functions..............................................

        public void TakeDamageFrom(object damageCauser, float damage)
        {
            //if is temporary invincible -> dont process damage and exit func
            if (isInvincible) return;

            if (damageCauser.GetType() == typeof(PlantProjectile))
            {
                PlantProjectile projectile = (PlantProjectile)damageCauser;

                //if this visitor is not a type that the hit projectile's plant is set to target -> dont do damage to visitor.
                if (projectile.plantUnitSO != null && projectile.plantUnitSO.plantTargetsSpecifically != VisitorUnitSO.VisitorType.None)
                {
                    if (projectile.plantUnitSO.plantTargetsSpecifically != visitorUnitSO.visitorType)
                    {
                        return;//exit function
                    }
                }

                //else

                //set hit color
                visitorHitColor = projectile.projectileSpriteRenderer.color;

                //set damage to HP
                currentVisitorHealth -= damage * DamageMultiplier(projectile);

                //Debug.Log("Visitor: " + name + " took: " + damage + " damage from projectile: " + projectile.name);

                //set health bar slider UI component value if not null
                if (visitorWorldUIComponent != null)
                {
                    visitorWorldUIComponent.SetHealthBarSliderValue(currentVisitorHealth, visitorUnitSO.happinessAsHealth, true);
                }
                
                //process visitor damage color changes (lerps to hit color)

                if (!isProcessingDamageColorChange && gameObject.activeInHierarchy)
                    StartCoroutine(ProcessVisitorDamageColorChange());

                // to activate heart effect -sarita
                if(heartEffect)
                {
                    heartEffect.transform.position = transform.position;

                    if(!heartEffect.gameObject.activeInHierarchy) heartEffect.gameObject.SetActive(true);
                }

                //process visitor appeasement related functions when happiness dropped below 0.0f (or reached 100.0f).
                if(currentVisitorHealth <= 0.0f)
                {
                    if (visitorCollider2D != null) visitorCollider2D.enabled = false;

                    VisitorHealsPlayerEmotionalHealthOnAppeased();

                    if(visitorCoinsDropper != null)
                    {
                        visitorCoinsDropper.DropResource();
                    }

                    if(!isProcessingAppeasement && gameObject.activeInHierarchy) 
                        StartCoroutine(ProcessVisitorAppeased());

                    if (visitorAppeasedEffect)
                    {
                        if(!isProcessingAppeasementEffect && gameObject.activeInHierarchy)
                            StartCoroutine(ProcessVisitorAppeasementEffect());
                    }

                    OnVisitorAppeased?.Invoke();
                }
            }
        }

        private float DamageMultiplier(PlantProjectile projectile)
        {
            float damageMultiplier = 0.0f;

            if (visitorUnitSO.visitorType == VisitorUnitSO.VisitorType.Human) damageMultiplier += projectile.plantUnitSO.humanMultiplier;

            if (visitorUnitSO.visitorType == VisitorUnitSO.VisitorType.Pollinator) damageMultiplier += projectile.plantUnitSO.pollinatorMultiplier;

            return damageMultiplier;
        }

        public object ObjectTakenDamage()
        {
            return this;
        }
    }
}
