// Copyright 2024 Victor Chavez (vchavezb@protonmail.com)
// SPDX-License-Identifier: GPL-3.0-or-later
using Antmicro.Renode.Peripherals.Bus;
using Antmicro.Renode.Logging;
using Antmicro.Renode.Core;
using System;
using System.Linq;
using Antmicro.Renode.Exceptions;
using Antmicro.Renode.Peripherals.Memory;
using Antmicro.Renode.Peripherals.CPU;
using Antmicro.Renode.Logging.Profiling;

namespace Antmicro.Renode.Peripherals.Miscellaneous
{
    public class NRF_NVCM : IDoubleWordPeripheral, IKnownSize
    {
        private readonly IMachine machine;
        private readonly IBusController sysbus;
        private readonly MappedMemory  flash;
        public long Size => 0x550;
        private uint config = 0x0;        
        private const uint PAGE_SIZE = 0x1000;
        private enum Register : long
        {
            READY = 0x400,
            CONFIG = 0x504,
            ERASEPAGE = 0x508,
            ICACHECNF = 0x540
        }
        private enum ConfigVal : uint
        {
            ReadOnly = 0,
            WriteEn = 0x1,
            EraseEn = 0x2
        }

        public NRF_NVCM(IMachine machine, MappedMemory flash)
        {
            this.machine = machine;
            this.sysbus = machine.SystemBus;
            this.flash = flash;
        }

        public uint ReadDoubleWord(long offset)
        {
            switch(offset) {
                // assume no erase nor write operation is in progress
                case (long)Register.READY:
                    return 1;
            }
            return 1;
        }

        private void set_access_hook(Action<ulong, MemoryOperation, ulong, ulong, ulong> hook) {
            if(!this.sysbus.TryGetCurrentCPU(out var icpu)) {
                this.Log(LogLevel.Error, "Failed to get CPU");
                return;
            }
            var cpu = icpu as ICPUWithMemoryAccessHooks;
            if(cpu == null) {
                this.Log(LogLevel.Error, "CPU does not support memory access hooks, cannot trigger Write erase");
                return;
            }
            cpu.SetHookAtMemoryAccess(hook);
        }
        public void WriteDoubleWord(long offset, uint value)
        {
            switch(offset){
                case (long)Register.CONFIG:
                    config = value;
                    if (config == (uint)ConfigVal.WriteEn) {
                        set_access_hook(WriteMemoryAccessHook);
                    }
                    if (config == (uint)ConfigVal.ReadOnly) {
                        set_access_hook(null);
                    }
                    break;
                case (long)Register.ERASEPAGE:
                    if (config != (uint)ConfigVal.EraseEn) {
                        return;
                    }
                    byte[] erase_data = new byte[PAGE_SIZE];
                    for (int i = 0; i < PAGE_SIZE; i++) {
                        erase_data[i] = 0xFF;
                    }
                    flash.WriteBytes(value, erase_data);
                    break;
                case (long)Register.ICACHECNF:
                    // Ignore cache requests
                    break;
                default:
                    this.Log(LogLevel.Warning, "Writing to unknown register 0x{0:X} with value 0x{1:X}", offset, value);
                    break;
            }
        }

        public byte ReadByte(long offset)
        {
            return 0;
        }

        public void WriteByte(long offset, byte value)
        {
        }

        public void Reset()
        {
            config = 0x0;
        }

        private void WriteMemoryAccessHook(ulong pc, MemoryOperation operation, ulong virtualAddress, ulong physicalAddress, ulong value)
        {
            if (config != (uint)ConfigVal.WriteEn) {
                return;
            }
            var registered = this.sysbus.WhatPeripheralIsAt(physicalAddress);
            if(registered != flash) {
                return;
            }
            if (operation ==  MemoryOperation.MemoryWrite ) {
                byte[] data = new byte[4];
                data[0] = (byte)(value & 0xFF);
                data[1] = (byte)((value >> 8) & 0xFF);
                data[2] = (byte)((value >> 16) & 0xFF);
                data[3] = (byte)((value >> 24) & 0xFF);
                flash.WriteBytes((long)virtualAddress, data);
            }
        }
    }
}
