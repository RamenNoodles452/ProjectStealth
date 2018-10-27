using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Talk UI
// - Gabriel Violette
public class UIDialogue : MonoBehaviour
{
    #region vars
    public UnityEngine.UI.Text character;
    public UnityEngine.UI.Text text;
    public UnityEngine.UI.Image character_background;
    public UnityEngine.UI.Image text_background;

    private Dialogue dialogue;
    private LineOfDialogue current_line;
    private float timer;
    private float duration;
    private const float HIDE_TIME = 0.05f;
    private bool is_BG_hidden = false;
    private bool is_showing_line = false;
    #endregion

    // Use this for initialization
    void Start ()
    {
        dialogue = Dialogue.Load( "scene.xml" );
        NextLine(); // first line
    }

    // Update is called once per frame
    void Update()
    {
        if ( current_line == null )
        {
            HideLine();
            HideBG();
            this.gameObject.SetActive( false ); // disable canvas obj to increase performance.
            return;
        }

        timer += Time.deltaTime * Time.timeScale;

        if ( timer >= HIDE_TIME && ! is_showing_line ) { ShowLine(); }
        if ( timer >= duration ) { NextLine(); }
    }

    /// <summary>
    /// Gets and starts the display process for the next line of text.
    /// </summary>
    private void NextLine()
    { 
        current_line = dialogue.NextLine();
        if ( current_line != null )
        {
            duration = current_line.Duration();

            if ( current_line.text == null && current_line.character == null )
            {
                HideBG();
            }
        }
        else
        {
            duration = 0.0f;
        }
        timer = 0.0f;
        HideLine(); // UX: hiding the line for a TINY amount of time makes the change register to users.
    }

    /// <summary>
    /// Hides the text
    /// </summary>
    private void HideLine()
    {
        is_showing_line = false;
        character.gameObject.SetActive( false );
        text.gameObject.SetActive( false );
    }

    /// <summary>
    /// Shows the text
    /// </summary>
    private void ShowLine()
    {
        is_showing_line = true;
        if ( current_line == null ) { return; }

        if ( is_BG_hidden && ( current_line.text != null || current_line.character != null ) ) { ShowBG(); }

        character.gameObject.SetActive( true );
        text.gameObject.SetActive( true );

        if ( current_line.character != null ) { character.text = current_line.character + ":"; }
        else { character.text = ""; }
        text.text = current_line.text;
    }

    /// <summary>
    /// Hides the background of the UI.
    /// </summary>
    private void HideBG()
    {
        is_BG_hidden = true;
        character_background.gameObject.SetActive( false );
        text_background.gameObject.SetActive( false );
    }

    /// <summary>
    /// Shows the background of the UI.
    /// </summary>
    private void ShowBG()
    {
        is_BG_hidden = false;
        character_background.gameObject.SetActive( true );
        text_background.gameObject.SetActive( true );
    }
}
