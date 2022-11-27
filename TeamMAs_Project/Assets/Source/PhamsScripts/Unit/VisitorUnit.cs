using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    public class VisitorUnit : MonoBehaviour, IUnit
    {
        [field: SerializeField] public VisitorUnitSO visitorUnitSO { get; private set; }

        private Wave waveSpawnedThisVisitor;

        private VisitorPool poolContainsThisVisitor;

        private List<Path> visitorPathsList = new List<Path>();

        private Path chosenPath;

        private Vector2 lastTilePos;

        private int currentPathElement = 0;

        private Vector2 currentTileWaypointPos;

        private bool startFollowingPath = false;

        private void Awake()
        {
            if(visitorUnitSO == null)
            {
                Debug.LogError("Visitor ScriptableObject data on visitor: " + name + " is missing! Visitor won't work. Disabling!");
                gameObject.SetActive(false);
                return;
            }

            GetVisitorPathsOnAwake();
        }

        private void OnEnable()
        {
            ProcessVisitorBecomesActive();
        }

        private void Update()
        {
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
            //TODO: Later if there are any appeasement effects/animations need to play before despawning
            //do them here and makes this function a coroutine if needed.

            poolContainsThisVisitor.ReturnVisitorToPool(this);

            waveSpawnedThisVisitor.RemoveInactiveVisitorsFromActiveList(this);
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
    }
}
