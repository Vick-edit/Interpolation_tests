using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PredictionTest;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay
{
    public sealed class ShootTargetPoolMono : MonoBehaviour
    {
        private Stack<ShootTargetMono> _targetsPool = new Stack<ShootTargetMono>(0);
        private System.Random _random = new System.Random();
        private int _activeTargets;
        private bool _isOnSpawning;

        private Transform _rootForTargets;

        [SerializeField] private SimulationSettingsMono _simulationSettings;
        [SerializeField] private ShootTargetMono[] _targetPrefabs;
        [SerializeField] private Transform UpLeftCornerSpawnZone;
        [SerializeField] private Transform DownRightCornerSpawnZone;
        

        public void ReturnToPool(ShootTargetMono shootTargetMono)
        {
            _targetsPool.Push(shootTargetMono);
            _activeTargets--;
        }

        public void TargetWasDestroyed()
        {
            _activeTargets--;
        }


        private void Start()
        {
            _rootForTargets = gameObject.transform;
        }

        private void Update()
        {
            if (_isOnSpawning || _activeTargets >= _simulationSettings.MaxLiveTargets)
            {
                return;
            }

            _activeTargets++;
            StartCoroutine(EmitNewTargetWithDelay());
        }
        
        private IEnumerator EmitNewTargetWithDelay()
        {
            _isOnSpawning = true;
            yield return new WaitForSeconds(_simulationSettings.SpawnTargetsDelay);

            var spawnX = Random.Range(UpLeftCornerSpawnZone.position.x, DownRightCornerSpawnZone.position.x);
            var spawnY = Random.Range(UpLeftCornerSpawnZone.position.y, DownRightCornerSpawnZone.position.y);
            var spawnPosition = new Vector2(spawnX, spawnY);
            if (_targetsPool.Any())
            {
                var target = _targetsPool.Pop();
                target.transform.position = spawnPosition;
                target.gameObject.SetActive(true);
            }
            else
            {
                var prefabId = _random.Next(_targetPrefabs.Length);
                var prefab = _targetPrefabs[prefabId];
                var newInstance = Instantiate(prefab, spawnPosition, Quaternion.identity, _rootForTargets);
                newInstance.RootPool = this;
            }

            _isOnSpawning = false;
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
            foreach (var targetMono in _targetsPool)
            {
                Destroy(targetMono.gameObject);
            }
        }
    }
}