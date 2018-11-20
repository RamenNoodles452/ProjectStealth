using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

// XML dialogue parser
// - Gabriel Violette

[XmlRoot( "scene" )]
public class Dialogue
{
    #region vars
    [XmlArray( "script" ),XmlArrayItem( "line" )]
    public List<LineOfDialogue> lines = new List<LineOfDialogue>();
    public int current_line;
    #endregion

    public Dialogue()
    {
        current_line = -1;
    }

    /// <summary>
    /// Loads dialogue from a file
    /// </summary>
    /// <param name="file_name">The name of the file (remember to append .xml).</param>
    /// <returns>A dialogue object with all the lines from the file.</returns>
    public static Dialogue Load( string file_name )
    {
        string path = System.IO.Path.Combine( System.IO.Path.Combine( Application.dataPath, "Talk" ), file_name );
        XmlSerializer serializer = new XmlSerializer( typeof( Dialogue ) );
        FileStream stream = new FileStream( path, FileMode.Open );
        Dialogue temp = serializer.Deserialize( stream ) as Dialogue;
        stream.Close();
        return temp;
    }

    /// <summary>
    /// API for getting a line of dialogue.
    /// </summary>
    /// <returns>The line of dialogue, or NULL if the conversation is OVER.</returns>
    public LineOfDialogue NextLine()
    {
        current_line++;
        if ( current_line > lines.Count - 1 ) { return null; }
        return lines[ current_line ];
    }
}

// Leightweight representation of a single line
public class LineOfDialogue
{
    #region vars
    [XmlAttribute( "character" )]
    public string character;
    [XmlText]
    public string text;
    //public string sound; // for when we do fully-voice acted?
    [XmlAttribute( "duration" )]
    public float duration; // for synchronizing
    #endregion

    public LineOfDialogue()
    {
    }

    public float Duration()
    {
        if ( duration > 0.0f ) { return duration; }
        // default: 5 char words @ 200 words per minute + buffer
        return ( text.Length / ( 5.0f * 200.0f ) * 60.0f )  + 1.0f;
    }
}
