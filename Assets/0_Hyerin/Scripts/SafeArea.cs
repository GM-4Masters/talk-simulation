using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeArea : MonoBehaviour
{
    private RectTransform rt;
    private Rect safeArea;
    private Vector2 minAnchor, maxAnchor;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        safeArea = Screen.safeArea;
        minAnchor = safeArea.position;
        maxAnchor = minAnchor + safeArea.size;

        minAnchor.x /= Screen.width;
        minAnchor.y /= Screen.height;
        maxAnchor.x /= Screen.width;
        maxAnchor.y /= Screen.height;

        rt.anchorMin = minAnchor;
        rt.anchorMax = maxAnchor;
    }
}
