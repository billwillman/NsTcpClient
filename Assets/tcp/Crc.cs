using System;

namespace NsTcpClient {

	internal interface ICRC 
	{ 
		
		long Value 
		{ 
			get; 
		} 
		
		void Reset(); 
		
		void Crc(int bval); 
		
		void Crc(byte[] buffer, int bufSize); 
		
		void Crc(byte[] buf, int off, int len); 
		
	} 


	public sealed class CRC8:ICRC 
	{ 
		#region CRC 8 位校验表 
		
		/// <summary> 
		/// CRC 8 位校验表 
		/// </summary> 
		readonly private static byte[] CRC8_Table = new byte[] 
		{ 
			0,94,188,226,97,63,221,131,194,156,126,32,163,253,31,65, 
			157,195,33,127,252,162,64,30, 95,1,227,189,62,96,130,220, 
			35,125,159,193,66,28,254,160,225,191,93,3,128,222,60,98, 
			190,224,2,92,223,129,99,61,124,34,192,158,29,67,161,255, 
			70,24,250,164,39,121,155,197,132,218,56,102,229,187,89,7,             
			219,133,103,57,186,228,6,88,25,71,165,251,120,38,196,154, 
			101,59,217,135,4,90,184,230,167,249,27,69,198,152,122,36,                         
			248,166,68,26,153,199,37,123,58,100,134,216,91,5,231,185,             
			140,210,48,110,237,179,81,15,78,16,242,172,47,113,147,205, 
			17,79,173,243,112,46,204,146,211,141,111,49,178,236,14,80, 
			175,241,19,77,206,144,114,44,109,51,209,143,12,82,176,238, 
			50,108,142,208,83,13,239,177,240,174,76,18,145,207,45,115, 
			202,148,118,40,171,245,23,73,8,86,180,234,105,55,213,139, 
			87,9,235,181,54,104,138,212,149,203, 41,119,244,170,72,22, 
			233,183,85,11,136,214,52,106,43,117,151,201,74,20,246,168, 
			116,42,200,150,21,75,169,247,182,232,10,84,215,137,107,53 
		}; 
		#endregion 
		
		uint crc = 0; 
		
		/// <summary> 
		///返回 CRC8校验结果; 
		/// </summary> 
		public long Value 
		{ 
			get 
			{ 
				return crc; 
			} 
			set 
			{ 
				crc = (uint)value; 
			} 
		} 
		
		
		/// <summary> 
		/// CRC校验前设置校验值 
		/// </summary> 
		public void Reset() 
		{ 
			crc = 0; 
		} 
		
		/// <summary> 
		/// 8 位 CRC 校验 产生校验码 需要被校验码和校验码 
		/// </summary> 
		/// <param name="CRC"></param> 
		/// <param name="OldCRC"> 初始为 0 ,以后为 返回值 ret </param> 
		/// <returns> 产生校验码时 ret　为校验码</returns> 
		
		public void Crc(byte CRC,byte OldCRC) 
		{ 
			crc = CRC8_Table[OldCRC ^ CRC]; 
		} 
		
		/// <summary> 
		/// 8 位 CRC 校验 产生校验码 只要被校验码 
		/// </summary> 
		/// <param name="bval"></param> 
		public void Crc(int bval) 
		{ 
			crc = CRC8_Table[crc ^ bval]; 
		} 
		
		/// <summary> 
		/// 8 位 CRC 校验 产生校验码 只要被校验的字节数组 
		/// </summary> 
		/// <param name="buffer"></param> 
		public void Crc(byte[] buffer, int bufSize) 
		{ 
			Crc(buffer,0, bufSize); 
		} 
		
		/// <summary> 
		/// 8 位 CRC 校验 产生校验码 要被校验的字节数组、起始结果位置和字节长度 
		/// </summary> 
		/// <param name="buf"></param> 
		/// <param name="off"></param> 
		/// <param name="len"></param> 
		public void Crc(byte[] buf,int off ,int len) 
		{ 
			if (buf == null) 
			{ 
				throw new ArgumentNullException("buf"); 
			} 
			
			if (off < 0 || len < 0 || off + len > buf.Length) 
			{ 
				throw new ArgumentOutOfRangeException(); 
			} 
			for (int i = off; i < len ; i ++) 
			{ 
				Crc(buf[i]); 
			} 
		} 
	} 

	public sealed class CRC16:ICRC 
	{ 
		#region CRC 16 位校验表 
		
		/// <summary> 
		/// 16 位校验表 Upper 表 
		/// </summary> 
		readonly private static ushort[] uppercrctab = new ushort[] 
		{ 
			0x0000,0x1231,0x2462,0x3653,0x48c4,0x5af5,0x6ca6,0x7e97, 
			0x9188,0x83b9,0xb5ea,0xa7db,0xd94c,0xcb7d,0xfd2e,0xef1f 
		}; 
		
		/// <summary> 
		/// 16 位校验表 Lower 表 
		/// </summary> 
		readonly private static ushort[] lowercrctab = new ushort[] 
		{ 
			0x0000,0x1021,0x2042,0x3063,0x4084,0x50a5,0x60c6,0x70e7, 
			0x8108,0x9129,0xa14a,0xb16b,0xc18c,0xd1ad,0xe1ce,0xf1ef 
		}; 
		#endregion 
		
		ushort crc = 0;
		
		/// <summary> 
		/// 校验后的结果 
		/// </summary> 
		public long Value 
		{ 
			get 
			{ 
				return crc; 
			} 
			set 
			{ 
				crc = (ushort)value; 
			} 
		} 
		
		/// <summary> 
		/// 设置crc 初始值 
		/// </summary> 
		public void Reset() 
		{ 
			crc = 0; 
		} 
		
		/// <summary> 
		/// Crc16 
		/// </summary> 
		/// <param name="ucrc"></param> 
		/// <param name="buf"></param> 
		public void Crc(ushort ucrc,byte[] buf, int bufSize) 
		{ 
			crc = ucrc; 
			Crc(buf, bufSize); 
		} 
		
		/// <summary> 
		/// Crc16 
		/// </summary> 
		/// <param name="bval"></param> 
		public void Crc(int bval) 
		{ 
			ushort h = (ushort)((crc >> 12) & 0x0f); 
			ushort l = (ushort)((crc >> 8 ) & 0x0f); 
			ushort temp = crc; 
			temp =(ushort)(((temp & 0x00ff) << 8) | bval); 
			temp =(ushort)(temp ^(uppercrctab[(h -1) + 1] ^ lowercrctab[(l - 1) + 1])); 
			crc = temp; 
		} 
		
		
		/// <summary> 
		/// Crc16 
		/// </summary> 
		/// <param name="buffer"></param> 
		public void Crc(byte[] buffer, int bufSize) 
		{ 
			Crc(buffer,0,bufSize); 
		} 
		
		/// <summary> 
		/// Crc16 
		/// </summary> 
		/// <param name="buf"></param> 
		/// <param name="off"></param> 
		/// <param name="len"></param> 
		public void Crc(byte[] buf,int off,int len) 
		{ 
			if (buf == null) 
			{ 
				throw new ArgumentNullException("buf"); 
			} 
			
			if (off < 0 || len < 0 || off + len > buf.Length) 
			{ 
				throw new ArgumentOutOfRangeException(); 
			} 
			for (int i = off; i < len ; i ++) 
			{ 
				Crc(buf[i]); 
			} 
		} 
	} 


	public sealed class Crc32 : ICRC 
	{ 
		readonly private static uint CrcSeed = 0xFFFFFFFF; 
		
		readonly private static uint[] CrcTable = new uint[] { 
			0x00000000, 0x77073096, 0xEE0E612C, 0x990951BA, 0x076DC419, 
			0x706AF48F, 0xE963A535, 0x9E6495A3, 0x0EDB8832, 0x79DCB8A4, 
			0xE0D5E91E, 0x97D2D988, 0x09B64C2B, 0x7EB17CBD, 0xE7B82D07, 
			0x90BF1D91, 0x1DB71064, 0x6AB020F2, 0xF3B97148, 0x84BE41DE, 
			0x1ADAD47D, 0x6DDDE4EB, 0xF4D4B551, 0x83D385C7, 0x136C9856, 
			0x646BA8C0, 0xFD62F97A, 0x8A65C9EC, 0x14015C4F, 0x63066CD9, 
			0xFA0F3D63, 0x8D080DF5, 0x3B6E20C8, 0x4C69105E, 0xD56041E4, 
			0xA2677172, 0x3C03E4D1, 0x4B04D447, 0xD20D85FD, 0xA50AB56B, 
			0x35B5A8FA, 0x42B2986C, 0xDBBBC9D6, 0xACBCF940, 0x32D86CE3, 
			0x45DF5C75, 0xDCD60DCF, 0xABD13D59, 0x26D930AC, 0x51DE003A, 
			0xC8D75180, 0xBFD06116, 0x21B4F4B5, 0x56B3C423, 0xCFBA9599, 
			0xB8BDA50F, 0x2802B89E, 0x5F058808, 0xC60CD9B2, 0xB10BE924, 
			0x2F6F7C87, 0x58684C11, 0xC1611DAB, 0xB6662D3D, 0x76DC4190, 
			0x01DB7106, 0x98D220BC, 0xEFD5102A, 0x71B18589, 0x06B6B51F, 
			0x9FBFE4A5, 0xE8B8D433, 0x7807C9A2, 0x0F00F934, 0x9609A88E, 
			0xE10E9818, 0x7F6A0DBB, 0x086D3D2D, 0x91646C97, 0xE6635C01, 
			0x6B6B51F4, 0x1C6C6162, 0x856530D8, 0xF262004E, 0x6C0695ED, 
			0x1B01A57B, 0x8208F4C1, 0xF50FC457, 0x65B0D9C6, 0x12B7E950, 
			0x8BBEB8EA, 0xFCB9887C, 0x62DD1DDF, 0x15DA2D49, 0x8CD37CF3, 
			0xFBD44C65, 0x4DB26158, 0x3AB551CE, 0xA3BC0074, 0xD4BB30E2, 
			0x4ADFA541, 0x3DD895D7, 0xA4D1C46D, 0xD3D6F4FB, 0x4369E96A, 
			0x346ED9FC, 0xAD678846, 0xDA60B8D0, 0x44042D73, 0x33031DE5, 
			0xAA0A4C5F, 0xDD0D7CC9, 0x5005713C, 0x270241AA, 0xBE0B1010, 
			0xC90C2086, 0x5768B525, 0x206F85B3, 0xB966D409, 0xCE61E49F, 
			0x5EDEF90E, 0x29D9C998, 0xB0D09822, 0xC7D7A8B4, 0x59B33D17, 
			0x2EB40D81, 0xB7BD5C3B, 0xC0BA6CAD, 0xEDB88320, 0x9ABFB3B6, 
			0x03B6E20C, 0x74B1D29A, 0xEAD54739, 0x9DD277AF, 0x04DB2615, 
			0x73DC1683, 0xE3630B12, 0x94643B84, 0x0D6D6A3E, 0x7A6A5AA8, 
			0xE40ECF0B, 0x9309FF9D, 0x0A00AE27, 0x7D079EB1, 0xF00F9344, 
			0x8708A3D2, 0x1E01F268, 0x6906C2FE, 0xF762575D, 0x806567CB, 
			0x196C3671, 0x6E6B06E7, 0xFED41B76, 0x89D32BE0, 0x10DA7A5A, 
			0x67DD4ACC, 0xF9B9DF6F, 0x8EBEEFF9, 0x17B7BE43, 0x60B08ED5, 
			0xD6D6A3E8, 0xA1D1937E, 0x38D8C2C4, 0x4FDFF252, 0xD1BB67F1, 
			0xA6BC5767, 0x3FB506DD, 0x48B2364B, 0xD80D2BDA, 0xAF0A1B4C, 
			0x36034AF6, 0x41047A60, 0xDF60EFC3, 0xA867DF55, 0x316E8EEF, 
			0x4669BE79, 0xCB61B38C, 0xBC66831A, 0x256FD2A0, 0x5268E236, 
			0xCC0C7795, 0xBB0B4703, 0x220216B9, 0x5505262F, 0xC5BA3BBE, 
			0xB2BD0B28, 0x2BB45A92, 0x5CB36A04, 0xC2D7FFA7, 0xB5D0CF31, 
			0x2CD99E8B, 0x5BDEAE1D, 0x9B64C2B0, 0xEC63F226, 0x756AA39C, 
			0x026D930A, 0x9C0906A9, 0xEB0E363F, 0x72076785, 0x05005713, 
			0x95BF4A82, 0xE2B87A14, 0x7BB12BAE, 0x0CB61B38, 0x92D28E9B, 
			0xE5D5BE0D, 0x7CDCEFB7, 0x0BDBDF21, 0x86D3D2D4, 0xF1D4E242, 
			0x68DDB3F8, 0x1FDA836E, 0x81BE16CD, 0xF6B9265B, 0x6FB077E1, 
			0x18B74777, 0x88085AE6, 0xFF0F6A70, 0x66063BCA, 0x11010B5C, 
			0x8F659EFF, 0xF862AE69, 0x616BFFD3, 0x166CCF45, 0xA00AE278, 
			0xD70DD2EE, 0x4E048354, 0x3903B3C2, 0xA7672661, 0xD06016F7, 
			0x4969474D, 0x3E6E77DB, 0xAED16A4A, 0xD9D65ADC, 0x40DF0B66, 
			0x37D83BF0, 0xA9BCAE53, 0xDEBB9EC5, 0x47B2CF7F, 0x30B5FFE9, 
			0xBDBDF21C, 0xCABAC28A, 0x53B39330, 0x24B4A3A6, 0xBAD03605, 
			0xCDD70693, 0x54DE5729, 0x23D967BF, 0xB3667A2E, 0xC4614AB8, 
			0x5D681B02, 0x2A6F2B94, 0xB40BBE37, 0xC30C8EA1, 0x5A05DF1B, 
			0x2D02EF8D 
		}; 
		
		internal static uint ComputeCrc32(uint oldCrc, byte bval) 
		{ 
			return (uint)(Crc32.CrcTable[(oldCrc ^ bval) & 0xFF] ^ (oldCrc >> 8)); 
		} 
		
		uint crc = 0; 
		
		public long Value 
		{ 
			get 
			{ 
				return (long)crc; 
			} 
			set 
			{ 
				crc = (uint)value; 
			} 
		} 
		
		public void Reset() 
		{ 
			crc = 0; 
		} 
		
		public void Crc(int bval) 
		{ 
			crc ^= CrcSeed; 
			crc  = CrcTable[(crc ^ bval) & 0xFF] ^ (crc >> 8); 
			crc ^= CrcSeed; 
		} 
		
		public void Crc(byte[] buffer, int bufSize) 
		{ 
			Crc(buffer, 0, bufSize); 
		} 
		
		public void Crc(byte[] buf, int off, int len) 
		{ 
			if (buf == null) 
			{ 
				throw new ArgumentNullException("buf"); 
			} 
			
			if (off < 0 || len < 0 || off + len > buf.Length) 
			{ 
				throw new ArgumentOutOfRangeException(); 
			} 
			
			crc ^= CrcSeed; 
			
			while (--len >= 0) 
			{ 
				crc = CrcTable[(crc ^ buf[off++]) & 0xFF] ^ (crc >> 8); 
			} 
			
			crc ^= CrcSeed; 
		} 
	}


    public static class CRC64 {
        private static readonly UInt64[] s_CRC64Table = new UInt64[256]
        {
        0x0000000000000000, 0x42F0E1EBA9EA3693, 0x85E1C3D753D46D26, 0xC711223CFA3E5BB5,
        0x493366450E42ECDF, 0x0BC387AEA7A8DA4C, 0xCCD2A5925D9681F9, 0x8E224479F47CB76A,
        0x9266CC8A1C85D9BE, 0xD0962D61B56FEF2D, 0x17870F5D4F51B498, 0x5577EEB6E6BB820B,
        0xDB55AACF12C73561, 0x99A54B24BB2D03F2, 0x5EB4691841135847, 0x1C4488F3E8F96ED4,
        0x663D78FF90E185EF, 0x24CD9914390BB37C, 0xE3DCBB28C335E8C9, 0xA12C5AC36ADFDE5A,
        0x2F0E1EBA9EA36930, 0x6DFEFF5137495FA3, 0xAAEFDD6DCD770416, 0xE81F3C86649D3285,
        0xF45BB4758C645C51, 0xB6AB559E258E6AC2, 0x71BA77A2DFB03177, 0x334A9649765A07E4,
        0xBD68D2308226B08E, 0xFF9833DB2BCC861D, 0x388911E7D1F2DDA8, 0x7A79F00C7818EB3B,
        0xCC7AF1FF21C30BDE, 0x8E8A101488293D4D, 0x499B3228721766F8, 0x0B6BD3C3DBFD506B,
        0x854997BA2F81E701, 0xC7B97651866BD192, 0x00A8546D7C558A27, 0x4258B586D5BFBCB4,
        0x5E1C3D753D46D260, 0x1CECDC9E94ACE4F3, 0xDBFDFEA26E92BF46, 0x990D1F49C77889D5,
        0x172F5B3033043EBF, 0x55DFBADB9AEE082C, 0x92CE98E760D05399, 0xD03E790CC93A650A,
        0xAA478900B1228E31, 0xE8B768EB18C8B8A2, 0x2FA64AD7E2F6E317, 0x6D56AB3C4B1CD584,
        0xE374EF45BF6062EE, 0xA1840EAE168A547D, 0x66952C92ECB40FC8, 0x2465CD79455E395B,
        0x3821458AADA7578F, 0x7AD1A461044D611C, 0xBDC0865DFE733AA9, 0xFF3067B657990C3A,
        0x711223CFA3E5BB50, 0x33E2C2240A0F8DC3, 0xF4F3E018F031D676, 0xB60301F359DBE0E5,
        0xDA050215EA6C212F, 0x98F5E3FE438617BC, 0x5FE4C1C2B9B84C09, 0x1D14202910527A9A,
        0x93366450E42ECDF0, 0xD1C685BB4DC4FB63, 0x16D7A787B7FAA0D6, 0x5427466C1E109645,
        0x4863CE9FF6E9F891, 0x0A932F745F03CE02, 0xCD820D48A53D95B7, 0x8F72ECA30CD7A324,
        0x0150A8DAF8AB144E, 0x43A04931514122DD, 0x84B16B0DAB7F7968, 0xC6418AE602954FFB,
        0xBC387AEA7A8DA4C0, 0xFEC89B01D3679253, 0x39D9B93D2959C9E6, 0x7B2958D680B3FF75,
        0xF50B1CAF74CF481F, 0xB7FBFD44DD257E8C, 0x70EADF78271B2539, 0x321A3E938EF113AA,
        0x2E5EB66066087D7E, 0x6CAE578BCFE24BED, 0xABBF75B735DC1058, 0xE94F945C9C3626CB,
        0x676DD025684A91A1, 0x259D31CEC1A0A732, 0xE28C13F23B9EFC87, 0xA07CF2199274CA14,
        0x167FF3EACBAF2AF1, 0x548F120162451C62, 0x939E303D987B47D7, 0xD16ED1D631917144,
        0x5F4C95AFC5EDC62E, 0x1DBC74446C07F0BD, 0xDAAD56789639AB08, 0x985DB7933FD39D9B,
        0x84193F60D72AF34F, 0xC6E9DE8B7EC0C5DC, 0x01F8FCB784FE9E69, 0x43081D5C2D14A8FA,
        0xCD2A5925D9681F90, 0x8FDAB8CE70822903, 0x48CB9AF28ABC72B6, 0x0A3B7B1923564425,
        0x70428B155B4EAF1E, 0x32B26AFEF2A4998D, 0xF5A348C2089AC238, 0xB753A929A170F4AB,
        0x3971ED50550C43C1, 0x7B810CBBFCE67552, 0xBC902E8706D82EE7, 0xFE60CF6CAF321874,
        0xE224479F47CB76A0, 0xA0D4A674EE214033, 0x67C58448141F1B86, 0x253565A3BDF52D15,
        0xAB1721DA49899A7F, 0xE9E7C031E063ACEC, 0x2EF6E20D1A5DF759, 0x6C0603E6B3B7C1CA,
        0xF6FAE5C07D3274CD, 0xB40A042BD4D8425E, 0x731B26172EE619EB, 0x31EBC7FC870C2F78,
        0xBFC9838573709812, 0xFD39626EDA9AAE81, 0x3A28405220A4F534, 0x78D8A1B9894EC3A7,
        0x649C294A61B7AD73, 0x266CC8A1C85D9BE0, 0xE17DEA9D3263C055, 0xA38D0B769B89F6C6,
        0x2DAF4F0F6FF541AC, 0x6F5FAEE4C61F773F, 0xA84E8CD83C212C8A, 0xEABE6D3395CB1A19,
        0x90C79D3FEDD3F122, 0xD2377CD44439C7B1, 0x15265EE8BE079C04, 0x57D6BF0317EDAA97,
        0xD9F4FB7AE3911DFD, 0x9B041A914A7B2B6E, 0x5C1538ADB04570DB, 0x1EE5D94619AF4648,
        0x02A151B5F156289C, 0x4051B05E58BC1E0F, 0x87409262A28245BA, 0xC5B073890B687329,
        0x4B9237F0FF14C443, 0x0962D61B56FEF2D0, 0xCE73F427ACC0A965, 0x8C8315CC052A9FF6,
        0x3A80143F5CF17F13, 0x7870F5D4F51B4980, 0xBF61D7E80F251235, 0xFD913603A6CF24A6,
        0x73B3727A52B393CC, 0x31439391FB59A55F, 0xF652B1AD0167FEEA, 0xB4A25046A88DC879,
        0xA8E6D8B54074A6AD, 0xEA16395EE99E903E, 0x2D071B6213A0CB8B, 0x6FF7FA89BA4AFD18,
        0xE1D5BEF04E364A72, 0xA3255F1BE7DC7CE1, 0x64347D271DE22754, 0x26C49CCCB40811C7,
        0x5CBD6CC0CC10FAFC, 0x1E4D8D2B65FACC6F, 0xD95CAF179FC497DA, 0x9BAC4EFC362EA149,
        0x158E0A85C2521623, 0x577EEB6E6BB820B0, 0x906FC95291867B05, 0xD29F28B9386C4D96,
        0xCEDBA04AD0952342, 0x8C2B41A1797F15D1, 0x4B3A639D83414E64, 0x09CA82762AAB78F7,
        0x87E8C60FDED7CF9D, 0xC51827E4773DF90E, 0x020905D88D03A2BB, 0x40F9E43324E99428,
        0x2CFFE7D5975E55E2, 0x6E0F063E3EB46371, 0xA91E2402C48A38C4, 0xEBEEC5E96D600E57,
        0x65CC8190991CB93D, 0x273C607B30F68FAE, 0xE02D4247CAC8D41B, 0xA2DDA3AC6322E288,
        0xBE992B5F8BDB8C5C, 0xFC69CAB42231BACF, 0x3B78E888D80FE17A, 0x7988096371E5D7E9,
        0xF7AA4D1A85996083, 0xB55AACF12C735610, 0x724B8ECDD64D0DA5, 0x30BB6F267FA73B36,
        0x4AC29F2A07BFD00D, 0x08327EC1AE55E69E, 0xCF235CFD546BBD2B, 0x8DD3BD16FD818BB8,
        0x03F1F96F09FD3CD2, 0x41011884A0170A41, 0x86103AB85A2951F4, 0xC4E0DB53F3C36767,
        0xD8A453A01B3A09B3, 0x9A54B24BB2D03F20, 0x5D45907748EE6495, 0x1FB5719CE1045206,
        0x919735E51578E56C, 0xD367D40EBC92D3FF, 0x1476F63246AC884A, 0x568617D9EF46BED9,
        0xE085162AB69D5E3C, 0xA275F7C11F7768AF, 0x6564D5FDE549331A, 0x279434164CA30589,
        0xA9B6706FB8DFB2E3, 0xEB46918411358470, 0x2C57B3B8EB0BDFC5, 0x6EA7525342E1E956,
        0x72E3DAA0AA188782, 0x30133B4B03F2B111, 0xF7021977F9CCEAA4, 0xB5F2F89C5026DC37,
        0x3BD0BCE5A45A6B5D, 0x79205D0E0DB05DCE, 0xBE317F32F78E067B, 0xFCC19ED95E6430E8,
        0x86B86ED5267CDBD3, 0xC4488F3E8F96ED40, 0x0359AD0275A8B6F5, 0x41A94CE9DC428066,
        0xCF8B0890283E370C, 0x8D7BE97B81D4019F, 0x4A6ACB477BEA5A2A, 0x089A2AACD2006CB9,
        0x14DEA25F3AF9026D, 0x562E43B4931334FE, 0x913F6188692D6F4B, 0xD3CF8063C0C759D8,
        0x5DEDC41A34BBEEB2, 0x1F1D25F19D51D821, 0xD80C07CD676F8394, 0x9AFCE626CE85B507
        };

        public static ulong Compute(byte[] data) {
            ulong crc = 0xffffffffffffffff;
            for (int i = 0; i < data.Length; i++) {
                uint tableIndex = (((uint)(crc >> 56)) ^ data[i]) & 0xff;
                crc = s_CRC64Table[tableIndex] ^ (crc << 8);
            }
            return (crc ^ 0xffffffffffffffff);
        }


        /// <summary>
        /// 允许中英文都有的
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public unsafe static ulong ComputeString(string text) {
            if (text == null)
                return 0;

            ulong crc = 0xffffffffffffffff;
            //int len = System.Text.Encoding.UTF8.GetByteCount(text);
            int len = text.Length * sizeof(char);
            fixed (char* ptr = text)
            {
                byte* pByte = (byte*)ptr;
                for (int i = 0; i < len; ++i)
                {
                    byte b = *(pByte++);
                    uint tableIndex = (((uint)(crc >> 56)) ^ b) & 0xff;
                    crc = s_CRC64Table[tableIndex] ^ (crc << 8);
                }
            }
            return (crc ^ 0xffffffffffffffff);
        }


    }
	
}
