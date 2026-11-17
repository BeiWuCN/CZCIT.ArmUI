using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;

namespace CZCIT.ArmUI
{
    [DisallowMultipleComponent]
    public sealed class WristPanelDock : MonoBehaviour
    {
        [SerializeField] private Transform leftDock;
        [SerializeField] private Transform rightHand;
        [SerializeField] private Transform viewer;
        [SerializeField] private CanvasGroup panel;
        [SerializeField] private Vector3 revealAxis = Vector3.up;
        [SerializeField, Range(-1f, 1f)] private float revealDot = 0.15f;
        [SerializeField, Min(0f)] private float snapDistance = 0.22f;

        private bool docked = true;
        private bool dragging;
        private int pointerId;
        private Vector3 grabOffset;

        private void LateUpdate()
        {
            if (!panel || !leftDock || !viewer)
                return;

            bool leftTracked = IsTracked(XRNode.LeftHand);
            if (dragging && (!rightHand || !IsTracked(XRNode.RightHand)))
                dragging = false;

            if (dragging)
                panel.transform.position = rightHand.TransformPoint(grabOffset);
            else if (docked && leftTracked)
                panel.transform.position = leftDock.position;

            bool show = dragging || !docked ||
                (leftTracked && Vector3.Dot(leftDock.TransformDirection(revealAxis.normalized), Vector3.up) >= revealDot);
            if (show && (dragging || docked))
            {
                Vector3 facing = panel.transform.position - viewer.position;
                if (Vector3.Cross(facing, Vector3.up).sqrMagnitude > 0.0001f)
                    panel.transform.rotation = Quaternion.LookRotation(facing, Vector3.up);
            }

            SetVisible(show);
        }

        public void BeginDrag(PointerEventData pointer)
        {
            if (dragging || !rightHand || !panel || panel.alpha < 0.99f || !IsTracked(XRNode.RightHand))
                return;

            // Preserve the offset so grabbing the handle does not jump the panel.
            pointerId = pointer.pointerId;
            grabOffset = rightHand.InverseTransformPoint(panel.transform.position);
            dragging = true;
        }

        public void EndDrag(PointerEventData pointer)
        {
            if (!dragging || pointer.pointerId != pointerId)
                return;

            dragging = false;
            docked = leftDock && IsTracked(XRNode.LeftHand) &&
                Vector3.Distance(panel.transform.position, leftDock.position) <= snapDistance;
        }

        public void ReturnToWrist()
        {
            dragging = false;
            docked = true;
        }

        private void OnDisable()
        {
            dragging = false;
            if (panel)
                SetVisible(false);
        }

        private void SetVisible(bool visible)
        {
            panel.alpha = visible ? 1f : 0f;
            panel.interactable = visible && !dragging;
            panel.blocksRaycasts = visible && !dragging;
        }

        private static bool IsTracked(XRNode node)
        {
            var device = InputDevices.GetDeviceAtXRNode(node);
            return device.isValid && device.TryGetFeatureValue(CommonUsages.isTracked, out bool tracked) && tracked;
        }
    }
}
