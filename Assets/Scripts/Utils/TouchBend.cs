using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TouchBend : MonoBehaviour {


    public Material mat;
    public string propertyName = "Player_Position";
    public Transform player;
    // teste

    private float hitTime;

    

	void Start () {
        //player = GameObject.FindGameObjectWithTag("Player");
        //mat = GetComponent<Material>();

	}
	
	// Update is called once per frame
	void Update () {
     

        if (player != null)
        {
            mat.SetVector(propertyName, player.position);
            
        }
        else
            Debug.Log("player not found");



        
        
	}

    
}
