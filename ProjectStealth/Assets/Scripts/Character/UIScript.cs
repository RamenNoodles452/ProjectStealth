using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Player data tracking UI script
public class UIScript : MonoBehaviour
{
    #region vars
    private Player player;

    public GameObject shieldUI;
    public GameObject energyUI;
    public GameObject cloakIcon;
    public GameObject evadeIcon;

    public bool hideAll;
    #endregion

    // Use this for initialization
    void Start ()
    {
        player = GameObject.Find("PlayerCharacter").GetComponent<Player>();
	}
	
	// Update is called once per frame
	void Update ()
    {
		if ( player.IsCloaking() )
        {
            //cloakIcon.GetComponent<Text>();
        }
	}
}
