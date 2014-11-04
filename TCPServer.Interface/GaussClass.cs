using System;
using System.Collections.Generic;
using System.Text;

namespace TCPServer.Interface
{
    /// <summary>
    /// 高斯投影算法
    /// </summary>
    public class GaussClass
    {
        /// <summary>
        /// 中央子午线
        /// </summary>
        public const double MERIDIAN = 123;
        /// <summary>
        /// x偏移量
        /// </summary>
        public const double OFFSET_X = 4.623774;
        /// <summary>
        /// y偏移量
        /// </summary>
        public const double OFFSET_Y = -3.573104;
        /// <summary>
        /// 保存坐标转换结果
        /// </summary>
        public struct stGauss
        {
            /// <summary>
            /// x值
            /// </summary>
            public double Unknow_x;
            /// <summary>
            /// y值
            /// </summary>
            public double Unknow_y;
            /// <summary>
            /// 纬度
            /// </summary>
            public double Unknow_B;
            /// <summary>
            /// 经度
            /// </summary>
            public double Unknow_L;
        }
        /// <summary>
        /// 度带
        /// </summary>
        public enum Zone
        {
            Zone3,
            Zone6
        }
        /// <summary>
        /// 获得当地中央子午线
        /// </summary>
        /// <param name="localLng">当地经度</param>
        /// <param name="zone">度带</param>
        /// <returns>中央子午线</returns>
        public static double GetCentralMeridian(double localLng, Zone zone)
        {
            double centralMeridian = 0;
            //6度带：中央子午线计算公式：中央子午线L=6 ×（N＋1）－3 。N=[当地经度/6]，N值不进行四舍五入，只取整数部分，（N＋1）即为6度带的带号。
            //3度带：中央子午线计算公式：中央子午线L=3 ×N 。N=当地经度/3，N值进行四舍五入后即为3度带的带号。
            if (zone == Zone.Zone3)
            {
                centralMeridian = 3 * Math.Round(localLng / 3.0);
            }
            else if (zone == Zone.Zone6)
            {
                centralMeridian = 6 * (((int)localLng / 6) + 1) - 3;
            }
            return centralMeridian;
        }
        /// <summary>
        /// 高斯正算，由经纬度转换到XY，54坐标系下转换
        /// </summary>
        /// <param name="doubleB">纬度</param>
        /// <param name="doubleL">经度</param>
        /// <param name="doubleMeridian">中央子午线</param>
        /// <param name="offset_X">x偏移量</param>
        /// <param name="offset_Y">y偏移量</param>
        /// <returns></returns>
        public static stGauss PositiveChange(double doubleB, double doubleL, double doubleMeridian, double offset_X, double offset_Y)
        {

            //把经纬度、中央子午线转换成秒：
            double B = doubleB * 3600;
            double L = doubleL * 3600;
            double L0 = doubleMeridian * 3600;
            //把纬度转换成弧度：
            double Radian_B = (B / 3600) / 57.2957795130823;
            //参数定义：
            double L1 = (L - L0) / 206264.80624709628;
            double L2 = L1 * L1;
            double b1 = Math.Cos(Radian_B);
            double b2 = b1 * b1;
            double N = 6399698.902 - (21562.267 - (108.973 - 0.612 * b2) * b2) * b2;
            double a0 = 32140.404 - (135.3302 - (0.7092 - 0.004 * b2) * b2) * b2;
            double a4 = (0.25 + 0.00252 * b2) * b2 - 0.04166;
            double a6 = (0.166 * b2 - 0.084) * b2;
            double a3 = (0.3333333 + 0.001123 * b2) * b2 - 0.1666667;
            double a5 = 0.0083 - (0.1667 - (0.1968 + 0.004 * b2) * b2) * b2;
            stGauss myGauss = new stGauss();
            //高斯公式：
            myGauss.Unknow_x = 6367558.4969 * B / 206264.80624709628 - (a0 - (0.5 + (a4 + a6 * L2) * L2) * L2 * N) * Math.Sin(Radian_B) * b1 + offset_X;
            myGauss.Unknow_y = (1 + (a3 + a5 * L2) * L2) * L1 * N * b1 + offset_Y + 500000;
            return myGauss;
        }

        public static stGauss PositiveChange(double doubleB, double doubleL, double offset_X, double offset_Y)
        {
            double meridian = GetCentralMeridian(doubleL, Zone.Zone6);
            return PositiveChange(doubleB, doubleL, meridian, offset_X, offset_Y);

        }
        /// <summary>
        /// 高斯反算，由XY转换到经纬度，54坐标系下转换
        /// </summary>
        /// <param name="x">x值</param>
        /// <param name="y">y值</param>
        /// <param name="doubleMeridian">中央子午线</param>
        /// <param name="offset_X">x偏移量</param>
        /// <param name="offset_Y">y偏移量</param>
        /// <returns></returns>
        public static stGauss NegativeChange(double x, double y, double doubleMeridian, double offset_X, double offset_Y)
        {
            y = y % 1000000;
            double L0 = doubleMeridian;
            double c = (x - offset_X) * 206264.80624709628 / 6367558.4969;
            double Radian_c = (c / 3600) / 57.2957795130823;
            double c1 = Math.Cos(Radian_c);
            double c12 = c1 * c1;
            double A = c + (50221746 + (293622 + (2350 + 22 * c12) * c12) * c12) * 0.0000000001 * Math.Sin(Radian_c) * c1 * 206264.80624709628;
            double Radian_A = (A / 3600) / 57.2957795130823;
            double A1 = Math.Cos(Radian_A);
            double A12 = A1 * A1;
            double N = 6399698.902 - (21562.267 - (108.973 - 0.612 * A12) * A12) * A12;
            double Z = (y - offset_Y - 500000) / (N * A1);
            double Z2 = Z * Z;
            double b2 = (0.5 + 0.003369 * A12) * A1 * Math.Sin(Radian_A);
            double b3 = 0.333333 - (0.166667 - 0.001123 * A12) * A12;
            double b4 = 0.25 + (0.16161 + 0.00562 * A12) * A12;
            double b5 = 0.2 - (0.1667 - 0.0088 * A12) * A12;
            double L1 = (1 - (b3 - b5 * Z2) * Z2) * Z * 206264.80624709628;
            double L = L1 / 3600;
            double angle_B = A - (1 - (b4 - 0.12 * Z2) * Z2) * Z2 * b2 * 206264.80624709628;
            stGauss myGauss = new stGauss();
            myGauss.Unknow_B = angle_B / 3600;
            myGauss.Unknow_L = L0 + L;
            return myGauss;
        }

        #region 此算法有问题
        ///// <summary>
        ///// 【bj1954 ==> wgs84】通过高斯投影算法从1954年北京大地坐标转换到经纬度坐标
        ///// </summary>
        ///// <param name="X">北京54坐标的X坐标</param>
        ///// <param name="Y">北京54坐标的Y坐标</param>
        ///// <param name="longitude">转换后的经度</param>
        ///// <param name="latitude">转换后的纬度</param>
        //public static void GaussProjInvCal(double X, double Y, out double longitude, out double latitude)
        //{
        //    int ProjNo;
        //    int ZoneWide;////带宽  
        //    double longitude1, latitude1, longitude0, X0, Y0, xval, yval; //latitude0,
        //    double e1, e2, f, a, ee, NN, T, C, M, D, R, u, fai, iPI;
        //    iPI = 0.0174532925199433;////3.1415926535898/180.0; 
        //    a = 6378245.0; f = 1.0 / 298.3;//54年北京坐标系参数 
        //    ////a=6378140.0; f=1/298.257;//80年西安坐标系参数 
        //    ZoneWide = 6; ////6度带宽  
        //    ProjNo = (int)(X / 1000000L);  //查找带号  
        //    longitude0 = (ProjNo - 1) * ZoneWide + ZoneWide / 2;
        //    longitude0 = longitude0 * iPI;  //中央经线
        //    X0 = ProjNo * 1000000L + 500000L;
        //    Y0 = 0;
        //    xval = X - X0; yval = Y - Y0; //带内大地坐标 
        //    e2 = 2 * f - f * f;
        //    e1 = (1.0 - Math.Sqrt(1 - e2)) / (1.0 + Math.Sqrt(1 - e2));
        //    ee = e2 / (1 - e2);
        //    M = yval;
        //    u = M / (a * (1 - e2 / 4 - 3 * e2 * e2 / 64 - 5 * e2 * e2 * e2 / 256));
        //    fai = u + (3 * e1 / 2 - 27 * e1 * e1 * e1 / 32) * Math.Sin(2 * u) + (21 * e1 * e1 / 16 - 55 * e1 * e1 * e1 * e1 / 32) * Math.Sin(4 * u) + (151 * e1 * e1 * e1 / 96) * Math.Sin(6 * u) + (1097 * e1 * e1 * e1 * e1 / 512) * Math.Sin(8 * u);
        //    C = ee * Math.Cos(fai) * Math.Cos(fai);
        //    T = Math.Tan(fai) * Math.Tan(fai);
        //    NN = a / Math.Sqrt(1.0 - e2 * Math.Sin(fai) * Math.Sin(fai));
        //    R = a * (1 - e2) / Math.Sqrt((1 - e2 * Math.Sin(fai) * Math.Sin(fai)) * (1 - e2 * Math.Sin(fai) * Math.Sin(fai)) * (1 - e2 * Math.Sin(fai) * Math.Sin(fai)));
        //    D = xval / NN;
        //    //计算经度(Longitude)纬度(Latitude) 
        //    longitude1 = longitude0 + (D - (1 + 2 * T + C) * D * D * D / 6 + (5 - 2 * C + 28 * T - 3 * C * C + 8 * ee + 24 * T * T) * D * D * D * D * D / 120) / Math.Cos(fai);
        //    latitude1 = fai - (NN * Math.Tan(fai) / R) * (D * D / 2 - (5 + 3 * T + 10 * C - 4 * C * C - 9 * ee) * D * D * D * D / 24 + (61 + 90 * T + 298 * C + 45 * T * T - 256 * ee - 3 * C * C) * D * D * D * D * D * D / 720);
        //    //转换为度DD  
        //    longitude = longitude1 / iPI;
        //    latitude = latitude1 / iPI;
        //}
        ///// <summary>
        ///// 【wgs84 ==> bj1954】通过高斯投影算法从经纬度坐标转换到1954年北京大地坐标
        ///// </summary>
        ///// <param name="longitude">经度</param>
        ///// <param name="latitude">纬度</param>
        ///// <param name="X">转换后的北京54坐标的X坐标</param>
        ///// <param name="Y">转换后的北京54坐标的Y坐标</param>
        //public static void GaussProjCal(double longitude, double latitude, out double X, out double Y)
        //{
        //    int ProjNo = 0; int ZoneWide; ////带宽  
        //    double longitude1, latitude1, longitude0, X0, Y0, xval, yval;//latitude0,
        //    double a, f, e2, ee, NN, T, C, A, M, iPI;
        //    iPI = 0.0174532925199433; ////3.1415926535898/180.0; 
        //    ZoneWide = 6; ////6度带宽 
        //    a = 6378245.0; f = 1.0 / 298.3; //54年北京坐标系参数 
        //    ////a=6378140.0; f=1/298.257; //80年西安坐标系参数 
        //    ProjNo = (int)(longitude / ZoneWide);
        //    longitude0 = ProjNo * ZoneWide + ZoneWide / 2;
        //    longitude0 = longitude0 * iPI;
        //    //latitude0 = 0;
        //    longitude1 = longitude * iPI;  //经度转换为弧度
        //    latitude1 = latitude * iPI;  //纬度转换为弧度 
        //    e2 = 2 * f - f * f;
        //    ee = e2 * (1.0 - e2);
        //    NN = a / Math.Sqrt(1.0 - e2 * Math.Sin(latitude1) * Math.Sin(latitude1));
        //    T = Math.Tan(latitude1) * Math.Tan(latitude1);
        //    C = ee * Math.Cos(latitude1) * Math.Cos(latitude1);
        //    A = (longitude1 - longitude0) * Math.Cos(latitude1);
        //    M = a * ((1 - e2 / 4 - 3 * e2 * e2 / 64 - 5 * e2 * e2 * e2 / 256) * latitude1 - (3 * e2 / 8 + 3 * e2 * e2 / 32 + 45 * e2 * e2 * e2 / 1024) * Math.Sin(2 * latitude1) + (15 * e2 * e2 / 256 + 45 * e2 * e2 * e2 / 1024) * Math.Sin(4 * latitude1) - (35 * e2 * e2 * e2 / 3072) * Math.Sin(6 * latitude1));
        //    xval = NN * (A + (1 - T + C) * A * A * A / 6 + (5 - 18 * T + T * T + 72 * C - 58 * ee) * A * A * A * A * A / 120);
        //    yval = M + NN * Math.Tan(latitude1) * (A * A / 2 + (5 - T + 9 * C + 4 * C * C) * A * A * A * A / 24 + (61 - 58 * T + T * T + 600 * C - 330 * ee) * A * A * A * A * A * A / 720);
        //    X0 = 1000000L * (ProjNo + 1) + 500000L;
        //    Y0 = 0;
        //    xval = xval + X0;
        //    yval = yval + Y0;
        //    X = xval;
        //    Y = yval;
        //}
        #endregion

        public static void CoordinateConvert(double longitude, double latitude, out double longitudeC, out double latitudeC)
        {
            double X, Y, divX, divY;
            double centralMeridian = GetCentralMeridian(longitude, Zone.Zone6);
            stGauss gauss54 = PositiveChange(latitude, longitude, centralMeridian, 0, 0);
            X = gauss54.Unknow_x;
            Y = gauss54.Unknow_y;
            //坐标系平移到中心点后的坐标
            //divX = X - Configure.SignalConfigure.CenterX;
            //divY = Y - Configure.SignalConfigure.CenterY;
            //平移后该点所在象限,注意北京54坐标横向是Y坐标，纵向是X坐标
            //int quadrant = JudgeQuadrant(divY, divX);
            //int diffAngle = DifferenceAngle(quadrant);
            //平移后该点的角度
            //double alpha = diffAngle;
            //if (divY != 0)
            //    alpha = (Math.Atan(divX / divY) * 180.0) / Math.PI + diffAngle;
            //旋转后该点坐标
            //double convertX = Math.Sqrt(divX * divX + divY * divY) * Math.Sin((alpha - SystemParam.Configure.SignalConfigure.RotationAngle) * Math.PI / 180.0);
            //double convertY = Math.Sqrt(divX * divX + divY * divY) * Math.Cos((alpha - SystemParam.Configure.SignalConfigure.RotationAngle) * Math.PI / 180.0);
            //平移回地球坐标系
            //double mX = Configure.SignalConfigure.CenterX + convertX;
            //double mY = Configure.SignalConfigure.CenterY + convertY;
            //转换回经纬度坐标
            //stGauss gauss84 = NegativeChange(mX, mY, centralMeridian, 0, 0);
            //longitudeC = gauss84.Unknow_L;
            //latitudeC = gauss84.Unknow_B;
            longitudeC = 0;
            latitudeC = 0;
        }
        private static int JudgeQuadrant(double x, double y)
        {
            int quadrant = 0;
            if (x > 0 && y > 0)
            {
                quadrant = 1;
            }
            if (x < 0 && y > 0)
            {
                quadrant = 2;
            }
            if (x < 0 && y < 0)
            {
                quadrant = 3;
            }
            if (x > 0 && y < 0)
            {
                quadrant = 4;
            }
            if (x == 0 && y > 0)
            {
                quadrant = 12;
            }
            if (x < 0 && y == 0)
            {
                quadrant = 23;
            }
            if (x == 0 && y < 0)
            {
                quadrant = 34;
            }
            if (x > 0 && y == 0)
            {
                quadrant = 41;
            }

            return quadrant;
        }
        private static int DifferenceAngle(int quadrant)
        {
            int diffAngle = 0;
            switch (quadrant)
            {
                case 41:
                case 1:
                    diffAngle = 0;
                    break;
                case 23:
                case 2:
                case 3:
                    diffAngle = 180;
                    break;
                case 4:
                    diffAngle = 360;
                    break;
                case 12:
                    diffAngle = 90;
                    break;
                case 34:
                    diffAngle = 270;
                    break;
            }
            return diffAngle;
        }

        private const double EARTH_RADIUS = 6378.137;//地球半径
        private static double Rad(double d)
        {
            return d * Math.PI / 180.0;
        }
        /// <summary>
        /// 计算两点之间的距离
        /// </summary>
        /// <param name="lat1">点1纬度</param>
        /// <param name="lng1">点1经度</param>
        /// <param name="lat2">点2纬度</param>
        /// <param name="lng2">点2经度</param>
        /// <returns>距离（KM）</returns>
        public static double GetDistance(double lat1, double lng1, double lat2, double lng2)
        {
            double radLat1 = Rad(lat1);
            double radLat2 = Rad(lat2);
            double a = radLat1 - radLat2;
            double b = Rad(lng1) - Rad(lng2);

            double s = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) +
             Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Pow(Math.Sin(b / 2), 2)));
            s = s * EARTH_RADIUS;
            s = Math.Round(s * 10000) / 10000;
            return s;
        }
    }
}
