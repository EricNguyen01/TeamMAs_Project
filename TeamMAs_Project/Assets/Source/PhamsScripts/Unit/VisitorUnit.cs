using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    public class VisitorUnit : MonoBehaviour, IUnit
    {
        [field: SerializeField] public VisitorUnitSO visitorUnitSO { get; private set; }

        private VisitorPool poolContainsThisVisitor;

        private Path[] visitorPaths;

        private Path chosenPath;

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
            GetChosenPath();
        }

        private void GetVisitorPathsOnAwake()
        {
            visitorPaths = FindObjectsOfType<Path>();

            if(visitorPaths == null || visitorPaths.Length == 0)
            {
                Debug.LogWarning("There appears to be no visitor paths in the scene for visitor: " + name + " to walk on.");
            }
        }

        private void GetChosenPath()
        {
            if (visitorPaths == null || visitorPaths.Length == 0) return;

            chosenPath = visitorPaths[Random.Range(0, visitorPaths.Length)];
        }

        private void WalkOnPath()
        {
            
        }

        private void DespawnAndReturnToPool()
        {

        }

        public void SetPoolContainsThisVisitor(VisitorPool visitorPool)
        {
            poolContainsThisVisitor = visitorPool;
        }

        //IUnit Interface functions....................................................
        public UnitSO GetUnitScriptableObjectData()
        {
            return visitorUnitSO;
        }
    }
}
