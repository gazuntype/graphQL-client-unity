using System;
using System.Collections.Generic;
using System.Linq;
using GraphQlClient.Core;
using UnityEditor;
using UnityEngine;

namespace GraphQlClient.Editor
{
    [CustomEditor(typeof(GraphApi))]
    public class GraphApiEditor : UnityEditor.Editor
    {
        private int index;
        public override void OnInspectorGUI()
        {
            GraphApi graph = (GraphApi)target;
            if (GUILayout.Button("Reset")){
                graph.DeleteAllQueries();
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();
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

            if (graph.queries != null){
                if (graph.queries.Count > 0)
                    EditorGUILayout.LabelField("Queries");
                for (int i = 0; i < graph.queries.Count; i++){
                    GraphApi.Query query = graph.queries[i];
                    query.name = EditorGUILayout.TextField("Query Name", query.name);
                    string[] options = query.queryOptions.ToArray();
                    if (String.IsNullOrEmpty(query.returnType)){
                        index = EditorGUILayout.Popup("Query", index, options);
                        query.queryString = options[index];
                        EditorGUILayout.LabelField(options[index]);
                        if (GUILayout.Button("Create Field")){
                            graph.GetQueryReturnType(query, index);
                            graph.CreateSubFields(query, query.returnType);
                        }
                    }
                    else{
                        EditorGUILayout.LabelField(query.queryString,
                            $"Return Type: {query.returnType}");
                    }

                    List<GraphApi.Field> fields = query.fields.FindAll((aField => aField.parent == null));
                    if (fields.Count > 0){
                        string[] returnTypeFields = fields.Select(o => o.name).ToArray();
                        EditorGUILayout.Popup("Query", 0, returnTypeFields);
                    }
                    
                        
                    if (GUILayout.Button("Delete")){
                        graph.DeleteQuery(i);
                    }
                }
            }



            EditorUtility.SetDirty(graph);
        }
    }
}

