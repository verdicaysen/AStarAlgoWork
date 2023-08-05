using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;

public class PathMarker {

    public MapLocation location;
    public float G, H, F;
    public GameObject marker;
    public PathMarker parent;

    public PathMarker(MapLocation l, float g, float h, float f, GameObject m, PathMarker p) {

        location = l;
        G = g;
        H = h;
        F = f;
        marker = m;
        parent = p;
    }

    public override bool Equals(object obj) {

        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            return false;
        else
            return location.Equals(((PathMarker)obj).location);
    }

    public override int GetHashCode() {

        return 0;
    }
}

public class FindPathAStar : MonoBehaviour {

    public Maze maze;
    public Material closedMaterial;
    public Material openMaterial;
    public GameObject start;
    public GameObject end;
    public GameObject pathP;

    PathMarker startNode;
    PathMarker goalNode;
    PathMarker lastPos;
    bool done = false;

    List<PathMarker> open = new List<PathMarker>();
    List<PathMarker> closed = new List<PathMarker>();

    void RemoveAllMarkers() {

        GameObject[] markers = GameObject.FindGameObjectsWithTag("marker");

        foreach (GameObject m in markers) Destroy(m);
    }

    void BeginSearch() {

        done = false;
        RemoveAllMarkers();

        List<MapLocation> locations = new List<MapLocation>();

        for (int z = 1; z < maze.depth - 1; ++z) {
            for (int x = 1; x < maze.width - 1; ++x) {

                if (maze.map[x, z] != 1) {

                    locations.Add(new MapLocation(x, z));
                }
            }
        }
        locations.Shuffle();

        Vector3 startLocation = new Vector3(locations[0].x * maze.scale, 0.0f, locations[0].z * maze.scale);
        startNode = new PathMarker(new MapLocation(locations[0].x, locations[1].z),
            0.0f, 0.0f, 0.0f, Instantiate(start, startLocation, Quaternion.identity), null);

        Vector3 endLocation = new Vector3(locations[1].x * maze.scale, 0.0f, locations[1].z * maze.scale);
        goalNode = new PathMarker(new MapLocation(locations[1].x, locations[1].z),
            0.0f, 0.0f, 0.0f, Instantiate(end, endLocation, Quaternion.identity), null);

            open.Clear();
            closed.Clear();
            open.Add(startNode);
            lastPos = startNode;
    }

    void Search(PathMarker thisNode)

    {
             
           //Avoids code issues if the keys are pushed out of order.
           if (thisNode == null) return;

           //Goal has been found.
           if(thisNode.Equals(goalNode)) {done = true; return;}

           foreach (MapLocation dir in maze.directions)
           {
               MapLocation neighbor = + thisNode.location;
               if (Maze.map[neighbor.x, neighbor.z] == 1) continue;
               if (neighbor.x < 1 || neighbor.x >= maze.width || neighbor.z < 1 || neighbor.z >= maze.depth) continue;
               if(IsClosed(neighbor)) continue;

               float G = Vector2.Distance(thisNode.Equalslocation.ToVector(), neighbor.ToVector()) + thisNode.G;
               float H = Vector2.Distance(neighbor.ToVector(), goalNode.location.ToVector());
               float F = G + H;

               GameObject pathBlock = Instanitate(pathP, new Vector3(neighbor.x * MapLocation.x * maze.scale, 0, neighbor.z * maze.scale), Quaternian.identity);

               TextMesh[] values = pathBlock.GetComponentsInChildren<TextMesh>();
               values[0].text = "G:" + G.ToString("0.00");
               values[1].text = "H:" + H.ToString("0.00");
               values[2].text = "F:" + F.ToString("0.00");

               if(!UpdateMarker(neighbor, G,H,F, thisNode))
                   open.Add(new PathMarker(neighbor, G, H, F, pathBlock, thisNode));               

           }

           //Reorder the open list from the largest to the smallest F values. Adjusted for efficiency.

           open = open.OrderBy(p => p.F).ThenBy(n => n.H).ToList<PathMarker>();
           PathMarker pm = (PathMarker)open.ElementAt(0);
           closed.Add(pm);
           open.RemoveAt(0);
           pm.marker.GetComponent<Renderer>().material = closedMaterial;

           lastPos = pm;

    }

    bool UpdateMarker(MapLocation pos, float g, float h, float f, PathMarker prt)

    {
        foreach (PathMarker p in open)

        {
            if (p.location.Equals(pos))

            {
                p.G = g;
                g.H = h;
                p.F = f;
                p.parent = prt;
                return true;
            }
        }

        return false;
    }

    bool IsClosed(MapLocation marker)

    {
        foreach (PathMarker p in closed)
    
        {
            if(p.location.Equals(marker))return true;
    
        }

return false;

    }

    void Start() 
    
    {

    }

    void Update() {

        if (Input.GetKeyDown(KeyCode.P)) BeginSearch();
        if (Input.GetKeyDown(KeyCode.C)) Search(lastPos);
    }
}
