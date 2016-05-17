namespace Synapsr.Core.Chat.Models.Slack
{
	public class Message
	{
		public string type { get; set; }
		public string channel { get; set; }
		public string user { get; set; }
		public string text { get; set; }
		public string ts { get; set; }
		public string team { get; set; }
	}

	public class MessageModel
	{
		public string Username { get; set; }
		public string Message { get; set; }
		public string AvatarUrl { get; set; }
		public string Channel { get; set; }

	}


	public class BotMessage
	{
		public string text { get; set; }
		public string username { get; set; }
		public string bot_id { get; set; }
		public string type { get; set; }
		public string subtype { get; set; }
		public string team { get; set; }
		public BotProfile user_profile { get; set; }
		public string channel { get; set; }
		public string ts { get; set; }
	}

	public class BotProfile
	{
		public object avatar_hash { get; set; }
		public string image_72 { get; set; }
		public object first_name { get; set; }
		public string real_name { get; set; }
		public object name { get; set; }
	}

}
