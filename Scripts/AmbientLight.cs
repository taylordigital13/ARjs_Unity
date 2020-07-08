using UnityEngine;

public class AmbientLight : MonoBehaviour
{
    public Color AmbientLightColor = Color.red;

    // Start is called before the first frame update
    private void OnValidate()
    {
        RenderSettings.ambientLight = AmbientLightColor;
    }

    void Start()
    {
        RenderSettings.ambientLight = AmbientLightColor;
    }

    private void OnDestroy()
    {
        RenderSettings.ambientLight = Color.white;
    }
}