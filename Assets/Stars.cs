using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Stars : MonoBehaviour
{
    [SerializeField] GameObject star;
    [SerializeField] int numStars;
    [SerializeField] Vector2 topLeft;
    [SerializeField] Vector2 bottomRight;
    [SerializeField] float expandTime;
    [SerializeField] float expandRate;
    [SerializeField] float expandDrop;
    [SerializeField] float shrinkTime;
    [SerializeField] float shrinkRate;
    [SerializeField] float maxBrightness;
    [SerializeField] float fadeRate;
    GameObject[] stars;
    GameObject square2;
    GameObject StarObject;
    SpriteRenderer squareRenderer;
    SpriteRenderer squareRenderer2;
    float width;

    // Start is called before the first frame update
    void Start()
    {
        width = Screen.width;
        stars = new GameObject[numStars];
        square2 = GameObject.Find("Square2");
        StarObject = GameObject.Find("Stars");
        squareRenderer = GameObject.Find("Square").GetComponent<SpriteRenderer>();
        squareRenderer2 = GameObject.Find("Square2").GetComponent<SpriteRenderer>();
        for (int i = 0; i < numStars; i++)
        {
            float x = UnityEngine.Random.Range(topLeft.x, bottomRight.x);
            float y = UnityEngine.Random.Range(topLeft.y, bottomRight.y);
            stars[i] = Instantiate(star, StarObject.transform, false);
            stars[i].transform.localPosition = new Vector3(x, y, 0f);
            //stars[i].transform.position = new Vector3(x, y, star.transform.position.z);
        }
        // Array.Sort(stars, (x, y) => (x.transform.localPosition.x <= y.transform.localPosition.x) ? -1 : 1);
        Expand();
        //expandRate /= (1920 / width);
    }

    public void Expand()
    {
        float startTime = Time.time;
        StartCoroutine(ExpandStars(startTime, startTime + expandTime));
    }

    public void Shrink()
    {
        float startTime = Time.time;
        StartCoroutine(ShrinkStars(startTime, startTime + shrinkTime));
    }

    public void Fade()
    {
        StartCoroutine(FadeStars());
    }

    IEnumerator ExpandStars(float startTime, float endTime)
    {
        while (Time.time < startTime + expandTime)
        {
            foreach (GameObject star in stars)
            {
                Vector3 scale = star.transform.localScale;
                scale.x += expandRate;
                scale.y += expandRate;
                star.transform.localScale = scale;
                StarObject.transform.localScale = new Vector3(0.5f + scale.x / 100, 0.5f + scale.y / 100);
                Vector3 pos = StarObject.transform.localPosition;
                pos.y -= expandDrop;
                StarObject.transform.localPosition = pos;
                float v = maxBrightness * (Time.time - startTime) / (endTime - startTime);
                squareRenderer.color = Color.HSVToRGB(0f, 0f, v);
                squareRenderer2.color = Color.HSVToRGB(0f, 0f, v);
            }
            yield return null;
        }
        while (Time.time < startTime + expandTime + 1)
            yield return new WaitForSeconds(1.0f);
        Shrink();
    }

    IEnumerator ShrinkStars(float startTime, float endTime)
    {
        while (Time.time < startTime + shrinkTime)
        {
            foreach (GameObject star in stars)
            {
                Vector3 scale = star.transform.localScale;
                scale.x = Mathf.Clamp(scale.x - shrinkRate, 0, scale.x);
                scale.y = Mathf.Clamp(scale.y - shrinkRate, 0, scale.x);
                star.transform.localScale = scale;
                Vector3 starScale = StarObject.transform.localScale;
                StarObject.transform.localScale = new Vector3(starScale.x - shrinkRate / 500, starScale.y - shrinkRate / 500);
                float v = maxBrightness * ( 1 - ((Time.time - startTime) / (endTime - startTime)));
                squareRenderer.color = Color.HSVToRGB(0f, 0f, v);
                squareRenderer2.color = Color.HSVToRGB(0f, 0f, v);
            }
            yield return null;
        }
    }

    IEnumerator FadeStars()
    {
        while (square2.transform.position.x < 0)
        {
            Vector3 pos = square2.transform.localPosition;
            pos.x += fadeRate;
            square2.transform.localPosition = pos;
            float v = maxBrightness * (-pos.x / 1920);
            squareRenderer.color = Color.HSVToRGB(0f, 0f, v);
            squareRenderer2.color = Color.HSVToRGB(0f, 0f, v);
            yield return null;
        }
    }




}
