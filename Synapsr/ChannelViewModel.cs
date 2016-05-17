using Synapsr.Core.Chat.Models.Slack;
using System.Collections.ObjectModel;

namespace Synapsr
{
	public class BaseChatViewModel : BindableBase
	{
		public ObservableCollection<MessageModel> Messages { get; set; } = new ObservableCollection<MessageModel>();

		public string Name { get; set; }
		public string Id { get; set; }
	}

	public class ChannelChatViewModel : BaseChatViewModel
	{

	}

	public class ImChatViewModel : BaseChatViewModel
	{
		public string Status { get; set; }
	}
}
