using UnityEngine;

namespace Combat.View
{
    /// <summary>
    /// 特效视图 - 管理实体的视觉特效
    /// </summary>
    public class VFXView : MonoBehaviour
    {
        [Header("VFX Prefabs")]
        [SerializeField] private GameObject _hitEffect;
        [SerializeField] private GameObject _healEffect;
        [SerializeField] private GameObject _deathEffect;
        [SerializeField] private GameObject _perfectBlockEffect;
        [SerializeField] private GameObject _counterEffect;
        [SerializeField] private GameObject _dodgeEffect;

        [Header("Settings")]
        [SerializeField] private float _effectDuration = 1f;
        [SerializeField] private Vector3 _effectOffset = Vector3.up;

        public void PlayHitEffect()
        {
            SpawnEffect(_hitEffect);
        }

        public void PlayHealEffect()
        {
            SpawnEffect(_healEffect);
        }

        public void PlayDeathEffect()
        {
            SpawnEffect(_deathEffect);
        }

        public void PlayPerfectBlockEffect()
        {
            SpawnEffect(_perfectBlockEffect);
        }

        public void PlayCounterEffect()
        {
            SpawnEffect(_counterEffect);
        }

        public void PlayDodgeEffect()
        {
            SpawnEffect(_dodgeEffect);
        }

        public void PlayCustomEffect(GameObject effectPrefab)
        {
            SpawnEffect(effectPrefab);
        }

        private void SpawnEffect(GameObject prefab)
        {
            if (prefab == null) return;

            var effect = Instantiate(prefab, transform.position + _effectOffset, Quaternion.identity);

            // 自动销毁
            Destroy(effect, _effectDuration);
        }
    }
}
