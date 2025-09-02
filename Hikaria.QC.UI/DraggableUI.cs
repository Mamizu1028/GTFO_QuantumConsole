using Il2CppInterop.Runtime.Attributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Hikaria.QC.UI
{
    [Il2CppImplements(typeof(IPointerDownHandler), typeof(IPointerUpHandler))]
    public class DraggableUI : MonoBehaviour
    {
        private RectTransform _dragRoot = null;
        private QuantumConsole _quantumConsole = null;
        private bool _lockInScreen = true;

        private UnityEvent _onBeginDrag = null;
        private UnityEvent _onDrag = null;
        private UnityEvent _onEndDrag = null;

        private Vector2 _lastPos;
        private bool _isDragging = false;

        public void OnPointerDown(PointerEventData eventData)
        {
            _isDragging =
                _quantumConsole &&
                _quantumConsole.KeyConfig &&
                _quantumConsole.KeyConfig.DragConsoleKey.IsHeld();

            if (_isDragging)
            {
                _onBeginDrag.Invoke();
                _lastPos = eventData.position;
            }
        }

        public void LateUpdate()
        {
            if (_isDragging)
            {
                Transform root = _dragRoot;
                if (!root) { root = transform as RectTransform; }

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
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_isDragging)
            {
                _isDragging = false;
                _onEndDrag.Invoke();
            }
        }
    }
}
