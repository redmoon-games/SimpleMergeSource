using System;
using System.Collections.Generic;

namespace Game.UI.Popups.BuildingGame
{
    public static class TaskExtensions
    {

        public static Dictionary<string, BuildingTaskData> MapBuildingTaskData(
            this IEnumerable<BuildingTaskData> buildingTasks)
        {
            var result = new Dictionary<string, BuildingTaskData>();

            foreach (var buildingTask in buildingTasks)
            {
                result[buildingTask.building.buildingId] = buildingTask;
            }

            return result;
        }
        
        public static int CountActiveDeliveries(this MetaGameData source)
        {
            var count = 0;
            foreach (var task in source.tasks)
            {
                count += task.state == TaskState.Delivery ? 1 : 0;
            }

            return count;
        }

        public static BuildingTaskData FindById(this IEnumerable<BuildingTaskData> source, string buildingId)
        {
            foreach (var taskData in source)
            {
                if (taskData.building.buildingId == buildingId) return taskData;
            }

            throw new Exception($"BuildingTaskData with buildingId: {buildingId} not found");
        }
    }
}