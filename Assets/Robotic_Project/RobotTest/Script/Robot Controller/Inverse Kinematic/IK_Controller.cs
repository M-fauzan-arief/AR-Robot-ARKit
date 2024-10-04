using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class IK_Controller : MonoBehaviour
{
    [Header("Joint Transforms")]
    public Transform J1;  // Joint 1: Rotates around Z axis
    public Transform J2;  // Joint 2: Rotates around Y axis
    public Transform J3;  // Joint 3: Rotates around X axis
    public Transform EF;  // End Effector as the 4th joint (optional)

    [Header("Buttons")]
    public Button targetUpButton;
    public Button targetDownButton;
    public Button targetLeftButton;
    public Button targetRightButton;

    [Header("IK Settings")]
    public Transform IKTarget;  // The target for IK
    public float IKThreshold = 0.1f;  // Threshold for IK to stop
    public float movementSpeed = 0.01f;  // Speed of the IK target movement

    [Header("Joint Limits")]
    public float j1MinAngle = -90f;
    public float j1MaxAngle = 90f;
    public float j2MinAngle = -90f;
    public float j2MaxAngle = 90f;
    public float j3MinAngle = -90f;
    public float j3MaxAngle = 90f;

    private void Start()
    {
        // Assign button events
        AssignButtonEvents(targetUpButton, () => MoveTarget(Vector3.up));
        AssignButtonEvents(targetDownButton, () => MoveTarget(Vector3.down));
        AssignButtonEvents(targetLeftButton, () => MoveTarget(Vector3.left));
        AssignButtonEvents(targetRightButton, () => MoveTarget(Vector3.right));
    }

    private void Update()
    {
        // Update inverse kinematics
        SolveInverseKinematics();
    }

    void AssignButtonEvents(Button button, System.Action action)
    {
        EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();

        var pointerDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        pointerDown.callback.AddListener((e) => StartCoroutine(ContinuousAction(action)));
        trigger.triggers.Add(pointerDown);

        var pointerUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        pointerUp.callback.AddListener((e) => StopAllCoroutines());
        trigger.triggers.Add(pointerUp);
    }

    IEnumerator ContinuousAction(System.Action action)
    {
        while (true)
        {
            action();
            yield return new WaitForSeconds(0.05f);  // Adjust delay to control movement speed
        }
    }

    void MoveTarget(Vector3 direction)
    {
        // Smoothly move the IK target in the specified direction
        IKTarget.position += direction * movementSpeed * Time.deltaTime;
    }

    private void SolveInverseKinematics()
    {
        // Iteratively solve IK
        for (int i = 0; i < 10; i++)  // Increase iterations for better convergence
        {
            // Solve IK for J3, J2, and EF first
            AdjustJointTowardsTarget(J3, IKTarget.position, J3.right);  // Adjust Joint 3 around X axis
            AdjustJointTowardsTarget(J2, IKTarget.position, J2.up);  // Adjust Joint 2 around Y axis
            AdjustEndEffectorToTarget(IKTarget.position);  // Adjust EF directly

            // Adjust J1 to follow the changes
            AdjustJoint1ToFollow();

            // Check if the end effector is within the threshold
            float distanceToTarget = Vector3.Distance(EF.position, IKTarget.position);
            if (distanceToTarget < IKThreshold)
            {
                Debug.Log("IK has converged within the threshold.");
                break;
            }
        }
    }

    private void AdjustJointTowardsTarget(Transform joint, Vector3 targetPosition, Vector3 axis)
    {
        // Get the direction from the joint to the end effector and to the target
        Vector3 toEndEffector = EF.position - joint.position;
        Vector3 toTarget = targetPosition - joint.position;

        // Project the directions onto the plane perpendicular to the specified axis
        Vector3 projectedToEndEffector = Vector3.ProjectOnPlane(toEndEffector, axis);
        Vector3 projectedToTarget = Vector3.ProjectOnPlane(toTarget, axis);

        // Calculate the rotation needed to align the end effector direction to the target direction
        Quaternion rotation = Quaternion.FromToRotation(projectedToEndEffector, projectedToTarget);

        // Apply the rotation only around the specified local axis
        Vector3 localEulerAngles = joint.localEulerAngles;
        float rotationAngle = Mathf.Clamp(rotation.eulerAngles.magnitude, -1f, 1f);  // Limit the rotation angle

        // Adjust rotation axis for each joint
        if (joint == J2)
        {
            localEulerAngles.y = Mathf.Clamp(localEulerAngles.y + rotationAngle, j2MinAngle, j2MaxAngle);  // J2 rotates around Y axis
        }
        else if (joint == J3)
        {
            localEulerAngles.x = Mathf.Clamp(localEulerAngles.x + rotationAngle, j3MinAngle, j3MaxAngle);  // J3 rotates around X axis
        }

        joint.localEulerAngles = localEulerAngles;

        // Debugging joint adjustments
        Debug.Log($"Adjusting {joint.name}: Rotation applied = {rotation.eulerAngles}");
    }

    private void AdjustEndEffectorToTarget(Vector3 targetPosition)
    {
        // Move the EF directly to the target position
        EF.position = targetPosition;
        // Adjust the orientation of the EF if needed
        // EF.LookAt(targetPosition); // Uncomment if EF needs to face the target
    }

    private void AdjustJoint1ToFollow()
    {
        // Adjust J1 to follow the orientation based on J2, J3, and EF
        // For a 3 DOF arm, J1 might not be directly controlled but follow the result of other joints
        // Add logic here if J1 needs to follow any specific constraints or orientations

        // Placeholder: Just ensure J1 remains within its limits
        Vector3 localEulerAngles = J1.localEulerAngles;
        localEulerAngles.z = Mathf.Clamp(localEulerAngles.z, j1MinAngle, j1MaxAngle);  // J1 rotates around Z axis
        J1.localEulerAngles = localEulerAngles;
    }
}
