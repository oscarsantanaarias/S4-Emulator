﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeoNetsphere.Database.Game
{
  [Table("players")]
  public class PlayerDto
  {
    [Key] public int Id { get; set; }

    public string PlayTime { get; set; }
    public byte TutorialState { get; set; }
    public byte Level { get; set; }
    public uint TotalExperience { get; set; }
    public int PEN { get; set; }
    public int AP { get; set; }
    public int Coins1 { get; set; }
    public int Coins2 { get; set; }
    public byte CurrentCharacterSlot { get; set; }
    public int TotalMatches { get; set; }
    public int TotalWins { get; set; }
    public int TotalLosses { get; set; }

    public IList<PlayerCharacterDto> Characters { get; set; } = new List<PlayerCharacterDto>();
    public IList<PlayerDenyDto> Ignores { get; set; } = new List<PlayerDenyDto>();
    public IList<PlayerItemDto> Items { get; set; } = new List<PlayerItemDto>();
    public IList<PlayerMailDto> Inbox { get; set; } = new List<PlayerMailDto>();
    public IList<PlayerSettingDto> Settings { get; set; } = new List<PlayerSettingDto>();
    public IList<PlayerFriendDto> Friends { get; set; } = new List<PlayerFriendDto>();

    public IList<PlayerDeathMatchDto> DeathMatchInfo { get; set; } = new List<PlayerDeathMatchDto>();
    public IList<PlayerTouchDownDto> TouchDownInfo { get; set; } = new List<PlayerTouchDownDto>();
    public IList<PlayerChaserDto> ChaserInfo { get; set; } = new List<PlayerChaserDto>();
    public IList<PlayerBattleRoyalDto> BattleRoyalInfo { get; set; } = new List<PlayerBattleRoyalDto>();
    public IList<PlayerCaptainDto> CaptainInfo { get; set; } = new List<PlayerCaptainDto>();
  }

  [Table("player_friends")]
  public class PlayerFriendDto
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey(nameof(Player))] public int PlayerId { get; set; }

    public PlayerDto Player { get; set; }

    public int FriendId { get; set; }

    public int PlayerState { get; set; }
    public int FriendState { get; set; }
  }

  [Table("player_info_deathmatch")]
  public class PlayerDeathMatchDto
  {
    [Key] [ForeignKey(nameof(Player))] public int PlayerId { get; set; }

    public PlayerDto Player { get; set; }

    public ulong Won { get; set; }
    public ulong Loss { get; set; }
    public ulong Kills { get; set; }
    public ulong KillAssists { get; set; }
    public ulong Deaths { get; set; }
  }

  [Table("player_info_touchdown")]
  public class PlayerTouchDownDto
  {
    [Key] [ForeignKey(nameof(Player))] public int PlayerId { get; set; }

    public PlayerDto Player { get; set; }

    public ulong Won { get; set; }
    public ulong Loss { get; set; }
    public ulong TD { get; set; }
    public ulong TDAssist { get; set; }
    public ulong Offense { get; set; }
    public ulong OffenseAssist { get; set; }
    public ulong Defense { get; set; }
    public ulong DefenseAssist { get; set; }
    public ulong Kill { get; set; }
    public ulong KillAssist { get; set; }
    public ulong OffenseRebound { get; set; }
    public ulong Heal { get; set; }
  }

  [Table("player_info_chaser")]
  public class PlayerChaserDto
  {
    [Key] [ForeignKey(nameof(Player))] public int PlayerId { get; set; }

    public PlayerDto Player { get; set; }

    public ulong ChasedWon { get; set; }
    public ulong ChasedRounds { get; set; }
    public ulong ChaserWon { get; set; }
    public ulong ChaserRounds { get; set; }
  }

  [Table("player_info_battleroyal")]
  public class PlayerBattleRoyalDto
  {
    [Key] [ForeignKey(nameof(Player))] public int PlayerId { get; set; }

    public PlayerDto Player { get; set; }

    public ulong Won { get; set; }
    public ulong Loss { get; set; }
    public ulong Kills { get; set; }
    public ulong KillAssists { get; set; }
    public ulong FirstKilled { get; set; }
    public ulong FirstPlace { get; set; }
  }

  [Table("player_info_captain")]
  public class PlayerCaptainDto
  {
    [Key] [ForeignKey(nameof(Player))] public int PlayerId { get; set; }

    public PlayerDto Player { get; set; }

    public ulong Won { get; set; }
    public ulong Loss { get; set; }
    public ulong CPTKilled { get; set; }
    public ulong CPTCount { get; set; }
  }

  //[Table("player_info_siege")]
  //public class PlayerSiegeDto
  //{
  //    [Key]
  //    [ForeignKey(nameof(Player))]
  //    public int PlayerId { get; set; }
  //    public PlayerDto Player { get; set; }


  //}

  [Table("player_characters")]
  public class PlayerCharacterDto
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey(nameof(Player))] public int PlayerId { get; set; }

    public PlayerDto Player { get; set; }

    public byte Slot { get; set; }
    public byte Gender { get; set; }
    public int? Weapon1Id { get; set; }
    public int? Weapon2Id { get; set; }
    public int? Weapon3Id { get; set; }
    public int? SkillId { get; set; }
    public int? HairId { get; set; }
    public int? FaceId { get; set; }
    public int? ShirtId { get; set; }
    public int? PantsId { get; set; }
    public int? GlovesId { get; set; }
    public int? ShoesId { get; set; }
    public int? AccessoryId { get; set; }
    public int? PetId { get; set; }
  }

  [Table("player_deny")]
  public class PlayerDenyDto
  {
    [Key] public int Id { get; set; }

    [ForeignKey(nameof(Player))] public int PlayerId { get; set; }

    public PlayerDto Player { get; set; }

    public int DenyPlayerId { get; set; }
  }

  [Table("player_items")]
  public class PlayerItemDto
  {
    [Key] public int Id { get; set; }

    [ForeignKey(nameof(Player))] public int PlayerId { get; set; }

    public PlayerDto Player { get; set; }

    public int ShopItemInfoId { get; set; }
    public int ShopPriceId { get; set; }
    public int Period { get; set; }
    public int DaysLeft { get; set; }
    public string Effects { get; set; }
    public byte Color { get; set; }
    public long PurchaseDate { get; set; }
    public int Durability { get; set; }
    public int Count { get; set; }
    //public uint EnchantMP { get; set; }
    //public byte EnchantLvl { get; set; }
        public PlayerItemDto() { }
  }

  [Table("player_mails")]
  public class PlayerMailDto
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey(nameof(Player))] public int PlayerId { get; set; }

    public PlayerDto Player { get; set; }

    public int SenderPlayerId { get; set; }
    public long SentDate { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public bool IsMailNew { get; set; }
    public bool IsMailDeleted { get; set; }
    public bool IsClubMail { get; set; }
  }

  [Table("player_settings")]
  public class PlayerSettingDto
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey(nameof(Player))] public int PlayerId { get; set; }

    public PlayerDto Player { get; set; }

    public string Setting { get; set; }
    public string Value { get; set; }
  }

  [Table("shop_effect_groups")]
  public class ShopEffectGroupDto
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string Name { get; set; }
    public uint Effect { get; set; }
    public IList<ShopEffectDto> ShopEffects { get; set; } = new List<ShopEffectDto>();
  }

  [Table("shop_effects")]
  public class ShopEffectDto
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey(nameof(EffectGroup))] public int EffectGroupId { get; set; }

    public ShopEffectGroupDto EffectGroup { get; set; }

    public uint Effect { get; set; }
  }

  [Table("shop_price_groups")]
  public class ShopPriceGroupDto
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string Name { get; set; }
    public byte PriceType { get; set; }

    public IList<ShopPriceDto> ShopPrices { get; set; } = new List<ShopPriceDto>();
  }

  [Table("shop_prices")]
  public class ShopPriceDto
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey(nameof(PriceGroup))] public int PriceGroupId { get; set; }

    public ShopPriceGroupDto PriceGroup { get; set; }

    public byte PeriodType { get; set; }
    public int Period { get; set; }
    public int Price { get; set; }
    public bool IsRefundable { get; set; }
    public int Durability { get; set; }
    public bool IsEnabled { get; set; }
  }

  [Table("shop_items")]
  public class ShopItemDto
  {
    [Key] public uint Id { get; set; }

    public byte RequiredGender { get; set; }
    public byte RequiredLicense { get; set; }
    public byte Colors { get; set; }
    public byte UniqueColors { get; set; }
    public byte RequiredLevel { get; set; }
    public byte LevelLimit { get; set; }
    public byte RequiredMasterLevel { get; set; }
    public bool IsOneTimeUse { get; set; }
    public bool IsDestroyable { get; set; }
    public byte MainTab { get; set; }
    public byte SubTab { get; set; }

    public IList<ShopItemInfoDto> ItemInfos { get; set; } = new List<ShopItemInfoDto>();
  }

  [Table("shop_iteminfos")]
  public class ShopItemInfoDto
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey(nameof(ShopItem))] public uint ShopItemId { get; set; }

    public ShopItemDto ShopItem { get; set; }

    public int PriceGroupId { get; set; }
    public int EffectGroupId { get; set; }
    public byte DiscountPercentage { get; set; }
    public byte ShopInfoType { get; set; }
  }

  [Table("shop_version")]
  public class ShopVersionDto
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public byte Id { get; set; }

    public string Version { get; set; }
  }

  [Table("start_items")]
  public class StartItemDto
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int ShopItemInfoId { get; set; }
    public int ShopPriceId { get; set; }
    public int ShopEffectId { get; set; }
    public byte Color { get; set; }
    public int Count { get; set; }
    public byte RequiredSecurityLevel { get; set; }
  }

  [Table("channels")]
  public class ChannelDto
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string Name { get; set; }
    public string Description { get; set; }
    public int PlayerLimit { get; set; }
    public byte MinLevel { get; set; }
    public byte MaxLevel { get; set; }
    public uint Color { get; set; }
  }

  [Table("clubs")]
  public class ClubDto
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public uint Id { get; set; }

    public string Name { get; set; }
    public string Icon { get; set; }
  }

  [Table("club_players")]
  public class ClubPlayerDto
  {
    [Key] [ForeignKey(nameof(Player))] public int PlayerId { get; set; }

    public PlayerDto Player { get; set; }

    [ForeignKey(nameof(Club))] public uint ClubId { get; set; }

    public ClubDto Club { get; set; }

    public int State { get; set; }
    public int Rank { get; set; }
  }
}
