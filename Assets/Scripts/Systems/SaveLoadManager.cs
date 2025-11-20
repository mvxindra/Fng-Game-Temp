using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using FnGMafia.Core;

namespace FnGMafia.Systems
{
    /// <summary>
    /// Comprehensive save/load system for all player data
    /// </summary>
    public class SaveLoadManager : Singleton<SaveLoadManager>
    {
        private const string SAVE_FILE_NAME = "PlayerSave.json";
        private const int SAVE_VERSION = 1;

        private string SaveFilePath => Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);

        // Auto-save settings
        [SerializeField] private bool enableAutoSave = true;
        [SerializeField] private float autoSaveInterval = 300f; // 5 minutes
        private float autoSaveTimer = 0f;

        protected override void Awake()
        {
            base.Awake();
            Debug.Log($"[SaveLoad] Save file path: {SaveFilePath}");
        }

        private void Update()
        {
            if (enableAutoSave)
            {
                autoSaveTimer += Time.deltaTime;
                if (autoSaveTimer >= autoSaveInterval)
                {
                    autoSaveTimer = 0f;
                    SaveGame();
                }
            }
        }

        /// <summary>
        /// Save all game data
        /// </summary>
        public bool SaveGame()
        {
            try
            {
                PlayerSaveData saveData = CollectSaveData();
                string json = JsonUtility.ToJson(saveData, true);
                File.WriteAllText(SaveFilePath, json);
                Debug.Log($"[SaveLoad] Game saved successfully to {SaveFilePath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveLoad] Failed to save game: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Load all game data
        /// </summary>
        public bool LoadGame()
        {
            try
            {
                if (!File.Exists(SaveFilePath))
                {
                    Debug.Log("[SaveLoad] No save file found. Starting new game.");
                    return false;
                }

                string json = File.ReadAllText(SaveFilePath);
                PlayerSaveData saveData = JsonUtility.FromJson<PlayerSaveData>(json);

                if (saveData == null)
                {
                    Debug.LogError("[SaveLoad] Failed to parse save data");
                    return false;
                }

                ApplySaveData(saveData);
                Debug.Log($"[SaveLoad] Game loaded successfully (Version: {saveData.saveVersion})");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveLoad] Failed to load game: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Delete save file
        /// </summary>
        public bool DeleteSave()
        {
            try
            {
                if (File.Exists(SaveFilePath))
                {
                    File.Delete(SaveFilePath);
                    Debug.Log("[SaveLoad] Save file deleted");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveLoad] Failed to delete save: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Check if save file exists
        /// </summary>
        public bool SaveExists()
        {
            return File.Exists(SaveFilePath);
        }

        /// <summary>
        /// Collect all data that needs to be saved
        /// </summary>
        private PlayerSaveData CollectSaveData()
        {
            PlayerSaveData data = new PlayerSaveData
            {
                saveVersion = SAVE_VERSION,
                saveTimestamp = DateTime.Now.ToString("o")
            };

            // Currency data
            if (CurrencyManager.Instance != null)
            {
                data.currencies = new Dictionary<string, int>(CurrencyManager.Instance.GetAllCurrencies());
            }

            // Wallet data
            if (PlayerWallet.Instance != null)
            {
                data.walletData = new WalletSaveData
                {
                    gold = PlayerWallet.Instance.gold,
                    gems = PlayerWallet.Instance.gems,
                    currentStamina = PlayerWallet.Instance.currentStamina,
                    maxStamina = PlayerWallet.Instance.maxStamina
                };
            }

            // Friend system data
            if (FriendManager.Instance != null)
            {
                data.friendData = FriendManager.Instance.GetSaveData();
            }

            // Research tree data
            if (ResearchTreeManager.Instance != null)
            {
                data.researchData = ResearchTreeManager.Instance.GetSaveData();
            }

            // Artifact data
            if (ArtifactManager.Instance != null)
            {
                data.artifactData = ArtifactManager.Instance.GetSaveData();
            }

            // Player profile data
            if (PlayerProfileManager.Instance != null)
            {
                data.profileData = PlayerProfileManager.Instance.GetSaveData();
            }

            // Chat cosmetics
            if (ChatCosmeticManager.Instance != null)
            {
                data.chatCosmeticData = ChatCosmeticManager.Instance.GetSaveData();
            }

            // Replay data
            if (ReplayManager.Instance != null)
            {
                data.replayData = ReplayManager.Instance.GetSaveData();
            }

            return data;
        }

        /// <summary>
        /// Apply loaded save data to all systems
        /// </summary>
        private void ApplySaveData(PlayerSaveData data)
        {
            // Apply currency data
            if (data.currencies != null && CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.LoadCurrencies(data.currencies);
            }

            // Apply wallet data
            if (data.walletData != null && PlayerWallet.Instance != null)
            {
                PlayerWallet.Instance.gold = data.walletData.gold;
                PlayerWallet.Instance.gems = data.walletData.gems;
                PlayerWallet.Instance.currentStamina = data.walletData.currentStamina;
                PlayerWallet.Instance.maxStamina = data.walletData.maxStamina;
            }

            // Apply friend data
            if (data.friendData != null && FriendManager.Instance != null)
            {
                FriendManager.Instance.LoadSaveData(data.friendData);
            }

            // Apply research data
            if (data.researchData != null && ResearchTreeManager.Instance != null)
            {
                ResearchTreeManager.Instance.LoadSaveData(data.researchData);
            }

            // Apply artifact data
            if (data.artifactData != null && ArtifactManager.Instance != null)
            {
                ArtifactManager.Instance.LoadSaveData(data.artifactData);
            }

            // Apply profile data
            if (data.profileData != null && PlayerProfileManager.Instance != null)
            {
                PlayerProfileManager.Instance.LoadSaveData(data.profileData);
            }

            // Apply chat cosmetic data
            if (data.chatCosmeticData != null && ChatCosmeticManager.Instance != null)
            {
                ChatCosmeticManager.Instance.LoadSaveData(data.chatCosmeticData);
            }

            // Apply replay data
            if (data.replayData != null && ReplayManager.Instance != null)
            {
                ReplayManager.Instance.LoadSaveData(data.replayData);
            }
        }

        private void OnApplicationQuit()
        {
            if (enableAutoSave)
            {
                SaveGame();
            }
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause && enableAutoSave)
            {
                SaveGame();
            }
        }
    }

    /// <summary>
    /// Master save data container
    /// </summary>
    [Serializable]
    public class PlayerSaveData
    {
        public int saveVersion;
        public string saveTimestamp;

        // Currency system
        public Dictionary<string, int> currencies;

        // Wallet
        public WalletSaveData walletData;

        // New systems
        public FriendSaveData friendData;
        public ResearchSaveData researchData;
        public ArtifactSaveData artifactData;
        public ProfileSaveData profileData;
        public ChatCosmeticSaveData chatCosmeticData;
        public ReplaySaveData replayData;
    }

    [Serializable]
    public class WalletSaveData
    {
        public int gold;
        public int gems;
        public int currentStamina;
        public int maxStamina;
    }
}
