using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnicornOverlord
{
	internal class Bond : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		private readonly uint mAddress;
		public Bond(uint address)
		{
			// ID P, Bond Value Q, Read Rapport ?
			// PP PP PP PP QQ QQ ?? ?? ...
			mAddress = address;
		}

		public uint ID
		{
			get => SaveData.Instance().ReadNumber(mAddress, 4);
		}

		public uint Value
		{
			get => SaveData.Instance().ReadNumber(mAddress + 4, 2);
			set
			{
				Util.WriteNumber(mAddress + 4, 2, value, 0, 900);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
			}
		}
	}
}
