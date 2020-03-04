using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GraphQlClient.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace GraphQlClient.Core
{
    [CreateAssetMenu(fileName = "GraphApi", menuName = "GraphQlClient/Core/GraphApi")]
    public class GraphApi : ScriptableObject
    {
        public string url;

        public List<Query> queries;

        public List<Mutation> mutations;
        
        private string introspection;
        
        public Introspection.SchemaClass schemaClass;

        private string queryEndpoint;
        
        
        public async void Introspect(){
            UnityWebRequest request = await HttpHandler.PostAsync(url, Introspection.schemaIntrospectionQuery);
            introspection = request.downloadHandler.text;
            File.WriteAllText(Application.dataPath + "\\schema.txt",introspection);
            schemaClass = JsonConvert.DeserializeObject<Introspection.SchemaClass>(introspection);
            queryEndpoint = schemaClass.data.__schema.queryType.name;
        }

        private void GetSchema(){
            if (schemaClass == null){
                introspection = File.ReadAllText(Application.dataPath + "\\schema.txt");
                schemaClass = JsonConvert.DeserializeObject<Introspection.SchemaClass>(introspection);
                queryEndpoint = schemaClass.data.__schema.queryType.name;
            }
        }

        public void CreateNewQuery(){
            GetSchema();
            if (queries == null)
                queries = new List<Query>();
            Query query = new Query{fields = new List<Field>(), queryOptions = new List<string>()};
            
            Introspection.SchemaClass.Data.Schema.Type queryType = schemaClass.data.__schema.types.Find((aType => aType.name == queryEndpoint));
            for (int i = 1; i < queryType.fields.Count; i++){
                query.queryOptions.Add(queryType.fields[i].name);
            }

            queries.Add(query);
        }

        

        private List<Field> GetSubFields(Introspection.SchemaClass.Data.Schema.Type type){
            List<Introspection.SchemaClass.Data.Schema.Type.Field> subFields = type.fields;
            List<Field> fields = new List<Field>();
            foreach (Introspection.SchemaClass.Data.Schema.Type.Field field in subFields){
                fields.Add((Field)field);
            }

            return fields;
        }

        public void AddField(Query query, string typeName){
            Introspection.SchemaClass.Data.Schema.Type type = schemaClass.data.__schema.types.Find((aType => aType.name == typeName));
            List<Introspection.SchemaClass.Data.Schema.Type.Field> subFields = type.fields;
            Field fielder = new Field();
            foreach (Introspection.SchemaClass.Data.Schema.Type.Field field in subFields){
                fielder.possibleFields.Add((Field)field);
            }
            query.fields.Add(fielder);
        }

        private string GetFieldType(Introspection.SchemaClass.Data.Schema.Type.Field field){
            Field newField = (Field)field;
            return newField.type;
        }
        


        public void GetQueryReturnType(Query query, string queryName){
            Introspection.SchemaClass.Data.Schema.Type queryType =
                schemaClass.data.__schema.types.Find((aType => aType.name == queryEndpoint));
            Introspection.SchemaClass.Data.Schema.Type.Field field =
                queryType.fields.Find((aField => aField.name == queryName));

            query.returnType = GetFieldType(field);
        }

        public void DeleteQuery(int index){
            queries.RemoveAt(index);
        }

        public void DeleteAllQueries(){
            queries = new List<Query>();
        }

        #region Classes

        [Serializable]
        public class Query
        {
            public string name;
            public string queryString;
            public string returnType;
            public List<string> queryOptions;
            public List<Field> fields;
        }

        [Serializable]
        public class Mutation
        {
            public string name;
        }
        
        [Serializable]
        public class Field
        {
            private int index;
            public int Index{
                get => index;
                set{
                    type = possibleFields[value].type;
                    name = possibleFields[value].name;
                    index = value;
                }
            }
            public string name;
            public string type;
            public Field parent;
            public bool hasSubField;
            public List<PossibleField> possibleFields;

            public Field(){
                possibleFields = new List<PossibleField>();
                index = 0;
            }
            
            public void CheckSubFields(Introspection.SchemaClass schemaClass){
                Introspection.SchemaClass.Data.Schema.Type t = schemaClass.data.__schema.types.Find((aType => aType.name == type));
                if (t.fields == null || t.fields.Count == 0){
                    hasSubField = false;
                    return;
                }

                hasSubField = true;
            }
            
            [Serializable]
            public class PossibleField
            {
                public string name;
                public string type;
                
                public static implicit operator PossibleField(Field field){
                    return new PossibleField{name = field.name, type = field.type};
                }
            }
            public static explicit operator Field(Introspection.SchemaClass.Data.Schema.Type.Field schemaField){
                Introspection.SchemaClass.Data.Schema.Type ofType = schemaField.type;
                string typeName;
                do{
                    typeName = ofType.name;
                    ofType = ofType.ofType;
                } while (ofType != null);
                return new Field{name = schemaField.name, type = typeName};
            }
        }

        #endregion
    }
}