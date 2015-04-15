// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AvatarMovement.cs" company="Jasper Ermatinger">
//   Copyright © 2014 Jasper Ermatinger
//   Do not distribute or publish this code or parts of it in any way without permission of the copyright owner.
// </copyright>
// <summary>
//   The avatar movement.
// </summary>
// --------------------------------------------------------------------------------------------------------------------


namespace QLDemo
{
    using UnityEngine;

    /// <summary>
    /// The avatar movement.
    /// </summary>
    public class AvatarMovement : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// The main camera.
        /// </summary>
        public Camera MainCamera;

        #endregion

        #region Methods

        /// <summary>
        /// The update.
        /// </summary>
        private void Update()
        {
            var hit = new RaycastHit();
            if (Physics.Raycast(
                this.MainCamera.ScreenPointToRay(Input.mousePosition),
                out hit,
                10000,
                1 << LayerMask.NameToLayer("Ground")))
            {
                this.transform.position = new Vector3(hit.point.x, this.transform.position.y, hit.point.z);
            }
        }

        #endregion
    }
}