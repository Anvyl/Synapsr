namespace Synapsr.Core.Chat.Models
{
	public class OutGoingMessage
	{
		public int id { get; set; }
		public string type { get; set; }
		public string channel { get; set; }
		public string text { get; set; }
	}
}
