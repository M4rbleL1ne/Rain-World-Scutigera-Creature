using UnityEngine;
using RWCustom;
using MonoMod.Cil;
using static Mono.Cecil.Cil.OpCodes;

namespace ScutigeraCreature;

sealed class ScutigeraGraphics
{
    internal ScutigeraGraphics()
    {
        On.CentipedeGraphics.ctor += (orig, self, ow) =>
        {
            orig(self, ow);
            if (self.centipede.Scutigera())
            {
                self.hue = Mathf.Lerp(.1527777777777778f, .1861111111111111f, Random.value);
                self.saturation = Mathf.Lerp(.294f, .339f, Random.value);
                self.wingPairs = self.centipede.bodyChunks.Length;
                self.wingLengths = new float[self.totSegs];
                for (int j = 0; j < self.totSegs; j++)
                {
                    var num = (float)j / (self.totSegs - 1);
                    var num2 = Mathf.Sin(Mathf.Pow(Mathf.InverseLerp(.5f, 0f, num), .75f) * Mathf.PI);
                    num2 *= 1f - num;
                    var num3 = Mathf.Sin(Mathf.Pow(Mathf.InverseLerp(1f, .5f, num), .75f) * Mathf.PI);
                    num3 *= num;
                    num2 = .5f + .5f * num2;
                    num3 = .5f + .5f * num3;
                    self.wingLengths[j] = Mathf.Lerp(3f, Custom.LerpMap(self.centipede.size, .5f, 1f, 60f, 80f), Mathf.Max(num2, num3) - Mathf.Sin(num * Mathf.PI) * .25f);
                }
            }
        };
        IL.CentipedeGraphics.InitiateSprites += il =>
        {
            ILCursor c = new(il);
            c.GotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<CentipedeGraphics>("centipede"),
                x => x.MatchCallvirt<Centipede>("get_Red"),
                x => x.MatchBrfalse(out _),
                x => x.MatchLdcI4(0),
                x => x.MatchStloc(7),
                x => x.MatchBr(out _));
            c.Emit(Ldarg_0);
            c.Emit(Ldarg_1);
            c.EmitDelegate((CentipedeGraphics self, RoomCamera.SpriteLeaser sLeaser) =>
            {
                if (self.centipede.Scutigera())
                {
                    for (int l = 0; l < 2; l++)
                    {
                        for (int num = 0; num < self.wingPairs; num++) sLeaser.sprites[self.WingSprite(l, num)] = new CustomFSprite("ScutigeraWing");
                    }
                    for (int i = 0; i < self.owner.bodyChunks.Length; i++)
                    {
                        sLeaser.sprites[self.SegmentSprite(i)] = new("ScutigeraSegment") { scaleY = self.owner.bodyChunks[i].rad * 1.8f * (1f / 12f) };
                        sLeaser.sprites[self.SegmentSprite(i)].element.atlas.texture.anisoLevel = 1;
                        sLeaser.sprites[self.SegmentSprite(i)].element.atlas.texture.filterMode = 0;
                        for (int j = 0; j < 2; j++) sLeaser.sprites[self.LegSprite(i, j, 1)] = new VertexColorSprite("ScutigeraLegB");
                    }
                }
            });
        };
        On.CentipedeGraphics.DrawSprites += (orig, self, sLeaser, rCam, timeStacker, camPos) =>
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            if (self.centipede.Scutigera())
            {
                for (int i = 0; i < self.owner.bodyChunks.Length; i++)
                {
                    if (sLeaser.sprites[self.ShellSprite(i)].element.name is "CentipedeBackShell") sLeaser.sprites[self.ShellSprite(i)].element = Futile.atlasManager.GetElementWithName("ScutigeraBackShell");
                    else if (sLeaser.sprites[self.ShellSprite(i)].element.name is "CentipedeBellyShell") sLeaser.sprites[self.ShellSprite(i)].element = Futile.atlasManager.GetElementWithName("ScutigeraBellyShell");
                }
                for (int k = 0; k < 2; k++)
                {
                    for (int num15 = 0; num15 < self.wingPairs; num15++)
                    {
                        if (sLeaser.sprites[self.WingSprite(k, num15)] is CustomFSprite cSpr)
                        {
                            var vector1 = (num15 != 0) ? Custom.DirVec(self.ChunkDrawPos(num15 - 1, timeStacker), self.ChunkDrawPos(num15, timeStacker)) : Custom.DirVec(self.ChunkDrawPos(0, timeStacker), self.ChunkDrawPos(1, timeStacker));
                            var vector2 = Custom.PerpendicularVector(vector1);
                            var vector3 = self.RotatAtChunk(num15, timeStacker);
                            var vector4 = self.WingPos(k, num15, vector1, vector2, vector3, timeStacker);
                            var vector5 = self.ChunkDrawPos(num15, timeStacker) + self.centipede.bodyChunks[num15].rad * ((k != 0) ? 1f : -1f) * vector2 * vector3.y;
                            cSpr.MoveVertice(1, vector4 + vector1 * 2f - camPos);
                            cSpr.MoveVertice(0, vector4 - vector1 * 2f - camPos);
                            cSpr.MoveVertice(2, vector5 + vector1 * 2f - camPos);
                            cSpr.MoveVertice(3, vector5 - vector1 * 2f - camPos);
                            cSpr.verticeColors[0] = self.SecondaryShellColor;
                            cSpr.verticeColors[1] = self.SecondaryShellColor;
                            cSpr.verticeColors[2] = self.blackColor;
                            cSpr.verticeColors[3] = self.blackColor;
                        }
                    }
                }
            }
        };
        On.CentipedeGraphics.WhiskerLength += (orig, self, part) => self.centipede.Scutigera() ? ((part != 0) ? 48f : 44f) : orig(self, part);
        IL.CentipedeGraphics.Update += il =>
        {
            ILCursor c = new(il);
            int loc = -1;
            c.GotoNext(MoveType.After,
                x => x.MatchSub(),
                x => x.MatchMul(),
                x => x.MatchCall<Mathf>("Lerp"),
                x => x.MatchStloc(out loc),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<CentipedeGraphics>("lightSource"),
                x => x.MatchLdloc(loc),
                x => x.MatchLdloc(loc),
                x => x.MatchLdcR4(1f));
            if (c.Next.MatchNewobj<Color>())
            {
                c.Next.OpCode = Call;
                c.Next.Operand = typeof(ScutigeraExtensions).GetMethod("ShockColorIfScut");
                c.Emit(Ldarg_0);
                c.Emit<CentipedeGraphics>(Ldfld, "centipede");
            }
        };
        HK.On.CentipedeGraphics.get_SecondaryShellColor += (orig, self) =>
        {
            var res = orig(self);
            if (self.centipede is not null && self.centipede.Scutigera())
            {
                int seed = Random.seed;
                Random.seed = self.centipede.abstractCreature.ID.RandomSeed;
                res = Color.Lerp(res, new(res.r + .2f, res.g + .2f, res.b + .2f), Mathf.Lerp(.1f, .2f, Random.value));
                Random.seed = seed;
            }
            return res;
        };
    }
}