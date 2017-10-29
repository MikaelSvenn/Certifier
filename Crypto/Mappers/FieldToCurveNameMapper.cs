using System.Collections.Generic;
using System.Linq;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.EC;
using Org.BouncyCastle.Math.EC;

namespace Crypto.Mappers
{
    public class FieldToCurveNameMapper
    {
        public string MapCurveToName(ECCurve curve)
        {
            IEnumerable<string> curveNames = ECNamedCurveTable.Names.Cast<string>().Concat(CustomNamedCurves.Names.Cast<string>());
            foreach (string curveName in curveNames)
            {
                X9ECParameters curveParameters = ECNamedCurveTable.GetByName(curveName) ?? CustomNamedCurves.GetByName(curveName);
                if (curve.Equals(curveParameters.Curve))
                {
                    return curveName;
                }
            }
            
            return "unknown";
        }
    }
}