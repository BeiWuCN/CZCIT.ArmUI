using UnityEngine;
using UnityEngine.EventSystems;

namespace CZCIT.ArmUI
{
    public sealed class WristPanelHandle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private WristPanelDock dock;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (dock) dock.BeginDrag(eventData);
        }

        // EventSystem requires IDragHandler to register a drag target.
        public void OnDrag(PointerEventData eventData) { }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (dock) dock.EndDrag(eventData);
        }
    }
}
