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

    [Header("Coins")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField, Range(0f, 1f)] private float coinSpawnChance = 0.7f;
    [SerializeField] private float coinHorizontalMargin = 0.3f;
    [SerializeField] private float coinVerticalOffset = 0.6f;

    [SerializeField] private GameObject winPanel;
    [SerializeField] private bool pauseOnWin = true;
    [SerializeField] private float winShowDelay = 0.5f;

    private bool hasWon = false;

    // Coin tracking
    private int totalCoins = 0;
    private int collectedCoins = 0;

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

        // Spawn coin only on non-last platforms according to chance
        if (!isLast && coinPrefab != null && Random.value <= coinSpawnChance)
        {
            // Determine platform world bounds using collider if present, otherwise fallback to transform & scale
            Collider2D platCol = platform.GetComponent<Collider2D>() ?? platform.GetComponentInChildren<Collider2D>();
            float platLeftWorld, platRightWorld, platTopWorld;

            if (platCol != null)
            {
                Bounds pBounds = platCol.bounds;
                platLeftWorld = pBounds.min.x;
                platRightWorld = pBounds.max.x;
                platTopWorld = pBounds.max.y;
            }
            else
            {
                float halfWidth = Mathf.Abs(width) * 0.5f;
                platLeftWorld = platform.transform.position.x - halfWidth;
                platRightWorld = platform.transform.position.x + halfWidth;
                platTopWorld = platform.transform.position.y + (height * 0.5f);
            }

            // Ensure margins do not invert range
            float minX = platLeftWorld + coinHorizontalMargin;
            float maxX = platRightWorld - coinHorizontalMargin;
            if (maxX <= minX)
            {
                // fallback to center if platform too narrow for margin
                minX = (platLeftWorld + platRightWorld) * 0.5f;
                maxX = minX;
            }

            float spawnX = Random.Range(minX, maxX);
            float spawnY = platTopWorld + coinVerticalOffset;

            // Instantiate coin in world space, preserve its world scale, then parent and compensate for platform scale
            GameObject coin = Instantiate(coinPrefab, new Vector3(spawnX, spawnY, 0f), Quaternion.identity);
            Vector3 origWorldScale = coin.transform.lossyScale;

            // Make sure collider is trigger if present
            Collider2D coinCol = coin.GetComponent<Collider2D>() ?? coin.GetComponentInChildren<Collider2D>();
            if (coinCol != null) coinCol.isTrigger = true;

            // Parent to platform but keep world position. Then adjust localScale so world scale equals original.
            coin.transform.SetParent(platform.transform, true);
            Vector3 pLossy = platform.transform.lossyScale;
            coin.transform.localScale = new Vector3(
                pLossy.x != 0f ? origWorldScale.x / pLossy.x : origWorldScale.x,
                pLossy.y != 0f ? origWorldScale.y / pLossy.y : origWorldScale.y,
                pLossy.z != 0f ? origWorldScale.z / pLossy.z : origWorldScale.z
            );

            // Ensure coin has Coin component and wire up the generator
            Coin c = coin.GetComponent<Coin>();
            if (c != null)
            {
                c.generator = this;
            }

            totalCoins++;
        }

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

    // Called by Coin when collected
    public void RegisterCoinCollected()
    {
        collectedCoins = Mathf.Min(totalCoins, collectedCoins + 1);
    }

    // Properties for WinTrigger checks
    public int TotalCoins => totalCoins;
    public int CollectedCoins => collectedCoins;

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

        // Require all coins collected before allowing win
        if (generator.TotalCoins > 0 && generator.CollectedCoins < generator.TotalCoins)
        {
            // Not all coins collected yet — do not trigger win.
            return;
        }

        generator.ShowWin();
        Collider2D myCol = GetComponent<Collider2D>();
        if (myCol != null) myCol.enabled = false;
    }
} // end of WinTrigger class

// Simple menu helper: attach this to a GameObject in your Menu scene (e.g. an empty "UIManager").
// Configure gameSceneName to the name of your gameplay Scene (the scene that should open when Play is pressed).
public class MenuController : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "GameScene"; // set this in Inspector to your gameplay scene name

    // Hook this method to your Play Button's OnClick() in the Menu scene.
    public void Play()
    {
        // Ensure timeScale is normal when entering gameplay
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogWarning("MenuController.Play called but gameSceneName is empty.");
        }
    }

    // Optional: hook to a Quit button
    public void Quit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}