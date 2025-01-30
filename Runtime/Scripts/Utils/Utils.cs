namespace Preliy.Flange
{
    public static class Utils
    {
        /*public static float ConverterSiToUnit(this float value, Joint.JointType jointType, Joint.JointValueUnit unit)
        {
            return jointType switch
            {
                Joint.JointType.Rotation => value,
                Joint.JointType.Displacement => unit switch
                {
                    Joint.JointValueUnit.Meter => value,
                    Joint.JointValueUnit.Millimeter => value * Math.MillimeterToMeter,
                    _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, null)
                },
                _ => throw new ArgumentOutOfRangeException(nameof(jointType), jointType, null)
            };
        }
        
        public static float ConverterUnitToSi(this float value, Joint.JointType jointType, Joint.JointValueUnit unit)
        {
            return jointType switch
            {
                Joint.JointType.Rotation => value,
                Joint.JointType.Displacement => unit switch
                {
                    Joint.JointValueUnit.Meter => value,
                    Joint.JointValueUnit.Millimeter => value / Math.MillimeterToMeter,
                    _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, null)
                },
                _ => throw new ArgumentOutOfRangeException(nameof(jointType), jointType, null)
            };
        }*/

        public static float[] CopyArray(this float[] value)
        {
            var array = new float[value.Length];
            value.CopyTo(array,0);
            return array;
        }
    }
}
