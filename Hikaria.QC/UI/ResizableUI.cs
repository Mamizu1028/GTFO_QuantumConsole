using Il2CppInterop.Runtime.Attributes;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Hikaria.QC.UI
{
    //[Il2CppImplements(typeof(IDragHandler))]
    public class ResizableUI : MonoBehaviour
    {
        private RectTransform _resizeRoot = null;
        private Canvas _resizeCanvas = null;
        private QuantumConsole _quantumConsole = null;

        private bool _lockInScreen = true;
        private Vector2 _minSize = new Vector2(500, 125);

        private void OnDrag(PointerEventData eventData)
        {
            Vector2 minBounds = (_resizeRoot.offsetMin + _minSize) * _resizeCanvas.scaleFactor;
            Vector2 maxBounds = _lockInScreen
                ? new Vector2(Screen.width, Screen.height)
                : new Vector2(Mathf.Infinity, Mathf.Infinity);

            Vector2 delta = eventData.delta;
            Vector2 posCurrent = eventData.position;
            Vector2 posLast = posCurrent - delta;

            Vector2 posCurrentBounded = new Vector2(
                Mathf.Clamp(posCurrent.x, minBounds.x, maxBounds.x),
                Mathf.Clamp(posCurrent.y, minBounds.y, maxBounds.y)
            );

            Vector2 posLastBounded = new Vector2(
                Mathf.Clamp(posLast.x, minBounds.x, maxBounds.x),
                Mathf.Clamp(posLast.y, minBounds.y, maxBounds.y)
            );

            Vector2 deltaBounded = posCurrentBounded - posLastBounded;

            _resizeRoot.offsetMax += deltaBounded / _resizeCanvas.scaleFactor;
        }

        private void OnEndDrag(PointerEventData eventData)
        {
            _quantumConsole.RequireRebuildLogLayout(true);
        }

        [HideFromIl2Cpp]
        public void Setup(QuantumConsole quantumConsole, RectTransform containerRect, Canvas canvas)
        {
            _resizeRoot = containerRect;
            _resizeCanvas = canvas;
            _quantumConsole = quantumConsole;

            var eventTrigger = gameObject.AddComponent<EventTrigger>();

            var onDragEntry = new EventTrigger.Entry();
            onDragEntry.eventID = EventTriggerType.Drag;
            onDragEntry.callback.AddListener((Action<BaseEventData>)((data) => { OnDrag(data.Cast<PointerEventData>()); }));
            eventTrigger.triggers.Add(onDragEntry);

            var onEndDragEntry = new EventTrigger.Entry();
            onEndDragEntry.eventID = EventTriggerType.EndDrag;
            onEndDragEntry.callback.AddListener((Action<BaseEventData>)((data) => { OnEndDrag(data.Cast<PointerEventData>()); }));
            eventTrigger.triggers.Add(onEndDragEntry);
        }
    }
}
