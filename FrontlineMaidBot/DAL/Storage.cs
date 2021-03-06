﻿using FrontlineMaidBot.Extensions;
using FrontlineMaidBot.Helpers;
using FrontlineMaidBot.Interfaces;
using FrontlineMaidBot.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FrontlineMaidBot.DAL
{
    public class Storage : IStorage
    {
        private const string _dataFolder = "Data";
        private const string _dollsFile = "Dolls.json";
        private const string _equipmentFile = "Equipment.json";
        private const string _aboutFile = "About.json";
        private const string _pokeFile = "Poke.json";
        private const string _slapFile = "Slap.json";
        private const string _feedback = "feedback.log";

        private readonly List<ProductionResult> _dolls;
        private readonly List<ProductionResult> _equipment;

        public Storage()
        {
            _dolls = LoadFromFile<List<ProductionResult>>(Path.Combine(_dataFolder, _dollsFile));
            _equipment = LoadFromFile<List<ProductionResult>>(Path.Combine(_dataFolder, _equipmentFile));
        }

        public IEnumerable<ProductionResult> GetByName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return new List<ProductionResult>();

            var productionResults = _dolls.Union(_equipment);

            //We need first exact run, to be sure that there is no matches on unique Name
            var byUniqueName = productionResults.Where(x => x.Name == name);

            if (byUniqueName.Any())
                return byUniqueName;

            var normal = name.ToLower().Replace(new[] { " ", "-", ".", "|" }, "");

            //the second run is deep - everything slightly similar will be good enough
            return productionResults.Where(x => x.Lookup.Contains(normal));
        }

        public IEnumerable<ProductionResult> GetByTime(string time)
        {
            if (string.IsNullOrEmpty(time))
                return new List<ProductionResult>();

            var normalTime = Utils.NormalizeTime(time);

            return _dolls.Where(x => x.Time == normalTime).Union(_equipment.Where(x => x.Time == normalTime));
        }

        public string GetAbout()
        {
            var help = LoadFromFile<List<string>>(Path.Combine(_dataFolder, _aboutFile));

            return string.Join(Environment.NewLine, help);
        }

        public IEnumerable<string> GetPokeJokes()
        {
            return LoadFromFile<List<string>>(Path.Combine(_dataFolder, _pokeFile));
        }

        public IEnumerable<string> GetSlapJokes()
        {
            return LoadFromFile<List<string>>(Path.Combine(_dataFolder, _slapFile));
        }

        public void SaveFeedback(string username, string chat, string text)
        {
            var content =
                $"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}" +
                $"{DateTime.Now.ToString("[dd.mm.yyyy] hh:MM:ss")}" +
                $"User name: {username}; chat: {chat};"+
                $"{Environment.NewLine} {text}"
                ;

            File.AppendAllText(Path.Combine(_dataFolder, _feedback), content);
        }

        private static T LoadFromFile<T>(string path)
        {
            var content = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(content);
        }

        public IEnumerable<ProductionResult> GetDollByTime(string time)
        {
            if (string.IsNullOrEmpty(time))
                return new List<ProductionResult>();

            var normalTime = Utils.NormalizeTime(time);

            return _dolls.Where(x => x.Time == normalTime);
        }

        public IEnumerable<ProductionResult> GetEquipmentByTime(string time)
        {
            if (string.IsNullOrEmpty(time))
                return new List<ProductionResult>();

            var normalTime = Utils.NormalizeTime(time);

            return _equipment.Where(x => x.Time == normalTime);
        }
    }
}