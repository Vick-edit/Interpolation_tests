using System.Collections.Generic;
using Gameplay.ClientServerWrappers;
using UnityEngine;

namespace Gameplay.ServerEntities
{
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class ServerBulletMono : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Rigidbody2D _rigidbody;

        public Rigidbody2D Rigidbody => _rigidbody;
        public bool IsVisible
        {
            get => _spriteRenderer.enabled;
            set => _spriteRenderer.enabled = value;
        }

        public bool IsDead { get; private set; }
        public BulletWrapperMono RootWrapper { get; set; }
        public Queue<BulletPositionSnapshot> ServerPositionSnapshots { get; private set; }


        private void Awake()
        {
            IsVisible = false;
            ServerPositionSnapshots = new Queue<BulletPositionSnapshot>(0);
        }

        private void OnEnable()
        {
            IsDead = false;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (IsDead || !(collision.gameObject.CompareTag(AppConstant.BulletTargetTag) || collision.gameObject.CompareTag(AppConstant.WallTag)))
            {
                return;
            }

            if (RootWrapper != null)
            {
                IsDead = true;
                gameObject.SetActive(false);

                ShootTargetMono target = null;
                if (collision.gameObject.CompareTag(AppConstant.BulletTargetTag))
                {
                    target = collision.gameObject.GetComponent<ShootTargetMono>();
                }

                RootWrapper.ServerCollisionCallback(target);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}