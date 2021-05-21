using PredictionTest;
using UnityEngine;

namespace Gameplay
{
    public sealed class SimulationSettingsMono : MonoBehaviour
    {
        [SerializeField, Range(1, 20)] private int _playerSpeed = 10;
        [SerializeField, Range(5, 30)] private int _playerRotationSpeed = 10;
        [SerializeField, Range(100, 5000)] private int _playerShootDelay = 200;

        [SerializeField, Range(0.1f, 25f)] private float _bulletSpeed = 1;

        [SerializeField, Range(1, 50)] private int _maxLiveTargets = 7;
        [SerializeField, Range(0, 5)] private int _spawnTargetsDelay = 1;

        [SerializeField, Range(0, 1000)] private int _maxPingMs = 0;
        [SerializeField, Range(0, 1000)] private int _minPingMs = 0;
        [SerializeField, Range(1, 30)] private int _serverTicksPerSeconds = 10;
        [SerializeField] private bool _showServerBullets = false;
        [SerializeField] private PredictionStrategy _predictionStrategy = PredictionStrategy.NoPrediction;


        public float PlayerSpeed => _playerSpeed;
        public float PlayerRotationSpeed => _playerRotationSpeed;
        public float PlayerShootDelay => _playerShootDelay;

        public float BulletSpeed => _bulletSpeed;

        public float MaxLiveTargets => _maxLiveTargets;
        public float SpawnTargetsDelay => _spawnTargetsDelay;

        public int MinPingMs => _minPingMs;
        public int MaxPingMs => _maxPingMs;
        public int ServerTicksPerSeconds => _serverTicksPerSeconds;
        public bool ShowServerBullets => _showServerBullets;
        public PredictionStrategy PredictionStrategy => _predictionStrategy;


#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_minPingMs > _maxPingMs)
            {
                _maxPingMs = _minPingMs;
            }
        }
#endif
    }
}