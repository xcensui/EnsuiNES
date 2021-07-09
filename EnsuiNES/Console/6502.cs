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

            lookup.Add(new Constants.instruction { name = "RTS", cycles = 6, operation = RTS, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "ADC", cycles = 5, operation = ADC, addressMode = IZX });
            lookup.Add(new Constants.instruction { name = "???", cycles = 2, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 8, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 3, operation = NOP, addressMode = IMP });
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
            lookup.Add(new Constants.instruction { name = "???", cycles = 4, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "ADC", cycles = 4, operation = ADC, addressMode = ZPX });
            lookup.Add(new Constants.instruction { name = "ROR", cycles = 6, operation = ROR, addressMode = ZPX });
            lookup.Add(new Constants.instruction { name = "???", cycles = 6, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "SEI", cycles = 7, operation = SEI, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "ADC", cycles = 7, operation = ADC, addressMode = ABY });
            lookup.Add(new Constants.instruction { name = "???", cycles = 2, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 4, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "ADC", cycles = 4, operation = ADC, addressMode = ABX });
            lookup.Add(new Constants.instruction { name = "ROR", cycles = 7, operation = ROR, addressMode = ABX });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });

            lookup.Add(new Constants.instruction { name = "???", cycles = 2, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "STA", cycles = 6, operation = STA, addressMode = IZX });
            lookup.Add(new Constants.instruction { name = "???", cycles = 2, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 6, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "STY", cycles = 3, operation = STY, addressMode = ZP0 });
            lookup.Add(new Constants.instruction { name = "STA", cycles = 3, operation = STA, addressMode = ZP0 });
            lookup.Add(new Constants.instruction { name = "STX", cycles = 3, operation = STX, addressMode = ZP0 });
            lookup.Add(new Constants.instruction { name = "???", cycles = 3, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "DEY", cycles = 2, operation = DEY, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 2, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "TXA", cycles = 2, operation = TXA, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 2, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "STY", cycles = 4, operation = STY, addressMode = ABS });
            lookup.Add(new Constants.instruction { name = "STA", cycles = 4, operation = STA, addressMode = ABS });
            lookup.Add(new Constants.instruction { name = "STX", cycles = 4, operation = STX, addressMode = ABS });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });

            lookup.Add(new Constants.instruction { name = "BCC", cycles = 2, operation = BCC, addressMode = REL });
            lookup.Add(new Constants.instruction { name = "STA", cycles = 6, operation = XXX, addressMode = IZY });
            lookup.Add(new Constants.instruction { name = "???", cycles = 2, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 6, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "STY", cycles = 4, operation = STY, addressMode = ZPX });
            lookup.Add(new Constants.instruction { name = "STA", cycles = 4, operation = STA, addressMode = ZPX });
            lookup.Add(new Constants.instruction { name = "STX", cycles = 4, operation = STX, addressMode = ZPY });
            lookup.Add(new Constants.instruction { name = "???", cycles = 4, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "TYA", cycles = 2, operation = TYA, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "STA", cycles = 5, operation = STA, addressMode = ABY });
            lookup.Add(new Constants.instruction { name = "TXS", cycles = 2, operation = TXS, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 5, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 5, operation = NOP, addressMode = IMP });
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
            lookup.Add(new Constants.instruction { name = "???", cycles = 2, operation = NOP, addressMode = IMP });
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
            lookup.Add(new Constants.instruction { name = "???", cycles = 4, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "CMP", cycles = 4, operation = CMP, addressMode = ZPX });
            lookup.Add(new Constants.instruction { name = "DEC", cycles = 6, operation = DEC, addressMode = ZPX });
            lookup.Add(new Constants.instruction { name = "???", cycles = 6, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "CLD", cycles = 2, operation = CLD, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "CMP", cycles = 4, operation = CMP, addressMode = ABY });
            lookup.Add(new Constants.instruction { name = "???", cycles = 2, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 4, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "CMP", cycles = 4, operation = CMP, addressMode = ABX });
            lookup.Add(new Constants.instruction { name = "DEC", cycles = 7, operation = DEC, addressMode = ABX });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });

            lookup.Add(new Constants.instruction { name = "CPX", cycles = 2, operation = CPX, addressMode = IMM });
            lookup.Add(new Constants.instruction { name = "SBC", cycles = 6, operation = SBC, addressMode = IZX });
            lookup.Add(new Constants.instruction { name = "???", cycles = 2, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 8, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "CPX", cycles = 3, operation = CPX, addressMode = ZP0 });
            lookup.Add(new Constants.instruction { name = "SBC", cycles = 3, operation = SBC, addressMode = ZP0 });
            lookup.Add(new Constants.instruction { name = "INC", cycles = 5, operation = INC, addressMode = ZP0 });
            lookup.Add(new Constants.instruction { name = "???", cycles = 5, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "INX", cycles = 2, operation = INX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "SBC", cycles = 2, operation = SBC, addressMode = IMM });
            lookup.Add(new Constants.instruction { name = "???", cycles = 2, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "SBC", cycles = 2, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "CPX", cycles = 4, operation = CPX, addressMode = ABS });
            lookup.Add(new Constants.instruction { name = "SBC", cycles = 4, operation = SBC, addressMode = ABS });
            lookup.Add(new Constants.instruction { name = "INC", cycles = 7, operation = INC, addressMode = ABS });
            lookup.Add(new Constants.instruction { name = "???", cycles = 6, operation = XXX, addressMode = IMP });

            lookup.Add(new Constants.instruction { name = "BEQ", cycles = 2, operation = BEQ, addressMode = REL });
            lookup.Add(new Constants.instruction { name = "SBC", cycles = 5, operation = SBC, addressMode = IZY });
            lookup.Add(new Constants.instruction { name = "???", cycles = 2, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 8, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 4, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "SBC", cycles = 4, operation = SBC, addressMode = ZPX });
            lookup.Add(new Constants.instruction { name = "INC", cycles = 6, operation = INC, addressMode = ZPX });
            lookup.Add(new Constants.instruction { name = "???", cycles = 6, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "SED", cycles = 2, operation = SED, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "SBC", cycles = 4, operation = SBC, addressMode = ABY });
            lookup.Add(new Constants.instruction { name = "???", cycles = 2, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 7, operation = XXX, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "???", cycles = 4, operation = NOP, addressMode = IMP });
            lookup.Add(new Constants.instruction { name = "SBC", cycles = 4, operation = SBC, addressMode = ABX });
            lookup.Add(new Constants.instruction { name = "INC", cycles = 7, operation = INC, addressMode = ABX });
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

            cycles = 0;
            opcode = 0x00;
            fetchedData = 0x00;
            addressAbs = 0x0000;
            addressRel = 0x0000;
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
