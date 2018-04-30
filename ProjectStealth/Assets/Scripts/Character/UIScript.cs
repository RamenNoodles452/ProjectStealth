using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// - GabeV
// Player data tracking UI script
public class UIScript : MonoBehaviour
{
    #region vars
    private Player player;

    public GameObject shield_ui;
    public GameObject energy_ui;
    public GameObject cloak_icon;
    public GameObject evade_icon;

    public bool hide_all;
    #endregion

    // TODO: should probably be non-destroyable, instantiated on load

    // Use this for initialization
    void Start()
    {
        player = Referencer.instance.player; //GameObject.Find("PlayerCharacter").GetComponent<Player>(); //bad
    }

    // Update is called once per frame
    void Update()
    {
        shield_ui.GetComponent<Text>().text = "Shields: " + (int)player.GetShields() + " / " + (int)player.GetShieldsMax();
        energy_ui.GetComponent<Text>().text = "Energy: "  + (int)player.GetEnergy() + " / " + (int)player.GetEnergyMax();

        if ( player.IsCloaking() )
        {
            cloak_icon.GetComponent<Text>().enabled = true;
        }
        else
        {
            cloak_icon.GetComponent<Text>().enabled = false;
        }

        if ( player.IsEvading())
        {
            evade_icon.GetComponent<Text>().enabled = true;
        }
        else
        {
            evade_icon.GetComponent<Text>().enabled = false;
        }
    }
}
