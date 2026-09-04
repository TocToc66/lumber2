using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Lumber.Loot;

namespace Lumber.Core
{
    [Serializable]
    public class SaveData
    {
        public int money = 25;
        public int level = 1;
        public int xp = 0;

        public List<AxeInstance> axes = new List<AxeInstance>();
        public string equippedAxeId = "";
        public int boxCount = 0;

        public int lvlEndurance = 0;
        public int lvlForce = 0;
        public int lvlChance = 0;
        public int lvlScierie = 0;
        public int lvlPepiniere = 0;
        public int lvlEntrepot = 0;
    }

    public static class SaveManager
    {
        private static string FilePath => Application.persistentDataPath + "/lumber_save.json";

        public static SaveData Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    string json = File.ReadAllText(FilePath);
                    SaveData data = JsonUtility.FromJson<SaveData>(json);
                    if (data != null)
                        return data;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[SaveManager] Failed to load save: " + e.Message);
            }

            return new SaveData();
        }

        public static void Save(SaveData data)
        {
            try
            {
                File.WriteAllText(FilePath, JsonUtility.ToJson(data, true));
            }
            catch (Exception e)
            {
                Debug.LogWarning("[SaveManager] Failed to save: " + e.Message);
            }
        }
    }
}
