using Gameplay;
using Gameplay.ClientEntities;
using UnityEngine;

namespace PredictionTest
{
    public sealed class StrategyExtrapolationOnly : IBulletMoveStrategy
    {
        public PredictionStrategy PredictionStrategy => PredictionStrategy.ExtrapolationOnly;
        public SimulationSettingsMono SimulationSettings { get; set; }


        public void ClientCall(ClientBulletMono clientBullet)
        {
            //check if only start of movement
            if (clientBullet.CurrentPosition == null)
            {
                //waiting first point to extrapolation 
                if (clientBullet.TargetPositions.Count < 1)
                {
                    return;
                }

                //find first point to extrapolation
                ExtractNewCurrentPosition(clientBullet);
                return;
            }

            //check that we alive if start movement
            if (clientBullet.IsLaunched)
            {
                while (clientBullet.TargetPositions.Count > 0)
                {
                    ExtractNewCurrentPosition(clientBullet);
                }
            }
        }

        public void UpdateCall(ClientBulletMono clientBullet, float deltaTime)
        {
            //no need visual update
            if (clientBullet.CurrentPosition == null || clientBullet.IsDead)
            {
                return;
            }

            //on extrapolation only start
            if (!clientBullet.IsLaunched)
            {
                clientBullet.IsLaunched = true;
                clientBullet.gameObject.SetActive(true);
                clientBullet.ObjectTransform.position = clientBullet.CurrentPosition.Position;
                return;
            }

            //on extrapolation finished
            if (clientBullet.IsReachedServerDeathPoint)
            {
                clientBullet.gameObject.SetActive(false);
                clientBullet.IsDead = true;
                return;
            }

            //on extrapolation
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