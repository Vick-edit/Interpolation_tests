using System.Collections;
using UnityEngine;

namespace Gameplay
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(ParticleSystem))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class ShootTargetMono : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private ParticleSystem _effects;
        [SerializeField] private Collider2D _collider;

        public ShootTargetPoolMono RootPool { get; set; }
        public bool IsDead { get; private set; }


        public void OnClientDeathDataReached()
        {
            StartCoroutine(OnDying());
        }

        public void OnServerDeathDataReached()
        {
            IsDead = true;
            _collider.enabled = false;
        }


        private void OnEnable()
        {
            IsDead = false;
            _collider.enabled = true;
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
            if (!IsDead && RootPool != null)
            {
                RootPool.TargetWasDestroyed();
            }
        }

        private IEnumerator OnDying()
        {
            var color = _renderer.color;
            color.a = 0;
            _renderer.color = color;

            _effects.Play(true);
            yield return new WaitWhile(() => _effects.isPlaying); ;
            
            if (RootPool != null)
            {
                RootPool.ReturnToPool(this);
                gameObject.SetActive(false);
                color.a = 1;
                _renderer.color = color;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}