using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaspberryPi.Common.Utilities;
public static class CryptographyKeySizes {
	public static int AesKeySizeBits => 256;
	public static int AesIvSizeBits => 128;
	public static int RsaKeySizeBits => 8000;
}
