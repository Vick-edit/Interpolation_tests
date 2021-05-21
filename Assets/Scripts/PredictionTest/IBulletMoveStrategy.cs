using Gameplay;
using Gameplay.ClientEntities;

namespace PredictionTest
{
    internal interface IBulletMoveStrategy
    {
        PredictionStrategy PredictionStrategy { get; }
        SimulationSettingsMono SimulationSettings { get; set; }

        void ClientCall(ClientBulletMono clientBullet);
        void UpdateCall(ClientBulletMono clientBullet, float deltaTime);
    }
}