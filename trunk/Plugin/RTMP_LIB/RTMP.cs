﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RTMP_LIB
{
    public class RTMP
    {
        #region  Constants

        static readonly byte[] DH_MODULUS_BYTES = new byte[128] 
                { 
    	            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xC9, 0x0F, 0xDA, 0xA2, 0x21, 0x68, 0xC2, 0x34, 0xC4, 0xC6, 0x62, 0x8B, 0x80, 0xDC, 0x1C, 0xD1, 0x29, 0x02, 0x4E, 0x08, 0x8A, 0x67, 0xCC, 0x74,
    	            0x02, 0x0B, 0xBE, 0xA6, 0x3B, 0x13, 0x9B, 0x22, 0x51, 0x4A, 0x08, 0x79, 0x8E, 0x34, 0x04, 0xDD, 0xEF, 0x95, 0x19, 0xB3, 0xCD, 0x3A, 0x43, 0x1B, 0x30, 0x2B, 0x0A, 0x6D, 0xF2, 0x5F, 0x14, 0x37, 
    	            0x4F, 0xE1, 0x35, 0x6D, 0x6D, 0x51, 0xC2, 0x45, 0xE4, 0x85, 0xB5, 0x76, 0x62, 0x5E, 0x7E, 0xC6, 0xF4, 0x4C, 0x42, 0xE9, 0xA6, 0x37, 0xED, 0x6B, 0x0B, 0xFF, 0x5C, 0xB6, 0xF4, 0x06, 0xB7, 0xED, 
    	            0xEE, 0x38, 0x6B, 0xFB, 0x5A, 0x89, 0x9F, 0xA5, 0xAE, 0x9F, 0x24, 0x11, 0x7C, 0x4B, 0x1F, 0xE6, 0x49, 0x28, 0x66, 0x51, 0xEC, 0xE6, 0x53, 0x81, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
                };

        const int SHA256_DIGEST_LENGTH = 32;

        public const short RTMP_PROTOCOL_UNDEFINED = -1;
        public const short RTMP_PROTOCOL_RTMP = 0;
        public const short RTMP_PROTOCOL_RTMPT = 1;
        public const short RTMP_PROTOCOL_RTMPS = 2;
        public const short RTMP_PROTOCOL_RTMPE = 3;
        public const short RTMP_PROTOCOL_RTMPTE = 4;
        public const short RTMP_PROTOCOL_RTMFP = 5;

        static readonly byte[] GenuineFPKey = new byte[62] 
        {
            0x47,0x65,0x6E,0x75,0x69,0x6E,0x65,0x20,0x41,0x64,0x6F,0x62,0x65,0x20,0x46,0x6C,
            0x61,0x73,0x68,0x20,0x50,0x6C,0x61,0x79,0x65,0x72,0x20,0x30,0x30,0x31,
            
            0xF0,0xEE,
            0xC2,0x4A,0x80,0x68,0xBE,0xE8,0x2E,0x00,0xD0,0xD1,0x02,0x9E,0x7E,0x57,0x6E,0xEC,
            0x5D,0x2D,0x29,0x80,0x6F,0xAB,0x93,0xB8,0xE6,0x36,0xCF,0xEB,0x31,0xAE
        };

        static readonly byte[] GenuineFMSKey = new byte[68]  
        {
            0x47, 0x65, 0x6e, 0x75, 0x69, 0x6e, 0x65, 0x20, 0x41, 0x64, 0x6f, 0x62, 0x65, 0x20, 0x46, 0x6c,
            0x61, 0x73, 0x68, 0x20, 0x4d, 0x65, 0x64, 0x69, 0x61, 0x20, 0x53, 0x65, 0x72, 0x76, 0x65, 0x72,
            0x20, 0x30, 0x30, 0x31, // Genuine Adobe Flash Media Server 001 

            0xf0, 0xee, 0xc2, 0x4a, 0x80, 0x68, 0xbe, 0xe8, 0x2e, 0x00, 0xd0, 0xd1,
            0x02, 0x9e, 0x7e, 0x57, 0x6e, 0xec, 0x5d, 0x2d, 0x29, 0x80, 0x6f, 0xab, 0x93, 0xb8, 0xe6, 0x36,
            0xcf, 0xeb, 0x31, 0xae 
        };

        const int RTMP_PACKET_SIZE_LARGE   = 0;
        const int RTMP_PACKET_SIZE_MEDIUM  = 1;
        const int RTMP_PACKET_SIZE_SMALL   = 2;
        const int RTMP_PACKET_SIZE_MINIMUM = 3;
        const int RTMP_LARGE_HEADER_SIZE = 12;
        const int RTMP_SIG_SIZE = 1536;
        static readonly uint[] packetSize = { 12, 8, 4, 1 };

        #endregion

        TcpClient tcpClient = null;
        NetworkStream networkStream = null;

        Link Link = new Link();        

        int bytesReadTotal = 0;
        int lastSentBytesRead = 0;

        public double Duration { get; protected set; } // duration of stream in seconds returned by Metadata
        public int ChunkSize { get; protected set; } // current ChunkSize, defaults to 128
        public bool Playing { get; protected set; } // indicates if currently streaming a media file
        public long CombinedTracksLength { get; protected set; }
        public long CombinedBitrates { get; protected set; }

        int m_nBufferMS = 300;        
        int m_stream_id; // returned in _result from invoking createStream            
        int m_nBWCheckCounter;        
        RTMPPacket[] m_vecChannelsIn = new RTMPPacket[64];
        RTMPPacket[] m_vecChannelsOut = new RTMPPacket[64];
        uint[] m_channelTimestamp = new uint[64]; // abs timestamp of last packet
        Stack<string> m_methodCalls = new Stack<string>(); //remote method calls queue

        public bool Connect(Link link)
        {
            // close any previous connection
            Close();

            Link = link;
            
            // connect            
            tcpClient = new TcpClient(Link.hostname, Link.port);
            networkStream = tcpClient.GetStream();

            if (!HandShake(Link.protocol == RTMP_PROTOCOL_RTMPE || Link.protocol == RTMP_PROTOCOL_RTMPTE)) return false;
            if (!SendConnectPacket()) return false;

            return true;
        }
        
        public bool IsConnected()
        {
            return tcpClient != null && tcpClient.Connected;
        }

        public bool GetNextMediaPacket(out RTMPPacket packet)
        {
            packet = null;
            bool bHasMediaPacket = false;
            while (!bHasMediaPacket && IsConnected() && ReadPacket(out packet))
            {
                if (!packet.IsReady())
                {                    
                    continue;
                }

                switch (packet.m_packetType)
                {
                    case 0x01:
                        // chunk size
                        HandleChangeChunkSize(packet);
                        break;
                    case 0x03:
                        // bytes read report
                        //CLog::Log(LOGDEBUG,"%s, received: bytes read report", __FUNCTION__);
                        break;
                    case 0x04:
                        // ping
                        HandlePing(packet);
                        break;
                    case 0x05:
                        // server bw
                        //CLog::Log(LOGDEBUG,"%s, received: server BW", __FUNCTION__);
                        break;
                    case 0x06:
                        // client bw
                        //CLog::Log(LOGDEBUG,"%s, received: client BW", __FUNCTION__);
                        break;
                    case 0x08:
                        // audio data
                        //CLog::Log(LOGDEBUG,"%s, received: audio %lu bytes", __FUNCTION__, packet.m_nBodySize);
                        //HandleAudio(packet);
                        bHasMediaPacket = true;
                        break;
                    case 0x09:
                        // video data
                        //CLog::Log(LOGDEBUG,"%s, received: video %lu bytes", __FUNCTION__, packet.m_nBodySize);
                        //HandleVideo(packet);
                        bHasMediaPacket = true;
                        break;
                    case 0x12:
                        // metadata (notify)
                        //CLog::Log(LOGDEBUG,"%s, received: notify %lu bytes", __FUNCTION__, packet.m_nBodySize);
                        HandleMetadata(packet);
                        bHasMediaPacket = true;
                        break;
                    case 0x14:
                        // invoke
                        //CLog::Log(LOGDEBUG,"%s, received: invoke %lu bytes", __FUNCTION__, packet.m_nBodySize);
                        HandleInvoke(packet);
                        break;
                    case 0x16:
                        // FLV tag(s)
                        HandleFlvTags(packet);
                        //Logger.Log(string.Format("received: FLV tag(s) {0} bytes", packet.m_nBodySize));
                        bHasMediaPacket = true;
                        break;
                    default:
                        Logger.Log(string.Format("Unknown packet type received: {0}", packet.m_packetType));
                        break;
                }
                //if (!bHasMediaPacket) packet.FreePacket();
                packet.m_nBytesRead = 0;
            }
            if (bHasMediaPacket) Playing = true;
            return bHasMediaPacket;
        }
        
        bool ReadPacket(out RTMPPacket packet)
        {
            // Chunk Basic Header (1, 2 or 3 bytes)
            // the two most significant bits hold the chunk type
            // value in the 6 least significant bits gives the chunk stream id (0,1,2 are reserved): 0 -> 3 byte header | 1 -> 2 byte header | 2 -> low level protocol message | 3-63 -> stream id
            byte type = ReadByte(); bytesReadTotal++;
            byte headerType = (byte)((type & 0xc0) >> 6);
            byte channel = (byte)(type & 0x3f);
            uint nSize = packetSize[headerType];

            //Logger.Log(string.Format("reading RTMP packet chunk on channel {0}, headersz {1}", channel, nSize));

            if (nSize < RTMP_LARGE_HEADER_SIZE)
                packet = m_vecChannelsIn[channel]; // using values from the last message of this channel
            else
                packet = new RTMPPacket() { m_headerType = headerType, m_nChannel = channel }; // new packet

            nSize--;

            byte[] header = new byte[RTMP_LARGE_HEADER_SIZE];            
            if (nSize > 0 && ReadN(header, 0, (int)nSize) != nSize)
            {
                Logger.Log(string.Format("failed to read RTMP packet header. type: {0}", type));
                return false;
            }
            bytesReadTotal += (int)nSize;

            if (nSize >= 3)
                packet.m_nInfoField1 = ReadInt24(header, 0);

            if (nSize >= 6)
            {
                packet.m_nBodySize = (uint)ReadInt24(header, 3);
                //Logger.Log(string.Format("new packet body to read {0}", packet.m_nBodySize));
                packet.m_nBytesRead = 0;
                packet.FreePacketHeader(); // new packet body
            }

            if (nSize > 6)
                packet.m_packetType = header[6];

            if (nSize == 11)
                packet.m_nInfoField2 = ReadInt32LE(header, 7);

            if (packet.m_nBodySize >= 0 && packet.m_body == null && !packet.AllocPacket((int)packet.m_nBodySize))
            {
                //CLog::Log(LOGDEBUG,"%s, failed to allocate packet", __FUNCTION__);
                return false;
            }

            uint nToRead = packet.m_nBodySize - packet.m_nBytesRead;
            uint nChunk = (uint)ChunkSize;
            if (nToRead < nChunk)
                nChunk = nToRead;

            int read = ReadN(packet.m_body, (int)packet.m_nBytesRead, (int)nChunk);
            if (read != nChunk)
            {
                //CLog::Log(LOGERROR, "%s, failed to read RTMP packet body. len: %lu", __FUNCTION__, packet.m_nBodySize);
                packet.m_body = null; // we dont want it deleted since its pointed to from the stored packets (m_vecChannelsIn)
                return false;
            }
            bytesReadTotal += (int)read;

            packet.m_nBytesRead += nChunk;

            // keep the packet as ref for other packets on this channel
            m_vecChannelsIn[packet.m_nChannel] = packet;
            
            if (bytesReadTotal > lastSentBytesRead + (600 * 1024)) SendBytesReceived(); // report every 600K

            if (packet.IsReady())
            {
                //Logger.Log(string.Format("packet with {0} bytes read", packet.m_nBytesRead));
                packet.m_nTimeStamp = (uint)packet.m_nInfoField1;

                // make packet's timestamp absolute 
                if (!packet.m_hasAbsTimestamp) 
                    packet.m_nTimeStamp += m_channelTimestamp[packet.m_nChannel]; // timestamps seem to be always relative!! 
                m_channelTimestamp[packet.m_nChannel] = packet.m_nTimeStamp;

                // reset the data from the stored packet. we keep the header since we may use it later if a new packet for this channel
                // arrives and requests to re-use some info (small packet header)
                //m_vecChannelsIn[packet.m_nChannel].m_body = null;
                //m_vecChannelsIn[packet.m_nChannel].m_nBytesRead = 0;
                m_vecChannelsIn[packet.m_nChannel].m_hasAbsTimestamp = false; // can only be false if we reuse header
            }

            return true;
        }

        void Close()
        {
            if (networkStream != null) networkStream.Close(1000);
            if (tcpClient != null && tcpClient.Connected) tcpClient.Close();

            networkStream = null;
            tcpClient = null;

            Duration = 0;
            ChunkSize = 128;
            Playing = false;
            CombinedTracksLength = 0;
            CombinedBitrates = 0;

            m_nBWCheckCounter = 0;
            bytesReadTotal = 0;
            lastSentBytesRead = 0;

            for (int i = 0; i < 64; i++)
            {
                m_vecChannelsIn[i] = null;
                m_vecChannelsOut[i] = null;
                m_channelTimestamp[i] = 0;
            }

            m_methodCalls.Clear();
        }

        #region Send Client Packets

        bool SendConnectPacket()
        {            
            RTMPPacket packet = new RTMPPacket();
            packet.m_nChannel = 0x03;   // control channel (invoke)
            packet.m_headerType = RTMP_PACKET_SIZE_LARGE;
            packet.m_packetType = 0x14; // INVOKE
            packet.AllocPacket(4096);

            List<byte> enc = new List<byte>();
            EncodeString(enc, "connect");
            EncodeNumber(enc, 1.0);
            enc.Add(0x03); //Object Datatype                
            EncodeString(enc, "app", Link.app);
            EncodeNumber(enc, "objectEncoding", 0.0);
            EncodeBoolean(enc, "fpad", false);
            EncodeString(enc, "flashVer", "WIN 9,0,115,0");//EncodeString(enc, "flashVer", "WIN 10,0,22,87");            
            if (!string.IsNullOrEmpty(Link.swfUrl)) EncodeString(enc, "swfUrl", Link.swfUrl);
            EncodeString(enc, "tcUrl", Link.tcUrl);                        
            EncodeNumber(enc, "audioCodecs", 1639.0);//EncodeNumber(enc, "audioCodecs", 3191.0);
            EncodeNumber(enc, "videoFunction", 1.0);
            EncodeNumber(enc, "capabilities", 15.0);
            EncodeNumber(enc, "videoCodecs", 252.0);            
            if (!string.IsNullOrEmpty(Link.pageUrl)) EncodeString(enc, "pageUrl", Link.pageUrl);
            enc.Add(0); enc.Add(0); enc.Add(0x09); // end of object - 0x00 0x00 0x09

            Array.Copy(enc.ToArray(), packet.m_body, enc.Count);
            packet.m_nBodySize = (uint)enc.Count; // todo : hier muss 0 1 2 oder 3 stehen

            return SendRTMP(packet);
        }

        bool SendPlay()
        {            
            RTMPPacket packet = new RTMPPacket();
            packet.m_nChannel = 0x08;   // we make 8 our stream channel
            packet.m_headerType = RTMP_PACKET_SIZE_LARGE;
            packet.m_packetType = 0x14; // INVOKE
            packet.m_nInfoField2 = m_stream_id;

            packet.AllocPacket(256); // should be enough
            List<byte> enc = new List<byte>();

            EncodeString(enc, "play");
            EncodeNumber(enc, 3.0);
            enc.Add(0x05); // NULL  

            Logger.Log(string.Format("invoking play '{0}'", Link.playpath));

            EncodeString(enc, Link.playpath);
            EncodeNumber(enc, 0.0);
            EncodeNumber(enc, -2.0);
            //EncodeBoolean(enc, true);

            packet.m_body = enc.ToArray();
            packet.m_nBodySize = (uint)enc.Count;

            return SendRTMP(packet);
        }

        bool SendPing(short nType, uint nObject, uint nTime)
        {
            Logger.Log(string.Format("sending ping. type: {0}", nType));

            RTMPPacket packet = new RTMPPacket();
            packet.m_nChannel = 0x02;   // control channel (ping)
            packet.m_headerType = RTMP_PACKET_SIZE_MEDIUM;
            packet.m_packetType = 0x04; // ping
            packet.m_nInfoField1 = System.Environment.TickCount;

            int nSize = (nType == 0x03 ? 10 : 6); // type 3 is the buffer time and requires all 3 parameters. all in all 10 bytes.
            packet.AllocPacket(nSize);
            packet.m_nBodySize = (uint)nSize;

            List<byte> buf = new List<byte>();
            EncodeInt16(buf, nType);

            if (nSize > 2)
                EncodeInt32(buf, (int)nObject);

            if (nSize > 6)
                EncodeInt32(buf, (int)nTime);

            packet.m_body = buf.ToArray();
            return SendRTMP(packet);
        }

        bool SendCheckBWResult()
        {
            RTMPPacket packet = new RTMPPacket();
            packet.m_nChannel = 0x03;   // control channel (invoke)
            packet.m_headerType = RTMP_PACKET_SIZE_MEDIUM;
            packet.m_packetType = 0x14; // INVOKE
            packet.m_nInfoField1 = 0x16 * m_nBWCheckCounter; // temp inc value. till we figure it out.

            packet.AllocPacket(256); // should be enough
            List<byte> enc = new List<byte>();
            EncodeString(enc, "_result");
            EncodeNumber(enc, (double)DateTime.Now.Ticks); // temp
            enc.Add(0x05); // NULL            
            EncodeNumber(enc, (double)m_nBWCheckCounter++);

            packet.m_nBodySize = (uint)enc.Count;
            packet.m_body = enc.ToArray();

            return SendRTMP(packet);
        }

        bool SendBytesReceived()
        {
            RTMPPacket packet = new RTMPPacket();
            packet.m_nChannel = 0x02;   // control channel (invoke)
            packet.m_headerType = RTMP_PACKET_SIZE_MEDIUM;
            packet.m_packetType = 0x03; // bytes in

            packet.AllocPacket(4);
            packet.m_nBodySize = 4;

            List<byte> enc = new List<byte>();
            EncodeInt32(enc, bytesReadTotal);
            packet.m_nBodySize = (uint)enc.Count;
            packet.m_body = enc.ToArray();

            lastSentBytesRead = bytesReadTotal;
            Logger.Log(string.Format("Send bytes report. ({0} bytes)", bytesReadTotal));
            return SendRTMP(packet);
        }

        bool SendServerBW()
        {
            RTMPPacket packet = new RTMPPacket();
            packet.m_nChannel = 0x02;   // control channel (invoke)
            packet.m_headerType = RTMP_PACKET_SIZE_LARGE;
            packet.m_packetType = 0x05; // Server BW

            packet.AllocPacket(4);
            packet.m_nBodySize = 4;

            List<byte> bytesToSend = new List<byte>();
            EncodeInt32(bytesToSend, 0x001312d0); // hard coded for now
            packet.m_body = bytesToSend.ToArray();
            return SendRTMP(packet);
        }

        bool SendCreateStream(double dStreamId)
        {
            RTMPPacket packet = new RTMPPacket();
            packet.m_nChannel = 0x03;   // control channel (invoke)
            packet.m_headerType = RTMP_PACKET_SIZE_MEDIUM;
            packet.m_packetType = 0x14; // INVOKE

            packet.AllocPacket(256); // should be enough
            List<byte> enc = new List<byte>();
            EncodeString(enc, "createStream");
            EncodeNumber(enc, dStreamId);
            enc.Add(0x05); // NULL

            packet.m_nBodySize = (uint)enc.Count;
            packet.m_body = enc.ToArray();

            return SendRTMP(packet);
        }

        bool SendRTMP(RTMPPacket packet)
        {
            RTMPPacket prevPacket = m_vecChannelsOut[packet.m_nChannel];
            if (packet.m_headerType != RTMP_PACKET_SIZE_LARGE)
            {
                // compress a bit by using the prev packet's attributes
                if (prevPacket.m_nBodySize == packet.m_nBodySize && packet.m_headerType == RTMP_PACKET_SIZE_MEDIUM)
                    packet.m_headerType = RTMP_PACKET_SIZE_SMALL;

                if (prevPacket.m_nInfoField2 == packet.m_nInfoField2 && packet.m_headerType == RTMP_PACKET_SIZE_SMALL)
                    packet.m_headerType = RTMP_PACKET_SIZE_MINIMUM;
            }

            if (packet.m_headerType > 3) // sanity
            {
                Logger.Log(string.Format("sanity failed!! tring to send header of type: {0}.", packet.m_headerType));
                return false;
            }

            uint nSize = packetSize[packet.m_headerType];
            List<byte> header = new List<byte>();//byte[RTMP_LARGE_HEADER_SIZE];
            header.Add((byte)((packet.m_headerType << 6) | packet.m_nChannel));
            if (nSize > 1)
                EncodeInt24(header, packet.m_nInfoField1);

            if (nSize > 4)
            {
                EncodeInt24(header, (int)packet.m_nBodySize);
                header.Add(packet.m_packetType);
            }

            if (nSize > 8)
                EncodeInt32LE(header, packet.m_nInfoField2);

            WriteN(header.ToArray(), 0, (int)nSize);

            nSize = packet.m_nBodySize;
            byte[] buffer = packet.m_body;
            uint bufferOffset = 0;
            while (nSize > 0)
            {
                uint nChunkSize = packet.m_packetType == 0x14 ? (uint)ChunkSize : packet.m_nBodySize;                
                if (nSize < ChunkSize)
                    nChunkSize = nSize;

                WriteN(buffer, (int)bufferOffset, (int)nChunkSize);

                nSize -= nChunkSize;
                bufferOffset += nChunkSize;

                if (nSize > 0)
                {
                    byte sep = (byte)(0xc0 | packet.m_nChannel);
                    WriteByte(sep);
                }
            }

            if (packet.m_packetType == 0x14) // we invoked a remote method, keep it in call queue till result arrives
                m_methodCalls.Push(ReadString(packet.m_body, 1));

            m_vecChannelsOut[packet.m_nChannel] = packet;
            m_vecChannelsOut[packet.m_nChannel].m_body = null;
            return true;
        }

        #endregion

        #region Handle Server Packets

        void HandleChangeChunkSize(RTMPPacket packet)
        {
            if (packet.m_nBodySize >= 4)
            {
                ChunkSize = ReadInt32(packet.m_body, 0);
                Logger.Log(string.Format("received: chunk size change to {0}", ChunkSize));
            }
        }

        void HandlePing(RTMPPacket packet)
        {
            short nType = -1;
            if (packet.m_body != null && packet.m_nBodySize >= 2)
                nType = ReadInt16(packet.m_body, 0);
            
            Logger.Log(string.Format("received: ping, type: {0}", nType));

            if (nType == 0x06 && packet.m_nBodySize >= 6) // server ping. reply with pong.
            {
                uint nTime = (uint)ReadInt32(packet.m_body, 2);
                SendPing(0x07, nTime, 0);
            }
        }

        void HandleInvoke(RTMPPacket packet)
        {
            if (packet.m_body[0] != 0x02) // make sure it is a string method name we start with
            {
                //CLog::Log(LOGWARNING,"%s, Sanity failed. no string method in invoke packet", __FUNCTION__);
                return;
            }

            AMFObject obj = new AMFObject();
            int nRes = obj.Decode(packet.m_body, 0, (int)packet.m_nBodySize, false);
            if (nRes < 0)
            {
                //CLog::Log(LOGERROR,"%s, error decoding invoke packet", __FUNCTION__);
                return;
            }

            obj.Dump();
            string method = obj.GetProperty(0).GetString();
            Logger.Log(string.Format("server invoking <{0}>", method));

            if (method == "_result")
            {
                string methodInvoked = m_methodCalls.Pop();                

                Logger.Log(string.Format("received result for method call <{0}>", methodInvoked));

                if (methodInvoked == "connect")
                {
                    SendServerBW();
                    SendPing(3, 0, 300);
                    SendCreateStream(2.0d);
                }
                else if (methodInvoked == "createStream")
                {
                    m_stream_id = (int)obj.GetProperty(3).GetNumber();
                    SendPlay();
                    SendPing(3, 1, (uint)m_nBufferMS);
                }
                else if (methodInvoked == "play")
                {
                }
            }
            else if (method == "onBWDone")
            {
                //SendCheckBW();
            }
            else if (method == "_onbwcheck")
            {
                SendCheckBWResult();
            }
            else if (method == "_error")
            {
                Logger.Log("rtmp server sent error");
            }
            else if (method == "close")
            {
                Logger.Log("rtmp server requested close");
                Close();
            }
            else if (method == "onStatus")
            {
                string code = obj.GetProperty(3).GetObject().GetProperty("code").GetString();
                string level = obj.GetProperty(3).GetObject().GetProperty("level").GetString();

                Logger.Log(string.Format("onStatus: code :{0}, level: {1}", code, level));

                if (code == "NetStream.Failed"
                || code == "NetStream.Play.Failed"
                || code == "NetStream.Play.Stop"
                || code == "NetStream.Play.StreamNotFound"
                || code == "NetConnection.Connect.InvalidApp")
                    Close();
            }
            else
            {

            }
        }

        void HandleMetadata(RTMPPacket packet)
        {
            HandleMetadata(packet.m_body, 0, (int)packet.m_nBodySize);
        }

        void HandleMetadata(byte[] buffer, int offset, int size)
        {            
            AMFObject obj = new AMFObject();
            int nRes = obj.Decode(buffer, offset, size, false);
            if (nRes < 0)
            {
                //Log(LOGERROR, "%s, error decoding meta data packet", __FUNCTION__);
                return;
            }

            obj.Dump();
            string metastring = obj.GetProperty(0).GetString();

            if (metastring == "onMetaData")
            {
                List<AMFObjectProperty> props = new List<AMFObjectProperty>();
                obj.FindMatchingProperty("duration", props, 1);
                if (props.Count > 0)
                {
                    Duration = props[0].GetNumber();
                    Logger.Log(string.Format("Set duration: {0}", Duration));
                }
                props.Clear();
                obj.FindMatchingProperty("length", props, int.MaxValue);
                if (props.Count > 0)
                {
                    foreach (AMFObjectProperty prop in props)
                    {
                        CombinedTracksLength += (int)prop.GetNumber();
                    }
                    Logger.Log(string.Format("Set CombinedTracksLength: {0}", CombinedTracksLength));
                }
                props.Clear();
                obj.FindMatchingProperty("audiodatarate", props, 1);
                if (props.Count > 0)
                {
                    CombinedBitrates += (int)props[0].GetNumber();
                    Logger.Log(string.Format("audiodatarate: {0}", Duration));
                }
                props.Clear();
                obj.FindMatchingProperty("videodatarate", props, 1);
                if (props.Count > 0)
                {
                    CombinedBitrates += (int)props[0].GetNumber();
                    Logger.Log(string.Format("audiodatarate: {0}", Duration));
                }
            }
        }

        void HandleFlvTags(RTMPPacket packet)
        {
            // go through FLV packets and handle metadata packets
            int pos = 0;

            while (pos + 11 < packet.m_nBodySize)
            {
                int dataSize = ReadInt24(packet.m_body, pos + 1); // size without header (11) and prevTagSize (4)

                if (pos + 11 + dataSize + 4 > packet.m_nBodySize)
                {
                    Logger.Log("Stream corrupt?!");
                    break;
                }
                if (packet.m_body[pos] == 0x12)
                {
                    HandleMetadata(packet.m_body, pos + 11, dataSize);
                }
                pos += (11 + dataSize + 4);
            }	
        }

        #endregion        

        #region Encode Functions

        public static void EncodeString(List<byte> output, string strName, string strValue)
        {
            short length = IPAddress.HostToNetworkOrder((short)strName.Length);
            output.AddRange(BitConverter.GetBytes(length));
            output.AddRange(Encoding.ASCII.GetBytes(strName));
            EncodeString(output, strValue);
        }

        public static void EncodeString(List<byte> output, string strValue)
        {
            output.Add(0x02); // type: String
            short length = IPAddress.HostToNetworkOrder((short)strValue.Length);
            output.AddRange(BitConverter.GetBytes(length));
            output.AddRange(Encoding.ASCII.GetBytes(strValue));
        }

        public static void EncodeBoolean(List<byte> output, string strName, bool bVal)
        {
            short length = IPAddress.HostToNetworkOrder((short)strName.Length);
            output.AddRange(BitConverter.GetBytes(length));
            output.AddRange(Encoding.ASCII.GetBytes(strName));
            EncodeBoolean(output, bVal);
        }

        public static void EncodeBoolean(List<byte> output, bool bVal)
        {
            output.Add(0x01); // type: Boolean
            output.Add(bVal ? (byte)0x01 : (byte)0x00);
        }

        public static void EncodeNumber(List<byte> output, string strName, double dVal)
        {
            short length = IPAddress.HostToNetworkOrder((short)strName.Length);
            output.AddRange(BitConverter.GetBytes(length));
            output.AddRange(Encoding.ASCII.GetBytes(strName));
            EncodeNumber(output, dVal);
        }

        public static void EncodeNumber(List<byte> output, double dVal)
        {
            output.Add(0x00); // type: Number
            byte[] bytes = BitConverter.GetBytes(dVal);
            for (int i = bytes.Length - 1; i >= 0; i--) output.Add(bytes[i]); // add in reversed byte order
        }

        public static void EncodeInt16(List<byte> output, short nVal)
        {
            output.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(nVal)));
        }

        public static void EncodeInt24(List<byte> output, int nVal)
        {
            byte[] bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(nVal));
            for (int i = 1; i < 4; i++) output.Add(bytes[i]);
        }

        /// <summary>
        /// big-endian 32bit integer
        /// </summary>
        /// <param name="output"></param>
        /// <param name="nVal"></param>
        public static void EncodeInt32(List<byte> output, int nVal)
        {
            output.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(nVal)));
        }

        /// <summary>
        /// little-endian 32bit integer
        /// TODO: this is wrong on big-endian processors
        /// </summary>
        /// <param name="output"></param>
        /// <param name="nVal"></param>
        public static void EncodeInt32LE(List<byte> output, int nVal)
        {
            output.AddRange(BitConverter.GetBytes(nVal));
        }

        #endregion

        #region Read Functions

        public static string ReadString(byte[] data, int offset)
        {
            string strRes = "";
            short length = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data, offset));
            if (length > 0) strRes = Encoding.ASCII.GetString(data, offset + 2, length);
            return strRes;
        }

        public static short ReadInt16(byte[] data, int offset)
        {
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data, offset));
        }

        public static int ReadInt24(byte[] data, int offset)
        {
            byte[] number = new byte[4];
            Array.Copy(data, offset, number, 1, 3);
            int result = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(number, 0));
            return result;
        }

        /// <summary>
        /// big-endian 32bit integer
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static int ReadInt32(byte[] data, int offset)
        {
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(data, offset));
        }

        /// <summary>
        /// little-endian 32bit integer
        /// TODO: this is wrong on big-endian processors
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static int ReadInt32LE(byte[] data, int offset)
        {
            return BitConverter.ToInt32(data, offset);
        }

        public static bool ReadBool(byte[] data, int offset)
        {
            return data[offset] == 0x01;
        }

        public static double ReadNumber(byte[] data, int offset)
        {
            byte[] bytes = new byte[8];
            Array.Copy(data, offset, bytes, 0, 8);
            Array.Reverse(bytes); // reversed byte order
            return BitConverter.ToDouble(bytes, 0);
        }

        #endregion        

        # region Handshake

        bool HandShake(bool FP9HandShake)
        {            
            bool encrypted = Link.protocol == RTMP_PROTOCOL_RTMPE || Link.protocol == RTMP_PROTOCOL_RTMPTE;            

            if (encrypted && !FP9HandShake)
            {
                Logger.Log("RTMPE requires FP9 handshake!");
                return false;
            }

            byte[] clientsig = new byte[RTMP_SIG_SIZE + 1];
            byte[] serversig = new byte[RTMP_SIG_SIZE];            

            if (encrypted) clientsig[0] = 0x06; // 0x08 is RTMPE as well
            else clientsig[0] = 0x03;

            int uptime = System.Environment.TickCount;
            byte[] uptime_bytes = BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(uptime));
            Array.Copy(uptime_bytes, 0, clientsig, 1, uptime_bytes.Length);

            if (FP9HandShake)
            {
                //* TODO RTMPE ;), its just RC4 with diffie-hellman
                // set version to at least 9.0.115.0
                clientsig[5] = 9;
                clientsig[6] = 0;
                clientsig[7] = 124;
                clientsig[8] = 2;
                
                Logger.Log(string.Format("Client type: {0}", clientsig[0]));
            }
            else
            {
                clientsig[5] = 0; clientsig[6] = 0; clientsig[7] = 0; clientsig[8] = 0;
            }

            // generate random data
            Random rand = new Random();
            for (int i = 9; i <= RTMP_SIG_SIZE; i++) clientsig[i] = (byte)rand.Next(0, 256);

            int dhposClient = 0;
            byte[] keyIn = null;
            byte[] keyOut = null;

            if (encrypted)
            {
                // generate Diffie-Hellmann parameters                                
                Org.BouncyCastle.Crypto.Parameters.DHParameters dhParams = 
                    new Org.BouncyCastle.Crypto.Parameters.DHParameters(
                        new Org.BouncyCastle.Math.BigInteger(1, DH_MODULUS_BYTES),
                        Org.BouncyCastle.Math.BigInteger.ValueOf(2));
                Org.BouncyCastle.Crypto.Parameters.DHKeyGenerationParameters keySpec = new Org.BouncyCastle.Crypto.Parameters.DHKeyGenerationParameters(new Org.BouncyCastle.Security.SecureRandom(), dhParams);
                Org.BouncyCastle.Crypto.Generators.DHBasicKeyPairGenerator keyGen = new Org.BouncyCastle.Crypto.Generators.DHBasicKeyPairGenerator();                
                keyGen.Init(keySpec);
                Org.BouncyCastle.Crypto.AsymmetricCipherKeyPair pair = keyGen.GenerateKeyPair();
                Org.BouncyCastle.Crypto.Agreement.DHBasicAgreement keyAgreement = new Org.BouncyCastle.Crypto.Agreement.DHBasicAgreement();
                keyAgreement.Init(pair.Private);
                Link.keyAgreement = keyAgreement;

                byte[] publicKey = (pair.Public as Org.BouncyCastle.Crypto.Parameters.DHPublicKeyParameters).Y.ToByteArray();

                byte[] temp = new byte[128];
                if (publicKey.Length < 128)
                {
                    Array.Copy(publicKey, 0, temp, 128 - publicKey.Length, publicKey.Length);
                    publicKey = temp;
                    Logger.Log("padded public key length to 128");
                }
                else if (publicKey.Length > 128)
                {
                    Array.Copy(publicKey, publicKey.Length - 128, temp, 0, 128);
                    publicKey = temp;
                    Logger.Log("truncated public key length to 128");
                }
                
                dhposClient = (int)GetDHOffset1(clientsig, 1, RTMP_SIG_SIZE);
                Logger.Log(string.Format("DH pubkey position: {0}", dhposClient));

                Array.Copy(publicKey, 0, clientsig, 1 + dhposClient, 128);                
            }

            // set handshake digest
            if (FP9HandShake)
            {
                int digestPosClient = (int)GetDigestOffset1(clientsig,1, RTMP_SIG_SIZE); // maybe reuse this value in verification
                Logger.Log(string.Format("Client digest offset: {0}", digestPosClient));

                CalculateDigest(digestPosClient, clientsig, 1, GenuineFPKey, 30, clientsig, 1 + digestPosClient);

                Logger.Log("Initial client digest: ");
                string digestAsHexString = "";
                for(int i = 1 + digestPosClient; i<1 + digestPosClient+SHA256_DIGEST_LENGTH;i++) digestAsHexString += clientsig[i].ToString("X2") + " ";
                Logger.Log(digestAsHexString);
            }

            WriteN(clientsig, 0, RTMP_SIG_SIZE + 1);

            byte type = ReadByte(); // 0x03 or 0x06

            Logger.Log(string.Format("Type Answer   : {0}", type.ToString("X2")));

            if (type != clientsig[0]) Logger.Log(string.Format("Type mismatch: client sent {0}, server answered {0}", clientsig[0], type));

            if (ReadN(serversig, 0, RTMP_SIG_SIZE) != RTMP_SIG_SIZE) return false;

            // decode server response
            uint suptime = (uint)ReadInt32(serversig,0);            

            Logger.Log(string.Format("Server Uptime : {0}",suptime));
            Logger.Log(string.Format("FMS Version   : {0}.{1}.{2}.{3}", serversig[4], serversig[5], serversig[6], serversig[7]));

            // we have to use this signature now to find the correct algorithms for getting the digest and DH positions
            int digestPosServer = (int)GetDigestOffset2(serversig,0, RTMP_SIG_SIZE);
            int dhposServer = (int)GetDHOffset2(serversig,0, RTMP_SIG_SIZE);

            if (!VerifyDigest(digestPosServer, serversig, GenuineFMSKey, 36))
            {
                Logger.Log("Trying different position for server digest!");
                digestPosServer = (int)GetDigestOffset1(serversig, 0, RTMP_SIG_SIZE);
                dhposServer = (int)GetDHOffset1(serversig,0, RTMP_SIG_SIZE);

                if (!VerifyDigest(digestPosServer, serversig, GenuineFMSKey, 36))
                {
                    Logger.Log("Couldn't verify the server digest");//,  continuing anyway, will probably fail!\n");
                    return false;
                }
            }

            Logger.Log(string.Format("Server DH public key offset: {0}",dhposServer));

            // generate SWFVerification token (SHA256 HMAC hash of decompressed SWF, key are the last 32 bytes of the server handshake)            
            if (Link.SWFHash != null)
            {
                byte[] swfVerify = new byte[2] { 0x01, 0x01 };
                Array.Copy(swfVerify, Link.SWFVerificationResponse, 2);
                List<byte> data = new List<byte>();
                EncodeInt32(data, Link.SWFSize);
                EncodeInt32(data, Link.SWFSize);
                Array.Copy(data.ToArray(), 0, Link.SWFVerificationResponse, 2, data.Count);
                byte[] key = new byte[SHA256_DIGEST_LENGTH];
                Array.Copy(serversig, RTMP_SIG_SIZE - SHA256_DIGEST_LENGTH, key, 0, SHA256_DIGEST_LENGTH);
                HMACsha256(Link.SWFHash, 0, SHA256_DIGEST_LENGTH, key, SHA256_DIGEST_LENGTH, Link.SWFVerificationResponse, 10);
            }

            // do Diffie-Hellmann Key exchange for encrypted RTMP
            if (encrypted)
            {
                // compute secret key	
                byte[] secretKey = new byte[128];

                byte[] serverKey = new byte[128];
                Array.Copy(serversig, dhposServer, serverKey, 0, 128);
                
                Org.BouncyCastle.Crypto.Parameters.DHParameters dhParams =
                    new Org.BouncyCastle.Crypto.Parameters.DHParameters(
                        new Org.BouncyCastle.Math.BigInteger(1, DH_MODULUS_BYTES),
                        Org.BouncyCastle.Math.BigInteger.ValueOf(2));

                Org.BouncyCastle.Crypto.Parameters.DHPublicKeyParameters dhPubKey =
                    new Org.BouncyCastle.Crypto.Parameters.DHPublicKeyParameters(
                        new Org.BouncyCastle.Math.BigInteger(1, serverKey),
                        dhParams);

                secretKey = Link.keyAgreement.CalculateAgreement(dhPubKey).ToByteArray();

                Logger.Log("DH SecretKey:");
                Logger.LogHex(secretKey, 0, 128);                
                
                InitRC4Encryption(
                    secretKey,
                    serversig, dhposServer,
                    clientsig, 1 + dhposClient,
                    out keyIn, out keyOut);
            }

            // 2nd part of handshake
            byte[] resp = new byte[RTMP_SIG_SIZE];            
            if (ReadN(resp, 0, RTMP_SIG_SIZE) != RTMP_SIG_SIZE) return false;

            if (FP9HandShake && resp[4] == 0 && resp[5] == 0 && resp[6] == 0 && resp[7] == 0)
            {
                Logger.Log("Wait, did the server just refuse signed authentication?");
            }

            if (!FP9HandShake)
            {
                for (int i = 0; i < RTMP_SIG_SIZE; i++)
                    if (resp[i] != clientsig[i + 1])
                    {
                        Logger.Log("client signature does not match!");
                        return false;
                    }                
                WriteN(serversig, 0, RTMP_SIG_SIZE); // send server signature back to finish handshake
            }
            else
            {
                // verify server response
                int digestPosClient = (int)GetDigestOffset1(clientsig, 1, RTMP_SIG_SIZE);

                byte[] signature = new byte[SHA256_DIGEST_LENGTH];
                byte[] digest = new byte[SHA256_DIGEST_LENGTH];

                Logger.Log(string.Format("Client signature digest position: {0}", digestPosClient));                
                HMACsha256(clientsig, 1 + digestPosClient, SHA256_DIGEST_LENGTH, GenuineFMSKey, GenuineFMSKey.Length, digest, 0);
                HMACsha256(resp,0, RTMP_SIG_SIZE - SHA256_DIGEST_LENGTH, digest, SHA256_DIGEST_LENGTH, signature, 0);

                // show some information
                Logger.Log("Digest key: ");
                Logger.LogHex(digest, 0, SHA256_DIGEST_LENGTH);

                Logger.Log("Signature calculated:");
                Logger.LogHex(signature, 0, SHA256_DIGEST_LENGTH);

                Logger.Log("Server sent signature:");
                Logger.LogHex(resp, RTMP_SIG_SIZE - SHA256_DIGEST_LENGTH, SHA256_DIGEST_LENGTH);

                for (int i = 0; i < SHA256_DIGEST_LENGTH; i++)
                    if (signature[i] != resp[RTMP_SIG_SIZE - SHA256_DIGEST_LENGTH + i])
                    {
                        Logger.Log("Server not genuine Adobe!");
                        return false;
                    }
                Logger.Log("Genuine Adobe Flash Media Server");
                
                // generate signed answer
                byte[] clientResp = new byte[RTMP_SIG_SIZE];
                for (int i = 0; i < RTMP_SIG_SIZE; i++) clientResp[i] = (byte)(rand.Next(0, 256));
                
                // calculate response now
                byte[] signatureResp = new byte[SHA256_DIGEST_LENGTH];
                byte[] digestResp = new byte[SHA256_DIGEST_LENGTH];

                HMACsha256(serversig, digestPosServer, SHA256_DIGEST_LENGTH, GenuineFPKey, GenuineFPKey.Length, digestResp, 0);
                HMACsha256(clientResp, 0, RTMP_SIG_SIZE - SHA256_DIGEST_LENGTH, digestResp, SHA256_DIGEST_LENGTH, signatureResp, 0);

                // some info output
                Logger.Log("Calculated digest key from secure key and server digest: ");
                Logger.LogHex(digestResp, 0, SHA256_DIGEST_LENGTH);

                Logger.Log("Client signature calculated:");
                Logger.LogHex(signatureResp, 0, SHA256_DIGEST_LENGTH);

                Array.Copy(signatureResp, 0, clientResp, RTMP_SIG_SIZE - SHA256_DIGEST_LENGTH, SHA256_DIGEST_LENGTH);

                WriteN(clientResp,0, RTMP_SIG_SIZE);
            }

            if (encrypted)
            {
                // set keys for encryption from now on
                Link.rc4In = new Org.BouncyCastle.Crypto.Engines.RC4Engine();
                Link.rc4In.Init(false, new Org.BouncyCastle.Crypto.Parameters.KeyParameter(keyIn));

                Link.rc4Out = new Org.BouncyCastle.Crypto.Engines.RC4Engine();
                Link.rc4Out.Init(true, new Org.BouncyCastle.Crypto.Parameters.KeyParameter(keyOut));

                // update 'encoder / decoder state' for the RC4 keys
                // both parties *pretend* as if handshake part 2 (1536 bytes) was encrypted
                // effectively this hides / discards the first few bytes of encrypted session
                // which is known to increase the secure-ness of RC4
                // RC4 state is just a function of number of bytes processed so far
                // that's why we just run 1536 arbitrary bytes through the keys below
                byte[] dummyBytes = new byte[RTMP_SIG_SIZE];
                Link.rc4In.ProcessBytes(dummyBytes, 0, RTMP_SIG_SIZE, new byte[RTMP_SIG_SIZE], 0);
                Link.rc4Out.ProcessBytes(dummyBytes, 0, RTMP_SIG_SIZE, new byte[RTMP_SIG_SIZE], 0);
            }

            Logger.Log("Handshaking finished....");
            return true;
        }

        uint GetDHOffset1(byte[] handshake, int bufferoffset, uint len)
        {
            int offset = 0;
            bufferoffset += 1532;

            offset += handshake[bufferoffset]; bufferoffset++;
            offset += handshake[bufferoffset]; bufferoffset++;
            offset += handshake[bufferoffset]; bufferoffset++;
            offset += handshake[bufferoffset];// (*ptr);

            int res = (offset % 632) + 772;

            if (res + 128 > 1531)
            {
                Logger.Log(string.Format("Couldn't calculate DH offset (got {0}), exiting!", res));
                throw new Exception();
            }

            return (uint)res;
        }

        uint GetDigestOffset1(byte[] handshake, int bufferoffset, uint len)
        {
            int offset = 0;
            bufferoffset += 8;

            offset += handshake[bufferoffset]; bufferoffset++;
            offset += handshake[bufferoffset]; bufferoffset++;
            offset += handshake[bufferoffset]; bufferoffset++;
            offset += handshake[bufferoffset];

            int res = (offset % 728) + 12;

            if (res + 32 > 771)
            {
                Logger.Log(string.Format("Couldn't calculate digest offset (got {0}), exiting!", res));
                throw new Exception();
            }

            return (uint)res;
        }

        uint GetDHOffset2(byte[] handshake, int bufferoffset, uint len)
        {
            uint offset = 0;
            bufferoffset += 768;
            //assert(RTMP_SIG_SIZE <= len);

            offset += handshake[bufferoffset]; bufferoffset++;
            offset += handshake[bufferoffset]; bufferoffset++;
            offset += handshake[bufferoffset]; bufferoffset++;
            offset += handshake[bufferoffset];

            uint res = (offset % 632) + 8;

            if (res + 128 > 767)
            {
                Logger.Log(string.Format("Couldn't calculate correct DH offset (got {0}), exiting!", res));
                throw new Exception();
            }
            return res;
        }

        uint GetDigestOffset2(byte[] handshake, int bufferoffset, uint len)
        {
            uint offset = 0;
            bufferoffset += 772;
            //assert(12 <= len);

            offset += handshake[bufferoffset]; bufferoffset++;
            offset += handshake[bufferoffset]; bufferoffset++;
            offset += handshake[bufferoffset]; bufferoffset++;
            offset += handshake[bufferoffset];// (*ptr);

            uint res = (offset % 728) + 776;

            if (res + 32 > 1535)
            {
                Logger.Log(string.Format("Couldn't calculate correct digest offset (got {0}), exiting", res));
                throw new Exception();
            }
            return res;
        }

        void CalculateDigest(int digestPos, byte[] handshakeMessage, int handshakeOffset, byte[] key, int keyLen, byte[] digest, int digestOffset)
        {
	        const int messageLen = RTMP_SIG_SIZE - SHA256_DIGEST_LENGTH;
            byte[] message = new byte[messageLen];

            Array.Copy(handshakeMessage, handshakeOffset, message, 0, digestPos);
            Array.Copy(handshakeMessage, handshakeOffset + digestPos + SHA256_DIGEST_LENGTH, message, digestPos, messageLen - digestPos);

            HMACsha256(message, 0, messageLen, key, keyLen, digest, digestOffset);
        }

        bool VerifyDigest(int digestPos, byte[] handshakeMessage, byte[] key, int keyLen)
        {
            byte[] calcDigest = new byte[SHA256_DIGEST_LENGTH];

	        CalculateDigest(digestPos, handshakeMessage, 0, key, keyLen, calcDigest, 0);

            for (int i = 0; i < SHA256_DIGEST_LENGTH; i++)
            {
                if (handshakeMessage[digestPos + i] != calcDigest[i]) return false;
            }
            return true;            	        
        }

        void HMACsha256(byte[] message, int messageOffset, int messageLen, byte[] key, int keylen, byte[] digest, int digestOffset)
        {
            System.Security.Cryptography.HMAC hmac = System.Security.Cryptography.HMACSHA256.Create("HMACSHA256");
            byte[] actualKey = new byte[keylen]; Array.Copy(key, actualKey, keylen);                       
            hmac.Key = actualKey;

            byte[] actualMessage = new byte[messageLen];
            Array.Copy(message, messageOffset, actualMessage, 0, messageLen);
            
            byte[] calcDigest = hmac.ComputeHash(actualMessage);
            Array.Copy(calcDigest, 0, digest, digestOffset, calcDigest.Length);
        }

        void InitRC4Encryption(byte[] secretKey, byte[] pubKeyIn, int inOffset, byte[] pubKeyOut, int outOffset, out byte[] rc4keyIn, out byte[] rc4keyOut)
        {
            byte[] digest = new byte[SHA256_DIGEST_LENGTH];            

            System.Security.Cryptography.HMAC hmac = System.Security.Cryptography.HMACSHA256.Create("HMACSHA256");            
            hmac.Key = secretKey;

            byte[] actualpubKeyIn = new byte[128];
            Array.Copy(pubKeyIn, inOffset, actualpubKeyIn, 0, 128);
            digest = hmac.ComputeHash(actualpubKeyIn);
            
            rc4keyOut = new byte[16];
            Array.Copy(digest, rc4keyOut, 16);
            Logger.Log("RC4 Out Key: ");
            Logger.LogHex(rc4keyOut, 0, 16);

            hmac = System.Security.Cryptography.HMACSHA256.Create("HMACSHA256");
            hmac.Key = secretKey;
            
            byte[] actualpubKeyOut = new byte[128];
            Array.Copy(pubKeyOut, outOffset, actualpubKeyOut, 0, 128);
            digest = hmac.ComputeHash(actualpubKeyOut);

            rc4keyIn = new byte[16];
            Array.Copy(digest, rc4keyIn, 16);
            Logger.Log("RC4 In Key: ");
            Logger.LogHex(rc4keyIn, 0, 16);
        }

        #endregion        

        int ReadN(byte[] buffer, int offset, int size)
        {            
            // wait (max) one second until data is available
            int i = 1000;
            while (tcpClient.Available < size && i > 0)
            {
                i--;
                System.Threading.Thread.Sleep(10);
            }
            if (tcpClient.Available < size) throw new Exception("No Data Available");

            byte[] data = new byte[size];
            int read = networkStream.Read(data, 0, size);

            // decrypt if needed
            if (read > 0 && Link.rc4In != null)
            {
                Link.rc4In.ProcessBytes(data, 0, size, buffer, offset);                
            }
            else
            {
                Array.Copy(data, 0, buffer, offset, size);
            }

            return read;
        }

        byte ReadByte()
        {
            byte[] buffer = new byte[1];
            ReadN(buffer, 0, 1);
            return buffer[0];
        }

        void WriteN(byte[] buffer, int offset, int size)
        {            
            // encrypt if needed
            if (Link.rc4Out != null)
            {
                byte[] result = new byte[size];
                Link.rc4Out.ProcessBytes(buffer, offset, size, result, 0);
                networkStream.Write(result, 0, size);                
            }
            else
            {
                networkStream.Write(buffer, offset, size);
            }
        }

        void WriteByte(byte data)
        {
            WriteN(new byte[1] { data }, 0, 1);
        }
    }
}
