using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(CanvasScaler))]
public class MinimumSizeCanvasScaler : MonoBehaviour
{
    [SerializeField]
    private int minWidth = 1024;

    [SerializeField]
    private int minHeight = 768;

    [SerializeField]
    private int maxWidth = 1920;

    [SerializeField]
    private int maxHeight = 1080;

    private CanvasScaler canvasScaler;

    private void Start()
    {
        canvasScaler = GetComponent<CanvasScaler>();
    }

    private void OnEnable()
    {
        Start();
        Update();
    }

    private void Update()
    {
        float scale;
        if (Screen.height < minHeight || Screen.width < minWidth)
        {
            scale = Mathf.Min(Screen.height / (float)minHeight, Screen.width / (float)minWidth);
        }
        else if (Screen.height > maxHeight || Screen.width > maxWidth)
        {
            scale = Mathf.Max(Screen.height / (float)maxHeight, Screen.width / (float)maxWidth);
        }
        else
        {
            scale = 1;
        }

        canvasScaler.scaleFactor = scale;
    }
    
#if UNITY_EDITOR
    private void OnGUI()
    {
        Update();
    }
#endif
}
