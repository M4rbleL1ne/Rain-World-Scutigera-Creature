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
            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdarg(1),
                x => x.MatchLdfld<AbstractCreature>("state"),
                x => x.MatchLdcR4(2.3f),
                x => x.MatchLdcR4(7f),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<Centipede>("size"),
                x => x.MatchCall<Mathf>("Lerp"),
                x => x.MatchCall<Mathf>("RoundToInt"),
                x => x.MatchStfld<CreatureState>("meatLeft")))
            {
                c.Emit(Ldarg_0);
                c.Emit(Ldarg_1);
                c.EmitDelegate((Centipede self, AbstractCreature abstractCreature) =>
                {
                    if (self.Scutigera())
                        abstractCreature.state.meatLeft = 5;
                });
            }
            else
                ScutigeraPlugin.logger?.LogError("Couldn't ILHook Centipede.ctor!");
        };
        On.Centipede.ctor += (orig, self, abstractCreature, world) =>
        {
            orig(self, abstractCreature, world);
            if (self.Scutigera())
            {
                for (var i = 0; i < self.bodyChunks.Length; i++)
                {
                    var num = (float)i / (self.bodyChunks.Length - 1);
                    var num2 = Mathf.Lerp(Mathf.Lerp(2f, 3.5f, self.size), Mathf.Lerp(4f, 6.5f, self.size), Mathf.Pow(Mathf.Clamp(Mathf.Sin(Mathf.PI * num), 0f, 1f), Mathf.Lerp(.7f, .3f, self.size)));
                    num2 = Mathf.Lerp(num2, Mathf.Lerp(2f, 3.5f, self.size), .4f);
                    self.bodyChunks[i].rad = num2;
                }
                var num3 = 0;
                for (var l = 0; l < self.bodyChunks.Length; l++)
                {
                    for (var m = l + 1; m < self.bodyChunks.Length; m++)
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
                if (chunk.index == self.shellJustFellOff)
                    self.shellJustFellOff = -1;
                return false;
            }
            return orig(self, source, dmg, chunk, appPos, direction);
        };
        IL.Centipede.Violence += il =>
        {
            ILCursor c = new(il);
            if (c.TryGotoNext(MoveType.After,
                x => x.MatchNewobj<CentipedeShell>()))
            {
                c.Emit(Ldarg_0);
                c.EmitDelegate((CentipedeShell shell, Centipede self) => self.Template.type == EnumExt_Scutigera.Scutigera ? new ScutigeraShell(shell.pos, shell.vel, shell.hue, shell.saturation, shell.scaleX, shell.scaleY) : shell);
            }
            else
                ScutigeraPlugin.logger?.LogError("Couldn't ILHook Centipede.Violence! (part 1)");
            if (c.TryGotoNext(MoveType.After,
                x => x.MatchCall<Creature>("Violence")))
            {
                c.Emit(Ldarg_0);
                c.EmitDelegate((Centipede self) =>
                {
                    if (self.Scutigera())
                        self.stun = 0;
                });
            }
            else
                ScutigeraPlugin.logger?.LogError("Couldn't ILHook Centipede.Violence! (part 2)");
            for (var i = 0; i < il.Instrs.Count; i++)
            {
                if (il.Instrs[i].MatchCallOrCallvirt<Centipede>("get_Red"))
                {
                    c.Goto(i, MoveType.After);
                    c.Emit(Ldarg_0);
                    c.EmitDelegate((bool flag, Centipede self) => flag || self.Template.type == EnumExt_Scutigera.Scutigera);
                }
            }
        };
        IL.Centipede.Crawl += il =>
        {
            ILCursor c = new(il);
            if (c.TryGotoNext(MoveType.After,
                x => x.MatchCallOrCallvirt<HealthState>("get_ClampedHealth"),
                x => x.MatchMul(),
                x => x.MatchCall<Mathf>("Lerp"),
                x => x.MatchCall<Vector2>("op_Multiply"),
                x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt<Centipede>("get_Red")))
            {
                c.Emit(Ldarg_0);
                c.EmitDelegate((bool flag, Centipede self) => flag || self.Template.type == EnumExt_Scutigera.Scutigera);
            }
            else
                ScutigeraPlugin.logger?.LogError("Couldn't ILHook Centipede.Crawl!");
        };
        On.Centipede.ShortCutColor += (orig, self) => self.Scutigera() ? Custom.HSL2RGB(Mathf.Lerp(.1527777777777778f, .1861111111111111f, .5f), Mathf.Lerp(.294f, .339f, .5f), .5f) : orig(self);
        IL.Centipede.Shock += il =>
        {
            ILCursor c = new(il);
            if (c.TryGotoNext(MoveType.After,
                x => x.MatchCall(typeof(Custom).GetMethod("RNV")),
                x => x.MatchLdcR4(4f),
                x => x.MatchLdcR4(14f),
                x => x.MatchCall<Random>("get_value"),
                x => x.MatchCall<Mathf>("Lerp"),
                x => x.MatchCall<Vector2>("op_Multiply"),
                x => x.MatchLdcR4(.7f),
                x => x.MatchLdcR4(.7f),
                x => x.MatchLdcR4(1f),
                x => x.MatchNewobj<Color>()))
            {
                c.Emit(Ldarg_0);
                c.EmitDelegate(Color (Color color, Centipede self) => self.Scutigera() ? new(color.r, color.b, color.g) : color);
            }
            else
                ScutigeraPlugin.logger?.LogError("Couldn't ILHook Centipede.Shock! (part 1)");
            if (c.TryGotoNext(MoveType.After,
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
                x => x.MatchLdcR4(1f),
                x => x.MatchNewobj<Color>()))
            {
                c.Emit(Ldarg_0);
                c.EmitDelegate(Color (Color color, Centipede self) => self.Scutigera() ? new(color.r, color.b, color.g) : color);
            }
            else
                ScutigeraPlugin.logger?.LogError("Couldn't ILHook Centipede.Shock! (part 2)");
        };
    }
}

public class ScutigeraShell : CentipedeShell
{
    public ScutigeraShell(Vector2 pos, Vector2 vel, float hue, float saturation, float scaleX, float scaleY) : base(pos, vel, hue, saturation, scaleX, scaleY) { }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);
        sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName("ScutigeraBackShell");
        sLeaser.sprites[1].element = Futile.atlasManager.GetElementWithName("ScutigeraBackShell");
    }
}

public static class ScutigeraExtensions
{
    public static bool Scutigera(this Centipede self) => self.abstractCreature.creatureTemplate.type == EnumExt_Scutigera.Scutigera;

    public static Color ShockColorIfScut(float nr, float ng, float nb, Centipede self) => self.Scutigera() ? new(nr, nb, ng) : new(nr, ng, nb);
}