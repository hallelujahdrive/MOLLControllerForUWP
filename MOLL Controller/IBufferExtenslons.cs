using System;
using System.Text;
using Windows.Storage.Streams;

namespace MOLL_Controller {
  static class IBufferExtentions {
    public static string DecodeUtf8String(this IBuffer buffer) {
      var data = buffer.ToArray();
      return Encoding.UTF8.GetString(data);
    }

    private static byte[] ToArray(this IBuffer buffer) {
      if(buffer == null) {
        throw new NullReferenceException();
      }
        byte[] bytes = new byte[buffer.Length];
        using (DataReader reader = DataReader.FromBuffer(buffer)) {
          reader.ReadBytes(bytes);
        }

      return bytes;
    }
  }
}