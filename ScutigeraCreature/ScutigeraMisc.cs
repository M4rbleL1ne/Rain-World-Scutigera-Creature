using System.Text.RegularExpressions;
using System;
using MonoMod.Cil;
using static Mono.Cecil.Cil.OpCodes;
using Mono.Cecil;
using UnityEngine;
using RWCustom;

namespace ScutigeraCreature;

sealed class ScutigeraMisc
{
    static bool AddToCat = true;

    internal ScutigeraMisc()
    {
        On.Player.CanEatMeat += (orig, self, crit) => (crit.dead && crit.Template.type == EnumExt_Scutigera.Scutigera) || orig(self, crit);
        On.WorldLoader.CreatureTypeFromString += (orig, s) => Regex.IsMatch(s, "/scut(igera)?/gi") ? EnumExt_Scutigera.Scutigera : orig(s);
        On.DevInterface.MapPage.CreatureVis.CritCol += (orig, crit) => crit.creatureTemplate.type == EnumExt_Scutigera.Scutigera ? Custom.HSL2RGB(Mathf.Lerp(.1527777777777778f, .1861111111111111f, .5f), Mathf.Lerp(.294f, .339f, .5f), .5f) : orig(crit);
        On.DevInterface.MapPage.CreatureVis.CritString += (orig, crit) => crit.creatureTemplate.type == EnumExt_Scutigera.Scutigera ? "scut" : orig(crit);
        On.ArenaBehaviors.SandboxEditor.CreaturePerfEstimate += delegate (On.ArenaBehaviors.SandboxEditor.orig_CreaturePerfEstimate orig, CreatureTemplate.Type critType, ref float linear, ref float exponential)
        {
            if (critType == EnumExt_Scutigera.Scutigera)
            {
                linear += .8f;
                exponential += .5f;
            }
            else orig(critType, ref linear, ref exponential);
        };
        On.DevInterface.RoomAttractivenessPanel.ctor += (orig, self, owner, IDstring, parentNode, pos, title, mapPage) =>
        {
            orig(self, owner, IDstring, parentNode, pos, title, mapPage);
            if (AddToCat)
            {
                int newSz = self.categories[5].Length + 1;
                Array.Resize(ref self.categories[5], newSz);
                self.categories[5][newSz - 1] = StaticWorld.GetCreatureTemplate(EnumExt_Scutigera.Scutigera).index;
                AddToCat = false;
            }
        };
        On.MultiplayerUnlocks.UnlockedCritters += (orig, ID) =>
        {
            var list = orig(ID);
            if (ID is MultiplayerUnlocks.LevelUnlockID.Hidden) list.Add(EnumExt_Scutigera.Scutigera);
            return list;
        };
        IL.ShelterDoor.KillAllHostiles += il =>
        {
            ILCursor c = new(il);
            int loc1 = -1;
            ILLabel? beq1 = null;
            MethodReference? callvirt2 = null;
            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<UpdatableAndDeletable>("room"),
                x => x.MatchCallvirt<Room>("get_abstractRoom"),
                x => x.MatchLdfld<AbstractRoom>("creatures"),
                x => x.MatchLdloc(out loc1),
                x => x.MatchCallvirt(out callvirt2),
                x => x.MatchLdfld<AbstractCreature>("creatureTemplate"),
                x => x.MatchLdfld<CreatureTemplate>("type"),
                x => x.MatchLdcI4(40),
                x => x.MatchBeq(out beq1))
            && loc1 != -1 && beq1 is not null && callvirt2 is not null)
            {
                c.Emit(Ldarg_0);
                c.Emit<UpdatableAndDeletable>(Ldfld, "room");
                c.Emit<Room>(Callvirt, "get_abstractRoom");
                c.Emit<AbstractRoom>(Ldfld, "creatures");
                c.Emit(Ldloc_S, il.Body.Variables[loc1]);
                c.Emit(Callvirt, callvirt2);
                c.Emit<AbstractCreature>(Ldfld, "creatureTemplate");
                c.Emit<CreatureTemplate>(Ldfld, "type");
                c.Emit(Ldsfld, typeof(EnumExt_Scutigera).GetField("Scutigera"));
                c.Emit(Beq, beq1);
            }
            else ScutigeraPlugin.logger?.LogError("Couldn't ILHook ShelterDoor.KillAllHostiles!");
        };
        HK.IL.BigSpiderAI.IUseARelationshipTracker_UpdateDynamicRelationship += il =>
        {
            ILCursor c = new(il);
            var stloc1 = -1;
            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdarg(1),
                x => x.MatchLdfld<RelationshipTracker.DynamicRelationship>("trackerRep"),
                x => x.MatchLdfld<Tracker.CreatureRepresentation>("representedCreature"),
                x => x.MatchCall<ArtificialIntelligence>("StaticRelationship"),
                x => x.MatchStloc(out stloc1))
            && stloc1 != -1)
            {
                c.Emit(Ldarg_0);
                c.Emit(Ldarg_1);
                c.Emit(Ldloca_S, il.Body.Variables[stloc1]);
                c.Emit(Call, typeof(ScutigeraExtensions).GetMethod("TweakSpiderRelationshipWithScut"));
            }
            else ScutigeraPlugin.logger?.LogError("Couldn't ILHook BigSpiderAI.IUseARelationshipTracker_UpdateDynamicRelationship!");
        };
    }
}