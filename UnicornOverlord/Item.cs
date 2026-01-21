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
				_name ??= Info.Item.TryGetValue(ID, out var info) ? info.Name : ID.ToString();
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
		/// Character ID of who has the item equipped, `uint.MaxValue` if not
		/// equipped. <br/>
		/// It's only used to know which character's icon to show on the item.
		/// Refer to pattern file for details.
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

		public uint _StatusBitField
		{
			get => SaveData.Instance.ReadNumber(mAddress + 16, 4);
			set
			{
				SaveData.Instance.WriteNumber(mAddress + 16, 4, value);
				OnPropertyChanged();
			}
		}
		public bool StatusUnseen
		{
			get => SaveData.Instance.ReadBit(mAddress + 16, 1); // 0 indexed
			set
			{
				SaveData.Instance.WriteBit(mAddress + 16, 1, value);
				OnPropertyChanged();
			}
		}
		public bool StatusSeen
		{
			get => SaveData.Instance.ReadBit(mAddress + 16, 2);
			set
			{
				SaveData.Instance.WriteBit(mAddress + 16, 2, value);
				OnPropertyChanged();
			}
		}
		public bool StatusUpgraded
		{
			get => SaveData.Instance.ReadBit(mAddress + 16, 4);
			set
			{
				SaveData.Instance.WriteBit(mAddress + 16, 4, value);
				OnPropertyChanged();
			}
		}
		public bool StatusFavorite
		{
			get => SaveData.Instance.ReadBit(mAddress + 16, 5);
			set
			{
				SaveData.Instance.WriteBit(mAddress + 16, 5, value);
				OnPropertyChanged();
			}
		}
	}
}