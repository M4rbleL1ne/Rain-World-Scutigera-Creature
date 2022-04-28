using MonoMod.RuntimeDetour.HookGen;
using MonoMod.Cil;
using static System.Reflection.BindingFlags;
using System.Reflection;
using System.ComponentModel;
using UnityEngine;

namespace HK
{
    public static class Infos
    {
        public static MethodInfo? BSAIUDR = typeof(BigSpiderAI).GetMethod("IUseARelationshipTracker.UpdateDynamicRelationship", Public | NonPublic | Instance | Static);

        public static MethodInfo? CAIUDR = typeof(CentipedeAI).GetMethod("IUseARelationshipTracker.UpdateDynamicRelationship", Public | NonPublic | Instance | Static);

        public static MethodInfo? LAFER = typeof(Fisobs.Core.Ext).GetMethod("LoadAtlasFromEmbRes", Public | NonPublic | Instance | Static);

        public static MethodInfo? gSSC = typeof(CentipedeGraphics).GetMethod("get_SecondaryShellColor", Public | NonPublic | Instance | Static);

        internal static void Dispose()
        {
            BSAIUDR = default;
            CAIUDR = default;
            LAFER = default;
            gSSC = default;
        }
    }

    namespace IL
    {
        public static class BigSpiderAI
        {
            public static event ILContext.Manipulator IUseARelationshipTracker_UpdateDynamicRelationship
            {
                add => HookEndpointManager.Modify<On.BigSpiderAI.hook_IUseARelationshipTracker_UpdateDynamicRelationship>(Infos.BSAIUDR, value);
                remove => HookEndpointManager.Unmodify<On.BigSpiderAI.hook_IUseARelationshipTracker_UpdateDynamicRelationship>(Infos.BSAIUDR, value);
            }
        }
    }

    namespace On
    {
        public static class BigSpiderAI
        {
            [EditorBrowsable(EditorBrowsableState.Never)]
            public delegate CreatureTemplate.Relationship orig_IUseARelationshipTracker_UpdateDynamicRelationship(global::BigSpiderAI self, RelationshipTracker.DynamicRelationship dRelation);

            [EditorBrowsable(EditorBrowsableState.Never)]
            public delegate CreatureTemplate.Relationship hook_IUseARelationshipTracker_UpdateDynamicRelationship(orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, global::BigSpiderAI self, RelationshipTracker.DynamicRelationship dRelation);

            public static event hook_IUseARelationshipTracker_UpdateDynamicRelationship IUseARelationshipTracker_UpdateDynamicRelationship
            {
                add => HookEndpointManager.Add<hook_IUseARelationshipTracker_UpdateDynamicRelationship>(Infos.BSAIUDR, value);
                remove => HookEndpointManager.Remove<hook_IUseARelationshipTracker_UpdateDynamicRelationship>(Infos.BSAIUDR, value);
            }
        }

        public static class CentipedeAI
        {
            [EditorBrowsable(EditorBrowsableState.Never)]
            public delegate CreatureTemplate.Relationship orig_IUseARelationshipTracker_UpdateDynamicRelationship(global::CentipedeAI self, RelationshipTracker.DynamicRelationship dRelation);

            [EditorBrowsable(EditorBrowsableState.Never)]
            public delegate CreatureTemplate.Relationship hook_IUseARelationshipTracker_UpdateDynamicRelationship(orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, global::CentipedeAI self, RelationshipTracker.DynamicRelationship dRelation);

            public static event hook_IUseARelationshipTracker_UpdateDynamicRelationship IUseARelationshipTracker_UpdateDynamicRelationship
            {
                add => HookEndpointManager.Add<hook_IUseARelationshipTracker_UpdateDynamicRelationship>(Infos.CAIUDR, value);
                remove => HookEndpointManager.Remove<hook_IUseARelationshipTracker_UpdateDynamicRelationship>(Infos.CAIUDR, value);
            }
        }

        public static class CentipedeGraphics
        {
            [EditorBrowsable(EditorBrowsableState.Never)]
            public delegate Color orig_get_SecondaryShellColor(global::CentipedeGraphics self);

            [EditorBrowsable(EditorBrowsableState.Never)]
            public delegate Color hook_get_SecondaryShellColor(orig_get_SecondaryShellColor orig, global::CentipedeGraphics self);

            public static event hook_get_SecondaryShellColor get_SecondaryShellColor
            {
                add => HookEndpointManager.Add<hook_get_SecondaryShellColor>(Infos.gSSC, value);
                remove => HookEndpointManager.Remove<hook_get_SecondaryShellColor>(Infos.gSSC, value);
            }
        }

        namespace Fisobs.Core
        {
            public static class Ext
            {
                [EditorBrowsable(EditorBrowsableState.Never)]
                public delegate FAtlas? orig_LoadAtlasFromEmbRes(Assembly assembly, string resource);

                [EditorBrowsable(EditorBrowsableState.Never)]
                public delegate FAtlas? hook_LoadAtlasFromEmbRes(orig_LoadAtlasFromEmbRes orig, Assembly assembly, string resource);

                public static event hook_LoadAtlasFromEmbRes LoadAtlasFromEmbRes
                {
                    add => HookEndpointManager.Add<hook_LoadAtlasFromEmbRes>(Infos.LAFER, value);
                    remove => HookEndpointManager.Remove<hook_LoadAtlasFromEmbRes>(Infos.LAFER, value);
                }
            }
        }
    }
}