using UnityEngine;
using System.Collections;

public class CharacterStatus : ScriptableObject
{

    public int Health;

    public void Clone(CharacterStatus status)
    {
        this.Health = status.Health;
    }
}
