using System.Collections.Generic;
using System.Linq;
using Gameplay.ClientServerWrappers;
using UnityEngine;

namespace Gameplay
{
    public sealed class BulletsPoolMono : MonoBehaviour
    {
        private readonly Stack<BulletWrapperMono> _wrappedBulletsPool = new Stack<BulletWrapperMono>(0);
        private readonly Stack<BulletPositionSnapshot> _freeSnapshots = new Stack<BulletPositionSnapshot>(0);
        private readonly Stack<EmitRequest> _freeEmitRequests = new Stack<EmitRequest>(0);
        private readonly Queue<EmitRequest> _currentEmitRequests = new Queue<EmitRequest>(0);
        private Transform _rootForTargets;

        [SerializeField] private SimulationSettingsMono _simulationSettings;
        [SerializeField] private BulletWrapperMono _targetPrefab;

        public List<BulletWrapperMono> ActiveBullets { get; private set; }


        public void RequestEmitBullet(Vector2 spawnPosition, Quaternion rotation)
        {
            if (_freeEmitRequests.Count > 0)
            {
                var cachedRequest = _freeEmitRequests.Pop();
                cachedRequest.SpawnPosition = spawnPosition;
                cachedRequest.Rotation = rotation;
                _currentEmitRequests.Enqueue(cachedRequest);
            }
            else
            {
                var newRequest = new EmitRequest()
                {
                    SpawnPosition = spawnPosition,
                    Rotation = rotation,
                };
                _currentEmitRequests.Enqueue(newRequest);
            }
        }

        public void ProcessRequestedEmits()
        {
            while (_currentEmitRequests.Count > 0)
            {
                var currentEmitRequest = _currentEmitRequests.Dequeue();
                BulletWrapperMono emittedServerBullet;
                if (_wrappedBulletsPool.Any())
                {
                    emittedServerBullet = _wrappedBulletsPool.Pop();
                }
                else
                {
                    emittedServerBullet = Instantiate(_targetPrefab, currentEmitRequest.SpawnPosition, currentEmitRequest.Rotation, _rootForTargets);
                    emittedServerBullet.SimulationSettingsMono = _simulationSettings;
                }

                emittedServerBullet.Launch(currentEmitRequest.SpawnPosition, currentEmitRequest.Rotation, _simulationSettings.BulletSpeed);
                ActiveBullets.Add(emittedServerBullet);
                _freeEmitRequests.Push(currentEmitRequest);
            }
        }

        public BulletPositionSnapshot GetNewSnapshot(ulong frameIndex, Vector2 position, bool isLiveTimeEndPosition = false)
        {
            BulletPositionSnapshot snapshot;
            if (_freeSnapshots.Count > 0)
            {
                snapshot = _freeSnapshots.Pop();
            }
            else
            {
                snapshot = new BulletPositionSnapshot();
            }

            snapshot.FrameIndex = frameIndex;
            snapshot.Position = position;
            snapshot.IsFinishedPosition = isLiveTimeEndPosition;
            return snapshot;
        }
        
        public void ReturnToPool(BulletWrapperMono wrappetBulletMono)
        {
            ActiveBullets.Remove(wrappetBulletMono);
            while (wrappetBulletMono.ServerBulletMono.ServerPositionSnapshots.Count > 0)
            {
                var bulletPositionSnapshot = wrappetBulletMono.ServerBulletMono.ServerPositionSnapshots.Dequeue();
                bulletPositionSnapshot.IsFinishedPosition = false;
                _freeSnapshots.Push(bulletPositionSnapshot);
            }
            while (wrappetBulletMono.ClientBulletMono.TargetPositions.Count > 0)
            {
                var bulletPositionSnapshot = wrappetBulletMono.ClientBulletMono.TargetPositions.Dequeue();
                bulletPositionSnapshot.IsFinishedPosition = false;
                _freeSnapshots.Push(bulletPositionSnapshot);
            }
            while (wrappetBulletMono.ClientBulletMono.PassedPositions.Count > 0)
            {
                var bulletPositionSnapshot = wrappetBulletMono.ClientBulletMono.PassedPositions.Dequeue();
                bulletPositionSnapshot.IsFinishedPosition = false;
                _freeSnapshots.Push(bulletPositionSnapshot);
            }
            if (wrappetBulletMono.ClientBulletMono.CurrentPosition != null)
            {
                var positionToCache = wrappetBulletMono.ClientBulletMono.CurrentPosition;
                wrappetBulletMono.ClientBulletMono.CurrentPosition = null;
                _freeSnapshots.Push(positionToCache);
            }
            wrappetBulletMono.ClientBulletMono.ResetAllDataAndStates();
            _wrappedBulletsPool.Push(wrappetBulletMono);
        }
        

        private void Start()
        {
            _rootForTargets = gameObject.transform;
            ActiveBullets = new List<BulletWrapperMono>(0);
        }

        private void OnDestroy()
        {
            foreach (var targetMono in _wrappedBulletsPool)
            {
                Destroy(targetMono.gameObject);
            }
        }

        private sealed class EmitRequest
        {
            public Vector2 SpawnPosition;
            public Quaternion Rotation;
        }
    }
}