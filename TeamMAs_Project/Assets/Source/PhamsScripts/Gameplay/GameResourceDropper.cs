// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    public class GameResourceDropper : MonoBehaviour
    {
        [System.Serializable]
        public struct ResourceDropperStruct
        {
            public GameResourceSO resourceToDrop;

            public int dropAmount;

            public StatPopupSpawner resourceDropPopupSpawner;

            [Tooltip("This resource drop chances compared to other resources in the resource drop list. In other words," +
                     "how likely is this resource is going to drop compared to all the resources that could be dropped by this dropper.")]
            [Range(1, 100)] public int resourceDropChanceBetweenResources;

            [Tooltip("This resource drop chances between itself and nothing. This means that after evaluating with other resources in drop list " +
                "or if this resource is the only one in drop list, calculate the drop chance of whether to drop this resource or drop nothing.")]
            [Range(0, 100)] public int chanceToNotDrop;

            [Tooltip("The opposite of chanceToNotDrop and is used to compare with this value")]
            [Range(1, 100)] public int chanceToDrop;

            public ResourceDropperStruct(GameResourceSO resourceSO, int amount, StatPopupSpawner resourcePopup, int dropChanceBtResources, int chanceToNotDrop, int chanceToDrop)
            {
                resourceToDrop = resourceSO;
                dropAmount = amount;
                resourceDropPopupSpawner = resourcePopup;
                resourceDropChanceBetweenResources = dropChanceBtResources;
                this.chanceToNotDrop = chanceToNotDrop;
                this.chanceToDrop = chanceToDrop;
            }
        }

        [field: SerializeField] public ResourceDropperStruct[] resourcesToDropArr { get; private set; }

        [SerializeField] 
        [Tooltip("Override drop chances between resources and always drop all resources in drop array whenever a drop occurs." +
            "If this is set to false then choose a random resource in the resource array and drop it")] 
        private bool dropAllResources = true;

        [SerializeField]
        [Tooltip("Override drop chances between resources and nothing so that " +
                 "whenever a drop occurs, a resource will always drop instead of having a chance of dropping nothing.")]
        private bool alwaysDropSomething = false;

        //this list stores the samples for the drop chances between resources within the resource to drop array
        private List<GameResourceSO> resourceVsResourcesDropChanceList = new List<GameResourceSO>();

        //this list is for calculate the chance between the chosen resource gotten from above list (or if there's only 1 resource only)
        //and the chance to drop nothing
        private List<int> resourceVsNothingDropChanceList = new List<int>();

        private void Awake()
        {
            if (resourcesToDropArr == null || resourcesToDropArr.Length == 0)
            {
                Debug.LogError("Resources To Drop Array of GameResourceDropper: " + name + " is not set. Resource dropper disabled!");

                enabled = false;

                return;
            }

            if (alwaysDropSomething)
            {
                if(resourcesToDropArr != null && resourcesToDropArr.Length > 0)
                {
                    for(int i = 0; i < resourcesToDropArr.Length; i++)
                    {
                        resourcesToDropArr[i].chanceToNotDrop = 0;
                    }
                }
            }

            SetDropChancesBetweenResourcesList();
        }

        //Public function to performs either dropping a random resource in the resource array based on total drop chances
        //or drop all resources in the array. Final resource drop can be something or nothing.
        //this function overloaded the base DropResource() below
        public void DropResource()
        {
            if (resourcesToDropArr == null || resourcesToDropArr.Length == 0)
            {
                Debug.LogError("Resources To Drop Array of GameResourceDropper: " + name + " is not set. Resource dropper disabled!");

                enabled = false;

                return;
            }

            //if drop all resources in resource to drop array
            if (dropAllResources)
            {
                //Performs DropResource func on all struck elements in resource to drop array
                //any of these resource element could also have the chance to drop nothing instead depending on "alwaysDropSomething" status
                DropAllResources(alwaysDropSomething);

                return;
            }

            //else if only drop 1 random resource in array
            //choose a random resource dropper struct element in resource drop array based on total chance between struct elements
            ResourceDropperStruct resourceToDropStruct = GetFinalResourceToDropFromDropChanceList();
            
            //process resource drop (if not always drop, also calculates the chance of this resource should drop or nothing is dropped instead)
            DropResource(resourceToDropStruct, alwaysDropSomething);
        }

        private void DropResource(ResourceDropperStruct resourceToDropStruct, bool alwaysDrop)
        {
            //if resource to drop is already nothing -> return nothing
            if (resourceToDropStruct.resourceToDrop == null) return;

            //if always drop this resource from resource to drop struct -> proceed to add to resource
            if (alwaysDrop)
            {
                resourceToDropStruct.resourceToDrop.AddResourceAmount(resourceToDropStruct.dropAmount);

                if(resourceToDropStruct.resourceDropPopupSpawner != null)
                {
                    resourceToDropStruct.resourceDropPopupSpawner.PopUp(null, "+" + resourceToDropStruct.dropAmount.ToString(), StatPopup.PopUpType.Positive);
                }

                return;
            }

            //else if there's a chance to drop this resource or drop nothing
            //calculate drop chance between resource and nothing to see if we should drop something or nothing
            bool dropSomething = ShouldDropResourceFrom(resourceToDropStruct);

            if (!dropSomething) return;

            resourceToDropStruct.resourceToDrop.AddResourceAmount(resourceToDropStruct.dropAmount);

            if (resourceToDropStruct.resourceDropPopupSpawner != null)
            {
                resourceToDropStruct.resourceDropPopupSpawner.PopUp(null, "+" + resourceToDropStruct.dropAmount.ToString(), StatPopup.PopUpType.Positive);
            }
        }

        public void DropAllResources(bool alwaysDrop)
        {
            for(int i = 0; i < resourcesToDropArr.Length; i++)
            {
                DropResource(resourcesToDropArr[i], alwaysDrop);
            }
        }

        public void DropASpecificResource(GameResourceSO gameResourceSOToDrop, bool alwaysDrop = true)
        {
            if (resourcesToDropArr == null || resourcesToDropArr.Length == 0) return;

            for (int i = 0; i < resourcesToDropArr.Length; i++)
            {
                if (resourcesToDropArr[i].resourceToDrop == gameResourceSOToDrop)
                {
                    DropResource(resourcesToDropArr[i], alwaysDrop);
                }
            }
        }

        private ResourceDropperStruct GetFinalResourceToDropFromDropChanceList()
        {
            //in case of invalid resources vs resources drop chance list
            if (resourceVsResourcesDropChanceList == null || resourceVsResourcesDropChanceList.Count == 0)
            {
                return new ResourceDropperStruct(null, 0, null, 0, 0, 0);//return a default layout with all null or 0 value 
            }

            if(resourceVsResourcesDropChanceList.Count == 1)
            {
                for(int i = 0; i < resourcesToDropArr.Length; i++)
                {
                    if (resourcesToDropArr[i].resourceToDrop == resourceVsResourcesDropChanceList[0])
                    {
                        return resourcesToDropArr[i];
                    }
                }
            }

            //shuffle the resourceVsResourcesDropChanceList
            resourceVsResourcesDropChanceList= HelperFunctions.RandomShuffleListElements(resourceVsResourcesDropChanceList);

            GameResourceSO randomlyChosenResourceSO = resourceVsResourcesDropChanceList[Random.Range(0, resourceVsResourcesDropChanceList.Count)];

            for (int i = 0; i < resourcesToDropArr.Length; i++)
            {
                if (resourcesToDropArr[i].resourceToDrop == randomlyChosenResourceSO)
                {
                    return resourcesToDropArr[i];
                }
            }

            //if no valid resource was acquired for dropping
            return new ResourceDropperStruct(null, 0, null, 0, 0, 0);//return a default layout with all null or 0 value 
        }

        private void SetDropChancesBetweenResourcesList()
        {
            //check if resources to drop array has been set in editor (not null and length > 0)
            if (resourcesToDropArr == null || resourcesToDropArr.Length == 0) return;
            
            //loop through resources to drop array
            for (int i = 0; i < resourcesToDropArr.Length; i++)
            {
                //if a resource to drop element is null -> skip
                if (resourcesToDropArr[i].resourceToDrop == null) continue;

                //fill the drop chance for the current resource to drop element by adding total to list
                for(int j = 0; j < resourcesToDropArr[i].resourceDropChanceBetweenResources; j++)
                {
                    resourceVsResourcesDropChanceList.Add(resourcesToDropArr[i].resourceToDrop);
                }
            }
        }

        private bool ShouldDropResourceFrom(ResourceDropperStruct resourceDropperStruct)
        {
            resourceVsNothingDropChanceList.Clear();

            int dropSmth = 0;

            int dropNothing = 1;

            int totalDropChance = resourceDropperStruct.chanceToDrop + resourceDropperStruct.chanceToNotDrop;

            int chanceToDropCount = 0;

            int chanceToNotDropCount = 0;

            while((chanceToDropCount + chanceToNotDropCount) < totalDropChance)
            {
                if(chanceToDropCount < resourceDropperStruct.chanceToDrop)
                {
                    resourceVsNothingDropChanceList.Add(dropSmth);

                    chanceToDropCount++;
                }
                if(chanceToNotDropCount < resourceDropperStruct.chanceToNotDrop)
                {
                    resourceVsNothingDropChanceList.Add(dropNothing);

                    chanceToNotDropCount++;
                }
            }

            resourceVsNothingDropChanceList = HelperFunctions.RandomShuffleListElements(resourceVsNothingDropChanceList);

            int dropChance = resourceVsNothingDropChanceList[Random.Range(0, resourceVsNothingDropChanceList.Count)];

            if (dropChance == dropSmth) return true;

            return false;
        }

        public void SetDropAmountForResource(GameResourceSO gameResourceSO, int dropAmount)
        {
            if (resourcesToDropArr == null || resourcesToDropArr.Length == 0) return;

            for(int i = 0; i < resourcesToDropArr.Length; i++)
            {
                if (resourcesToDropArr[i].resourceToDrop == gameResourceSO)
                {
                    resourcesToDropArr[i].dropAmount = dropAmount;
                }
            }
        }

        public void SetDropChanceForResource(GameResourceSO resourceSO, int dropChanceBtResources, int chanceToDrop, int chanceToNotDrop)
        {
            if (resourcesToDropArr == null || resourcesToDropArr.Length == 0) return;

            for (int i = 0; i < resourcesToDropArr.Length; i++)
            {
                if (resourcesToDropArr[i].resourceToDrop == resourceSO)
                {
                    if(dropChanceBtResources > 0)
                    {
                        resourcesToDropArr[i].resourceDropChanceBetweenResources = dropChanceBtResources;
                    }
                    if(chanceToDrop > 0)
                    {
                        resourcesToDropArr[i].chanceToDrop = chanceToDrop;
                    }
                    if(chanceToNotDrop >= 0)
                    {
                        resourcesToDropArr[i].chanceToNotDrop = chanceToNotDrop;
                    }
                }
            }
        }
    }
}
