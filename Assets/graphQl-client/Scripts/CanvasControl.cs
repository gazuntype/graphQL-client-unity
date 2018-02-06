using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using UnityEngine.EventSystems;

public class CanvasControl : MonoBehaviour
{
	#region Variables Declaration
	public static string userDetails;
	public enum Status { Neutral, Loading, Complete, Error };

	public static string chosenSession;

	public static Status queryStatus;
	public static string queryReturn;
	public Text status;

	GameObject activeParent;

	string sessionID;

	public GameObject canvas;
	public GameObject grid;

	[Header("Login")]
	public InputField username;
	public Button login;
	public Text loginStatus;

	[Header("Game")]
	public GameObject gameParent;
	public Button newGame;
	public Button continueGame;

	[Header("Session")]
	public GameObject sessionParent;
	public Button createSession;
	public Button joinSession;

	[Header("Continue")]
	public GameObject continueParent;
	public ScrollRect playedGames;
	public GameObject playedContent;
	public Button continuePlay;

	[Header("Join Session")]
	public GameObject joinParent;
	public ScrollRect openGames;
	public GameObject scrollContent;
	public Button enterGame;

	[Header("Create Session")]
	public GameObject createParent;
	public InputField sessionName;
	public Button create;
	public Image simple, team;

	SessionQuery sessionQuery;
	GameType type = GameType.SIMPLE;
	enum GameType { SIMPLE, TEAM }
	#endregion



	#region Login
	public void LogIn()
	{
		queryStatus = Status.Loading;
		sessionQuery.LogIn(username.text);
		loginStatus.text = "Logging in...";
		StartCoroutine(GetUserDetails());
	}

	IEnumerator GetUserDetails()
	{
		yield return new WaitUntil(() => queryStatus != Status.Loading);
		if (queryStatus == Status.Complete)
		{
			userDetails = queryReturn;
			loginStatus.text = "Logged in";
		}
		else
		{
			loginStatus.text = "error";
		}
	}

	#endregion



	#region Show Open Games
	public void ContinueGame()
	{
		gameParent.SetActive(false);
		continueParent.SetActive(true);
		activeParent = continueParent;
		var N = JSON.Parse(userDetails);
		string[] sessionNumber = new string[N["data"]["user"]["sessionID"].Count];
		for (int i = 0; i < N["data"]["user"]["sessionID"].Count; i++)
		{
			sessionNumber[i] = N["data"]["user"]["sessionID"][i].Value;
		}
		queryStatus = Status.Loading;
		sessionQuery.ContinueGame(sessionNumber);
		StartCoroutine(ShowOpenGame());
	}

	IEnumerator ShowOpenGame()
	{
		yield return new WaitUntil(() => queryStatus != Status.Loading);
		if (queryStatus == Status.Complete)
		{
			var N = JSON.Parse(queryReturn);
			for (int i = 0; i < N["data"]["session"].Count; i++)
			{
				GameObject button = playedContent.transform.GetChild(i).gameObject;
				button.SetActive(true);
				button.name = N["data"]["session"][i]["id"].Value;
				button.transform.GetChild(0).GetComponent<Text>().text = N["data"]["session"][i]["name"].Value + " by " + N["data"]["session"][i]["players"]["createrUsername"].Value;
			}
		}
	}

	#endregion

	#region Create/Join Session
	public void CreateSession()
	{
		sessionParent.SetActive(false);
		createParent.SetActive(true);
		activeParent = createParent;
	}

	public void JoinSession()
	{
		sessionParent.SetActive(false);
		joinParent.SetActive(true);
		activeParent = joinParent;
		queryStatus = Status.Loading;
		sessionQuery.CheckAvailableSessions();
		StartCoroutine(ShowSessions());
	}

	public void Create()
	{
		sessionQuery.Create(type.ToString(), sessionName.text, username.text);
		queryStatus = Status.Loading;
	}

	public void Join()
	{
		sessionQuery.JoinSession(sessionID, username.text);
		queryStatus = Status.Loading;	}



	IEnumerator ShowSessions()
	{
		yield return new WaitUntil(() => queryStatus != Status.Loading);
		if (queryStatus == Status.Complete)
		{
			var N = JSON.Parse(queryReturn);
			var L = JSON.Parse(userDetails);
			List<int> userIDs = new List<int>();
			int counter = 0;
			for (int j = 0; j < L["data"]["user"]["sessionID"].Count; j++)
			{
				userIDs.Add(L["data"]["user"]["sessionID"][j].AsInt);
			}
			for (int i = 0; i < N["data"]["allSessions"].Count; i++)
			{
				if (!userIDs.Contains(N["data"]["allSessions"][i]["id"].AsInt))
				{
					GameObject button = scrollContent.transform.GetChild(counter).gameObject;
					button.SetActive(true);
					button.name = N["data"]["allSessions"][i]["id"].Value;
					button.transform.GetChild(0).GetComponent<Text>().text = N["data"]["allSessions"][i]["name"].Value + " by " + N["data"]["allSessions"][i]["players"]["createrUsername"].Value;
					counter++;
				}
			}
		}
	}

	#endregion

	#region UI Manipulation
	public void SpawnGrid()
	{
		canvas.SetActive(false);
		grid.SetActive(true);	}

	public void Start()
	{
		activeParent = gameParent;
		sessionQuery = GetComponent<SessionQuery>();
	}

	public void Update()
	{
		status.text = "Query Status: " + queryStatus.ToString();	}

	public void HighlightSessionButton()
	{
		GameObject[] buttons = GameObject.FindGameObjectsWithTag("button");
		foreach (GameObject b in buttons)
		{
			b.GetComponent<Image>().color = Color.white;
		}
		EventSystem.current.currentSelectedGameObject.GetComponent<Image>().color = Color.green;
		sessionID = EventSystem.current.currentSelectedGameObject.name;
		chosenSession = sessionID;
		Debug.Log("this is the session ID of this selected session" + sessionID);
	}


	public void SelectGameType()
	{
		if (type == GameType.SIMPLE)
		{
			type = GameType.TEAM;
			simple.color = Color.white;
			team.color = Color.green;
		}
		else
		{
			type = GameType.SIMPLE;
			simple.color = Color.green;
			team.color = Color.white;
		}
	}

	public void NewGame()
	{
		gameParent.SetActive(false);
		sessionParent.SetActive(true);
		activeParent = sessionParent;	}

	public void Back()
	{
		if (activeParent == gameParent)
		{

		}
		else if (activeParent == joinParent)
		{
			sessionParent.SetActive(true);
			activeParent = sessionParent;
			joinParent.SetActive(false);
		}
		else if (activeParent == createParent)
		{
			sessionParent.SetActive(true);
			activeParent = sessionParent;
			createParent.SetActive(false);
		}
		else if (activeParent == sessionParent)
		{
			gameParent.SetActive(true);
			activeParent = gameParent;
			sessionParent.SetActive(false);
		}
		else if (activeParent == continueParent)
		{
			gameParent.SetActive(true);
			activeParent = gameParent;
			continueParent.SetActive(false);
		}
	}

#endregion
}
