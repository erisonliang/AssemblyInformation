﻿using System;
using System.IO;

namespace AssemblyInformation 
{
    // ReSharper disable InconsistentNaming
    public enum MachineType : ushort 
    {
        IMAGE_FILE_MACHINE_UNKNOWN = 0x0,
        IMAGE_FILE_MACHINE_AM33 = 0x1d3,
        IMAGE_FILE_MACHINE_AMD64 = 0x8664,
        IMAGE_FILE_MACHINE_ARM = 0x1c0,
        IMAGE_FILE_MACHINE_EBC = 0xebc,
        IMAGE_FILE_MACHINE_I386 = 0x14c,
        IMAGE_FILE_MACHINE_IA64 = 0x200,
        IMAGE_FILE_MACHINE_M32R = 0x9041,
        IMAGE_FILE_MACHINE_MIPS16 = 0x266,
        IMAGE_FILE_MACHINE_MIPSFPU = 0x366,
        IMAGE_FILE_MACHINE_MIPSFPU16 = 0x466,
        IMAGE_FILE_MACHINE_POWERPC = 0x1f0,
        IMAGE_FILE_MACHINE_POWERPCFP = 0x1f1,
        IMAGE_FILE_MACHINE_R4000 = 0x166,
        IMAGE_FILE_MACHINE_SH3 = 0x1a2,
        IMAGE_FILE_MACHINE_SH3DSP = 0x1a3,
        IMAGE_FILE_MACHINE_SH4 = 0x1a6,
        IMAGE_FILE_MACHINE_SH5 = 0x1a8,
        IMAGE_FILE_MACHINE_THUMB = 0x1c2,
        IMAGE_FILE_MACHINE_WCEMIPSV2 = 0x169,
    }
    // ReSharper restore InconsistentNaming

    class Platform 
    {
        public static MachineType GetDllMachineType(string dllPath) 
        {
            //see http://www.microsoft.com/whdc/system/platform/firmware/PECOFF.mspx
            //offset to PE header is always at 0x3C
            //PE header starts with "PE\0\0" =  0x50 0x45 0x00 0x00
            //followed by 2-byte machine type field (see document above for enum)
            using (var fs = new FileStream(dllPath, FileMode.Open, FileAccess.Read)) 
            {
                var br = new BinaryReader(fs);
                fs.Seek(0x3c, SeekOrigin.Begin);
                Int32 peOffset = br.ReadInt32();
                fs.Seek(peOffset, SeekOrigin.Begin);
                UInt32 peHead = br.ReadUInt32();
                if (peHead != 0x00004550) // "PE\0\0", little-endian
                    throw new BadImageFormatException("Unable to determine the assembly's type. Can't find PE header");
                var machineType = (MachineType)br.ReadUInt16();
                br.Close();
                fs.Close();
                return machineType;
            }
        }

        /// <summary>
        /// returns true if the dll is 64-bit, false if 32-bit, and null if unknown
        /// </summary>
        /// <param name="dllPath"></param>
        /// <returns></returns>
        public static bool? Is64BitAssembly(string dllPath) 
        {
            switch (GetDllMachineType(dllPath)) 
            {
                case MachineType.IMAGE_FILE_MACHINE_AMD64:
                case MachineType.IMAGE_FILE_MACHINE_IA64:
                    return true;
                case MachineType.IMAGE_FILE_MACHINE_I386:
                    return false;
                default:
                    return null;
            }
        }
        public static bool IsRunningAs64Bit 
        {
            get
            {
                return Environment.Is64BitProcess;
                //var currentAssembly = Assembly.GetCallingAssembly();
                //PortableExecutableKinds kinds;
                //ImageFileMachine imgFileMachine;
                //currentAssembly.ManifestModule.GetPEKind(out kinds, out imgFileMachine);

                //if (kinds == PortableExecutableKinds.NotAPortableExecutableImage) return false;
                //var is64Bit = ((kinds & PortableExecutableKinds.PE32Plus) == PortableExecutableKinds.PE32Plus);
                //return is64Bit;
            }
        }
    }
}
