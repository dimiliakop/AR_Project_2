using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void LoadMainMenuScene()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    public void LoadGameScene()
    {
        SceneManager.LoadScene("GameScene"); 
    }

    public void LoadCloudAnchorsScene()
    {
        SceneManager.LoadScene("CloudAnchorsScene"); 
    }    
    
    public void LoadCloudAnchorResolverScene()
    {
        SceneManager.LoadScene("CloudAnchorResolverScene"); 
    }

    public void LoadFaceMaskingScene()
    {
        SceneManager.LoadScene("FaceMaskingScene");
    }
}
