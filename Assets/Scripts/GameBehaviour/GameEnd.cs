using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameEnd : MonoBehaviour
{
    public float fadeDuration = 1f;
    public float displayImageDuration = 5f;
    public CanvasGroup exitBackgroundImageCanvasGroup; 
    public CanvasGroup caughtBackgroundImageCanvasGroup;

    public bool m_IsPlayerAtExit;
    public bool m_IsPlayerCaught;
    float m_Timer;
    private int timesSeen = 0;

    public Camera camera;
    public Image image;
    private Texture2D tex;
    private bool end = false;
    private bool simplecheck = true;
    public TextMeshProUGUI text;
    

    
    public void CaughtPlayer ()
    {
        m_IsPlayerCaught = true;
    }

    void FixedUpdate ()
    {
        if (m_IsPlayerAtExit)
        {
            if (simplecheck)
            {
                text.text += timesSeen;
                simplecheck = false;
            }
            EndLevel (exitBackgroundImageCanvasGroup, false);
        }
        else if (m_IsPlayerCaught)
        {
            //Debug.LogWarning("debug");
            EndLevel (caughtBackgroundImageCanvasGroup, true);
        }
    }
    
    
    

    void EndLevel (CanvasGroup imageCanvasGroup, bool doRestart)
    {
        if (!end)
        {
            tex = camera.RTImage();  
            image.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
            end = true;
        }

        m_Timer += Time.deltaTime;
        imageCanvasGroup.alpha = m_Timer / fadeDuration;

        if (m_Timer >  displayImageDuration)
        {
            imageCanvasGroup.alpha = 0;
            if (doRestart)
            {
                end = false;
                m_IsPlayerCaught = false;
                m_Timer = 0;
                timesSeen++;
                //SceneManager.LoadScene (0);
            }
            else
            {
                Application.Quit ();
            }
        }
    }
}
