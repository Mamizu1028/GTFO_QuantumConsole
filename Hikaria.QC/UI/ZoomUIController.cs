using Il2CppInterop.Runtime.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hikaria.QC.UI
{
    public class ZoomUIController : MonoBehaviour
    {
        private float _zoomIncrement = 0.1f;
        private float _minZoom = 0.1f;
        private float _maxZoom = 2f;

        private Button _zoomDownBtn = null;
        private Button _zoomUpBtn = null;

        private DynamicCanvasScaler _scaler = null;
        private QuantumConsole _quantumConsole = null;
        private TextMeshProUGUI _text = null;

        private float _lastZoom = -1;

        private float ClampAndSnapZoom(float zoom)
        {
            float clampedZoom = Mathf.Min(_maxZoom, Mathf.Max(_minZoom, zoom));
            float snappedZoom = Mathf.Round(clampedZoom / _zoomIncrement) * _zoomIncrement;
            return snappedZoom;
        }

        public void ZoomUp()
        {
            _scaler.ZoomMagnification = ClampAndSnapZoom(_scaler.ZoomMagnification + _zoomIncrement);
        }

        public void ZoomDown()
        {
            _scaler.ZoomMagnification = ClampAndSnapZoom(_scaler.ZoomMagnification - _zoomIncrement);
        }

        private void Update()
        {
            if (_quantumConsole && _quantumConsole.KeyConfig is not null)
            {
                if (_quantumConsole.KeyConfig.ZoomInKey.IsPressed()) { ZoomUp(); }
                if (_quantumConsole.KeyConfig.ZoomOutKey.IsPressed()) { ZoomDown(); }
            }
        }

        private void LateUpdate()
        {
            if (_scaler && _text)
            {
                float zoom = _scaler.ZoomMagnification;
                if (zoom != _lastZoom)
                {
                    _lastZoom = zoom;

                    int percentage = Mathf.RoundToInt(100 * zoom);
                    _text.text = $"{percentage}%";
                }
            }

            if (_zoomDownBtn)
            {
                _zoomDownBtn.interactable = _lastZoom > _minZoom;
            }

            if (_zoomUpBtn)
            {
                _zoomUpBtn.interactable = _lastZoom < _maxZoom;
            }
        }

        [HideFromIl2Cpp]
        public void Setup(Button zoomSizeDownButton, Button zoomSizeUpButton, DynamicCanvasScaler dynamicCanvasScaler, QuantumConsole quantumConsole, TextMeshProUGUI textMeshProUGUI)
        {
            _zoomDownBtn = zoomSizeDownButton;
            _zoomUpBtn = zoomSizeUpButton;
            _scaler = dynamicCanvasScaler;
            _quantumConsole = quantumConsole;
            _text = textMeshProUGUI;
        }
    }
}
