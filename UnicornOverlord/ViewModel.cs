using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace UnicornOverlord
{
	internal class ViewModel : ObservableObject
	{
		const string baseWindowTitle = "UnicornOverlord Save Editor (Nintendo Switch)";
		public string WindowTitle
		{
			get
			{
				return string.IsNullOrEmpty(WindowTitlePrefix)
					? baseWindowTitle
					: WindowTitlePrefix
						+ (SaveData.Instance.IsDirty ? "(*)" : "")
						+ " - "
						+ baseWindowTitle;
			}
		}
		public string WindowTitlePrefix
		{
			get;
			set
			{
				SetProperty(ref field, value);
				OnPropertyChanged(nameof(WindowTitle));
			}
		} = "";

		private readonly Info Info = Info.Instance();
		public ICommand OpenFileCommand { get; set; }
		public ICommand SaveFileCommand { get; set; }
		public ICommand SaveAsFileCommand { get; set; }
		public ICommand ChoiceItemCommand { get; set; }
		public ICommand ChoiceEquipmentCommand { get; set; }
		public ICommand ChoiceClassCommand { get; set; }
		public ICommand AppendItemCommand { get; set; }
		public ICommand AppendEquipmentCommand { get; set; }
		public ICommand ExportCharacterCommand { get; set; }
		public ICommand ImportCharacterCommand { get; set; }
		public ICommand InsertCharacterCommand { get; set; }
		public ICommand ChangeItemCountMaxCommand { get; set; }
		public ICommand ChangeCharacterBondMaxCommand { get; set; }
		public ICommand ChangeCharacterBondMaxAllCommand { get; set; }

		public Basic Basic { get; set; } = new Basic();
		public ObservableCollection<Character> Characters { get; set; } = new ObservableCollection<Character>();
		public Dictionary<uint, Character> CharactersById { get; set; } = [];
		public ObservableCollection<Item> Items { get; set; } = new ObservableCollection<Item>();
		public ObservableCollection<Item> Equipments { get; set; } = new ObservableCollection<Item>();
		public ObservableCollection<Unit> Units { get; set; } = new ObservableCollection<Unit>();

		public ICollectionView ItemsView { get; }
		public string ItemFilterText
		{
			get;
			set
			{
				SetProperty(ref field, value);
				ItemsView.Refresh();
			}
		} = "";

		public ICollectionView EquipmentsView { get; }
		public string EquipmentFilterText
		{
			get;
			set
			{
				SetProperty(ref field, value);
				EquipmentsView.Refresh();
			}
		} = "";

		public ViewModel()
		{
			OpenFileCommand = new ActionCommand(OpenFile);
			SaveFileCommand = new ActionCommand(SaveFile);
			SaveAsFileCommand = new ActionCommand(SaveAsFile);
			ChoiceItemCommand = new ActionCommand(ChoiceItem);
			ChoiceEquipmentCommand = new ActionCommand(ChoiceEquipment);
			ChoiceClassCommand = new ActionCommand(ChoiceClass);
			AppendItemCommand = new ActionCommand(AppendItem);
			AppendEquipmentCommand = new ActionCommand(AppendEquipment);
			ExportCharacterCommand = new ActionCommand(ExportCharacter);
			ImportCharacterCommand = new ActionCommand(ImportCharacter);
			InsertCharacterCommand = new ActionCommand(InsertCharacter);
			ChangeItemCountMaxCommand = new ActionCommand(ChangeItemCountMax);
			ChangeCharacterBondMaxCommand = new ActionCommand(ChangeCharacterBondMax);
			ChangeCharacterBondMaxAllCommand = new ActionCommand(ChangeCharacterBondMaxAll);

			ItemsView = CollectionViewSource.GetDefaultView(Items);
			ItemsView.Filter = ItemFilter(() => ItemFilterText);
			EquipmentsView = CollectionViewSource.GetDefaultView(Equipments);
			EquipmentsView.Filter = ItemFilter(() => EquipmentFilterText);

			Predicate<object> ItemFilter(Func<string> value)
			{
				return obj =>
				{
					if (obj is not Item item)
						return false;
					if (string.IsNullOrWhiteSpace(value()))
						return true;

					return item.Name.Contains(
						value(),
						StringComparison.OrdinalIgnoreCase);
				};
			}

			SaveData.Instance.PropertyChanged += (sender, e) => {
				if (e.PropertyName == nameof(SaveData.FilePath))
					WindowTitlePrefix = Path.GetFileName(SaveData.Instance.FilePath);
				else if (e.PropertyName == nameof(SaveData.IsDirty))
					OnPropertyChanged(nameof(WindowTitle));
			};
		}

		private void Initialize()
		{
			CharactersById.Clear();
			Characters.Clear();
			Items.Clear();
			Equipments.Clear();
			Units.Clear();

			// create character
			// counter ??
			for (uint i = 0; i < 500; i++)
			{
				var ch = new Character(Util.calcCharacterAddress(i));
				if (ch.ID == 0xFFFFFFFF) break;

				CharactersById.Add(ch.ID, ch);
				Characters.Add(ch);
			}

			SetCharacterBonds(Characters);

			// create item
			for (uint i = 0; i < 3800; i++)
			{
				var item = new Item(0xA0 + i * 20);
				if (item.Index == 0) break;

				if(item.Count== 0)
					Equipments.Add(item);
				else
					Items.Add(item);
			}

			// create unit
			for (uint i = 0; i < 10; i++)
			{
				var unit = new Unit(0x10D89A + i * 1720);
				Units.Add(unit);
			}

			OnPropertyChanged(nameof(Basic));
		}

		static void SetCharacterBonds(IList<Character> characters)
		{
			static Dictionary<uint, ObservableCollection<Bond>> BondsMapping()
			{
				var bondsMap = new Dictionary<uint, ObservableCollection<Bond>>();
				for (uint char_idx = 0; char_idx < 164; char_idx++)
				{
					uint baseAddr = Util.calcBondAddress(char_idx);
					uint char_id = SaveData.Instance.ReadNumber(baseAddr, 4);
					if (char_id == 0xFFFFFFFF) break;

					var bonds = new ObservableCollection<Bond>();
					bondsMap.Add(char_id, bonds);
					for (uint bond_idx = 0; bond_idx < 164; bond_idx++)
					{
						uint bondAddr = baseAddr + 4 + bond_idx * 8;
						char_id = SaveData.Instance.ReadNumber(bondAddr, 4);
						if (char_id == 0xFFFFFFFF) break;

						bonds.Add(new Bond(bondAddr));
					}
				}

				return bondsMap;
			}

			var bondsMap = BondsMapping();
			foreach (var ch in characters)
			{
				if (bondsMap.TryGetValue(ch.ID, out var bonds))
					ch.Bonds = bonds;
			}
		}

		private void OpenFile(object? parameter)
		{
			var dlg = new OpenFileDialog();
			dlg.Filter = "UCSAVEFILE|UCSAVEFILE*.DAT";
			if (dlg.ShowDialog() == false) return;

			SaveData.Instance.Open(dlg.FileName);
			Initialize();
		}

		private void SaveFile(object? parameter)
		{
			SaveData.Instance.Save();
		}

		private void SaveAsFile(object? parameter)
		{
			var dlg = new SaveFileDialog();
			dlg.Filter = "UCSAVEFILE|UCSAVEFILE*.DAT";
			if (dlg.ShowDialog() == false) return;

			SaveData.Instance.SaveAs(dlg.FileName);
		}

		private void ChoiceItem(object? parameter)
		{
			Item? item = parameter as Item;
			if(item == null) return;

			ChoiceItem(ChoiceWindow.eType.eItem, item);
		}

		private void ChoiceEquipment(object? parameter)
		{
			Item? item = parameter as Item;
			if (item == null) return;

			ChoiceItem(ChoiceWindow.eType.eEquipment, item);
			// NOTE: Leave Status as is when replacing, it's not some item-type
			// identifier as presumed here and in some other places
			//
			// var info = Info.Search(Info.Kind, item.ID);
			// if (info != null)
			// {
			// 	item.Status = uint.Parse(info.Name);
			// }
		}

		private void ChoiceItem(ChoiceWindow.eType type, Item item)
		{
			var dlg = new ChoiceWindow();
			dlg.Type = type;
			dlg.ID = item.ID;
			dlg.ShowDialog();
			item.ID = dlg.ID;
			// item.Status = 2;
		}

		private void ChoiceClass(object? parameter)
		{
			Character? ch = parameter as Character;
			if (ch == null) return;

			var dlg = new ChoiceWindow();
			dlg.Type = ChoiceWindow.eType.eClass;
			dlg.ID = ch.Class;
			dlg.ShowDialog();
			ch.Class = dlg.ID;
		}

		private void AppendItem(object? parameter)
		{
			var item = AppendItem(ChoiceWindow.eType.eItem);
			if (item == null) return;

			// TODO: Prevent user from adding items that are already present

			item.Count = 1;
			Items.Add(item);
		}

		private void AppendEquipment(object? parameter)
		{
			var item = AppendItem(ChoiceWindow.eType.eEquipment);
			if (item == null) return;

			Equipments.Add(item);
		}

		private Item? AppendItem(ChoiceWindow.eType type)
		{
			uint index = (uint)(Items.Count + Equipments.Count);
			if (index >= 3800) return null;

			var dlg = new ChoiceWindow();
			dlg.Type = type;
			dlg.ShowDialog();
			if (dlg.ID == 0) return null;

			var item = new Item(0xA0 + index * 20);
			item.ID = dlg.ID;
			item.Index = index + 1;

			// Reset, just in case they aren't already
			item.Count = 0; // Set to 1 in AppendItem
			item.EquipSlotIndex = byte.MaxValue;
			item.UnitEquippedIconId = uint.MaxValue;

			// 2 (0b10) seems to be for unviewed/new items
			item.Status = 2;
			// var info = Info.Search(Info.Kind, item.ID);
			// if (info != null)
			// {
			// 	item.Status = uint.Parse(info.Name);
			// }

			return item;
		}

		private void ExportCharacter(object? parameter)
		{
			if (parameter == null) return;

			int index = Convert.ToInt32(parameter);
			if (index == -1) return;

			var dlg = new SaveFileDialog();
			dlg.Filter = "Unicorn Overlord Character's Dump|*.uocd";
			if (dlg.ShowDialog() == false) return;

			uint address = Util.calcCharacterAddress((uint)index);
			Byte[] buffer = SaveData.Instance.ReadValue(address, 464);

			System.IO.File.WriteAllBytes(dlg.FileName, buffer);
		}

		private void ImportCharacter(object? parameter)
		{
			if (parameter == null) return;

			int index = Convert.ToInt32(parameter);
			if (index == -1) return;

			var dlg = new OpenFileDialog();
			dlg.Filter = "Unicorn Overlord Character's Dump|*.uocd";
			if (dlg.ShowDialog() == false) return;

			Byte[] buffer = System.IO.File.ReadAllBytes(dlg.FileName);
			if (buffer.Length != 464) return;
			buffer = ProcessingCharacter(buffer);

			uint address = Util.calcCharacterAddress((uint)index);

			// use original id
			uint id = SaveData.Instance.ReadNumber(address, 4);
			Array.Copy(BitConverter.GetBytes(id), buffer, 4);

			SaveData.Instance.WriteValue(address, buffer);

			// swap
			var ch = new Character(address);
			ch.Bonds = Characters.ElementAt(index).Bonds;
			CharactersById.Remove(Characters.ElementAt(index).ID);
			Characters.RemoveAt(index);
			Characters.Insert(index, ch);
			CharactersById.Add(ch.ID, ch);
		}

		private void InsertCharacter(object? parameter)
		{
			uint count = (uint)Characters.Count;
			if (count >= 500) return;

			var dlg = new OpenFileDialog();
			dlg.Multiselect = true;
			dlg.Filter = "Unicorn Overlord Character's Dump|*.uocd";
			if (dlg.ShowDialog() == false) return;

			foreach (String filename in dlg.FileNames)
			{
				count = (uint)Characters.Count;
				if (count >= 500) break;

				Byte[] buffer = System.IO.File.ReadAllBytes(filename);
				if (buffer.Length != 464) continue;

				buffer = ProcessingCharacter(buffer);
				uint id = SaveData.Instance.ReadNumber(0x63980, 4) + 1;
				Array.Copy(BitConverter.GetBytes(id), buffer, 4);
				uint address = Util.calcCharacterAddress(count);
				SaveData.Instance.WriteValue(address, buffer);

				SaveData.Instance.WriteNumber(0x63980, 4, id);
				count = SaveData.Instance.ReadNumber(0x63984, 4);
				SaveData.Instance.WriteNumber(0x63984, 4, count + 1);

				InsertFriendship(id);

				var ch = new Character(Util.calcCharacterAddress((uint)Characters.Count));
				if (ch.ID == 0xFFFFFFFF) continue;
				Characters.Add(ch);
				CharactersById.Add(ch.ID, ch);

				SetCharacterBonds(Characters);
			}
		}

		private void ChangeItemCountMax(object? parameter)
		{
			foreach(var item in Items)
			{
				if (item.ID <= 4) continue;
				item.Count = 99;
			}
		}

		private void ChangeCharacterBondMax(object? parameter)
		{
			Character? ch = parameter as Character;
			if (ch == null) return;
			if (ch.Bonds == null) return;

			foreach (var bond in ch.Bonds)
			{
				bond.Value = 900;
			}
		}

		void ChangeCharacterBondMaxAll(object? parameter)
		{
			foreach (var ch in Characters)
			{
				if (ch == null) continue;
				if (ch.Bonds == null) continue;

				foreach (var bond in ch.Bonds)
				{
					bond.Value = 900;
				}
			}
		}

		private Byte[] ProcessingCharacter(Byte[] buffer)
		{
			// formation clear
			Array.Copy(BitConverter.GetBytes(0xFFFFFFFF), 0, buffer, 4, 4);
			buffer[32] = 0xFF;

			// buffer[460]
			// character's status
			// 1Bit => formation join
			// 3Bit => join
			// 4Bit => mercenary?
			// 5Bit => use
			buffer[460] &= 0xFE;

			// equipment clear
			// elements => 4Byte
			// count => 4
			// (or Append Item)
			Array.Clear(buffer, 76, 16);

			// update uint?
			/*
			buffer[456] = 9;
			buffer[458] = 9;
			*/
			return buffer;
		}

		private void InsertFriendship(uint id)
		{
			for (uint index = 0; index < 164; index++)
			{
				uint baseAddress = Util.calcBondAddress(index);
				var current_id = SaveData.Instance.ReadNumber(baseAddress, 4);

				// chack blank character
				if(current_id == 0xFFFFFFFF)
				{
					// insert new character
					SaveData.Instance.WriteNumber(baseAddress, 4, id);
					for (uint count = 0; count < Characters.Count; count++)
					{
						uint address = baseAddress + 4 + count * 8;
						// insert existing character
						SaveData.Instance.WriteNumber(address, 4, Characters[(int)count].ID);
					}
					return;
				}

				// existing character
				for (uint count = 0; count < 164; count++)
				{
					uint address = baseAddress + 4 + count * 8;
					if (SaveData.Instance.ReadNumber(address, 4) == 0xFFFFFFFF)
					{
						// insert new character
						SaveData.Instance.WriteNumber(address, 4, id);
						break;
					}
				}
			}
		}
	}
}
