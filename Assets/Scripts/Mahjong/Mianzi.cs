﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace Mahjong
{
    [Serializable]
    public struct Mianzi : IComparable<Mianzi>//, IEnumerable<Tile>
    {
        public MianziType Type { get; }
        public Tile First { get; }
        public bool Open { get; set; }
        private bool isGangzi;

        public Mianzi(Tile first, MianziType type, bool open = false, bool gangzi = false) : this()
        {
            First = first;
            Type = type;
            Open = open;
            isGangzi = gangzi;
        }

        public Tile Last
        {
            get
            {
                switch (Type)
                {
                    case MianziType.Kezi:
                    case MianziType.Jiang:
                    case MianziType.Single:
                        return First;
                    case MianziType.Shunzi:
                        return First.Next.Next;
                    default:
                        throw new ArgumentException("Invalid MianziType");
                }
            }
        }

        public bool IsGangzi
        {
            get
            {
                if (Type != MianziType.Kezi) return false;
                return isGangzi;
            }
            set { isGangzi = value; }
        }

        public bool HasYaojiu => Type != MianziType.Single && (First.IsYaojiu || Last.IsYaojiu);

        public bool IsYaojiu => Type != MianziType.Single && First.IsYaojiu && Last.IsYaojiu;

        public bool HasLaotou => Type != MianziType.Single && (First.IsLaotou || Last.IsLaotou);

        public bool IsLaotou => Type != MianziType.Single && First.IsLaotou && Last.IsLaotou;

        public Suit Suit => First.Suit;

        public int Index => First.Index;

        public int CompareTo(Mianzi other)
        {
            if (!First.Equals(other.First)) return First.CompareTo(other.First); // 先按花色排序，然后按序数排序
            return Type - other.Type; // 第一张牌相同时，按刻子、顺子、将的顺序排列
        }

        public bool Contains(Tile tile)
        {
            if (tile.Suit != Suit) return false;
            return tile.Index >= First.Index && tile.Index <= Last.Index;
        }

        public override string ToString()
        {
            switch (Type)
            {
                case MianziType.Kezi:
                    return $"{First}{First}{First}";
                case MianziType.Shunzi:
                    return $"{First}{First.Next}{First.Next.Next}";
                case MianziType.Jiang:
                    return $"{First}{First}";
                case MianziType.Single:
                    return First.ToString();
                default:
                    throw new ArgumentException("Will not happen");
            }
        }

//        IEnumerator IEnumerable.GetEnumerator()
//        {
//            return GetEnumerator();
//        }
//
//        public IEnumerator<Tile> GetEnumerator()
//        {
//            return new MianziEnumerator(this);
//        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is Mianzi)
            {
                var other = (Mianzi) obj;
                return Type == other.Type && First.Equals(other.First);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        private struct MianziEnumerator : IEnumerator<Tile>
        {
            private Tile current;
            private readonly int indexStep;
            private readonly int tilesCount;
            private int currentIndex;

            public MianziEnumerator(Mianzi mianzi)
            {
                current = mianzi.First;
                indexStep = mianzi.Type == MianziType.Shunzi ? 1 : 0;
                switch (mianzi.Type)
                {
                    case MianziType.Single:
                        tilesCount = 1;
                        break;
                    case MianziType.Jiang:
                        tilesCount = 2;
                        break;
                    case MianziType.Kezi:
                    case MianziType.Shunzi:
                        tilesCount = 3;
                        break;
                    default: throw new ArgumentException("Should not happen");
                }

                currentIndex = 0;
            }

            public bool MoveNext()
            {
                if (currentIndex >= tilesCount) return false;
                currentIndex++;
                current = new Tile(current.Suit, current.Index + indexStep);
                return true;
            }

            public void Reset()
            {
                currentIndex = 0;
            }

            public Tile Current => current;

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }
        }
    }

    public enum MianziType
    {
        Kezi = 0,
        Shunzi = 1,
        Jiang = 2,
        Single = 3
    }
}