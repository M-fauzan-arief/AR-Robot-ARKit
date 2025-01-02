using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

/**
 * Defines a Hinge joint with rotation limits.
 * 
 * Author: Pascal Zwick
 * e-mail: zwick@fzi.de
 * */

namespace BurstIK
{
    public class RobotHingeJoint : RobotJoint
    {
        // Define min and max rotation angles for the joint in degrees
        [Tooltip("Minimum rotation angle for the hinge joint in degrees.")]
        public float minAngle = -90f; // Set this to your desired minimum angle
        [Tooltip("Maximum rotation angle for the hinge joint in degrees.")]
        public float maxAngle = 90f;  // Set this to your desired maximum angle

        public override void Awake()
        {
            base.Awake();
            this.Type = JointType.HINGE;
        }

        public override void ApplyForwardKinematics(float value, float3 initialPosition, quaternion initialRotation)
        {
            // Clamp the rotation value between minAngle and maxAngle
            float clampedValue = math.clamp(value, minAngle, maxAngle);

            // Apply the clamped rotation
            this.transform.localRotation = math.mul(initialRotation, quaternion.AxisAngle(this.Axis, math.radians(clampedValue)));
        }
    }
}
