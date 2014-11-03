using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPServer
{
    public class ChinaMapShift
    {
        public struct Location
        {
            public double Lng;
            public double Lat;
        }

        public static ChinaMapShift MapShift = new ChinaMapShift();

        private const double pi = 3.14159265358979324;
        private const double a = 6378245.0;
        // Krasovsky 1940
        //
        // a = 6378245.0, 1/f = 298.3
        // b = a * (1 - f)
        // ee = (a^2 - b^2) / a^2;
        private const double ee = 0.00669342162296594323;
        private const double x_pi = 3.14159265358979324 * 3000.0 / 180.0;

        private Location _LocationMake(double lng,double lat)
        {
            Location loc = new Location();
            loc.Lng = lng;
            loc.Lat = lat;

            return loc;
        }

        private bool OutOfChina(double lat,double lon)
        {
            if(lon < 72.004 || lon > 137.8347)
                return true;
            if(lat < 0.8293 || lat > 55.8271)
                return true;
            return false;
        }

        private double TransformLat(double x,double y)
        {
            double ret = -100.0 + 2.0 * x + 3.0 * y + 0.2 * y * y + 0.1 * x * y + 0.2 * Math.Sqrt(x > 0 ? x : -x);
            ret += (20.0 * Math.Sin(6.0 * x * pi) + 20.0 * Math.Sin(2.0 * x * pi)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(y * pi) + 40.0 * Math.Sin(y / 3.0 * pi)) * 2.0 / 3.0;
            ret += (160.0 * Math.Sin(y / 12.0 * pi) + 320 * Math.Sin(y * pi / 30.0)) * 2.0 / 3.0;
            return ret;
        }

        private double TransformLon(double x,double y)
        {
            double ret = 300.0 + x + 2.0 * y + 0.1 * x * x + 0.1 * x * y + 0.1 * Math.Sqrt(x > 0 ? x : -x);
            ret += (20.0 * Math.Sin(6.0 * x * pi) + 20.0 * Math.Sin(2.0 * x * pi)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(x * pi) + 40.0 * Math.Sin(x / 3.0 * pi)) * 2.0 / 3.0;
            ret += (150.0 * Math.Sin(x / 12.0 * pi) + 300.0 * Math.Sin(x / 30.0 * pi)) * 2.0 / 3.0;
            return ret;
        }

        private Location _TransformFromWGSToGCJ(Location wgsLoc)
        {
            Location gcjLoc;
            if(OutOfChina(wgsLoc.Lat,wgsLoc.Lng)) {
                gcjLoc = wgsLoc;
                return gcjLoc;
            }
            double dLat = TransformLat(wgsLoc.Lng - 105.0,wgsLoc.Lat - 35.0);
            double dLon = TransformLon(wgsLoc.Lng - 105.0,wgsLoc.Lat - 35.0);
            double radLat = wgsLoc.Lat / 180.0 * pi;
            double magic = Math.Sin(radLat);
            magic = 1 - ee * magic * magic;
            double sqrtMagic = Math.Sqrt(magic);
            dLat = (dLat * 180.0) / ((a * (1 - ee)) / (magic * sqrtMagic) * pi);
            dLon = (dLon * 180.0) / (a / sqrtMagic * Math.Cos(radLat) * pi);
            gcjLoc.Lat = wgsLoc.Lat + dLat;
            gcjLoc.Lng = wgsLoc.Lng + dLon;

            return gcjLoc;
        }

        private Location _TransformFromGCJToWGS(Location gcjLoc)
        {
            Location wgsLoc = gcjLoc;
            Location currGcjLoc,dLoc;
            while(true) {
                currGcjLoc = _TransformFromWGSToGCJ(wgsLoc);
                dLoc.Lat = gcjLoc.Lat - currGcjLoc.Lat;
                dLoc.Lng = gcjLoc.Lng - currGcjLoc.Lng;
                if(Math.Abs(dLoc.Lat) < 1e-7 && Math.Abs(dLoc.Lng) < 1e-7) {
                    // 1e-7 ~ centimeter level accuracy
                    // Result of experiment:
                    //   Most of the time 2 iterations would be enough for an 1e-8 accuracy (milimeter level).
                    //
                    return wgsLoc;
                }
                wgsLoc.Lat += dLoc.Lat;
                wgsLoc.Lng += dLoc.Lng;
            }

            return wgsLoc;
        }


        private Location _TransformFromGCJToBD(Location gcjLoc)
        {
            double x = gcjLoc.Lng,y = gcjLoc.Lat;
            double z = Math.Sqrt(x * x + y * y) + 0.00002 * Math.Sin(y * x_pi);
            double theta = Math.Atan2(y,x) + 0.000003 * Math.Cos(x * x_pi);
            return LocationMake(z * Math.Cos(theta) + 0.0065,z * Math.Sin(theta) + 0.006);
        }

        private Location _TransformFromBDToGCJ(Location bdLoc)
        {
            double x = bdLoc.Lng - 0.0065,y = bdLoc.Lat - 0.006;
            double z = Math.Sqrt(x * x + y * y) - 0.00002 * Math.Sin(y * x_pi);
            double theta = Math.Atan2(y,x) - 0.000003 * Math.Cos(x * x_pi);
            return LocationMake(z * Math.Cos(theta),z * Math.Sin(theta));
        }

        public static Location LocationMake(double lng,double lat)
        {
            return MapShift._LocationMake(lng,lat);
        }
        public static Location TransformFromWGSToGCJ(Location wgsLoc)
        {
            return MapShift._TransformFromWGSToGCJ(wgsLoc);
        }

        public static Location TransformFromGCJToWGS(Location gcjLoc)
        {
            return MapShift._TransformFromGCJToWGS(gcjLoc);
        }

        public static Location TransformFromGCJToBD(Location gcjLoc)
        {
            return MapShift._TransformFromGCJToBD(gcjLoc);
        }

        public static Location TransformFromBDToGCJ(Location bdLoc)
        {
            return MapShift._TransformFromBDToGCJ(bdLoc);
        }
    }
}
