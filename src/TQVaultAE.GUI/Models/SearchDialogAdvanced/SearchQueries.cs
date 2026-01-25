using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TQVaultAE.Domain.Contracts.Services;

namespace TQVaultAE.GUI.Models.SearchDialogAdvanced;

public class SearchQueries : List<SearchQuery>
{
	#region Logic

	static SearchQueries _Default;
	static IGamePathService GamePathService;
	static IFileIO FileIO;
	static IPathIO PathIO;

	public static SearchQueries Default(IGamePathService gamePathService, IFileIO fileIO, IPathIO pathIO)
	{
		GamePathService = gamePathService;
		FileIO = fileIO;
		PathIO = pathIO;

		if (_Default is null) _Default = Read();
		return _Default;
	}

	public void Save()
	{
		string xmlPath = ResolveSearchQueriesFilePath();
		var json = JsonConvert.SerializeObject(this, Formatting.Indented);
		FileIO.WriteAllText(xmlPath, json);
	}

	private static string ResolveSearchQueriesFilePath()
		=> PathIO.Combine(GamePathService.TQVaultConfigFolder, "SearchQueries.json");

	public static SearchQueries Read()
	{
		string jsonPath = ResolveSearchQueriesFilePath();

		if (FileIO.Exists(jsonPath))
			return ParseSettings(FileIO.ReadAllText(jsonPath));

		return new SearchQueries();// Default
	}

	public static SearchQueries ParseSettings(string xmlData)
	{
		return JsonConvert.DeserializeObject<SearchQueries>(xmlData);
	}

	#endregion
}


