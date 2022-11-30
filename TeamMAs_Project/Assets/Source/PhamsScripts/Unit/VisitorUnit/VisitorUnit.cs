using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    [DisallowMultipleComponent]
    public class VisitorUnit : MonoBehaviour, IUnit, IDamageable
    {
        [field: SerializeField] public VisitorUnitSO visitorUnitSO { get; private set; }

        private float currentVisitorHealth = 0.0f;

        private Wave waveSpawnedThisVisitor;

        private VisitorPool poolContainsThisVisitor;

        private List<Path> visitorPathsList = new List<Path>();

        private Path chosenPath;

        private Vector2 lastTilePos;

        private int currentPathElement = 0;

        private Vector2 currentTileWaypointPos;

        private bool startFollowingPath = false;

        private float currentAppeasementTime = 0.0f;

        private float baseDamageVisualTime = 0.5f;

        private float currentDamageVisualTime = 0.0f;

        private SpriteRenderer visitorSpriteRenderer;

        private UnitWorldUI visitorWorldUIComponent;

        private Animation visitorAnimation;

        private void Awake()
        {
            if(visitorUnitSO == null)
            {
                Debug.LogError("Visitor ScriptableObject data on visitor: " + name + " is missing! Visitor won't work. Destroying Visitor!");
                return;
            }

            currentVisitorHealth = visitorUnitSO.happinessAsHealth;

            //if an appeasement anim clip is provided in visitorUnitSO-> set current appeasement time to that clip's length in seconds
            if (visitorUnitSO.visitorAppeasementAnimClip != null)
            {
                //need to have an animation component to play appeasenemt anim clip
                visitorAnimation = GetComponent<Animation>();

                if(visitorAnimation == null)
                {
                    visitorAnimation = gameObject.AddComponent<Animation>();
                }

                currentAppeasementTime = visitorUnitSO.visitorAppeasementAnimClip.length;

                visitorAnimation.AddClip(visitorUnitSO.visitorAppeasementAnimClip, "Appease");
            }
            //else set current appeasement time to visitor appeasement time in visitorUnitSO.
            else currentAppeasementTime = visitorUnitSO.visitorAppeasementTime;

            GetVisitorPathsOnAwake();

            visitorSpriteRenderer = GetComponent<SpriteRenderer>();

            visitorWorldUIComponent = GetComponent<UnitWorldUI>();
        }

        private void OnEnable()
        {
            ProcessVisitorBecomesActive();
        }

        private void Start()
        {
            if (visitorUnitSO == null)
            {
                if(poolContainsThisVisitor != null)
                {
                    poolContainsThisVisitor.RemoveVisitorFromPool(this);
                    Destroy(gameObject);
                    return;
                }

                enabled = false;
                gameObject.SetActive(false);
                return;
            }
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

        private void ProcessVisitorBecomesActive()
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

        //This function returns visitor to pool and deregister it from active visitor list in the wave that spawned it.
        private void ProcessVisitorDespawns()
        {
            visitorAnimation.Stop();

            //if either of these are null -> destroy visitor instead
            if(poolContainsThisVisitor == null || waveSpawnedThisVisitor == null)
            {
                Destroy(gameObject);
                return;
            }

            poolContainsThisVisitor.ReturnVisitorToPool(this);

            waveSpawnedThisVisitor.RemoveInactiveVisitorsFromActiveList(this);
        }

        private void ProcessVisitorDamageVisual()
        {
            if(currentDamageVisualTime <= 0.0f)
            {
                currentDamageVisualTime = 0.0f;
                if (visitorSpriteRenderer != null && visitorSpriteRenderer.color != Color.white) visitorSpriteRenderer.color = Color.white;
                return;
            }

            currentDamageVisualTime -= Time.deltaTime;

            Color white = Color.white;
            Color red = Color.red;
            Color color = Color.Lerp(red, white, currentDamageVisualTime / baseDamageVisualTime);
            if (visitorSpriteRenderer != null) visitorSpriteRenderer.color = color;
        }

        private void ProcessVisitorAppeased()
        {
            currentAppeasementTime -= Time.deltaTime;

            //return visitor to pool when appeasement time is up
            if (currentAppeasementTime <= 0.0f)
            {
                currentAppeasementTime = 0.0f;
                ProcessVisitorDespawns();
                return;
            }

            //if during appeasement time...
            //if there's an appease anim clip, it is triggered in TakeDamageFrom() function below...
            //else process procedural appeasment visual only if there's no provided appeasement anim clip in visitorUnitSO.
            if (visitorUnitSO.visitorAppeasementAnimClip == null) ProcessProceduralAppeasingVisual();
        }

        private void ProcessProceduralAppeasingVisual()
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

        //IUnit Interface functions....................................................
        public UnitSO GetUnitScriptableObjectData()
        {
            return visitorUnitSO;
        }

        //IDamageable Interface functions..............................................
        public void TakeDamageFrom(object damageCauser, float damage)
        {
            if(damageCauser.GetType() == typeof(PlantProjectile))
            {
                currentVisitorHealth -= damage;

                Debug.Log("Visitor: " + name + " took: " + damage + " damage.");

                //set health bar slider UI component value if not null
                if (visitorWorldUIComponent != null)
                {
                    visitorWorldUIComponent.SetHealthBarSliderValue(currentVisitorHealth, visitorUnitSO.happinessAsHealth);
                }
                
                //to trigger damage color change in ProcessDamageVisual() function above
                currentDamageVisualTime = baseDamageVisualTime;

                //play appease anim clip when happiness dropped below 0.0f.
                if(currentVisitorHealth <= 0.0f)
                {
                    if(visitorAnimation != null)
                    {
                        visitorAnimation.Play("Appease");
                    }
                }
            }
        }
    }
}
