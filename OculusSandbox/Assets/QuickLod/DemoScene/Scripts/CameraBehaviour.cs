// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CameraBehaviour.cs" company="Jasper Ermatinger">
//   Copyright � 2014 Jasper Ermatinger
//   Do not distribute or publish this code or parts of it in any way without permission of the copyright owner.
// </copyright>
// <summary>
//   The camera behaviour.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QLDemo
{
    using System;

    using System.Collections.Generic;

    using UnityEngine;

    /// <summary>
    /// The camera behaviour.
    /// </summary>
    public class CameraBehaviour : MonoBehaviour
    {
        [Serializable]
        public class AxisSettings
        {
            public string AxisName;

            public bool Invert;
        }

        #region Fields

        /// <summary>
        /// The absolute horizontal distance.
        /// </summary>
        public float AbsoluteHorizontalDistance;

        /// <summary>
        /// The absolute vertical distance.
        /// </summary>
        public float AbsoluteVerticalDistance;

        /// <summary>
        /// The allow camera collision.
        /// </summary>
        public bool AllowCameraCollision;

        /// <summary>
        /// The allow x rotation.
        /// </summary>
        public bool AllowXRotation;

        /// <summary>
        /// The allow y rotation.
        /// </summary>
        public bool AllowYRotation;

        /// <summary>
        /// The allow zoom.
        /// </summary>
        public bool AllowZoom;

        /// <summary>
        /// The camera damping.
        /// </summary>
        public float CameraDamping;

        /// <summary>
        /// The collision precission.
        /// </summary>
        public int CollisionPrecission;

        /// <summary>
        /// The collision radius.
        /// </summary>
        public float CollisionRadius;

        /// <summary>
        /// The current x rotation.
        /// </summary>
        public float CurrentXRotation;

        /// <summary>
        /// The current y rotation.
        /// </summary>
        public float CurrentYRotation;

        /// <summary>
        /// The current zoom.
        /// </summary>
        public float CurrentZoom;

        /// <summary>
        /// The ignore layer.
        /// </summary>
        public LayerMask IgnoreLayer;

        /// <summary>
        /// The lock cursor to center screen.
        /// </summary>
        public bool LockCursorToCenterScreen;

        /// <summary>
        /// The main camera.
        /// </summary>
        public Camera MainCamera;

        /// <summary>
        /// The max horizontal distance.
        /// </summary>
        public float MaxHorizontalDistance;

        /// <summary>
        /// The max vertical distance.
        /// </summary>
        public float MaxVerticalDistance;

        /// <summary>
        /// The max x rotation.
        /// </summary>
        public float MaxXRotation;

        /// <summary>
        /// The max y rotation.
        /// </summary>
        public float MaxYRotation;

        /// <summary>
        /// The min horizontal distance.
        /// </summary>
        public float MinHorizontalDistance;

        /// <summary>
        /// The min vertical distance.
        /// </summary>
        public float MinVerticalDistance;

        /// <summary>
        /// The min x rotation.
        /// </summary>
        public float MinXRotation;

        /// <summary>
        /// The min y rotation.
        /// </summary>
        public float MinYRotation;

        /// <summary>
        /// The restrict x rotation.
        /// </summary>
        public bool RestrictXRotation;

        /// <summary>
        /// The restrict y rotation.
        /// </summary>
        public bool RestrictYRotation;

        /// <summary>
        /// The rotation speed.
        /// </summary>
        public float RotationSpeed;

        /// <summary>
        /// The show collision calculation.
        /// </summary>
        public bool ShowCollisionCalculation;

        /// <summary>
        /// The show cursor.
        /// </summary>
        public bool ShowCursor;

        // X Rotation

        /// <summary>
        /// The triggered x rotation input axis names.
        /// </summary>
        public AxisSettings[] TriggeredXRotationInputAxisNames;

        /// <summary>
        /// The triggered y rotation input axis names.
        /// </summary>
        public AxisSettings[] TriggeredYRotationInputAxisNames;

        /// <summary>
        /// The triggered zoom input axis names.
        /// </summary>
        public AxisSettings[] TriggeredZoomInputAxisNames;

        /// <summary>
        /// The x rotation input axis names.
        /// </summary>
        public AxisSettings[] XRotationInputAxisNames;

        /// <summary>
        /// The x trigger key code.
        /// </summary>
        public KeyCode XTriggerKeyCode;

        /// <summary>
        /// The y rotation input axis names.
        /// </summary>
        public AxisSettings[] YRotationInputAxisNames;

        /// <summary>
        /// The y trigger key code.
        /// </summary>
        public KeyCode YTriggerKeyCode;

        /// <summary>
        /// The zoom input axis names.
        /// </summary>
        public AxisSettings[] ZoomInputAxisNames;

        /// <summary>
        /// The zoom speed.
        /// </summary>
        public float ZoomSpeed;

        /// <summary>
        /// The zoom trigger key code.
        /// </summary>
        public KeyCode ZoomTriggerKeyCode;

        /// <summary>
        /// The previous avatar position.
        /// </summary>
        private Vector3 previousAvatarPosition;

        public int previousTabSelection;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CameraBehaviour"/> class.
        /// </summary>
        public CameraBehaviour()
        {
            this.MainCamera = null;

            this.ZoomSpeed = 1.0f;
            this.RotationSpeed = 4.0f;
            this.CameraDamping = 4.0f;

            this.LockCursorToCenterScreen = false;
            this.ShowCursor = true;

            this.AllowZoom = true;
            this.ZoomInputAxisNames = new AxisSettings[0];
            this.TriggeredZoomInputAxisNames = new AxisSettings[0];
            this.ZoomTriggerKeyCode = KeyCode.None;
            this.AbsoluteVerticalDistance = 1.0f;
            this.AbsoluteHorizontalDistance = 1.0f;
            this.CurrentZoom = 1.0f;
            this.MaxVerticalDistance = 4.0f;
            this.MinVerticalDistance = 1.0f;
            this.MaxHorizontalDistance = 2.0f;
            this.MinHorizontalDistance = 1.0f;

            this.AllowXRotation = true;

            this.XRotationInputAxisNames = new AxisSettings[0];
            this.TriggeredXRotationInputAxisNames = new AxisSettings[0];
            this.XTriggerKeyCode = KeyCode.None;
            this.CurrentXRotation = 0.0f;
            this.RestrictXRotation = true;
            this.MaxXRotation = 90.0f;
            this.MinXRotation = -30.0f;

            this.AllowYRotation = true;
            this.YRotationInputAxisNames = new AxisSettings[0];
            this.TriggeredYRotationInputAxisNames = new AxisSettings[0];
            this.YTriggerKeyCode = KeyCode.None;
            this.CurrentYRotation = 0.0f;
            this.RestrictYRotation = false;
            this.MaxYRotation = 0.0f;
            this.MinYRotation = 0.0f;

            this.AllowCameraCollision = true;
            this.CollisionRadius = 0.3f;
            this.CollisionPrecission = 1;
            this.ShowCollisionCalculation = false;
            this.IgnoreLayer = new LayerMask();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the input from the definied rotation axis
        /// </summary>
        /// <param name="xRotation">
        /// X rotation.
        /// </param>
        /// <param name="yRotation">
        /// Y rotation.
        /// </param>
        private void GetRotationKeyDown(out float xRotation, out float yRotation)
        {
            xRotation = 0.0f;
            yRotation = 0.0f;

            foreach (var xInput in this.XRotationInputAxisNames)
            {
                if (xInput.Invert)
                {
                    xRotation -= Input.GetAxis(xInput.AxisName);
                }
                else
                {
                    xRotation += Input.GetAxis(xInput.AxisName);
                }
            }

            foreach (var yInput in this.YRotationInputAxisNames)
            {
                if (yInput.Invert)
                {
                    yRotation -= Input.GetAxis(yInput.AxisName);
                }
                else
                {
                    yRotation += Input.GetAxis(yInput.AxisName);
                }

            }

            if (Input.GetKey(this.XTriggerKeyCode))
            {
                foreach (var triggeredXInput in this.TriggeredXRotationInputAxisNames)
                {
                    if (triggeredXInput.Invert)
                    {
                        xRotation -= Input.GetAxis(triggeredXInput.AxisName);
                    }
                    else
                    {
                        xRotation += Input.GetAxis(triggeredXInput.AxisName);
                    }
                }
            }

            if (Input.GetKey(this.YTriggerKeyCode))
            {
                foreach (var triggeredYInput in this.TriggeredYRotationInputAxisNames)
                {
                    if (triggeredYInput.Invert)
                    {
                        yRotation -= Input.GetAxis(triggeredYInput.AxisName);
                    }
                    else
                    {
                        yRotation += Input.GetAxis(triggeredYInput.AxisName);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the input from the definied zoom axis
        /// </summary>
        /// <param name="zoom">
        /// How much the camera should zoom
        /// </param>
        private void GetZoomKeyDown(out float zoom)
        {
            zoom = 0.0f;

            foreach (var zoomInput in this.ZoomInputAxisNames)
            {
                if (zoomInput.Invert)
                {
                    zoom -= Input.GetAxis(zoomInput.AxisName);
                }
                else
                {
                    zoom += Input.GetAxis(zoomInput.AxisName);
                }
            }

            if (Input.GetKey(this.ZoomTriggerKeyCode))
            {
                foreach (var triggeredZoomInput in this.TriggeredZoomInputAxisNames)
                {
                    if (triggeredZoomInput.Invert)
                    {
                        zoom -= Input.GetAxis(triggeredZoomInput.AxisName);
                    }
                    else
                    {
                        zoom += Input.GetAxis(triggeredZoomInput.AxisName);
                    }
                }
            }
        }

        /// <summary>
        /// The rotate x.
        /// </summary>
        /// <param name="v">
        /// The v.
        /// </param>
        /// <param name="angle">
        /// The angle.
        /// </param>
        private void RotateX(ref Vector3 v, float angle)
        {
            v = Quaternion.Euler(angle, 0.0f, 0.0f) * v;
        }

        /// <summary>
        /// The rotate y.
        /// </summary>
        /// <param name="v">
        /// The v.
        /// </param>
        /// <param name="angle">
        /// The angle.
        /// </param>
        private void RotateY(ref Vector3 v, float angle)
        {
            v = Quaternion.Euler(0.0f, angle, 0.0f) * v;
        }

        /// <summary>
        /// The update.
        /// </summary>
        private void Update()
        {
            if (this.MainCamera == null)
            {
                return;
            }

            Cursor.lockState = this.LockCursorToCenterScreen ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = this.ShowCursor;

            this.MainCamera.transform.position += this.transform.position - this.previousAvatarPosition;
            this.previousAvatarPosition = this.transform.position;

            // Zoom
            if (this.AllowZoom)
            {
                float addZoom;
                this.GetZoomKeyDown(out addZoom);

                if (Math.Abs(addZoom - 0.0f) > Mathf.Epsilon)
                {
                    this.CurrentZoom += addZoom * this.ZoomSpeed * Time.deltaTime * 3 * (0.1f + this.CurrentZoom) / 1.1f;

                    if (this.CurrentZoom < 0.0f)
                    {
                        this.CurrentZoom = 0.0f;
                    }

                    if (this.CurrentZoom > 1.0f)
                    {
                        this.CurrentZoom = 1.0f;
                    }
                }
            }

            // Rotation
            if (this.AllowXRotation || this.AllowYRotation)
            {
                var addXRotation = 0.0f;
                var addYRotation = 0.0f;

                this.GetRotationKeyDown(out addXRotation, out addYRotation);

                if (this.AllowXRotation && addXRotation != 0.0f)
                {
                    this.CurrentXRotation += addXRotation * this.RotationSpeed * Time.deltaTime * 10;

                    if (this.RestrictXRotation)
                    {
                        if (this.CurrentXRotation < this.MinXRotation)
                        {
                            this.CurrentXRotation = this.MinXRotation;
                        }
                        else if (this.CurrentXRotation > this.MaxXRotation)
                        {
                            this.CurrentXRotation = this.MaxXRotation;
                        }
                    }
                }

                if (this.AllowYRotation && addYRotation != 0.0f)
                {
                    this.CurrentYRotation += addYRotation * this.RotationSpeed * Time.deltaTime * 10;

                    if (this.RestrictYRotation)
                    {
                        if (this.CurrentYRotation < this.MinYRotation)
                        {
                            this.CurrentYRotation = this.MinYRotation;
                        }
                        else if (this.CurrentYRotation > this.MaxYRotation)
                        {
                            this.CurrentYRotation = this.MaxYRotation;
                        }
                    }
                }
            }

            // Calculates the position of the camera
            var destination = Vector3.zero;
            var verticalZoomDistance = (this.MaxVerticalDistance - this.MinVerticalDistance) * this.CurrentZoom
                                       + this.MinVerticalDistance;
            var horizontalZoomDistance = (this.MaxHorizontalDistance - this.MinHorizontalDistance) * this.CurrentZoom
                                         + this.MinHorizontalDistance;

            var dist = Vector3.Distance(
                Vector3.zero,
                new Vector3(
                    Mathf.Sin(this.CurrentXRotation / 180.0f * Mathf.PI)
                    * (this.AllowZoom ? verticalZoomDistance : this.AbsoluteVerticalDistance),
                    Mathf.Cos(this.CurrentXRotation / 180.0f * Mathf.PI)
                    * (this.AllowZoom ? horizontalZoomDistance : this.AbsoluteHorizontalDistance),
                    0.0f));

            // Calculates the relative position for the camera without YRotation
            var absoluteVector = new Vector3(
                Mathf.Cos(this.CurrentXRotation / 180.0f * Mathf.PI) * dist,
                Mathf.Sin(this.CurrentXRotation / 180.0f * Mathf.PI) * dist,
                0.0f);
            this.RotateY(ref absoluteVector, this.CurrentYRotation);

            // Calculates the desired position
            destination = absoluteVector + this.transform.position;

            // When camera collision is active, also check for collision
            if (this.AllowCameraCollision)
            {
                var rayInfo = new List<RaycastHit>();
                var hasHit = new List<bool>();

                // First check for collission (with rays) and store the data in the list
                for (int i = 0; i < 4 * this.CollisionPrecission; i++)
                {
                    // Calculates the side direction to calculate the startpoint and targetpoint
                    var direction = Quaternion.Euler(
                        this.CurrentXRotation,
                        this.CurrentYRotation - 90.0f,
                        i * (360.0f / (4.0f * this.CollisionPrecission)));

                    // Calculates from where the ray should be fired
                    var startPosition = direction * new Vector3(this.CollisionRadius, 0.0f, 0.0f)
                                        + this.transform.position;

                    // Calculates a relative target position
                    var targetPosition = direction * new Vector3(0.0f, 0.0f, -dist);

                    // Shows where the ray goes through
                    if (this.ShowCollisionCalculation)
                    {
                        Debug.DrawLine(this.transform.position, startPosition);
                        Debug.DrawLine(startPosition, targetPosition + startPosition);
                    }

                    // Shoot a ray and stores if the ray has hit something and the hitinfo
                    var temporarHitInfo = new RaycastHit();
                    hasHit.Add(
                        Physics.Raycast(
                            startPosition, targetPosition, out temporarHitInfo, dist * 1.1f, ~this.IgnoreLayer));
                    rayInfo.Add(temporarHitInfo);

                    // Shows what the ray actualy does
                    if (hasHit[hasHit.Count - 1] && this.ShowCollisionCalculation)
                    {
                        Debug.DrawLine(startPosition, temporarHitInfo.point, Color.magenta);
                    }
                }

                // Needed to determine which 
                var nearestValue = float.PositiveInfinity;
                var nearestIndex = -1;

                // Calculates the index of the ray which has hit at the shortest distance from the startpoint
                for (int i = 0; i < rayInfo.Count; i++)
                {
                    // Only use rays which hit something
                    if (hasHit[i])
                    {
                        if (rayInfo[i].distance < nearestValue)
                        {
                            nearestValue = rayInfo[i].distance;
                            nearestIndex = i;
                        }
                    }
                }

                if (nearestIndex > -1)
                {
                    destination = Vector3.Lerp(this.transform.position, destination, nearestValue / dist);
                    destination += rayInfo[nearestIndex].normal * this.CollisionRadius;
                }
            }

            this.MainCamera.transform.position = Vector3.Lerp(
                this.MainCamera.transform.position, destination, this.CameraDamping * Time.deltaTime * 5.0f);

            this.MainCamera.transform.LookAt(this.transform.position);

            if (Mathf.Abs(this.MainCamera.transform.position.x - this.transform.position.x) < 0.0001
                || Mathf.Abs(this.MainCamera.transform.position.z - this.transform.position.z) < 0.0001)
            {
                this.MainCamera.transform.rotation = Quaternion.Euler(
                    this.MainCamera.transform.rotation.eulerAngles.x,
                    this.CurrentYRotation - 90.0f,
                    this.MainCamera.transform.rotation.eulerAngles.z);
            }
        }

        #endregion
    }
}
