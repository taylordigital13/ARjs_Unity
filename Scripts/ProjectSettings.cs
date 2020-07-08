using UnityEngine;

public class ProjectSettings : MonoBehaviour
{
    public bool UseAmbientLight = false;
    public Color AmbientLightColor = Color.red;

    void Start()
    {
        RenderSettings.ambientLight = AmbientLightColor;
    }

    private void OnValidate()
    {
        if (UseAmbientLight)
        {
            RenderSettings.ambientLight = AmbientLightColor;
        }
        else
        {
            RenderSettings.ambientLight = Color.white;
        }
    }
}