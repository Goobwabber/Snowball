using Snowball.Networking;
using System.Linq;
using UnityEngine;
using VRUIControls;
using Zenject;

namespace Snowball.Objects
{
    public class LocalSnowball : MonoBehaviour
    {
        private const float MinScrollDistance = 0.25f;
        private const float MaxLaserDistance = 3;
        private const float MinYPosition = -0.5f;
        private const float MaxDistance = 70f;

        private Rigidbody rigidbody = null!;

        private VRPointer _vrPointer = null!;
        private VRController _grabbingController = null!;
        private Vector3 _grabPos;
        private Quaternion _grabRot;
        private Vector3 _realPos;
        private Quaternion _realRot;
        private FirstPersonFlyingController _fpfc = null!;
        private IMultiplayerSessionManager _sessionManager = null!;
        private Config _config = null!;

        private Vector3 SpawnLocation => new Vector3(_config.SpawnX, _config.SpawnY, _config.SpawnZ);
        private bool IsFpfc => _fpfc != null && _fpfc.enabled;

        [Inject]
        internal void Construct(
            IMultiplayerSessionManager sessionManager,
            Config config)
        {
            _sessionManager = sessionManager;
            _config = config;
        }

        public void Start()
        {
            transform.position = SpawnLocation;
            transform.localScale = Vector3.one * 0.25f;
            rigidbody = gameObject.AddComponent<Rigidbody>();
            rigidbody.isKinematic = true;
            rigidbody.detectCollisions = true;

            _realPos = transform.position;
            _realRot = transform.rotation;
            _fpfc = Resources.FindObjectsOfTypeAll<FirstPersonFlyingController>().FirstOrDefault();
            _vrPointer = Resources.FindObjectsOfTypeAll<VRPointer>().FirstOrDefault();
            _sessionManager.playerConnectedEvent += HandlePlayerConnected;
        }

        private void HandlePlayerConnected(IConnectedPlayer obj) 
            => SendSnowballPacket();

        protected void Update()
        {
            if (_vrPointer?.vrController != null && (_vrPointer.vrController.triggerValue > 0.9f || Input.GetMouseButtonDown(0)))
            {
                if (_grabbingController != null) 
                    return;
                if (Physics.Raycast(_vrPointer.vrController.position, _vrPointer.vrController.forward, out RaycastHit hit, MaxLaserDistance))
                {
                    if (hit.transform != transform && !hit.transform.IsChildOf(transform))
                        return;
                    _grabbingController = _vrPointer.vrController;
                    _grabPos = _vrPointer.vrController.transform.InverseTransformPoint(transform.position);
                    _grabRot = Quaternion.Inverse(_vrPointer.vrController.transform.rotation) * transform.rotation;
                    Grabbed();
                }
            }

            if (_grabbingController == null || !IsFpfc && _grabbingController.triggerValue > 0.9f || IsFpfc && Input.GetMouseButton(0))
                return;
            _grabbingController = null!;
            Released();
        }

        protected void LateUpdate()
        {
            if (transform.position.y < MinYPosition || transform.position.magnitude >= MaxDistance)
            {
                rigidbody.velocity = Vector3.zero;
                rigidbody.angularDrag = 0;
                rigidbody.angularVelocity = Vector3.zero;
                transform.position = SpawnLocation;
                transform.rotation = Quaternion.identity;
                rigidbody.isKinematic = true;
                _grabbingController = null!;
                SendSnowballPacket();
            }

            if (_grabbingController != null)
            {
                float diff = _grabbingController.verticalAxisValue * Time.unscaledDeltaTime;
                if (_grabPos.magnitude > MinScrollDistance)
                {
                    _grabPos -= Vector3.forward * diff;
                }
                else
                {
                    _grabPos -= Vector3.forward * Mathf.Clamp(diff, float.MinValue, 0);
                }
                _realPos = _grabbingController.transform.TransformPoint(_grabPos);
                _realRot = _grabbingController.transform.rotation * _grabRot;
            }
            else return;

            rigidbody.AddForce((_realPos - transform.position) * 3 / Time.deltaTime, ForceMode.Acceleration);
            transform.position = _realPos;
            rigidbody.AddTorque((_realRot * Quaternion.Inverse(transform.rotation)).eulerAngles / Time.deltaTime, ForceMode.Acceleration);
            transform.rotation = _realRot;
        }

        protected void FixedUpdate()
        {
            if (!rigidbody.isKinematic)
                SendSnowballPacket();
        }

        protected void Grabbed()
        {
            rigidbody.useGravity = false;
            rigidbody.isKinematic = false;
        }

        protected void Released()
        {
            rigidbody.useGravity = true;
        }

        private void SendSnowballPacket()
            => _sessionManager.Send(new SnowballPacket
            {
                position = transform.position,
                rotation = transform.rotation
            });
    }
}
