﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShrinelandsTactics.World;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ShrinelandsTactics.Mechanics;
using Action = ShrinelandsTactics.Mechanics.Action;

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

    //TODO: fix hack
    public readonly Dictionary<Tuple<byte, byte, byte>, char> ColorToIcon = new Dictionary<Tuple<byte, byte, byte>, char>()
    {
        {new Tuple<byte, byte, byte>(0,0,0 ), '#' },
        {new Tuple<byte, byte, byte>(255,255,255 ), '.' },
    };

    public char GetIconByColor(byte r, byte g, byte b)
    {
        return ColorToIcon[new Tuple<byte, byte, byte>(r, g, b)];
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

    public Character LoadCharacterByClass(string characterClass)
    {
        var charTemplate = Characters.FirstOrDefault(c => c.Name == characterClass);
        if (charTemplate == null)
        {
            throw new ArgumentException("No such character as " + characterClass);
        }

        var newChar = charTemplate.Clone() as Character;

        FleshOutActions(newChar);

        return newChar;
    }

    private void FleshOutActions(Character newChar)
    {
        for (int i = 0; i < newChar.Actions.Count; i++)
        {
            Action action = GetActionByName(newChar.Actions[i].Name);
            newChar.Actions[i] = action;
        }
    }

    private Action GetActionByName(string name)
    {
        var action = Actions.FirstOrDefault(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        return action;
    }
}