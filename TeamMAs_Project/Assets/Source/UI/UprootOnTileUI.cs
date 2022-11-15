using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace TeamMAsTD
{
    public class UprootOnTileUI : MonoBehaviour
    {
        private Canvas tileWorldCanvas;
        private Camera mainCam;

        //PRIVATES.............................................................................................
        private void Awake()
        {
            tileWorldCanvas = GetComponentInChildren<Canvas>();

            if(tileWorldCanvas == null)
            {
                Debug.LogError("Tile World Canvas children component not found on tile: " + name + ". Plant uprooting won't work!");
                enabled = false;
                return;
            }

            mainCam = Camera.main;

            tileWorldCanvas.worldCamera = mainCam;
        }

        //PUBLICS..............................................................................................
        public void OnUprootOptionClicked()
        {
            //spawn uproot prompt
        }
    }
}
