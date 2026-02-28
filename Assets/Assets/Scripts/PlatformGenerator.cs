using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlatformGenerator : MonoBehaviour
{
    [SerializeField] private GameObject platformPrefab;
    [SerializeField] private int platformCount = 5;
    [SerializeField] private float platformSpacing = 5f;
    [SerializeField] private float platformWidth = 3f;
    [SerializeField] private float platformHeight = 0.5f;
    [SerializeField] private Vector2 yRandomRange = new Vector2(-1f, 2f);

    [SerializeField] private GameObject winPanel;
    [SerializeField] private bool pauseOnWin = true;
    [SerializeField] private float winShowDelay = 0.5f;

    private bool hasWon = false;

    private void Start()
    {
        if (platformPrefab == null) return;
        GeneratePlatforms();
    }

    private void GeneratePlatforms()
    {
        CreatePlatform(0, -3f, platformWidth * 2, platformHeight, false);

        for (int i = 1; i < platformCount; i++)
        {
            float xPos = i * platformSpacing;
            float yPos = Random.Range(yRandomRange.x, yRandomRange.y);
            bool isLast = (i == platformCount - 1);
            CreatePlatform(xPos, yPos, platformWidth, platformHeight, isLast);
        }
    }

    private void CreatePlatform(float x, float y, float width, float height, bool isLast)
    {
        GameObject platform = Instantiate(platformPrefab, new Vector3(x, y, 0), Quaternion.identity, transform);
        platform.transform.localScale = new Vector3(width, height, 1);
        platform.name = $"Platform ({x:F1}, {y:F1})";

        if (isLast)
        {
            GameObject triggerObj = new GameObject("WinTrigger");
            triggerObj.transform.SetParent(platform.transform, false);

            float triggerOffsetY = (height / 2f) + 0.25f;
            triggerObj.transform.localPosition = new Vector3(0f, triggerOffsetY, 0f);
            triggerObj.transform.localScale = Vector3.one;

            BoxCollider2D box = triggerObj.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(width, 0.5f);

            WinTrigger wt = triggerObj.AddComponent<WinTrigger>();
            wt.generator = this;
        }
    }

    public void ShowWin()
    {
        if (hasWon) return;
        hasWon = true;
        StartCoroutine(ShowWinRoutine());
    }

    private IEnumerator ShowWinRoutine()
    {
        yield return new WaitForSecondsRealtime(winShowDelay);

        if (winPanel != null)
        {
            winPanel.SetActive(true);

            foreach (Transform t in winPanel.GetComponentsInChildren<Transform>(true))
            {
                t.gameObject.SetActive(true);
            }

            Button restart = winPanel.GetComponentInChildren<Button>(true);
            if (restart != null)
            {
                restart.onClick.RemoveAllListeners();
                restart.onClick.AddListener(RestartGame);
            }

            if (pauseOnWin)
            {
                Time.timeScale = 0f;
            }
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

public class WinTrigger : MonoBehaviour
{
    [HideInInspector] public PlatformGenerator generator;

    private float createdTime;
    private const float minAliveDelay = 0.1f;
    private const float feetTolerance = 0.2f;
    private const float horizontalTolerance = 0.1f;

    private Collider2D parentCollider;
    private Transform parentTransform;

    private void Awake()
    {
        createdTime = Time.time;
        parentTransform = transform.parent;
        parentCollider = parentTransform ? parentTransform.GetComponent<Collider2D>() : null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (generator == null) return;
        if (!other.CompareTag("Player")) return;
        if (Time.time - createdTime < minAliveDelay) return;

        Bounds playerBounds = other.bounds;
        float playerFeetY = playerBounds.min.y;
        float playerCenterX = playerBounds.center.x;

        float platformTopY;
        float platformLeftX;
        float platformRightX;

        if (parentCollider != null)
        {
            Bounds pBounds = parentCollider.bounds;
            platformTopY = pBounds.max.y;
            platformLeftX = pBounds.min.x;
            platformRightX = pBounds.max.x;
        }
        else if (parentTransform != null)
        {
            platformTopY = parentTransform.position.y + (parentTransform.localScale.y / 2f);
            float halfWidth = Mathf.Abs(parentTransform.localScale.x) / 2f;
            platformLeftX = parentTransform.position.x - halfWidth;
            platformRightX = parentTransform.position.x + halfWidth;
        }
        else
        {
            platformTopY = transform.position.y;
            platformLeftX = transform.position.x - 0.5f;
            platformRightX = transform.position.x + 0.5f;
        }

        if (playerFeetY + feetTolerance < platformTopY) return;

        if (playerCenterX < platformLeftX - horizontalTolerance || playerCenterX > platformRightX + horizontalTolerance) return;

        generator.ShowWin();
        Collider2D myCol = GetComponent<Collider2D>();
        if (myCol != null) myCol.enabled = false;
    }
}