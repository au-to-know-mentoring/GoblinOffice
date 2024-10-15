using UnityEngine;

public class SlowMotionDebug : MonoBehaviour
{
    [SerializeField] private float slowMotionTimeScale = 0.33f;
    private bool isSlowMotion = false;

    void Update()
    {
        // Toggle slow motion on/off with the 'T' key
        if (Input.GetKeyDown(KeyCode.T))
        {
            ToggleSlowMotion();
        }
    }

    void ToggleSlowMotion()
    {
        isSlowMotion = !isSlowMotion;
        Time.timeScale = isSlowMotion ? slowMotionTimeScale : 1f;
    }
}