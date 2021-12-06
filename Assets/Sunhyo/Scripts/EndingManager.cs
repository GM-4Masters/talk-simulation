using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndingManager : MonoBehaviour
{
    public bool repeat;
    public float scrollSpeed;
    public GameObject textToScroll;
    public Canvas canvas;
    public Text endingTxt;
    public GameObject end;

    Vector3 canvasWorldPointWH;

    private Rect screen;

    public void Start()
    {
        // Grab the tranform position as a world point
        // First zero then the width and height
        Vector3 canvasWorldPointZero = canvas.worldCamera.ScreenToWorldPoint(Vector3.zero);
        canvasWorldPointWH = canvas.worldCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height));

        // Draw our rectangle. First the position from canvasWorldPointZero then to the width and height.
        screen = new Rect(canvasWorldPointZero, new Vector2(canvasWorldPointWH.x - canvasWorldPointZero.x, canvasWorldPointWH.y - canvasWorldPointZero.y));

        // StartCoroutine("PauseBeforeStart");
        StartCoroutine("ScrollTxt");
    }

    public void Update()
    {
        //Create an array of four values to store our text corners
        Vector3[] wc = new Vector3[4];

        // Grab the corners of our text rect tranform. 
        textToScroll.GetComponent<RectTransform>().GetWorldCorners(wc);

        // Create a rectangle based on our text to scroll game object
        // the same as we did above
        Rect rect = new Rect(wc[0].x, wc[0].y, wc[2].x - wc[0].x, wc[2].y - wc[0].y);


        // Check if it overlaps the canvas rect using the overlap function
        if(rect.Overlaps(screen))
        {
            // Move the text up
            textToScroll.transform.Translate(Vector3.up * (scrollSpeed * Time.deltaTime));
        }
        
        if(rect.y >= canvasWorldPointWH.y)
            print("³¡");
    }

    public void GetEnding(string text)
    {
        endingTxt.text = text;
    }

    IEnumerator PauseBeforeStart()
    {
        yield return new WaitForSeconds(3.0f);
        scrollSpeed = 0.5f;
    }

    IEnumerator ScrollTxt()
    {
        while(true)
        {
            //Create an array of four values to store our text corners
            Vector3[] wc = new Vector3[4];

            // Grab the corners of our text rect tranform. 
            textToScroll.GetComponent<RectTransform>().GetWorldCorners(wc);

            // Create a rectangle based on our text to scroll game object
            // the same as we did above
            Rect rect = new Rect(wc[0].x, wc[0].y, wc[2].x - wc[0].x, wc[2].y - wc[0].y);


            // Check if it overlaps the canvas rect using the overlap function
            if(rect.Overlaps(screen))
            {
                // Move the text up
                textToScroll.transform.Translate(Vector3.up * (scrollSpeed * Time.deltaTime));
            }
            
            if(rect.y >= canvasWorldPointWH.y)
                break;

            yield return null;
        }

        End();
    }

    void End()
    {
        end.SetActive(true);
    }
}
