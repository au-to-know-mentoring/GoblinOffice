using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public SettingsData settingsData;

    private void Awake()
    {
        // Load the ScriptableObject instance from the asset file
        settingsData = Resources.Load<SettingsData>("SettingsData");
    }
}
