using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnicornOverlord
{
	internal class Bond : ObservableObject
	{
		private readonly uint mAddress;
		public Bond(uint address)
		{
			// ID P, Bond Value Q, Rapport Progression
			// PP PP PP PP QQ QQ RR RR
			mAddress = address;
		}

		public uint ID
		{
			get => SaveData.Instance.ReadNumber(mAddress, 4);
		}

		public ushort Value
		{
			get => (ushort)SaveData.Instance.ReadNumber(mAddress + 4, 2);
			set
			{
				Util.WriteNumber(mAddress + 4, 2, value, 0, 900);
				OnPropertyChanged();
			}
		}
	}
}
