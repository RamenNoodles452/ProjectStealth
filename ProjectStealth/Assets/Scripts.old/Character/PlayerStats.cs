using UnityEngine;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    // random values
    public int Health;

    // progress values
	public bool AquiredMagGrip;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (AquiredMagGrip)
                AquiredMagGrip = false;
            else
                AquiredMagGrip = true;
        }
    }
	
}
