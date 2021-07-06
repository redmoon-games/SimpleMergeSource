using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Core.BigNumberAsset;
using Game.Enums;
using Game.Models;
using GoogleSheetsToUnity;
using ModestTree;
using Utils;

namespace Game.Databases.Helpers
{
    public static class DatabaseHelpers
    {
        public class ParseContext<T>
        {
            public Action<ParseContext<T>> parseRow;
            public Action<ParseContext<T>> init;
            public T value;
            public GstuSpreadSheet ss;
            public List<GSTU_Cell> row;
            public GSTU_Cell cell;
        }
        
        public static T ParseTable<T>(ParseContext<T> context, string column = "Id")
        {
            context.init(context);
            foreach (var cell in context.ss.columns[column])
            {
                if (cell.value == column)
                    continue;
                
                if (cell.value == "--")
                    break;

                context.cell = cell;
                context.row = context.ss.rows[cell.value];
                context.parseRow(context);
            }

            return context.value;
        }

        public static WorldChainVo ParseWorldChainTable(GstuSpreadSheet spreadSheet)
        {
            var context = new ParseContext<WorldChainVo> {ss = spreadSheet};
            context.init = c => c.value = null;
            context.parseRow = c =>
            {
                var vo = new WorldChainVo {id = c.cell.value};
                foreach (var rowCell in c.row)
                {
                    if (rowCell.value.Trim() == string.Empty)
                        continue;

                    switch (rowCell.columnId)
                    {
                        case "Nodes":
                            vo.nodes = rowCell.value.Split('/').ToList();
                            break;
                    }
                }

                c.value = vo;
            };
            
            return ParseTable(context);
        }
        
        public static List<WorldNodeVo> ParseWorldNodeTable(GstuSpreadSheet spreadSheet)
        {
            var context = new ParseContext<List<WorldNodeVo>> {ss = spreadSheet};
            context.init = c => c.value = new List<WorldNodeVo>();
            context.parseRow = c =>
            {
                var vo = new WorldNodeVo {id = c.cell.value};
                foreach (var rowCell in c.row)
                {
                    if (rowCell.value.Trim() == string.Empty)
                        continue;

                    switch (rowCell.columnId)
                    {
                        case "FirstChain":
                            vo.firstChainId = rowCell.value;
                            break;
                        case "SecondChain":
                            vo.secondChainId = rowCell.value;
                            break;
                        case "ThirdChain":
                            vo.thirdChainId = rowCell.value;
                            break;
                    }
                }
                c.value.Add(vo);
            };
            
            return ParseTable(context);
        }
        
        public static List<RoomNodeVo> ParseRoomNodeTable(GstuSpreadSheet spreadSheet)
        {
            var context = new ParseContext<List<RoomNodeVo>> {ss = spreadSheet};
            context.init = c => c.value = new List<RoomNodeVo>();
            context.parseRow = c =>
            {
                var vo = new RoomNodeVo {id = c.cell.value};
                foreach (var rowCell in c.row)
                {
                    if (rowCell.value.Trim() == string.Empty)
                        continue;

                    switch (rowCell.columnId)
                    {
                        case "MergeChains":
                            vo.mergeChains = rowCell.value.Split('/').ToList();;
                            break;
                    }
                }
                c.value.Add(vo);
            };
            
            return ParseTable(context);
        }
        
        public static List<MergeChainVo> ParseMergeChainTable(GstuSpreadSheet spreadSheet)
        {
            var context = new ParseContext<List<MergeChainVo>> {ss = spreadSheet};
            context.init = c => c.value = new List<MergeChainVo>();
            context.parseRow = c =>
            {
                var vo = new MergeChainVo {id = c.cell.value};
                foreach (var rowCell in c.row)
                {
                    if (rowCell.value.Trim() == string.Empty)
                        continue;

                    switch (rowCell.columnId)
                    {
                        case "Category":
                            Enum.TryParse(rowCell.value, out EMergeCategory category);
                            vo.category = category;
                            break;
                        case "StartFrom":
                            vo.startFrom = rowCell.value;
                            break;
                        case "EndWith":
                            vo.endWith = rowCell.value.Split('/').ToList();;
                            break;
                        case "Nodes":
                            vo.nodes = rowCell.value.Split('/').ToList();;
                            break;
                    }
                }
                c.value.Add(vo);
            };
            
            return ParseTable(context);
        }

        public static List<MergeNodeVo> ParseMergeNodeTable(GstuSpreadSheet spreadSheet)
        {
            var context = new ParseContext<List<MergeNodeVo>> {ss = spreadSheet};
            context.init = c => c.value = new List<MergeNodeVo>();
            context.parseRow = c =>
            {
                var vo = new MergeNodeVo {id = c.cell.value};
                foreach (var rowCell in c.row)
                {
                    if (rowCell.value.Trim() == string.Empty)
                        continue;

                    switch (rowCell.columnId)
                    {
                        case "Category":
                            Enum.TryParse(rowCell.value, out EMergeCategory category);
                            vo.category = category;
                            break;
                        case "Type":
                            vo.type = Convert.ToInt32(rowCell.value);;
                            break;
                    }
                }
                c.value.Add(vo);
            };
            
            return ParseTable(context);
        }
        
        public static List<UnitVo> ParseUnitTable(GstuSpreadSheet spreadSheet)
        {
            var context = new ParseContext<List<UnitVo>> {ss = spreadSheet};
            context.init = c => c.value = new List<UnitVo>();
            context.parseRow = c =>
            {
                var vo = new UnitVo {id = c.cell.value, mergeNodeId = c.cell.value};
                foreach (var rowCell in c.row)
                {
                    if (rowCell.value.Trim() == string.Empty)
                        continue;

                    switch (rowCell.columnId)
                    {
                        case "BehaviourPauseRange":
                            vo.behaviourPauseRange = ParseRange(rowCell.value);
                            break;
                    }
                }
                c.value.Add(vo);
            };
            
            return ParseTable(context);
        }
        
        public static List<LevelVo> ParseLevelTable(GstuSpreadSheet spreadSheet)
        {
            var context = new ParseContext<List<LevelVo>>(){ss = spreadSheet};
            context.init = c => c.value = new List<LevelVo>();
            context.parseRow = c =>
            {
                var vo = new LevelVo() {level = Convert.ToInt32(c.cell.value)};
                foreach (var rowCell in c.row)
                {
                    if (rowCell.value.Trim() == string.Empty)
                        continue;

                    switch (rowCell.columnId)
                    {
                        case "Income":
                            vo.income = ParseBigValue(rowCell.value);
                            break;
                        case "StartPrice":
                            vo.startPrice = ParseBigValue(rowCell.value);
                            break;
                        case "PriceFactor":
                            vo.priceFactor = Convert.ToSingle(rowCell.value);
                            break;
                        case "PriceStep":
                            vo.priceStep = Convert.ToInt32(rowCell.value);
                            break;
                        case "CrystalPrice":
                            vo.crystalPrice = Convert.ToInt32(rowCell.value);
                            break;
                    }
                }
                c.value.Add(vo);
            };
            
            return ParseTable(context, "Level");
        }

        public static List<OfflinePopupLevelVo> ParseOfflinePopupLevelTable(GstuSpreadSheet ss)
        {
            var context = new ParseContext<List<OfflinePopupLevelVo>>() { ss = ss };
            context.init = c => c.value = new List<OfflinePopupLevelVo>();
            context.parseRow = c =>
            {
                var vo = new OfflinePopupLevelVo() { level = Convert.ToInt32(c.cell.value) };
                foreach (var rowCell in c.row)
                {
                    if (rowCell.value.Trim() == string.Empty)
                        continue;

                    switch (rowCell.columnId)
                    {
                        case "Content":
                            vo.spinerContent = ParseSpinnerItems(rowCell.value);
                            break;
                        case "CrystalPrice":
                            vo.crystalPrice = Convert.ToInt32(rowCell.value);
                            break;
                        case "CanWatchAd":
                            vo.canWatchAd = Convert.ToBoolean(rowCell.value);
                            break;
                    }
                }
                c.value.Add(vo);
            };

            return ParseTable(context, "Level");
        }

        private static FloatRange ParseRange(string data)
        {
            var minMax = data.Split('/');
            
            return new FloatRange(Convert.ToSingle(minMax[0]), Convert.ToSingle(minMax[1]));
        }
        
        private static BigValue ParseBigValue(string data)
        {
            var values = data.Split('/');
            
            return new BigValue(Convert.ToSingle(values[0]), Convert.ToInt32(values[1]));
        }

        private static SpinnerItemData[] ParseSpinnerItems(string data)
        {
            var items = data.Split('/');

            SpinnerItemData[] spinnerItems = new SpinnerItemData[items.Length];
            for(int i = 0; i < items.Length; i++)
            {
                var itemData = items[i].Split('-');
                var spinerItem = new SpinnerItemData()
                {
                    multiplier =  Convert.ToSingle(itemData[0]),
                    chance = Convert.ToSingle(itemData[1])
                };

                spinnerItems[i] = spinerItem;
            }

            return spinnerItems;
        }
         
        private static List<string> ParseStringList(string data)
        {
            var result = new List<string>();
            foreach (var element in data.Split('/'))
            {
                if (!element.Trim().IsEmpty())
                {
                    result.Add(element.Trim());
                }
            }

            return result;
        }

        public static List<BuildingTypeVo> ParseBuildingTypeTable(GstuSpreadSheet spreadSheet)
        {
            var context = new ParseContext<List<BuildingTypeVo>>
            {
                ss = spreadSheet, 
                init = c => c.value = new List<BuildingTypeVo>(), 
                parseRow = ParseBuildingTypeRow
            };

            return ParseTable(context);
        }

        private static void ParseBuildingTypeRow(ParseContext<List<BuildingTypeVo>> context)
        {
            var result = new BuildingTypeVo { id = context.cell.value };
            foreach (var rowCell in context.row)
            {
                if (rowCell.value.Trim() == string.Empty) continue;

                switch (rowCell.columnId)
                {
                    case "NameId":
                        result.nameId = rowCell.value;
                        break;
                    case "DescriptionId":
                        result.descriptionId = rowCell.value;
                        break;
                }
            }

            context.value.Add(result);
        }

        public static List<RoomSlotVo> ParseRoomSlotsTable(GstuSpreadSheet spreadSheet)
        {
            var context = new ParseContext<List<RoomSlotVo>>
            {
                ss = spreadSheet, 
                init = c => c.value = new List<RoomSlotVo>(), 
                parseRow = ParseRoomSlotsRow
            };

            return ParseTable(context);
        }

        private static void ParseRoomSlotsRow(ParseContext<List<RoomSlotVo>> context)
        {
            var result = new RoomSlotVo{ id = context.cell.value};
            foreach (var rowCell in context.row)
            {
                if (rowCell.value.Trim() == string.Empty) continue;

                switch (rowCell.columnId)
                {
                    case "RoomId":
                        result.roomId = rowCell.value;
                        break;
                    case "BuildingTypeId":
                        result.buildingTypeId = rowCell.value;
                        break;
                    case "xCoord":
                        result.xCoord = float.Parse(rowCell.value);
                        break;
                    case "yCoord":
                        result.yCoord = float.Parse(rowCell.value);
                        break;
                }
            }

            context.value.Add(result);
        }

        public static List<BuildingCostVo> ParseBuildingCostsTable(GstuSpreadSheet spreadSheet)
        {
            var context = new ParseContext<List<BuildingCostVo>>
            {
                ss = spreadSheet, 
                init = c => c.value = new List<BuildingCostVo>(), 
                parseRow = ParseBuildingCostsRow
            };

            return ParseTable(context);
        }

        private static void ParseBuildingCostsRow(ParseContext<List<BuildingCostVo>> context)
        {
            var result = new BuildingCostVo{ id = context.cell.value};
            foreach (var rowCell in context.row)
            {
                if (rowCell.value.Trim() == string.Empty) continue;

                switch (rowCell.columnId)
                {
                    case "BuildingTypeId":
                        result.buildingTypeId = rowCell.value;
                        break;
                    case "Level":
                        result.level = int.Parse(rowCell.value);
                        break;
                    case "Experience":
                        result.experience = int.Parse(RemoveWhitespaces(rowCell.value));
                        break;
                    case "Cost":
                        result.cost = BigValue.Parse(RemoveWhitespaces(rowCell.value));
                        break;
                    case "DeliveryTime":
                        result.deliveryTime = int.Parse(RemoveWhitespaces(rowCell.value));
                        break;
                    case "DeliveryCostCrystals":
                        result.deliveryCostCrystal = int.Parse(rowCell.value);
                        break;
                    case "DeliveryCostUnit":
                        result.deliveryCostUnits = ParseStringList(rowCell.value);
                        break;
                }
            }

            context.value.Add(result);
        }

        public static List<BuildingVo> ParseBuildingsTable(GstuSpreadSheet spreadSheet)
        {
            var context = new ParseContext<List<BuildingVo>>
            {
                ss = spreadSheet, 
                init = c => c.value = new List<BuildingVo>(), 
                parseRow = ParseBuildingsRow
            };

            return ParseTable(context, "BuildingId");
        }

        private static void ParseBuildingsRow(ParseContext<List<BuildingVo>> context)
        {
            var result = new BuildingVo{ id = context.cell.value};
            foreach (var rowCell in context.row)
            {
                if (rowCell.value.Trim() == string.Empty) continue;

                switch (rowCell.columnId)
                {
                    case "BuildingTypeId":
                        result.buildingTypeId = rowCell.value;
                        break;
                    case "Level":
                        result.level = int.Parse(rowCell.value);
                        break;
                    case "Image":
                        result.image = rowCell.value;
                        break;
                    case "Name":
                        result.nameId = rowCell.value;
                        break;
                    case "Description":
                        result.descriptionId = rowCell.value;
                        break;
                }
            }

            context.value.Add(result);
        }
        
        private static readonly Regex Whitespace = new Regex(@"\s+");
        
        private static string RemoveWhitespaces(string input) 
        {
            return Whitespace.Replace(input, string.Empty);
        }

        public static List<BuildingRequirementVo> BuildingRequirementsTable(GstuSpreadSheet spreadSheet)
        {
            var context = new ParseContext<List<BuildingRequirementVo>>
            {
                ss = spreadSheet, 
                init = c => c.value = new List<BuildingRequirementVo>(), 
                parseRow = ParseBuildingRequirementsRow
            };

            return ParseTable(context);
        }

        private static void ParseBuildingRequirementsRow(ParseContext<List<BuildingRequirementVo>> context)
        {
            var result = new BuildingRequirementVo{ id = context.cell.value};
            foreach (var rowCell in context.row)
            {
                if (rowCell.value.Trim() == string.Empty) continue;

                switch (rowCell.columnId)
                {
                    case "BuildingId":
                        result.buildingId = rowCell.value;
                        break;
                    case "RequiredBuildingId":
                        result.requiredBuildingId = rowCell.value;
                        break;
                }
            }

            context.value.Add(result);
        }
    }
}
