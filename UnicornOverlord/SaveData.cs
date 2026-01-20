using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnicornOverlord
{
	internal class SaveData : ObservableObject
	{
		private static readonly SaveData mThis = new();
		private Byte[]? mBuffer = null;
		private readonly System.Text.Encoding mEncode = System.Text.Encoding.UTF8;
		public uint Adventure { private get; set; } = 0;

		private readonly string backupDir = Path.Combine(Directory.GetCurrentDirectory(), "backup");
		private bool backupDoneOncePerOpen = false;
		private readonly Dictionary<string, string> backupHashes = [];

		public static SaveData Instance => mThis;
		public string FilePath
		{
			get;
			private set => SetProperty(ref field, value);
		} = String.Empty;
		public bool IsDirty
		{
			get;
			private set => SetProperty(ref field, value);
		} = false;

		private SaveData()
		{
			Directory.CreateDirectory(backupDir);
			foreach (var filepath in Directory.GetFiles(backupDir, "*.DAT"))
			{
				backupHashes.Add(Path.GetFullPath(filepath), Util.CalcMD5(filepath));
			}
		}

		public bool Open(String filepath)
		{
			if (System.IO.File.Exists(filepath) == false) return false;

			var buffer = System.IO.File.ReadAllBytes(filepath);
			String header = mEncode.GetString(buffer, 4, 4);
			if(header != "UCSD") return false;

			mBuffer = buffer;
			FilePath = filepath;

			IsDirty = false;
			backupDoneOncePerOpen = false;

			return true;
		}

		public bool Save(string? filepath = null)
		{
			if (String.IsNullOrEmpty(FilePath) || mBuffer == null) return false;

			BackupOpenedSave(); // FilePath

			if (filepath != null)
				FilePath = filepath;
			System.IO.File.WriteAllBytes(FilePath, mBuffer);

			IsDirty = false;
			return true;
		}

		public bool SaveAs(String filepath)
		{
			return Save(filepath);
		}

		public void Import(String filename)
		{
			if (String.IsNullOrEmpty(FilePath)) return;

			mBuffer = System.IO.File.ReadAllBytes(filename);
		}

		public void Export(String filename)
		{
			if (mBuffer == null) return;
			System.IO.File.WriteAllBytes(filename, mBuffer);
		}

		public uint ReadNumber(uint address, uint size)
		{
			if (mBuffer == null) return 0;
			address = CalcAddress(address);
			if (address + size >= mBuffer.Length) return 0;
			uint result = 0;
			for (int i = 0; i < size; i++)
			{
				result += (uint)mBuffer[address + i] << (i * 8);
			}
			return result;
		}

		public Byte[] ReadValue(uint address, uint size)
		{
			Byte[] result = new Byte[size];
			if (mBuffer == null) return result;
			address = CalcAddress(address);
			if (address + size >= mBuffer.Length) return result;
			Array.Copy(mBuffer, address, result, 0, size);
			return result;
		}

		// 0 to 7.
		public bool ReadBit(uint address, uint bit)
		{
			if (bit > 7) return false;
			if (mBuffer == null) return false;
			address = CalcAddress(address);
			if (address >= mBuffer.Length) return false;
			Byte mask = (Byte)(1 << (int)bit);
			return (mBuffer[address] & mask) != 0;
		}

		public String ReadText(uint address, uint size)
		{
			if (mBuffer == null) return "";
			address = CalcAddress(address);
			if (address + size >= mBuffer.Length) return "";

			Byte[] tmp = new Byte[size];
			for (uint i = 0; i < size; i++)
			{
				if (mBuffer[address + i] == 0) break;
				tmp[i] = mBuffer[address + i];
			}
			return mEncode.GetString(tmp).Trim('\0');
		}

		public void WriteNumber(uint address, uint size, uint value)
		{
			if (mBuffer == null) return;
			address = CalcAddress(address);
			if (address + size >= mBuffer.Length) return;
			for (uint i = 0; i < size; i++)
			{
				mBuffer[address + i] = (Byte)(value & 0xFF);
				value >>= 8;
			}
			IsDirty = true;
		}

		// 0 to 7.
		public void WriteBit(uint address, uint bit, bool value)
		{
			if (bit > 7) return;
			if (mBuffer == null) return;
			address = CalcAddress(address);
			if (address >= mBuffer.Length) return;
			Byte mask = (Byte)(1 << (int)bit);
			if (value) mBuffer[address] = (Byte)(mBuffer[address] | mask);
			else mBuffer[address] = (Byte)(mBuffer[address] & ~mask);
			IsDirty = true;
		}

		public void WriteText(uint address, uint size, String value)
		{
			if (mBuffer == null) return;
			address = CalcAddress(address);
			if (address + size >= mBuffer.Length) return;
			Byte[] tmp = mEncode.GetBytes(value);
			Array.Resize(ref tmp, (int)size);
			Array.Copy(tmp, 0, mBuffer, address, size);
			IsDirty = true;
		}

		public void WriteValue(uint address, Byte[] buffer)
		{
			if (mBuffer == null) return;
			address = CalcAddress(address);
			if (address + buffer.Length >= mBuffer.Length) return;
			Array.Copy(buffer, 0, mBuffer, address, buffer.Length);
			IsDirty = true;
		}

		public void Fill(uint address, uint size, Byte number)
		{
			if (mBuffer == null) return;
			address = CalcAddress(address);
			if (address + size >= mBuffer.Length) return;
			for (uint i = 0; i < size; i++)
			{
				mBuffer[address + i] = number;
			}
			IsDirty = true;
		}

		public void Copy(uint from, uint to, uint size)
		{
			if (mBuffer == null) return;
			from = CalcAddress(from);
			to = CalcAddress(to);
			if (from + size >= mBuffer.Length) return;
			if (to + size >= mBuffer.Length) return;
			for (uint i = 0; i < size; i++)
			{
				mBuffer[to + i] = mBuffer[from + i];
			}
			IsDirty = true;
		}

		public void Swap(uint from, uint to, uint size)
		{
			if (mBuffer == null) return;
			from = CalcAddress(from);
			to = CalcAddress(to);
			if (from + size >= mBuffer.Length) return;
			if (to + size >= mBuffer.Length) return;
			for (uint i = 0; i < size; i++)
			{
				Byte tmp = mBuffer[to + i];
				mBuffer[to + i] = mBuffer[from + i];
				mBuffer[from + i] = tmp;
			}
			IsDirty = true;
		}

		public List<uint> FindAddress(String name, uint index)
		{
			List<uint> result = new List<uint>();
			if (mBuffer == null) return result;
			for (; index < mBuffer.Length; index++)
			{
				if (mBuffer[index] != name[0]) continue;

				int len = 1;
				for (; len < name.Length; len++)
				{
					if (mBuffer[index + len] != name[len]) break;
				}
				if (len >= name.Length) result.Add(index);
				index += (uint)len;
			}
			return result;
		}

		private uint CalcAddress(uint address)
		{
			return address + Adventure;
		}

		/// <summary>
		/// Backup is done on Save, and only once per Open.
		/// </summary>
		private void BackupOpenedSave()
		{
			if (backupDoneOncePerOpen)
				return;

			var now = DateTime.Now;

			foreach (var (k, _) in backupHashes.Where(kv => !Path.Exists(kv.Key)))
				// Doesn't invalidate the enumerator, ToList not required
				backupHashes.Remove(k);

			var hash = Util.CalcMD5(FilePath);
			if (backupHashes.Values.Any(v => v == hash))
			{
					backupDoneOncePerOpen = true;
					return; // Already backed up
			}

			Directory.CreateDirectory(backupDir);
			var filename = $"{now:yyyy-MM-dd HH-mm-ss} {Path.GetFileName(FilePath)}";
			var path = Path.Combine(backupDir, filename);
			File.Copy(FilePath, path, true);

			backupDoneOncePerOpen = true;
			backupHashes.Add(Path.GetFullPath(path), hash);
		}
	}
}
