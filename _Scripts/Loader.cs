using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


public class Loader : MonoBehaviour {

    public GameObject gameManager;
	public static GameManager instance;				//Static instance of GameManager which allows it to be accessed by any other script.

    // Use this for initialization
    void Awake () {
       /* if (GameManager.instance == null)
            Instantiate(gameManager);*/		         
    }

	void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode){	
		if(GameManager.instance==null){ Instantiate (gameManager); }
	}
	void OnEnable(){
		SceneManager.sceneLoaded += OnLevelFinishedLoading;
	}
	void OnDisable(){	
		SceneManager.sceneLoaded -= OnLevelFinishedLoading;
	}

}//end class Loader.
