# Leveling Progression & Drop System Guide

## Table of Contents
1. [Hero Leveling System (1-100)](#hero-leveling-system)
2. [Dungeon Drop Improvements](#dungeon-drop-improvements)
3. [Black Market System](#black-market-system)
4. [Drop Rate Analysis](#drop-rate-analysis)

---

## Hero Leveling System (1-100)

### Example: Wolf Hero Progression

**Base Stats (Level 1)**:
- ATK: 50
- DEF: 30
- HP: 500
- SPD: 80
- CRIT: 5%
- CRIT DMG: 150%
- ACC: 85%
- RES: 10%

**Growth Per Level**:
- ATK: +3
- DEF: +2
- HP: +25
- SPD: +0.5 (rounded)
- CRIT: +0.1%
- CRIT DMG: +1%
- ACC: +0.15%
- RES: +0.2%

### Detailed Level Milestones

| Level | ATK | DEF | HP | SPD | CRIT | CRIT DMG | Power | Key Unlocks |
|-------|-----|-----|-----|-----|------|----------|-------|-------------|
| **1** | 50 | 30 | 500 | 80 | 5% | 150% | 1,000 | Starting stats |
| **10** | 77 | 48 | 725 | 85 | 6% | 159% | 1,450 | First skill upgrade unlock |
| **20** | 107 | 68 | 975 | 90 | 7% | 169% | 1,950 | Equipment slot 2 |
| **30** | 137 | 88 | 1,225 | 95 | 8% | 179% | 2,450 | Talent tree unlocked |
| **40** | 167 | 108 | 1,475 | 100 | 9% | 189% | 2,950 | Ascension tier 1 available |
| **50** | 197 | 128 | 1,725 | 105 | 10% | 199% | 3,450 | Equipment slot 3 |
| **60** | 227 | 148 | 1,975 | 110 | 11% | 209% | 3,950 | Ascension tier 2 |
| **70** | 257 | 168 | 2,225 | 115 | 12% | 219% | 4,450 | Equipment slot 4, Limit Break |
| **80** | 287 | 188 | 2,475 | 120 | 13% | 229% | 4,950 | Ascension tier 3 |
| **90** | 317 | 208 | 2,725 | 125 | 14% | 239% | 5,450 | Awakening available |
| **100** | 347 | 228 | 2,975 | 130 | 15% | 249% | 5,950 | Max level |

**Power Calculation**: ATK + DEF + (HP/10) + (SPD × 2)

### Experience Requirements

| Level Range | XP Per Level | Cumulative XP | Time to Level (avg) |
|-------------|--------------|---------------|---------------------|
| 1-10 | 100-500 | 3,000 | 5 min per level |
| 11-20 | 600-1,500 | 13,500 | 10 min per level |
| 21-30 | 1,600-3,000 | 37,500 | 15 min per level |
| 31-40 | 3,100-5,000 | 78,000 | 20 min per level |
| 41-50 | 5,100-8,000 | 143,500 | 30 min per level |
| 51-60 | 8,100-12,000 | 244,500 | 45 min per level |
| 61-70 | 12,100-18,000 | 395,500 | 1 hour per level |
| 71-80 | 18,100-25,000 | 610,000 | 1.5 hours per level |
| 81-90 | 25,100-35,000 | 910,500 | 2 hours per level |
| 91-100 | 35,100-50,000 | 1,336,000 | 3 hours per level |

**Total XP to Max**: ~1,336,000 XP
**Total Time (Active Play)**: ~100-150 hours

### Ascension System

**Ascension Tiers**:
1. **Tier 0** (Base): No bonuses
2. **Tier 1** (Level 40+): +10% all stats, unlock talent nodes 1-10
3. **Tier 2** (Level 60+): +20% all stats, unlock talent nodes 11-20, +1 skill level cap
4. **Tier 3** (Level 80+): +30% all stats, unlock talent nodes 21-30, +2 skill level cap
5. **Tier 4** (Level 90+): +40% all stats, unlock all talent nodes, Awakening skill

**Ascension Materials** (per tier):
- Tier 1: 20x Ascension Crystals, 50,000 Gold
- Tier 2: 40x Ascension Crystals, 15x Rare Essence, 150,000 Gold
- Tier 3: 60x Ascension Crystals, 30x Rare Essence, 10x Epic Shard, 300,000 Gold
- Tier 4: 100x Ascension Crystals, 50x Rare Essence, 25x Epic Shard, 5x Mythic Core, 500,000 Gold

### Stats at Level 100 (Fully Ascended)

| Stat | Base (Lv100) | +Ascension (40%) | +Gear (avg) | +Talents (avg) | **Final** |
|------|--------------|------------------|-------------|----------------|-----------|
| ATK | 347 | +139 | +200 | +100 | **786** |
| DEF | 228 | +91 | +150 | +75 | **544** |
| HP | 2,975 | +1,190 | +2,000 | +1,000 | **7,165** |
| SPD | 130 | +52 | +30 | +20 | **232** |
| CRIT | 15% | +6% | +25% | +15% | **61%** |
| CRIT DMG | 249% | +100% | +80% | +50% | **479%** |

**Final Power**: ~15,000-20,000 (depending on gear quality)

---

## Dungeon Drop Improvements

### Current Issues Identified:
1. ❌ Only gold, gems, and generic gear - lacks variety
2. ❌ No progression feeling - similar drops across tiers
3. ❌ Missing: materials, runes, XP items, shards
4. ❌ No rare/chase items
5. ❌ Drop weights don't scale well
6. ❌ No multi-drop tables (only get 1 item per clear)

### Improvements Made:

#### 1. **Multi-Tier Drop System**
Each dungeon clear now rolls on MULTIPLE drop tables:
- **Guaranteed Drop**: Always get gold + XP
- **Common Drop** (80% chance): Enhancement materials, basic gear
- **Uncommon Drop** (40% chance): Better gear, more materials
- **Rare Drop** (15% chance): Epic gear, hero shards, runes
- **Epic Drop** (5% chance): Legendary gear, artifacts, keys
- **Mythic Drop** (1% chance): Mythic gear, complete artifacts

#### 2. **Item Type Diversity**
Now includes:
- **Currency**: Gold, Gems, Arena Tokens, Guild Coins
- **Materials**: Enhancement Stones (T1-T4), Ascension Crystals, Limit Break Stones
- **Gear**: Weapons, Armor, Accessories (Common → Mythic)
- **Runes**: Stat runes (ATK, DEF, HP, SPD, CRIT)
- **Consumables**: XP Potions, Stamina Refills, Buff Scrolls
- **Hero Shards**: Farmable hero shards
- **Special**: Artifact Fragments, Research Points, Mythic Keys

#### 3. **Progressive Rewards**
Rewards scale meaningfully with difficulty:

| Difficulty | Gold Range | Gems | Gear Tier | Materials | Special Items |
|------------|------------|------|-----------|-----------|---------------|
| Normal T1-T6 | 2k-10k | 3-20 | Common-Rare | T1 | - |
| Hard T1-T6 | 10k-50k | 20-80 | Rare-Epic | T2 | Hero Shards (rare) |
| Nightmare T1-T6 | 50k-150k | 80-250 | Epic-Legendary | T3 | Hero Shards, Runes |
| Mythic T1-T4 | 150k-500k | 250-1000 | Legendary-Mythic | T4 | Artifacts, Keys |

#### 4. **Pity System**
- After 10 clears without a rare+ drop → guaranteed rare
- After 50 clears without epic+ drop → guaranteed epic
- After 100 clears → guaranteed legendary
- Mythic dungeons have higher base rates

#### 5. **Bonus Drops**
- **First Clear**: 2x rewards + guaranteed rare item
- **Daily Bonus**: First 3 clears get +50% drops
- **Weekend Bonus**: Saturday/Sunday +25% drop rates
- **Event Multipliers**: 2x-5x during events

---

## Black Market System

### Overview
The Black Market is a rotating shop that offers **discounted items** (10-70% off) from all game systems.

### Features:
- **Rotates every 6 hours** (4 refreshes per day)
- **3 free manual refreshes per day**, then costs 50 gems
- **6 simultaneous offers** with varying rarities
- **One featured deal** per rotation (extra 10% off)
- **Limited stock** on premium items
- **Level-gated offers** (better items at higher levels)

### Offer Tiers by Player Level:

#### Early Game (1-20)
- Gold bundles (30-50% off)
- Enhancement Stones T1 (25-45% off)
- Rare gear (30-60% off)
- XP potions (35-55% off)

#### Mid Game (21-50)
- Larger gold bundles (25-45% off)
- Enhancement Stones T2 (20-40% off)
- Epic gear (25-50% off)
- Ascension materials (30-50% off)
- Hero shards 3-8 (20-35% off)
- Runes T2 (25-45% off)

#### Late Game (51-80)
- Massive gold bundles (20-35% off)
- Mythic gear (15-30% off)
- Limit Break materials (25-40% off)
- Hero shard bundles 10-20 (15-30% off)
- Runes T3 (20-35% off)
- Artifact fragments (25-40% off)

#### End Game (81-100)
- Legendary Relics (10-25% off)
- Awakening materials (15-30% off)
- Premium hero shard bundles 20-50 (10-20% off)
- Legendary Runes T4 (15-25% off)
- Complete Artifacts (10-20% off) - RARE!
- Research Points (20-35% off)
- Mythic Keys (15-30% off)

### Strategy Tips:
1. **Save gems for Mythic+ item appearances** - they're rare!
2. **Use free refreshes daily** - don't waste them
3. **Buy featured deals** - extra 10% off is significant
4. **Stock up on materials during mid-game** - you'll need them later
5. **Artifact appearances are rare** - buy on sight if affordable

---

## Drop Rate Analysis

### Current Drop Rates (OLD SYSTEM)
```
Normal T1: 60% gold, 20% gems, 20% gear (1 item total)
Hard T1: 40% gold, 30% gems, 30% gear (1 item total)
```

**Issues**:
- Only 1 item per clear
- High currency drop rates (boring)
- Low gear drop rates
- No variety

### New Drop Rates (IMPROVED SYSTEM)

**Normal Dungeon T1** (Multiple rolls):
```
Guaranteed: 100 XP + Gold (2,000-3,000)
Common (80%): Enhancement Stone T1 (60%), Common Gear (40%)
Uncommon (40%): Rare Gear (50%), XP Potion (30%), Gems (20%)
Rare (15%): Epic Gear (40%), Hero Shards (35%), Rune T1 (25%)
Epic (5%): Legendary Gear (50%), Artifact Fragment (30%), Gems (20%)
Mythic (1%): Mythic Gear (70%), Artifact (30%)
```

**Expected drops per clear**:
- 1.0 guaranteed (XP + Gold)
- 0.8 common items
- 0.4 uncommon items
- 0.15 rare items
- 0.05 epic items
- 0.01 mythic items
= **~2.4 items per clear average**

### Comparison Table

| Metric | Old System | New System | Improvement |
|--------|-----------|------------|-------------|
| Items per clear | 1-2 | 2-4 | **+100%** |
| Gear drop rate | 20-30% | 60-80% | **+200%** |
| Variety | 3 types | 20+ types | **+566%** |
| Rare+ items | 0-5% | 20%+ | **+300%** |
| Feels rewarding? | ❌ No | ✅ Yes | 👍 |

### Recommended Drop Rates by Difficulty

| Difficulty | Common | Uncommon | Rare | Epic | Mythic |
|------------|--------|----------|------|------|--------|
| Normal | 80% | 40% | 15% | 5% | 1% |
| Hard | 90% | 60% | 30% | 10% | 3% |
| Nightmare | 100% | 80% | 50% | 20% | 5% |
| Mythic | 100% | 100% | 80% | 40% | 10% |

---

## Loot Table Design Philosophy

### Core Principles:
1. **Always feel rewarded** - Multiple drops per clear
2. **Progressive improvement** - Better loot at higher tiers
3. **Variety is key** - Avoid boring gold-only drops
4. **Chase items exist** - 1% mythic drops keep it exciting
5. **Respect player time** - Guaranteed progress via pity system
6. **Special occasions matter** - First clear, daily bonus, events

### Material Economy Balance:
- **Common materials** (Enhancement Stones): Plentiful, used constantly
- **Uncommon materials** (Ascension Crystals): Regular supply needed
- **Rare materials** (Limit Break Stones): Moderate scarcity
- **Epic materials** (Awakening Essence): Rare, valuable
- **Mythic materials** (Mythic Cores): Very rare, endgame only

### Player Progression Curve:
- **Levels 1-20**: Focus on XP, gold, basic gear (learning phase)
- **Levels 21-40**: Need ascension materials, rare gear (growth phase)
- **Levels 41-60**: Farming hero shards, epic gear, runes (power phase)
- **Levels 61-80**: Limit break materials, legendary gear (refinement phase)
- **Levels 81-100**: Awakening materials, mythic gear, artifacts (endgame phase)

### Recommended Farming Routes:

**Early Game** (1-20):
- Farm: Normal Dungeons T1-T3
- Goal: XP, gold, common/rare gear
- Time: 2-3 hours to hit level 20

**Mid Game** (21-50):
- Farm: Hard Dungeons T1-T4, Arena
- Goal: Ascension materials, epic gear, hero shards
- Time: 20-30 hours of progression

**Late Game** (51-80):
- Farm: Nightmare Dungeons, World Boss, Guild Raids
- Goal: Limit break materials, legendary gear, runes
- Time: 40-60 hours of grinding

**End Game** (81-100):
- Farm: Mythic Dungeons, High-tier World Boss, Guild Raids
- Goal: Awakening materials, mythic gear, artifacts
- Time: 50+ hours at endgame

---

## Summary of Improvements

### Black Market:
✅ Time-gated rotating shop
✅ Significant discounts (10-70% off)
✅ Level-based offer pools
✅ Featured deals system
✅ Manual refresh option

### Drop System:
✅ Multi-roll drop tables (2-4 items per clear)
✅ 20+ item types (was 3)
✅ Pity system for rare drops
✅ Progressive scaling by difficulty
✅ Bonus systems (first clear, daily, events)
✅ Chase items (mythic tier 1% drops)

### Leveling:
✅ Clear stat progression (1-100)
✅ Ascension tier system
✅ Milestone unlocks every 10 levels
✅ Meaningful power growth
✅ Balanced time investment

**Result**: A more engaging, rewarding, and balanced progression system that respects player time while maintaining excitement and chase goals.
