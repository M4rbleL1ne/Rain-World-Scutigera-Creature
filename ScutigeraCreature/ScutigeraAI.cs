﻿using MonoMod.Cil;
using UnityEngine;
using static Mono.Cecil.Cil.OpCodes;

namespace ScutigeraCreature;

sealed class ScutigeraAI
{
    internal ScutigeraAI()
    {
        On.CentipedeAI.ctor += (orig, self, creature, world) =>
        {
            orig(self, creature, world);
            if (self.centipede.Scutigera())
                self.pathFinder.stepsPerFrame = 15;
        };
        HK.On.CentipedeAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) =>
        {
            var result = orig(self, dRelation);
            if (self.centipede.Scutigera())
            {
                if (dRelation?.trackerRep?.representedCreature is not null)
                {
                    if (self.DoIWantToShockCreature(dRelation.trackerRep.representedCreature) && result.type is CreatureTemplate.Relationship.Type.Attacks or CreatureTemplate.Relationship.Type.Eats)
                    {
                        result.type = CreatureTemplate.Relationship.Type.Eats;
                        result.intensity = 1f;
                        if (self.preyTracker is not null)
                            self.preyTracker.currentPrey = new(self.preyTracker, dRelation.trackerRep);
                    }
                    else if (result.type is CreatureTemplate.Relationship.Type.Attacks or CreatureTemplate.Relationship.Type.Eats)
                    {
                        result.type = CreatureTemplate.Relationship.Type.Ignores;
                        result.intensity = 0f;
                    }
                }
            }
            return result;
        };
        On.CentipedeAI.DoIWantToShockCreature += (orig, self, critter) =>
        {
            var result = orig(self, critter);
            if (self.centipede.Scutigera())
            {
                if (critter.realizedCreature is Creature c)
                {
                    if (self.StaticRelationship(c.abstractCreature).type == CreatureTemplate.Relationship.Type.Afraid && !c.dead)
                        result = true;
                    else if (!c.dead && c.grasps is not null && c.grasps.Length > 0)
                    {
                        for (var i = 0; i < c.grasps.Length; i++)
                        {
                            if (c.grasps[i]?.grabbed is Weapon || self.centipede?.CentiState?.health < .4f)
                            {
                                result = true;
                                break;
                            }
                            else
                                result = false;
                        }
                    }
                    else if (c.dead || c.grasps is null || c.grasps.Length <= 0)
                        result = false;
                }
            }
            return result;
        };
        IL.CentipedeAI.VisualScore += il =>
        {
            ILCursor c = new(il);
            for (var i = 0; i < il.Instrs.Count; i++)
            {
                if (il.Instrs[i].MatchCallOrCallvirt<Centipede>("get_Red"))
                {
                    c.Goto(i, MoveType.After);
                    c.Emit(Ldarg_0);
                    c.EmitDelegate((bool flag, CentipedeAI self) => flag || (self.centipede is Centipede ce && ce.Template.type == EnumExt_Scutigera.Scutigera));
                }
            }
        };
        IL.CentipedeAI.Update += il =>
        {
            ILCursor c = new(il);
            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt<ArtificialIntelligence>("get_noiseTracker"),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<CentipedeAI>("centipede"),
                x => x.MatchLdfld<Centipede>("moving"),
                x => x.MatchBrfalse(out _),
                x => x.MatchLdcR4(.0f),
                x => x.MatchBr(out _),
                x => x.MatchLdcR4(1.5f),
                x => x.MatchStfld<NoiseTracker>("hearingSkill")))
            {
                c.Emit(Ldarg_0);
                c.EmitDelegate((CentipedeAI self) =>
                {
                    if (self.centipede is not null && self.centipede.Scutigera())
                        self.noiseTracker.hearingSkill = 1.5f;
                });
            }
            else
                ScutigeraPlugin.logger?.LogError("Couldn't ILHook CentipedeAI.Update! (part 1)");
            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdcR4(.1f),
                x => x.MatchCall<Mathf>("Lerp"),
                x => x.MatchStfld<CentipedeAI>("excitement"),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<CentipedeAI>("centipede"),
                x => x.MatchCallOrCallvirt<Centipede>("get_Centiwing")))
            {
                c.Emit(Ldarg_0);
                c.EmitDelegate((bool flag, CentipedeAI self) => flag || (self.centipede is Centipede ce && ce.Template.type == EnumExt_Scutigera.Scutigera));
            }
            else
                ScutigeraPlugin.logger?.LogError("Couldn't ILHook CentipedeAI.Update! (part 2)");
        };
        On.CentipedeAI.CreatureSpotted += (orig, self, firstSpot, creatureRep) =>
        {
            if (creatureRep.representedCreature?.realizedCreature is not null && !creatureRep.representedCreature.realizedCreature.dead && self.centipede?.room is not null && self.centipede.Scutigera() && !self.centipede.dead && self.DoIWantToShockCreature(creatureRep.representedCreature) && self.StaticRelationship(creatureRep.representedCreature).type is CreatureTemplate.Relationship.Type.Eats or CreatureTemplate.Relationship.Type.Attacks && self.centipede.bodyChunks is not null && self.centipede.bodyChunks.Length > 0)
            {
                for (var i = 0; i < self.centipede.bodyChunks.Length; i++)
                {
                    if (Random.value < .1f)
                        self.centipede.room.AddObject(new ScutigeraFlash(self.centipede.bodyChunks[i].pos, self.centipede.bodyChunks[i].rad / (self.centipede.bodyChunks[i].rad * 30f), self.centipede));
                }
            }
            orig(self, firstSpot, creatureRep);
        };
    }
}

public class ScutigeraFlash : ElectricDeath.SparkFlash
{
    public Centipede owner;

    public ScutigeraFlash(Vector2 pos, float size, Centipede ow) : base(pos, size) => owner = ow;

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[3]
        {
            new("Futile_White") { shader = rCam.room.game.rainWorld.Shaders["LightSource"], color = ScutigeraExtensions.ShockColorIfScut(.7f, .7f, 1f, owner) },
            new("Futile_White") { shader = rCam.room.game.rainWorld.Shaders["FlatLight"], color = ScutigeraExtensions.ShockColorIfScut(.7f, .7f, 1f, owner) },
            new("Futile_White") { shader = rCam.room.game.rainWorld.Shaders["FlareBomb"], color = ScutigeraExtensions.ShockColorIfScut(.7f, .7f, 1f, owner) }
        };
        AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Water"));
    }
}