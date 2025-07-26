using NeoNetsphere.Network;

namespace NeoNetsphere.Game.GameRules
{
  using System;
  using System.IO;
    using System.Linq;
    using NeoNetsphere;
  using NeoNetsphere.Network.Data.GameRule;
  using NeoNetsphere.Network.Message.GameRule;

  internal class TrainingGameRule : GameRuleBase
  {
    public TrainingGameRule(Room room)
        : base(room)
    {
      Briefing = new Briefing(this);

      StateMachine.Configure(GameRuleState.Waiting)
          .PermitIf(GameRuleStateTrigger.StartPrepare, GameRuleState.Preparing, CanStartGame);

      StateMachine.Configure(GameRuleState.Preparing)
          .Permit(GameRuleStateTrigger.StartGame, GameRuleState.Playing);

      StateMachine.Configure(GameRuleState.Playing)
          .Permit(GameRuleStateTrigger.StartResult, GameRuleState.EnteringResult);

      StateMachine.Configure(GameRuleState.EnteringResult)
          .Permit(GameRuleStateTrigger.StartResult, GameRuleState.Result);

      StateMachine.Configure(GameRuleState.Result)
          .Permit(GameRuleStateTrigger.EndGame, GameRuleState.Waiting);
    }

    public override bool CountMatch => false; // No cuenta en estadísticas
    public override GameRule GameRule => GameRule.Challenge; // Asegúrate de que exista este modo
    public override Briefing Briefing { get; }

    public override void Initialize()
    {
      // Solo un equipo (todos contra todos o práctica libre)
      Room.TeamManager.Add(Team.Alpha, (uint)Room.Options.PlayerLimit, 0);
      base.Initialize();
    }

    public override void Cleanup()
    {
      Room.TeamManager.Remove(Team.Alpha);
      base.Cleanup();
    }

    public override void Update(AccurateDelta delta)
    {
      base.Update(delta);

      if (Room.GameState == GameState.Playing &&
          !StateMachine.IsInState(GameRuleState.EnteringResult) &&
          !StateMachine.IsInState(GameRuleState.Result))
      {
        // No hay límite de tiempo ni de puntuación (modo infinito)
        // Opcional: Terminar si solo queda 1 jugador
        var players = Room.TeamManager.Players.Count(p => p.RoomInfo.State == PlayerState.Alive);
        if (players <= 1 && Room.Options.IsFriendly)
          StateMachine.Fire(GameRuleStateTrigger.StartResult);
      }
    }

    public override PlayerRecord GetPlayerRecord(Player plr)
    {
      return new TrainingPlayerRecord(plr);
    }

    private bool CanStartGame()
    {
      return StateMachine.IsInState(GameRuleState.Waiting);
    }

    // Sin penalización por muerte ni puntuación competitiva
    public override void OnScoreKill(Player killer, Player assist, Player target, AttackAttribute attackAttribute,
        LongPeerId scoreTarget, LongPeerId scoreKiller, LongPeerId scoreAssist)
    {
      // No afecta el score (solo registro)
      if (killer != null)
        GetRecord(killer).Kills++;
      if (assist != null)
        GetRecord(assist).KillAssists++;
      if (target != null)
        GetRecord(target).Deaths++;
    }

    private static TrainingPlayerRecord GetRecord(Player plr)
    {
      return (TrainingPlayerRecord)plr.RoomInfo.Stats;
    }
  }

  internal class TrainingPlayerRecord : PlayerRecord
  {
    public TrainingPlayerRecord(Player plr)
        : base(plr)
    {
    }

    public override uint TotalScore => 0; // Sin puntuación global

    public override void Serialize(BinaryWriter w, bool isResult)
    {
      base.Serialize(w, isResult);
      w.Write(Kills);
      w.Write(KillAssists);
      w.Write(Deaths);
    }

    public override void Reset()
    {
      base.Reset();
    }

    public override int GetExpGain(out int bonusExp)
    {
      bonusExp = 0;
      return 0; // Sin experiencia (o ajusta según tu servidor)
    }
  }
}