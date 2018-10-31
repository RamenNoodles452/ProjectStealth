using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

// Reads and rolls credits
public class Credits : MonoBehaviour
{
    #region vars
    [SerializeField]
    private GameObject text_prefab;
    private CreditData credit_data;
    float timer;
    #endregion
    // Use for pre-initialization
    private void Awake()
    {
        string path = System.IO.Path.Combine( Application.dataPath, "Scripts/credits.xml" );
        FileStream stream = new FileStream( path, FileMode.Open );
        XmlSerializer serializer = new XmlSerializer( typeof( CreditData ) );
        credit_data = serializer.Deserialize( stream ) as CreditData;
        stream.Close();

        float y = 0;
        foreach ( CreditDataLine line in credit_data.content )
        {
            GameObject text_obj = Instantiate( text_prefab, this.gameObject.transform );
            Text text = text_obj.GetComponent<Text>();
            text.text = line.text;

            float x_offset = 0.0f; //32.0f;
            float y_increment = 24.0f;
            int font_size = 22;
            Color outline_color = new Color( 0.0f, 0.5f, 1.0f, 0.25f );
            if ( line.heading == 1 )
            {
                //x_offset = 0.0f;
                font_size = 30;
                y_increment = 32.0f;
                outline_color = new Color( 1.0f, 1.0f, 0.0f, 0.5f );
            }
            else if ( line.heading == 2 )
            {
                //x_offset = 16.0f;
                font_size = 26;
                y_increment = 28.0f;
                outline_color = new Color( 1.0f, 0.0f, 0.5f, 0.35f );
            }

            text.fontSize = font_size;
            text.rectTransform.sizeDelta = new Vector2( text.rectTransform.sizeDelta.x, y_increment );
            text.rectTransform.position = new Vector3( text.rectTransform.position.x + x_offset, y, 0.0f );
            text.GetComponent<Outline>().effectColor = outline_color;
            y -= y_increment;
        }
    }

    // Use this for initialization
    void Start ()
    {
        
    }
    
    // Update is called once per frame
    void Update ()
    {
        // Scroll
        float scroll_speed = 48.0f;
        float t = Time.deltaTime * Time.timeScale;
        timer += t;
        for ( int i = 0; i < transform.childCount; i++ )
        {
            transform.GetChild( i ).GetComponent<Text>().rectTransform.position += new Vector3( 0.0f, t * scroll_speed, 0.0f );
        }

        // Credits are done?
        if ( t > 30.0f )
        {
            Application.Quit();
        }
    }
}

[XmlRoot( "credits" )]
public class CreditData
{
    #region vars
    //[XmlAttribute]
    //public int header; // 1 = heading, 2 = subheading
    [XmlArray( "lines" )]
    [XmlArrayItem( "text" )]
    public CreditDataLine[] content;
    #endregion
}

public class CreditDataLine
{
    #region vars
    [XmlAttribute( "header" )]
    public int heading;
    [XmlText]
    public string text;
    #endregion
}
