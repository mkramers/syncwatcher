using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace Common.Focal
{
    public class UsImageKey
    {
        public UsImageKey(int _depth, Direction _direction)
        {
            Depth = _depth;
            Direction = _direction;
        }

        public bool Equals(UsImageKey _spacing)
        {
            var equals = false;

            if (_spacing != null)
            {
                equals = true;
                equals &= Depth == _spacing.Depth;
                equals &= Direction == _spacing.Direction;
            }

            return equals;
        }

        public override bool Equals(object _obj)
        {
            var equals = false;

            if (_obj != null)
                equals = Equals(_obj as UsImageKey);

            return equals;
        }

        public override int GetHashCode()
        {
            return Depth.GetHashCode() * 1000 + Direction.GetHashCode();
        }

        public int Depth { get; }
        public Direction Direction { get; }
    }

    public class ProbeFileKey
    {
        public ProbeFileKey(Probe _probe, Direction _direction)
        {
            Probe = _probe;
            Direction = _direction;
        }

        public bool Equals(ProbeFileKey _spacing)
        {
            var equals = false;

            if (_spacing != null)
            {
                equals = true;
                equals &= Probe == _spacing.Probe;
                equals &= Direction == _spacing.Direction;
            }

            return equals;
        }

        public override bool Equals(object _obj)
        {
            var equals = false;

            if (_obj != null)
                equals = Equals(_obj as ProbeFileKey);

            return equals;
        }

        public override int GetHashCode()
        {
            return Probe.GetHashCode() * 1000 + Direction.GetHashCode();
        }

        public static string GetBxFileName(ProbeFileKey _key)
        {
            var probeName = "";
            switch (_key.Probe)
            {
                case Probe.BK8808:
                    probeName = "8808";
                    break;
                case Probe.BK8818:
                    probeName = "8818";
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }

            var fireName = "";
            switch (_key.Direction)
            {
                case Direction.SIDEFIRE:
                    fireName = "SideFire";
                    break;
                case Direction.ENDFIRE:
                    fireName = "EndFire";
                    break;
                case Direction.TRANSVERSE:
                    fireName = "TransverseFire";
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }

            var name = $"{probeName}{fireName}Settings.xml";
            return name;
        }

        public Probe Probe { get; }
        public Direction Direction { get; }
    }

    public class UsImageInfo
    {
        public Vector2 PixelSpacing { get; set; }
        public float ImageShift { get; set; }
        public Dictionary<Guide, Dictionary<LeftRight, NeedleGuideValues2>> Guides { get; set; }
    }

    public class ProbeUsImage
    {
        public static ProbeUsImage GetFromStrings(List<string> _row)
        {
            Debug.Assert(_row != null);

            const int probeRow = 0;
            const int fireRow = 1;
            const int depthRow = 2;
            const int leftRightRow = 3;
            const int spacingRowStart = 4;
            const int imageShiftRow = 6;
            const int needleNameRow = 7;
            const int needlePointsRowStart = 8;

            var ignoreNeedleGuide = _row.Count == 8;
            Debug.Assert(ignoreNeedleGuide || _row.Count == 14, "CSV missing values!");

            var probe = (Probe) Enum.Parse(typeof(Probe), _row[probeRow]);

            var fire = (Direction) Enum.Parse(typeof(Direction), _row[fireRow]);

            var depth = Convert.ToInt16(_row[depthRow]);

            var leftRight = (LeftRight) Enum.Parse(typeof(LeftRight), _row[leftRightRow]);

            var spacingX = Convert.ToSingle(_row[spacingRowStart]);
            var spacingY = Convert.ToSingle(_row[spacingRowStart + 1]);
            var pixelSpacing = new Vector2(spacingX, spacingY);

            var imageShift = Convert.ToSingle(_row[imageShiftRow]);

            var needleGuideName = (Guide) Enum.Parse(typeof(Guide), _row[needleNameRow]);

            NeedleGuideValues2 needleGuide;
            if (!ignoreNeedleGuide)
            {
                var startX = Convert.ToSingle(_row[needlePointsRowStart]);
                var startY = Convert.ToSingle(_row[needlePointsRowStart + 1]);
                var startZ = Convert.ToSingle(_row[needlePointsRowStart + 2]);

                var endX = Convert.ToSingle(_row[needlePointsRowStart + 3]);
                var endY = Convert.ToSingle(_row[needlePointsRowStart + 4]);
                var endZ = Convert.ToSingle(_row[needlePointsRowStart + 5]);

                needleGuide = new NeedleGuideValues2
                {
                    Start = new Vector3(startX, startY, startZ),
                    End = new Vector3(endX, endY, endZ)
                };
            }
            else
            {
                needleGuide = new NeedleGuideValues2
                {
                    Start = Vector3.Zero,
                    End = Vector3.One
                };
            }

            return new ProbeUsImage
            {
                Probe = probe,
                Depth = depth,
                Direction = fire,
                LeftRight = leftRight,
                PixelSpacing = pixelSpacing,
                ImageShift = imageShift,
                NeedleGuideName = needleGuideName,
                NeedleGuideValues = needleGuide
            };
        }

        public Probe Probe { get; set; }
        public int Depth { get; set; }
        public Direction Direction { get; set; }
        public Vector2 PixelSpacing { get; set; }
        public float ImageShift { get; set; }
        public LeftRight LeftRight { get; set; }
        public Guide NeedleGuideName { get; set; }
        public NeedleGuideValues2 NeedleGuideValues { get; set; }
    }

    public class GuideXml
    {
        public int Depth { get; set; }
        public LeftRight LeftRight { get; set; }
        public NeedleGuideValues2 Points { get; set; }
    }

    public class NeedleGuideValues2
    {
        public Vector3 Start { get; set; }
        public Vector3 End { get; set; }
    }

    public enum Guide
    {
        Standard_18Degrees,
        Custom_40Degrees
    }

    public enum Probe
    {
        BK8808,
        BK8818
    }

    public enum Direction
    {
        SIDEFIRE,
        ENDFIRE,
        TRANSVERSE
    }

    public enum LeftRight
    {
        BOTTOM_LEFT,
        BOTTOM_RIGHT
    }
}