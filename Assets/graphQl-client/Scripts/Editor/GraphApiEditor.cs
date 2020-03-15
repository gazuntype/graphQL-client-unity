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

            if (graph.loading){
                EditorGUILayout.LabelField("API is being introspected. Please wait...");
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            if (graph.schemaClass == null){
                return;
            }
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create New Query")){
                graph.CreateNewQuery();
            }

            if (GUILayout.Button("Create New Mutation")){
                graph.CreateNewMutation();
            }

            if (GUILayout.Button("Create New Subscription")){
                graph.CreateNewSubscription();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            DisplayFields(graph, graph.queries, "Query");
            DisplayFields(graph, graph.mutations, "Mutation");
            DisplayFields(graph, graph.subscriptions, "Subscription");

            EditorUtility.SetDirty(graph);
        }

        private void DisplayFields(GraphApi graph, List<GraphApi.Query> queryList, string type){
            if (queryList != null){
                if (queryList.Count > 0)
                    EditorGUILayout.LabelField(type);
                for (int i = 0; i < queryList.Count; i++){
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    GraphApi.Query query = queryList[i];
                    query.name = EditorGUILayout.TextField($"{type} Name", query.name);
                    string[] options = query.queryOptions.ToArray();
                    if (String.IsNullOrEmpty(query.returnType)){
                        index = EditorGUILayout.Popup(type, index, options);
                        query.queryString = options[index];
                        EditorGUILayout.LabelField(options[index]);
                        if (GUILayout.Button($"Confirm {type}")){
                            graph.GetQueryReturnType(query, options[index]);
                        }
                        if (GUILayout.Button("Delete")){
                            graph.DeleteQuery(queryList, i);
                        }

                        continue;
                    }

                    if (query.isComplete){
                        GUILayout.Label(query.query);
                        if (query.fields.Count > 0){
                            if (GUILayout.Button($"Edit {type}")){
                                graph.EditQuery(query);
                            }
                        }

                        if (GUILayout.Button("Delete")){
                            graph.DeleteQuery(queryList, i);
                        }

                        continue;
                    }


                    EditorGUILayout.LabelField(query.queryString,
                        $"Return Type: {query.returnType}");
                    if (graph.CheckSubFields(query.returnType)){
                        if (GUILayout.Button("Create Field")){
                            graph.GetQueryReturnType(query, options[index]);
                            graph.AddField(query, query.returnType);
                        }
                    }
                    

                    foreach (GraphApi.Field field in query.fields){
                        GUI.color = new Color(0.8f,0.8f,0.8f);
                        string[] fieldOptions = field.possibleFields.Select((aField => aField.name)).ToArray();
                        EditorGUILayout.BeginHorizontal();
                        GUIStyle fieldStyle = EditorStyles.popup;
                        fieldStyle.contentOffset = new Vector2(field.parentIndexes.Count * 20, 0);
                        field.Index = EditorGUILayout.Popup(field.Index, fieldOptions, fieldStyle);
                        GUI.color = Color.white;
                        field.CheckSubFields(graph.schemaClass);
                        if (field.hasSubField){
                            if (GUILayout.Button("Create Sub Field")){
                                graph.AddField(query, field.possibleFields[field.Index].type, field);
                                break;
                            }
                        }

                        if (GUILayout.Button("x", GUILayout.MaxWidth(20))){
                            int parentIndex = query.fields.FindIndex(aField => aField == field);
                            query.fields.RemoveAll(afield => afield.parentIndexes.Contains(parentIndex));
                            query.fields.Remove(field);
                            field.hasChanged = false;
                            break;
                        }

                        EditorGUILayout.EndHorizontal();

                        if (field.hasChanged){
                            int parentIndex = query.fields.FindIndex(aField => aField == field);
                            query.fields.RemoveAll(afield => afield.parentIndexes.Contains(parentIndex));
                            field.hasChanged = false;
                            break;
                        }

                        
                    }
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    if (query.fields.Count > 0){
                        if (GUILayout.Button($"Preview {type}")){
                            query.CompleteQuery();
                        }
                    }

                    if (GUILayout.Button("Delete")){
                        graph.DeleteQuery(queryList, i);
                    }
                    
                }

                EditorGUILayout.Space();
            }
            
        }
    }
}

