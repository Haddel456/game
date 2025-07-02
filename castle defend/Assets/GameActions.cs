using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameActions : MonoBehaviour
{
    // Start is called before the first frame update
    public void QuitGame()
    {
        Application.Quit();
    }


    // Update is called once per frame
    public void PlayGame()
    {
        //SceneManager.LoadSceneAsync(0);
        SceneManager.LoadSceneAsync("Game");
    }

}
