using System.Collections.Generic;
using Gameplay.ClientServerWrappers;
using UnityEngine;

namespace Gameplay.ClientEntities
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class ClientBulletMono : MonoBehaviour
    {
        public Transform ObjectTransform { get; private set; }

        public Queue<BulletPositionSnapshot> TargetPositions { get; private set; }
        public Queue<BulletPositionSnapshot> PassedPositions { get; private set; }
        public BulletPositionSnapshot CurrentPosition { get; set; }

        public bool IsLaunched { get;  set; }
        public bool IsReachedServerDeathPoint { get;  set; }
        public bool IsDead { get; set; }
        public Vector2 ServerVelocity { get; set; }
        public float ServerSpeed { get; set; }
        public float ClientSpeed { get; set; }
        public int TickSeanceFirstPointReached { get; set; }


        private void Awake()
        {
            ObjectTransform = gameObject.transform;
            TargetPositions = new Queue<BulletPositionSnapshot>(0);
            PassedPositions = new Queue<BulletPositionSnapshot>(0);
        }

        public void ResetAllDataAndStates()
        {
            IsLaunched = false;
            IsReachedServerDeathPoint = false;
            IsDead = false;
            ServerSpeed = float.NaN;
            ClientSpeed = float.NaN;
            TickSeanceFirstPointReached = 0;
            gameObject.SetActive(false);
        }
    }
}