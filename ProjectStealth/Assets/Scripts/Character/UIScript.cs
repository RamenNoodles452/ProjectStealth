using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    // TODO: should probably be non-destroyable, instantiated on load

    // Use this for initialization
    void Start()
    {
        player = Referencer.Instance.player; //GameObject.Find("PlayerCharacter").GetComponent<Player>(); //bad
    }

    // Update is called once per frame
    void Update()
    {
        shieldUI.GetComponent<Text>().text = "Shields: " + (int)player.GetShields() + " / " + (int)player.GetShieldsMax();
        energyUI.GetComponent<Text>().text = "Energy: "  + (int)player.GetEnergy() + " / " + (int)player.GetEnergyMax();

        if ( player.IsCloaking() )
        {
            cloakIcon.GetComponent<Text>().enabled = true;
        }
        else
        {
            cloakIcon.GetComponent<Text>().enabled = false;
        }

        if ( player.IsEvading())
        {
            evadeIcon.GetComponent<Text>().enabled = true;
        }
        else
        {
            evadeIcon.GetComponent<Text>().enabled = false;
        }
    }
}
