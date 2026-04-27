namespace GameCore.Core.Comp.Event{
    /// <summary>
    /// 事件ID枚举
    /// 定义游戏内所有事件的唯一标识
    /// </summary>
    public enum EventId{
        None = 0,

        // 玩家事件 (1000-1999)
        PlayerLevelUp = 1000,
        PlayerDeath = 1001,
        PlayerRespawn = 1002,
        PlayerHPChange = 1003,
        PlayerMPChange = 1004,

        // 游戏事件 (2000-2999)
        GameStart = 2000,
        GamePause = 2001,
        GameResume = 2002,
        GameOver = 2003,
        GameRestart = 2004,
        ChangeGameState = 2005,

        // UI事件 (3000-3999)
        UIOpen = 3000,
        UIClose = 3001,
        UIShow = 3002,
        UIHide = 3003,

        // 战斗事件 (4000-4999)
        BattleStart = 4000,
        BattleEnd = 4001,
        EnemyDeath = 4002,
        BossDeath = 4003,
        ComboHit = 4004,
        SkillUse = 4005,

        // 物品事件 (5000-5999)
        ItemPickup = 5000,
        ItemDrop = 5001,
        ItemUse = 5002,
        ItemEquip = 5003,
        ItemUnequip = 5004,

        // 任务事件 (6000-6999)
        QuestAccept = 6000,
        QuestComplete = 6001,
        QuestAbandon = 6002,
        QuestProgress = 6003,

        // 音频事件 (7000-7999)
        AudioPlay = 7000,
        AudioStop = 7001,
        AudioPause = 7002,
        AudioResume = 7003,

        // 场景事件 (8000-8999)
        SceneLoad = 8000,
        SceneUnload = 8001,
        SceneChange = 8002,
    }
}