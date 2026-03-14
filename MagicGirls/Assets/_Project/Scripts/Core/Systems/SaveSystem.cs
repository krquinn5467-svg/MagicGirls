using System;
using System.IO;
using System.Linq;
using UnityEngine;
using MagicGirls.Core.Models;

namespace MagicGirls.Core.Systems
{
    public class SaveSystem
    {
        private const string SaveFileName = "magicgirls_save.json";

        private readonly TimeSystem _time;
        private readonly TeamSystem _team;
        private readonly EconomySystem _economy;
        private readonly InventorySystem _inventory;

        public event Action OnSaveCompleted;
        public event Action OnLoadCompleted;

        public SaveSystem(TimeSystem time, TeamSystem team, EconomySystem economy, InventorySystem inventory)
        {
            _time = time;
            _team = team;
            _economy = economy;
            _inventory = inventory;
        }

        public string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        public bool HasSaveFile => File.Exists(SaveFilePath);

        public SaveData CreateSaveData(int totalWeeksCompleted)
        {
            var data = new SaveData
            {
                SaveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                CurrentSegmentIndex = _time.Calendar.CurrentSegmentIndex,
                CurrentDay = _time.Calendar.CurrentDay,
                CurrentWeek = _time.Calendar.CurrentWeek,
                CurrentMonth = _time.Calendar.CurrentMonth,
                TotalWeeksCompleted = totalWeeksCompleted,
                Gold = _economy.Gold
            };

            foreach (var character in _team.ActiveTeam)
                data.Characters.Add(CharacterSaveData.FromCharacter(character));

            foreach (var item in _inventory.Items)
            {
                data.InventoryItems.Add(new InventoryItemSaveData
                {
                    ItemName = item.Definition.itemName,
                    Quantity = item.Quantity
                });
            }

            return data;
        }

        public bool Save(int totalWeeksCompleted)
        {
            try
            {
                var data = CreateSaveData(totalWeeksCompleted);
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(SaveFilePath, json);
                Debug.Log($"[MagicGirls] Game saved to {SaveFilePath}");
                OnSaveCompleted?.Invoke();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[MagicGirls] Save failed: {e.Message}");
                return false;
            }
        }

        public SaveData Load()
        {
            try
            {
                if (!HasSaveFile)
                {
                    Debug.LogWarning("[MagicGirls] No save file found.");
                    return null;
                }

                string json = File.ReadAllText(SaveFilePath);
                var data = JsonUtility.FromJson<SaveData>(json);
                Debug.Log($"[MagicGirls] Game loaded from {SaveFilePath} (saved {data.SaveDate})");
                OnLoadCompleted?.Invoke();
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"[MagicGirls] Load failed: {e.Message}");
                return null;
            }
        }

        public void DeleteSave()
        {
            if (HasSaveFile)
            {
                File.Delete(SaveFilePath);
                Debug.Log("[MagicGirls] Save file deleted.");
            }
        }
    }
}
