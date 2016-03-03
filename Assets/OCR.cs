using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
public class OCR : MonoBehaviour
{
    public int cleaningRange;
    public Vector2 drawPos { get; set; }
    public float frequency;
    public float captureDuration;
    bool activateOCR { get; set; }
    Vector2 defaultPos { get; set; }
    public List<float> scheme { get; set; }
    // Use this for initialization

    void Start()
    {
        frequency = 3f;
        defaultPos = new Vector2();
        scheme = new List<float>();
    }
    private float SidesToAngle(float x, float y)
    {
        return System.Math.Abs((float)((180 / (float)System.Math.PI) * (System.Math.Atan((float)(y / x)))));
    }
    private float AngleToSide(float angle)
    {
        float rads =  (float)((System.Math.PI / 180) * angle);
        float side = (float)System.Math.Tan((double)rads) * frequency;
        return side;
    }
    
    private void DrawScheme()
    {
        float angle = 0f;
        float side = 0f;
        Vector2 finalPoint = new Vector2();
        Vector2 startPoint = drawPos;
        foreach (var item in scheme)
        {


            if (item==0f)
            {

                finalPoint = new Vector2(startPoint.x + frequency, startPoint.y);
            }
            else if (item==90f)
            {
                finalPoint = new Vector2(startPoint.x , startPoint.y + frequency);

            }
            else if (item==180f)
            {
                finalPoint = new Vector2(startPoint.x - frequency, startPoint.y);

            }
            else if(item==270)
            {
                finalPoint = new Vector2(startPoint.x, startPoint.y - frequency);

            }
            else if (item > 270)
            {
                angle = 360 - item;
                side = AngleToSide(angle);
                finalPoint = new Vector2(startPoint.x + frequency, startPoint.y - side);
            }
            else if (item > 180)
            {
                angle = 270 - item;
                side = AngleToSide(angle);
                finalPoint = new Vector2(startPoint.x - side, startPoint.y - frequency);

            }
            else if (item > 90)
            {
                angle = 180 - item;
                side = AngleToSide(angle);
                finalPoint = new Vector2(startPoint.x - frequency, startPoint.y + side);

            }
            else if (item > 0)
            {
               
                side = AngleToSide(item);
                finalPoint = new Vector2(startPoint.x + frequency, startPoint.y + side);
            }
            Debug.DrawLine(startPoint,finalPoint, Color.red, 2, false);
            startPoint = finalPoint;


        }

    }
    private void CalculateAngle(Vector2 defaultPos, Vector2 currentPos)
    {
        float tg = 0f;
        float x = currentPos.x - defaultPos.x;
        float y = currentPos.y - defaultPos.y;
        if (currentPos.x==defaultPos.x && defaultPos.y<currentPos.y)
        {
            tg = 90f;

        }
        else if (currentPos.x == defaultPos.x && defaultPos.y > currentPos.y)
        {
            tg = 270f;

        }
        else if (currentPos.y == defaultPos.y && defaultPos.x < currentPos.x)
        {
            tg = 0.00000001f;    
        }
        else if (currentPos.y == defaultPos.y && defaultPos.x > currentPos.x)
        {
            tg = 180f;
        }
        else if (currentPos.x < defaultPos.x && currentPos.y < defaultPos.y)
        {
            tg = SidesToAngle(x, y);
            tg += 180;
        }
        else if (currentPos.x < defaultPos.x && currentPos.y > defaultPos.y)
        {
            tg = SidesToAngle(x, y);
            tg = 180 - tg;
        }
        else if (currentPos.x > defaultPos.x && currentPos.y < defaultPos.y)
        {
            tg = SidesToAngle(x, y);
            tg = 360 - tg;
        }
        else if (currentPos.x > defaultPos.x && currentPos.y > defaultPos.y)
        {
            tg = SidesToAngle(x, y);

        }

        if (tg!=0f)
        {
            scheme.Add(tg);

        }
        tg = (float)System.Math.Tan((double)((System.Math.PI / 180) * tg));
        Debug.Log(tg);
        Debug.DrawLine(new Vector2(defaultPos.x, defaultPos.y), new Vector2(currentPos.x, defaultPos.y), Color.green, 2, false);
        Debug.DrawLine(new Vector2(currentPos.x, defaultPos.y), new Vector2(currentPos.x, currentPos.y), Color.green, 2, false);
        Debug.DrawLine(defaultPos, currentPos, Color.green, 2, false);

    }
    IEnumerator Recognize()
    {
        Vector2 defaultPos = Input.mousePosition;
        drawPos = defaultPos;
        Vector2 currentPos = new Vector2();
        captureDuration = Time.deltaTime*1f;
        while (activateOCR)
        {
            //if (captureDuration > 0)
            //{
            //    captureDuration -= Time.deltaTime;
            //}
            //else
            //{
                currentPos = Input.mousePosition;
                CalculateAngle(defaultPos, currentPos);
                defaultPos = currentPos;
            //}
            yield return null;
        }
    }
    private void schemeCleaner()
    {


        for (var i = 1; i < scheme.Count/2; i++)
        {

            if (System.Math.Abs(scheme[i] - scheme[i + 2]) < cleaningRange)
            {
                scheme[i+1] = (scheme[i]+scheme[i+2])/2;
            }
        }
        for (var i = 1; i< scheme.Count-1; i++)
        {
            
            if (System.Math.Abs(scheme[i] - scheme[i-1]) < cleaningRange)
            {
                scheme[i] = -1f;
            }
        }
        scheme.RemoveAll(x => x == -1);
    }
    private void LoadData()
    {
        FileStream file = File.Open(Application.persistentDataPath + "/new.dat", FileMode.Open);
        BinaryFormatter bf = new BinaryFormatter();
        Data dt = (Data)bf.Deserialize(file);
        file.Close();
        string name = dt.name;
       //scheme = dt.serializableScheme;
        schemeCleaner();
        DrawScheme();

    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            scheme.Clear();
            activateOCR = true;
            StopCoroutine(Recognize());
            StartCoroutine(Recognize());
            Debug.Log(defaultPos);

        }
        if (Input.GetMouseButtonUp(0))
        {
            activateOCR = false;
            schemeCleaner();
            DrawScheme();
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(Application.persistentDataPath + "/new.dat");
            Data dt = new Data();
            dt.serializableScheme = scheme;
            dt.name = "air";
            bf.Serialize(file, dt);
            file.Close();
         }
        LoadData();

    }
    
}
[System.Serializable()]
class Data
{
    public string name;
    public List<float> serializableScheme;
    
}