using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AbilityEffectReceivedInventory))]
    public class VisitorUnit : MonoBehaviour, IUnit, IDamageable
    {
        [field: SerializeField] public VisitorUnitSO visitorUnitSO { get; private set; }

        [SerializeField] private HeartEffect heartEffect;

        [SerializeField] private GameResourceDropper visitorCoinsDropper;

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

        private float baseAppeasementTime;

        private float currentAppeasementTime = 0.0f;

        private float baseDamageVisualTime = 0.5f;

        private float currentDamageVisualTime = 0.0f;

        private SpriteRenderer visitorSpriteRenderer;

        private Color visitorOriginalSpriteColor = Color.white;

        private Color visitorHitColor = Color.white;

        private UnitWorldUI visitorWorldUIComponent;

        private Collider2D visitorCollider2D;
    

        //Invoked on visitor appeased
        //PlantAimShootSystem.cs sub to this event to update its targets list
        public static event System.Action OnVisitorAppeased;

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

            abilityEffectReceivedInventory = GetComponent<AbilityEffectReceivedInventory>();

            if(abilityEffectReceivedInventory == null)
            {
                abilityEffectReceivedInventory = gameObject.AddComponent<AbilityEffectReceivedInventory>();
            }

            baseAppeasementTime = visitorUnitSO.visitorAppeasementTime;

            currentAppeasementTime = visitorUnitSO.visitorAppeasementTime;

            if (heartEffect == null) heartEffect = GetComponentInChildren<HeartEffect>();

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
        }

        private void OnEnable()
        {
            ProcessVisitorBecomesActiveFromPool();
        }

        private void Update()
        {
            //functions that no depend on whether the visitor is appeased or not:
            ProcessVisitorDamageVisual();

            //if happiness(health) is below 0:
            if (currentVisitorHealth <= 0.0f)
            {
                ProcessVisitorAppeased();
                return;
            }
            
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
                return;
            }

            //get last tile's pos in path
            lastTilePos = (Vector2)chosenPath.orderedPathTiles[chosenPath.orderedPathTiles.Count - 1].transform.position;

            //reset to start tile
            currentPathElement = 0;
            currentTileWaypointPos = (Vector2)chosenPath.orderedPathTiles[currentPathElement].transform.position;

            //reset visitors health, various timers, collider, UI elements, and colors to their original values
            ResetVisitorStatsAndLooks();

            //start following path
            startFollowingPath = true;
        }

        private void GetVisitorPathsOnAwake()
        {
            Path[] paths = FindObjectsOfType<Path>();

            for (int i = 0; i < paths.Length; i++)
            {
                if (paths[i].orderedPathTiles == null || paths[i].orderedPathTiles.Count == 0) continue;

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
            else chosenPath = visitorPathsList[Random.Range(0, visitorPathsList.Count)];

            return chosenPath;
        }

        private void SetVisitorToFirstTileOnPath(Path chosenPath)
        {
            if (chosenPath == null || chosenPath.orderedPathTiles.Count == 0) return;

            transform.position = chosenPath.orderedPathTiles[0].transform.position;
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
                currentTileWaypointPos = (Vector2)chosenPath.orderedPathTiles[currentPathElement].transform.position;
            }

            transform.position = Vector2.MoveTowards(transform.position, currentTileWaypointPos, visitorUnitSO.moveSpeed * Time.deltaTime);
        }

        private void VisitorHealsHealthOnAppeased()
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

            currentDamageVisualTime = 0.0f;

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

        private void ProcessVisitorDamageVisual()
        {
            if (currentDamageVisualTime <= 0.0f) return;

            currentDamageVisualTime -= Time.deltaTime;

            Color color = Color.Lerp(visitorHitColor, visitorOriginalSpriteColor, currentDamageVisualTime / baseDamageVisualTime);
            color.a = 255.0f;

            if (visitorSpriteRenderer != null) visitorSpriteRenderer.color = color;

            if (currentDamageVisualTime <= 0.0f)
            {
                currentDamageVisualTime = 0.0f;

                if (visitorSpriteRenderer != null && visitorSpriteRenderer.color != visitorOriginalSpriteColor)
                {
                    visitorOriginalSpriteColor.a = 255.0f;

                    visitorSpriteRenderer.color = visitorOriginalSpriteColor;
                }
            }
        }

        private void ProcessVisitorAppeased()
        {
            if (currentAppeasementTime <= 0.0f)
            {
                if (gameObject.activeInHierarchy) ProcessVisitorDespawns();

                return;
            }

            currentAppeasementTime -= Time.deltaTime;

            //if during appeasement time...
            //if there's an appease anim clip, it is triggered in TakeDamageFrom() function below...
            //else process procedural appeasment visual only if there's no provided appeasement anim clip in visitorUnitSO.
            ProcessProceduralAppeasementVisual();

            //return visitor to pool when appeasement time is up
            if (currentAppeasementTime <= 0.0f)
            {
                currentAppeasementTime = 0.0f;

                ProcessVisitorDespawns();
            }
        }

        private void ProcessProceduralAppeasementVisual()
        {
            //hide name and happiness bar on being appeased
            if(visitorWorldUIComponent != null)
            {
                //hide name text UI
                visitorWorldUIComponent.EnableUnitNameTextUI(false);
                //hide happiness bar slider UI
                visitorWorldUIComponent.EnableUnitHealthBarSlider(false);
            }

            //slowly fade visitor on being appeased
            if(visitorSpriteRenderer != null)
            {
                var color = visitorSpriteRenderer.color;

                color.a = currentAppeasementTime / visitorUnitSO.visitorAppeasementTime;

                visitorSpriteRenderer.color = color;
            }
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
            Tile tileOn = null;

            //cast a 2d ray backward
            RaycastHit2D hit2DBackward = HelperFunctions.PerformSingleHit2DRaycastInDirection(transform.position, -transform.forward, Mathf.Infinity, "Tile");

            if(hit2DBackward.collider != null)
            {
                tileOn = hit2DBackward.collider.GetComponent<Tile>();

                if (tileOn != null) return tileOn;
            }

            //cast a 2d ray foreward
            RaycastHit2D hit2DForward = HelperFunctions.PerformSingleHit2DRaycastInDirection(transform.position, transform.forward, Mathf.Infinity, "Tile");

            if (hit2DForward.collider != null)
            {
                tileOn = hit2DForward.collider.GetComponent<Tile>();

                if (tileOn != null) return tileOn;
            }

            return tileOn;
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
            if(damageCauser.GetType() == typeof(PlantProjectile))
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
                
                //to trigger damage color change in ProcessDamageVisual() function above
                currentDamageVisualTime = baseDamageVisualTime;

                // to activate heart effect -sarita
                if(heartEffect != null)
                {
                    heartEffect.transform.position = transform.position;

                    heartEffect.gameObject.SetActive(true);
                }

                //process visitor appeasement related functions when happiness dropped below 0.0f (or reached 100.0f).
                if(currentVisitorHealth <= 0.0f)
                {
                    if (visitorCollider2D != null) visitorCollider2D.enabled = false;

                    VisitorHealsHealthOnAppeased();

                    if(visitorCoinsDropper != null)
                    {
                        visitorCoinsDropper.DropResource();
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
