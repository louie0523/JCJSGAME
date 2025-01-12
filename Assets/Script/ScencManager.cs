using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScencManager : MonoBehaviour
{
    public string SceneName;

    public void SceneLoadBtn()
    {
        SceneManager.LoadScene(SceneName);
    }

    public void EixtBtn()
    {
        Application.Quit();
    }
}
