using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;
using EnderLibrary;

namespace PlantAnythingLib.handler
{
    /*public class ItemProps_Plantable() //maybe come back and make this work with proper json deserialization
    {
        public string? plantOn;
        public string? plantAs;

        public ItemProps_Plantable Clone()
        {
            return new ItemProps_Plantable
            {
                plantOn = plantOn,
                plantAs = plantAs
            };
        }
    }*/

    internal class PAL_Handle_Plant
    {
        private static ICoreAPI API = VarRef.API;
        private static ICoreClientAPI API_Client = VarRef.API_Client;
        private static ICoreServerAPI API_Server = VarRef.API_Server;

        //public ItemProps_Plantable itemProps = { get; protected set; }


        public class Behavior_Plantable(CollectibleObject collObj) : CollectibleBehavior(collObj)
        {
            private string[] plantOn = [];
            private Block? plantAs;


            public override void Initialize(JsonObject properties)
            {
                if (properties["plantAs"].Exists)
                {
                    if (properties["plantOn"].Exists)
                    {
                        List<string> plantOnList = [];
                        foreach (var i in properties["plantOn"])
                        {
                            if (i is null || i.Count < 1) { continue; }
                            plantOnList.Add(i.ToString());
                        }
                        plantOn = [.. plantOnList];
                    }

                    plantAs = API.World.GetBlock(new AssetLocation(properties["plantAs"].ToString()));
                }
                Function_General.Log_Debug_Verbose("Found Behavior_Plantable on item: {0}; plantOn: list[{1}], plantAs: {2}", loggers: [collObj.Code, plantOn?.Length ?? 0, plantAs?.Code.GetNameWithDomain() ?? "null"]);
                base.Initialize(properties);
            }

            public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
            {
                if (byEntity is EntityPlayer && byEntity.Controls.Sneak) { base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handHandling, ref handling); return; } //bypass interaction when crouching to allow other ineraction systems
                Function_General.Log_Debug_Verbose("Ran with item: {0} on selected block: {1}", loggers: [slot?.Itemstack?.GetName() ?? "null", blockSel?.Block?.Code?.Path?.ToString() ?? "null"]);
                if (plantAs is null || blockSel is null || slot is null) { base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handHandling, ref handling); ; return; }


                bool isListEmpty = (plantOn is null || plantOn.Length < 1);
                if (isListEmpty && blockSel.Block is BlockFarmland) { PAL_Plant(slot, byEntity, blockSel, entitySel, firstEvent, ref handHandling, ref handling); }
                if (!isListEmpty && Function_General.Match_Wildcard(plantOn!, blockSel.Block)) { PAL_Plant(slot, byEntity, blockSel, entitySel, firstEvent, ref handHandling, ref handling); }


                base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handHandling, ref handling);
            }


            /// <summary>
            /// slightly modified vanilla BlockEntityFarmland.TryPlant code to not explicitly require a seed item or item cropbehavior or farmland block target.
            /// </summary>
            private void PAL_Plant(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
            {
                if (plantAs is null || blockSel is null || byEntity is null) { return; }

                BlockPos abvBlockPos = blockSel.Position.UpCopy();
                if (API.World.BlockAccessor.GetBlock(abvBlockPos) is not null && API.World.BlockAccessor.GetBlock(abvBlockPos).BlockMaterial == EnumBlockMaterial.Air)
                {
                    if (plantAs.CollisionBoxes is not null && plantAs.CollisionBoxes.Length != 0 && API.World.GetIntersectingEntities(abvBlockPos, plantAs.CollisionBoxes, (Entity e) => e.IsInteractable).Length != 0) { return; }
                    API.World.BlockAccessor.SetBlock(plantAs.BlockId, abvBlockPos);
                    if (plantAs.CropProps is not null)
                    {
                        CropBehavior[] cropBehaviors = plantAs.CropProps.Behaviors;
                        for (int i = 0; i < cropBehaviors.Length; i++)
                        {
                            cropBehaviors[i].OnPlanted(API, slot, byEntity, blockSel);
                        }
                    }
                    //modified VintageStory.GameContent.ItemPlantableSeed
                    IPlayer? player = null;
                    IPlayer? serverPlayer = null;

                    if (byEntity is EntityPlayer entityPlayer)
                    {
                        player = byEntity.World.PlayerByUid(entityPlayer.PlayerUID);
                        serverPlayer = API_Server.World.PlayerByUid(entityPlayer.PlayerUID);
                    }

                    byEntity.World.PlaySoundAt(new AssetLocation("game:sounds/block/plant"), abvBlockPos, 0.4375, player);
                    ((byEntity as EntityPlayer)?.Player as IClientPlayer)?.TriggerFpAnimation(EnumHandInteract.HeldItemInteract); //whats the point of this animation, it looks exactly the same with or without
                    if (serverPlayer is null || serverPlayer.WorldData?.CurrentGameMode != EnumGameMode.Creative)
                    {
                        ItemSlot serverSlot = serverPlayer!.InventoryManager.ActiveHotbarSlot; //for some reason i have to do this to get the server to TakeOut() instead of the client despite VS using the normal slot that is passed through... if anyone knows why id appreciate it.

                        serverSlot.TakeOut(1);
                        serverSlot.MarkDirty();
                    }

                    handling = EnumHandling.Handled;
                }
            }


            public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
            {
                base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

                if (plantAs is null) { return; }
                dsc.AppendLine("PAL Plantable: See handbook for info");
            }
        }
    }
}
