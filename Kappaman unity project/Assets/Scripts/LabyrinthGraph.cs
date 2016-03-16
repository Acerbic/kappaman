using System;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
// Required to save the settings
using Pathfinding.Serialization.JsonFx;
// using Pathfinding.Serialization;

// Inherit our new graph from a base graph type
[JsonOptIn]
public class LabyrinthGraph : NavGraph {
  [JsonMember]
  public LabyrinthLayout layout = LabyrinthLayout.empty;
  [JsonMember]
  public Vector3 bottomLeftCorner = Vector3.zero;
  [JsonMember]
  public float scale = 1;

  public PointNode[,] nodes;

  private PointNode[,] CreateNodes () {
    // for n*m playable labyrinth it creates nodes with an extra row
    // on bottom and an extra column on left
    
    int size_x = layout.width + 1;
    int size_y = layout.height + 1;
    
    PointNode[,] created = new PointNode[size_x, size_y];
    for (int i = 0; i < size_x; i++) {
      for (int j = 0; j < size_y; j++) {
        PointNode node = new PointNode(active);
        Vector3 pos = new Vector3(i, j, 0);
        pos = matrix.MultiplyPoint(pos);
        if (i == 0) {pos = new Vector3(pos.x, pos.y-0.2F, pos.z);}
        node.position = (Int3)pos;
        created[i, j] = node;
      }
    }
    return created;
  }

  /**
   * [SetUpNodeConnections description]
   * @param int x - index in nodes array
   * @param int y - index in nodes array
   */
  private void SetUpNodeConnections(int x, int y) {
    int last_col = nodes.GetLength(0) -1;
    int top_row = nodes.GetLength(1) -1;

    uint wrap_cost = 1000;
    uint step_cost = 1;

    // Get the current node
    PointNode node = nodes[x, y];

    if ((x==0 && y==0)) {
      // XXX: keep null?
      node.connections = new GraphNode[0];
      node.connectionCosts = new uint[0];
      return;
    }

    // var connections = new GraphNode[1];
    var connections = new List<GraphNode>();
    // var connectionCosts = new uint[1];
    var connectionCosts = new List<uint>();

    try {
      // top
      if (layout[x, y].top) {
        if (y == top_row) {
          connections.Add(nodes[x, 0]);
          connectionCosts.Add(wrap_cost);
        } else {
          connections.Add(nodes[x, y + 1]);
          connectionCosts.Add(step_cost);
        }
      }

      // right
      if (layout[x, y].right) {
        if (x == last_col) {
          connections.Add(nodes[0, y]);
          connectionCosts.Add(wrap_cost);
        } else {
          connections.Add(nodes[x + 1, y]);
          connectionCosts.Add(step_cost);
        }
      }

      // bottom
      if (layout[x, y-1].top) {
        if (y == 0) {
          connections.Add(nodes[x, top_row]);
          connectionCosts.Add(wrap_cost);
        } else {
          connections.Add(nodes[x, y - 1]);
          connectionCosts.Add(step_cost);
        }
      }

      // left
      if (layout[x-1, y].right) {
        if (x == 0) {
          connections.Add(nodes[last_col, y]);
          connectionCosts.Add(wrap_cost);
        } else {
          connections.Add(nodes[x - 1, y]);
          connectionCosts.Add(step_cost);
        }
      }
    } catch (IndexOutOfRangeException e) {
      Debug.Log(x);
      Debug.Log(y);
      throw e;
    }

    node.connections = connections.ToArray();
    node.connectionCosts = connectionCosts.ToArray();

    node.Walkable = true;
  }

  public override void ScanInternal (OnScanStatus statusCallback) {

    // Create a matrix which just moves the nodes and scales their positions by #scale
    // The SetMatrix call will save it to a variable called just "matrix"
    SetMatrix(Matrix4x4.TRS(bottomLeftCorner, Quaternion.identity, Vector3.one*scale));

    // Create an array containing all nodes
    nodes = CreateNodes();

    // Now all nodes are created, let's create some connections between them!
    for (int i = 0; i < nodes.GetLength(0); i++) {
      for (int j = 0; j < nodes.GetLength(1); j++) {
        SetUpNodeConnections(i, j);
      }
    }
  }

  public override void GetNodes (GraphNodeDelegateCancelable del) {
    if (nodes == null) return;
    for (int i = 0; i < nodes.GetLength(0); i++) {
      for (int j = 0; j < nodes.GetLength(1); j++) {
        // Call the delegate and check if it wants
        // more nodes or not
        if (!del(nodes[i, j])) {
          // If it did not want more nodes
          // then just return
          return;
        }
      }
    }
  }
}