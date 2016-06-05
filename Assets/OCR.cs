using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.UI;

public class OCR : MonoBehaviour
{
    public int cleaningRange;
    public Vector2 drawPos { get; set; }
    public float frequency;
    public float captureDuration;
    bool activateOCR { get; set; }
    Vector2 defaultPos { get; set; }
    public List<float> scheme { get; set; }
    List<Scheme> schemes { get; set; }
    // Use this for initialization

    void Start()
    {
        frequency = 3f;
        defaultPos = new Vector2();
        scheme = new List<float>();
        schemes = new List<Scheme>();
        LoadData();
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
    private void DrawScheme(Scheme scheme)
    {
        float angle = 0f;
        float side = 0f;
        Vector2 finalPoint = new Vector2();
        Vector2 startPoint = drawPos;
        foreach (var item in scheme.serializableScheme)
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
    private float CalculateAngle(Vector2 defaultPos, Vector2 currentPos)
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
        return tg;

    }
    private bool MouseMoved()
    {
        //I feel dirty even doing this 
        return (Input.GetAxis("Mouse X") != 0) || (Input.GetAxis("Mouse Y") != 0);
    }
    IEnumerator Tangent()
    {
        Vector2 defaultPos = Input.mousePosition;
        drawPos = defaultPos;
        Vector2 currentPos = new Vector2();
        captureDuration = Time.deltaTime*6f;
        while (activateOCR)
        {
            if (captureDuration > 0)
            {
                currentPos = Input.mousePosition;
                defaultPos = currentPos;
                scheme.Add(CalculateAngle(defaultPos, currentPos));

                captureDuration -= Time.deltaTime;

            }
            yield return null;

            //else
            //{
            //if (MouseMoved())
            //{

            //}

            //}
        }
    }
    private Scheme schemeCleaner(Scheme scheme)
    {


        for (var i = 0; i < scheme.serializableScheme.Count/2; i++)
        {

            if (System.Math.Abs(scheme.serializableScheme[i] - scheme.serializableScheme[i + 2]) < 15)
            {
                scheme.serializableScheme[i+1] = (scheme.serializableScheme[i]+scheme.serializableScheme[i+2])/2;
            }
        }
        //for (var i = 1; i< scheme.serializableScheme.Count-1; i++)
        //{
            
        //    if (System.Math.Abs(scheme.serializableScheme[i] - scheme.serializableScheme[i-1]) < cleaningRange)
        //    {
        //        scheme.serializableScheme[i] = -1f;
        //    }
        //}
        //scheme.serializableScheme.RemoveAll(x => x == -1);
        return scheme;
    }
    private void LoadData()
    {
        DirectoryInfo di = new DirectoryInfo((Application.persistentDataPath));
        FileInfo[] files = di.GetFiles("*.dat");
        foreach (var item in files)
        {
            FileStream file = File.Open(item.FullName, FileMode.Open);
            BinaryFormatter bf = new BinaryFormatter();
            Scheme sch = (Scheme)bf.Deserialize(file);
            file.Close();
            DrawScheme(sch);
            sch = schemeCleaner(sch);
            sch.serializableScheme = Optimize(sch);
            schemes.Add(sch);
            DrawScheme(sch);
        }
        
    }
    public void SaveData()
    {
        InputField inputs = GameObject.Find("Canvas").GetComponentInChildren<InputField>();
        Scheme sch = new Scheme();
        sch.name = inputs.text;
        sch.serializableScheme = scheme;
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + sch.name + "/.dat");
        bf.Serialize(file, sch);
        file.Close();
    }

    public void Reconize()
    {
        foreach (var item in schemes)
        {
            foreach (var i in item.serializableScheme)
            {
                
            }
        }

    }
    private List<float> Optimize(Scheme sch)
    {
        int startingPoint = 0;
        List<float> optimized = sch.serializableScheme;
        for (int i = 0; i < optimized.Count; i++)
        {
            if (System.Math.Abs(optimized[i]-optimized[startingPoint])>45)
            {
                optimized[i] = 9999f;
            }
        }
        return optimized;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            scheme.Clear();
            activateOCR = true;
            StopCoroutine(Tangent());
            StartCoroutine(Tangent());
            Debug.Log(defaultPos);

        }
        
        if (Input.GetMouseButtonUp(0))
        {
            activateOCR = false;
            //SaveData();
            //DrawScheme(scheme);
               
        }

    }
    
}
[System.Serializable()]
class Scheme
{
    public string name;
    public List<float> serializableScheme;
    
}