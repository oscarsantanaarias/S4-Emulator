using NeoNetsphere.Network;

namespace NeoNetsphere.Game.GameRules
{
    using System;
    using System.IO;
    using System.Linq;
    using NeoNetsphere;
    using NeoNetsphere.Network.Data.GameRule;
    using NeoNetsphere.Network.Message.GameRule;

    internal class ArcadeGameRule : GameRuleBase
    {
        public ArcadeGameRule(Room room)
            : base(room)
        {
            Briefing = new ArcadeBriefing(this);

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

        public override bool CountMatch => false;
        public override GameRule GameRule => GameRule.Horde; // Cambiar si necesitas otro modo
        public override Briefing Briefing { get; }

        public override void Initialize()
        {
            if (!Room.TeamManager.Teams.ContainsKey(Team.Alpha))
                Room.TeamManager.Add(Team.Alpha, (uint)Room.Options.PlayerLimit, 0);
            if (!Room.TeamManager.Teams.ContainsKey(Team.Beta))
                Room.TeamManager.Add(Team.Beta, 0, 0);

            base.Initialize();
        }

        public override void Cleanup()
        {
            Room.TeamManager.Remove(Team.Alpha);
            Room.TeamManager.Remove(Team.Beta);
            base.Cleanup();
        }

        public override void Update(AccurateDelta delta)
        {
            base.Update(delta);

            try
            {
                if (Room.GameState == GameState.Playing &&
                    !StateMachine.IsInState(GameRuleState.EnteringResult) &&
                    !StateMachine.IsInState(GameRuleState.Result))
                {
                    var playersAlive = Room.TeamManager.Players.Count(p => p.RoomInfo.State == PlayerState.Alive);
                    if (playersAlive <= 1)
                    {
                        StateMachine.Fire(GameRuleStateTrigger.StartResult);
                    }
                }
            }
            catch (Exception ex)
            {
                Room.Logger.Error("ArcadeGameRule.Update: " + ex);
            }
        }

        public override PlayerRecord GetPlayerRecord(Player plr)
        {
            return new ArcadePlayerRecord(plr);
        }

        private bool CanStartGame()
        {
            // Puedes poner condiciones aquí (mínimo 1 jugador, etc.)
            return true;
        }

        public override void OnScoreKill(Player killer, Player assist, Player target, AttackAttribute attackAttribute,
            LongPeerId scoreTarget, LongPeerId scoreKiller, LongPeerId scoreAssist)
        {
            if (killer != null)
                GetRecord(killer).Kills++;

            if (assist != null)
                GetRecord(assist).KillAssists++;

            if (target != null)
                GetRecord(target).Deaths++;
        }

        private static ArcadePlayerRecord GetRecord(Player plr)
        {
            return (ArcadePlayerRecord)plr.RoomInfo.Stats;
        }
    }

    internal class ArcadeBriefing : Briefing
    {
        public ArcadeBriefing(GameRuleBase gameRule)
            : base(gameRule)
        {
        }

        protected override void WriteData(BinaryWriter w, bool isResult)
        {
            base.WriteData(w, isResult);
        }
    }

    internal class ArcadePlayerRecord : PlayerRecord
    {
        public ArcadePlayerRecord(Player plr)
            : base(plr)
        {
        }

        public override uint TotalScore => GetTotalScore();

        public override void Serialize(BinaryWriter w, bool isResult)
        {
            base.Serialize(w, isResult);
            w.Write(Kills);
            w.Write(KillAssists);
            w.Write(Deaths);
        }

        private uint GetTotalScore()
        {
            return Kills * 10 + KillAssists * 5;
        }

        public override int GetExpGain(out int bonusExp)
        {
            bonusExp = 0;
            return 0;
        }
    }
}
