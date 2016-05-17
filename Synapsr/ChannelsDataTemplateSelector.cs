using Synapsr.Core.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Synapsr
{
	public class ChannelsDataTemplateSelector : DataTemplateSelector
	{
		public DataTemplate UserTemplate { get; set; }
		public DataTemplate ChannelTemplate { get; set; }

		protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
		{
			if (item is ChannelChatViewModel)
				return ChannelTemplate;
			else if (item is ImChatViewModel)
				return UserTemplate;

			return base.SelectTemplateCore(item, container);
		}

	}
}
