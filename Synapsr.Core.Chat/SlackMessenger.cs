
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Synapsr.Core.Chat;
using Synapsr.Core.Chat.Models;
using Synapsr.Core.ServiceConnector;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Security.Authentication.Web;
using Windows.Storage.Streams;

namespace Synapsr.Core.Chat
{
	public interface IMessenger
	{
		Task SendMessage(string text);
		Task Connect();
	}

	public class SlackMessenger : IMessenger
	{
		IServiceConnector _connector;
		SlackRTMEndPoint _endpoint;
		MessageWebSocket socket = new MessageWebSocket();
		public SlackMessenger(IServiceConnector connector)
		{
			_connector = connector;
			socket.Control.MessageType = SocketMessageType.Utf8;
			socket.MessageReceived += MessageReceived;

		}

		public async Task SendMessage(string text)
		{
			var message = new OutGoingMessage()
			{
				type = "message",
				id = 1,
				channel = _endpoint.channels[0].id,
				text = text
			};
			var jsonMessage = JsonConvert.SerializeObject(message);
			await SendJson(jsonMessage);
		}

		public async Task Connect()
		{
			Token token;
			var cachedToken = TokenManager.GetToken(TokenManager.TokenType.Slack);
			if (cachedToken != string.Empty) //TODO check if token is cached in settings
			{
				token = new Token { access_token = cachedToken };
			}
			else //GET tocket
			{
				token = await AquireToken();
				TokenManager.AddToken(token.access_token, TokenManager.TokenType.Slack);
			}

			_endpoint = await AquireRTM(token);

			Uri serverUri = new Uri(_endpoint.url);
			await socket.ConnectAsync(serverUri);
			
		}

		private async Task SendJson(string jsonMessage)
		{
			DataWriter messageWriter = new DataWriter(socket.OutputStream);
			messageWriter.WriteString(jsonMessage);
			await messageWriter.StoreAsync();
		}

		private void MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
		{
			DataReader messageReader = args.GetDataReader();
			string result = messageReader.ReadString(messageReader.UnconsumedBufferLength);
			JObject jobject = JObject.Parse(result);
			JToken t = jobject["type"];
			switch (t?.Value<string>())
			{
				case "hello":
					break;
				default:
					break;
			}


			Debug.WriteLine(result);
		}

		public async Task<Token> AquireToken()
		{
			string startURL = "https://slack.com/oauth/authorize?client_id=42964459713.42961925893&scope=client";
			string endURL = "http://facebook.com";
			string tokenURL = "https://slack.com/api/oauth.access?client_id=42964459713.42961925893&client_secret=bb2eb7758bb58135d456c329a9cb1c50&code=";
			return await _connector.Connect(startURL, endURL, tokenURL);
		}

		public async Task<SlackRTMEndPoint> AquireRTM(Token token)
		{
			string rtmURL = "https://slack.com/api/rtm.start?token=" + token.access_token;
			HttpClient client = new HttpClient();
			HttpResponseMessage response = await client.GetAsync(rtmURL);
			if (response.IsSuccessStatusCode)
			{
				string json = await response.Content.ReadAsStringAsync();
				return JsonConvert.DeserializeObject<SlackRTMEndPoint>(json);
			}
			return null;

		}
	}
}
