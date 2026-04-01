using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagement : SingletonMonoBehaviour<SceneManagement>
{
    protected override void Awake()
    {
        base.Awake();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameManager.Instance.OnSceneLoadedCheck();
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    
    public void LoadNextScene()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentIndex + 1);
    }

    private void OnDestroy()
    {   
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
