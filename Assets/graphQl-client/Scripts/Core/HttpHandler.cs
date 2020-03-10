using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GraphQlClient.EventCallbacks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace GraphQlClient.Core
{
	public class HttpHandler
	{
		ClientWebSocket cws = null;
		ArraySegment<byte> buf = new ArraySegment<byte>(new byte[1024]);
		
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
        
        #region Websocket

        //Use this to subscribe to a graphql endpoint
		public async Task WebsocketConnect(string subscriptionUrl, string details){
			cws = new ClientWebSocket();
			cws.Options.AddSubProtocol("graphql-subscriptions");
			Uri u = new Uri(subscriptionUrl);
			try{
				await cws.ConnectAsync(u, CancellationToken.None);
				if (cws.State == WebSocketState.Open)
					Debug.Log("connected");
				await WebsocketInit();
				await WebsocketSend(details);		
			}
			catch (Exception e){
				Debug.Log("woe " + e.Message);
			}
		}

		async Task WebsocketInit(){
			string jsonData = "{\"type\":\"init\"}";
			ArraySegment<byte> b = new ArraySegment<byte>(Encoding.ASCII.GetBytes(jsonData));
			await cws.SendAsync(b, WebSocketMessageType.Text, true, CancellationToken.None);
			await GetWsReturn();
		}
		
		async Task WebsocketSend(string details){
			string jsonData = JsonConvert.SerializeObject(new {id = "1",  type = "subscription_start", query = details});
			Debug.Log(jsonData);
			ArraySegment<byte> b = new ArraySegment<byte>(Encoding.ASCII.GetBytes(jsonData));
			await cws.SendAsync(b, WebSocketMessageType.Text, true, CancellationToken.None);
		}
		
		//use this to get information from the websocket
		public async Task<string> GetWsReturn(){
			buf = WebSocket.CreateClientBuffer(1024, 1024);
			WebSocketReceiveResult r;
			string result = "";
			do{
				r = await cws.ReceiveAsync(buf, CancellationToken.None);
				result += Encoding.UTF8.GetString(buf.Array ?? throw new ApplicationException("Buf = null"), buf.Offset, r.Count);
			} while (!r.EndOfMessage);
			
			JObject obj = new JObject();
			try{
				obj = JObject.Parse(result);
			}
			catch (JsonReaderException e){
				throw new ApplicationException(e.Message);
			}

			string subType = (string) obj["type"];
			switch (subType){
				case "init_success":{
					Debug.Log("init_success, the handshake is complete");
					break;
				}
				case "init_fail": {
					throw new ApplicationException("The handshake failed. Error: " + result);
				}
				case "subscription_data":{
					Debug.Log("subscription data has been received");
					return result;
				}
				case "subscription_success":{
					Debug.Log("subscription_success");
					result = await GetWsReturn();
					break;
				}
				case "subscription_fail": {
					throw new ApplicationException("The subscription data failed");
				}
			}
			return result;
		}

		public async Task WebsocketDisconnect(){
			await cws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
		}
		
		#endregion
	}
}
