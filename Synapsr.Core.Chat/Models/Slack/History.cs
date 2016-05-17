using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapsr.Core.Chat.Models.Slack
{

	public class History
	{
		public bool ok { get; set; }
		public ChannelMessage[] messages { get; set; }
		public bool has_more { get; set; }
	}

	public class ChannelMessage
	{
		public string text { get; set; }
		public string bot_id { get; set; }
		public string type { get; set; }
		public string subtype { get; set; }
		public string user { get; set; }
		public string ts { get; set; }
		public bool mrkdwn { get; set; }
		public Attachment[] attachments { get; set; }
	}

	public class Attachment
	{
		public string fallback { get; set; }
		public string text { get; set; }
		public int id { get; set; }
		public string color { get; set; }
	}

}
