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
        float initialJ1Rotation = armController.J1YRot;
        float initialJ2Rotation = armController.J2YRot;
        float initialJ3Rotation = armController.J3YRot;
        float initialJ4Rotation = armController.J4YRot;

        while (elapsedTime < resetDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / resetDuration;

            // Lerp between the initial and target (0) rotation values
            armController.J1YRot = (int)Mathf.Lerp(initialJ1Rotation, 0, t);
            armController.J2YRot = (int)Mathf.Lerp(initialJ2Rotation, 0, t);
            armController.J3YRot = (int)Mathf.Lerp(initialJ3Rotation, 0, t);
            armController.J4YRot = (int)Mathf.Lerp(initialJ4Rotation, 0, t);

            armController.UpdateJointRotations();  // Update joint rotations in Arm_Controller
            yield return null;
        }

        // Ensure joints are fully reset to zero
        armController.J1YRot = 0;
        armController.J2YRot = 0;
        armController.J3YRot = 0;
        armController.J4YRot = 0;
        armController.UpdateJointRotations();
    }
}
