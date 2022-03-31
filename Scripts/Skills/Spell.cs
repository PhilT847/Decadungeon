public abstract class Spell : Ability
{
    public int MP_Cost;

    public abstract void CastSpell(Unit caster, Unit target);
}
