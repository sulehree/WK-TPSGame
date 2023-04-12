using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    public GameObject buttonPanel, characterSelectPanel, characterCreatePanel ;
    private MainMenuCamera mainMenuCamera;
    // Start is called before the first frame update
    private void Awake()
    {
        mainMenuCamera = Camera.main.GetComponent<MainMenuCamera>();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

     public void PlayGame()
    {
        // mainMenuCamera.CanClick = false;
        // mainMenuCamera.ReachedCharacterSelectPosition = false;

        mainMenuCamera.ChangeCameraPosition(1);
        // Panel hide and Show
        buttonPanel.SetActive(false);
        characterSelectPanel.SetActive(true);

    }
     public void BackToMainMenu()
    {

        mainMenuCamera.ChangeCameraPosition(0);
        buttonPanel.SetActive(true);
        characterSelectPanel.SetActive(false);


        //if (mainMenuCamera.CanClick)
        //{
        //    mainMenuCamera.CanClick = false;
        //    mainMenuCamera.BackToMainMenu = true;

        //    // Panel Hide and Show
        //    buttonPanel.SetActive(true);
        //    characterSelectPanel.SetActive(false);
        //}
    }

    public void CreateCharacter()
    {
        characterSelectPanel.SetActive(false);
        characterCreatePanel.SetActive(true);
        
    }

    public void Accept()
    {
        characterSelectPanel.SetActive(true);
        characterCreatePanel.SetActive(false);
    }
    public void Cancel()
    {
        characterSelectPanel.SetActive(true);
        characterCreatePanel.SetActive(false);
    }


}
