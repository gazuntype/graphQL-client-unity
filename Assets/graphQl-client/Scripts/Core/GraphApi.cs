using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
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

        public Query GetQueryByName(string queryName){
            return queries.Find(aQuery => aQuery.name == queryName);
        }

        public async Task<UnityWebRequest> Post(Query query, string authToken = null){
            return await HttpHandler.PostAsync(url, query.query, authToken);
        }

        public async Task<UnityWebRequest> Post(string queryName, string authToken = null){
            Query query = GetQueryByName(queryName);
            return await Post(query, authToken);
        }

        #region Utility

        public static string JsonToArgument(string jsonInput){
            char[] jsonChar = jsonInput.ToCharArray();
            List<int> indexes = new List<int>();
            for (int i = 0; i < jsonChar.Length; i++){
                if (jsonChar[i] == '\"'){
                    if (indexes.Count == 2)
                        indexes = new List<int>();
                    indexes.Add(i);
                }

                if (jsonChar[i] == ':'){
                    jsonChar[indexes[0]] = ' ';
                    jsonChar[indexes[1]] = ' ';
                }
            }

            string result = new string(jsonChar);
            return result;
        }

        #endregion



        #region Editor Use

        public async void Introspect(){
            UnityWebRequest request = await HttpHandler.PostAsync(url, Introspection.schemaIntrospectionQuery);
            introspection = request.downloadHandler.text;
            File.WriteAllText(Application.dataPath + "\\schema.txt",introspection);
            schemaClass = JsonConvert.DeserializeObject<Introspection.SchemaClass>(introspection);
            queryEndpoint = schemaClass.data.__schema.queryType.name;
        }

        public void GetSchema(){
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

        public void EditQuery(Query query){
            query.isComplete = false;
        }

        

        private List<Field> GetSubFields(Introspection.SchemaClass.Data.Schema.Type type){
            List<Introspection.SchemaClass.Data.Schema.Type.Field> subFields = type.fields;
            List<Field> fields = new List<Field>();
            foreach (Introspection.SchemaClass.Data.Schema.Type.Field field in subFields){
                fields.Add((Field)field);
            }

            return fields;
        }

        //ToDo: Do not allow addition of subfield that already exists
        public void AddField(Query query, string typeName, Field parent = null){
            Introspection.SchemaClass.Data.Schema.Type type = schemaClass.data.__schema.types.Find((aType => aType.name == typeName));
            List<Introspection.SchemaClass.Data.Schema.Type.Field> subFields = type.fields;
            int parentIndex = query.fields.FindIndex(aField => aField == parent);
            List<int> parentIndexes = new List<int>();
            if (parent != null){
                parentIndexes = new List<int>(parent.parentIndexes){parentIndex};
            }
            Field fielder = new Field{parentIndexes = parentIndexes};
            foreach (Introspection.SchemaClass.Data.Schema.Type.Field field in subFields){
                fielder.possibleFields.Add((Field)field);
            }

            if (fielder.parentIndexes.Count == 0){
                query.fields.Add(fielder);
            }
            else{

                int index;
                index = query.fields.FindLastIndex(aField =>
                    aField.parentIndexes.Count > fielder.parentIndexes.Count &&
                    aField.parentIndexes.Contains(fielder.parentIndexes.Last()));

                if (index == -1){
                    index = query.fields.FindLastIndex(aField =>
                        aField.parentIndexes.Count > fielder.parentIndexes.Count &&
                        aField.parentIndexes.Last() == fielder.parentIndexes.Last());
                }

                if (index == -1){
                    index = fielder.parentIndexes.Last();
                }

                index++;
                query.fields[parentIndex].hasChanged = false;
                query.fields.Insert(index, fielder);
            }
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

        #endregion
        

        #region Classes

        [Serializable]
        public class Query
        {
            public string name;
            public string query;
            public string queryString;
            public string returnType;
            private string args;
            public List<string> queryOptions;
            public List<Field> fields;
            public bool isComplete;

            
            public void SetArgs(string args){
                this.args = args;
                CompleteQuery();
            }

            public void CompleteQuery(){
                isComplete = true;
                string data = null;
                string parent = null;
                Field previousField = null;
                for (int i = 0; i < fields.Count; i++){
                    Field field = fields[i];
                    if (field.parentIndexes.Count == 0){
                        if (parent == null){
                            data += $"\n{GenerateStringTabs(field.parentIndexes.Count + 2)}{field.name}";
                        }
                        else{
                            int count = previousField.parentIndexes.Count - field.parentIndexes.Count;
                            while (count > 0){
                                data += $"\n{GenerateStringTabs(count + 1)}}}";
                                count--;
                            }

                            data += $"\n{GenerateStringTabs(field.parentIndexes.Count + 2)}{field.name}";
                            parent = null;

                        }

                        previousField = field;
                        continue;
                    }

                    if (fields[field.parentIndexes.Last()].name != parent){

                        parent = fields[field.parentIndexes.Last()].name;

                        if (fields[field.parentIndexes.Last()] == previousField){

                            data += $"{{\n{GenerateStringTabs(field.parentIndexes.Count + 2)}{field.name}";
                        }
                        else{
                            int count = previousField.parentIndexes.Count - field.parentIndexes.Count;
                            while (count > 0){
                                data += $"\n{GenerateStringTabs(count + 1)}}}";
                                count--;
                            }

                            data += $"\n{GenerateStringTabs(field.parentIndexes.Count + 2)}{field.name}";
                        }

                        previousField = field;

                    }
                    else{
                        data += $"\n{GenerateStringTabs(field.parentIndexes.Count + 2)}{field.name}";
                        previousField = field;
                    }

                    if (i == fields.Count - 1){
                        int count = previousField.parentIndexes.Count;
                        while (count > 0){
                            data += $"\n{GenerateStringTabs(count + 1)}}}";
                            count--;
                        }
                    }

                }

                string arg = String.IsNullOrEmpty(args) ? "" : $"({args})";
                query =
                    $"query {name}{{\n{GenerateStringTabs(1)}{queryString}{arg}{{{data}\n{GenerateStringTabs(1)}}}\n}}";
            }

            private string GenerateStringTabs(int number){
                string result = "";
                for (int i = 0; i < number; i++){
                    result += "    ";
                }

                return result;
            }
        }

        [Serializable]
        public class Mutation
        {
            public string name;
        }
        
        [Serializable]
        public class Field
        {
            public int index;
            public int Index{
                get => index;
                set{
                    type = possibleFields[value].type;
                    name = possibleFields[value].name;
                    if (value != index)
                        hasChanged = true;
                    index = value;
                    
                }
            }

            public string name;
            public string type;
            public List<int> parentIndexes;
            public bool hasSubField;
            public List<PossibleField> possibleFields;

            public bool hasChanged;

            public Field(){
                possibleFields = new List<PossibleField>();
                parentIndexes = new List<int>();
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