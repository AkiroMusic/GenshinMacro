using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        string path = args.Length > 0 ? args[0] : @"E:\Code\Akiro Project\AkiMacro\REF\鼠标连点器\双击启动.exe";
        byte[] bytes = File.ReadAllBytes(path);
        
        Console.WriteLine($"File size: {bytes.Length} bytes");
        Console.WriteLine($"First 16 bytes: {BitConverter.ToString(bytes, 0, 16)}");
        
        // Check DOS header
        if (bytes[0] == 0x4D && bytes[1] == 0x5A) // "MZ"
        {
            Console.WriteLine("DOS header: MZ (valid)");
            // PE header offset at 0x3C
            if (bytes.Length > 0x3C + 4)
            {
                int peOffset = BitConverter.ToInt32(bytes, 0x3C);
                Console.WriteLine($"PE header offset: 0x{peOffset:X}");
                
                if (bytes.Length > peOffset + 4)
                {
                    Console.WriteLine($"PE header: {BitConverter.ToString(bytes, peOffset, 4)}");
                    if (bytes[peOffset] == 0x50 && bytes[peOffset+1] == 0x45) // "PE"
                    {
                        Console.WriteLine("PE header: Valid PE signature");
                    }
                    else
                    {
                        Console.WriteLine("PE header: NOT a PE file (not .NET managed)");
                    }
                }
            }
        }
        else
        {
            Console.WriteLine("NOT a valid PE file");
        }
        
        // Check for .NET metadata signature (BSJB)
        for (int i = 0; i < bytes.Length - 4; i++)
        {
            if (bytes[i] == 0x42 && bytes[i+1] == 0x53 && bytes[i+2] == 0x4A && bytes[i+3] == 0x42) // "BSJB"
            {
                Console.WriteLine($"Found .NET metadata signature (BSJB) at offset 0x{i:X}");
                break;
            }
        }
    }
}
