using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace TCPServer
{
    /// <summary>
    /// 消息对象基础类，所有需要通过通讯类传输到远端的数据必须封装成MsgBase的子类，
    /// </summary>
    public class MsgBase
    {
        /// <summary>
        /// 此消息是否是回复消息
        /// </summary>
        private bool _isReplay;
        public bool IsReplay
        {
            get { return _isReplay; }
        }
        /// <summary>
        /// 创建者ID，有效值 0-65535（2个字节有效数据）
        /// </summary>
        public int CreaterID;
        /// <summary>
        /// 无参数构造函数，最好不要直接使用无参数构造函数实例化类，
        /// 因该构造函数会导致CreaterID和IsReplay无法被赋值，该函数仅供反序列化调用
        /// </summary>
        public MsgBase() { CreaterID = 0; _isReplay = false; }
        /// <summary>
        /// 通过创建者ID实例化类
        /// </summary>
        /// <param name="createrID">创建者ID</param>
        /// <param name="isReplay">当前消息是否是回复消息</param>
        public MsgBase(int createrID, bool isReplay)
        {
            CreaterID = Math.Abs(createrID % 0x10000);
            _isReplay = isReplay;
        }
        /// <summary>
        /// 从数据流执行构造函数
        /// [封闭]因NetCF2.0无法通过有参数构造函数反射类的实例，故封闭该函数，改用LoadData初始化
        /// </summary>
        /// <param name="sourceStream">从目标流读取当前内的数据</param>
        private MsgBase(BinaryReader sourceStream)
        {
            CreaterID = sourceStream.ReadByte() + sourceStream.ReadByte() * 0x100;
        }
        /// <summary>
        /// 从数据流载入消息数据
        /// [注：该虚拟函数为兼容NetCF2.0需要，因NetCF2.0不支持通过反射带参数实例化类]
        /// </summary>
        /// <param name="sourceStream"></param>
        public virtual void DeSerialize(BinaryReader sourceStream)
        {
            CreaterID = sourceStream.ReadByte() + sourceStream.ReadByte() * 0x100;
            _isReplay = sourceStream.ReadBoolean();
        }
        /// <summary>
        /// 序列化函数
        /// </summary>
        /// <param name="targetStream">将当前类的数据写入到目标流</param>
        public virtual void Serialize(BinaryWriter targetStream)
        {
            byte h, l;
            l = (byte)(CreaterID & 0xFF);
            h = (byte)((CreaterID & 0xFF00) >> 8);
            targetStream.Write(l);
            targetStream.Write(h);
            targetStream.Write(IsReplay);
        }
    }
    public class MsgText : MsgBase
    {
        public string Text = "";
        public MsgText() : base() { }
        public MsgText(int createID, string msgText)
            : base(createID, false)
        {
            Text = msgText;
        }
        //public MsgText(BinaryReader sourceStream)
        //    : base(sourceStream)
        //{
        //    Text = sourceStream.ReadString();
        //}
        public override void Serialize(BinaryWriter targetStream)
        {
            base.Serialize(targetStream);
            targetStream.Write(Text);
        }
        public override void DeSerialize(BinaryReader sourceStream)
        {
            base.DeSerialize(sourceStream);
            Text = sourceStream.ReadString();
        }
    }
}
