using System.Collections;
using System.Collections.Generic;
using System.Text;
using SimpleJSON;
using UnityEngine.UI;
using UnityEngine;

public class GraphQuery : MonoBehaviour
{
	[Tooltip("The url of the node endpoint of the graphQL server being queried")] 
	public string url;

	public delegate void QueryComplete();
	public static event QueryComplete onQueryComplete;


	public enum Status { Neutral, Loading, Complete, Error };

	public static Status queryStatus;
	public static string queryReturn;


	public class Query
	{
		public string query;
	}
	// Use this for initialization
	public void Start()
	{


	}

	public WWW POST(string details)
	{
		Query query = new Query();
		string jsonData = "";
		WWWForm form = new WWWForm();
		query = new Query { query = details };
		jsonData = JsonUtility.ToJson(query);
		byte[] postData = Encoding.ASCII.GetBytes(jsonData);
		Dictionary<string, string> postHeader = form.headers;
		if (postHeader.ContainsKey("Content-Type"))
			postHeader["Content-Type"] = "application/json";
		else
			postHeader.Add("Content-Type", "application/json");

		WWW www = new WWW(url, postData, postHeader);
		StartCoroutine(WaitForRequest(www));
		queryStatus = Status.Loading;
		return www;
	}

	IEnumerator WaitForRequest(WWW data)
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
}
