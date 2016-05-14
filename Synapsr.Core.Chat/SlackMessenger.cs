
using Newtonsoft.Json;
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
		void SendMessage(string message);
		Task Connect();
	}

	public class SlackMessenger : IMessenger
	{
		IServiceConnector _connector;
		MessageWebSocket socket = new MessageWebSocket();
		public SlackMessenger(IServiceConnector connector)
		{
			_connector = connector;
		}

		public void SendMessage(string message)
		{

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

			SlackRTMEndPoint endpoint = await AquireRTM(token);
			socket.Control.MessageType = SocketMessageType.Utf8;
			socket.MessageReceived += Socket_MessageReceived;
			
			Uri serverUri = new Uri(endpoint.url);
			await socket.ConnectAsync(serverUri);
			var message = new Message()
			{
				type = "message",
				id = 1,
				channel = endpoint.channels[0].id,
				text = "darova natasha"
			};
			var jsonMessage = JsonConvert.SerializeObject(message);

			DataWriter messageWriter = new DataWriter(socket.OutputStream);
			messageWriter.WriteString(jsonMessage);
			await messageWriter.StoreAsync();
			Debug.WriteLine(jsonMessage);
		}

		private void Socket_MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
		{
			var messageReader = args.GetDataReader();
			var resutl = messageReader.ReadString(messageReader.UnconsumedBufferLength);
			Debug.WriteLine(resutl);
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
