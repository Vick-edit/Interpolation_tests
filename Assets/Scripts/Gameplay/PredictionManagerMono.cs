using System;
using System.Collections.Generic;
using System.Linq;
using PredictionTest;
using UnityEngine;
using Random = System.Random;

namespace Gameplay
{

    public sealed class PredictionManagerMono : MonoBehaviour
    {
        private readonly Random _random = new Random();
        private readonly Queue<float> _clientSendDataDelays = new Queue<float>(0);
        private readonly Dictionary<PredictionStrategy, IBulletMoveStrategy> _bulletMoveStrategies = new Dictionary<PredictionStrategy, IBulletMoveStrategy>(0);
        private float _tickTimePassed;

        private ulong _currentFrameIndex;
        private ulong _transferFrameIndex;

        [SerializeField] private SimulationSettingsMono _simulationSettings;
        [SerializeField] private BulletsPoolMono _bulletsPoolMono;


        private void Awake()
        {
            var strategyInterface = typeof(IBulletMoveStrategy);
            var strategyImplementationTypes = strategyInterface
                .Assembly
                .GetTypes()
                .Where(p => strategyInterface.IsAssignableFrom(p))
                .Where(p => !p.IsAbstract && !p.IsInterface)
                .Where(p => p.GetConstructor(Type.EmptyTypes) != null);

            foreach (var implementationType in strategyImplementationTypes)
            {
                var strategyInterfaceImplementation = (IBulletMoveStrategy) Activator.CreateInstance(implementationType);
                if (_bulletMoveStrategies.ContainsKey(strategyInterfaceImplementation.PredictionStrategy))
                {
                    Debug.LogError($"There are two implementation of strategy {strategyInterfaceImplementation.PredictionStrategy}");
                }
                else
                {
                    strategyInterfaceImplementation.SimulationSettings = _simulationSettings;
                    _bulletMoveStrategies[strategyInterfaceImplementation.PredictionStrategy] = strategyInterfaceImplementation;
                }
            }

            var allStrategies = (PredictionStrategy[])Enum.GetValues(typeof(PredictionStrategy));
            foreach (var predictionStrategy in allStrategies)
            {
                if (!_bulletMoveStrategies.ContainsKey(predictionStrategy))
                {
                    Debug.LogError($"There are no any implementation of strategy {predictionStrategy}");
                }
            }
        }

        private void FixedUpdate()
        {
            var gameTime = Time.realtimeSinceStartup;
            var deltaTime = Time.fixedDeltaTime;

            _tickTimePassed += deltaTime;
            var tickTimeFromSettings = 1f / _simulationSettings.ServerTicksPerSeconds;
            var isTickPassed = _tickTimePassed >= tickTimeFromSettings;

            //Reset tracking data
            if (isTickPassed)
            {
                _currentFrameIndex++;
                _tickTimePassed = 0;
            }
            
            //Server tick logic
            if (isTickPassed)
            {
                //HandleUserInputs
                _bulletsPoolMono.ProcessRequestedEmits();

                //WriteServerPositions
                foreach (var bulletWrapperMono in _bulletsPoolMono.ActiveBullets)
                {
                    var serverBullet = bulletWrapperMono.ServerBulletMono;
                    var positionSnapshot = _bulletsPoolMono.GetNewSnapshot(_currentFrameIndex, serverBullet.Rigidbody.position, serverBullet.IsDead);
                    serverBullet.ServerPositionSnapshots.Enqueue(positionSnapshot);
                }

                //Save time when server data should reach client
                var ping = _random.Next(_simulationSettings.MinPingMs, _simulationSettings.MaxPingMs) / 1000f;
                _clientSendDataDelays.Enqueue(gameTime + ping);
            }

            //Check if ping passed
            if (_clientSendDataDelays.Count > 0)
            {
                var nextDataSendTime = _clientSendDataDelays.First();
                while (nextDataSendTime <= gameTime || Mathf.Approximately(nextDataSendTime, gameTime))
                {
                    //Transfer data to client bullets if exists
                    _transferFrameIndex++;
                    foreach (var bulletWrapperMono in _bulletsPoolMono.ActiveBullets)
                    {
                        if (bulletWrapperMono.ServerBulletMono.ServerPositionSnapshots.Count <= 0) continue;
                        var positionToTransfer = bulletWrapperMono.ServerBulletMono.ServerPositionSnapshots.First();
                        while (positionToTransfer.FrameIndex <= _transferFrameIndex)
                        {
                            positionToTransfer = bulletWrapperMono.ServerBulletMono.ServerPositionSnapshots.Dequeue();
                            bulletWrapperMono.ClientBulletMono.TargetPositions.Enqueue(positionToTransfer);
                            if (positionToTransfer.IsFinishedPosition)
                            {
                                bulletWrapperMono.OnClientDataReached();
                            }

                            //Check next server data exists
                            if (bulletWrapperMono.ServerBulletMono.ServerPositionSnapshots.Count <= 0) break;
                            positionToTransfer = bulletWrapperMono.ServerBulletMono.ServerPositionSnapshots.First();
                        }
                    }

                    //Check next delay if exists
                    _clientSendDataDelays.Dequeue();
                    if (_clientSendDataDelays.Count <= 0) break;
                    nextDataSendTime = _clientSendDataDelays.First();
                }
            }
            
            //Perform client tick
            if (isTickPassed)
            {
                //Use strategy call
                var strategy = _bulletMoveStrategies[_simulationSettings.PredictionStrategy];
                _bulletsPoolMono
                    .ActiveBullets
                    .ForEach(wb => strategy.ClientCall(wb.ClientBulletMono));

                //Clear active bullets
                for (var i = _bulletsPoolMono.ActiveBullets.Count - 1; i >= 0; i--)
                {
                    var bulletWrapperMono = _bulletsPoolMono.ActiveBullets[i];
                    if (bulletWrapperMono.ServerBulletMono.IsDead && bulletWrapperMono.ClientBulletMono.IsDead)
                    {
                        _bulletsPoolMono.ReturnToPool(bulletWrapperMono);
                    }
                }
            }
        }

        private void Update()
        {
            var deltaTime = Time.deltaTime;
            var strategy = _bulletMoveStrategies[_simulationSettings.PredictionStrategy];
            _bulletsPoolMono
                .ActiveBullets
                .ForEach(wb => strategy.UpdateCall(wb.ClientBulletMono, deltaTime));
        }
    }
}