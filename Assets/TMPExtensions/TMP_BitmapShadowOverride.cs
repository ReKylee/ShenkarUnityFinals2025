using TMPro;
using UnityEngine;

namespace TMPExtensions
{
    [ExecuteAlways]
    [RequireComponent(typeof(TMP_Text))]
    public class TMPBitmapShadowOverride : MonoBehaviour
    {
        private static readonly int ShadowHorStrength = Shader.PropertyToID("_ShadowHorStrength");
        private static readonly int ShadowVerStrength = Shader.PropertyToID("_ShadowVerStrength");

        [Range(0f, 1f)]
        public float shadowHorStrength = 1f;
        [Range(0f, 1f)]
        public float shadowVerStrength = 1f;

        private TMP_Text _tmpText;

        void Awake() => InitAndApply();
        void OnEnable() => InitAndApply();
        void OnValidate() => InitAndApply();

        void InitAndApply()
        {
            if (!_tmpText)
                _tmpText = GetComponent<TMP_Text>();
            if (!_tmpText)
                return;

            // Clones the material instance if needed
            Material matInstance = _tmpText.fontMaterial;
            matInstance.SetFloat(ShadowHorStrength, shadowHorStrength);
            matInstance.SetFloat(ShadowVerStrength, shadowVerStrength);
            _tmpText.fontMaterial = matInstance;

            // Mark graphic as dirty to refresh mesh/material
            _tmpText.SetAllDirty();
        }
    }
}
