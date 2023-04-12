using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateCharacter : MonoBehaviour
{
    public GameObject[] characters;
    public GameObject characterPosition;

    private int sharpShooterOneIndex = 0;
    private int sharpShooterTwoIndex = 1;
    private int sharpShooterThreeIndex = 2;
    // Start is called before the first frame update
    void Start()
    {
        characters[sharpShooterOneIndex].SetActive(true);
        characters[sharpShooterOneIndex].transform.position = characterPosition.transform.position;

            }

    public void SeleectCharacter() {

        int index = int.Parse(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name);
        TurnOffCharacters();
        characters[index].SetActive(true);
        characters[index].transform.position = characterPosition.transform.position;
    }

    private void TurnOffCharacters()
    {
        for(int counter=0; counter<characters.Length; counter++)
        {
            characters[counter].SetActive(false);
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
