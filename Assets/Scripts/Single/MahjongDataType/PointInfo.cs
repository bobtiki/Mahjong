using System;
using System.Collections.Generic;
using System.Linq;
using Multi.MahjongMessages;

namespace Single.MahjongDataType
{
    [Serializable]
    public struct PointInfo : IComparable<PointInfo>
    {
        public int Fu { get; }
        private int Fan;
        private YakuValue[] Yakus;
        public bool IsYakuman { get; }
        public bool Is青天井 { get; }
        public int Dora { get; }
        public int UraDora { get; }
        public int RedDora { get; }

        public PointInfo(int fu, IList<YakuValue> yakuValues, bool 青天井, int dora = 0, int uraDora = 0, int redDora = 0)
        {
            Fu = fu;
            Yakus = yakuValues.ToArray();
            Fan = 0;
            Is青天井 = 青天井;
            Dora = dora;
            UraDora = uraDora;
            RedDora = redDora;
            IsYakuman = false;
            if (青天井)
            {
                foreach (var yaku in yakuValues)
                {
                    Fan += yaku.Type == YakuType.Yakuman ? yaku.Value * YakuSettings.YakumanBaseFan : yaku.Value;
                    if (yaku.Type == YakuType.Yakuman) IsYakuman = true;
                }
            }
            else
            {
                foreach (var yaku in yakuValues)
                {
                    Fan += yaku.Value;
                    if (yaku.Type == YakuType.Yakuman) IsYakuman = true;
                }
            }
            FanWithoutDora = yakuValues.Sum(y => y.Type == YakuType.Yakuman ? y.Value * YakuSettings.YakumanBaseFan : y.Value);

            if (yakuValues.Count == 0)
            {
                BasePoint = 0;
                TotalFan = 0;
                return;
            }

            if (青天井)
            {
                TotalFan = Fan + dora + uraDora + redDora;
                int point = Fu * (int)Math.Pow(2, TotalFan + 2);
                BasePoint = MahjongLogic.ToNextUnit(point, 100);
            }
            else if (IsYakuman)
            {
                BasePoint = Fan * MahjongConstants.Yakuman;
                TotalFan = Fan;
            }
            else
            {
                TotalFan = Fan + dora + uraDora + redDora;
                if (TotalFan >= 13) BasePoint = MahjongConstants.Yakuman;
                else if (TotalFan >= 11) BasePoint = MahjongConstants.Sanbaiman;
                else if (TotalFan >= 8) BasePoint = MahjongConstants.Baiman;
                else if (TotalFan >= 6) BasePoint = MahjongConstants.Haneman;
                else if (TotalFan >= 5) BasePoint = MahjongConstants.Mangan;
                else
                {
                    int point = Fu * (int)Math.Pow(2, TotalFan + 2);
                    point = MahjongLogic.ToNextUnit(point, 100);
                    BasePoint = Math.Min(MahjongConstants.Mangan, point);
                }
            }
            Array.Sort(Yakus);
        }

        public PointInfo(NetworkPointInfo netInfo) 
            : this(netInfo.Fu, netInfo.YakuValues, netInfo.IsQTJ, netInfo.Dora, netInfo.UraDora, netInfo.RedDora)
        {
        }

        public int BasePoint { get; }
        public int TotalFan { get; }
        public int FanWithoutDora { get; }

        public IList<YakuValue> YakuList
        {
            get
            {
                return new List<YakuValue>(Yakus);
            }
        }

        public override string ToString()
        {
            var yakus = Yakus == null ? "" : string.Join(", ", Yakus.Select(yaku => yaku.ToString()));
            return
                $"Fu = {Fu}, Fan = {Fan}, Dora = {Dora}, UraDora = {UraDora}, RedDora = {RedDora}, "
                + $"Yakus = [{yakus}], BasePoint = {BasePoint}";
        }

        public int CompareTo(PointInfo other)
        {
            var basePointComparison = BasePoint.CompareTo(other.BasePoint);
            if (basePointComparison != 0) return basePointComparison;
            var fanComparison = TotalFan.CompareTo(other.TotalFan);
            if (fanComparison != 0) return fanComparison;
            return Fu.CompareTo(other.Fu);
        }
    }
}