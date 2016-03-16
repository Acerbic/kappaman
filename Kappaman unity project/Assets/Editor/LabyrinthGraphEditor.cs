using UnityEditor;
using Pathfinding;

[CustomGraphEditor (typeof(LabyrinthGraph),"Labyrinth Graph")]
public class LabyrinthGraphGeneratorEditor : GraphEditor {
    // Here goes the GUI
    public override void OnInspectorGUI (NavGraph target) {
        var graph = target as LabyrinthGraph;
        graph.scale = EditorGUILayout.FloatField ("Scale", graph.scale);
        graph.bottomLeftCorner = EditorGUILayout.Vector3Field ("Bottom Left", graph.bottomLeftCorner);
    }
}