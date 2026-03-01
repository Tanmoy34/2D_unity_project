using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    // Use Canvas (or assign the Panel GameObject's Canvas component) — no fading, just deactivate it.
    [Tooltip("Optional Canvas used to represent the menu/panel. Will be deactivated immediately when loading a scene.")]
    [SerializeField] private Canvas panelCanvas;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (panelCanvas == null)
            panelCanvas = GetComponentInChildren<Canvas>();

        // Ensure panel (if present) is active by default
        if (panelCanvas != null)
            panelCanvas.gameObject.SetActive(true);
    }

    // Public API for UI buttons or other code
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    public void ReloadScene()
    {
        StartCoroutine(LoadSceneRoutine(SceneManager.GetActiveScene().name));
    }

    public void Quit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        // Ensure game is unpaused before switching scenes
        Time.timeScale = 1f;

        // Deactivate the assigned Canvas panel immediately (no fade)
        if (panelCanvas != null)
            panelCanvas.gameObject.SetActive(false);

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = true;

        while (!op.isDone)
            yield return null;

        yield break;
    }
}
