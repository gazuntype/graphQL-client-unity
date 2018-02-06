using System.Collections;
using System.Collections.Generic;
using System.Text;
using SimpleJSON;
using UnityEngine;

public class GraphQuery : MonoBehaviour
{
	public UnityEngine.UI.Text debug;
	public class Query
	{
		public string query;
	}
	Vector3 positional = new Vector3(3, 4, 5);
	WWW dataWanted;
	string allOpposerNames = "{allOpposers{name location position{x y z}}}";
	// Use this for initialization
	public void Start()
	{
		
		string createMutation = "mutation{createOpposer(name: \"Alubarika\", location: 4, x: " + positional.x + " y: " + positional.y + " z: " + positional.z + " ){name location position{x y z}}}";
		string deleteMutation = "mutation{deleteAllOpposers{name}}";
		string login = "mutation{loginUser(email: \"gazuntype@gmail.com\", password: \"Ayalatobe\"){message}}";
		//POST(login);
		//POST(createMutation);
		//POST(allOpposerNames);

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

		WWW www;
		if (Application.isEditor)
		{
			www = new WWW("http://localhost:3000/graphql", postData, postHeader);
		}
		else
		{
			www = new WWW("http://192.168.1.140:3000/graphql", postData, postHeader);
		}
		StartCoroutine(WaitForRequest(www));
		return www;
	}

	IEnumerator WaitForRequest(WWW data)
	{
		yield return data; // Wait until the download is done
		if (data.error != null)
		{
			Debug.Log("There was an error sending request: " + data.error);
			CanvasControl.queryStatus = CanvasControl.Status.Error;
		}
		else{
			Debug.Log(data.text);
			CanvasControl.queryReturn = data.text;
			CanvasControl.queryStatus = CanvasControl.Status.Complete;
		}
	}
}
