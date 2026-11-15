using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChanger : MonoBehaviour
{
    private bool isTransitioning = false;
    private float checkTimer = 0f;

    public static SceneChanger Instance;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    public void LoadScene(string targetSceneName, float transitionBeforeDuration, float transitionAfterDuration, float waitBeforeLoad, Action onSceneLoaded = null)
    {
        if (!isTransitioning)
            StartCoroutine(LoadLevelRoutine(targetSceneName, transitionBeforeDuration, transitionAfterDuration,waitBeforeLoad, onSceneLoaded));
    }

    
    private IEnumerator LoadLevelRoutine(string targetSceneName, float transitionBeforeDuration, float transitionAfterDuration,float waitBeforeLoad, Action onSceneLoaded)
    {
        isTransitioning = true;

        if (waitBeforeLoad > 0)
        {
            yield return new WaitForSeconds(waitBeforeLoad);
        }

        if (TryGetComponent<Collider>(out var col))
        {
            col.enabled = false;
        }

        GameObject transitionCanvas = CreateTransitionCanvas();
        CanvasGroup canvasGroup = transitionCanvas.GetComponent<CanvasGroup>();

        DontDestroyOnLoad(this.gameObject);
        DontDestroyOnLoad(transitionCanvas);

        yield return StartCoroutine(FadeCanvasGroup(canvasGroup, 0f, 1f, transitionBeforeDuration));

        AsyncOperation operation = SceneManager.LoadSceneAsync(targetSceneName);
        operation.allowSceneActivation = false;

        while (operation.progress < 0.9f)
        {
            yield return null;
        }

        operation.allowSceneActivation = true;

        while (!operation.isDone)
        {
            yield return null;
        }

        onSceneLoaded?.Invoke();

        yield return StartCoroutine(FadeCanvasGroup(canvasGroup, 1f, 0f, transitionAfterDuration));

        Destroy(transitionCanvas);
        isTransitioning = false;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float startAlpha, float endAlpha, float transitionDuration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / transitionDuration);
            yield return null;
        }
        cg.alpha = endAlpha;
    }

    private GameObject CreateTransitionCanvas()
    {
        // Create a new GameObject with a Canvas
        GameObject canvasGO = new GameObject("TransitionCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        // Set a very high sort order to ensure it covers EVERYTHING in the new scene
        canvas.sortingOrder = 32767; 

        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Create the black image
        GameObject imageGO = new GameObject("FadeImage");
        imageGO.transform.SetParent(canvasGO.transform, false);
        Image image = imageGO.AddComponent<Image>();
        image.color = Color.black;
        image.raycastTarget = true; // Block clicks

        // Stretch the image to fill the screen
        RectTransform rt = image.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        // Add CanvasGroup to control transparency easily
        CanvasGroup cg = canvasGO.AddComponent<CanvasGroup>();
        cg.alpha = 0f; // Start transparent

        return canvasGO;
    }
}
