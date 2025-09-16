using Il2CppInterop.Runtime.Attributes;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hikaria.QC.UI
{
    //[Il2CppImplements(typeof(IPointerDownHandler), typeof(IPointerUpHandler), typeof(IScrollHandler))]
    public class DraggableUI : MonoBehaviour
    {
        private RectTransform _dragRoot = null;
        private QuantumConsole _quantumConsole = null;
        private bool _lockInScreen = true;
        private ScrollRect _scrollRect = null;

        private UnityEvent _onBeginDrag = null;
        private UnityEvent _onDrag = null;
        private UnityEvent _onEndDrag = null;

        private Vector2 _lastPos;
        private bool _isDragging = false;
        private bool _isDraggingScroll = false;

        private RaycastResult _pointerCurrentRaycast;

        private void OnPointerDown(PointerEventData eventData)
        {
            _isDragging =
                _quantumConsole &&
                _quantumConsole.KeyConfig is not null &&
                _quantumConsole.KeyConfig.DragConsoleKey.IsHeld();

            _pointerCurrentRaycast = eventData.pointerCurrentRaycast;

            if (_isDragging)
            {
                _onBeginDrag.Invoke();
                _lastPos = eventData.position;
            }
            else
            {
                _isDraggingScroll = true;
                _scrollRect.OnBeginDrag(eventData);
            }
        }

        private void LateUpdate()
        {
            if (_isDragging)
            {
                Transform root = _dragRoot;
                if (!root) { root = transform.Cast<RectTransform>(); }

                Vector2 pos = InputHelper.GetMousePosition();
                Vector2 delta = pos - _lastPos;
                _lastPos = pos;

                if (_lockInScreen)
                {
                    Vector2 resolution = new Vector2(Screen.width, Screen.height);
                    if (pos.x <= 0 || pos.x >= resolution.x) { delta.x = 0; }
                    if (pos.y <= 0 || pos.y >= resolution.y) { delta.y = 0; }
                }

                root.Translate(delta);
                _onDrag.Invoke();
            }
            else if (_isDraggingScroll)
            {
                _scrollRect.OnDrag(new PointerEventData(EventSystem.current)
                {
                    button = PointerEventData.InputButton.Left,
                    position = InputHelper.GetMousePosition(),
                    pointerCurrentRaycast = _pointerCurrentRaycast
                });
            }
        }

        private void OnPointerUp(PointerEventData eventData)
        {
            if (_isDragging)
            {
                _isDragging = false;
                _onEndDrag.Invoke();
            }
            else
            {
                _isDraggingScroll = false;
                _scrollRect.OnEndDrag(eventData);
            }
        }

        private void OnScroll(PointerEventData eventData)
        {
            _scrollRect.OnScroll(eventData);
        }

        [HideFromIl2Cpp]
        public void Setup(QuantumConsole quantumConsole, RectTransform containerRect, ScrollRect scrollRect)
        {
            _dragRoot = containerRect;
            _quantumConsole = quantumConsole;
            _scrollRect = scrollRect;

            _onBeginDrag = new();
            _onDrag = new();
            _onEndDrag = new();

            _onBeginDrag.AddListener(new Action(() => { scrollRect.enabled = false; }));
            _onDrag.AddListener(new Action(() => { }));
            _onEndDrag.AddListener(new Action(() => { scrollRect.enabled = true; }));

            var eventTrigger = gameObject.AddComponent<EventTrigger>();

            var onBeginDragEntry = new EventTrigger.Entry();
            onBeginDragEntry.eventID = EventTriggerType.PointerDown;
            onBeginDragEntry.callback.AddListener(new Action<BaseEventData>((data) => { OnPointerDown(data.Cast<PointerEventData>()); }));
            eventTrigger.triggers.Add(onBeginDragEntry);

            var onEndDragEntry = new EventTrigger.Entry();
            onEndDragEntry.eventID = EventTriggerType.PointerUp;
            onEndDragEntry.callback.AddListener(new Action<BaseEventData>((data) => { OnPointerUp(data.Cast<PointerEventData>()); }));
            eventTrigger.triggers.Add(onEndDragEntry);

            var onScrollEntry = new EventTrigger.Entry();
            onScrollEntry.eventID = EventTriggerType.Scroll;
            onScrollEntry.callback.AddListener(new Action<BaseEventData>((data) => { OnScroll(data.Cast<PointerEventData>()); }));
            eventTrigger.triggers.Add(onScrollEntry);
        }
    }
}
