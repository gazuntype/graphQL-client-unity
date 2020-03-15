using GraphQlClient.Core;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class User : MonoBehaviour
{
    public GameObject loading;
    
    [Header("API")]
    public GraphApi userApi;

    [Header("Query")]
    public Text queryDisplay;

    [Header("Mutation")]
    public InputField id;
    public InputField username;
    public Text mutationDisplay;

    private bool subscribed;
    
    [Header("Subscription")]
    public Text subscriptionDisplay;

    public async void GetQuery(){
        loading.SetActive(true);
        UnityWebRequest request = await userApi.Post("GetUsers", GraphApi.Query.Type.Query);
        loading.SetActive(false);
        queryDisplay.text = HttpHandler.FormatJson(request.downloadHandler.text);
    }

    public async void CreateNewUser(){
        loading.SetActive(true);
        GraphApi.Query query = userApi.GetQueryByName("CreateNewUser", GraphApi.Query.Type.Mutation);
        string jsonArgs = JsonConvert.SerializeObject(new{objects = new{id = id.text, name = username.text}});
        query.SetArgs(GraphApi.JsonToArgument(jsonArgs));
        UnityWebRequest request = await userApi.Post(query);
        loading.SetActive(false);
        mutationDisplay.text = HttpHandler.FormatJson(request.downloadHandler.text);
    }

    public async void Subscribe(){
        loading.SetActive(true);
        subscribed = true;
        await userApi.Subscribe("SubscribeToUsers", GraphApi.Query.Type.Subscription);
        loading.SetActive(false);
        while (subscribed){
            string data = await HttpHandler.GetWsReturn();
            subscriptionDisplay.text = HttpHandler.FormatJson(data);
        }
    }

    public async void CancelSubscribe(){
        subscribed = false;
        await HttpHandler.WebsocketDisconnect();
    }
}
