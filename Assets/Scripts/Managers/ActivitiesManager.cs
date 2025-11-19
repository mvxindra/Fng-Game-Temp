using System;

using System.Collections.Generic;

using System.Linq;

using UnityEngine;

using FnGMafia.Core;

 

/// <summary>

/// Manager for daily/weekly/monthly activities and events

/// </summary>

public class ActivitiesManager : Singleton<ActivitiesManager>

{

    private ActivitiesDatabase activitiesDatabase;

    private ActivitySchedule currentSchedule;

    private LoginRewardSystem loginRewards;

    private SeasonPass currentSeasonPass;

    private DateTime lastDailyReset;

    private DateTime lastWeeklyReset;

    private DateTime lastMonthlyReset;

 

    protected override void Awake()

    {

        base.Awake();

        LoadActivitiesDatabase();

        InitializeSchedule();

    }

 

    private void LoadActivitiesDatabase()

    {

        TextAsset activitiesData = Resources.Load<TextAsset>("Config/activities_system");

        if (activitiesData != null)

        {

            activitiesDatabase = JsonUtility.FromJson<ActivitiesDatabase>(activitiesData.text);

            Debug.Log($"Loaded {activitiesDatabase.dailyContractPool.Count} daily contracts");

        }

        else

        {

            Debug.LogError("Failed to load activities_system.json");

            activitiesDatabase = new ActivitiesDatabase();

        }

    }

 

    private void InitializeSchedule()

    {

        currentSchedule = new ActivitySchedule();

        loginRewards = activitiesDatabase.loginRewards;

        currentSeasonPass = activitiesDatabase.currentSeasonPass;

 

        lastDailyReset = DateTime.Now.Date;

        lastWeeklyReset = DateTime.Now.Date;

        lastMonthlyReset = DateTime.Now.Date;

 

        RefreshDailyContracts();

    }

 

    private void Update()

    {

        CheckDailyReset();

        CheckWeeklyReset();

        CheckMonthlyReset();

    }

 

    /// <summary>

    /// Check and perform daily reset

    /// </summary>

    private void CheckDailyReset()

    {

        if (DateTime.Now.Date > lastDailyReset)

        {

            PerformDailyReset();

        }

    }

 

    /// <summary>

    /// Check and perform weekly reset

    /// </summary>

    private void CheckWeeklyReset()

    {

        // Reset on Monday

        if (DateTime.Now.DayOfWeek == DayOfWeek.Monday && DateTime.Now.Date > lastWeeklyReset)

        {

            PerformWeeklyReset();

        }

    }

 

    /// <summary>

    /// Check and perform monthly reset

    /// </summary>

    private void CheckMonthlyReset()

    {

        // Reset on first day of month

        if (DateTime.Now.Day == 1 && DateTime.Now.Date > lastMonthlyReset)

        {

            PerformMonthlyReset();

        }

    }

 

    /// <summary>

    /// Perform daily reset

    /// </summary>

    private void PerformDailyReset()

    {

        RefreshDailyContracts();

        lastDailyReset = DateTime.Now.Date;

        Debug.Log("Daily activities reset");

    }

 

    /// <summary>

    /// Perform weekly reset

    /// </summary>

    private void PerformWeeklyReset()

    {

        RefreshWeeklyContracts();

        RefreshBounties();

        lastWeeklyReset = DateTime.Now.Date;

        Debug.Log("Weekly activities reset");

    }

 

    /// <summary>

    /// Perform monthly reset

    /// </summary>

    private void PerformMonthlyReset()

    {

        RefreshMonthlyContracts();

        lastMonthlyReset = DateTime.Now.Date;

        Debug.Log("Monthly activities reset");

    }

 

    /// <summary>

    /// Refresh daily contracts

    /// </summary>

    public void RefreshDailyContracts()

    {

        currentSchedule.dailyContracts.Clear();

 

        // Select 5 random daily contracts

        var available = activitiesDatabase.dailyContractPool

            .OrderBy(x => UnityEngine.Random.value)

            .Take(5)

            .ToList();

 

        foreach (var contract in available)

        {

            var newContract = JsonUtility.FromJson<DailyContract>(JsonUtility.ToJson(contract));

            newContract.isComplete = false;

            newContract.isClaimed = false;

            newContract.expiryTime = DateTime.Now.AddDays(1);

            currentSchedule.dailyContracts.Add(newContract);

        }

 

        Debug.Log($"Refreshed {currentSchedule.dailyContracts.Count} daily contracts");

    }

 

    /// <summary>

    /// Refresh weekly contracts

    /// </summary>

    public void RefreshWeeklyContracts()

    {

        currentSchedule.weeklyContracts.Clear();

 

        var available = activitiesDatabase.weeklyContractPool

            .OrderBy(x => UnityEngine.Random.value)

            .Take(3)

            .ToList();

 

        foreach (var contract in available)

        {

            var newContract = JsonUtility.FromJson<WeeklyContract>(JsonUtility.ToJson(contract));

            newContract.isComplete = false;

            newContract.isClaimed = false;

            newContract.expiryTime = DateTime.Now.AddDays(7);

            currentSchedule.weeklyContracts.Add(newContract);

        }

 

        Debug.Log($"Refreshed {currentSchedule.weeklyContracts.Count} weekly contracts");

    }

 

    /// <summary>

    /// Refresh monthly contracts

    /// </summary>

    public void RefreshMonthlyContracts()

    {

        currentSchedule.monthlyContracts.Clear();

 

        var available = activitiesDatabase.monthlyContractPool

            .OrderBy(x => UnityEngine.Random.value)

            .Take(2)

            .ToList();

 

        foreach (var contract in available)

        {

            var newContract = JsonUtility.FromJson<MonthlyContract>(JsonUtility.ToJson(contract));

            newContract.isComplete = false;

            newContract.isClaimed = false;

            newContract.expiryTime = DateTime.Now.AddMonths(1);

            currentSchedule.monthlyContracts.Add(newContract);

        }

 

        Debug.Log($"Refreshed {currentSchedule.monthlyContracts.Count} monthly contracts");

    }

 

    /// <summary>

    /// Refresh bounties

    /// </summary>

    public void RefreshBounties()

    {

        currentSchedule.activeBounties.Clear();

 

        var available = activitiesDatabase.bountyPool

            .OrderBy(x => UnityEngine.Random.value)

            .Take(5)

            .ToList();

 

        foreach (var bounty in available)

        {

            var newBounty = JsonUtility.FromJson<Bounty>(JsonUtility.ToJson(bounty));

            newBounty.isComplete = false;

            newBounty.isClaimed = false;

            newBounty.attemptsUsed = 0;

            newBounty.availableUntil = DateTime.Now.AddDays(7);

            currentSchedule.activeBounties.Add(newBounty);

        }

 

        Debug.Log($"Refreshed {currentSchedule.activeBounties.Count} bounties");

    }

 

    /// <summary>

    /// Update contract objective progress

    /// </summary>

    public void UpdateContractProgress(string objectiveType, string targetId, int count = 1)

    {

        // Update daily contracts

        foreach (var contract in currentSchedule.dailyContracts.Where(c => !c.isComplete))

        {

            UpdateObjectives(contract.objectives, objectiveType, targetId, count);

            CheckContractCompletion(contract);

        }

 

        // Update weekly contracts

        foreach (var contract in currentSchedule.weeklyContracts.Where(c => !c.isComplete))

        {

            UpdateObjectives(contract.objectives, objectiveType, targetId, count);

            CheckContractCompletion(contract);

        }

 

        // Update monthly contracts

        foreach (var contract in currentSchedule.monthlyContracts.Where(c => !c.isComplete))

        {

            UpdateObjectives(contract.objectives, objectiveType, targetId, count);

            CheckContractCompletion(contract);

        }

    }

 

    /// <summary>

    /// Update objectives

    /// </summary>

    private void UpdateObjectives(List<ContractObjective> objectives, string objectiveType, string targetId, int count)

    {

        foreach (var objective in objectives)

        {

            if (objective.objectiveType == objectiveType)

            {

                // Check if target matches

                bool matches = false;

                if (!string.IsNullOrEmpty(objective.targetId))

                {

                    matches = objective.targetId == targetId;

                }

                else if (!string.IsNullOrEmpty(objective.targetCategory))

                {

                    // TODO: Check category

                    matches = true;

                }

                else

                {

                    matches = true; // Any target

                }

 

                if (matches)

                {

                    objective.currentCount = Mathf.Min(objective.currentCount + count, objective.targetCount);

                }

            }

        }

    }

 

    /// <summary>

    /// Check if contract is complete

    /// </summary>

    private void CheckContractCompletion(DailyContract contract)

    {

        bool allComplete = contract.objectives.All(obj => obj.currentCount >= obj.targetCount);

        if (allComplete && !contract.isComplete)

        {

            contract.isComplete = true;

            Debug.Log($"Contract completed: {contract.contractName}");

        }

    }

 

    /// <summary>

    /// Claim contract rewards

    /// </summary>

    public bool ClaimContractReward(string contractId)

    {

        var contract = FindContract(contractId);

        if (contract == null)

        {

            Debug.LogError("Contract not found");

            return false;

        }

 

        if (!contract.isComplete)

        {

            Debug.Log("Contract not complete");

            return false;

        }

 

        if (contract.isClaimed)

        {

            Debug.Log("Rewards already claimed");

            return false;

        }

 

        // Give rewards

        foreach (var reward in contract.rewards)

        {

            GiveReward(reward);

        }

 

        contract.isClaimed = true;

        Debug.Log($"Claimed rewards for {contract.contractName}");

        return true;

    }

 

    /// <summary>

    /// Start bounty

    /// </summary>

    public bool StartBounty(string bountyId)

    {

        var bounty = currentSchedule.activeBounties.Find(b => b.bountyId == bountyId);

        if (bounty == null)

        {

            Debug.LogError("Bounty not found");

            return false;

        }

 

        if (bounty.attemptsUsed >= bounty.attemptsAllowed)

        {

            Debug.Log("No attempts remaining");

            return false;

        }

 

        // TODO: Check requirements (level, reputation, energy)

 

        bounty.attemptsUsed++;

        Debug.Log($"Started bounty: {bounty.bountyName}");

        return true;

    }

 

    /// <summary>

    /// Complete bounty

    /// </summary>

    public bool CompleteBounty(string bountyId, bool success)

    {

        var bounty = currentSchedule.activeBounties.Find(b => b.bountyId == bountyId);

        if (bounty == null)

        {

            Debug.LogError("Bounty not found");

            return false;

        }

 

        if (success)

        {

            bounty.isComplete = true;

 

            // Give rewards

            foreach (var reward in bounty.rewards)

            {

                if (reward.isGuaranteed || UnityEngine.Random.value <= reward.dropChance)

                {

                    GiveBountyReward(reward);

                }

            }

 

            Debug.Log($"Bounty completed: {bounty.bountyName}");

        }

 

        return true;

    }

 

    /// <summary>

    /// Claim daily login reward

    /// </summary>

    public bool ClaimDailyLoginReward()

    {

        if (loginRewards.hasClaimedToday)

        {

            Debug.Log("Already claimed today's login reward");

            return false;

        }

 

        DateTime today = DateTime.Now.Date;

        TimeSpan daysSinceLastLogin = today - loginRewards.lastLoginDate.Date;

 

        if (daysSinceLastLogin.Days > 1)

        {

            // Reset streak if more than 1 day

            loginRewards.consecutiveDays = 1;

        }

        else if (daysSinceLastLogin.Days == 1)

        {

            // Continue streak

            loginRewards.consecutiveDays++;

        }

 

        // Find reward for current day

        var reward = loginRewards.rewards.Find(r => r.day == loginRewards.consecutiveDays);

        if (reward != null)

        {

            // Give reward

            GiveLoginReward(reward);

            reward.isClaimed = true;

        }

 

        loginRewards.lastLoginDate = DateTime.Now;

        loginRewards.hasClaimedToday = true;

 

        Debug.Log($"Claimed day {loginRewards.consecutiveDays} login reward");

        return true;

    }

 

    /// <summary>

    /// Add season pass experience

    /// </summary>

    public void AddSeasonPassExp(int exp)

    {

        if (currentSeasonPass == null)

            return;

 

        currentSeasonPass.currentExp += exp;

 

        // Check for level ups

        while (currentSeasonPass.currentLevel < currentSeasonPass.maxLevel)

        {

            var nextTier = currentSeasonPass.tiers.Find(t => t.tier == currentSeasonPass.currentLevel + 1);

            if (nextTier != null && currentSeasonPass.currentExp >= nextTier.requiredExp)

            {

                currentSeasonPass.currentLevel++;

                Debug.Log($"Season Pass level up! Now level {currentSeasonPass.currentLevel}");

            }

            else

            {

                break;

            }

        }

    }

 

    /// <summary>

    /// Claim season pass tier reward

    /// </summary>

    public bool ClaimSeasonPassReward(int tier, bool premium = false)

    {

        if (currentSeasonPass == null)

        {

            Debug.Log("No active season pass");

            return false;

        }

 

        var passTier = currentSeasonPass.tiers.Find(t => t.tier == tier);

        if (passTier == null)

        {

            Debug.LogError($"Tier {tier} not found");

            return false;

        }

 

        if (currentSeasonPass.currentLevel < tier)

        {

            Debug.Log("Season pass level too low");

            return false;

        }

 

        if (premium)

        {

            if (!currentSeasonPass.hasPremiumPass)

            {

                Debug.Log("Premium pass not purchased");

                return false;

            }

 

            if (passTier.premiumRewardClaimed)

            {

                Debug.Log("Premium reward already claimed");

                return false;

            }

 

            // Give premium rewards

            foreach (var reward in passTier.premiumRewards)

            {

                GivePassReward(reward);

            }

            passTier.premiumRewardClaimed = true;

        }

        else

        {

            if (passTier.freeRewardClaimed)

            {

                Debug.Log("Free reward already claimed");

                return false;

            }

 

            // Give free rewards

            foreach (var reward in passTier.freeRewards)

            {

                GivePassReward(reward);

            }

            passTier.freeRewardClaimed = true;

        }

 

        Debug.Log($"Claimed season pass tier {tier} {(premium ? "premium" : "free")} reward");

        return true;

    }

 

    /// <summary>

    /// Get current activity schedule

    /// </summary>

    public ActivitySchedule GetCurrentSchedule()

    {

        return currentSchedule;

    }

 

    /// <summary>

    /// Get login reward system

    /// </summary>

    public LoginRewardSystem GetLoginRewards()

    {

        return loginRewards;

    }

 

    /// <summary>

    /// Get current season pass

    /// </summary>

    public SeasonPass GetSeasonPass()

    {

        return currentSeasonPass;

    }

 

    // Helper methods

    private DailyContract FindContract(string contractId)

    {

        var contract = currentSchedule.dailyContracts.Find(c => c.contractId == contractId);

        if (contract != null) return contract;

 

        contract = currentSchedule.weeklyContracts.Find(c => c.contractId == contractId);

        if (contract != null) return contract;

 

        return currentSchedule.monthlyContracts.Find(c => c.contractId == contractId);

    }

 

    private void GiveReward(ContractReward reward)

    {

        Debug.Log($"Received: {reward.quantity}x {reward.rewardId}");

        // TODO: Add to player inventory based on rewardType

    }

 

    private void GiveBountyReward(BountyReward reward)

    {

        Debug.Log($"Received: {reward.quantity}x {reward.rewardId}");

        // TODO: Add to player inventory based on rewardType

    }

 

    private void GiveLoginReward(DailyLoginReward reward)

    {

        Debug.Log($"Received login reward: {reward.quantity}x {reward.rewardId}");

        // TODO: Add to player inventory based on rewardType

    }

 

    private void GivePassReward(PassReward reward)

    {

        Debug.Log($"Received season pass reward: {reward.quantity}x {reward.rewardId}");

        // TODO: Add to player inventory based on rewardType

    }

}