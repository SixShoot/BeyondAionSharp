using Aion.GameServer.SkillEngine.Effects;
using Aion.GameServer.SkillEngine.Model;

namespace Aion.GameServer.Tests;

/// <summary>
/// Java Effect.getSuccessEffects() is the success map's live values() view (Effect.java 854-856). EffectTemplate.calculate empties
/// the map through it when an effect after the first is dodged or resisted (EffectTemplate.java 315-316); the port threw
/// "Collection is read-only" there (NR-R11a).
/// </summary>
public sealed class EffectSuccessEffectsViewPortTests
{
    [Fact]
    public void ClearOnTheViewEmptiesTheSuccessMap()
    {
        Effect effect = WithSuccessAt(1, 2);

        ICollection<EffectTemplate> view = effect.GetSuccessEffects();
        Assert.Equal(2, view.Count);
        view.Clear();

        Assert.Empty(effect.GetSuccessEffects());
        Assert.False(effect.IsInSuccessEffects(1));
        Assert.Null(effect.EffectInPos(2));
    }

    [Fact]
    public void TheViewFollowsTheMap()
    {
        Effect effect = WithSuccessAt(1);
        ICollection<EffectTemplate> view = effect.GetSuccessEffects();

        var later = new RootEffect { Position = 3 };
        effect.AddSuccessEffect(later);

        Assert.Equal(2, view.Count);
        Assert.Contains(later, view);
    }

    [Fact]
    public void RemoveOnTheViewTakesTheEntryOut()
    {
        Effect effect = WithSuccessAt(1, 2);
        EffectTemplate second = effect.EffectInPos(2);

        Assert.True(effect.GetSuccessEffects().Remove(second));

        Assert.True(effect.IsInSuccessEffects(1));
        Assert.False(effect.IsInSuccessEffects(2));
    }

    private static Effect WithSuccessAt(params int[] positions)
    {
        var effect = new Effect(null!, null!, new SkillTemplate(), 1);
        foreach (int position in positions)
            effect.AddSuccessEffect(new RootEffect { Position = position });
        return effect;
    }
}
