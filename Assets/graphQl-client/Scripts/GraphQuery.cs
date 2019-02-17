using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using SimpleJSON;
using UnityEngine;

namespace graphQLClient
{
	public class GraphQuery : MonoBehaviour
	{
		public static GraphQuery instance = null;
		[Tooltip("The url of the node endpoint of the graphQL server being queried")]
		public static string url;

		public delegate void QueryComplete();
		public static event QueryComplete onQueryComplete;


		public enum Status { Neutral, Loading, Complete, Error };

		public static Status queryStatus;
		public static string queryReturn;

		public static string authToken = "";


		public class Query
		{
			public string query;
		}

		public void Awake()
		{
			if (instance == null)
			{
				instance = this;
			}
			else if (instance != this)
			{
				Destroy(gameObject);
			}

            DontDestroyOnLoad(gameObject);
		}

		public static Dictionary<string, string> variable = new Dictionary<string, string>();
		public static Dictionary<string, string[]> array = new Dictionary<string, string[]>();

		public static WWW POST(string details)
		{
			details = QuerySorter(details);
			Query query = new Query();
			string jsonData = "";
			WWWForm form = new WWWForm();
			query = new Query { query = details };
			jsonData = JsonUtility.ToJson(query);
			byte[] postData = Encoding.ASCII.GetBytes(jsonData);
			Dictionary<string, string> postHeader = form.headers;
			if (postHeader.ContainsKey("Content-Type")){
				postHeader.Add("Authorization", authToken);
				postHeader["Content-Type"] = "application/json";
			}
				
			else
				postHeader.Add("Content-Type", "application/json");

			WWW www = new WWW(url, postData, postHeader);
			instance.StartCoroutine(WaitForRequest(www));
			queryStatus = Status.Loading;
			return www;
		}

		static IEnumerator WaitForRequest(WWW data)
		{
			yield return data; // Wait until the download is done
			if (data.error != null)
			{
				Debug.Log("There was an error sending request: " + data.error);
				queryStatus = Status.Error;
			}
			else
			{
				queryReturn = data.text;
				queryStatus = Status.Complete;
			}
			onQueryComplete();
		}

		public static string QuerySorter(string query)
		{
			string finalString;
			string[] splitString;
			string[] separators = { "$", "^" };
			splitString = query.Split(separators, StringSplitOptions.RemoveEmptyEntries);
			finalString = splitString[0];
			for (int i = 1; i < splitString.Length; i++)
			{
				if (i % 2 == 0)
				{
					finalString += splitString[i];
				}
				else
				{
					if (!splitString[i].Contains("[]"))
					{
						finalString += variable[splitString[i]];
					}
					else
					{
						finalString += ArraySorter(splitString[i]);
					}
				}
			}
			return finalString;
		}

		public static string ArraySorter(string theArray)
		{
			string[] anArray;
			string solution;
			anArray = array[theArray];
			solution = "[";
			foreach (string a in anArray)
			{

			}
			for (int i = 0; i < anArray.Length; i++)
			{
				solution += anArray[i].Trim(new Char[] { '"' });
				if (i < anArray.Length - 1)
					solution += ",";
			}
			solution += "]";
			Debug.Log("This is solution " + solution);
			return solution;
		}
	}
}
