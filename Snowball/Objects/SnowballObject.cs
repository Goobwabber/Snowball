using Snowball.Managers;
using Snowball.Networking;
using System;
using System.Linq;
using UnityEngine;
using VRUIControls;
using Zenject;

namespace Snowball.Objects
{
    public class SnowballObject : MonoBehaviour
    {
        private const float MaxLaserDistance = 3;
        private const float MinYPosition = -0.5f;
        private const float MaxDistance = 70f;

        public Guid id { get; internal set; }
        public bool IsGrabbed = false;

        private Rigidbody rigidbody = null!;

        private VRPointer _vrPointer = null!;
        private VRController _grabbingController = null!;
        private Vector3 _grabPos;
        private Quaternion _grabRot;
        private FirstPersonFlyingController _fpfc = null!;
        private IMultiplayerSessionManager _sessionManager = null!;
        private SnowballManager _snowballManager = null!;

        private bool IsFpfc => _fpfc != null && _fpfc.enabled;

        [Inject]
        internal void Construct(
            IMultiplayerSessionManager sessionManager,
            SnowballManager snowballManager)
        {
            _sessionManager = sessionManager;
            _snowballManager = snowballManager;
            _sessionManager.playerDisconnectedEvent += HandlePlayerDisconnected;
            _snowballManager.SnowballGrabReceived += HandleSnowballGrab;
            _snowballManager.SnowballReleaseReceived += HandleSnowballRelease;
            transform.localScale = Vector3.one * 0.25f;
            rigidbody = gameObject.AddComponent<Rigidbody>();
            _fpfc = Resources.FindObjectsOfTypeAll<FirstPersonFlyingController>().FirstOrDefault();
            _vrPointer = Resources.FindObjectsOfTypeAll<VRPointer>().FirstOrDefault();
            rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        }

        public void Destroy()
        {
            _sessionManager.playerDisconnectedEvent -= HandlePlayerDisconnected;
            _snowballManager.SnowballGrabReceived -= HandleSnowballGrab;
            _snowballManager.SnowballReleaseReceived -= HandleSnowballRelease;
        }

        private void HandleSnowballGrab(SnowballGrabPacket packet, IConnectedPlayer player)
        {
            if (packet.id != id)
                return;
            var avatar = _snowballManager.GetPlayerAvatar(player);
            if (avatar.transform != transform.parent)
                transform.SetParent(avatar.transform);
            rigidbody.useGravity = false;
            IsGrabbed = true;
            transform.SetLocalPositionAndRotation(packet.position, packet.rotation);
        }

        private void HandleSnowballRelease(SnowballReleasePacket packet, IConnectedPlayer player)
        {
            if (packet.id != id)
                return;
            rigidbody.useGravity = true;
            IsGrabbed = false;
            transform.SetLocalPositionAndRotation(packet.position, packet.rotation);
            rigidbody.velocity = packet.velocity;
            rigidbody.angularVelocity = packet.angular;
        }

        private void HandlePlayerDisconnected(IConnectedPlayer player)
        {
            if (gameObject.activeInHierarchy)
                return;
            _snowballManager.RemoveSnowball(id);
            Destroy(gameObject);
        }

        protected void Update()
        {
            if (_vrPointer?.vrController != null && (_vrPointer.vrController.triggerValue > 0.9f || Input.GetMouseButtonDown(0)))
            {
                if (IsGrabbed)
                    return;
                if (_grabbingController != null)
                    return;
                if (Physics.Raycast(_vrPointer.vrController.position, _vrPointer.vrController.forward, out RaycastHit hit, MaxLaserDistance))
                {
                    if (hit.transform != transform && !hit.transform.IsChildOf(transform))
                        return;
                    _grabbingController = _vrPointer.vrController;
                    _grabPos = _vrPointer.vrController.transform.InverseTransformPoint(transform.position);
                    _grabRot = Quaternion.Inverse(_vrPointer.vrController.transform.rotation) * transform.rotation;
                    IsGrabbed = true;
                    rigidbody.useGravity = false;
                }
            }

            if (_grabbingController == null || !IsFpfc && _grabbingController.triggerValue > 0.9f || IsFpfc && Input.GetMouseButton(0))
                return;

            //grab end
            var pos = _grabbingController.transform.TransformPoint(_grabPos);
            var rot = _grabbingController.transform.rotation * _grabRot;
            rigidbody.velocity = (pos - transform.position) / Time.deltaTime;
            rigidbody.angularVelocity = (rot * Quaternion.Inverse(transform.rotation)).eulerAngles / Time.deltaTime;
            transform.position = pos;
            transform.rotation = rot;

            _grabbingController = null!;
            IsGrabbed = false;
            rigidbody.useGravity = true;
            _sessionManager.Send(new SnowballReleasePacket
            {
                id = id,
                position = transform.position,
                rotation = transform.rotation,
                velocity = rigidbody.velocity,
                angular = rigidbody.angularVelocity
            });
        }

        protected void LateUpdate()
        {
            if (transform.position.y < MinYPosition || transform.position.magnitude >= MaxDistance)
            {
                _snowballManager.RemoveSnowball(id);
                Destroy(gameObject);
            }

            if (_grabbingController == null)
                return;

            var pos = _grabbingController.transform.TransformPoint(_grabPos);
            var rot = _grabbingController.transform.rotation * _grabRot;
            transform.position = _grabbingController.transform.TransformPoint(_grabPos);
            transform.rotation = _grabbingController.transform.rotation * _grabRot;
        }

        protected void FixedUpdate()
        {
            if (_grabbingController != null)
                _sessionManager.Send(new SnowballGrabPacket
                {
                    id = id,
                    position = transform.position,
                    rotation = transform.rotation
                });
        }

        internal void SetGrabbed(VRController grabbingController, Vector3 grabPos, Quaternion grabRot)
        {
            _grabbingController = grabbingController;
            _grabPos = grabPos;
            _grabRot = grabRot;
            IsGrabbed = true;
            rigidbody.useGravity = false;
        }
    }
}
