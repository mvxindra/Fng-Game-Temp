using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleController : MonoBehaviour
{
    public static BattleController Instance { get; private set; }

    public BattleInitializer initializer;
    public SkillButtonPanel skillPanel;
    public WaveUI waveUI;

    public EnemyParty currentEnemyParty;
    public bool autoBattleEnabled = false;

    private bool isWaveBattle = false;

    public List<CombatUnit> Allies  => initializer.AllyUnits;
    public List<CombatUnit> Enemies => initializer.EnemyUnits;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetAutoBattle(bool value)
    {
        autoBattleEnabled = value;
    }

    public void StartBattle()
    {
        isWaveBattle = false;

        initializer.InitializeBattle(
            PartyManager.Instance.activeParty,
            currentEnemyParty
        );

        TurnManager.Instance.InitializeTurns(Allies, Enemies);
        StartNextTurn();
    }

    public void StartWaveBattle()
    {
        isWaveBattle = true;

        currentEnemyParty = WaveManager.Instance.NextWave();
        
        if (currentEnemyParty == null)
        {
            Debug.LogError("[BattleController] No waves available!");
            return;
        }

        initializer.InitializeBattle(
            PartyManager.Instance.activeParty,
            currentEnemyParty
        );

        TurnManager.Instance.InitializeTurns(Allies, Enemies);
        
        if (waveUI != null)
        {
            waveUI.UpdateWave(WaveManager.Instance.CurrentWave, WaveManager.Instance.TotalWaves);
        }

        StartNextTurn();
    }

    public void StartNextTurn()
    {
        TurnManager.Instance.RemoveDead();

        if (Allies.TrueForAll(a => a.CurrentHP <= 0))
        {
            OnDefeat();
            return;
        }

        if (Enemies.TrueForAll(e => e.CurrentHP <= 0))
        {
            if (isWaveBattle && WaveManager.Instance.HasMoreWaves())
            {
                StartNextWave();
                return;
            }
            else
            {
                OnVictory();
                skillPanel?.Hide();
                return;
            }
        }

        CombatUnit unit = TurnManager.Instance.GetCurrentUnit();
        if (unit == null)
            return;

        unit.OnTurnStart(new BattleContext());

        if (!unit.isEnemy)
        {
            if (autoBattleEnabled)
            {
                skillPanel?.Hide();
                AllyAIAct(unit);
            }
            else
            {
                skillPanel?.ShowSkills(unit);
            }
        }
        else
        {
            skillPanel?.Hide();
            EnemyAIAct(unit);
        }
    }

    private void StartNextWave()
    {
        Debug.Log("[BattleController] Starting next wave...");
        
        foreach (var ally in Allies)
        {
            int healAmount = Mathf.RoundToInt(ally.BaseHP * 0.2f);
            ally.CurrentHP = Mathf.Min(ally.BaseHP, ally.CurrentHP + healAmount);
        }

        currentEnemyParty = WaveManager.Instance.NextWave();
        
        foreach (var enemy in Enemies)
        {
            if (enemy != null && enemy.gameObject != null)
                Destroy(enemy.gameObject);
        }
        initializer.EnemyUnits.Clear();

        foreach (string enemyId in currentEnemyParty.enemyIds)
        {
            if (string.IsNullOrEmpty(enemyId)) continue;

            var enemyCfg = EnemyDatabase.Instance.GetEnemy(enemyId);
            if (enemyCfg == null)
            {
                Debug.LogError($"Enemy not found: {enemyId}");
                continue;
            }

            var go = Object.Instantiate(initializer.combatUnitPrefab, initializer.enemySpawnRoot);
            var cu = go.GetComponent<CombatUnit>();
            cu.isEnemy = true;
            cu.InitializeFromHeroConfig(enemyCfg);
            initializer.EnemyUnits.Add(cu);
        }

        TurnManager.Instance.InitializeTurns(Allies, Enemies);

        if (waveUI != null)
        {
            waveUI.UpdateWave(WaveManager.Instance.CurrentWave, WaveManager.Instance.TotalWaves);
        }

        StartNextTurn();
    }

    public void UseSkill(CombatUnit caster, SkillInstance skill)
    {
        skillPanel?.Hide();

        var allies  = Allies;
        var enemies = Enemies;

        var targets = TargetSelector.GetTargets(
            skill.Config.targetType,
            caster,
            allies,
            enemies
        );

        var ctx = new BattleContext();
        skill.Activate(caster, targets, ctx);

        EndTurn();
    }

    private void EnemyAIAct(CombatUnit enemy)
    {
        SkillInstance chosen = null;

        foreach (var s in enemy.Skills)
        {
            if (s.IsReady)
            {
                chosen = s;
                break;
            }
        }

        if (chosen == null)
        {
            EndTurn();
            return;
        }

        UseSkill(enemy, chosen);
    }

    private void AllyAIAct(CombatUnit ally)
    {
        SkillInstance chosen = null;

        foreach (var s in ally.Skills)
        {
            if (s.IsReady &&
                s.Config.effects.Exists(eff =>
                    eff.type == "Damage" ||
                    eff.type == "Debuff"))
            {
                chosen = s;
                break;
            }
        }

        if (chosen == null)
        {
            foreach (var s in ally.Skills)
            {
                if (s.IsReady)
                {
                    chosen = s;
                    break;
                }
            }
        }

        if (chosen == null)
        {
            EndTurn();
            return;
        }   

        UseSkill(ally, chosen);
    }

    private void EndTurn()
    {
        var curr = TurnManager.Instance.GetCurrentUnit();
        if (curr != null)
            curr.OnTurnEnd();

        TurnManager.Instance.NextTurn();
        StartNextTurn();
    }
    
    public void OnVictory()
    {
        Debug.Log("[BattleController] VICTORY!");

        if (PlayerPrefs.HasKey("StoryActIndex"))
        {
            int actIdx = PlayerPrefs.GetInt("StoryActIndex");
            StoryModeManager.ReportProgress(30);
        
            PlayerPrefs.DeleteKey("StoryActIndex");
            PlayerPrefs.DeleteKey("StoryChapterGUID");
            PlayerPrefs.DeleteKey("StoryDifficulty");
        
            SceneManager.LoadScene("FNG_Main");
            return;
        }
    
        if (DungeonManager.Instance?.currentFloor != null)
        {
            var rewards = DungeonRewardRoller.RollFloorRewards(DungeonManager.Instance.currentFloor);
            RewardDistributor.Instance?.DistributeRewards(rewards, "DUNGEON");
            Debug.Log($"[BattleController] Victory! Earned {rewards.Count} rewards");
            
            Invoke(nameof(ReturnToMain), 2f);
            return;
        }

        Debug.Log("[BattleController] Test battle victory");
        Invoke(nameof(ReturnToMain), 2f);
    }

    public void OnDefeat()
    {
        Debug.Log("[BattleController] DEFEAT!");

        if (PlayerPrefs.HasKey("StoryActIndex"))
        {
            PlayerPrefs.DeleteKey("StoryActIndex");
            PlayerPrefs.DeleteKey("StoryChapterGUID");
            PlayerPrefs.DeleteKey("StoryDifficulty");
            
            SceneManager.LoadScene("FNG_Main");
            return;
        }
    
        Invoke(nameof(ReturnToMain), 2f);
    }

    private void ReturnToMain()
    {
        SceneManager.LoadScene("FNG_Main");
    }
}