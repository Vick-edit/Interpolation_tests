using Gameplay.ClientEntities;
using Gameplay.ServerEntities;
using UnityEngine;

namespace Gameplay.ClientServerWrappers
{
    public sealed class BulletWrapperMono : MonoBehaviour
    {
        private Transform _serverBulletTransform;
        private Transform _clientBulletTransform;
        private ShootTargetMono _serverCollidedTarget;

        [SerializeField] private ServerBulletMono _serverBulletMono;
        [SerializeField] private ClientBulletMono _clientBulletMono;

        public ServerBulletMono ServerBulletMono => _serverBulletMono;
        public ClientBulletMono ClientBulletMono => _clientBulletMono;
        public SimulationSettingsMono SimulationSettingsMono { get; set; }

        public void Launch(Vector2 spawnPosition, Quaternion rotation, float speed)
        {
            _serverBulletTransform.position = spawnPosition;
            _serverBulletTransform.gameObject.SetActive(true);
            _clientBulletTransform.gameObject.SetActive(false);
            _clientBulletMono.ServerSpeed = speed;

            var velocity = rotation * Vector2.up * speed;
            _serverBulletMono.Rigidbody.velocity = velocity;
            _clientBulletMono.ServerVelocity = velocity;
        }

        public void ServerCollisionCallback(ShootTargetMono collidedTarget)
        {
            _serverCollidedTarget = collidedTarget;
            _serverBulletMono.Rigidbody.velocity = Vector2.zero;

            if (!(_serverCollidedTarget is null))
            {
                _serverCollidedTarget.OnServerDeathDataReached();
            }
        }

        public void OnClientDataReached()
        {
            if (!(_serverCollidedTarget is null))
            {
                _serverCollidedTarget.OnClientDeathDataReached();
                _serverCollidedTarget = null;
            }
        }


        private void Awake()
        {
            _serverBulletTransform = _serverBulletMono.transform;
            _clientBulletTransform = _clientBulletMono.transform;
            _serverBulletMono.RootWrapper = this;
        }

        private void Update()
        {
            if (SimulationSettingsMono is null || SimulationSettingsMono.ShowServerBullets == ServerBulletMono.IsVisible)
            {
                return;
            }
            ServerBulletMono.IsVisible = SimulationSettingsMono.ShowServerBullets;
        }

        private void OnDestroy()
        {
            ServerBulletMono.RootWrapper = null;
        }
    }
}