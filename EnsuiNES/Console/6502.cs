using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Diagnostics;

namespace EnsuiNES.Console
{
    class _6502
    {
        private NES bus;         
        private List<Constants.instruction> lookup;       

        public byte accumulator;
        public byte xRegister;
        public byte yRegister;
        public byte stackPointer;
        public ushort programCounter;
        public byte statusRegister;

        private byte cycles = 0;
        private byte opcode = 0x00;
        private byte fetchedData = 0x00;
        private ushort addressAbs = 0x0000;
        private ushort addressRel = 0x0000;

        public _6502()
        {
            this.reset();
            this.setLookup();
        }

        private void setLookup()
        {
            lookup = new List<Constants.instruction>();

            lookup.Add(new Constants.instruction { name = "BRK", cycles = 7, operation = BRK, addressMode = IMM });
            lookup.Add(new Constants.instruction { name = "ORA", cycles = 6, operation = ORA, addressMode = IZX });
            lookup.Add(new Constants.instruction { name = "???", cycles = 2, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 8, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 3, operation = NOP, addressMode = IMP });            
            lookup.Add(new Constants.instruction { name = "ORA", cycles = 3, operation = ORA, addressMode = ZP0 });
            lookup.Add(new Constants.instruction { name = "ASL", cycles = 5, operation = ASL, addressMode = ZP0 });
            lookup.Add(new Constants.instruction { name = "???", cycles = 5, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "PHP", cycles = 3, operation = PHP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "ORA", cycles = 2, operation = ORA, addressMode = IMM });
            lookup.Add(new Constants.instruction { name = "ASL", cycles = 2, operation = ASL, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 2, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 4, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "ORA", cycles = 4, operation = ORA, addressMode = ABS });
            lookup.Add(new Constants.instruction { name = "ASL", cycles = 6, operation = ASL, addressMode = ABS });
            lookup.Add(new Constants.instruction { name = "???", cycles = 6, operation = XXX, addressMode = IMP });

            lookup.Add(new Constants.instruction { name = "BPL", cycles = 2, operation = BPL, addressMode = REL });
            lookup.Add(new Constants.instruction { name = "ORA", cycles = 5, operation = ORA, addressMode = IZY });
            lookup.Add(new Constants.instruction { name = "???", cycles = 2, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 8, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 4, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "ORA", cycles = 4, operation = ORA, addressMode = ZPX });
            lookup.Add(new Constants.instruction { name = "ASL", cycles = 6, operation = ASL, addressMode = ZPX });
            lookup.Add(new Constants.instruction { name = "???", cycles = 6, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "CLC", cycles = 2, operation = CLC, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "ORA", cycles = 4, operation = ORA, addressMode = ABY });
            lookup.Add(new Constants.instruction { name = "???", cycles = 2, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 4, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "ORA", cycles = 4, operation = ORA, addressMode = ABX });
            lookup.Add(new Constants.instruction { name = "ASL", cycles = 7, operation = ASL, addressMode = ABX });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });

            lookup.Add(new Constants.instruction { name = "JSR", cycles = 6, operation = JSR, addressMode = ABS });
            lookup.Add(new Constants.instruction { name = "AND", cycles = 6, operation = AND, addressMode = IZX });
            lookup.Add(new Constants.instruction { name = "???", cycles = 2, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 8, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "BIT", cycles = 3, operation = BIT, addressMode = ZP0 });
            lookup.Add(new Constants.instruction { name = "AND", cycles = 3, operation = AND, addressMode = ZP0 });
            lookup.Add(new Constants.instruction { name = "ROL", cycles = 5, operation = ROL, addressMode = ZP0 });
            lookup.Add(new Constants.instruction { name = "???", cycles = 5, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "PLP", cycles = 4, operation = PLP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "AND", cycles = 2, operation = AND, addressMode = IMM });
            lookup.Add(new Constants.instruction { name = "ROL", cycles = 2, operation = ROL, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 2, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "BIT", cycles = 4, operation = BIT, addressMode = ABS });
            lookup.Add(new Constants.instruction { name = "AND", cycles = 4, operation = AND, addressMode = ABS });
            lookup.Add(new Constants.instruction { name = "ROL", cycles = 6, operation = ROL, addressMode = ABS });
            lookup.Add(new Constants.instruction { name = "???", cycles = 6, operation = XXX, addressMode = IMP });

            lookup.Add(new Constants.instruction { name = "BMI", cycles = 2, operation = BMI, addressMode = REL });
            lookup.Add(new Constants.instruction { name = "AND", cycles = 5, operation = AND, addressMode = IZY });
            lookup.Add(new Constants.instruction { name = "???", cycles = 2, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 8, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 4, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "AND", cycles = 4, operation = AND, addressMode = ZPX });
            lookup.Add(new Constants.instruction { name = "ROL", cycles = 6, operation = ROL, addressMode = ZPX });
            lookup.Add(new Constants.instruction { name = "???", cycles = 6, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "SEC", cycles = 2, operation = SEC, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "AND", cycles = 4, operation = AND, addressMode = ABY });
            lookup.Add(new Constants.instruction { name = "???", cycles = 2, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 4, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "AND", cycles = 4, operation = AND, addressMode = ABX });
            lookup.Add(new Constants.instruction { name = "ROL", cycles = 7, operation = ROL, addressMode = ABX });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });

            lookup.Add(new Constants.instruction { name = "RTI", cycles = 6, operation = RTI, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "EOR", cycles = 6, operation = EOR, addressMode = IZX });
            lookup.Add(new Constants.instruction { name = "???", cycles = 2, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 8, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 3, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "EOR", cycles = 3, operation = EOR, addressMode = ZP0 });
            lookup.Add(new Constants.instruction { name = "LSR", cycles = 5, operation = LSR, addressMode = ZP0 });
            lookup.Add(new Constants.instruction { name = "???", cycles = 5, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "PHA", cycles = 3, operation = PHA, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "EOR", cycles = 2, operation = EOR, addressMode = IMM });
            lookup.Add(new Constants.instruction { name = "LSR", cycles = 2, operation = LSR, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 2, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "JMP", cycles = 3, operation = JMP, addressMode = ABS });
            lookup.Add(new Constants.instruction { name = "EOR", cycles = 4, operation = EOR, addressMode = ABS });
            lookup.Add(new Constants.instruction { name = "LSR", cycles = 6, operation = LSR, addressMode = ABS });
            lookup.Add(new Constants.instruction { name = "???", cycles = 6, operation = XXX, addressMode = IMP });

            lookup.Add(new Constants.instruction { name = "BVC", cycles = 2, operation = BVC, addressMode = REL });
            lookup.Add(new Constants.instruction { name = "EOR", cycles = 5, operation = EOR, addressMode = IZY });
            lookup.Add(new Constants.instruction { name = "???", cycles = 2, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 8, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 4, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "EOR", cycles = 4, operation = EOR, addressMode = ZPX });
            lookup.Add(new Constants.instruction { name = "LSR", cycles = 6, operation = LSR, addressMode = ZPX });
            lookup.Add(new Constants.instruction { name = "???", cycles = 6, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "CLI", cycles = 2, operation = CLI, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "EOR", cycles = 4, operation = EOR, addressMode = ABY });
            lookup.Add(new Constants.instruction { name = "???", cycles = 2, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 4, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "EOR", cycles = 4, operation = EOR, addressMode = ABX });
            lookup.Add(new Constants.instruction { name = "LSR", cycles = 7, operation = LSR, addressMode = ABX });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });

            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });

            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });

            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });

            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });

            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });

            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });

            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });

            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });

            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });

            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
        }

        public void Connect(NES console)
        {
            bus = console;
        }

        public void reset()
        {
            accumulator = 0x00;
            xRegister = 0x00;
            yRegister = 0x00;
            stackPointer = 0x00;
            programCounter = 0x0000;
            statusRegister = 0x00;
        }

        private void write(ushort address, byte data)
        {
            bus.write(address, data);
        }

        private byte read(ushort address)
        {
            return 0x00;
            return bus.read(address);
        }

        public void clock()
        {
            Debug.WriteLine("Clock");
            if (cycles == 0)
            {
                opcode = read(programCounter);
                programCounter++;
                cycles = lookup[opcode].cycles;
                byte additionalAddressCycle = lookup[opcode].addressMode();
                byte additionalOperationCycle = lookup[opcode].operation();

                cycles += (byte)(additionalAddressCycle & additionalOperationCycle);
            }

            cycles--;
        }

        public void IRQ()
        {

        }
        public void NMI()
        {
            
        }

        public byte fetchData()
        {

        }

        public void setFlag(Constants.flags flag, bool value)
        {

        }

        public byte getFlag(Constants.flags flag)
        {
            return 0x00;
        }

        //Addressing Modes
        private byte IMM()
        {
            Debug.WriteLine("IMM");
            return 0x00;
        }

        private byte IMP()
        {

        }

        private byte ZP0()
        {

        }
        private byte ZPX()
        {

        }

        private byte ZPY()
        {

        }
        private byte REL()
        {

        }
        private byte ABS()
        {

        }

        private byte ABX()
        {

        }
        private byte ABY()
        {

        }

        private byte IND()
        {

        }

        private byte IZX()
        {

        }
        private byte IZY()
        {

        }

        //Opcodes

        private byte BRK()
        {
            Debug.WriteLine("BRK");
            return 0x00;
        }

        private byte XXX()
        {
            return 0x00;
        }
    }
}
