using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Stars : MonoBehaviour
{
    [Header("Inner Stars")]
    [SerializeField] GameObject star;
    [SerializeField] int numStars;
    [SerializeField] Vector2 topLeft;
    [SerializeField] Vector2 bottomRight;

    [Header("Outer Stars")]
    [SerializeField] GameObject star2;
    [SerializeField] int numOuterStars;
    [SerializeField] Vector2 outerTopLeft;
    [SerializeField] Vector2 outerBottomRight;

    [Header("Expand")]
    [SerializeField] float expandTime;
    [SerializeField] float expandRate;
    [SerializeField] float expandDrop;
    [SerializeField] float expandScale;
    [SerializeField] float expandRotationRate;

    [Header("Expand2")]
    [SerializeField] float expandTime2;
    [SerializeField] float expandRate2;
    [SerializeField] float expandDrop2;
    [SerializeField] float expandScale2;
    [SerializeField] float expandRotationRate2;

    [Header("Shrink")]
    [SerializeField] float shrinkTime;
    [SerializeField] float shrinkRate;
    [SerializeField] float shrinkScale;
    [SerializeField] float maxBrightness;
    [SerializeField] float shrinkRotationRate;

    [Header("Fade")]
    [SerializeField] float fadeRate;
    GameObject[] stars;
    GameObject[] outerStars;
    GameObject square2;
    GameObject StarObject;
    GameObject StarObject2;
    SpriteRenderer squareRenderer;
    SpriteRenderer squareRenderer2;
    float width;

    // Start is called before the first frame update
    void Start()
    {
        width = Screen.width;
        stars = new GameObject[numStars];
        outerStars = new GameObject[numOuterStars];
        square2 = GameObject.Find("Square2");
        StarObject = GameObject.Find("Stars");
        StarObject2 = GameObject.Find("Stars2");
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

        for (int i = 0; i < numOuterStars; i++)
        {
            float x = UnityEngine.Random.Range(outerTopLeft.x, outerBottomRight.x);
            float y = UnityEngine.Random.Range(outerTopLeft.y, outerBottomRight.y);
            outerStars[i] = Instantiate(star, StarObject2.transform, false);
            outerStars[i].transform.localPosition = new Vector3(x, y, 0f);
        }
        // Array.Sort(stars, (x, y) => (x.transform.localPosition.x <= y.transform.localPosition.x) ? -1 : 1);
        BeginSequence();
        //expandRate /= (1920 / width);
    }

    public void BeginSequence()
    {
        StartCoroutine(Wait(Time.time, 3f, Expand));
    }

    IEnumerator Wait(float startTime, float duration, Action callable)
    {
        while (Time.time < startTime + duration)
        {
            yield return null;
        }
        callable();
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

                Vector3 planeScale = StarObject.transform.localScale;
                planeScale.x += expandScale;
                planeScale.y += expandScale;
                StarObject.transform.localScale = planeScale;

                Vector3 pos = StarObject.transform.localPosition;
                pos.y -= expandDrop;
                StarObject.transform.localPosition = pos;

                Vector3 rotation = StarObject.transform.rotation.eulerAngles;
                rotation.z += expandRotationRate;
                StarObject.transform.rotation = Quaternion.Euler(rotation);


                float v = maxBrightness * (Time.time - startTime) / (endTime - startTime);
                squareRenderer.color = Color.HSVToRGB(0f, 0f, v);
                //squareRenderer2.color = Color.HSVToRGB(0f, 0f, v);
            }

            foreach (GameObject star in outerStars)
            {
                Vector3 scale = star.transform.localScale;
                scale.x += expandRate2;
                scale.y += expandRate2;
                star.transform.localScale = scale;

                Vector3 planeScale = StarObject2.transform.localScale;
                planeScale.x += expandScale2;
                planeScale.y += expandScale2;
                StarObject2.transform.localScale = planeScale;

                Vector3 pos = StarObject2.transform.localPosition;
                pos.y -= expandDrop2;
                StarObject2.transform.localPosition = pos;

                Vector3 rotation = StarObject2.transform.rotation.eulerAngles;
                rotation.z += expandRotationRate2;
                StarObject.transform.rotation = Quaternion.Euler(rotation);

                float v = maxBrightness * (Time.time - startTime) / (endTime - startTime);
                squareRenderer.color = Color.HSVToRGB(0f, 0f, v);
                //squareRenderer2.color = Color.HSVToRGB(0f, 0f, v);
            }
            yield return null;
        }
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
                StarObject.transform.localScale = starScale * shrinkScale; //new Vector3(starScale.x - shrinkScale, starScale.y - shrinkScale);

                Vector3 pos = StarObject.transform.localPosition;
                pos.y += expandDrop * (expandTime / shrinkTime) * 2;
                StarObject.transform.localPosition = pos;

                Vector3 rotation = StarObject.transform.rotation.eulerAngles;
                rotation.z -= shrinkRotationRate;
                StarObject.transform.rotation = Quaternion.Euler(rotation);

                float v = maxBrightness * ( 1 - ((Time.time - startTime) / (endTime - startTime)));
                squareRenderer.color = Color.HSVToRGB(0f, 0f, v);
                //squareRenderer2.color = Color.HSVToRGB(0f, 0f, v);
            }
            yield return null;
        }
        Destroy(StarObject);
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
