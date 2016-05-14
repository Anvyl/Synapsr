using Synapsr.Core.Chat;
using Synapsr.Core.ServiceConnector;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Synapsr
{
    public class Test
    {
        public string Username { get; set; }
        public string Message { get; set; }
        public string AvatarUrl { get; set; }
    }
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
			this.Loaded += PageLoaded;

        }

		private void PageLoaded(object sender, RoutedEventArgs e)
		{
            ObservableCollection<Test> items = new ObservableCollection<Test>();
            items.Add(new Test() { Username = "aodpi", Message = "zdarova natasha", AvatarUrl = "https://avatars1.githubusercontent.com/u/6562956?v=3&s=40" });
            items.Add(new Test() { Username = "aodpi", Message = "zdarova natasha", AvatarUrl = "https://avatars1.githubusercontent.com/u/6562956?v=3&s=40" });
            items.Add(new Test() { Username = "aodpi", Message = "zdarova natasha", AvatarUrl = "https://avatars1.githubusercontent.com/u/6562956?v=3&s=40" });
            items.Add(new Test() { Username = "aodpi", Message = "zdarova natasha", AvatarUrl = "https://avatars1.githubusercontent.com/u/6562956?v=3&s=40" });
            MessagesContainer.ItemsSource = items;
            IServiceConnector connector = new ServiceConnector();
			IMessenger messenger = new SlackMessenger(connector);
			messenger.Connect();
		}
	}
}
