using Random = UnityEngine.Random;
using UnityEngine;
using MonoMod.Cil;
using static Mono.Cecil.Cil.OpCodes;
using RWCustom;

namespace ScutigeraCreature;

sealed class Scutigera
{
    internal Scutigera()
    {
        On.Centipede.GenerateSize += (orig, abstrCrit) => abstrCrit.creatureTemplate.type == EnumExt_Scutigera.Scutigera ? 1f : orig(abstrCrit);
        IL.Centipede.ctor += il =>
        {
            ILCursor c = new(il);
            c.GotoNext(MoveType.After,
                x => x.MatchLdarg(1),
                x => x.MatchLdfld<AbstractCreature>("state"),
                x => x.MatchLdcR4(2.3f),
                x => x.MatchLdcR4(7f),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<Centipede>("size"),
                x => x.MatchCall<Mathf>("Lerp"),
                x => x.MatchCall<Mathf>("RoundToInt"),
                x => x.MatchStfld<CreatureState>("meatLeft"));
            c.Emit(Ldarg_0);
            c.Emit(Ldarg_1);
            c.EmitDelegate((Centipede self, AbstractCreature abstractCreature) =>
            {
                if (self.Scutigera()) abstractCreature.state.meatLeft = 5;
            });
        };
        On.Centipede.ctor += (orig, self, abstractCreature, world) =>
        {
            orig(self, abstractCreature, world);
            if (self.Scutigera())
            {
                for (int i = 0; i < self.bodyChunks.Length; i++)
                {
                    var num = (float)i / (self.bodyChunks.Length - 1);
                    var num2 = Mathf.Lerp(Mathf.Lerp(2f, 3.5f, self.size), Mathf.Lerp(4f, 6.5f, self.size), Mathf.Pow(Mathf.Clamp(Mathf.Sin(Mathf.PI * num), 0f, 1f), Mathf.Lerp(.7f, .3f, self.size)));
                    num2 = Mathf.Lerp(num2, Mathf.Lerp(2f, 3.5f, self.size), .4f);
                    self.bodyChunks[i].rad = num2;
                }
                int num3 = 0;
                for (int l = 0; l < self.bodyChunks.Length; l++)
                {
                    for (int m = l + 1; m < self.bodyChunks.Length; m++)
                    {
                        self.bodyChunkConnections[num3].distance = self.bodyChunks[l].rad + self.bodyChunks[m].rad;
                        num3++;
                    }
                }
            }
        };
        On.Centipede.SpearStick += (orig, self, source, dmg, chunk, appPos, direction) =>
        {
            if (self.Scutigera() && Random.value < .25f && chunk is not null && chunk.index >= 0 && chunk.index < self.CentiState.shells.Length && (chunk.index == self.shellJustFellOff || self.CentiState.shells[chunk.index]))
            {
                if (chunk.index == self.shellJustFellOff) self.shellJustFellOff = -1;
                return false;
            }
            return orig(self, source, dmg, chunk, appPos, direction);
        };
        IL.Centipede.Violence += il =>
        {
            ILCursor c = new(il);
            c.GotoNext(
                x => x.MatchNewobj<CentipedeShell>());
            c.Next.Operand = typeof(ScutigeraShell).GetConstructor(new[] { typeof(Vector2), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(float), typeof(bool) });
            c.Emit(Ldarg_0);
            c.Emit(Call, typeof(ScutigeraExtensions).GetMethod("Scutigera"));
            foreach (var i in il.Instrs)
            {
                if (i.MatchCall<Centipede>("get_Red")) i.Operand = typeof(ScutigeraExtensions).GetMethod("ScutOrRed");
            }
            c.GotoNext(MoveType.After,
                x => x.MatchCall<Creature>("Violence"));
            c.Emit(Ldarg_0);
            c.EmitDelegate((Centipede self) =>
            {
                if (self.Scutigera()) self.stun = 0;
            });
        };
        IL.Centipede.Crawl += il =>
        {
            ILCursor c = new(il);
            c.GotoNext(MoveType.After,
                x => x.MatchCallvirt<HealthState>("get_ClampedHealth"),
                x => x.MatchMul(),
                x => x.MatchCall<Mathf>("Lerp"),
                x => x.MatchCall<Vector2>("op_Multiply"),
                x => x.MatchLdarg(0));
            if (c.Next.MatchCall<Centipede>("get_Red")) c.Next.Operand = typeof(ScutigeraExtensions).GetMethod("ScutOrRed");
        };
        On.Centipede.ShortCutColor += (orig, self) => self.Scutigera() ? Custom.HSL2RGB(Mathf.Lerp(.1527777777777778f, .1861111111111111f, .5f), Mathf.Lerp(.294f, .339f, .5f), .5f) : orig(self);
        IL.Centipede.Shock += il =>
        {
            ILCursor c = new(il);
            c.GotoNext(MoveType.After,
                x => x.MatchCall(typeof(Custom).GetMethod("RNV")),
                x => x.MatchLdcR4(4f),
                x => x.MatchLdcR4(14f),
                x => x.MatchCall<Random>("get_value"),
                x => x.MatchCall<Mathf>("Lerp"),
                x => x.MatchCall<Vector2>("op_Multiply"),
                x => x.MatchLdcR4(.7f),
                x => x.MatchLdcR4(.7f),
                x => x.MatchLdcR4(1f));
            if (c.Next.MatchNewobj<Color>())
            {
                c.Next.OpCode = Call;
                c.Next.Operand = typeof(ScutigeraExtensions).GetMethod("ShockColorIfScut");
                c.Emit(Ldarg_0);
            }
            c.GotoNext(MoveType.After,
                x => x.MatchLdfld<Centipede>("size"),
                x => x.MatchCall<Mathf>("Lerp"),
                x => x.MatchLdcR4(.2f),
                x => x.MatchLdcR4(1.9f),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<Centipede>("size"),
                x => x.MatchMul(),
                x => x.MatchAdd(),
                x => x.MatchLdarg(0),
                x => x.MatchLdcR4(.7f),
                x => x.MatchLdcR4(.7f),
                x => x.MatchLdcR4(1f));
            if (c.Next.MatchNewobj<Color>())
            {
                c.Next.OpCode = Call;
                c.Next.Operand = typeof(ScutigeraExtensions).GetMethod("ShockColorIfScut");
                c.Emit(Ldarg_0);
            }
        };
    }
}

public class ScutigeraShell : CentipedeShell
{
    public bool isScutigera;

    public ScutigeraShell(Vector2 pos, Vector2 vel, float hue, float saturation, float scaleX, float scaleY, bool isScutigera) : base(pos, vel, hue, saturation, scaleX, scaleY) => this.isScutigera = isScutigera;

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);
        if (isScutigera)
        {
            sLeaser.sprites[0] = new("ScutigeraBackShell");
            sLeaser.sprites[1] = new("ScutigeraBackShell");
            AddToContainer(sLeaser, rCam, null);
        }
    }
}

public static class ScutigeraExtensions
{
    public static bool Scutigera(this Centipede self) => self.abstractCreature.creatureTemplate.type == EnumExt_Scutigera.Scutigera;

    public static bool ScutOrRed(this Centipede self) => self.abstractCreature.creatureTemplate.type == EnumExt_Scutigera.Scutigera || self.abstractCreature.creatureTemplate.type == CreatureTemplate.Type.RedCentipede;

    public static bool ScutOrWing(this Centipede self) => self.abstractCreature.creatureTemplate.type == EnumExt_Scutigera.Scutigera || self.abstractCreature.creatureTemplate.type == CreatureTemplate.Type.Centiwing;

    public static Color ShockColorIfScut(float nr, float ng, float nb, Centipede self) => self.Scutigera() ? new(nr, nb, ng) : new(nr, ng, nb);

    // When you have problems with EmitDelegate...
    public static void TweakSpiderRelationshipWithScut(BigSpiderAI self, RelationshipTracker.DynamicRelationship dRelation, ref CreatureTemplate.Relationship result)
    {
        if (self.bug is not null && dRelation?.trackerRep?.representedCreature?.creatureTemplate?.type == EnumExt_Scutigera.Scutigera && dRelation.state is BigSpiderAI.SpiderTrackState st && st.consious && st.totalMass > self.bug.TotalMass * 2f) result = new(CreatureTemplate.Relationship.Type.Afraid, Mathf.InverseLerp(self.bug.TotalMass, self.bug.TotalMass * 7f, st.totalMass));
    }
}