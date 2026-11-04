
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeath : MonoBehaviour
{
    [Header("Player Health")]
    public int maxHealth = 3;

    [Header("Animations")]
    public Animator animator;
    public float deathAnimationDuration = 2f;

    private int currentHealth;
    private bool isDead = false;
    private bool showGameOver = false;

    public bool IsDead
    {
        get { return isDead; }
    }

    void Awake()
    {
        currentHealth = maxHealth;

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    // Called whenever a zombie hits the wizard.
    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth = Mathf.Max(
            0,
            currentHealth - damage
        );

        Debug.Log(
            "Wizard Health: " +
            currentHealth + "/" + maxHealth
        );

        if (currentHealth > 0)
        {
            PlayHitAnimation();
        }
        else
        {
            Die();
        }
    }

    void PlayHitAnimation()
    {
        if (animator == null)
            return;

        if (animator.HasState(
            0,
            Animator.StringToHash("Base Layer.HitBody")
        ))
        {
            animator.CrossFade(
                "HitBody",
                0.05f,
                0,
                0f
            );
        }
        else
        {
            Debug.LogWarning(
                "HitBody state missing in WizardAnimator!"
            );
        }
    }

    public void Die()
    {
        if (isDead)
            return;

        isDead = true;
        currentHealth = 0;

        Debug.Log("Wizard died! Playing Death animation.");

        // Stop the player from moving.
        ThirdPersonAvatarController movement =
            GetComponent<ThirdPersonAvatarController>();

        if (movement != null)
            movement.enabled = false;

        // Prevent other animation conditions from
        // interrupting the Death animation.
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsJumping", false);
            animator.SetBool("IsSwimming", false);
            animator.SetBool("IsSwimMoving", false);

            animator.CrossFade(
                "Death",
                0.05f,
                0,
                0f
            );
        }

        StartCoroutine(FinishDeath());
    }

    IEnumerator FinishDeath()
    {
        // Let the wizard's death animation finish.
        yield return new WaitForSecondsRealtime(
            deathAnimationDuration
        );

        // Stop the camera's mouse controls.
        ThirdPersonCamera cameraController =
            FindFirstObjectByType<ThirdPersonCamera>();

        if (cameraController != null)
            cameraController.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        showGameOver = true;

        // Freeze gameplay after the animation.
        Time.timeScale = 0f;
    }

    void OnGUI()
    {
        GUIStyle healthStyle =
            new GUIStyle(GUI.skin.box);

        healthStyle.fontSize = 22;
        healthStyle.fontStyle = FontStyle.Bold;
        healthStyle.alignment = TextAnchor.MiddleCenter;
        healthStyle.normal.textColor = Color.white;

        GUI.Box(
            new Rect(20, 20, 180, 50),
            "HEALTH: " + currentHealth + "/" + maxHealth,
            healthStyle
        );

        if (!showGameOver)
            return;

        float x = Screen.width / 2f;
        float y = Screen.height / 2f;

        GUI.Box(
            new Rect(x - 200, y - 110, 400, 220),
            ""
        );

        GUIStyle titleStyle =
            new GUIStyle(GUI.skin.label);

        titleStyle.fontSize = 36;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = Color.red;

        GUI.Label(
            new Rect(x - 180, y - 85, 360, 60),
            "GAME OVER",
            titleStyle
        );

        GUI.Label(
            new Rect(x - 130, y - 20, 260, 30),
            "The zombies caught you!"
        );

        if (GUI.Button(
            new Rect(x - 90, y + 35, 180, 45),
            "RESTART"
        ))
        {
            Time.timeScale = 1f;

            SceneManager.LoadScene(
                SceneManager.GetActiveScene().buildIndex
            );
        }
    }

    void OnDisable()
    {
        if (isDead)
            Time.timeScale = 1f;
    }
}
