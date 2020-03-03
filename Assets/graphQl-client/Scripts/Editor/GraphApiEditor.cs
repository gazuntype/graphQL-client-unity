using GraphQlClient.Core;
using UnityEditor;
using UnityEngine;

namespace GraphQlClient.Editor
{
    [CustomEditor(typeof(GraphApi))]
    public class GraphApiEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            GraphApi graph = (GraphApi)target;
            EditorGUILayout.LabelField(graph.name);
            graph.url = EditorGUILayout.TextField("Url", graph.url);
            if(GUILayout.Button("Introspect")){
                graph.Introspect();
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            if (GUILayout.Button("Create New Query")){
                graph.CreateNewQuery();
            }

            if (GUILayout.Button("Create New Mutation")){
                
            }

            if (GUILayout.Button("Create New Subscription")){
                
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            if (graph.queries.Count > 0)
                EditorGUILayout.LabelField("Queries");
            for (int i = 0; i < graph.queries.Count; i++){
                graph.queries[i].queryName = EditorGUILayout.TextField("Query Name", graph.queries[i].queryName);
                if (GUILayout.Button("Delete")){
                    graph.DeleteQuery(i);
                }
            }
            
            
            EditorUtility.SetDirty(graph);
        }
    }
}

