using System.Linq;
using Gameplay;
using Gameplay.ClientEntities;
using UnityEngine;

namespace PredictionTest
{
    public sealed class StrategyCombineInterExtra : IBulletMoveStrategy
    {
        private float TickDuration => 1f / SimulationSettings.ServerTicksPerSeconds;

        public PredictionStrategy PredictionStrategy => PredictionStrategy.CombineInterpolationAndExtrapolation;
        public SimulationSettingsMono SimulationSettings { get; set; }


        public void ClientCall(ClientBulletMono clientBullet)
        {
            //waiting first point to interpolate 
            if (clientBullet.CurrentPosition == null)
            {
                if (clientBullet.TargetPositions.Count < 1)
                {
                    return;
                }
                ExtractNewCurrentPosition(clientBullet);

                //if we don't take several frame at once retern other go to logic of launch
                if (clientBullet.TargetPositions.Count == 0)
                {
                    return;
                }
            }

            //waiting second point to interpolate or start extropolate
            if (!clientBullet.IsLaunched)
            {
                clientBullet.IsLaunched = true;
                clientBullet.ObjectTransform.position = clientBullet.CurrentPosition.Position;
                clientBullet.gameObject.SetActive(true);

                //if we don't take several frame at once retern other go to pop last data
                if (clientBullet.TargetPositions.Count > 1)
                {
                    return;
                }
            }

            //pop all data to have one current point and one target
            while (clientBullet.TargetPositions.Count > 1)
            {
                ExtractNewCurrentPosition(clientBullet);
            }
        }

        public void UpdateCall(ClientBulletMono clientBullet, float deltaTime)
        {
            //no need visual update if bullet already or still dead
            if (!clientBullet.IsLaunched || clientBullet.IsDead)
            {
                return;
            }

            if (clientBullet.TargetPositions.Count >= 1)
            {
                Interpolate(clientBullet, deltaTime);
            }
            else
            {
                Extrapolate(clientBullet, deltaTime);
            }
        }


        private void Interpolate(ClientBulletMono clientBullet, float deltaTime)
        {
            var targetInterpolationPoint = clientBullet.TargetPositions.First().Position;
            var currentInterpolationPoint = (Vector2)clientBullet.ObjectTransform.position;

            //check that we don't reach target
            var toTargetSqrDistance = (targetInterpolationPoint - currentInterpolationPoint).sqrMagnitude;
            var currentMovementMagnitude = clientBullet.ServerSpeed * deltaTime;
            if (toTargetSqrDistance < currentMovementMagnitude * currentMovementMagnitude)
            {
                //pop target as current location
                ExtractNewCurrentPosition(clientBullet);
                //if it was last traget kill client bullet
                if (clientBullet.IsReachedServerDeathPoint)
                {
                    clientBullet.ObjectTransform.position = targetInterpolationPoint;
                    clientBullet.gameObject.SetActive(false);
                    clientBullet.IsDead = true;
                }
                else
                {
                    Extrapolate(clientBullet, deltaTime);
                }
                return;
            }

            //move bullet to target
            var movementVector = (targetInterpolationPoint - currentInterpolationPoint).normalized * currentMovementMagnitude;
            clientBullet.ObjectTransform.position = currentInterpolationPoint + movementVector;
        }

        private void Extrapolate(ClientBulletMono clientBullet, float deltaTime)
        {
            var maxExtrapolation = clientBullet.ServerSpeed * TickDuration;
            var sqrMaxExtrapolation = maxExtrapolation * maxExtrapolation;
            var sqrDistanceFromCurrent = (clientBullet.CurrentPosition.Position - (Vector2)clientBullet.ObjectTransform.position).sqrMagnitude;
            if (sqrMaxExtrapolation <= sqrDistanceFromCurrent)
            {
                return;
            }

            var currentMovementVector = (Vector3)clientBullet.ServerVelocity.normalized * (clientBullet.ServerSpeed * deltaTime);
            clientBullet.ObjectTransform.position = clientBullet.ObjectTransform.position + currentMovementVector;
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