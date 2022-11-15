using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TeamMAsTD
{
    public class UnitShopSlotUIDragDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] [Tooltip("The Unit Scriptable Object of this slot.")] private UnitSO slotUnitScriptableObject;

        [SerializeField]
        [Tooltip("The object with the UI Image component attached that will follow the mouse when dragging. " +
        "Dropping returns the object to its original position.")]
        private Image dragDropUIImageObject;

        [SerializeField] 
        [Tooltip("Is the UI image when dragging and dropping the same as the UI Image of this shop slot's image? " +
        "If an image is set for the drag drop UI object, setting this option to true will override it. The default setting is true.")] 
        private bool dragDropVisualSameAsShopSlots = true;

        //INTERNALS....................................................................................

        //the original position of the dragDropUIImageObject (obj with UI Image that moves with mouse when dragging)
        private Vector3 originalDragDropPos;

        //the top most UI Canvas component that houses the rest of the children UI elements
        private Canvas inventoryParentCanva;
        //the Rect Transform of the top most parent UI canva above
        private RectTransform inventoryParentCanvaRect;

        private Tile currentlySelectedTile;

        //PRIVATES.........................................................................

        private void Awake()
        {
            if (dragDropVisualSameAsShopSlots) SetDragDropSameVisualAsShopSlot();
        }

        private void Start()
        {
            originalDragDropPos = dragDropUIImageObject.transform.localPosition;
        }

        private void CheckDragDropRequirements()
        {

        }

        private void SetDragDropSameVisualAsShopSlot()
        {

        }

        //UnityEventSystem Interface functions.........................................
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (slotUnitScriptableObject == null) return;

            
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (slotUnitScriptableObject == null) return;
            
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (slotUnitScriptableObject == null) return;
        }
    }
}
