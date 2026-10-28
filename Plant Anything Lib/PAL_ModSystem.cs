using PlantAnythingLib.config;
using PlantAnythingLib.handler;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace PlantAnythingLib
{
    public class PAL_ModSystem : ModSystem
    {
        public override void StartPre(ICoreAPI api)
        {
            VarRef.API ??= api;
            switch (api.Side)
            {
                case (EnumAppSide.Server): { VarRef.API_Server = (ICoreServerAPI)api; break; }
                case (EnumAppSide.Client): { VarRef.API_Client = (ICoreClientAPI)api; break; }
                default: { break; }
            }
        }

        public override void Start(ICoreAPI api)
        {
            api.RegisterCollectibleBehaviorClass(nameof(PAL_Handle_Plant.Behavior_Plantable), typeof(PAL_Handle_Plant.Behavior_Plantable));
        }

    }
}
