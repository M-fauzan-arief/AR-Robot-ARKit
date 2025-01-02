using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ResetButtonController : MonoBehaviour
{
    public Button resetButton;  // Reference to the reset button
    public Arm_Controller armController;  // Reference to the Arm_Controller script
    public float resetDuration = 2.0f;  // Duration of the reset

    private void Start()
    {
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(StartReset);
        }
    }

    // Start the reset process
    private void StartReset()
    {
        if (armController != null)
        {
            StartCoroutine(ResetJoints());
        }
    }

    // Coroutine to smoothly reset the joints to their default positions over a duration
    private IEnumerator ResetJoints()
    {
        float elapsedTime = 0.0f;

        // Store initial joint rotations from Arm_Controller
        float initialJ1Rotation = armController.J1ZRot;  // J1 rotates on Z-axis
        float initialJ2Rotation = armController.J2XRot;  // J2 rotates on X-axis
        float initialJ3Rotation = armController.J3XRot;  // J3 rotates on X-axis
        float initialJ4Rotation = armController.J4ZRot;  // J4 rotates on Z-axis

        while (elapsedTime < resetDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / resetDuration;

            // Lerp between the initial and target (0) rotation values
            armController.J1ZRot = (int)Mathf.Lerp(initialJ1Rotation, 0, t);
            armController.J2XRot = (int)Mathf.Lerp(initialJ2Rotation, 0, t);
            armController.J3XRot = (int)Mathf.Lerp(initialJ3Rotation, 0, t);
            armController.J4ZRot = (int)Mathf.Lerp(initialJ4Rotation, 0, t);

            armController.UpdateJointRotations();  // Update joint rotations in Arm_Controller
            yield return null;
        }

        // Ensure joints are fully reset to zero
        armController.J1ZRot = 0;
        armController.J2XRot = 0;
        armController.J3XRot = 0;
        armController.J4ZRot = 0;
        armController.UpdateJointRotations();
    }
}
