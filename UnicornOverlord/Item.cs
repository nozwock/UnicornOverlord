using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnicornOverlord
{
	internal class Item : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		private readonly uint mAddress;

		public Item(uint address)
		{
			mAddress = address;
		}

		public uint ID
		{
			get => SaveData.Instance.ReadNumber(mAddress, 4);
			set
			{
				SaveData.Instance.WriteNumber(mAddress, 4, value);
				_name = null;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ID)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
			}
		}

		private string? _name;
		public string Name
		{
			get
			{
				_name ??= Info.Instance().Search(Info.Instance().Item, ID)?.Name ?? ID.ToString();
				return _name;
			}
		}

		public uint Index
		{
			get => SaveData.Instance.ReadNumber(mAddress + 4, 4);
			set => SaveData.Instance.WriteNumber(mAddress + 4, 4, value);
		}

		public uint Count
		{
			get => SaveData.Instance.ReadNumber(mAddress + 8, 3);
			set
			{
				Util.WriteNumber(mAddress + 8, 3, value, 0, (uint)1 << (8 * 3));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
			}
		}

		/// <summary>
		/// Equip slot of an unit the is item in. FF if the item was never
		/// equipped.
		/// </summary>
		public byte EquipSlotIndex
		{
			get => (byte)SaveData.Instance.ReadNumber(mAddress + 11, 1);
		}

		/// <summary>
		/// Some portrait icon id for the unit that the item is equipped by. <br/>
		/// Value is `uint.MaxValue` if the item is not equipped by any unit.
		/// </summary>
		public uint UnitEquippedIconId
		{
			get => SaveData.Instance.ReadNumber(mAddress + 12, 4);
		}

		public uint Status
		{
			get => SaveData.Instance.ReadNumber(mAddress + 16, 4);
			set => SaveData.Instance.WriteNumber(mAddress + 16, 4, value);
		}
	}
}