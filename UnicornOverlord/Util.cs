using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UnicornOverlord
{
	internal class Util
	{
		public static Byte[] Resize(Byte[] bytes, uint length)
		{
			Byte[] buffer = new Byte[length];
			Array.Copy(bytes, buffer, length);
			return buffer;
		}

		public static void WriteNumber(uint address, uint size, uint value, uint min, uint max)
		{
			if (value < min) value = min;
			if (value > max) value = max;
			SaveData.Instance.WriteNumber(address, size, value);
		}

		public static uint calcCharacterAddress(uint index)
		{
			return 0x2AF40 + index * 464;
		}

		public static uint calcBondAddress(uint index)
		{
			return 0x1B5830 + index * 1316;
		}

		public static string CalcMD5(string filepath)
		{
			using var md5 = MD5.Create();
			using var stream = File.OpenRead(filepath);
			return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
		}

		public static T? BinarySearch<T>(IList<T> list, T key)
			where T : IComparable<T>
		{
			return BinarySearch(list, key, it => it);
		}

		public static T? BinarySearch<T, K>(IList<T> list, K key, Func<T, K> keyMap)
			where K : IComparable<K>
		{
			int min = 0;
			int max = list.Count;
			for (; min < max;)
			{
				int mid = min + (max - min) / 2;
				var cmp = keyMap(list[mid]).CompareTo(key);
				if (cmp == 0) return list[mid];
				else if (cmp > 0) max = mid;
				else min = mid + 1;
			}
			return default;
		}
	}
}
