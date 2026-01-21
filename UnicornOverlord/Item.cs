using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnicornOverlord
{
	internal class Item : ObservableObject
	{
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
				OnPropertyChanged();
				OnPropertyChanged(nameof(Name));
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
			set
			{
				SaveData.Instance.WriteNumber(mAddress + 4, 4, value);
				OnPropertyChanged();
			}
		}

		public uint Count
		{
			get => SaveData.Instance.ReadNumber(mAddress + 8, 3);
			set
			{
				Util.WriteNumber(mAddress + 8, 3, value, 0, (uint)1 << (8 * 3));
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Equip slot of an unit the is item in. FF if the item was never
		/// equipped.
		/// </summary>
		public byte EquipSlotIndex
		{
			get => (byte)SaveData.Instance.ReadNumber(mAddress + 11, 1);
			set
			{
				SaveData.Instance.WriteNumber(mAddress + 11, 1, value);
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Id of the character that the item is equipped by. <br/>
		/// Value is `uint.MaxValue` if the item is not equipped by any character.
		/// </summary>
		public uint EquippedCharId
		{
			get => SaveData.Instance.ReadNumber(mAddress + 12, 4);
			set
			{
				SaveData.Instance.WriteNumber(mAddress + 12, 4, value);
				OnPropertyChanged();
			}
		}

		// Whether the item is favorited, has been viewed, etc
		// Seems to be a bit array? (Of 4 bytes!?)
		// 1st bit - ??
		// 2nd bit - 0b000010 (02) - unviewed/new
		// 3rd bit - 0b000100 (04) - viewed
		// 6th bit - 0b100100 (24) - favorite & viewed
		public uint Status
		{
			get => SaveData.Instance.ReadNumber(mAddress + 16, 4);
			set
			{
				SaveData.Instance.WriteNumber(mAddress + 16, 4, value);
				OnPropertyChanged();
			}
		}
	}
}