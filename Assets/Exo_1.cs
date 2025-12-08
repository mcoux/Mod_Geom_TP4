using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UIElements;

public class Exo_1 : MonoBehaviour
{
    public float epsilon;
    Mesh mesh;
    Vector3 origin;
    float dim;
    List<Vector3> pointList = new List<Vector3>();
    List<Vector3> newPoints = new List<Vector3>();
    Dictionary<int, List<int>> groupIndexes = new Dictionary<int, List<int>>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        
        createBoundingBox(out dim);
        subDivide();
        Dictionary<Vector3, List<Vector3>> pointGroups = createPointsGroup();
        pointList = regroupPoints(pointGroups);
        Debug.Log(pointList.Count);
        redrawMesh();
      
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

    private Dictionary<Vector3, List<Vector3>> createPointsGroup()
    {
        float areaDim = dim / epsilon;
        List<Vector3> currentPointlist;
        List<int> indexesList;
        Dictionary<Vector3,List<Vector3>> pointGroups = new Dictionary<Vector3,List<Vector3>>();
        Vector3 currentVertex;
        float margin = 1e-10f;
        int index = 0;
        foreach (Vector3 point in pointList) 
        {
            currentPointlist = new List<Vector3>();
            indexesList = new List<int>();
            

            for(int i = 0; i < mesh.vertices.Length; i++)
            {
                
                currentVertex = mesh.vertices[i] + transform.position;
                if ((currentVertex.x >= point.x  && currentVertex.x <= point.x + areaDim) && (currentVertex.y >= point.y  && currentVertex.y <= point.y + areaDim) && (currentVertex.z >= point.z  && currentVertex.z <= point.z + areaDim))
                {
                    currentPointlist.Add(mesh.vertices[i]);
                    indexesList.Add(i);
                }
            }
            //Debug.Log(currentPointlist.Count);
            if(currentPointlist.Count != 0)
            {
                pointGroups.Add(point, currentPointlist);
                groupIndexes.Add(index, indexesList);
                index++;

            }
        }
        return pointGroups;
    }

    private List<Vector3> regroupPoints(Dictionary<Vector3, List<Vector3>> pointGroups)
    {
        List<Vector3> newPoints = new List<Vector3>();
        Vector3 avg;

        foreach( var (origin, vertices)  in pointGroups)
        {
            avg = Vector3.zero;
            foreach(Vector3 vertex in vertices)
            {
                avg += vertex;
            }

            avg.x /= vertices.Count;
            avg.y /= vertices.Count;
            avg.z /= vertices.Count;

            newPoints.Add(avg);
        }

        return newPoints;
    }

    private void redrawMesh()
    {
        List<int> currentTriangle = new List<int>();
        List<int> newTriangle = new List<int>();
        List<List<int>> newTrianglesList = new List<List<int>>();
        for (int i = 0; i < pointList.Count; i++)
        {
            for (int j = 0; j < mesh.triangles.Length; j += 3)
            {
                currentTriangle = new List<int>();
                newTriangle = new List<int>();
                currentTriangle.Add(mesh.triangles[j]);
                currentTriangle.Add(mesh.triangles[j+1]);
                currentTriangle.Add(mesh.triangles[j+2]);
                if (groupIndexes.ContainsKey(i))
                {
                    if (groupIndexes[i].Contains(mesh.triangles[j]) || groupIndexes[i].Contains(mesh.triangles[j + 1]) || groupIndexes[i].Contains(mesh.triangles[j + 2]))
                    {
                        newTriangle.Add(getIndexOfAvgPoint(currentTriangle[0]));
                        newTriangle.Add(getIndexOfAvgPoint(currentTriangle[1]));
                        newTriangle.Add(getIndexOfAvgPoint(currentTriangle[2]));
                        if (!newTrianglesList.Contains(newTriangle) && newTriangle[0] != newTriangle[1] && newTriangle[2] != newTriangle[1] && newTriangle[0] != newTriangle[2])
                        {
                            newTrianglesList.Add(newTriangle);
                        }
                    }
                }
                

            }
        }

        int[] newTrianglesArray = new int[newTrianglesList.Count*3];
        List<int> tempList = new List<int>();
        foreach (List<int> triangle in newTrianglesList) 
        {
            tempList.Add(triangle[0]);
            tempList.Add(triangle[1]);
            tempList.Add(triangle[2]);


        }
        newTrianglesArray = tempList.ToArray();

        Mesh newMesh = new Mesh();
        newMesh.vertices = pointList.ToArray();
        newMesh.triangles = newTrianglesArray;

        MeshFilter filter = GetComponent<MeshFilter>();
        filter.mesh = newMesh;


    }


    private int getIndexOfAvgPoint(int index)
    {
        int pointIndex = -1;

        int i = 0;


        foreach (var (pIndex, indexList) in groupIndexes)
        {
            if (indexList.Contains(index))
            {
                pointIndex = pIndex;
                break;
            }
        } 
        //Y a 64 zones avant le tri.
        return pointIndex;
    }


    //private void OnDrawGizmos()
    //{
    //    for (int i = 0; i < pointList.Count; i++)
    //    {
    //        //Gizmos.color = new UnityEngine.Color(255/(i+1), 255 / (i + 1), 255 / (i + 1));
    //        //Debug.Log(pointList[i]);
    //        Gizmos.DrawIcon(pointList[i] + transform.position, "p");
    //    }

    //}



}
