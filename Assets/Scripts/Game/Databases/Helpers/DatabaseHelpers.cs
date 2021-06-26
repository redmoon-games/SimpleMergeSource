using System;
using System.Collections.Generic;
using System.Linq;
using Core.BigNumberAsset;
using Game.Enums;
using Game.Models;
using GoogleSheetsToUnity;
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

        public static WorldChainVo ParseWorldChainTable(GstuSpreadSheet ss)
        {
            var context = new ParseContext<WorldChainVo>(){ss = ss};
            context.init = c => c.value = null;
            context.parseRow = c =>
            {
                var vo = new WorldChainVo() {id = c.cell.value};
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
        
        public static List<WorldNodeVo> ParseWorldNodeTable(GstuSpreadSheet ss)
        {
            var context = new ParseContext<List<WorldNodeVo>>(){ss = ss};
            context.init = c => c.value = new List<WorldNodeVo>();
            context.parseRow = c =>
            {
                var vo = new WorldNodeVo() {id = c.cell.value};
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
        
        public static List<RoomNodeVo> ParseRoomNodeTable(GstuSpreadSheet ss)
        {
            var context = new ParseContext<List<RoomNodeVo>>(){ss = ss};
            context.init = c => c.value = new List<RoomNodeVo>();
            context.parseRow = c =>
            {
                var vo = new RoomNodeVo() {id = c.cell.value};
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
        
        public static List<MergeChainVo> ParseMergeChainTable(GstuSpreadSheet ss)
        {
            var context = new ParseContext<List<MergeChainVo>>(){ss = ss};
            context.init = c => c.value = new List<MergeChainVo>();
            context.parseRow = c =>
            {
                var vo = new MergeChainVo() {id = c.cell.value};
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

        public static List<MergeNodeVo> ParseMergeNodeTable(GstuSpreadSheet ss)
        {
            var context = new ParseContext<List<MergeNodeVo>>(){ss = ss};
            context.init = c => c.value = new List<MergeNodeVo>();
            context.parseRow = c =>
            {
                var vo = new MergeNodeVo() {id = c.cell.value};
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
        
        public static List<UnitVo> ParseUnitTable(GstuSpreadSheet ss)
        {
            var context = new ParseContext<List<UnitVo>>(){ss = ss};
            context.init = c => c.value = new List<UnitVo>();
            context.parseRow = c =>
            {
                var vo = new UnitVo() {id = c.cell.value, mergeNodeId = c.cell.value};
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
        
        public static List<LevelVo> ParseLevelTable(GstuSpreadSheet ss)
        {
            var context = new ParseContext<List<LevelVo>>(){ss = ss};
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
    }
}
