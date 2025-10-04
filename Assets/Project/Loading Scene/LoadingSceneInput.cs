using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingSceneInput : MonoBehaviour
{
    private void Update()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if(scene.name == "Loading Scene 1")
                LoadingSceneController.Instance.LoadScene("Loading Scene 2");
            else if(scene.name == "Loading Scene 2")
                LoadingSceneController.Instance.LoadScene("Loading Scene 1");
            else
                Debug.Log("이름이 잘못 되었습니다.");
        }
    }
}