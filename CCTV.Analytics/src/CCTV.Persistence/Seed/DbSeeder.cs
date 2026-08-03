using CCTV.Domain.Entities.Analytics;
using CCTV.Domain.Entities.Orchestration;
using CCTV.Domain.Entities.Spatial;
using CCTV.Domain.Enumerations;
using CCTV.Persistence.Contexts;
using CCTV.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CCTV.Persistence.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(CCTVAnalyticsDbContext context, ITenantProvider tenantProvider)
    {
        var tenantId = tenantProvider.ApplicationId;

        // STAGE 1: Sites (Parent)
        var siteRepo = context.Set<Site>();
        Site? hqSite = await siteRepo.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Code == "site_hq_building" && s.ApplicationId == tenantId);
        if (hqSite == null)
        {
            hqSite = new Site
            {
                ApplicationId = tenantId,
                Name = "HQ Headquarters",
                Code = "site_hq_building",
                Address = "100 Enterprise Way, Silicon Valley, CA"
            };

            var westSite = new Site
            {
                ApplicationId = tenantId,
                Name = "West Wing Plant",
                Code = "site_west_wing",
                Address = "200 Manufacturing Blvd, Austin, TX"
            };

            await siteRepo.AddRangeAsync(hqSite, westSite);
            await context.SaveChangesAsync();
        }

        // STAGE 2: Buildings (Requires valid hqSite.Id)
        var buildingRepo = context.Set<Building>();
        Building? mainBuilding = await buildingRepo.IgnoreQueryFilters().FirstOrDefaultAsync(b => b.BuildingCode == "bldg_main" && b.SiteId == hqSite.Id);
        if (mainBuilding == null)
        {
            mainBuilding = new Building
            {
                ApplicationId = tenantId,
                SiteId = hqSite.Id,
                Name = "Main Administration Building",
                BuildingCode = "bldg_main",
                TotalFloors = 3
            };

            await buildingRepo.AddAsync(mainBuilding);
            await context.SaveChangesAsync();
        }

        // STAGE 3: Floors (Requires valid mainBuilding.Id)
        var floorRepo = context.Set<Floor>();
        Floor? groundFloor = await floorRepo.IgnoreQueryFilters().FirstOrDefaultAsync(f => f.BuildingId == mainBuilding.Id && f.FloorLevel == 1);
        if (groundFloor == null)
        {
            groundFloor = new Floor
            {
                ApplicationId = tenantId,
                BuildingId = mainBuilding.Id,
                Name = "Ground Floor",
                FloorLevel = 1
            };

            await floorRepo.AddAsync(groundFloor);
            await context.SaveChangesAsync();
        }

        // STAGE 3.5: Rooms (Requires valid groundFloor.Id)
        var roomRepo = context.Set<Room>();
        Room? mainLobbyRoom = await roomRepo.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.FloorId == groundFloor.Id && r.Code == "room_main_lobby");
        if (mainLobbyRoom == null)
        {
            mainLobbyRoom = new Room
            {
                ApplicationId = tenantId,
                FloorId = groundFloor.Id,
                Name = "Main Lobby Area",
                Code = "room_main_lobby",
                Description = "Ground Floor Reception & Entrance Lobby",
                SortOrder = 1,
                IsActive = true
            };

            await roomRepo.AddAsync(mainLobbyRoom);
            await context.SaveChangesAsync();
        }

        // STAGE 4: Cameras (Requires valid mainLobbyRoom.Id)
        var cameraRepo = context.Set<Camera>();
        const string defaultRtspUrl = "rtsp://admin:Admin1234@192.168.1.64/Streaming/channels/2";

        var camerasToSeed = new[]
        {
            new Camera { ApplicationId = tenantId, RoomId = mainLobbyRoom.Id, Name = "CAM-01 Main Lobby", DeviceCode = "cam_01", IpAddress = "192.168.1.202", RtspStreamUrl = defaultRtspUrl, Status = CameraStatus.Online, Resolution = "1920x1080", FrameRate = 30 },
            new Camera { ApplicationId = tenantId, RoomId = mainLobbyRoom.Id, Name = "CAM-02 Service Counter", DeviceCode = "cam_02", IpAddress = "192.168.1.202", RtspStreamUrl = defaultRtspUrl, Status = CameraStatus.Online, Resolution = "1920x1080", FrameRate = 30 },
            new Camera { ApplicationId = tenantId, RoomId = mainLobbyRoom.Id, Name = "CAM-04 Vault Corridor", DeviceCode = "cam_04", IpAddress = "192.168.1.202", RtspStreamUrl = defaultRtspUrl, Status = CameraStatus.Online, Resolution = "1920x1080", FrameRate = 30 },
            new Camera { ApplicationId = tenantId, RoomId = mainLobbyRoom.Id, Name = "CAM-08 Loading Dock", DeviceCode = "cam_08", IpAddress = "192.168.1.202", RtspStreamUrl = defaultRtspUrl, Status = CameraStatus.Online, Resolution = "1920x1080", FrameRate = 30 },
            new Camera { ApplicationId = tenantId, RoomId = mainLobbyRoom.Id, Name = "CAM-12 Warehouse B", DeviceCode = "cam_12", IpAddress = "192.168.1.202", RtspStreamUrl = defaultRtspUrl, Status = CameraStatus.Online, Resolution = "1920x1080", FrameRate = 30 }
        };

        var newCameras = new List<Camera>();
        foreach (var cam in camerasToSeed)
        {
            bool exists = await cameraRepo.IgnoreQueryFilters().AnyAsync(c => c.DeviceCode == cam.DeviceCode && c.ApplicationId == tenantId);
            if (!exists)
            {
                newCameras.Add(cam);
            }
        }

        if (newCameras.Count > 0)
        {
            await cameraRepo.AddRangeAsync(newCameras);
            await context.SaveChangesAsync();
        }

        // Fetch cam1 reference for STAGE 7
        Camera? cam1 = await cameraRepo.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.DeviceCode == "cam_01" && c.ApplicationId == tenantId);

        // STAGE 5: WorkerNodes
        var nodeRepo = context.Set<WorkerNode>();
        WorkerNode? workerNode1 = await nodeRepo.IgnoreQueryFilters().FirstOrDefaultAsync(n => n.Hostname == "edge-server-01" && n.ApplicationId == tenantId);
        if (workerNode1 == null)
        {
            workerNode1 = new WorkerNode
            {
                ApplicationId = tenantId,
                NodeId = Guid.NewGuid(),
                Hostname = "edge-server-01",
                IpAddress = "192.168.1.120",
                MaxCapacityPipelineSlots = 8,
                TotalGpuMemoryMb = 24576,
                CpuCoreCount = 16,
                Status = "Active",
                LastSeenAt = DateTime.UtcNow
            };

            await nodeRepo.AddAsync(workerNode1);
            await context.SaveChangesAsync();
        }

        // STAGE 6: EngineInstances & EngineCapabilities (Requires valid workerNode1.Id)
        var engineRepo = context.Set<EngineInstance>();
        EngineInstance? engine1 = await engineRepo.IgnoreQueryFilters().FirstOrDefaultAsync(e => e.InstanceId == "engine_edge_01" && e.WorkerNodeId == workerNode1.Id);
        if (engine1 == null)
        {
            engine1 = new EngineInstance
            {
                ApplicationId = tenantId,
                WorkerNodeId = workerNode1.Id,
                InstanceId = "engine_edge_01",
                Hostname = "edge-server-01",
                IpAddress = "192.168.1.120",
                GpuModel = "NVIDIA RTX 4090",
                TotalVramMb = 24576,
                State = EngineState.Online,
                LastHeartbeatUtc = DateTime.UtcNow
            };

            engine1.Capabilities.Add(new EngineCapability { CapabilityCode = "people_counting", Version = "1.0", SdkVersion = "v8" });
            engine1.Capabilities.Add(new EngineCapability { CapabilityCode = "queue_detection", Version = "1.0", SdkVersion = "v8" });
            engine1.Capabilities.Add(new EngineCapability { CapabilityCode = "loitering_detection", Version = "1.0", SdkVersion = "v8" });

            await engineRepo.AddAsync(engine1);
            await context.SaveChangesAsync();
        }

        // STAGE 7: PipelineAssignments (Requires valid cam1.Id and engine1.Id)
        var pipelineRepo = context.Set<PipelineAssignment>();
        PipelineAssignment? pipeline1 = null;
        if (cam1 != null)
        {
            pipeline1 = await pipelineRepo.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.PipelineId == "pipeline_cam_lobby_01" && p.CameraId == cam1.Id);
            if (pipeline1 == null)
            {
                pipeline1 = new PipelineAssignment
                {
                    ApplicationId = tenantId,
                    CameraId = cam1.Id,
                    InstanceId = engine1.Id,
                    PipelineId = "pipeline_cam_lobby_01",
                    PipelineName = "CAM-01 Main Lobby Analytics Pipeline",
                    Priority = 1,
                    RequiredCapabilities = new List<string> { "people_counting", "loitering_detection" },
                    AnalyticsModules = new List<string> { "people_counter", "loitering" },
                    Status = PipelineStatus.Running,
                    AssignedAtUtc = DateTime.UtcNow,
                    StartedAtUtc = DateTime.UtcNow
                };

                await pipelineRepo.AddAsync(pipeline1);
                await context.SaveChangesAsync();
            }
        }

        // STAGE 8: AnalyticsModules (Plugin Catalog)
        var moduleRepo = context.Set<AnalyticsModule>();
        var defaultModules = new List<AnalyticsModule>
        {
            new AnalyticsModule { ModuleCode = "people_counting", DisplayName = "People Counting", Category = "BUSINESS_INTELLIGENCE", Description = "Realtime bidirectional people counting & occupancy analytics.", PythonEntryPoint = "camera_analytics.plugins.people_counting", Icon = "IconUsers", Version = "1.0.0", Vendor = "Bionics AI", MinimumCudaCapability = "6.0", Status = "Active" },
            new AnalyticsModule { ModuleCode = "queue_detection", DisplayName = "Queue Detection", Category = "BUSINESS_INTELLIGENCE", Description = "Monitors queue lengths, dwell times, and service delays.", PythonEntryPoint = "camera_analytics.plugins.queue_detection", Icon = "IconBuildingStore", Version = "1.0.0", Vendor = "Bionics AI", MinimumCudaCapability = "6.0", Status = "Active" },
            new AnalyticsModule { ModuleCode = "loitering_detection", DisplayName = "Loitering Detection", Category = "SAFETY_SECURITY", Description = "Detects unauthorized loitering in designated zones.", PythonEntryPoint = "camera_analytics.plugins.loitering_detection", Icon = "IconClock", Version = "1.0.0", Vendor = "Bionics AI", MinimumCudaCapability = "6.0", Status = "Active" },
            new AnalyticsModule { ModuleCode = "intrusion_detection", DisplayName = "Intrusion Detection", Category = "SAFETY_SECURITY", Description = "Triggers alerts when humans/objects breach restricted areas.", PythonEntryPoint = "camera_analytics.plugins.intrusion_detection", Icon = "IconShield", Version = "1.0.0", Vendor = "Bionics AI", MinimumCudaCapability = "6.0", Status = "Active" },
            new AnalyticsModule { ModuleCode = "ppe_compliance", DisplayName = "PPE Compliance", Category = "OPERATIONAL_COMPLIANCE", Description = "Detects hardhats, safety vests, gloves, and safety goggles.", PythonEntryPoint = "camera_analytics.plugins.ppe_compliance", Icon = "IconShirt", Version = "1.0.0", Vendor = "Bionics AI", MinimumCudaCapability = "7.0", Status = "Active" },
            new AnalyticsModule { ModuleCode = "fire_smoke", DisplayName = "Fire & Smoke", Category = "SAFETY_SECURITY", Description = "Early optical detection of open flame and smoke plumes.", PythonEntryPoint = "camera_analytics.plugins.fire_smoke", Icon = "IconFlame", Version = "1.0.0", Vendor = "Bionics AI", MinimumCudaCapability = "7.0", Status = "Active" },
            new AnalyticsModule { ModuleCode = "face_recognition", DisplayName = "Face Recognition", Category = "SAFETY_SECURITY", Description = "Biometric face identification & watchlist matching.", PythonEntryPoint = "camera_analytics.plugins.face_recognition", Icon = "IconUserCheck", Version = "1.0.0", Vendor = "Bionics AI", MinimumCudaCapability = "7.5", Status = "Active" },
            new AnalyticsModule { ModuleCode = "vehicle_counting", DisplayName = "Vehicle Counting", Category = "TRAFFIC_MOBILITY", Description = "Vehicle classification and traffic volume counting.", PythonEntryPoint = "camera_analytics.plugins.vehicle_counting", Icon = "IconCar", Version = "1.0.0", Vendor = "Bionics AI", MinimumCudaCapability = "6.0", Status = "Active" },
            new AnalyticsModule { ModuleCode = "anpr_license_plate", DisplayName = "License Plate Recognition (ANPR)", Category = "TRAFFIC_MOBILITY", Description = "Optical character recognition for vehicle license plates.", PythonEntryPoint = "camera_analytics.plugins.anpr", Icon = "IconId", Version = "1.0.0", Vendor = "Bionics AI", MinimumCudaCapability = "7.0", Status = "Active" },
            new AnalyticsModule { ModuleCode = "crowd_density", DisplayName = "Crowd Density", Category = "BUSINESS_INTELLIGENCE", Description = "Measures spatial crowd density and heatmaps.", PythonEntryPoint = "camera_analytics.plugins.crowd_density", Icon = "IconUsersGroup", Version = "1.0.0", Vendor = "Bionics AI", MinimumCudaCapability = "6.0", Status = "Active" },
            new AnalyticsModule { ModuleCode = "violence_detection", DisplayName = "Violence Detection", Category = "SAFETY_SECURITY", Description = "Realtime detection of aggressive physical altercations.", PythonEntryPoint = "camera_analytics.plugins.violence_detection", Icon = "IconAlertTriangle", Version = "1.0.0", Vendor = "Bionics AI", MinimumCudaCapability = "7.5", Status = "Active" },
            new AnalyticsModule { ModuleCode = "fall_detection", DisplayName = "Fall Detection", Category = "SAFETY_SECURITY", Description = "Detects human slips, trips, falls, and medical emergencies.", PythonEntryPoint = "camera_analytics.plugins.fall_detection", Icon = "IconActivity", Version = "1.0.0", Vendor = "Bionics AI", MinimumCudaCapability = "6.0", Status = "Active" },
            new AnalyticsModule { ModuleCode = "line_crossing", DisplayName = "Line Crossing", Category = "SAFETY_SECURITY", Description = "Virtual tripwire detection across customizable directional lines.", PythonEntryPoint = "camera_analytics.plugins.line_crossing", Icon = "IconVectorTriangle", Version = "1.0.0", Vendor = "Bionics AI", MinimumCudaCapability = "6.0", Status = "Active" },
            new AnalyticsModule { ModuleCode = "object_left_behind", DisplayName = "Object Left Behind", Category = "SAFETY_SECURITY", Description = "Identifies unattended luggage or suspicious packages.", PythonEntryPoint = "camera_analytics.plugins.object_left", Icon = "IconPackage", Version = "1.0.0", Vendor = "Bionics AI", MinimumCudaCapability = "6.0", Status = "Active" },
            new AnalyticsModule { ModuleCode = "object_removed", DisplayName = "Object Removed", Category = "SAFETY_SECURITY", Description = "Detects missing high-value assets or stolen items.", PythonEntryPoint = "camera_analytics.plugins.object_removed", Icon = "IconBoxOff", Version = "1.0.0", Vendor = "Bionics AI", MinimumCudaCapability = "6.0", Status = "Active" }
        };

        foreach (var mod in defaultModules)
        {
            if (!await moduleRepo.IgnoreQueryFilters().AnyAsync(m => m.ModuleCode == mod.ModuleCode))
            {
                await moduleRepo.AddAsync(mod);
            }
        }
        await context.SaveChangesAsync();

        // STAGE 9: PipelineModuleAssignments
        if (pipeline1 != null)
        {
            var pmaRepo = context.Set<PipelineModuleAssignment>();
            var seededModules = await moduleRepo.IgnoreQueryFilters().ToListAsync();
            var targetModules = seededModules.Where(m => m.ModuleCode == "people_counting" || m.ModuleCode == "queue_detection" || m.ModuleCode == "loitering_detection").ToList();

            foreach (var mod in targetModules)
            {
                if (!await pmaRepo.IgnoreQueryFilters().AnyAsync(pma => pma.PipelineAssignmentId == pipeline1.Id && pma.ModuleId == mod.Id))
                {
                    await pmaRepo.AddAsync(new PipelineModuleAssignment
                    {
                        ApplicationId = tenantId,
                        PipelineAssignmentId = pipeline1.Id,
                        ModuleId = mod.Id,
                        IsEnabled = true,
                        AssignedAtUtc = DateTime.UtcNow
                    });
                }
            }
            await context.SaveChangesAsync();
        }
    }
}
