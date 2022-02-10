using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Stars : MonoBehaviour
{
    [Header("Stars")]
    [SerializeField] GameObject star;
    [SerializeField] int numStars;
    [SerializeField] Vector2 topLeft;
    [SerializeField] Vector2 bottomRight;
    [SerializeField] float maxRadius;
    [SerializeField] float maxBrightness;

    [Header("Expand")]
    [SerializeField] float expandTime;
    [SerializeField] float[] expandScale;
    [SerializeField] float[] expandRate;
    [SerializeField] float[] expandDrop;
    [SerializeField] float[] expandRotationRate;

    [Header("Shrink")]
    [SerializeField] float shrinkTime;
    [SerializeField] float shrinkRate;
    [SerializeField] float[] shrinkScale;
    [SerializeField] float[] shrinkRotationRate;

    [Header("Fade")]
    [SerializeField] float fadeRate;

    GameObject[] stars;
    Star[] starData;
    GameObject[] outerStars;
    GameObject square2;
    GameObject[] StarObjects;
    SpriteRenderer squareRenderer;
    SpriteRenderer squareRenderer2;
    float width;

    public class Star
    {
        float expandRate;
        float expandDrop;
        public Vector3 position;
        public Vector3 scale;

        public Star(float rate, float drop, Vector3 initialPosition, Vector3 initialScale)
        {
            expandRate = rate + (UnityEngine.Random.Range(-1, 1) * rate * 0.1f);
            expandDrop = drop + (UnityEngine.Random.Range(-1, 1) * drop * 0.1f);

            position = initialPosition;
            scale = initialScale;
        }

        public void Expand()
        {
            position.x += expandDrop;
            position.y += expandDrop;
            scale.x += expandRate;
            scale.y += expandRate;
        }

        public void Shrink()
        {
            scale.x -= expandRate * 10;
            scale.y -= expandRate * 10;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        width = Screen.width;
        stars = new GameObject[numStars];
        starData = new Star[numStars];
        square2 = GameObject.Find("Square2");
        StarObjects = new GameObject[2];
        StarObjects[0] = GameObject.Find("Stars");
        StarObjects[1] = GameObject.Find("Stars2");
        squareRenderer = GameObject.Find("Square").GetComponent<SpriteRenderer>();
        squareRenderer2 = GameObject.Find("Square2").GetComponent<SpriteRenderer>();

        for (int i = 0; i < numStars; i++)
        {
            float radius = UnityEngine.Random.Range(0, maxRadius);
            float radians = UnityEngine.Random.Range(0, 2 * Mathf.PI);
            float x = Mathf.Cos(radians) * radius * 1.2f;
            float y = Mathf.Sin(radians) * radius * 1.2f;
            //float x = UnityEngine.Random.Range(topLeft.x, bottomRight.x);
            //float y = UnityEngine.Random.Range(topLeft.y, bottomRight.y);

            stars[i] = Instantiate(star, StarObjects[i % 2].transform, false);
            stars[i].transform.localPosition = new Vector3(x, y, 0f);
            starData[i] = new Star(expandRate[i % 2], expandDrop[i % 2], stars[i].transform.localPosition, stars[i].transform.localScale);
        }

        BeginSequence();
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
        StartCoroutine(FadeStars(Time.time));
    }

    IEnumerator ExpandStars(float startTime, float endTime)
    {
        while (Time.time < startTime + expandTime)
        {
            for (int i = 0; i < numStars; i++)
            {
                starData[i].Expand();
                stars[i].transform.localScale = starData[i].scale;
                stars[i].transform.localPosition = starData[i].position;
            }

            for (int i = 0; i < 2; i++)
            {
                Vector3 pos = StarObjects[i].transform.localPosition;
                pos.y -= expandDrop[i];

                Vector3 rot = StarObjects[i].transform.rotation.eulerAngles;
                rot.z += expandRotationRate[i];

                Vector3 scale = StarObjects[i].transform.localScale;
                scale.x += expandScale[i];
                scale.y += expandScale[i]; 

                StarObjects[i].transform.localPosition = pos;
                StarObjects[i].transform.rotation = Quaternion.Euler(rot);
                StarObjects[i].transform.localScale = scale;
            }

            float v = maxBrightness * (Time.time - startTime) / (endTime - startTime);
            squareRenderer.color = Color.HSVToRGB(0f, 0f, v);
            yield return null;
        }
        Shrink();
    }
    IEnumerator ShrinkStars(float startTime, float endTime)
    {
        while (Time.time < startTime + shrinkTime)
        {
            for (int i = 0; i < numStars; i++)
            {
                starData[i].Shrink();
                stars[i].transform.localScale = starData[i].scale;
            }

            for (int i = 0; i < 2; i++)
            {
                Vector3 pos = StarObjects[i].transform.localPosition;
                pos.y += expandDrop[i];

                Vector3 rot = StarObjects[i].transform.rotation.eulerAngles;
                rot.z += shrinkRotationRate[i];

                Vector3 scale = StarObjects[i].transform.localScale;
                scale.x = Mathf.Clamp(scale.x - shrinkScale[i], 0, scale.x);
                scale.y = Mathf.Clamp(scale.y * 1.01f, 0, scale.x * 5);

                StarObjects[i].transform.localPosition = pos;
                StarObjects[i].transform.rotation = Quaternion.Euler(rot);
                StarObjects[i].transform.localScale = scale;
            }

            float v = maxBrightness * (1 - (Time.time - startTime) / (endTime - startTime));
            squareRenderer.color = Color.HSVToRGB(0f, 0f, v);
            yield return null;
        }

        Destroy(StarObjects[0]);
        Destroy(StarObjects[1]);
    }
    
    IEnumerator FadeStars(float startTime)
    {
        while (Time.time < startTime + 1f)
        {
            foreach (GameObject star in stars)
            {
                Vector3 pos = star.transform.localPosition;
                pos.x += 0.005f;
                star.transform.localPosition = pos;

                Vector3 scale = star.transform.localScale;
                scale.y *= 0.99f;
                scale.x *= 0.99f;
                star.transform.localScale = scale;
            }

            float v = maxBrightness * (1 - ((Time.time - startTime) / (1f - startTime)));
            squareRenderer.color = Color.HSVToRGB(0f, 0f, v);
            yield return null;
        }
    }




}
