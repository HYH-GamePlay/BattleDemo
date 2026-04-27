using UnityEngine;
using UnityEngine.UI;

namespace Combat.View
{
    /// <summary>
    /// 血条视图 - 显示实体的HP和耐力
    /// </summary>
    public class HealthBarView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image _healthFill;
        [SerializeField] private Image _staminaFill;
        [SerializeField] private Text _healthText;
        [SerializeField] private GameObject _rootObject;

        [Header("Settings")]
        [SerializeField] private bool _faceCamera = true;
        [SerializeField] private Vector3 _offset = new Vector3(0, 2f, 0);

        private float _maxHealth = 100f;
        private float _maxStamina = 100f;

        private void Start()
        {
            if (_rootObject == null)
                _rootObject = gameObject;
        }

        private void LateUpdate()
        {
            if (_faceCamera && Camera.main != null)
            {
                transform.rotation = Camera.main.transform.rotation;
            }
        }

        public void SetHealth(float current, float max)
        {
            _maxHealth = max;
            if (_healthFill != null)
            {
                _healthFill.fillAmount = Mathf.Clamp01(current / max);
            }
            if (_healthText != null)
            {
                _healthText.text = $"{Mathf.Ceil(current)}/{Mathf.Ceil(max)}";
            }
        }

        public void SetStamina(float current, float max)
        {
            _maxStamina = max;
            if (_staminaFill != null)
            {
                _staminaFill.fillAmount = Mathf.Clamp01(current / max);
            }
        }

        public void Show()
        {
            if (_rootObject != null)
                _rootObject.SetActive(true);
        }

        public void Hide()
        {
            if (_rootObject != null)
                _rootObject.SetActive(false);
        }

        public void SetOffset(Vector3 offset)
        {
            _offset = offset;
        }
    }
}
