using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagement : SingletonMonoBehaviour<SceneManagement>
{
    private bool isTuTorial = false;
    public string CurrentSceneName { get; private set; }
    
    protected override void Awake()
    {
        base.Awake();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CurrentSceneName = scene.name;
        DialogueTest.Instance.CreateTextBox();
        GameManager.Instance.OnSceneLoadedCheck();
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    
    public void LoadNextScene()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        if (currentIndex == 4) currentIndex = -1;
        SceneManager.LoadScene(currentIndex + 1);
    }

    private void OnDestroy()
    {   
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
