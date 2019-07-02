using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShrinelandsTactics.World;
using System;
using System.Collections.Generic;
using System.IO;



public class GameData 
{
    public Dictionary<string, Tile> Tiles = new Dictionary<string, Tile>();
    public List<Character> Characters = new List<Character>();


    public GameData()
    {

    }

    public static GameData CreateFromJson(string tileJson, string characterJson)
    {
        var data = new GameData();

        var j = JObject.Parse(tileJson);
        foreach (var tileDataEntry in j["tileData"])
        {
            Tile t = tileDataEntry.ToObject<Tile>();
            data.Tiles.Add(t.Name, t);
        }

        j = JObject.Parse(characterJson);
        foreach (var charEntry in j["characterData"])
        {
            data.Characters.Add(charEntry.ToObject<Character>());
        }

        return data;
    }

    public static GameData ReadDatafilesInDirectory(string path)
    {
        var data = new GameData();
        string tileJson = null;
        string charJson = null;

        foreach (var filePath in Directory.GetFiles(path, "*.json"))
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);

            switch (fileName)
            {
                case "tileData":
                    using (StreamReader r = new StreamReader(filePath))
                    {
                        tileJson = r.ReadToEnd();
                    }
                    break;
                case "characterData":using (StreamReader r = new StreamReader(filePath))
                    {
                        charJson = r.ReadToEnd();
                    }
                    break;

                default:
                    break;
            }
        }

        data = GameData.CreateFromJson(tileJson, charJson);

        return data;
    }
}