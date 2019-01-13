using System;
using System.Collections.Generic;
using NitroxModel.Logger;
using NitroxServer.Serialization.World;
using NitroxServer.ConfigParser;
using NitroxModel.DataStructures.Util;
using System.IO;
using NitroxServer.Serialization;
using NitroxServer.GameLogic.Entities;
using NitroxServer.GameLogic.Items;
using NitroxServer.GameLogic.Vehicles;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;
using NitroxServer.GameLogic.Entities.Spawning;
using NitroxServer.GameLogic.Players;
using NitroxServer.GameLogic.Unlockables;
using NitroxModel;


namespace NitroxSaveFilePatcher
{


    class Patcher
    {
        private readonly ServerProtobufSerializer serializer = new ServerProtobufSerializer();
        //private readonly World world;
        //private readonly WorldPersistence worldPersistence;
        private readonly string fileName = @"save.nitrox";
        private readonly string fileName_backup = @"save_backup.nitrox";
        private readonly ServerConfig config;
       // public bool Upgraded = false; 
        //public WorldPersistence(ServerConfig config)
        //{
        //     this.config = config;
        //}
        public bool Patching()
        {
            World world = LoadFromFile();

            if(Program.UpgradeWorld == true)
            {

                Save(world);
                return true;
            }
            
           

            return false;
        }
        public void Save(World world)
        {
            //Log.Info("Saving world state.");

            try
            {
                PersistedWorldData persistedData = new PersistedWorldData();
                persistedData.ParsedBatchCells = world.BatchEntitySpawner.SerializableParsedBatches;
                persistedData.ServerStartTime = world.TimeKeeper.ServerStartTime;
                persistedData.EntityData = world.EntityData;
                persistedData.BaseData = world.BaseData;
                persistedData.VehicleData = world.VehicleData;
                persistedData.InventoryData = world.InventoryData;
                persistedData.PlayerData = world.PlayerData;
                persistedData.GameData = world.GameData;
                persistedData.EscapePodData = world.EscapePodData;

                using (Stream stream = File.OpenWrite(fileName))
                {
                    serializer.Serialize(stream, persistedData);
                }

                Log.Info("World state saved.");
            }
            catch (Exception ex)
            {
                Log.Info("Could not save world: " + ex);
            }
        }
        public void SaveBackup(World world)
        {
            //Log.Info("Saving world state.");

            try
            {
                PersistedWorldData persistedData = new PersistedWorldData();
                persistedData.ParsedBatchCells = world.BatchEntitySpawner.SerializableParsedBatches;
                persistedData.ServerStartTime = world.TimeKeeper.ServerStartTime;
                persistedData.EntityData = world.EntityData;
                persistedData.BaseData = world.BaseData;
                persistedData.VehicleData = world.VehicleData;
                persistedData.InventoryData = world.InventoryData;
                persistedData.PlayerData = world.PlayerData;
                persistedData.GameData = world.GameData;
                persistedData.EscapePodData = world.EscapePodData;

                using (Stream stream = File.OpenWrite(fileName_backup))
                {
                    serializer.Serialize(stream, persistedData);
                }

                Log.Info("World state saved.");
            }
            catch (Exception ex)
            {
                Log.Info("Could not save world: " + ex);
            }
        }

        public World LoadFromFile()
        {
            World world;
            try
            {
                PersistedWorldData persistedData;

                using (Stream stream = File.OpenRead(fileName))
                {
                    persistedData = serializer.Deserialize<PersistedWorldData>(stream);
                }

                if (persistedData == null || !persistedData.IsValid())
                {
                    throw new InvalidDataException("Persisted state is not valid");
                }

                // IF SaveFile Version 4 CreateWorld with new EscapePodData and Set Version to 5
                if (persistedData.version == 4)
                {
                    world = CreateWorld(persistedData.ServerStartTime,
                                          persistedData.EntityData,
                                          persistedData.BaseData,
                                          persistedData.VehicleData,
                                          persistedData.InventoryData,
                                          persistedData.PlayerData,
                                          persistedData.GameData,
                                          persistedData.ParsedBatchCells,
                                          new EscapePodData(),
                                          config.GameMode);

                    persistedData.version = 5;
                    
                    Program.UpgradeWorld = true;
                    //Log.Info("Save file Version 4 Upgradet to Version 5, EXPERIMENTAL!!");
                }
                else
                {
                    world = CreateWorld(persistedData.ServerStartTime,
                                        persistedData.EntityData,
                                        persistedData.BaseData,
                                        persistedData.VehicleData,
                                        persistedData.InventoryData,
                                        persistedData.PlayerData,
                                        persistedData.GameData,
                                        persistedData.ParsedBatchCells,
                                        persistedData.EscapePodData,
                                        config.GameMode);
                }

                return world;
            }
            catch (FileNotFoundException ex)
            {
                Log.Info("No previous save file found - creating a new one.");
            }
            catch (Exception ex)
            {
                Log.Info("Could not load world: " + ex.ToString() + " creating a new one.");

                Program.FreshWorld = true;
                return CreateFreshWorld();
            }

            return null;
        }

        //public World Load()
        //{
        //    World fileLoadedWorld = LoadFromFile();

        //    if (fileLoadedWorld.IsPresent())
        //    {
        //        return fileLoadedWorld.Get();
        //    }

        //    return CreateFreshWorld();
        //}

        private World CreateFreshWorld()
        {
            World world = CreateWorld(DateTime.Now, new EntityData(), new BaseData(), new VehicleData(), new InventoryData(), new PlayerData(), new GameData() { PDAState = new PDAStateData() }, new List<Int3>(), new EscapePodData(), GameModeOption.Survival);
            return world;
        }

        private World CreateWorld(DateTime serverStartTime,
                                  EntityData entityData,
                                  NitroxServer.GameLogic.Bases.BaseData baseData,
                                  VehicleData vehicleData,
                                  InventoryData inventoryData,
                                  PlayerData playerData,
                                  GameData gameData,
                                  List<Int3> parsedBatchCells,
                                  EscapePodData escapePodData,
                                  GameModeOption gameMode)
        {
            World world = new World();
            world.TimeKeeper = new TimeKeeper();
            world.TimeKeeper.ServerStartTime = serverStartTime;

            world.SimulationOwnershipData = new SimulationOwnershipData();
            world.PlayerManager = new PlayerManager(playerData);
            world.EntityData = entityData;
            world.EventTriggerer = new EventTriggerer(world.PlayerManager);
            world.BaseData = baseData;
            world.VehicleData = vehicleData;
            world.InventoryData = inventoryData;
            world.PlayerData = playerData;
            world.GameData = gameData;
            world.EscapePodData = escapePodData;
            world.EscapePodManager = new EscapePodManager(escapePodData);
            world.EntitySimulation = new EntitySimulation(world.EntityData, world.SimulationOwnershipData, world.PlayerManager);
            world.GameMode = gameMode;

            ResourceAssets resourceAssets = ResourceAssetsParser.Parse();
            world.BatchEntitySpawner = new BatchEntitySpawner(resourceAssets, parsedBatchCells);

            return world;
        }
    }
}
