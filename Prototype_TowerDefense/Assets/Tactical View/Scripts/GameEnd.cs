using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

/// Ends the game when a main tower is destroyed:
/// enemy main tower falls -> victory, your own main tower falls -> defeat.
public class GameEnd : MonoBehaviour
{
    [Header("Victory")]
    [Tooltip("The enemy main tower in the scene (Enemy Main)")]
    [FormerlySerializedAs("mainTower")]   // the field used to be called mainTower: keeps what you already dragged in
    public Health enemyMainTower;
    [Tooltip("UI panel shown on victory (switched off at start)")]
    public GameObject victoryScreen;

    [Header("Defeat")]
    [Tooltip("Your own main tower in the scene (Ally Main)")]
    public Health playerMainTower;
    [Tooltip("UI panel shown on defeat (switched off at start)")]
    public GameObject defeatScreen;

    bool gameOver;   // only the first result counts

    void Start()
    {
        victoryScreen.SetActive(false);
        defeatScreen.SetActive(false);
        enemyMainTower.OnDeath += Victory;    // Health raises OnDeath when it reaches 0
        playerMainTower.OnDeath += Defeat;
    }

    void OnDestroy()
    {
        // always unsubscribe what you subscribed
        if (enemyMainTower) enemyMainTower.OnDeath -= Victory;
        if (playerMainTower) playerMainTower.OnDeath -= Defeat;
    }

    void Victory() => EndGame(victoryScreen);
    void Defeat() => EndGame(defeatScreen);

    void EndGame(GameObject screen)
    {
        if (gameOver) return;
        gameOver = true;

        // Close the tactical map and stop TAB from opening it again
        TacticalMapView map = FindAnyObjectByType<TacticalMapView>();
        if (map) { map.SetOpen(false); map.enabled = false; }

        // Stop looking around and free the mouse so the button can be clicked
        MouseMovement look = FindAnyObjectByType<MouseMovement>();
        if (look) look.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;            // freezes everything that uses Time.deltaTime: units, towers, recruit timers
        screen.SetActive(true);
    }

    /// For the "Restart" buttons on both screens
    public void Restart()
    {
        Time.timeScale = 1f;   // timeScale survives loading a scene, so reset it first
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
