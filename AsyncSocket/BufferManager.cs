using System.Collections.Generic;
using System.Net.Sockets;

namespace AsyncSockets
{
    /// <summary>
    /// ����ഴ��һ�����Ա����ֺͷ����SocketAsyncEventArgs����ÿ�β�������ʹ�õĵ����Ĵ�Ļ�����
    /// �����ʹ�����������׵ر��ظ�ʹ�ò��ҷ�ֹ���ڴ��жѻ���Ƭ��    
    /// BufferManager �౩¶�Ĳ��������̰߳�ȫ��(��Ҫ���̰߳�ȫ����)
    /// </summary>
    class BufferManager
    {
        int m_numBytes;                 // ���������ع������ֽ�����
        byte[] m_buffer;                // ��BufferManagerά�ֵĻ����ֽ�����
        Stack<int> m_freeIndexPool;     // �ͷŵ�������
        int m_currentIndex;             //��ǰ����
        int m_bufferSize;               //��������С

        /// <summary>
        /// ��ʼ����������������
        /// </summary>
        /// <param name="totalBytes">��������������������ֽ�����</param>
        /// <param name="bufferSize">ÿ����������С</param>
        public BufferManager(int totalBytes, int bufferSize)
        {
            m_numBytes = totalBytes;
            m_currentIndex = 0;
            m_bufferSize = bufferSize;
            m_freeIndexPool = new Stack<int>();
        }

        /// <summary>
        /// ���䱻��������ʹ�õĻ������ռ�
        /// </summary>
        public void InitBuffer()
        {
            // ����һ����Ĵ󻺳������һ��ָ�ÿһ��SocketAsyncEventArgs����
            m_buffer = new byte[m_numBytes];
        }

        /// <summary>
        /// �ӻ��������з���һ����������ָ����SocketAsyncEventArgs����
        /// </summary>
        /// <returns>������������ɹ����÷�������򷵻ؼ�</returns>
        public bool SetBuffer(SocketAsyncEventArgs args)
        {

            if (m_freeIndexPool.Count > 0)
            {
                args.SetBuffer(m_buffer, m_freeIndexPool.Pop(), m_bufferSize);
            }
            else
            {
                if ((m_numBytes - m_bufferSize) < m_currentIndex)
                {
                    return false;
                }
                args.SetBuffer(m_buffer, m_currentIndex, m_bufferSize);
                m_currentIndex += m_bufferSize;
            }
            return true;
        }

        /// <summary>
        /// ��һ��SocketAsyncEventArgs������ɾ�����������⽫�ѻ������ͷŻػ�������        
        /// </summary>
        public void FreeBuffer(SocketAsyncEventArgs args)
        {
            m_freeIndexPool.Push(args.Offset);
            args.SetBuffer(null, 0, 0);
        }


    }
}