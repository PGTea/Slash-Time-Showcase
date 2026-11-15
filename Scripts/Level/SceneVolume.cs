using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SceneVolume : MonoBehaviour
{
    [SerializeField] private Volume volume;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color slowMotionColor = Color.blue;
    [SerializeField] private float transitionDuration;

    private float transitionPercentage;
    private ColorAdjustments colorAdjustments;
    private bool isSlowMotion;

    public static SceneVolume Instance;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        if (volume.profile.TryGet(out colorAdjustments))
        {
            colorAdjustments.colorFilter.overrideState = true;

            colorAdjustments.colorFilter.value = Color.blue;
        }
    }

    void Update()
    {
        ChangeColor();
    }

    private void ChangeColor()
    {
        if(isSlowMotion) transitionPercentage += Time.unscaledDeltaTime / transitionDuration;
        else transitionPercentage -= Time.unscaledDeltaTime / transitionDuration;

        transitionPercentage = Mathf.Clamp01(transitionPercentage);

        Debug.Log(transitionPercentage);
        colorAdjustments.colorFilter.value = Color.Lerp(normalColor, slowMotionColor, transitionPercentage);
    }

    public void SetSlowMotion(bool isSlowMotion)
    {
        this.isSlowMotion = isSlowMotion;
    }
}
