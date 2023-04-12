using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuCamera : MonoBehaviour
{
    // creating an List
    private List<GameObject> cameraMenuPosition = new List<GameObject>();



    // Start is called before the first frame update
    public GameObject gameStartPosition;
    public GameObject charSelectPosition;

    private bool reached_GameStartPosition;
    private bool reached_CharacterSelectPosition=true;
    private bool canClick;
    private bool backToMainMenu;
    private void Awake()
    {
        cameraMenuPosition.Add(gameStartPosition); // as list is initiated and we add a value in it

    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       // MoveToGameStartedPosition();
       // MovetoCharacterSelectMenu();
       // MoveBackToMainMenu();
        MoveToMenuPosition();
    }
     void MoveToMenuPosition()
    {
        if( cameraMenuPosition.Count>0)
        {
            transform.position = Vector3.Lerp(transform.position, cameraMenuPosition[0].transform.position,1f* Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, cameraMenuPosition[0].transform.rotation, 1f * Time.deltaTime);
        }
    }

    public void ChangeCameraPosition(int index)
    {
        cameraMenuPosition.RemoveAt(0);
        if (index == 0)
        {
            cameraMenuPosition.Add(gameStartPosition);
        }
        else
        {
            cameraMenuPosition.Add(charSelectPosition);
        }
        
    }



     void MoveToGameStartedPosition() {

        if (!reached_GameStartPosition) { 
        
            if(Vector3.Distance(transform.position , gameStartPosition.transform.position)<0.2f)
            {
                reached_GameStartPosition = true;
                canClick = true;
            }
        
        
        }
        if (!reached_GameStartPosition)
        {
            transform.position = Vector3.Lerp(transform.position, gameStartPosition.transform.position, 1f * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, gameStartPosition.transform.rotation, 1f * Time.deltaTime);
        }
        
    }

    void MovetoCharacterSelectMenu()
    {
        if (!reached_CharacterSelectPosition)
        {
            if (Vector3.Distance(transform.position, charSelectPosition.transform.position) < 0.2f)
            {
                reached_CharacterSelectPosition = true;
                canClick = true;
            }
        }

        if (!reached_CharacterSelectPosition)
        {
            transform.position = Vector3.Lerp(transform.position, charSelectPosition.transform.position, 1f * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, charSelectPosition.transform.rotation, 1f * Time.deltaTime);
        }
        
        
    }

    void MoveBackToMainMenu()
    {
        if (backToMainMenu)
        {
            if (Vector3.Distance(transform.position, gameStartPosition.transform.position) < 0.2f)
            {
                backToMainMenu = false;
                canClick = true;
            }
        }

        if(backToMainMenu)
        {
            transform.position = Vector3.Lerp(transform.position, gameStartPosition.transform.position, 1f * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, gameStartPosition.transform.rotation, 1f * Time.deltaTime);
        }

    }

    public bool ReachedCharacterSelectPosition
    {
        get { return reached_CharacterSelectPosition; }
        set { reached_CharacterSelectPosition = value; }
    }

    public bool CanClick
    {
        get { return canClick ; }
        set { canClick = value; }
    }
    public bool BackToMainMenu
    {
        get { return backToMainMenu; }
        set { backToMainMenu = value; }
    }
}
