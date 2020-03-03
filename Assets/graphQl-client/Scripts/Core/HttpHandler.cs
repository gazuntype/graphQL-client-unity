using System;
using System.Text;
using System.Threading.Tasks;
using GraphQlClient.EventCallbacks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace GraphQlClient.Core
{
	public class HttpHandler
	{
		public static async Task<UnityWebRequest> PostAsync(string url, string details, string authToken = null){
            string jsonData = JsonConvert.SerializeObject(new{query = details});
            byte[] postData = Encoding.ASCII.GetBytes(jsonData);
            UnityWebRequest request = UnityWebRequest.Post(url, UnityWebRequest.kHttpVerbPOST);
            request.uploadHandler = new UploadHandlerRaw(postData);
            request.SetRequestHeader("Content-Type", "application/json");
            if (!String.IsNullOrEmpty(authToken)) 
                request.SetRequestHeader("Authorization", "Bearer " + authToken);
            
            OnRequestBegin  requestBegin = new OnRequestBegin();
            requestBegin.FireEvent();
            
            try{
                await request.SendWebRequest();
            }
            catch(Exception e){
                Debug.Log("Testing exceptions");
                OnRequestEnded requestFailed = new OnRequestEnded(e);
                requestFailed.FireEvent();
            }
			Debug.Log(request.downloadHandler.text);
            
            OnRequestEnded requestSucceeded = new OnRequestEnded(request.downloadHandler.text);
            requestSucceeded.FireEvent();
            return request;
        }

        public static async Task<UnityWebRequest> GetAsync(string url, string authToken){
            UnityWebRequest request = UnityWebRequest.Get(url);
            if (!String.IsNullOrEmpty(authToken)) 
                request.SetRequestHeader("Authorization", "Bearer " + authToken);
            OnRequestBegin  requestBegin = new OnRequestBegin();
            requestBegin.FireEvent();
            try{
                await request.SendWebRequest();
            }
            catch(Exception e){
                Debug.Log("Testing exceptions");
                OnRequestEnded requestEnded = new OnRequestEnded(e);
                requestEnded.FireEvent();
            }
            Debug.Log(request.downloadHandler.text);
            OnRequestEnded requestSucceeded = new OnRequestEnded(request.downloadHandler.text);
            requestSucceeded.FireEvent();
            return request;
        }
	}
}
