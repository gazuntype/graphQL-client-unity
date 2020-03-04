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
        private int fieldIndex;

        public override void OnInspectorGUI(){
            GraphApi graph = (GraphApi) target;
            if (GUILayout.Button("Reset")){
                graph.DeleteAllQueries();
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(graph.name);
            graph.url = EditorGUILayout.TextField("Url", graph.url);
            if (GUILayout.Button("Introspect")){
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
                            graph.GetQueryReturnType(query, options[index]);
                            graph.AddField(query, query.returnType);
                        }

                        if (GUILayout.Button("Delete")){
                            graph.DeleteQuery(i);
                        }

                        return;
                    }

                    EditorGUILayout.LabelField(query.queryString,
                        $"Return Type: {query.returnType}");
                    if (GUILayout.Button("Create Field")){
                        graph.GetQueryReturnType(query, options[index]);
                        graph.AddField(query, query.returnType);
                    }

                    foreach (GraphApi.Field field in query.fields){
                        string[] fieldOptions = field.possibleFields.Select((aField => aField.name)).ToArray();
                        string label = field.parent == null ? "Field" : "Sub Field";
                        field.Index = EditorGUILayout.Popup(label, field.Index, fieldOptions);
                        field.CheckSubFields(graph.schemaClass);
                        if (field.parent == null)
                            EditorGUILayout.LabelField(fieldOptions[field.Index]);
                        else{
                            EditorGUILayout.LabelField(fieldOptions[field.Index], $"Parent: {field.parent.name}");
                        }
                        if (field.hasSubField){
                            if (GUILayout.Button("Create Sub Field")){
                                graph.AddField(query, field.possibleFields[field.Index].type, field);
                                break;
                            }
                        }
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

