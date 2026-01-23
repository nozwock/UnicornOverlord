using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnicornOverlord
{
	internal class Info
	{
		public static ImmutableSortedDictionary<uint, NameValueInfo> Item { get; private set; }
		public static ImmutableSortedDictionary<uint, NameValueInfo> Class { get; private set; }
		public static ImmutableSortedDictionary<uint, NameValueInfo> Name { get; private set; }
		public static ImmutableSortedDictionary<uint, uint> EquipmentKind { get; private set; }

		private Info() { }

		static Info()
		{
			var infoDir = Path.Combine(AppContext.BaseDirectory, "info");

			Item = ReadSortedNameValueInfo(Path.Combine(infoDir, "item.txt"))
				?? ImmutableSortedDictionary<uint, NameValueInfo>.Empty;
			Class = ReadSortedNameValueInfo(Path.Combine(infoDir, "class.txt"))
				?? ImmutableSortedDictionary<uint, NameValueInfo>.Empty;
			Name = ReadSortedNameValueInfo(Path.Combine(infoDir, "name.txt"))
				?? ImmutableSortedDictionary<uint, NameValueInfo>.Empty;
			EquipmentKind = ReadSortedKV(Path.Combine(infoDir, "kind.txt"), ParseUInt32, v =>  ParseUInt32(v[1]))
				?? ImmutableSortedDictionary<uint, uint>.Empty;
		}

		static (bool, uint) ParseUInt32(string number)
		{
			try
			{
				if (number.Length > 1 && number[..2] == "0x") return (true, Convert.ToUInt32(number, 16));
				else return (true, Convert.ToUInt32(number));
			}
			catch (Exception ex) when (ex is FormatException || ex is OverflowException)
			{
				return (false, 0);
			}
		}

		static ImmutableSortedDictionary<uint, NameValueInfo>? ReadSortedNameValueInfo(string filepath)
		{
			return ReadSortedKV(filepath, ParseUInt32, v =>
			{
				var info = new NameValueInfo()!;
				if (info.Line(v))
					return (true, info);
				return (false, info); // Filter out
			});
		}

		static ImmutableSortedDictionary<K, V>? ReadSortedKV<K, V>(
			string filepath,
			Func<string, (bool, K)> parseK,
			Func<string[], (bool, V)> parseV)
			where K : notnull, IComparable<K>
			where V : notnull
		{
			if (!File.Exists(filepath)) return null;

			var kvPairs = new List<(K, V)>();
			ReadTsv(File.ReadLines(filepath), values =>
			{
				var (ok1, k) = parseK(values[0]);
				var (ok2, v) = parseV(values);
				if (ok1 && ok2)
					kvPairs.Add((k, v));
			});

			return kvPairs.OrderBy(it => it.Item1).ToImmutableSortedDictionary(it => it.Item1, it => it.Item2);
		}

		/// <summary>
		/// Tab separated values, rows with less than two values are skipped.
		/// </summary>
		static void ReadTsv(IEnumerable<string> lines, Action<string[]> consumeValues)
		{
			foreach (var line in lines)
			{
				var trimmed_line = line.Trim();
				if (trimmed_line.Length < 3 || trimmed_line[0] == '#') continue;

				var values = line.Split('\t');
				if (values.Length < 2) continue;
				if (String.IsNullOrEmpty(values[0])) continue;
				if (String.IsNullOrEmpty(values[1])) continue;

				consumeValues(values);
			}
		}
	}
}
