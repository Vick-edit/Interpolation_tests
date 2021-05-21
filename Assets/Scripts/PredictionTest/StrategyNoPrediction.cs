using Gameplay;
using Gameplay.ClientEntities;

namespace PredictionTest
{
    internal class StrategyNoPrediction : IBulletMoveStrategy
    {
        public PredictionStrategy PredictionStrategy => PredictionStrategy.NoPrediction;
        public SimulationSettingsMono SimulationSettings { get; set; }


        public void ClientCall(ClientBulletMono clientBullet)
        {
            while (clientBullet.TargetPositions.Count > 0)
            {
                var newCurrentPosition = clientBullet.TargetPositions.Dequeue();
                if (clientBullet.CurrentPosition != null)
                {
                    clientBullet.PassedPositions.Enqueue(clientBullet.CurrentPosition);
                }
                clientBullet.CurrentPosition = newCurrentPosition;
                clientBullet.IsReachedServerDeathPoint = clientBullet.CurrentPosition.IsFinishedPosition;
            }
        }

        public void UpdateCall(ClientBulletMono clientBullet, float deltaTime)
        {
            if (clientBullet.CurrentPosition == null || clientBullet.IsDead)
            {
                return;
            }

            if (!clientBullet.IsLaunched)
            {
                clientBullet.IsLaunched = true;
                clientBullet.gameObject.SetActive(true);
            }

            clientBullet.ObjectTransform.position = clientBullet.CurrentPosition.Position;
            if (clientBullet.IsReachedServerDeathPoint)
            {
                clientBullet.gameObject.SetActive(false);
                clientBullet.IsDead = true;
            }
        }
    }
}