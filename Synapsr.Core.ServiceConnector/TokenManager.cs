using Windows.Storage;

namespace Synapsr.Core.ServiceConnector
{
	public class TokenManager
	{
		public enum TokenType
		{
			Slack,
			Asana,
			Bot
		}
		public static void AddToken(string token, TokenType type)
		{
			switch (type)
			{
				case TokenType.Slack:
					ApplicationData.Current.LocalSettings.Values["slack_token"] = token;
					break;
				case TokenType.Asana:
					ApplicationData.Current.LocalSettings.Values["asana_token"] = token;
					break;
				case TokenType.Bot:
					ApplicationData.Current.LocalSettings.Values["bot_token"] = token;
					break;
				default:
					break;
			}
		}

		public static string GetToken(TokenType type)
		{
			switch (type)
			{
				case TokenType.Slack:
					return ApplicationData.Current.LocalSettings.Values["slack_token"]?.ToString() ?? string.Empty;
				case TokenType.Asana:
					return ApplicationData.Current.LocalSettings.Values["asana_token"]?.ToString() ?? string.Empty;
				case TokenType.Bot:
					return ApplicationData.Current.LocalSettings.Values["bot_token"]?.ToString() ?? string.Empty;
				default:
					return string.Empty;
			}
		}
	}
}
