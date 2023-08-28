using UnityEngine;
using UnityEngine.UI;

public class UIImageScroll : MonoBehaviour
{
    public float scrollSpeed = 50.0f;
    public float despawnThreshold = 0.0f;

    private RectTransform rectTransform;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        rectTransform.anchoredPosition -= Vector2.up * scrollSpeed * Time.deltaTime;

        if (rectTransform.anchoredPosition.y <= -despawnThreshold)
        {
            Debug.Log("beat event despawned at:" + Time.time);
            Destroy(gameObject);
        }
    }
}