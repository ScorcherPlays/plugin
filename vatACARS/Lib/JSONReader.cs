﻿using Newtonsoft.Json;
using System;
using System.IO;
using vatACARS.Util;

namespace vatACARS.Lib
{
    public static class JSONReader
    {
        public static dynamic quickFillItems;
        private static string dirPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\vatACARS";
        private static Logger logger = new Logger("JSONReader");

        public static void MakeQuickFillItems()
        {
            try
            {
                logger.Log("Reading quickfill items...");

                logger.Log("Deserializing...");
                string jsonString = File.ReadAllText($"{dirPath}\\data\\quickfill.json");
                quickFillItems = JsonConvert.DeserializeObject(jsonString);
                logger.Log("Done!");
            }
            catch (Exception ex)
            {
                logger.Log($"Something went wrong!\n{ex.ToString()}");
            }
        }
    }
}