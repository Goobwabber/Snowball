using Snowball.Managers;
using System;
using System.Linq;
using UnityEngine;
using VRUIControls;
using Zenject;

namespace Snowball.Objects
{
    public class SnowballSpawner : MonoBehaviour
    {
        private const float MaxLaserDistance = 10;

        private bool IsGrabbing;

        private VRPointer _vrPointer = null!;
        private FirstPersonFlyingController _fpfc = null!;
        private SnowballManager _snowballManager = null!;
        private Config _config = null!;

        [Inject]
        internal void Construct(
            SnowballManager snowballManager,
            Config config)
        {
            _snowballManager = snowballManager;
            _config = config;
        }

        public void Start()
        {
            transform.localScale = Vector3.one * 0.25f;
            transform.position = new(_config.SpawnX, _config.SpawnY, _config.SpawnZ);
            _fpfc = Resources.FindObjectsOfTypeAll<FirstPersonFlyingController>().FirstOrDefault();
            _vrPointer = Resources.FindObjectsOfTypeAll<VRPointer>().FirstOrDefault();
        }

        protected void Update()
        {
            if (_vrPointer?.vrController != null) {
                if (!IsGrabbing && (_vrPointer.vrController.triggerValue > 0.9f || Input.GetMouseButtonDown(0)))
                {
                    IsGrabbing = true;
                    if (Physics.Raycast(_vrPointer.vrController.position, _vrPointer.vrController.forward, out RaycastHit hit, MaxLaserDistance))
                    {
                        if (hit.transform != transform && !hit.transform.IsChildOf(transform))
                            return;
                        var grabbingController = _vrPointer.vrController;
                        var grabPos = _vrPointer.vrController.transform.InverseTransformPoint(transform.position);
                        var grabRot = Quaternion.Inverse(_vrPointer.vrController.transform.rotation) * transform.rotation;
                        var snowball = _snowballManager.CreateSnowball(Guid.NewGuid());
                        snowball.SetGrabbed(grabbingController, grabPos, grabRot);
                    }
                }
                else if (IsGrabbing && (_vrPointer.vrController.triggerValue <= 0.9f || Input.GetMouseButtonUp(0)))
                    IsGrabbing = false;
            }
        }
    }
}
