using System;
using System.Collections.Generic;
using UnityEngine;

public class SessionQuery : MonoBehaviour {

	public GraphQuery graphQL;

	Dictionary<string, string> variable = new Dictionary<string, string>();
	Dictionary<string, string[]> array = new Dictionary<string, string[]>();
	[TextArea]
	public string userDetails;
	[TextArea]
	public string createSession;
	[TextArea]
	public string availableSessions;
	[TextArea]
	public string joinSession;
	[TextArea]
	public string continueGame;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void LogIn(string username)
	{
		variable["username"] = username;
		graphQL.POST(QuerySorter(userDetails));
	}

	public void Create(string type, string sessionName, string username)
	{
		variable["sessionName"] = sessionName;
		variable["username"] = username;
		variable["type"] = type;
		graphQL.POST(QuerySorter(createSession));
	}

	public void CheckAvailableSessions()
	{
		graphQL.POST(availableSessions);
	}

	public void JoinSession(string id, string username)
	{
		variable["id"] = id;
		variable["username"] = username;
		graphQL.POST(QuerySorter(joinSession));

	}

	public void ContinueGame(string[] id)
	{
		array["id[]"] = id;
		graphQL.POST(QuerySorter(continueGame));
	}

	string QuerySorter(string query)
	{
		string finalString;
		string[] splitString;
		string[] separators = {"#", "^"};
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

	string ArraySorter(string theArray)
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
