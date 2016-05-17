
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Synapsr.Core.Chat;
using Synapsr.Core.Chat.Models;
using Synapsr.Core.Chat.Models.Slack;
using Synapsr.Core.ServiceConnector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Security.Authentication.Web;
using Windows.Storage.Streams;

namespace Synapsr.Core.Chat
{
	public interface IMessenger
	{
		User CurrentUser { get; }
		List<Channel> Channels { get; }
		List<User> Users { get; }
		List<Im> ImChannels { get; }
		List<Bot> Bots { get; }
		Task SendChannelMessage(string text, string channel);
		Task Connect();
		Task<MessageModel> ReceiveMessage();

		Task<History> GetHistory(string channel);
		Task<History> GetHistoryIM(string id);
	}



	public class SlackMessenger : IMessenger
	{

		public User CurrentUser => _endpoint.users.Single(x => x.id == _endpoint.self.id);
		public List<Channel> Channels => _endpoint.channels.ToList();
		public List<Im> ImChannels => _endpoint.ims.ToList();
		public List<User> Users => _endpoint.users.ToList();
		public List<Bot> Bots => _endpoint.bots.ToList();


		int _id = 1;
		IServiceConnector _connector;
		SlackRTMEndPoint _endpoint;
		DataWriter _messageWriter;
		MessageWebSocket socket = new MessageWebSocket();
		HttpClient client = new HttpClient();
		string _token = string.Empty;

		Queue<MessageModel> messages = new Queue<MessageModel>();

		public SlackMessenger(IServiceConnector connector)
		{
			_connector = connector;
			socket.Control.MessageType = SocketMessageType.Utf8;
			socket.MessageReceived += MessageReceived;
			_messageWriter = new DataWriter(socket.OutputStream);
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
			_token = token.access_token;
			_endpoint = await AquireRTM(token);

			Uri serverUri = new Uri(_endpoint.url);
			await socket.ConnectAsync(serverUri);

		}

		public async Task SendChannelMessage(string text, string channel)
		{
			var message = new OutGoingMessage()
			{
				type = "message",
				id = _id++,
				channel = _endpoint.channels.SingleOrDefault(x=>x.id == channel)?.id ?? _endpoint.ims.Single(x=>x.id == channel).id,
				text = text
			};
			var jsonMessage = JsonConvert.SerializeObject(message);
			await SendJson(jsonMessage);
		}

		public async Task<MessageModel> ReceiveMessage()
		{
			while(messages.Count < 1)
				await Task.Delay(100);

			return messages.Dequeue();
		}

		public async Task<History> GetHistory(string channel)
		{
			var response = await client.GetAsync("https://slack.com/api/channels.history?token=" + _token + "&count=20&channel=" + channel);
			var content = await response.Content.ReadAsStringAsync();
			var history = JsonConvert.DeserializeObject<History>(content);
			return history;
		}

		public async Task<History> GetHistoryIM (string channel)
		{
			var response = await client.GetAsync("https://slack.com/api/im.history?token=" + _token + "&count=20&channel=" + channel);
			var content = await response.Content.ReadAsStringAsync();
			var history = JsonConvert.DeserializeObject<History>(content);
			return history;
		}


		private async Task SendJson(string jsonMessage)
		{
			_messageWriter.WriteString(jsonMessage);
			await _messageWriter.StoreAsync();
		}

		DataReader messageReader;

		private void MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
		{
			try
			{
				messageReader = args.GetDataReader();
			}
			catch
			{
				return;
			}

			string result = messageReader.ReadString(messageReader.UnconsumedBufferLength);
			JObject jobject = JObject.Parse(result);
			JToken t = jobject["type"];
			switch (t?.Value<string>())
			{
				case "hello":
					break;
				case "message":
					var subtype = jobject["subtype"];
					switch (subtype?.Value<string>())
					{
						case "bot_message":

							var botmessage = JsonConvert.DeserializeObject<BotMessage>(result);
							var bot = Bots.SingleOrDefault(x => x.id == botmessage.bot_id);
							if (bot == null)
								return;
							var msg = new MessageModel
							{
								Username = bot.name,
								AvatarUrl = bot.icons.image_48,
								Channel = ImChannels.SingleOrDefault(x => x.id == botmessage.channel).id,
								Message = botmessage.text
							};
							messages.Enqueue(msg);
							break;
						default:
							var message = JsonConvert.DeserializeObject<Message>(result);
							var user = _endpoint.users.First(x => x.id == message.user);

							var msgModel = new MessageModel
							{
								Username = user.name,
								AvatarUrl = user.profile.image_48,
								Message = message.text,
								Channel = message.channel,
							};
							messages.Enqueue(msgModel);
							break;
					}


					
					break;
				default:
					break;
			}
			
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
