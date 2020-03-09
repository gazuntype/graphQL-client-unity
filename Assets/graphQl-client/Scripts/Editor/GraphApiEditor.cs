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

        public override void OnInspectorGUI(){
            GraphApi graph = (GraphApi) target;
            GUIStyle style = new GUIStyle{fontSize = 15, alignment = TextAnchor.MiddleCenter};
            EditorGUILayout.LabelField(graph.name, style);
            EditorGUILayout.Space();
            graph.GetSchema();
            if (GUILayout.Button("Reset")){
                graph.DeleteAllQueries();
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            graph.url = EditorGUILayout.TextField("Url", graph.url);
            if (GUILayout.Button("Introspect")){
                graph.Introspect();
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create New Query")){
                graph.CreateNewQuery();
            }

            if (GUILayout.Button("Create New Mutation")){

            }

            if (GUILayout.Button("Create New Subscription")){

            }

            EditorGUILayout.EndHorizontal();

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

                        continue;
                    }

                    if (query.isComplete){
                        GUILayout.Label(query.query);
                        if (query.fields.Count > 0){
                            if (GUILayout.Button("Edit Query")){
                                graph.EditQuery(query);
                            }
                        }

                        if (GUILayout.Button("Delete")){
                            graph.DeleteQuery(i);
                        }

                        continue;
                    }

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(query.queryString,
                        $"Return Type: {query.returnType}");
                    if (GUILayout.Button("Create Field")){
                        graph.GetQueryReturnType(query, options[index]);
                        graph.AddField(query, query.returnType);
                    }

                    EditorGUILayout.EndHorizontal();

                    foreach (GraphApi.Field field in query.fields){
                        string[] fieldOptions = field.possibleFields.Select((aField => aField.name)).ToArray();
                        EditorGUILayout.BeginHorizontal();
                        GUIStyle fieldStyle = new GUIStyle
                            {contentOffset = new Vector2(field.parentIndexes.Count * 20, 0)};
                        field.Index = EditorGUILayout.Popup(field.Index, fieldOptions, fieldStyle);
                        field.CheckSubFields(graph.schemaClass);
                        if (field.hasSubField){
                            if (GUILayout.Button("Create Sub Field")){
                                graph.AddField(query, field.possibleFields[field.Index].type, field);
                                break;
                            }
                        }

                        EditorGUILayout.EndHorizontal();

                        if (field.hasChanged){
                            int parentIndex = query.fields.FindIndex(aField => aField == field);
                            query.fields.RemoveAll(afield => afield.parentIndexes.Contains(parentIndex));
                            field.hasChanged = false;
                            break;
                        }

                    }

                    if (query.fields.Count > 0){
                        if (GUILayout.Button("Complete Query")){
                            query.CompleteQuery();
                        }
                    }

                    if (GUILayout.Button("Delete")){
                        graph.DeleteQuery(i);
                    }
                }

                EditorGUILayout.Space();
            }

            EditorUtility.SetDirty(graph);
        }
    }


}

