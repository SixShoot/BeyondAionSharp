using Aion.Bots.Protocol;
using Aion.GameServer.Model;
using Aion.GameServer.Network.Aion.ServerPackets;

namespace Aion.Bots.Scenarios;

/// <summary>Client-visible NPC interaction order and reaction pacing from the recorded 4.8 client.</summary>
public static class NaturalDialogProtocol
{
	public static readonly TimeSpan TargetReaction = TimeSpan.FromMilliseconds(250);
	public static readonly TimeSpan PageReaction = TimeSpan.FromMilliseconds(750);

	public static async Task OpenAsync(INaturalJourneySession session, int targetObjectId, CancellationToken token)
	{
		await session.SendPacketAsync(GameClientPackets.Emotion((byte)EmotionType.SELECT_TARGET), token);
		await session.SendPacketAsync(session.Api.Target(targetObjectId), token);
		await session.AdvanceAsync(TargetReaction, token);
		await session.SendPacketAsync(session.Api.TalkTo(targetObjectId), token);
	}

	public static async Task SelectAsync(INaturalJourneySession session, BotClientPacket choice, CancellationToken token)
	{
		await session.AdvanceAsync(PageReaction, token);
		await session.SendPacketAsync(choice, token);
	}

	/// <summary>NR-R11b: the creature <paramref name="npc"/> was last seen trading blows with, in the session's most recent
	/// packets; null when it was in none of their swings.</summary>
	public static int? OpponentOf(INaturalJourneySession session, int npc, int recentPackets = 120)
	{
		IReadOnlyList<DecodedBotServerPacket> history = session.PacketHistory;
		for (int index = history.Count - 1; index >= Math.Max(0, history.Count - recentPackets); index--)
		{
			DecodedBotServerPacket packet = history[index];
			if (packet.PacketType != typeof(SM_ATTACK)) continue;
			int attacker = packet.Get<int>("attackerObjId"), target = packet.Get<int>("targetObjId");
			if (attacker == npc) return target;
			if (target == npc) return attacker;
		}
		return null;
	}
}
