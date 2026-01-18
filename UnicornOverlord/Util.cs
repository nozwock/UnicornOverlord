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
	}
}
