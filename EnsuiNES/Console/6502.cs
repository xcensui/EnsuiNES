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

        private byte cycles;
        private byte opcode;
        private byte fetchedData;
        private ushort addressAbs;
        private ushort addressRel;

        private bool inCycle = false;

        private bool boundaryBug;

        private List<byte> nopCycles;

        public _6502()
        {
            this.setLookup();

            nopCycles = new List<byte>();
            nopCycles.Add(0x1C);
            nopCycles.Add(0x3C);
            nopCycles.Add(0x5C);
            nopCycles.Add(0x7C);
            nopCycles.Add(0xDC);
            nopCycles.Add(0xFC);
        }

        public ushort pCounter
        {
            get
            {
                while (inCycle)
                {
                }

                return programCounter;
            }
        }

        public byte acc
        {
            get => accumulator;
        }

        public byte xReg
        {
            get => xRegister;
        }

        public byte yReg
        {
            get => yRegister;
        }

        public byte stack
        {
            get => stackPointer;
        }

        private void setLookup()
        {
            lookup = new List<Constants.instruction>();

            lookup.Add(new Constants.instruction { name = "BRK", cycles = 7, operation = BRK, addressMode = IMM });
            lookup.Add(new Constants.instruction { name = "ORA", cycles = 6, operation = ORA, addressMode = IZX });
            lookup.Add(new Constants.instruction { name = "???", cycles = 2, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 8, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "NOP", cycles = 3, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "ORA", cycles = 3, operation = ORA, addressMode = ZP0 });
            lookup.Add(new Constants.instruction { name = "ASL", cycles = 5, operation = ASL, addressMode = ZP0 });
            lookup.Add(new Constants.instruction { name = "???", cycles = 5, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "PHP", cycles = 3, operation = PHP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "ORA", cycles = 2, operation = ORA, addressMode = IMM });
            lookup.Add(new Constants.instruction { name = "ASL", cycles = 2, operation = ASL, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 2, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "NOP", cycles = 4, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "ORA", cycles = 4, operation = ORA, addressMode = ABS });
            lookup.Add(new Constants.instruction { name = "ASL", cycles = 6, operation = ASL, addressMode = ABS });
            lookup.Add(new Constants.instruction { name = "???", cycles = 6, operation = XXX, addressMode = IMP });

            lookup.Add(new Constants.instruction { name = "BPL", cycles = 2, operation = BPL, addressMode = REL });
            lookup.Add(new Constants.instruction { name = "ORA", cycles = 5, operation = ORA, addressMode = IZY });
            lookup.Add(new Constants.instruction { name = "???", cycles = 2, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 8, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "NOP", cycles = 4, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "ORA", cycles = 4, operation = ORA, addressMode = ZPX });
            lookup.Add(new Constants.instruction { name = "ASL", cycles = 6, operation = ASL, addressMode = ZPX });
            lookup.Add(new Constants.instruction { name = "???", cycles = 6, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "CLC", cycles = 2, operation = CLC, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "ORA", cycles = 4, operation = ORA, addressMode = ABY });
            lookup.Add(new Constants.instruction { name = "NOP", cycles = 2, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "NOP", cycles = 4, operation = NOP, addressMode = IMP });
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
            lookup.Add(new Constants.instruction { name = "NOP", cycles = 4, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "AND", cycles = 4, operation = AND, addressMode = ZPX });
            lookup.Add(new Constants.instruction { name = "ROL", cycles = 6, operation = ROL, addressMode = ZPX });
            lookup.Add(new Constants.instruction { name = "???", cycles = 6, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "SEC", cycles = 2, operation = SEC, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "AND", cycles = 4, operation = AND, addressMode = ABY });
            lookup.Add(new Constants.instruction { name = "NOP", cycles = 2, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "NOP", cycles = 4, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "AND", cycles = 4, operation = AND, addressMode = ABX });
            lookup.Add(new Constants.instruction { name = "ROL", cycles = 7, operation = ROL, addressMode = ABX });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });

            lookup.Add(new Constants.instruction { name = "RTI", cycles = 6, operation = RTI, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "EOR", cycles = 6, operation = EOR, addressMode = IZX });
            lookup.Add(new Constants.instruction { name = "???", cycles = 2, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 8, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "NOP", cycles = 3, operation = NOP, addressMode = IMP });
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
            lookup.Add(new Constants.instruction { name = "NOP", cycles = 4, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "EOR", cycles = 4, operation = EOR, addressMode = ZPX });
            lookup.Add(new Constants.instruction { name = "LSR", cycles = 6, operation = LSR, addressMode = ZPX });
            lookup.Add(new Constants.instruction { name = "???", cycles = 6, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "CLI", cycles = 2, operation = CLI, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "EOR", cycles = 4, operation = EOR, addressMode = ABY });
            lookup.Add(new Constants.instruction { name = "NOP", cycles = 2, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "NOP", cycles = 4, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "EOR", cycles = 4, operation = EOR, addressMode = ABX });
            lookup.Add(new Constants.instruction { name = "LSR", cycles = 7, operation = LSR, addressMode = ABX });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });

            lookup.Add(new Constants.instruction { name = "RTS", cycles = 6, operation = RTS, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "ADC", cycles = 5, operation = ADC, addressMode = IZX });
            lookup.Add(new Constants.instruction { name = "???", cycles = 2, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 8, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "NOP", cycles = 3, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "ADC", cycles = 3, operation = ADC, addressMode = ZP0 });
            lookup.Add(new Constants.instruction { name = "ROR", cycles = 5, operation = ROR, addressMode = ZP0 });
            lookup.Add(new Constants.instruction { name = "???", cycles = 5, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "PLA", cycles = 4, operation = PLA, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "ADC", cycles = 2, operation = ADC, addressMode = IMM });
            lookup.Add(new Constants.instruction { name = "ROR", cycles = 2, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 2, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "JMP", cycles = 5, operation = JMP, addressMode = IND });
            lookup.Add(new Constants.instruction { name = "ADC", cycles = 4, operation = ADC, addressMode = ABS });
            lookup.Add(new Constants.instruction { name = "ROR", cycles = 6, operation = ROR, addressMode = ABS });
            lookup.Add(new Constants.instruction { name = "???", cycles = 6, operation = XXX, addressMode = IMP });

            lookup.Add(new Constants.instruction { name = "BVS", cycles = 2, operation = BVS, addressMode = REL });
            lookup.Add(new Constants.instruction { name = "ADC", cycles = 5, operation = ADC, addressMode = IZY });
            lookup.Add(new Constants.instruction { name = "???", cycles = 2, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 8, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "NOP", cycles = 4, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "ADC", cycles = 4, operation = ADC, addressMode = ZPX });
            lookup.Add(new Constants.instruction { name = "ROR", cycles = 6, operation = ROR, addressMode = ZPX });
            lookup.Add(new Constants.instruction { name = "???", cycles = 6, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "SEI", cycles = 7, operation = SEI, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "ADC", cycles = 7, operation = ADC, addressMode = ABY });
            lookup.Add(new Constants.instruction { name = "NOP", cycles = 2, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "NOP", cycles = 4, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "ADC", cycles = 4, operation = ADC, addressMode = ABX });
            lookup.Add(new Constants.instruction { name = "ROR", cycles = 7, operation = ROR, addressMode = ABX });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });

            lookup.Add(new Constants.instruction { name = "NOP", cycles = 2, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "STA", cycles = 6, operation = STA, addressMode = IZX });
            lookup.Add(new Constants.instruction { name = "NOP", cycles = 2, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 6, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "STY", cycles = 3, operation = STY, addressMode = ZP0 });
            lookup.Add(new Constants.instruction { name = "STA", cycles = 3, operation = STA, addressMode = ZP0 });
            lookup.Add(new Constants.instruction { name = "STX", cycles = 3, operation = STX, addressMode = ZP0 });
            lookup.Add(new Constants.instruction { name = "???", cycles = 3, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "DEY", cycles = 2, operation = DEY, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "NOP", cycles = 2, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "TXA", cycles = 2, operation = TXA, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 2, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "STY", cycles = 4, operation = STY, addressMode = ABS });
            lookup.Add(new Constants.instruction { name = "STA", cycles = 4, operation = STA, addressMode = ABS });
            lookup.Add(new Constants.instruction { name = "STX", cycles = 4, operation = STX, addressMode = ABS });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });

            lookup.Add(new Constants.instruction { name = "BCC", cycles = 2, operation = BCC, addressMode = REL });
            lookup.Add(new Constants.instruction { name = "STA", cycles = 6, operation = XXX, addressMode = IZY });
            lookup.Add(new Constants.instruction { name = "NOP", cycles = 2, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 6, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "STY", cycles = 4, operation = STY, addressMode = ZPX });
            lookup.Add(new Constants.instruction { name = "STA", cycles = 4, operation = STA, addressMode = ZPX });
            lookup.Add(new Constants.instruction { name = "STX", cycles = 4, operation = STX, addressMode = ZPY });
            lookup.Add(new Constants.instruction { name = "???", cycles = 4, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "TYA", cycles = 2, operation = TYA, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "STA", cycles = 5, operation = STA, addressMode = ABY });
            lookup.Add(new Constants.instruction { name = "TXS", cycles = 2, operation = TXS, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 5, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "NOP", cycles = 5, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "STA", cycles = 5, operation = STA, addressMode = ABX });
            lookup.Add(new Constants.instruction { name = "???", cycles = 5, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 5, operation = XXX, addressMode = IMP });

            lookup.Add(new Constants.instruction { name = "LDY", cycles = 2, operation = LDY, addressMode = IMM });
            lookup.Add(new Constants.instruction { name = "LDA", cycles = 6, operation = LDA, addressMode = IZX });
            lookup.Add(new Constants.instruction { name = "LDX", cycles = 2, operation = LDX, addressMode = IMM });
            lookup.Add(new Constants.instruction { name = "???", cycles = 6, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "LDY", cycles = 3, operation = LDY, addressMode = ZP0 });
            lookup.Add(new Constants.instruction { name = "LDA", cycles = 3, operation = LDA, addressMode = ZP0 });
            lookup.Add(new Constants.instruction { name = "LDX", cycles = 3, operation = LDX, addressMode = ZP0 });
            lookup.Add(new Constants.instruction { name = "???", cycles = 3, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "TAY", cycles = 2, operation = TAY, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "LDA", cycles = 2, operation = LDA, addressMode = IMM });
            lookup.Add(new Constants.instruction { name = "TAX", cycles = 2, operation = TAX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 2, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "LDY", cycles = 4, operation = LDY, addressMode = ABS });
            lookup.Add(new Constants.instruction { name = "LDA", cycles = 4, operation = LDA, addressMode = ABS });
            lookup.Add(new Constants.instruction { name = "LDX", cycles = 4, operation = LDX, addressMode = ABS });
            lookup.Add(new Constants.instruction { name = "???", cycles = 4, operation = XXX, addressMode = IMP });


            lookup.Add(new Constants.instruction { name = "BCS", cycles = 2, operation = BCS, addressMode = REL });
            lookup.Add(new Constants.instruction { name = "LDA", cycles = 5, operation = LDA, addressMode = IZY });
            lookup.Add(new Constants.instruction { name = "???", cycles = 2, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 5, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "LDY", cycles = 4, operation = LDY, addressMode = ZPX });
            lookup.Add(new Constants.instruction { name = "LDA", cycles = 4, operation = LDA, addressMode = ZPX });
            lookup.Add(new Constants.instruction { name = "LDX", cycles = 4, operation = LDX, addressMode = ZPY });
            lookup.Add(new Constants.instruction { name = "???", cycles = 4, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "CLV", cycles = 2, operation = CLV, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "LDA", cycles = 4, operation = LDA, addressMode = ABY });
            lookup.Add(new Constants.instruction { name = "TSX", cycles = 2, operation = TSX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 4, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "LDY", cycles = 4, operation = LDY, addressMode = ABX });
            lookup.Add(new Constants.instruction { name = "LDA", cycles = 4, operation = LDA, addressMode = ABX });
            lookup.Add(new Constants.instruction { name = "LDX", cycles = 4, operation = LDX, addressMode = ABY });
            lookup.Add(new Constants.instruction { name = "???", cycles = 4, operation = XXX, addressMode = IMP });

            lookup.Add(new Constants.instruction { name = "CPY", cycles = 2, operation = CPY, addressMode = IMM });
            lookup.Add(new Constants.instruction { name = "CMP", cycles = 6, operation = CMP, addressMode = IZX });
            lookup.Add(new Constants.instruction { name = "NOP", cycles = 2, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 8, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "CPY", cycles = 3, operation = CPY, addressMode = ZP0 });
            lookup.Add(new Constants.instruction { name = "CMP", cycles = 3, operation = CMP, addressMode = ZP0 });
            lookup.Add(new Constants.instruction { name = "DEC", cycles = 5, operation = DEC, addressMode = ZP0 });
            lookup.Add(new Constants.instruction { name = "???", cycles = 5, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "INY", cycles = 2, operation = INY, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "CMP", cycles = 2, operation = CMP, addressMode = IMM });
            lookup.Add(new Constants.instruction { name = "DEX", cycles = 2, operation = DEX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 2, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "CPY", cycles = 4, operation = CPY, addressMode = ABS });
            lookup.Add(new Constants.instruction { name = "CMP", cycles = 4, operation = CMP, addressMode = ABS });
            lookup.Add(new Constants.instruction { name = "DEC", cycles = 6, operation = DEC, addressMode = ABS });
            lookup.Add(new Constants.instruction { name = "???", cycles = 6, operation = XXX, addressMode = IMP });

            lookup.Add(new Constants.instruction { name = "BNE", cycles = 2, operation = BNE, addressMode = REL });
            lookup.Add(new Constants.instruction { name = "CMP", cycles = 5, operation = CMP, addressMode = IZY });
            lookup.Add(new Constants.instruction { name = "???", cycles = 2, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 8, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "NOP", cycles = 4, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "CMP", cycles = 4, operation = CMP, addressMode = ZPX });
            lookup.Add(new Constants.instruction { name = "DEC", cycles = 6, operation = DEC, addressMode = ZPX });
            lookup.Add(new Constants.instruction { name = "???", cycles = 6, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "CLD", cycles = 2, operation = CLD, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "CMP", cycles = 4, operation = CMP, addressMode = ABY });
            lookup.Add(new Constants.instruction { name = "NOP", cycles = 2, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "NOP", cycles = 4, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "CMP", cycles = 4, operation = CMP, addressMode = ABX });
            lookup.Add(new Constants.instruction { name = "DEC", cycles = 7, operation = DEC, addressMode = ABX });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });

            lookup.Add(new Constants.instruction { name = "CPX", cycles = 2, operation = CPX, addressMode = IMM });
            lookup.Add(new Constants.instruction { name = "SBC", cycles = 6, operation = SBC, addressMode = IZX });
            lookup.Add(new Constants.instruction { name = "NOP", cycles = 2, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 8, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "CPX", cycles = 3, operation = CPX, addressMode = ZP0 });
            lookup.Add(new Constants.instruction { name = "SBC", cycles = 3, operation = SBC, addressMode = ZP0 });
            lookup.Add(new Constants.instruction { name = "INC", cycles = 5, operation = INC, addressMode = ZP0 });
            lookup.Add(new Constants.instruction { name = "???", cycles = 5, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "INX", cycles = 2, operation = INX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "SBC", cycles = 2, operation = SBC, addressMode = IMM });
            lookup.Add(new Constants.instruction { name = "NOP", cycles = 2, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "SBC", cycles = 2, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "CPX", cycles = 4, operation = CPX, addressMode = ABS });
            lookup.Add(new Constants.instruction { name = "SBC", cycles = 4, operation = SBC, addressMode = ABS });
            lookup.Add(new Constants.instruction { name = "INC", cycles = 7, operation = INC, addressMode = ABS });
            lookup.Add(new Constants.instruction { name = "???", cycles = 6, operation = XXX, addressMode = IMP });

            lookup.Add(new Constants.instruction { name = "BEQ", cycles = 2, operation = BEQ, addressMode = REL });
            lookup.Add(new Constants.instruction { name = "SBC", cycles = 5, operation = SBC, addressMode = IZY });
            lookup.Add(new Constants.instruction { name = "???", cycles = 2, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 8, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "NOP", cycles = 4, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "SBC", cycles = 4, operation = SBC, addressMode = ZPX });
            lookup.Add(new Constants.instruction { name = "INC", cycles = 6, operation = INC, addressMode = ZPX });
            lookup.Add(new Constants.instruction { name = "???", cycles = 6, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "SED", cycles = 2, operation = SED, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "SBC", cycles = 4, operation = SBC, addressMode = ABY });
            lookup.Add(new Constants.instruction { name = "NOP", cycles = 2, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "NOP", cycles = 4, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "SBC", cycles = 4, operation = SBC, addressMode = ABX });
            lookup.Add(new Constants.instruction { name = "INC", cycles = 7, operation = INC, addressMode = ABX });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
        }

        public void Connect(NES console)
        {
            bus = console;
        }

        public bool complete()
        {
            return cycles == 0;
        }

        public void reset()
        {
            inCycle = false;
            accumulator = 0x00;
            xRegister = 0x00;
            yRegister = 0x00;
            stackPointer = 0xFD;
            programCounter = getSetProgramCounter(0xFFFC);

            statusRegister = (byte)(0x00 | Constants.flags.U);

            cycles = 8;
            opcode = 0x00;
            fetchedData = 0x00;
            addressAbs = 0x0000;
            addressRel = 0x0000;

            while (!complete())
            {
                clock();
            }
        }

        private void write(ushort address, byte data)
        {
            bus.cpuWrite(address, data);
        }

        private byte read(ushort address)
        {
            return bus.cpuRead(address, false);
        }

        private ushort readPage()
        {
            boundaryBug = false;

            byte lo = read(programCounter);
            programCounter++;
            byte hi = read(programCounter);
            programCounter++;

            if (lo == 0x00FF) boundaryBug = true;

            return (ushort)((hi << 8) | lo);
        }

        public void clock()
        {
            if (cycles == 0)
            {
                inCycle = true;
                opcode = read(programCounter);
                programCounter++;
                cycles = lookup[opcode].cycles;
                byte additionalAddressCycle = lookup[opcode].addressMode();
                byte additionalOperationCycle = lookup[opcode].operation();

                cycles += (byte)(additionalAddressCycle & additionalOperationCycle);
            }

            inCycle = false;

            cycles--;
        }

        public void IRQ()
        {
            if (getFlag(Constants.flags.I) == 0)
            {
                performInterrupt();
                programCounter = getSetProgramCounter(0xFFFE);

                cycles = 7;
            }
        }
        public void NMI()
        {
            performInterrupt();
            programCounter = getSetProgramCounter(0xFFFA);

            cycles = 8;
        }

        public byte fetchData()
        {
            if (lookup[opcode].addressMode != IMP)
            {
                fetchedData = read(addressAbs);
            }

            return fetchedData;
        }

        public void setFlag(Constants.flags flag, bool value)
        {
            if (value)
            {
                statusRegister |= (byte)flag;
            }
            else
            {
                statusRegister &= (byte)~flag;
            }
        }

        public byte getFlag(Constants.flags flag)
        {
            return ((byte)((byte)(statusRegister & (byte)flag) > 0 ? 1 : 0));
        }

        //Addressing Modes
        private byte IMM()
        {
            addressAbs = programCounter;
            programCounter++;

            return 0;
        }

        private byte IMP()
        {
            fetchedData = accumulator;
            return 0;
        }

        private byte ZP0()
        {
            addressAbs = read(programCounter);
            programCounter++;
            addressAbs &= 0x00FF;

            return 0;
        }
        private byte ZPX()
        {
            addressAbs = (ushort)(read(programCounter) + xRegister);
            programCounter++;
            addressAbs &= 0x00FF;

            return 0;
        }

        private byte ZPY()
        {
            addressAbs = (ushort)(read(programCounter) + yRegister);
            programCounter++;
            addressAbs &= 0x00FF;

            return 0;
        }
        private byte REL()
        {
            addressRel = read(programCounter);
            programCounter++;

            if ((addressRel & 0x80) > 0)
            {
                addressRel |= 0xFF00;
            }

            return 0;
        }

        private byte ABS()
        {
            addressAbs = readPage();
            return 0;
        }

        private byte ABX()
        {
            byte lo = read(programCounter);
            programCounter++;
            byte hi = read(programCounter);
            programCounter++;

            addressAbs = (ushort)((hi << 8) | lo);
            addressAbs += xRegister;

            if ((addressAbs & 0xFF00) != (hi << 8))
            {
                return 1;
            }

            return 0;
        }

        private byte ABY()
        {
            byte lo = read(programCounter);
            programCounter++;
            byte hi = read(programCounter);
            programCounter++;

            addressAbs = (ushort)((hi << 8) | lo);
            addressAbs += yRegister;

            if ((addressAbs & 0xFF00) != (hi << 8))
            {
                return 1;
            }

            return 0;
        }

        private byte IND()
        {
            ushort pointer = readPage();

            if (boundaryBug)
            {
                addressAbs = (ushort)((read((ushort)(pointer & 0xFF00)) << 8) | read((ushort)(pointer + 0)));
            }
            else
            {
                addressAbs = (ushort)((read((ushort)(pointer + 1)) << 8) | read((ushort)(pointer + 0)));
            }

            return 0;
        }

        private byte IZX()
        {
            byte offset = read(programCounter);
            programCounter++;

            ushort lo = (ushort)(read((ushort)((offset + xRegister) & 0x00FF)));
            ushort hi = (ushort)(read((ushort)((offset + xRegister + 1) & 0x00FF)));

            addressAbs = (ushort)((hi << 8) | lo);

            return 0;
        }

        private byte IZY()
        {
            byte offset = read(programCounter);
            programCounter++;

            ushort lo = read((ushort)(offset & 0x00FF));
            ushort hi = read((ushort)((offset + 1) & 0x00FF));

            addressAbs = (ushort)((hi << 8) | lo);
            addressAbs += yRegister;

            if ((addressAbs & 0xFF00) != (hi << 8))
            {
                return 1;
            }

            return 0;
        }

        //Opcodes

        private byte AND()
        {
            fetchData();
            accumulator = (byte)(accumulator & fetchedData);

            setFlag(Constants.flags.Z, accumulator == 0x00);
            setFlag(Constants.flags.N, (accumulator & 0x80) > 0);

            return 1;
        }

        private byte BCS()
        {
            if (getFlag(Constants.flags.C) == 1)
            {
                branch();
            }

            return 0;
        }

        private byte BCC()
        {
            if (getFlag(Constants.flags.C) == 0)
            {
                branch();
            }

            return 0;
        }

        private byte BEQ()
        {
            if (getFlag(Constants.flags.Z) == 1)
            {
                branch();
            }

            return 0;
        }

        private byte BNE()
        {
            if (getFlag(Constants.flags.Z) == 0)
            {
                branch();
            }

            return 0;
        }

        private byte BMI()
        {
            if (getFlag(Constants.flags.N) == 1)
            {
                branch();
            }

            return 0;
        }

        private byte BPL()
        {
            if (getFlag(Constants.flags.N) == 0)
            {
                branch();
            }

            return 0;
        }

        private byte BVS()
        {
            if (getFlag(Constants.flags.V) == 1)
            {
                branch();
            }

            return 0;
        }

        private byte BVC()
        {
            if (getFlag(Constants.flags.V) == 0)
            {
                branch();
            }

            return 0;
        }

        private byte BRA()
        {
            branch();

            return 0;
        }

        private byte CLC()
        {
            setFlag(Constants.flags.C, false);

            return 0;
        }

        private byte CLD()
        {
            setFlag(Constants.flags.D, false);

            return 0;
        }

        private byte CLI()
        {
            setFlag(Constants.flags.I, false);

            return 0;
        }

        private byte CLV()
        {
            setFlag(Constants.flags.V, false);

            return 0;
        }

        private byte SEC()
        {
            setFlag(Constants.flags.C, true);

            return 0;
        }

        private byte SED()
        {
            setFlag(Constants.flags.D, true);

            return 0;
        }

        private byte SEI()
        {
            setFlag(Constants.flags.I, true);

            return 0;
        }

        private byte PHA()
        {
            write((ushort)(0x0100 + stackPointer), accumulator);
            stackPointer--;

            return 0;
        }

        private byte PHP()
        {
            write((ushort)(0x0100 + stackPointer), (byte)(statusRegister | (byte)Constants.flags.B | (byte)Constants.flags.U));

            setFlag(Constants.flags.B, false);
            setFlag(Constants.flags.U, false);

            stackPointer--;            

            return 0;
        }

        private byte PHX()
        {
            write((ushort)(0x0100 + stackPointer), xRegister);
            stackPointer--;

            return 0;
        }

        private byte PHY()
        {
            write((ushort)(0x0100 + stackPointer), yRegister);
            stackPointer--;

            return 0;
        }

        private byte PLA()
        {
            stackPointer++;
            accumulator = read((ushort)(0x0100 + stackPointer));

            setFlag(Constants.flags.Z, accumulator == 0x00);
            setFlag(Constants.flags.N, (accumulator & 0x80) > 0);
            
            return 0;
        }

        private byte PLX()
        {
            stackPointer++;
            accumulator = read((ushort)(0x0100 + xRegister));

            setFlag(Constants.flags.Z, xRegister== 0x00);
            setFlag(Constants.flags.N, (xRegister & 0x80) > 0);

            return 0;
        }

        private byte PLY()
        {
            stackPointer++;
            accumulator = read((ushort)(0x0100 + yRegister));

            setFlag(Constants.flags.Z, yRegister == 0x00);
            setFlag(Constants.flags.N, (yRegister & 0x80) > 0);

            return 0;
        }

        private byte ADC() {
            fetchData();

            ushort result = (ushort)((ushort)accumulator + fetchedData + (ushort)getFlag(Constants.flags.C));

            ushort overflow = (ushort)(~(ushort)(accumulator ^ (ushort)fetchedData) & ((ushort)accumulator ^ (ushort)result) & 0x0080);

            setFlag(Constants.flags.N, (result & 0x80) > 0);
            setFlag(Constants.flags.Z, (result & 0x00FF) == 0);
            setFlag(Constants.flags.C, (result > 255));
            setFlag(Constants.flags.V, overflow > 0);

            accumulator = (byte)(result & 0x00FF);

            return 1;
        }

        private byte SBC()
        {
            fetchData();

            ushort value = (ushort)(fetchedData ^ 0x00FF);
            ushort result = (ushort)(accumulator + value + getFlag(Constants.flags.C));

            ushort overflow = (ushort)((ushort)(result ^ accumulator) & (result ^ value) & 0x0080);

            setFlag(Constants.flags.N, (result & 0x0080) > 0);
            setFlag(Constants.flags.Z, (result & 0x00FF) == 0);
            setFlag(Constants.flags.C, (result > 255));
            setFlag(Constants.flags.V, overflow > 0);

            accumulator = (byte)(result & 0x00FF);

            return 1;
        }

        private byte ORA()
        {
            fetchData();
            accumulator = (byte)(accumulator | fetchedData);

            setFlag(Constants.flags.N, (accumulator & 0x80) > 0);
            setFlag(Constants.flags.Z, accumulator == 0);

            return 1;
        }

        private byte RTI()
        {
            statusRegister = fetchStack();

            programCounter = fetchStack();
            programCounter |= (ushort)(fetchStack() << 8);

            return 0;
        }

        private byte INC()
        {
            fetchData();
            byte result = fetchedData++;

            write(addressAbs, result);

            setFlag(Constants.flags.N, (result & 0x0080) > 0);
            setFlag(Constants.flags.Z, (result & 0x00FF) == 0);

            return 0;
        }

        private byte DEC()
        {
            fetchData();
            byte result = fetchedData--;

            write(addressAbs, result);

            setFlag(Constants.flags.N, (result & 0x0080) > 0);
            setFlag(Constants.flags.Z, (result & 0x00FF) == 0);

            return 0;
        }

        private byte INY()
        {
            yRegister++;

            setFlag(Constants.flags.N, (yRegister & 0x80) > 0);
            setFlag(Constants.flags.Z, (yRegister & 0xFF) == 0);

            return 0;
        }

        private byte INX()
        {
            xRegister++;

            setFlag(Constants.flags.N, (xRegister & 0x80) > 0);
            setFlag(Constants.flags.Z, (xRegister & 0xFF) == 0);

            return 0;
        }

        private byte DEY()
        {
            yRegister--;

            setFlag(Constants.flags.N, (yRegister & 0x80) > 0);
            setFlag(Constants.flags.Z, (yRegister & 0xFF) == 0);

            return 0;
        }

        private byte DEX()
        {
            xRegister--;

            setFlag(Constants.flags.N, (xRegister & 0x80) > 0);
            setFlag(Constants.flags.Z, (xRegister & 0xFF) == 0);

            return 0;
        }

        private byte LSR()
        {
            fetchData();
            ushort result = (ushort)(fetchedData >> 1);

            setFlag(Constants.flags.C, (fetchedData & 0x0001) > 0);
            setFlag(Constants.flags.N, (result & 0x0080) > 0);
            setFlag(Constants.flags.Z, (result & 0x00FF) == 0);

            if (lookup[opcode].addressMode == IMP)
            {
                accumulator = (byte)(result & 0x00FF);
            }

            write(addressAbs, (byte)(result & 0x00FF));

            return 0;
        }

        private byte ROL()
        {
            fetchData();

            ushort result = (ushort)((fetchedData << 1) | (byte)(Constants.flags.C));

            setFlag(Constants.flags.N, (result & 0x0080) > 0);
            setFlag(Constants.flags.Z, (result & 0x00FF) == 0);
            setFlag(Constants.flags.C, (result & 0xFF00) > 0);

            if (lookup[opcode].addressMode == IMP)
            {
                accumulator = (byte)(result & 0x00FF);
            }

            write(addressAbs, (byte)(result & 0x00FF));

            return 0;
        }

        private byte EOR()
        {
            fetchData();

            accumulator = (byte)(accumulator ^ fetchedData);

            setFlag(Constants.flags.N, (accumulator & 0x80) > 0);
            setFlag(Constants.flags.Z, accumulator == 0);

            return 1;
        }

        private byte ROR()
        {
            fetchData();

            ushort result = (ushort)((byte)((byte)(Constants.flags.C) << 7) | (fetchedData >> 1));

            setFlag(Constants.flags.N, (result & 0x0080) > 0);
            setFlag(Constants.flags.Z, (result & 0x00FF) == 0);
            setFlag(Constants.flags.C, (result & 0xFF00) > 0);

            if (lookup[opcode].addressMode == IMP)
            {
                accumulator = (byte)(result & 0x00FF);
            }

            write(addressAbs, (byte)(result & 0x00FF));

            return 0;
        }

        private byte ASL()
        {
            fetchData();

            ushort result = (ushort)(fetchedData << 1);

            setFlag(Constants.flags.N, (result & 0x0080) > 0);
            setFlag(Constants.flags.Z, (result & 0x00FF) == 0);
            setFlag(Constants.flags.C, (result & 0xFF00) > 0);

            if (lookup[opcode].addressMode == IMP)
            {
                accumulator = (byte)(result & 0x00FF);
            }

            write(addressAbs, (byte)(result & 0x00FF));

            return 0;
        }

        private byte BIT()
        {
            fetchData();

            byte result = (byte)(accumulator & fetchedData);

            setFlag(Constants.flags.N, (fetchedData & (1 << 7)) > 0);
            setFlag(Constants.flags.Z, (result & 0x00FF) == 0);
            setFlag(Constants.flags.V, (fetchedData & (1 << 6)) > 0);

            return 0;
        }

        private byte PLP()
        {
            statusRegister = fetchStack();

            setFlag(Constants.flags.U, true);

            return 0;
        }

        private byte JMP()
        {
            programCounter = addressAbs;

            return 0;
        }

        private byte JSR()
        {
            programCounter--;

            pushProgramCounter();

            programCounter = addressAbs;

            return 0;
        }

        private byte BRK()
        {
            programCounter++;

            setFlag(Constants.flags.I, true);

            pushProgramCounter();

            setFlag(Constants.flags.B, true);
            
            write((ushort)(0x0100 + stackPointer), statusRegister);
            stackPointer--;

            setFlag(Constants.flags.B, false);

            programCounter = getSetProgramCounter(0xFFFE);

            return 0;
        }

        private byte RTS()
        {
            ushort lo = fetchStack();
            ushort hi = fetchStack();

            programCounter = (ushort)((hi << 8) | lo);
            programCounter++;

            return 0;
        }

        private byte STA()
        {
            write(addressAbs, accumulator);

            return 0;
        }

        private byte STY()
        {
            write(addressAbs, yRegister);

            return 0;
        }

        private byte STX()
        {
            write(addressAbs, xRegister);

            return 0;
        }
        
        private byte LDA()
        {
            fetchData();
            accumulator = fetchedData;

            setFlag(Constants.flags.N, (accumulator & 0x80) > 0);
            setFlag(Constants.flags.Z, accumulator == 0);

            return 1;
        }

        private byte LDX()
        {
            fetchData();
            xRegister = fetchedData;

            setFlag(Constants.flags.N, (xRegister & 0x80) > 0);
            setFlag(Constants.flags.Z, xRegister == 0);

            return 1;
        }

        private byte LDY()
        {
            fetchData();
            yRegister = fetchedData;

            setFlag(Constants.flags.N, (yRegister & 0x80) > 0);
            setFlag(Constants.flags.Z, yRegister == 0);

            return 1;
        }

        private byte TAX()
        {
            xRegister = accumulator;

            setFlag(Constants.flags.N, (xRegister & 0x80) > 0);
            setFlag(Constants.flags.Z, xRegister == 0);

            return 0;
        }

        private byte TAY()
        {
            yRegister = accumulator;

            setFlag(Constants.flags.N, (yRegister & 0x80) > 0);
            setFlag(Constants.flags.Z, yRegister == 0);

            return 0;
        }

        private byte TSX()
        {
            xRegister = stackPointer;

            setFlag(Constants.flags.N, (xRegister & 0x80) > 0);
            setFlag(Constants.flags.Z, xRegister == 0);

            return 0;
        }

        private byte TXA()
        {
            accumulator = xRegister;

            setFlag(Constants.flags.N, (accumulator & 0x80) > 0);
            setFlag(Constants.flags.Z, accumulator == 0);

            return 0;
        }

        private byte TXS()
        {
            stackPointer = xRegister;

            return 0;
        }

        private byte TYA()
        {
            accumulator = yRegister;

            setFlag(Constants.flags.N, (accumulator & 0x80) > 0);
            setFlag(Constants.flags.Z, accumulator == 0);

            return 0;
        }

        private byte CMP()
        {
            fetchData();

            ushort result = (ushort)((ushort)(accumulator) - fetchedData);

            setFlag(Constants.flags.N, (result & 0x0080) > 0);
            setFlag(Constants.flags.Z, (result & 0x00FF) == 0);
            setFlag(Constants.flags.C, (accumulator >= fetchedData));

            return 1;
        }

        private byte CPX()
        {
            fetchData();

            ushort result = (ushort)(xRegister - fetchedData);

            setFlag(Constants.flags.N, (result & 0x0080) > 0);
            setFlag(Constants.flags.Z, (result & 0x00FF) == 0);
            setFlag(Constants.flags.C, (xRegister >= fetchedData));

            return 1;
        }

        private byte CPY()
        {
            fetchData();

            ushort result = (ushort)(yRegister - fetchedData);

            setFlag(Constants.flags.N, (result & 0x0080) > 0);
            setFlag(Constants.flags.Z, (result & 0x00FF) == 0);
            setFlag(Constants.flags.C, (yRegister >= fetchedData));

            return 1;
        }

        private byte XXX()
        {
            return 0;
        }

        private byte NOP()
        {
            if (nopCycles.Contains(opcode))
            {
                return 1;
            }

            return 0;
        }

        private void pushProgramCounter()
        {
            write((ushort)(0x0100 + stackPointer), (byte)((ushort)(programCounter >> 8) & 0x00FF));
            stackPointer--;
            write((ushort)(0x0100 + stackPointer), (byte)((ushort)(programCounter & 0x00FF)));
            stackPointer--;
        }

        private void branch()
        {
            cycles++;
            addressAbs = (ushort)(programCounter + addressRel);

            if ((addressAbs & 0xFF00) != (programCounter & 0xFF00))
            {
                cycles++;
            }

            programCounter = addressAbs;
        }

        private byte fetchStack()
        {
            stackPointer++;
            return read((byte)(0x0100 & stackPointer));
        }

        private ushort getSetProgramCounter(ushort address)
        {
            addressAbs = address;

            ushort lo = read((ushort)(addressAbs + 0));
            ushort hi = read((ushort)(addressAbs + 1));

            return (ushort)((hi << 8) | lo);
        }

        private void performInterrupt()
        {
            pushProgramCounter();

            setFlag(Constants.flags.B, false);
            setFlag(Constants.flags.U, true);
            setFlag(Constants.flags.I, true);

            write((ushort)(0x0100 + stackPointer), statusRegister);
            stackPointer--;
        }

        public SortedList<ushort, string> disassembled(ushort addressStart, ushort addressEnd)
        {
            ushort address = addressStart;
            
            byte value = 0x00;
            byte lo = 0x00;
            byte hi = 0x00;
            ushort readAddress = 0x0000;
            ushort lineAddress = 0x0000;
            string instruction = "";
            long count = 0;

            SortedList<ushort, string> lineMap = new SortedList<ushort, string>();

            while (address <= addressEnd)
            {
                count++;
                lineAddress = address;
                opcode = bus.cpuRead(address, true);
                address++;
                
                Constants.method addressMode = lookup[opcode].addressMode;

                instruction = String.Concat("$", lineAddress.ToString("X4"), ": ", lookup[opcode].name);

                if (addressMode == IMP)
                {
                    instruction = String.Concat(instruction, " {IMP}");
                }

                if (addressMode == IMM)
                {
                    value = bus.cpuRead(address, true);
                    address++;

                    instruction = String.Concat(instruction, " #$", value.ToString("X2"), " {IMM}");
                }

                if (addressMode == ZP0)
                {
                    lo = bus.cpuRead(address, true);                   
                    address++;

                    instruction = String.Concat(instruction, " $", lo.ToString("X2"), " {ZP0}");
                }

                if (addressMode == ZPX)
                {
                    lo = bus.cpuRead(address, true);
                    address++;

                    instruction = String.Concat(instruction, " $", lo.ToString("X2"), " {ZPX}");
                }

                if (addressMode == ZPY)
                {
                    lo = bus.cpuRead(address, true);
                    address++;

                    instruction = String.Concat(instruction, " $", lo.ToString("X2"), " {ZPY}");
                }

                if (addressMode == IZX)
                {
                    lo = bus.cpuRead(address, true);
                    address++;

                    instruction = String.Concat(instruction, " ($", lo.ToString("X2"), "), X {IZX}");
                }


                if (addressMode == IZY)
                {
                    lo = bus.cpuRead(address, true);
                    address++;

                    instruction = String.Concat(instruction, " ($", lo.ToString("X2"), "), Y {IZY}");
                }


                if (addressMode == ABS)
                {
                    lo = bus.cpuRead(address, true);
                    address++;
                    hi = bus.cpuRead(address, true);
                    address++;

                    readAddress = (ushort)((hi << 8) | lo);

                    instruction = String.Concat(instruction, " $", readAddress.ToString("X4"), " {ABS}");
                }


                if (addressMode == ABX)
                {
                    lo = bus.cpuRead(address, true);
                    address++;
                    hi = bus.cpuRead(address, true);
                    address++;

                    readAddress = (ushort)((hi << 8) | lo);

                    instruction = String.Concat(instruction, " $", readAddress.ToString("X4"), " X {ABX}");
                }


                if (addressMode == ABY)
                {
                    lo = bus.cpuRead(address, true);
                    address++;
                    hi = bus.cpuRead(address, true);
                    address++;

                    readAddress = (ushort)((hi << 8) | lo);

                    instruction = String.Concat(instruction, " $", readAddress.ToString("X4"), " Y {ABY}");
                }


                if (addressMode == IND)
                {
                    lo = bus.cpuRead(address, true);
                    address++;
                    hi = bus.cpuRead(address, true);
                    address++;

                    readAddress = (ushort)((hi << 8) | lo);

                    instruction = String.Concat(instruction, " ($", readAddress.ToString("X4"), ") {IND}");
                }


                if (addressMode == REL)
                {
                    value = bus.cpuRead(address, true);
                    address++;

                    instruction = String.Concat(instruction, " $", value.ToString("X2"), " [$", ((ushort)(address + (sbyte)value)).ToString("X4"), "] {REL}");
                }

                if (address == 0 || address == addressEnd)
                {
                    break;
                }

                try
                {
                    lineMap.Add(lineAddress, instruction);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(String.Concat("LA: ", lineAddress.ToString("X4"), " - Ins: ", instruction));
                    Debug.WriteLine(String.Concat("'Overflow, ", count.ToString()));

                }
            }

            return lineMap;
        }
    }
}
