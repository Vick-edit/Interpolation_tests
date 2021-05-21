using System.Linq;
using Gameplay;
using Gameplay.ClientEntities;
using UnityEngine;

namespace PredictionTest
{
    public sealed class StrategyInterpolationOnlyWithSkip : IBulletMoveStrategy
    {
        private const float TeleportIfDelayMoreThanTickPercent = 0.2f;

        private float TickDuration => 1f / SimulationSettings.ServerTicksPerSeconds;
        public PredictionStrategy PredictionStrategy => PredictionStrategy.InterpolationOnlyWithSkip;
        public SimulationSettingsMono SimulationSettings { get; set; }


        public void ClientCall(ClientBulletMono clientBullet)
        {
            //check if only start of movement
            if (clientBullet.CurrentPosition == null)
            {
                //waiting first two point to interpolate 
                if (clientBullet.TargetPositions.Count < 2)
                {
                    return;
                }

                //find first two point to interpolate
                while (clientBullet.TargetPositions.Count > 1)
                {
                    ExtractNewCurrentPosition(clientBullet);
                }

                //start interpolation
                var targetPoint = clientBullet.TargetPositions.First().Position;
                var currentPoint = clientBullet.CurrentPosition.Position;
                clientBullet.ClientSpeed = (targetPoint - currentPoint).magnitude / TickDuration;
            }

            //update current and target
            while (clientBullet.TargetPositions.Count > 1)
            {
                ExtractNewCurrentPosition(clientBullet);
            }
        }

        public void UpdateCall(ClientBulletMono clientBullet, float deltaTime)
        {
            //no need visual update
            if (clientBullet.CurrentPosition == null || clientBullet.TargetPositions.Count == 0 || clientBullet.IsDead)
            {
                return;
            }

            //on interpolation only start
            if (!clientBullet.IsLaunched)
            {
                clientBullet.IsLaunched = true;
                clientBullet.gameObject.SetActive(true);
                clientBullet.ObjectTransform.position = clientBullet.CurrentPosition.Position;
                return;
            }

            var startInterpolationPoint = clientBullet.CurrentPosition.Position;
            var targetInterpolationPoint = clientBullet.TargetPositions.First().Position;
            var currentInterpolationPoint = (Vector2)clientBullet.ObjectTransform.position;

            //check that we do not need teleport
            var toTargetSqrDistance = (targetInterpolationPoint - currentInterpolationPoint).sqrMagnitude;
            var interpolationSqrDistance = (targetInterpolationPoint - startInterpolationPoint).sqrMagnitude;
            if (toTargetSqrDistance > interpolationSqrDistance)
            {
                var allowedDelay = TickDuration * TeleportIfDelayMoreThanTickPercent * clientBullet.ServerSpeed;
                var toStartSqrDistance = (startInterpolationPoint - currentInterpolationPoint).sqrMagnitude;
                if (toStartSqrDistance > allowedDelay * allowedDelay)
                {
                    clientBullet.ObjectTransform.position = startInterpolationPoint;
                    return;
                }
            }

            //check that we don't reach target
            var currentMovementMagnitude = deltaTime * clientBullet.ServerSpeed;
            if (toTargetSqrDistance < currentMovementMagnitude * currentMovementMagnitude)
            {
                //if no other target put bullet on last known target
                clientBullet.ObjectTransform.position = targetInterpolationPoint;
                //if it was last traget kill client bullet
                if (clientBullet.IsReachedServerDeathPoint)
                {
                    clientBullet.gameObject.SetActive(false);
                    clientBullet.IsDead = true;
                }
                return;
            }

            //move bullet to target
            var movementVector = (targetInterpolationPoint - currentInterpolationPoint).normalized * currentMovementMagnitude;
            clientBullet.ObjectTransform.position = currentInterpolationPoint + movementVector;
        }


        private static void ExtractNewCurrentPosition(ClientBulletMono clientBullet)
        {
            if (clientBullet.CurrentPosition != null)
            {
                clientBullet.PassedPositions.Enqueue(clientBullet.CurrentPosition);
            }

            clientBullet.CurrentPosition = clientBullet.TargetPositions.Dequeue();
            if (clientBullet.CurrentPosition.IsFinishedPosition)
            {
                clientBullet.IsReachedServerDeathPoint = true;
            }
        }
    }
}