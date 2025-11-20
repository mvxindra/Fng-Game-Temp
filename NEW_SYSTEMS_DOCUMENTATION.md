# New Game Systems Documentation

This document describes all the new systems implemented for the game.

## Table of Contents

1. [Save/Load System](#saveload-system)
2. [Friend System](#friend-system)
3. [Replay/Spectate System](#replayspectate-system)
4. [Chat Cosmetics](#chat-cosmetics)
5. [Player Profile System](#player-profile-system)
6. [Research Tree](#research-tree)
7. [Artifact/Relic System](#artifactrelic-system)
8. [Updated Currency System](#updated-currency-system)

---

## Save/Load System

**Location**: `Assets/Scripts/Systems/SaveLoadManager.cs`

### Overview
Comprehensive save/load system that persists all player data to disk.

### Features
- Auto-save every 5 minutes (configurable)
- JSON-based save format
- Saves on application quit/pause
- Versioned save system
- Integrates with all game managers

### Usage
```csharp
// Save game
SaveLoadManager.Instance.SaveGame();

// Load game
SaveLoadManager.Instance.LoadGame();

// Check if save exists
bool hasSave = SaveLoadManager.Instance.SaveExists();

// Delete save
SaveLoadManager.Instance.DeleteSave();
```

### Save File Location
`Application.persistentDataPath/PlayerSave.json`

---

## Friend System

**Location**:
- Data Models: `Assets/Scripts/DataModels/FriendSystem.cs`
- Manager: `Assets/Scripts/Managers/FriendManager.cs`
- UI: `Assets/Scripts/UI/FriendListUI.cs`
- Config: `Assets/Resources/Config/friend_shop.json`

### Features
1. **Friend List Management**
   - Send/accept/reject friend requests
   - Remove friends
   - Block players
   - Favorite friends
   - View online status
   - Up to 100 friends

2. **Support Hero System**
   - Rent support hero once per day per friend
   - Earn friend points when friends use your support hero
   - Track usage statistics

3. **Friend Points**
   - Earn by using support heroes (10 points)
   - Earn when friends use your hero (25 points)
   - Daily gift system (5 points)
   - Co-op completion (50 points)
   - Daily cap: 500 points
   - Max storage: 10,000 points

4. **Friend Shop**
   - Purchase materials with friend points
   - Cosmetics and rare items
   - Universal hero shards
   - Daily/total purchase limits
   - Unlock requirements based on friend count

### API Examples
```csharp
// Send friend request
FriendManager.Instance.SendFriendRequest("playerId", "Optional message");

// Accept request
FriendManager.Instance.AcceptFriendRequest("requestId");

// Rent support hero
SupportHero hero = FriendManager.Instance.RentSupportHero("friendId");

// Purchase from shop
FriendManager.Instance.PurchaseFriendShopItem("itemId");

// Get friends list
List<FriendEntry> friends = FriendManager.Instance.GetFriends();
```

---

## Replay/Spectate System

**Location**:
- Data Models: `Assets/Scripts/DataModels/ReplaySystem.cs`
- Manager: `Assets/Scripts/Managers/ReplayManager.cs`
- UI: `Assets/Scripts/UI/ReplayViewerUI.cs`

### Features
1. **Battle Recording**
   - Arena battles
   - World Boss runs
   - Guild Raids
   - General battles
   - Turn-by-turn recording

2. **Replay Management**
   - Store up to 50 replays
   - Favorite system
   - Auto-delete oldest non-favorite
   - Search and filter
   - View count tracking

3. **Playback Controls**
   - Play/pause/stop
   - Speed control (1x, 1.5x, 2x, 3x)
   - Jump to specific turn
   - Skip animations

4. **Copy Formation Feature**
   - Extract team composition from replays
   - Copy attacker or defender formation
   - Apply to your own party
   - Includes hero positions, gear, talents

5. **Top Replays**
   - Featured arena replays
   - Top World Boss runs
   - Collections (weekly top, hall of fame)

### API Examples
```csharp
// Record Arena battle
string replayId = ReplayManager.Instance.RecordArenaBattle(
    attackers, defenders, turns, winnerTeam
);

// Record World Boss
ReplayManager.Instance.RecordWorldBossBattle(
    bossId, team, turns, damageDealt, timeElapsed, rank
);

// Get replay
ExtendedBattleReplay replay = ReplayManager.Instance.GetReplay(replayId);

// Copy formation
TeamFormation formation = ReplayManager.Instance.CopyFormation(replayId);

// Apply formation
ReplayManager.Instance.ApplyFormationToParty(formation);

// Start playback
ReplayManager.Instance.StartPlayback(replayId);
ReplayManager.Instance.SetPlaybackSpeed(2.0f);
```

---

## Chat Cosmetics

**Location**:
- Data Models: `Assets/Scripts/DataModels/ChatCosmeticSystem.cs`
- Manager: `Assets/Scripts/Managers/ChatCosmeticManager.cs`
- Config: `Assets/Resources/Config/chat_cosmetics.json`

### Features
1. **Emojis**
   - Static and animated emojis
   - Categories: emotion, reaction, event, special
   - Favorite system (up to 20 favorites)
   - Rarity tiers

2. **Stickers**
   - Animated sprite sheets
   - Limited-time stickers
   - Event-exclusive stickers

3. **Badges**
   - Animated reaction badges
   - Types: Achievement, Rank, Event, VIP, Seasonal, Special
   - Animation types: glow, pulse, sparkle, rotate

4. **Chat Bubble Styles**
   - Custom borders and frames
   - Background/text colors
   - Particle effects

5. **Chat Effects**
   - Send/receive effects
   - Sound effects
   - Visual effects

### Unlocking
- Achievement rewards
- Event participation
- Shop purchases
- Rank milestones
- Season pass

### API Examples
```csharp
// Unlock emoji
ChatCosmeticManager.Instance.UnlockEmoji("sparkles");

// Equip chat bubble
ChatCosmeticManager.Instance.EquipChatBubble("gold_bubble");

// Equip badge
ChatCosmeticManager.Instance.EquipBadge("arena_champion");

// Add favorite emoji
ChatCosmeticManager.Instance.AddFavoriteEmoji("victory");

// Unlock from achievement
ChatCosmeticManager.Instance.UnlockFromAchievement("achievementId");
```

---

## Player Profile System

**Location**:
- Data Models: `Assets/Scripts/DataModels/PlayerProfileSystem.cs`
- Manager: `Assets/Scripts/Managers/PlayerProfileManager.cs`

### Features
1. **Profile Information**
   - Player name, level, experience
   - Account creation date
   - Total play time
   - Guild affiliation

2. **Customization**
   - Profile icons/avatars
   - Profile frames (borders)
   - Background images
   - Nameplates
   - Titles
   - Status message
   - Bio (500 characters)

3. **Stats Showcase**
   - Combat stats (battles, win rate, highest damage)
   - Hero collection stats
   - Progression (tower floor, dungeons)
   - PvP stats (arena rank, season wins)
   - Guild stats
   - World Boss stats

4. **Profile Showcase**
   - Featured heroes (up to 3)
     - 3D or 2D display
     - Custom poses
     - Background effects
   - Featured achievements (up to 5)
   - Hall of Fame entries
   - Recent activity log

5. **Privacy Settings**
   - Show/hide online status
   - Profile visibility (Everyone, Friends, Guild, Private)
   - Toggle hero collection visibility
   - Toggle stats visibility

### API Examples
```csharp
// Update player name
PlayerProfileManager.Instance.UpdatePlayerName("NewName");

// Equip title
PlayerProfileManager.Instance.EquipTitle("champion");

// Set featured hero
PlayerProfileManager.Instance.SetFeaturedHero(0, "heroId");

// Add featured achievement
PlayerProfileManager.Instance.AddFeaturedAchievement("achievementId");

// Update combat stats
PlayerProfileManager.Instance.UpdateCombatStats(won: true, damage: 50000);

// Get profile for display
ProfileViewData profile = PlayerProfileManager.Instance.GetProfileViewData();
```

---

## Research Tree

**Location**:
- Data Models: `Assets/Scripts/DataModels/ResearchTreeSystem.cs`
- Manager: `Assets/Scripts/Managers/ResearchTreeManager.cs`
- Config: `Assets/Resources/Config/research_trees.json`

### Overview
Account-wide permanent upgrade system with slow progression and powerful effects.

### Features
1. **Research Categories**
   - Combat Mastery (stat boosts)
   - Economy (resource bonuses)
   - Collection (hero/gear bonuses)
   - Convenience (QoL features)

2. **Research Points**
   - Earned from: Events, World Boss, Endgame dungeons
   - Spent to unlock research nodes
   - Permanent investment

3. **Node Types**
   - **StatBoost**: Permanent stat increases (+2% HP, +2% ATK, etc.)
   - **ResourceBonus**: Increase resource gains (+10% gold, +5% drop rate)
   - **UnlockFeature**: Unlock new features
   - **QualityOfLife**: Convenience features
   - **Special**: Unique effects

4. **Progression System**
   - Tier-based (1-5)
   - Prerequisites required
   - Account level requirements
   - Upgradeable nodes (1-5 levels)
   - Time-gated research (optional)

5. **Example Nodes**
   - "+2% HP to all heroes" (upgradeable to +10%)
   - "+10% Gold from battles"
   - "+1 extra item from dungeon chests"
   - "Unlock Auto-Repeat for dungeons"
   - "+5% EXP gain"
   - "Increase friend list capacity by 10"

### API Examples
```csharp
// Add research points
ResearchTreeManager.Instance.AddResearchPoints(10);

// Unlock node
ResearchTreeManager.Instance.UnlockNode("combat_hp_1");

// Upgrade node
ResearchTreeManager.Instance.UpgradeNode("combat_hp_1");

// Get stat bonus
float hpBonus = ResearchTreeManager.Instance.GetStatBonus("hp");

// Check if unlocked
bool unlocked = ResearchTreeManager.Instance.IsNodeUnlocked("combat_hp_1");

// Get available points
int points = ResearchTreeManager.Instance.GetAvailablePoints();
```

### Research Trees Included
1. **Combat Mastery**
   - Vitality I-V (+2% HP per level)
   - Strength I-V (+2% ATK per level)
   - Resilience I-V (+2% DEF per level)
   - Critical Strike (+5% Crit Damage)

2. **Economy**
   - Gold Rush (+10% gold from battles)
   - Quick Learner (+5% EXP gain)
   - Lucky Find (+5% material drop rate)

---

## Artifact/Relic System

**Location**:
- Data Models: `Assets/Scripts/DataModels/ArtifactSystem.cs`
- Manager: `Assets/Scripts/Managers/ArtifactManager.cs`
- Config: `Assets/Resources/Config/artifacts.json`

### Overview
Endgame equipment system with unique, powerful effects. 1-2 artifact slots per hero.

### Features
1. **Artifact Types**
   - **Weapon**: Offensive artifacts
   - **Armor**: Defensive artifacts
   - **Accessory**: Utility artifacts
   - **Special**: Unique artifacts

2. **Rarity Tiers**
   - Legendary
   - Mythic
   - Ancient

3. **Unique Effects**
   - Resurrection ("Phoenix Feather")
   - Conditional bonuses ("Curse of the Infected")
   - Damage amplification
   - Damage reduction
   - Defense penetration
   - Turn-based effects

4. **Upgrade System**
   - Max level: 10 (configurable per artifact)
   - Enhancement levels
   - Random substats
   - Increasing power per level

5. **Artifact Sets**
   - 2-piece bonuses
   - 4-piece bonuses
   - 6-piece bonuses (optional)
   - Mix and match across heroes

6. **Acquisition Sources**
   - World Boss drops
   - Guild Raids
   - Endgame dungeons
   - Special events
   - Crafting system

7. **Management**
   - Lock/unlock system
   - Favorite system
   - Dismantle for materials
   - Auto-unequip when equipping to another hero

### Example Artifacts

**Phoenix Feather** (Mythic Accessory)
- Base: First time hero dies, revive with 30% HP
- Level 5: Revive with 50% HP
- Level 10: Revive with 50% HP + cleanse debuffs

**Curse of the Infected** (Legendary Special)
- Base: 2+ Infected heroes → +8% ATK to Infected
- Level 5: 3+ Infected heroes → +10% ATK
- Level 10: 3+ Infected heroes → +15% ATK, +10% HP

**Dragon's Might** (Ancient Weapon)
- Base: +10% damage to all enemies
- Level 5: +15% damage, +5% crit rate
- Level 10: +20% damage, +10% crit, +25% crit dmg

**Guardian's Oath** (Mythic Armor)
- Base: -10% damage taken when HP > 50%
- Level 5: -15% damage taken
- Level 10: -20% damage, reflect 10% damage

**Void Crystal** (Ancient Accessory)
- Base: Skills ignore 10% enemy DEF
- Level 5: Skills ignore 15% enemy DEF
- Level 10: Skills ignore 20% DEF, +10% skill damage

### API Examples
```csharp
// Award artifact
string instanceId = ArtifactManager.Instance.AwardArtifact("phoenix_feather");

// Equip to hero (max 2 per hero)
ArtifactManager.Instance.EquipArtifact("heroId", instanceId);

// Unequip
ArtifactManager.Instance.UnequipArtifact("heroId", instanceId);

// Upgrade
ArtifactManager.Instance.UpgradeArtifact(instanceId);

// Get equipped artifacts
List<ArtifactInstance> artifacts = ArtifactManager.Instance.GetEquippedArtifacts("heroId");

// Get active set bonuses
List<ArtifactEffect> setBonuses = ArtifactManager.Instance.GetActiveSetBonuses("heroId");

// Lock/favorite
ArtifactManager.Instance.ToggleLock(instanceId);
ArtifactManager.Instance.ToggleFavorite(instanceId);
```

---

## Updated Currency System

**Location**:
- Data Models: `Assets/Scripts/DataModels/CurrencySystem.cs`
- Manager: `Assets/Scripts/Managers/CurrencyManager.cs`

### New Currencies Added
1. **Friend Points** - For friend shop purchases
2. **Research Points** - For research tree unlocks

### Usage
```csharp
// Get currency
int friendPoints = CurrencyManager.Instance.GetCurrency("friendPoints");
int researchPoints = CurrencyManager.Instance.GetCurrency("researchPoints");

// Add currency
CurrencyManager.Instance.AddCurrency("friendPoints", 50);
CurrencyManager.Instance.AddCurrency("researchPoints", 5);

// Deduct currency
CurrencyManager.Instance.DeductCurrency("friendPoints", 100);
```

---

## Integration Notes

### Initialization Order
1. SaveLoadManager (loads saved data)
2. CurrencyManager
3. FriendManager
4. ResearchTreeManager
5. ArtifactManager
6. PlayerProfileManager
7. ChatCosmeticManager
8. ReplayManager

### Data Flow
```
Game Start
    → SaveLoadManager.LoadGame()
    → All managers receive their save data
    → Systems apply loaded state

Game Runtime
    → Players interact with systems
    → Systems update their data

Game End/Auto-Save
    → SaveLoadManager.SaveGame()
    → All managers provide their save data
    → Data written to disk
```

### UI Integration
All UI controllers follow the pattern:
- Simple MonoBehaviour components
- Direct references to UI elements
- Button click handlers
- Refresh methods to update display
- Integration with singleton managers

### Configuration Files
All systems use JSON configuration files in `Assets/Resources/Config/`:
- `friend_shop.json` - Friend shop items
- `chat_cosmetics.json` - Emojis, stickers, badges
- `research_trees.json` - Research nodes and trees
- `artifacts.json` - Artifact configs and sets

---

## Future Enhancements

### Potential Additions
1. **Friend System**
   - Daily gift exchange
   - Co-op dungeon invitations
   - Friend recommendations
   - Mutual friend tracking

2. **Replay System**
   - Share replays with friends
   - Public replay library
   - Replay comments/reactions
   - Downloadable replays

3. **Player Profile**
   - Profile themes
   - Animated backgrounds
   - Badge collections
   - Achievement showcase expansion

4. **Research Tree**
   - Multiple research paths
   - Specialization trees
   - Research presets
   - Reset functionality

5. **Artifacts**
   - Artifact fusion
   - Set dungeon bonuses
   - Artifact awakening
   - Legendary effects

---

## Testing Checklist

### Save/Load System
- [ ] Save game and verify file created
- [ ] Load game and verify data restored
- [ ] Test auto-save functionality
- [ ] Test save on quit/pause

### Friend System
- [ ] Send/accept friend requests
- [ ] Rent support hero
- [ ] Purchase from friend shop
- [ ] Test daily limits

### Replay System
- [ ] Record battle replay
- [ ] Play back replay
- [ ] Copy formation
- [ ] Test playback controls

### Chat Cosmetics
- [ ] Unlock emojis/stickers/badges
- [ ] Equip cosmetics
- [ ] Test achievement integration

### Player Profile
- [ ] Customize profile
- [ ] Set featured heroes
- [ ] Update stats
- [ ] View profile

### Research Tree
- [ ] Unlock research nodes
- [ ] Upgrade nodes
- [ ] Test prerequisites
- [ ] Verify stat bonuses applied

### Artifacts
- [ ] Award artifacts
- [ ] Equip to heroes
- [ ] Upgrade artifacts
- [ ] Test set bonuses
- [ ] Verify 2-slot limit

---

## Support

For issues or questions about these systems:
1. Check manager Debug.Log outputs
2. Verify JSON configs are properly formatted
3. Ensure save/load integration is working
4. Check singleton initialization order

All systems use extensive debug logging for troubleshooting.
