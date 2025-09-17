﻿using ICSharpCode.SharpZipLib.Zip;
using System.Net.Http;

namespace AssetRipper.AssemblyDumper.Downloader;

internal static class Program
{
	static async Task Main()
	{
		const string BinDirectory = "../../";
		const string ProjectDirectory = BinDirectory + "AssetRipper.AssemblyDumper/";
		string[] outputFolders =
		[
			ProjectDirectory + "Debug/",
			ProjectDirectory + "Release/",
		];
		(string, string)[] files =
		[
			("type_tree.tpk", @"https://nightly.link/AssetRipper/Tpk/workflows/type_tree_tpk/master/brotli_file.zip"),
			("engine_assets.tpk", @"https://nightly.link/AssetRipper/Tpk/workflows/engine_assets_tpk/master/brotli_file.zip"),
		];

		foreach ((string fileName, string url) in files)
		{
			Console.WriteLine($"Downloading {fileName}...");
			MemoryStream stream = await Download(url);
			Console.WriteLine($"Decompressing {fileName}...");
			byte[] data = DecompressZipFile(stream);
			foreach (string outputFolder in outputFolders)
			{
				Directory.CreateDirectory(outputFolder);
				string outputPath = outputFolder + fileName;
				Console.WriteLine($"Writing {outputPath}...");
				File.WriteAllBytes(outputPath, data);
			}
		}

		Console.WriteLine("Done!");
	}

	private static async Task<MemoryStream> Download(string url)
	{
		Stream stream;
		using (HttpClient client = new())
		{
			stream = await client.GetStreamAsync(url);
		}
		MemoryStream result = new();
		await stream.CopyToAsync(result);
		result.Position = 0;
		return result;
	}

	private static byte[] DecompressZipFile(Stream inputStream)
	{
		using ZipInputStream zipInputStream = new ZipInputStream(inputStream);
		ZipEntry entry;
		while ((entry = zipInputStream.GetNextEntry()) is not null)
		{
			if (entry.IsFile)
			{
				using MemoryStream outputStream = new();
				zipInputStream.CopyTo(outputStream);
				return outputStream.ToArray();
			}
		}

		throw new Exception("No file found in zip file");
	}
}
