using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ScrollListener : MonoBehaviour
{
    public ScrollRect scrollRect; // Assign the Scroll View
    public List<Button> buttonsToShow; // List of buttons assigned in the editor

    void Update()
    {
        foreach (Button btn in buttonsToShow)
        {
            if (btn != null)
            {
                btn.gameObject.SetActive(IsButtonVisible(btn));
            }
        }
    }

    bool IsButtonVisible(Button btn)
    {
        if (scrollRect == null || btn == null) return false;

        RectTransform viewport = scrollRect.viewport;
        RectTransform buttonRect = btn.GetComponent<RectTransform>();

        Vector3[] viewportCorners = new Vector3[4];
        Vector3[] buttonCorners = new Vector3[4];

        viewport.GetWorldCorners(viewportCorners);
        buttonRect.GetWorldCorners(buttonCorners);

        // Check if the button is inside the viewport (y-axis visibility)
        return buttonCorners[0].y < viewportCorners[1].y && buttonCorners[1].y > viewportCorners[0].y;
    }
}
