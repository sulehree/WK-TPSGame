using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField]
    private GameObject[] characters;
    
    [HideInInspector]
    public int selectedCharacter;
    // Start is called before the first frame update

    private void Awake()
    {
        MakeSingleton();
    }
   
    private void OnEnable()
    {
        SceneManager.sceneLoaded += LevelFinishLoading;   // here we are attaching a method to the Event method is sceneloaded and the method is levelfinishloading
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= LevelFinishLoading;  // here we are dettaching a method from the Event method is sceneloaded and the method is levelfinishloading
    }
    void LevelFinishLoading( Scene scene,LoadSceneMode mode)
    {
        if(scene.name!="MainMenu")
        {
            Vector3 pos = GameObject.FindGameObjectWithTag("SpawnPoint").transform.position;
            Instantiate(characters[selectedCharacter], pos, Quaternion.identity);
        
        }

    }

    void MakeSingleton()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
