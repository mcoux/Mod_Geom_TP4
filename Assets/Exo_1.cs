using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class Exo_1 : MonoBehaviour
{
    public float epsilon;
    Mesh mesh;
    Vector3 origin;
    float dim;
    List<Vector3> pointList = new List<Vector3>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        
        createBoundingBox(out dim);
        subDivide();
        
        origin += transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //Créé la bounding box de la mesh. Marche uniquement si la mesh est centrée
    private void createBoundingBox(out float dim)
    {
        Vector3 maxVals = mesh.vertices[0];

        foreach(Vector3 point in mesh.vertices)
        {
            if (Mathf.Abs(point.x - transform.position.x) > maxVals.x)
            {
                maxVals.x = point.x;
            }
            if (Mathf.Abs(point.y - transform.position.y) > maxVals.y)
            {
                maxVals.y = point.y;
            }

            if (Mathf.Abs(point.z - transform.position.z) > maxVals.z)
            {
                maxVals.z = point.z;
            }
        }
        
        dim = 2*Mathf.Max(maxVals.x , maxVals.y , maxVals.z);
    }

    private void subDivide()
    {
        float subDim = dim / 2;
        Vector3 newPoint;
        List<Vector3> output = new List<Vector3>();
        for (int i = 0; i < epsilon; i++)
        {
            for (int j = 0; j < epsilon; j++)
            {
                for (int k = 0; k < epsilon; k++)
                {
                    newPoint = new Vector3(-subDim  + dim * i  / epsilon, -subDim + dim * j / epsilon, -subDim + dim * k / epsilon);
                    newPoint += transform.position;
                 
                    pointList.Add(newPoint);
                }
            }


        }

        
    }

    private Dictionary<Vector3, List<Vector3>> createRepPoints()
    {
        float areaDim = dim / epsilon;
        List<Vector3> currentPointlist;
        Dictionary<Vector3,List<Vector3>> pointGroups = new Dictionary<Vector3,List<Vector3>>();
        foreach (Vector3 point in pointList) 
        { 
            currentPointlist = new List<Vector3>();
            foreach(Vector3 vertex in pointList)
            {
                if( (vertex.x > point.x && vertex.x< point.x + areaDim) && (vertex.y > point.y && vertex.y < point.y + areaDim) && (vertex.z > point.z && vertex.z < point.z + areaDim))
                {
                    currentPointlist.Add(vertex);
                }
            }
            pointGroups.Add(point,currentPointlist);
        }
        return pointGroups;
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < pointList.Count; i++)
        {
            //Gizmos.color = new UnityEngine.Color(255/(i+1), 255 / (i + 1), 255 / (i + 1));
            Gizmos.DrawIcon(pointList[i], "p");
        }

    }



}
