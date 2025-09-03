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

        private bool _lockInScreen = true;
        private Vector2 _minSize = new Vector2(500, 125);

        private Vector2 _lastDragPos = Vector2.zero;
        private bool _isDragging = false;

        public void OnDrag(PointerEventData eventData)
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

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isDragging = true;
            _lastDragPos = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;
        }

        public void OnDrag()
        {
            Vector2 minBounds = (_resizeRoot.offsetMin + _minSize) * _resizeCanvas.scaleFactor;
            Vector2 maxBounds = _lockInScreen
                ? new Vector2(Screen.width - 15, Screen.height - 15)
                : new Vector2(Mathf.Infinity, Mathf.Infinity);
            Vector2 mousePos = InputHelper.GetMousePosition();
            Vector2 delta = mousePos - _lastDragPos;
            Vector2 posCurrent = mousePos;
            Vector2 posLast = _lastDragPos;
            _lastDragPos = mousePos;

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

        private void LateUpdate()
        {
            if (_isDragging)
            {
                OnDrag();
            }
        }

        [HideFromIl2Cpp]
        public void Setup(RectTransform containerRect, Canvas canvas)
        {
            _resizeRoot = containerRect;
            _resizeCanvas = canvas;

            var eventTrigger = gameObject.AddComponent<EventTrigger>();

            var onBeginDragEntry = new EventTrigger.Entry();
            onBeginDragEntry.eventID = EventTriggerType.BeginDrag;
            onBeginDragEntry.callback.AddListener(new Action<BaseEventData>((data) => { OnBeginDrag(data.Cast<PointerEventData>()); }));
            eventTrigger.triggers.Add(onBeginDragEntry);

            var onEndDragEntry = new EventTrigger.Entry();
            onEndDragEntry.eventID = EventTriggerType.EndDrag;
            onEndDragEntry.callback.AddListener(new Action<BaseEventData>((data) => { OnEndDrag(data.Cast<PointerEventData>()); }));
            eventTrigger.triggers.Add(onEndDragEntry);
        }
    }
}
