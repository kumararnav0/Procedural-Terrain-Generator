using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;  // Reference to the pause menu UI panel
    public CustomTerrain customTerrain;  // Reference to the CustomTerrain component

    private bool isPaused = false;

    void Start()
    {
        // Ensure the pause menu is hidden when the game starts
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;  // Ensure the game time is running
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))  // Check if Escape key is pressed
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);  // Hide the pause menu
        Time.timeScale = 1f;  // Resume game time
        isPaused = false;
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);  // Show the pause menu
        Time.timeScale = 0f;  // Pause game time
        isPaused = true;
    }

    public void ResetTerrain()
    {
        if (customTerrain != null)
        {
            customTerrain.ResetTerrain();  // Call the ResetTerrain method from CustomTerrain script
        }
    }
}