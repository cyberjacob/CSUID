using System.IO;
using System.Text;
using JetBrains.Annotations;

namespace CSUID
{
    /// <summary>
    /// Slightly reduced Base64 implementation, to ensure URL-safety
    /// </summary>
    internal static class Base62
    {
        private const string Base62CodingSpace = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        [NotNull]
        public static string ToBase62([NotNull] byte[] original)
        {
            var sb = new StringBuilder();
            var stream = new BitStream(original);
            stream.Seek(0, SeekOrigin.Begin);
            var read = new byte[1];
            while (true)
            {
                read[0] = 0;
                var length = stream.Read(read, 0, 6);
                if (length == 6)
                {
                    switch (read[0] >> 3)
                    {
                        case 0x1f:
                            sb.Append(Base62CodingSpace[61]);
                            stream.Seek(-1, SeekOrigin.Current);
                            break;
                        case 0x1e:
                            sb.Append(Base62CodingSpace[60]);
                            stream.Seek(-1, SeekOrigin.Current);
                            break;
                        default:
                            sb.Append(Base62CodingSpace[read[0] >> 2]);
                            break;
                    }
                } else if (length == 0)
                    break;
                else
                {
                    sb.Append(Base62CodingSpace[read[0] >> (8 - length)]);
                    break;
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Convert a Base62 string to byte array
        /// </summary>
        /// <param name="base62">Base62 string</param>
        /// <returns>Byte array</returns>
        [NotNull]
        public static byte[] FromBase62([NotNull] string base62)
        {
            // Character count
            var count = 0;

            // Set up the BitStream
            var stream = new BitStream(base62.Length * 6 / 8);

            foreach (var c in base62)
            {
                // Look up coding table
                var index = Base62CodingSpace.IndexOf(c);

                // If end is reached
                if (count == base62.Length - 1)
                {
                    // Check if the ending is good
                    var mod = (int) (stream.Position % 8);
                    if (mod == 0)
                        throw new InvalidDataException("an extra character was found");

                    if (index >> (8 - mod) > 0)
                        throw new InvalidDataException("invalid ending character was found");

                    stream.Write(new[] {(byte) (index << mod)}, 0, 8 - mod);
                } else
                {
                    // If 60 or 61 then only write 5 bits to the stream, otherwise 6 bits.
                    switch (index)
                    {
                        case 60:
                            stream.Write(new byte[] {0xf0}, 0, 5);
                            break;
                        case 61:
                            stream.Write(new byte[] {0xf8}, 0, 5);
                            break;
                        default:
                            stream.Write(new[] {(byte) index}, 2, 6);
                            break;
                    }
                }

                count++;
            }

            // Dump out the bytes
            var result = new byte[stream.Position / 8];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(result, 0, result.Length * 8);
            return result;
        }
    }
}
