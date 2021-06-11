using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphQlClient.Core
{
    public static class Introspection
    {
        public static string schemaIntrospectionQuery =
            "query IntrospectionQuery {\n  __schema {\n    queryType {\n      name\n    }\n    mutationType {\n      name\n    }\n    subscriptionType {\n      name\n    }\n    types {\n      ...FullType\n    }\n    directives {\n      name\n      description\n      locations\n      args {\n        ...InputValue\n      }\n    }\n  }\n}\n\nfragment FullType on __Type {\n  kind\n  name\n  description\n  fields(includeDeprecated: true) {\n    name\n    description\n    args {\n      ...InputValue\n    }\n    type {\n      ...TypeRef\n    }\n    isDeprecated\n    deprecationReason\n  }\n  inputFields {\n    ...InputValue\n  }\n  interfaces {\n    ...TypeRef\n  }\n  enumValues(includeDeprecated: true) {\n    name\n    description\n    isDeprecated\n    deprecationReason\n  }\n  possibleTypes {\n    ...TypeRef\n  }\n}\n\nfragment InputValue on __InputValue {\n  name\n  description\n  type {\n    ...TypeRef\n  }\n  defaultValue\n}\n\nfragment TypeRef on __Type {\n  kind\n  name\n  ofType {\n    kind\n    name\n    ofType {\n      kind\n      name\n      ofType {\n        kind\n        name\n        ofType {\n          kind\n          name\n          ofType {\n            kind\n            name\n            ofType {\n              kind\n              name\n              ofType {\n                kind\n                name\n              }\n            }\n          }\n        }\n      }\n    }\n  }\n}";
        
        [Serializable]        
        public class SchemaClass
        {
            public Data data;
            public class Data
            {
                public Schema __schema;
                public class Schema
                {
                    public List<Type> types;
                    public Type queryType;
                    public Type mutationType;
                    public Type subscriptionType;
                    public List<Directive> directives;


                    public class Directive
                    {
                        public string name;
                        public string description;
                        public List<InputValue> args;
                        public List<DirectiveLocation> locations;

                        public enum DirectiveLocation
                        {
                            QUERY,
                            MUTATION,
                            SUBSCRIPTION,
                            FIELD,
                            FRAGMENT_DEFINITION,
                            FRAGMENT_SPREAD,
                            INLINE_FRAGMENT,
                            SCHEMA,
                            SCALAR,
                            OBJECT,
                            FIELD_DEFINITION,
                            ARGUMENT_DEFINITION,
                            INTERFACE,
                            UNION,
                            ENUM,
                            ENUM_VALUE,
                            INPUT_OBJECT,
                            INPUT_FIELD_DEFINITION
                        }
                    }
                    public class Type
                    {
                        public TypeKind kind;
                        public string name;
                        public string description;
                        public List<Field> fields;
                        public List<Type> interfaces;
                        public List<Type> possibleTypes;
                        public List<EnumValue> enumValues;
                        public List<InputValue> inputFields;
                        public Type ofType;
                        
                        public class Field
                        {
                            public string name;
                            public string description;
                            public List<InputValue> args;
                            public Type type;
                            public bool isDeprecated;
                            public string deprecationReason;
                        }

                        public class EnumValue
                        {
                            public string name;
                            public string description;
                            public bool isDeprecated;
                            public string deprecationReason;
                        }

                        public enum TypeKind
                        {
                            SCALAR, OBJECT, INTERFACE, UNION, ENUM, INPUT_OBJECT, LIST, NON_NULL
                        }
                    }
                    public class InputValue
                    {
                        public string name;
                        public string description;
                        public Type type;
                        public string defaultValue;
                    }
                }
            }
            
        }
    }
}

