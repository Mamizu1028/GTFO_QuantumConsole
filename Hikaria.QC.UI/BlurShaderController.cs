using UnityEngine;

namespace Hikaria.QC.UI
{
    public class BlurShaderController : MonoBehaviour
    {
        private Material _blurMaterial = null;
        private float _blurRadius = 1f;
        private Vector2 _referenceResolution = new Vector2(3840, 2160);

        private void LateUpdate()
        {
            if (_blurMaterial)
            {
                Vector2 resolution = new Vector2(Screen.width, Screen.height);
                float correction = resolution.y / _referenceResolution.y;
                _blurMaterial.SetFloat("_Radius", _blurRadius);
                _blurMaterial.SetFloat("_BlurMultiplier", correction);
            }
        }
    }
}
