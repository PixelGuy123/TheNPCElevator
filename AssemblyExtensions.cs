using System.IO;
using System.Reflection;
using UnityEngine;

namespace TheNPCElevator
{
	internal static class AssemblyExtensions
	{
		public static Texture2D LoadTextureFromResources(string name)
		{
			using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(defaultPath + name);
			using MemoryStream memoryStream = new();

			stream.CopyTo(memoryStream);
			byte[] bytes = memoryStream.ToArray();

			Texture2D texture2D = new(2, 2, TextureFormat.RGBA32, mipChain: false);
			texture2D.LoadImage(bytes);
			texture2D.filterMode = FilterMode.Point;
			texture2D.name = Path.GetFileNameWithoutExtension("TheNPCElevator_" + name);
			return texture2D;
		}

		const string defaultPath = "TheNPCElevator.ModResources.";
	}
}
