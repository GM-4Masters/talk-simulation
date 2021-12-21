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

    public Text stageTxt;
    public Image bannerImg;
    public Image endingImg;
    public Text endingTxt;

    Vector3 canvasWorldPointWH;
    Vector3 canvasWorldPointZero;

    private Rect screen;

    private string[] endingTitle = { "#4. Good Ending", "#1. Bad Ending", "#2. Bad Ending", "#3. Bad Ending" };



    private RectTransform contentRT;
    private ScrollRect scrollRect;
    float contentHeight;
    float scrollViewHeight;
    float offset = 0f;
    Vector2 contentAnchoredPos;
    bool isScrollActive = false;


    public void Start()
    {
        // Grab the tranform position as a world point
        // First zero then the width and height
        canvasWorldPointZero = canvas.worldCamera.ScreenToWorldPoint(Vector3.zero);
        canvasWorldPointWH = canvas.worldCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height));

        // Draw our rectangle. First the position from canvasWorldPointZero then to the width and height.
        screen = new Rect(canvasWorldPointZero, new Vector2(canvasWorldPointWH.x - canvasWorldPointZero.x, canvasWorldPointWH.y - canvasWorldPointZero.y));

        //StartCoroutine("PauseBeforeStart");
        //StartCoroutine("ScrollTxt");




        //// 테스트
        //SetEnding("Stage Test", "Ending0", "엔딩입니다");
    }

    private void OnEnable()
    {
        int endingIndex = (int)GameManager.Instance.Ending;
        //Debug.Log("ending:" + DataManager.Instance.endingList[endingIndex]);
        SetEnding(endingTitle[endingIndex], "Ending" + endingIndex, DataManager.Instance.endingList[endingIndex]);


        contentRT = textToScroll.transform.parent.GetComponent<RectTransform>();
        scrollRect = contentRT.transform.parent.parent.GetComponent<ScrollRect>();
        contentHeight = textToScroll.GetComponent<Text>().preferredHeight;
        scrollViewHeight = scrollRect.GetComponent<RectTransform>().rect.height;
        contentAnchoredPos = contentRT.anchoredPosition;
        contentRT.anchoredPosition = new Vector2(contentAnchoredPos.x, offset);
        scrollSpeed = 0.5f;
    }

    private void Update()
    {
        // 엔딩 길이에 따라 스크롤 종료지점 지정
        float destPos = ((contentHeight < scrollViewHeight) ? scrollViewHeight:contentHeight);
        if (offset < destPos)
        {
            offset += (Time.deltaTime * scrollSpeed * 100);
            contentRT.anchoredPosition = new Vector2(contentAnchoredPos.x, offset);
        }
        else if (!isScrollActive)
        {
            scrollRect.vertical = true;
            isScrollActive = true;
        }
    }

    public void SetEnding(string _stageTxt, string _endingImg, string _endingTxt)
    {
        stageTxt.text = _stageTxt;
        endingImg.sprite = Resources.Load<Sprite>("Sprites/Ending/" + _endingImg);
        endingTxt.text = _endingTxt;
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
    }

    public void GoMain()
    {
        GameManager.Instance.ChangeScene(GameManager.SCENE.MAIN);
        GameManager.Instance.ResetGame();
    }
}
