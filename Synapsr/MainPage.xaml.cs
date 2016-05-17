using Synapsr.Core.Chat;
using Synapsr.Core.Chat.Models.Slack;
using Synapsr.Core.ServiceConnector;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;


namespace Synapsr
{

	public class MainPageViewModel : BindableBase
	{
		private BaseChatViewModel _selectedChannel;
		public BaseChatViewModel SelectedChannel { get { return _selectedChannel; } set { _selectedChannel = value; OnPropertyChanged(); } }
	}

	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		IServiceConnector _connector;
		IMessenger _chatClient;
		MainPageViewModel viewModel = new MainPageViewModel();


		public MainPage()
		{
			this.InitializeComponent();
			this.Loaded += PageLoaded;
			_connector = new ServiceConnector();
			_chatClient = new SlackMessenger(_connector);
			DataContext = viewModel;
		}

		private async void PageLoaded(object sender, RoutedEventArgs e)
		{
			ProgressRing.IsActive = true;
			await _chatClient.Connect();

			List<object> channels = new List<object>();
			channels.AddRange(_chatClient.Channels);
			channels.AddRange(_chatClient.ImChannels);
			List<BaseChatViewModel> chats = channels.Select<object, BaseChatViewModel>(x=> {
				if (x is Im)
					return new ImChatViewModel() { Id = (x as Im).id, Name = _chatClient.Users.Single(y=>y.id == (x as Im).user).name, Status = _chatClient.Users.Single(y => y.id == (x as Im).user).presence };
				else if (x is Channel)
					return new ChannelChatViewModel() { Id = (x as Channel).id, Name = (x as Channel).name };
				else
					return null;
			}).ToList();


			ChannelsList.ItemsSource = chats;
			ChannelsList.SelectedItem = chats[0];
			viewModel.SelectedChannel = chats[0];

			ProgressRing.IsActive = false;
			ChannelsList.SelectionChanged += (s, args) => { splitView.IsPaneOpen = !splitView.IsPaneOpen; };
			foreach (var item in chats)
			{
				History history = null;
				if (item is ChannelChatViewModel)
					history = await _chatClient.GetHistory(item.Id);
				else if (item is ImChatViewModel)
					history = await _chatClient.GetHistoryIM(item.Id);

				foreach (var entry in history.messages.OrderBy(x=>x.ts))
				{
					var user = _chatClient.Users.SingleOrDefault(x => x.id == entry.user);
					if(user == null)
					{
						Bot bot = _chatClient.Bots.SingleOrDefault(x => x.id == entry.bot_id);
						item.Messages.Add(new MessageModel()
						{
							Username = bot.name,
							Message = entry.text,
							AvatarUrl = bot.icons.image_48,
							Channel = item.Id
						});
					}
					else
						item.Messages.Add(new MessageModel()
						{
							Username = user.name,
							Message = entry.text,
							AvatarUrl = user.profile.image_48,
							Channel = item.Id
						});
				}
			}

			ScrollContainer.ChangeView(null, ScrollContainer.ExtentHeight, 1, false);

			while (true)
			{
				var message = await _chatClient.ReceiveMessage();
				chats.SingleOrDefault(x => x.Id == message.Channel)?.Messages.Add(message);
				await Task.Delay(50);
				ScrollContainer.ChangeView(null, ScrollContainer.ExtentHeight, 1, false);
			}
		}
		

		private async void InputBoxKeyDown(object sender, KeyRoutedEventArgs e)
		{
			if (e.Key == VirtualKey.Enter && e.KeyStatus.RepeatCount == 1)
			{
				await _chatClient.SendChannelMessage((sender as TextBox).Text, viewModel.SelectedChannel.Id);

				viewModel.SelectedChannel.Messages.Add(new MessageModel
				{
					Username = _chatClient.CurrentUser.name,
					AvatarUrl = _chatClient.CurrentUser.profile.image_48,
					Message = (sender as TextBox).Text,
					Channel = viewModel.SelectedChannel.Id
				});

				(sender as TextBox).Text = string.Empty;
				ScrollContainer.ChangeView(null, ScrollContainer.ExtentHeight, 1, false);
			}
		}

		private void Hamburger_Click(object sender, RoutedEventArgs e)
		{
			splitView.IsPaneOpen = !splitView.IsPaneOpen;
		}

	}
}
