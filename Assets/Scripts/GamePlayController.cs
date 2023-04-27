using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayController : MonoBehaviour
{
   public void LoadOtherLevels()
    {
        string name = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name; // This will give the name of object that will be currently selected;
        SceneLoader.instance.LoadLevel(name);
        
 
 
    }
}
