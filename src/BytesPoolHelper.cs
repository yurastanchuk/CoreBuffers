using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBuffers {
public static class 
BytesPoolHelper {

public static void
Write(this Stream stream, BytesRent bytesRent) => stream.Write(bytesRent.BytesChunk.Bytes, offset: bytesRent.StartIndex, count: bytesRent.RentLength);

public static byte[]
GetBytes(this BytesRent bytesRent) {
    var bytes = new byte[bytesRent.RentLength];
    Array.Copy(sourceArray: bytesRent.BytesChunk.Bytes, sourceIndex: bytesRent.StartIndex, destinationArray: bytes, destinationIndex: 0, length: bytesRent.RentLength);
    return bytes;
}
}
}
