using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TCPServer
{/// <summary>
    /// GPS坐标，构造函数之后，必须立即调用UpdateBase函数以使用基点对坐标进行修正
    /// </summary>
    public class GPS:MsgBase
    {
        public GPS()
            : base()
        {
        }
        public GPS(int createID)
            : base(createID,false)
        {

        }
        //public GPS(BinaryReader sourceStream)
        //    : base(sourceStream)
        //{
        //    Init(sourceStream);
        //}
        public override void DeSerialize(BinaryReader sourceStream)
        {
            base.DeSerialize(sourceStream);

            Init(sourceStream);
        }
        void Init(BinaryReader sourceStream)
        {
            //this._BaseLng = decimal.Parse(SystemParam.Configure.SignalConfigure.BaseLng.ToString("F6"));
            //this._BaseLat = decimal.Parse(SystemParam.Configure.SignalConfigure.BaseLat.ToString("F6"));
            //this._BaseHeight = decimal.Parse(SystemParam.Configure.SignalConfigure.BaseHeight.ToString("F6"));

            _CarID = sourceStream.ReadUInt32();
            byte b1,b2,b3;
            b1 = sourceStream.ReadByte();
            b2 = sourceStream.ReadByte();
            b3 = sourceStream.ReadByte();
            _Lng = ((decimal)(b1 + b2 * 0x100 + b3 * 0x10000)) / 10000000 + _BaseLng;
            b1 = sourceStream.ReadByte();
            b2 = sourceStream.ReadByte();
            b3 = sourceStream.ReadByte();
            _Lat = ((decimal)(b1 + b2 * 0x100 + b3 * 0x10000)) / 10000000 + _BaseLat;
            b1 = sourceStream.ReadByte();
            b2 = sourceStream.ReadByte();
            _Height = ((decimal)(b1 + b2 * 0x100)) / 100 + _BaseLat;

            _Speed = sourceStream.ReadByte();
            _Directtion = sourceStream.ReadByte() * 2;

            ComputeXY();
        }
        public override void Serialize(BinaryWriter targetStream)
        {
            base.Serialize(targetStream);

            //this._BaseLng = decimal.Parse(SystemParam.Configure.SignalConfigure.BaseLng.ToString("F6"));
            //this._BaseLat = decimal.Parse(SystemParam.Configure.SignalConfigure.BaseLat.ToString("F6"));
            //this._BaseHeight = decimal.Parse(SystemParam.Configure.SignalConfigure.BaseHeight.ToString("F6"));

            decimal dLng = _Lng - _BaseLng;
            decimal dLat = _Lat - _BaseLat;
            decimal dH = _Height - _BaseHeight;
            int iLng = (int)(dLng * 10000000);//精确到经纬度后第7位小数
            int iLat = (int)(dLat * 10000000);
            int iHeight = (int)(dH * 100);//精确到厘米
            byte speed = (byte)_Speed;//速度
            byte direct = (byte)(_Directtion / 2);//方向
            targetStream.Write(_CarID);
            targetStream.Write((byte)(iLng & 0xFF));
            targetStream.Write((byte)((iLng & 0xFF00) >> 8));
            targetStream.Write((byte)((iLng & 0xFF0000) >> 16));
            targetStream.Write((byte)(iLat & 0xFF));
            targetStream.Write((byte)((iLat & 0xFF00) >> 8));
            targetStream.Write((byte)((iLat & 0xFF0000) >> 16));
            targetStream.Write((byte)(iHeight & 0xFF));
            targetStream.Write((byte)((iHeight & 0xFF00) >> 8));
            targetStream.Write(speed);
            targetStream.Write(direct);
        }
        /// <summary>
        /// 更新GPS类的基点经度、纬度、高程，【经纬度精确到小数点后第7位，第8位将被忽略，高程精确到小数点第2位】
        /// </summary>
        /// <param name="baseLng">基点经度</param>
        /// <param name="baseLat">基点纬度</param>
        /// <param name="baseHeight">基点高程</param>
        public void UpdateBase(double baseLng,double baseLat,double baseHeight)
        {
            decimal d1,d2,d3;
            d1 = decimal.Parse(baseLng.ToString("0.0000000"));
            d2 = decimal.Parse(baseLat.ToString("0.0000000"));
            d3 = decimal.Parse(baseHeight.ToString("0.00"));
            _Lng += (d1 - _BaseLng);
            _Lat += (d2 - _BaseLat);
            _Height += (d3 - _BaseHeight);

            _BaseLng = d1;
            _BaseLat = d2;
            _BaseHeight = d3;

            ComputeXY();
        }
        /// <summary>
        /// 更新GPS数据
        /// </summary>
        /// <param name="carID">车辆ID</param>
        /// <param name="lng">当前经度</param>
        /// <param name="lat">当前纬度</param>
        /// <param name="height">当前高程</param>
        /// <param name="speed">当前速度</param>
        /// <param name="direct">当前方向，正北为0度，正东为90度，正南为180度</param>
        public void Update(UInt32 carID,double lng,double lat,double height,int speed,int direct)
        {
            _CarID = carID;
            _Lng = decimal.Parse(lng.ToString("0.0000000"));
            _Lat = decimal.Parse(lat.ToString("0.0000000"));
            _Height = decimal.Parse(height.ToString("0.00"));
            _Speed = speed;
            _Directtion = direct;

            ComputeXY();
            //ComputeConvert();
        }
        /// <summary>
        /// GPS坐标进行机场内部旋转
        /// </summary>
        public void ComputeConvert()
        {
            double meridian = GaussClass.GetCentralMeridian(double.Parse(this._Lng.ToString("F6")),GaussClass.Zone.Zone6);
            //取得周边机场编辑区域
            GaussClass.stGauss adsb54 = GaussClass.PositiveChange(double.Parse(this._Lat.ToString()),double.Parse(this._Lng.ToString()),meridian,0,0);
            System.Drawing.Point gpsPoint = new System.Drawing.Point((int)adsb54.Unknow_y,(int)adsb54.Unknow_x);
            double convertLng,convertLat;
            //if (JudgeInRect(SystemParam.Configure.SignalConfigure.AirporBorder[0].X, SystemParam.Configure.SignalConfigure.AirporBorder[0].Y,
            //    SystemParam.Configure.SignalConfigure.AirporBorder[1].X, SystemParam.Configure.SignalConfigure.AirporBorder[1].Y, gpsPoint.X, gpsPoint.Y))
            //{
            //    GaussClass.CoordinateConvert(double.Parse(this._Lng.ToString("F6")), double.Parse(this._Lat.ToString("F6")),
            //                            out convertLng, out convertLat);
            //    this._OriginalLng = this._Lng;
            //    this._Lng = decimal.Parse(convertLng.ToString("F6"));
            //    this._OriginalLat = this._Lat;
            //    this._Lat = decimal.Parse(convertLat.ToString("F6"));
            //    this._OriginalDirection = this._Directtion;
            //    this._Directtion = (this._Directtion + 90 > 360) ? this._Directtion + 90 - 360 : this._Directtion + 90;
            //}
            //else
            {
                this._OriginalLng = this._Lng;
                this._OriginalLat = this._Lat;
                this._OriginalDirection = this._Directtion;
            }
        }
        /// <summary>
        /// 计算北京54坐标
        /// </summary>
        void ComputeXY()
        {
            double d1 = (double)_Lng,d2 = (double)_Lat;
            double dd1,dd2;
            double meridian = GaussClass.GetCentralMeridian(d1,GaussClass.Zone.Zone6);
            GaussClass.stGauss guass = GaussClass.PositiveChange(d2,d1,meridian,0,0);
            dd1 = guass.Unknow_x;
            dd2 = guass.Unknow_y;
            _X = decimal.Parse(dd1.ToString("0.000"));
            _Y = decimal.Parse(dd2.ToString("0.000"));

            ComputeOffsetXY();
        }
        /// <summary>
        /// 计算北京54坐标基于基点的偏移量
        /// </summary>
        void ComputeOffsetXY()
        {
            double d1 = (double)_BaseLng,d2 = (double)_BaseLat;
            double dd1,dd2;
            double meridian = GaussClass.GetCentralMeridian(d1,GaussClass.Zone.Zone6);
            GaussClass.stGauss guass = GaussClass.PositiveChange(d2,d1,meridian,0,0);
            dd1 = guass.Unknow_x;
            dd2 = guass.Unknow_y; ;
            _OffsetX = decimal.Parse(dd1.ToString("0.000"));
            _OffsetY = decimal.Parse(dd2.ToString("0.000"));
            _OffsetX = _X - _OffsetX;
            _OffsetY = _Y - _OffsetY;
        }

        public bool JudgeInRect(double minPtx,double minPty,double maxPtx,double maxPty,double ptx,double pty)
        {
            if(ptx >= minPtx && ptx <= maxPtx && pty >= minPty && pty <= maxPty) {
                return true;
            } else {
                return false;
            }
        }

        UInt32 _CarID=1201;
        decimal _BaseLng;
        decimal _BaseLat;
        decimal _BaseHeight;
        decimal _Lng;
        decimal _Lat;
        decimal _Height;
        int _Speed=100;
        int _Directtion=270;
        decimal _X;
        decimal _Y;
        decimal _OffsetX,_OffsetY;
        decimal _OriginalLng = (decimal)40.034386,_OriginalLat = (decimal)124.287201,_OriginalDirection = 123;
        //decimal _C
        /// <summary>
        /// 车辆编码
        /// </summary>
        public UInt32 CarID { set { _CarID=value; } get { return _CarID; } }
        /// <summary>
        /// 【只读】基点经度
        /// </summary>
        public decimal BaseLng { get { return _BaseLng; } }
        /// <summary>
        /// 【只读】基点纬度
        /// </summary>
        public decimal BaseLat { get { return _BaseLat; } }
        /// <summary>
        /// 【只读】基点高程
        /// </summary>
        public decimal BaseHeight { get { return _BaseHeight; } }
        /// <summary>
        /// 【只读】实际经度
        /// </summary>
        public decimal Lng { set; get; }
        /// <summary>
        /// 【只读】实际纬度
        /// </summary>
        public decimal Lat { set; get; }
        /// <summary>
        /// 【只读】速度
        /// </summary>
        public int Speed { set; get; }
        /// <summary>
        /// 【只读】方向
        /// </summary>
        public int Direction { set; get; }
        /// <summary>
        /// 【只读】实际海拔高度
        /// </summary>
        public decimal Height { set; get; }
        /// <summary>
        /// 【只读】北京54坐标X
        /// </summary>
        public decimal X { get { return _X; } }
        /// <summary>
        /// 【只读】北京54坐标Y
        /// </summary>
        public decimal Y { get { return _Y; } }
        /// <summary>
        /// 【只读】北京54坐标X偏移量
        /// </summary>
        public decimal OffsetX { get { return _OffsetX; } }
        /// <summary>
        /// 【只读】北京54坐标Y偏移量
        /// </summary>
        public decimal OffsetY { get { return _OffsetY; } }
        /// <summary>
        /// 【只读】原始的经度
        /// </summary>
        public decimal OriginalLng { set; get; }
        /// <summary>
        /// 【只读】原始的纬度
        /// </summary>
        public decimal OriginalLat { set; get; }
        /// <summary>
        /// 【只读】原始的方向
        /// </summary>
        public decimal OriginalDirection { set; get; }
        /// <summary>
        /// 【已重载】将GPS对象输出到字符串表示形式
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            //return base.ToString();
            return "CID=" + this.CreaterID.ToString() + ",CarID=" + this.CarID.ToString() + ",Lng=" + this.Lng.ToString() + ",Lat=" + this.Lat.ToString() +
                ",bj54=[" + this.X.ToString() + "," + this.Y.ToString() + "(" + this.OffsetX.ToString() + "," + this.OffsetY.ToString() + ")]" +
                ",Height=" + this.Height.ToString() + ",Speed=" + this.Speed.ToString() + ",Dir=" + this.Direction.ToString();
        }
    }

}
