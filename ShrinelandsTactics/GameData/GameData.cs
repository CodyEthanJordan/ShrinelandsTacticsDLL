using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShrinelandsTactics.World;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;



public class GameData 
{
    public Dictionary<string, Tile> Tiles = new Dictionary<string, Tile>();
    public List<Character> Characters = new List<Character>();
    public List<Action> Actions = new List<Action>();

    public Character GetCharacterByName(string name)
    {
        var charTemplate = Characters.FirstOrDefault(c => c.Name == name);
        if(charTemplate == null)
        {
            throw new ArgumentException("No such character as " + name);
        }

        return charTemplate.Clone() as Character;
    }

    public static GameData CreateFromJson(string tileJson, string characterJson,
        string actionJson)
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

        j = JObject.Parse(actionJson);
        foreach (var actionEntry in j["actionData"])
        {
            data.Actions.Add(actionEntry.ToObject<Action>());
        }

        return data;
    }

    public static GameData ReadDatafilesInDirectory(string path)
    {
        var data = new GameData();
        string tileJson = null;
        string charJson = null;
        string actionJson = null;

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
                case "characterData":
                    using (StreamReader r = new StreamReader(filePath))
                    {
                        charJson = r.ReadToEnd();
                    }
                    break;
                case "actionData":
                    using (StreamReader r = new StreamReader(filePath))
                    {
                        actionJson = r.ReadToEnd();
                    }
                    break;
                default:
                    break;
            }
        }

        data = GameData.CreateFromJson(tileJson, charJson, actionJson);

        return data;
    }
}