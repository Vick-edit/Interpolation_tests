using UnityEngine;

namespace Gameplay
{
    public sealed class PlayerMono : MonoBehaviour
    {
        private const float RotationMinAngel = 1f;
        private const float MinInputSqrMagnitude = 0.0001f;
        private float _timeFormLastShoot;
        private Transform _transform;
        private Camera _mainCamera;


        [SerializeField] private SimulationSettingsMono _simulationSettings;
        [SerializeField] private Transform _muzzlePoint;
        [SerializeField] private Rigidbody2D _body;
        [SerializeField] private ParticleSystem _effect;
        [SerializeField] private BulletsPoolMono _bulletsPool;

        
        private void Start()
        {
            _transform = transform;
            _mainCamera = Camera.main;
        }


        private void Update()
        {
            var horizontalMove = Input.GetAxis(AppConstant.HorizontalAxis);
            var verticalMove = Input.GetAxis(AppConstant.VerticalAxis);
            var inputVector = new Vector3(horizontalMove, verticalMove);
            var aimingVector = _mainCamera.ScreenToWorldPoint(Input.mousePosition) - _transform.position;

            var velocity = Vector2.zero;
            if (inputVector.sqrMagnitude > MinInputSqrMagnitude)
            {
                velocity = inputVector * _simulationSettings.PlayerSpeed;
            }
            if (!Mathf.Approximately(aimingVector.magnitude, 0))
            {
                var targetDirection = aimingVector.normalized;
                var currentDirection = _transform.localRotation * Vector2.up;
                var angel = Vector2.SignedAngle(currentDirection, targetDirection);
                var lerpValue = Mathf.Clamp01(Time.deltaTime * _simulationSettings.PlayerRotationSpeed);
                _transform.Rotate(Vector3.forward, angel * lerpValue);
            }
            _body.velocity = velocity;

            _timeFormLastShoot += Time.deltaTime;
            if ((Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0)) && _timeFormLastShoot > _simulationSettings.PlayerShootDelay / 1000f)
            {
                _timeFormLastShoot = 0;
                _bulletsPool.RequestEmitBullet(_muzzlePoint.position, _transform.rotation);
                _effect.Play(true);
            }
        }
    }
}