using UnityEngine;

namespace Gameplay.ClientServerWrappers
{
    public class BulletPositionSnapshot
    {
        public ulong FrameIndex;
        public Vector2 Position;
        public bool IsFinishedPosition;
    }
}