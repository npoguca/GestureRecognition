using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.UI;

[System.Serializable]
public class Schema
{
   public List<Vector2Replacement> schema = new List<Vector2Replacement>();
    public string name;
    public Schema(List<Vector2> schema, string name)
    {
       
        for (int i = 0; i < schema.Count; i++)
        {
            Vector2Replacement newVector = new Vector2Replacement(schema[i]);
            this.schema.Add(newVector);
        }
        this.name = name;
    }
     public List<Vector2> RevertSchema(List<Vector2Replacement> schema)
    {
        List<Vector2> points = new List<Vector2>();
        for (int i = 0; i < schema.Count; i++)
        {
            points.Add(new Vector2(schema[i].x,schema[i].y));
        }
        return points;
    }
}
[System.Serializable]
public struct Vector2Replacement
{
    public float x;
    public float y;
    public  Vector2Replacement(Vector2 vector)
    {
        x = vector.x;
        y = vector.y;
    }
}
public class Recognition : MonoBehaviour {

    public Text pixelsLeft; 
    List<Vector2> savedSchema;
    List<Vector2> points;
    List<Vector2> newPoints;
    // Use this for initialization
    enum Directions
    {
        NN,
        NE,
        EE,
        SE,
        SS,
        SW,
        WW,
        NW,
    }
    void Start() {
        points = new List<Vector2>();
        newPoints = new List<Vector2>();
        savedSchema = new List<Vector2>();
    }
    void Recognize()
    {
        StartCoroutine("RecognitionCoroutine");
    }
    // Update is called once per frame
    void Update() {
        if (Input.GetMouseButtonDown(0))
        {
            Recognize();
        }
        pixelsLeft.transform.position = Input.mousePosition + new Vector3(0, 2,0);
    }
    void Draw(List<Vector2> blueprint,Color color)
    {
        for (int i = 0; i < blueprint.Count - 1; i++)
        {

            Debug.DrawLine(blueprint[i], blueprint[i + 1], color, 2, false);
        }
    }
    void Simplify()
    {
        if (points.Count > savedSchema.Count)
        {
            while (points.Count != savedSchema.Count)
            {
                for (int i = 0; i < points.Count - 1; i++)
                {
                    newPoints.Add(new Vector2((points[i].x + points[i + 1].x) / 2, (points[i].y + points[i + 1].y) / 2));
                    //if (Vector2.Distance(points[i], points[i + 1])>0.2f)
                    //{

                    //}
                }
                points = new List<Vector2>(newPoints);

                newPoints.Clear();


            }
        }
        else if (points.Count < savedSchema.Count)
        {
            newPoints.Clear();

            while (points.Count != savedSchema.Count)
            {
                for (int i = 0; i < points.Count - 1; i++)
                {
                    newPoints.Add(new Vector2((savedSchema[i].x + savedSchema[i + 1].x) / 2, (savedSchema[i].y + savedSchema[i + 1].y) / 2));
                    //if (Vector2.Distance(points[i], points[i + 1])>0.2f)
                    //{

                    //}
                }
                savedSchema = new List<Vector2>(newPoints);

                newPoints.Clear();


            }
        }


    }
    void Save()
    {
        Schema schema = new Schema(points, "at");
        BinaryFormatter bf = new BinaryFormatter();
        FileStream fs = new FileStream(Application.persistentDataPath + "at" + ".dat", FileMode.Create);
        bf.Serialize(fs, schema);
        fs.Close();
    }
    void Load()
    {
        
        FileStream fsReader = new FileStream(Application.persistentDataPath + "at" + ".dat",FileMode.Open);
        BinaryFormatter bf = new BinaryFormatter();
        Schema schema = (Schema)bf.Deserialize(fsReader);
        fsReader.Close();
        savedSchema = schema.RevertSchema(schema.schema);
        Draw(savedSchema,Color.green);
    }
    void SimplifyMinimal()
    {
        float minimalDistance = 1920*1080;
        int minInd = 0;
        if (points.Count > 20)
        {
            while (points.Count != 20)
            {
                for (int i = 0; i < points.Count - 1; i++)
                {
                    if (Vector2.Distance(points[i], points[i+1])< minimalDistance)
                    {
                        minimalDistance = Vector2.Distance(points[i], points[i + 1]);
                        minInd = i;
                    }
                }
                Vector2 middle = new Vector2((points[minInd].x + points[minInd + 1].x) / 2, (points[minInd].y + points[minInd + 1].y) / 2);
                points.RemoveAt(minInd);
                points.RemoveAt(minInd);
                points.Insert(minInd, middle);



            }
        }
    }
    IEnumerator RecognitionCoroutine()
    {
        points.Clear();
        float pixels = 68;
        
        while (/*!Input.GetMouseButtonUp(0) || */pixels>0)
        {
            points.Add(Input.mousePosition);
            pixelsLeft.text = pixels.ToString();
            pixels--;
            //if (points.Count>1)
            //{
            //    Debug.DrawLine(points[points.Count - 1], points[points.Count], Color.red, 2,false);

            //}
            yield return null;
        }
        //Save();
        Load();
        Simplify();
        Draw(points,Color.red);
        List<float> pointsAngles = new List<float>();
        List<float> oldSchemeAngles = new List<float>();
        List<float> offset = new List<float>();
        for (int i = 0; i < points.Count-1; i++)
        {
            
            
            pointsAngles.Add(Vector2.Angle(points[i], points[i+1]));
            oldSchemeAngles.Add(Vector2.Angle(savedSchema[i ], savedSchema[i+1]));

        }
        for (int i = 0; i < pointsAngles.Count; i++)
        {
            offset.Add(pointsAngles[i] - oldSchemeAngles[i]);

        }
        float f = 0;
        foreach (var item in offset)
        {
            f += item;

        }
    }    
}
