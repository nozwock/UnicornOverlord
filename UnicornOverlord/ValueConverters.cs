using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace UnicornOverlord
{
	class ClassIDConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			uint id = (uint)value;
			if (Info.Class.TryGetValue(id, out var info)) return info.Name;
			return id.ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	internal class ItemEquipSlotConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var slot = (byte)value;
			return slot == byte.MaxValue ? "N/A" : (slot + 1).ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

    internal class CharIdNameMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			var char_id = (uint)values[0];
			if (char_id == uint.MaxValue || char_id == 0)
				return "N/A";
			var charsById = (Dictionary<uint, Character>)values[1];
			if (charsById.TryGetValue(char_id, out var ch))
				return ch.Name;
			return char_id.ToString();
		}

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
