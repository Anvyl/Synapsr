using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Synapsr
{
	public class StatusToColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			string val = value as string;
			if (val == null)
				return null;
			switch(val)
			{
				case "active":
					return new SolidColorBrush(Colors.Green);
				default:
					return new SolidColorBrush(Colors.Gray);
			}

		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}
