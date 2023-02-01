using static NNEmu.Hardware.INSTRUCTION;

namespace NNEmu.Hardware
{

    //ISTRUZIONE Cpu
    public class INSTRUCTION
    {
        public string Name { get; set; }
        public Func<byte> Operate { get; set; }
        public Func<byte> Addrmode { get; set; }
        public byte Cycles { get; set; }

        public INSTRUCTION (string name, Func<byte> operate, Func<byte> addrmode, byte cycles)
        {
            Name = name;
            Operate = operate;
            Addrmode = addrmode;
            Cycles = cycles;
        }
        //Set di bit per gli stati della CPU
        public enum FLAGS6502
        {
            C = 1 << 0,   // Carry Bit
            Z = 1 << 1,   // Zero
            I = 1 << 2,   // Disable Interrupts
            D = 1 << 3,   // Decimal Mode (unused in this implementation)
            B = 1 << 4,   // Break
            U = 1 << 5,   // Unused
            V = 1 << 6,   // Overflow
            N = 1 << 7,   // Negative
        };
    }


    //6502 Cpu

    //In questa implementazione si simula anche il numero di clock necessari per la varie operazioni.
    public class CPU
    {
        // Registri della Cpu
        public byte A = 0;       // Accumulator Register
        public byte X = 0;       // X Register
        public byte Y = 0;       // Y Register
        public byte Stkp = 0;        // Stack Pointer (points to location on bus)
        public ushort Pc = 0;   // Program Counter
        public byte Status = 0;      // Status Register

        // Variabili temporanee
        private byte Fetched = 0;
        private ushort Temp = 0;
        private ushort Addr_abs = 0;
        private ushort Addr_rel = 0;
        private byte Opcode = 0;
        private byte Cycles = 0;
        private ulong CpuClockCount = 0;

        // Riferimento al BUS
        private BUS Bus;

        //OP Table
        INSTRUCTION[] Lookup;

        public CPU(BUS bus)
        {

            INSTRUCTION[] tmpLook = {
                            new INSTRUCTION( "BRK", BRK, IMM, 7 ),new INSTRUCTION( "ORA", ORA, IZX, 6 ),new INSTRUCTION( "???", XXX, IMP, 2 ),new INSTRUCTION( "???", XXX, IMP, 8 ),new INSTRUCTION( "???", NOP, IMP, 3 ),new INSTRUCTION( "ORA", ORA, ZP0, 3 ),new INSTRUCTION( "ASL", ASL, ZP0, 5 ),new INSTRUCTION( "???", XXX, IMP, 5 ),new INSTRUCTION( "PHP", PHP, IMP, 3 ),new INSTRUCTION( "ORA", ORA, IMM, 2 ),new INSTRUCTION( "ASL", ASL, IMP, 2 ),new INSTRUCTION( "???", XXX, IMP, 2 ),new INSTRUCTION( "???", NOP, IMP, 4 ),new INSTRUCTION( "ORA", ORA, ABS, 4 ),new INSTRUCTION( "ASL", ASL, ABS, 6 ),new INSTRUCTION( "???", XXX, IMP, 6 ),
                            new INSTRUCTION( "BPL", BPL, REL, 2 ),new INSTRUCTION( "ORA", ORA, IZY, 5 ),new INSTRUCTION( "???", XXX, IMP, 2 ),new INSTRUCTION( "???", XXX, IMP, 8 ),new INSTRUCTION( "???", NOP, IMP, 4 ),new INSTRUCTION( "ORA", ORA, ZPX, 4 ),new INSTRUCTION( "ASL", ASL, ZPX, 6 ),new INSTRUCTION( "???", XXX, IMP, 6 ),new INSTRUCTION( "CLC", CLC, IMP, 2 ),new INSTRUCTION( "ORA", ORA, ABY, 4 ),new INSTRUCTION( "???", NOP, IMP, 2 ),new INSTRUCTION( "???", XXX, IMP, 7 ),new INSTRUCTION( "???", NOP, IMP, 4 ),new INSTRUCTION( "ORA", ORA, ABX, 4 ),new INSTRUCTION( "ASL", ASL, ABX, 7 ),new INSTRUCTION( "???", XXX, IMP, 7 ),
                            new INSTRUCTION( "JSR", JSR, ABS, 6 ),new INSTRUCTION( "AND", AND, IZX, 6 ),new INSTRUCTION( "???", XXX, IMP, 2 ),new INSTRUCTION( "???", XXX, IMP, 8 ),new INSTRUCTION( "BIT", BIT, ZP0, 3 ),new INSTRUCTION( "AND", AND, ZP0, 3 ),new INSTRUCTION( "ROL", ROL, ZP0, 5 ),new INSTRUCTION( "???", XXX, IMP, 5 ),new INSTRUCTION( "PLP", PLP, IMP, 4 ),new INSTRUCTION( "AND", AND, IMM, 2 ),new INSTRUCTION( "ROL", ROL, IMP, 2 ),new INSTRUCTION( "???", XXX, IMP, 2 ),new INSTRUCTION( "BIT", BIT, ABS, 4 ),new INSTRUCTION( "AND", AND, ABS, 4 ),new INSTRUCTION( "ROL", ROL, ABS, 6 ),new INSTRUCTION( "???", XXX, IMP, 6 ),
                            new INSTRUCTION( "BMI", BMI, REL, 2 ),new INSTRUCTION( "AND", AND, IZY, 5 ),new INSTRUCTION( "???", XXX, IMP, 2 ),new INSTRUCTION( "???", XXX, IMP, 8 ),new INSTRUCTION( "???", NOP, IMP, 4 ),new INSTRUCTION( "AND", AND, ZPX, 4 ),new INSTRUCTION( "ROL", ROL, ZPX, 6 ),new INSTRUCTION( "???", XXX, IMP, 6 ),new INSTRUCTION( "SEC", SEC, IMP, 2 ),new INSTRUCTION( "AND", AND, ABY, 4 ),new INSTRUCTION( "???", NOP, IMP, 2 ),new INSTRUCTION( "???", XXX, IMP, 7 ),new INSTRUCTION( "???", NOP, IMP, 4 ),new INSTRUCTION( "AND", AND, ABX, 4 ),new INSTRUCTION( "ROL", ROL, ABX, 7 ),new INSTRUCTION( "???", XXX, IMP, 7 ),
                            new INSTRUCTION( "RTI", RTI, IMP, 6 ),new INSTRUCTION( "EOR", EOR, IZX, 6 ),new INSTRUCTION( "???", XXX, IMP, 2 ),new INSTRUCTION( "???", XXX, IMP, 8 ),new INSTRUCTION( "???", NOP, IMP, 3 ),new INSTRUCTION( "EOR", EOR, ZP0, 3 ),new INSTRUCTION( "LSR", LSR, ZP0, 5 ),new INSTRUCTION( "???", XXX, IMP, 5 ),new INSTRUCTION( "PHA", PHA, IMP, 3 ),new INSTRUCTION( "EOR", EOR, IMM, 2 ),new INSTRUCTION( "LSR", LSR, IMP, 2 ),new INSTRUCTION( "???", XXX, IMP, 2 ),new INSTRUCTION( "JMP", JMP, ABS, 3 ),new INSTRUCTION( "EOR", EOR, ABS, 4 ),new INSTRUCTION( "LSR", LSR, ABS, 6 ),new INSTRUCTION( "???", XXX, IMP, 6 ),
                            new INSTRUCTION( "BVC", BVC, REL, 2 ),new INSTRUCTION( "EOR", EOR, IZY, 5 ),new INSTRUCTION( "???", XXX, IMP, 2 ),new INSTRUCTION( "???", XXX, IMP, 8 ),new INSTRUCTION( "???", NOP, IMP, 4 ),new INSTRUCTION( "EOR", EOR, ZPX, 4 ),new INSTRUCTION( "LSR", LSR, ZPX, 6 ),new INSTRUCTION( "???", XXX, IMP, 6 ),new INSTRUCTION( "CLI", CLI, IMP, 2 ),new INSTRUCTION( "EOR", EOR, ABY, 4 ),new INSTRUCTION( "???", NOP, IMP, 2 ),new INSTRUCTION( "???", XXX, IMP, 7 ),new INSTRUCTION( "???", NOP, IMP, 4 ),new INSTRUCTION( "EOR", EOR, ABX, 4 ),new INSTRUCTION( "LSR", LSR, ABX, 7 ),new INSTRUCTION( "???", XXX, IMP, 7 ),
                            new INSTRUCTION( "RTS", RTS, IMP, 6 ),new INSTRUCTION( "ADC", ADC, IZX, 6 ),new INSTRUCTION( "???", XXX, IMP, 2 ),new INSTRUCTION( "???", XXX, IMP, 8 ),new INSTRUCTION( "???", NOP, IMP, 3 ),new INSTRUCTION( "ADC", ADC, ZP0, 3 ),new INSTRUCTION( "ROR", ROR, ZP0, 5 ),new INSTRUCTION( "???", XXX, IMP, 5 ),new INSTRUCTION( "PLA", PLA, IMP, 4 ),new INSTRUCTION( "ADC", ADC, IMM, 2 ),new INSTRUCTION( "ROR", ROR, IMP, 2 ),new INSTRUCTION( "???", XXX, IMP, 2 ),new INSTRUCTION( "JMP", JMP, IND, 5 ),new INSTRUCTION( "ADC", ADC, ABS, 4 ),new INSTRUCTION( "ROR", ROR, ABS, 6 ),new INSTRUCTION( "???", XXX, IMP, 6 ),
                            new INSTRUCTION( "BVS", BVS, REL, 2 ),new INSTRUCTION( "ADC", ADC, IZY, 5 ),new INSTRUCTION( "???", XXX, IMP, 2 ),new INSTRUCTION( "???", XXX, IMP, 8 ),new INSTRUCTION( "???", NOP, IMP, 4 ),new INSTRUCTION( "ADC", ADC, ZPX, 4 ),new INSTRUCTION( "ROR", ROR, ZPX, 6 ),new INSTRUCTION( "???", XXX, IMP, 6 ),new INSTRUCTION( "SEI", SEI, IMP, 2 ),new INSTRUCTION( "ADC", ADC, ABY, 4 ),new INSTRUCTION( "???", NOP, IMP, 2 ),new INSTRUCTION( "???", XXX, IMP, 7 ),new INSTRUCTION( "???", NOP, IMP, 4 ),new INSTRUCTION( "ADC", ADC, ABX, 4 ),new INSTRUCTION( "ROR", ROR, ABX, 7 ),new INSTRUCTION( "???", XXX, IMP, 7 ),
                            new INSTRUCTION( "???", NOP, IMP, 2 ),new INSTRUCTION( "STA", STA, IZX, 6 ),new INSTRUCTION( "???", NOP, IMP, 2 ),new INSTRUCTION( "???", XXX, IMP, 6 ),new INSTRUCTION( "STY", STY, ZP0, 3 ),new INSTRUCTION( "STA", STA, ZP0, 3 ),new INSTRUCTION( "STX", STX, ZP0, 3 ),new INSTRUCTION( "???", XXX, IMP, 3 ),new INSTRUCTION( "DEY", DEY, IMP, 2 ),new INSTRUCTION( "???", NOP, IMP, 2 ),new INSTRUCTION( "TXA", TXA, IMP, 2 ),new INSTRUCTION( "???", XXX, IMP, 2 ),new INSTRUCTION( "STY", STY, ABS, 4 ),new INSTRUCTION( "STA", STA, ABS, 4 ),new INSTRUCTION( "STX", STX, ABS, 4 ),new INSTRUCTION( "???", XXX, IMP, 4 ),
                            new INSTRUCTION( "BCC", BCC, REL, 2 ),new INSTRUCTION( "STA", STA, IZY, 6 ),new INSTRUCTION( "???", XXX, IMP, 2 ),new INSTRUCTION( "???", XXX, IMP, 6 ),new INSTRUCTION( "STY", STY, ZPX, 4 ),new INSTRUCTION( "STA", STA, ZPX, 4 ),new INSTRUCTION( "STX", STX, ZPY, 4 ),new INSTRUCTION( "???", XXX, IMP, 4 ),new INSTRUCTION( "TYA", TYA, IMP, 2 ),new INSTRUCTION( "STA", STA, ABY, 5 ),new INSTRUCTION( "TXS", TXS, IMP, 2 ),new INSTRUCTION( "???", XXX, IMP, 5 ),new INSTRUCTION( "???", NOP, IMP, 5 ),new INSTRUCTION( "STA", STA, ABX, 5 ),new INSTRUCTION( "???", XXX, IMP, 5 ),new INSTRUCTION( "???", XXX, IMP, 5 ),
                            new INSTRUCTION( "LDY", LDY, IMM, 2 ),new INSTRUCTION( "LDA", LDA, IZX, 6 ),new INSTRUCTION( "LDX", LDX, IMM, 2 ),new INSTRUCTION( "???", XXX, IMP, 6 ),new INSTRUCTION( "LDY", LDY, ZP0, 3 ),new INSTRUCTION( "LDA", LDA, ZP0, 3 ),new INSTRUCTION( "LDX", LDX, ZP0, 3 ),new INSTRUCTION( "???", XXX, IMP, 3 ),new INSTRUCTION( "TAY", TAY, IMP, 2 ),new INSTRUCTION( "LDA", LDA, IMM, 2 ),new INSTRUCTION( "TAX", TAX, IMP, 2 ),new INSTRUCTION( "???", XXX, IMP, 2 ),new INSTRUCTION( "LDY", LDY, ABS, 4 ),new INSTRUCTION( "LDA", LDA, ABS, 4 ),new INSTRUCTION( "LDX", LDX, ABS, 4 ),new INSTRUCTION( "???", XXX, IMP, 4 ),
                            new INSTRUCTION( "BCS", BCS, REL, 2 ),new INSTRUCTION( "LDA", LDA, IZY, 5 ),new INSTRUCTION( "???", XXX, IMP, 2 ),new INSTRUCTION( "???", XXX, IMP, 5 ),new INSTRUCTION( "LDY", LDY, ZPX, 4 ),new INSTRUCTION( "LDA", LDA, ZPX, 4 ),new INSTRUCTION( "LDX", LDX, ZPY, 4 ),new INSTRUCTION( "???", XXX, IMP, 4 ),new INSTRUCTION( "CLV", CLV, IMP, 2 ),new INSTRUCTION( "LDA", LDA, ABY, 4 ),new INSTRUCTION( "TSX", TSX, IMP, 2 ),new INSTRUCTION( "???", XXX, IMP, 4 ),new INSTRUCTION( "LDY", LDY, ABX, 4 ),new INSTRUCTION( "LDA", LDA, ABX, 4 ),new INSTRUCTION( "LDX", LDX, ABY, 4 ),new INSTRUCTION( "???", XXX, IMP, 4 ),
                            new INSTRUCTION( "CPY", CPY, IMM, 2 ),new INSTRUCTION( "CMP", CMP, IZX, 6 ),new INSTRUCTION( "???", NOP, IMP, 2 ),new INSTRUCTION( "???", XXX, IMP, 8 ),new INSTRUCTION( "CPY", CPY, ZP0, 3 ),new INSTRUCTION( "CMP", CMP, ZP0, 3 ),new INSTRUCTION( "DEC", DEC, ZP0, 5 ),new INSTRUCTION( "???", XXX, IMP, 5 ),new INSTRUCTION( "INY", INY, IMP, 2 ),new INSTRUCTION( "CMP", CMP, IMM, 2 ),new INSTRUCTION( "DEX", DEX, IMP, 2 ),new INSTRUCTION( "???", XXX, IMP, 2 ),new INSTRUCTION( "CPY", CPY, ABS, 4 ),new INSTRUCTION( "CMP", CMP, ABS, 4 ),new INSTRUCTION( "DEC", DEC, ABS, 6 ),new INSTRUCTION( "???", XXX, IMP, 6 ),
                            new INSTRUCTION( "BNE", BNE, REL, 2 ),new INSTRUCTION( "CMP", CMP, IZY, 5 ),new INSTRUCTION( "???", XXX, IMP, 2 ),new INSTRUCTION( "???", XXX, IMP, 8 ),new INSTRUCTION( "???", NOP, IMP, 4 ),new INSTRUCTION( "CMP", CMP, ZPX, 4 ),new INSTRUCTION( "DEC", DEC, ZPX, 6 ),new INSTRUCTION( "???", XXX, IMP, 6 ),new INSTRUCTION( "CLD", CLD, IMP, 2 ),new INSTRUCTION( "CMP", CMP, ABY, 4 ),new INSTRUCTION( "NOP", NOP, IMP, 2 ),new INSTRUCTION( "???", XXX, IMP, 7 ),new INSTRUCTION( "???", NOP, IMP, 4 ),new INSTRUCTION( "CMP", CMP, ABX, 4 ),new INSTRUCTION( "DEC", DEC, ABX, 7 ),new INSTRUCTION( "???", XXX, IMP, 7 ),
                            new INSTRUCTION( "CPX", CPX, IMM, 2 ),new INSTRUCTION( "SBC", SBC, IZX, 6 ),new INSTRUCTION( "???", NOP, IMP, 2 ),new INSTRUCTION( "???", XXX, IMP, 8 ),new INSTRUCTION( "CPX", CPX, ZP0, 3 ),new INSTRUCTION( "SBC", SBC, ZP0, 3 ),new INSTRUCTION( "INC", INC, ZP0, 5 ),new INSTRUCTION( "???", XXX, IMP, 5 ),new INSTRUCTION( "INX", INX, IMP, 2 ),new INSTRUCTION( "SBC", SBC, IMM, 2 ),new INSTRUCTION( "NOP", NOP, IMP, 2 ),new INSTRUCTION( "???", SBC, IMP, 2 ),new INSTRUCTION( "CPX", CPX, ABS, 4 ),new INSTRUCTION( "SBC", SBC, ABS, 4 ),new INSTRUCTION( "INC", INC, ABS, 6 ),new INSTRUCTION( "???", XXX, IMP, 6 ),
                            new INSTRUCTION( "BEQ", BEQ, REL, 2 ),new INSTRUCTION( "SBC", SBC, IZY, 5 ),new INSTRUCTION( "???", XXX, IMP, 2 ),new INSTRUCTION( "???", XXX, IMP, 8 ),new INSTRUCTION( "???", NOP, IMP, 4 ),new INSTRUCTION( "SBC", SBC, ZPX, 4 ),new INSTRUCTION( "INC", INC, ZPX, 6 ),new INSTRUCTION( "???", XXX, IMP, 6 ),new INSTRUCTION( "SED", SED, IMP, 2 ),new INSTRUCTION( "SBC", SBC, ABY, 4 ),new INSTRUCTION( "NOP", NOP, IMP, 2 ),new INSTRUCTION( "???", XXX, IMP, 7 ),new INSTRUCTION( "???", NOP, IMP, 4 ),new INSTRUCTION( "SBC", SBC, ABX, 4 ),new INSTRUCTION( "INC", INC, ABX, 7 ),new INSTRUCTION( "???", XXX, IMP, 7 ),
            };

            Lookup = tmpLook;
            Bus = bus;
        }

        // Legge 8 byte dal buffer dall'indirizzo a
        public byte Read(ushort a)
        {
            return Bus.CpuRead(a, false);
        }

        // Scrive 8 byte(d) sul buffer all'indirizzo a
        public void Write(ushort a, byte d)
        {
            Bus.CpuWrite(a, d);
        }



        //Disassembler per la CPU
        public Dictionary<ushort, string> Disassemble(ushort nStart, ushort nStop, out LinkedList<ushort> adrsMap)
        {
            uint addr = nStart;
            byte value = 0x00, lo = 0x00, hi = 0x00;
            Dictionary<ushort, string> mapLines = new Dictionary<ushort, string>();
            ushort line_addr = 0;

            adrsMap = new LinkedList<ushort>();

            while (addr <= nStop)
            {
                line_addr = (ushort)addr;

                string sInst = "$" + HexConvert(addr, 4) + ": ";

                byte opcode = Bus.CpuRead((ushort)addr, true); addr++;
                sInst += Lookup[opcode].Name + " ";

                if (Lookup[opcode].Addrmode == IMP)
                {
                    sInst += " {IMP}";
                }
                else if (Lookup[opcode].Addrmode == IMM)
                {
                    value = Bus.CpuRead((ushort)addr, true); addr++;
                    sInst += "#$" + HexConvert(value, 2) + " {IMM}";
                }
                else if (Lookup[opcode].Addrmode == ZP0)
                {
                    lo = Bus.CpuRead((ushort)addr, true); addr++;
                    hi = 0x00;
                    sInst += "$" + HexConvert(lo, 2) + " {ZP0}";
                }
                else if (Lookup[opcode].Addrmode == ZPX)
                {
                    lo = Bus.CpuRead((ushort)addr, true); addr++;
                    hi = 0x00;
                    sInst += "$" + HexConvert(lo, 2) + ", X {ZPX}";
                }
                else if (Lookup[opcode].Addrmode == ZPY)
                {
                    lo = Bus.CpuRead((ushort)addr, true); addr++;
                    hi = 0x00;
                    sInst += "$" + HexConvert(lo, 2) + ", Y {ZPY}";
                }
                else if (Lookup[opcode].Addrmode == IZX)
                {
                    lo = Bus.CpuRead((ushort)addr, true); addr++;
                    hi = 0x00;
                    sInst += "($" + HexConvert(lo, 2) + ", X) {IZX}";
                }
                else if (Lookup[opcode].Addrmode == IZY)
                {
                    lo = Bus.CpuRead((ushort)addr, true); addr++;
                    hi = 0x00;
                    sInst += "($" + HexConvert(lo, 2) + "), Y {IZY}";
                }
                else if (Lookup[opcode].Addrmode == ABS)
                {
                    lo = Bus.CpuRead((ushort)addr, true); addr++;
                    hi = Bus.CpuRead((ushort)addr, true); addr++;
                    sInst += "$" + HexConvert((uint)((ushort)(hi << 8) | lo), 4) + " {ABS}";
                }
                else if (Lookup[opcode].Addrmode == ABX)
                {
                    lo = Bus.CpuRead((ushort)addr, true); addr++;
                    hi = Bus.CpuRead((ushort)addr, true); addr++;
                    sInst += "$" + HexConvert((uint)((ushort)(hi << 8) | lo), 4) + ", X {ABX}";
                }
                else if (Lookup[opcode].Addrmode == ABY)
                {
                    lo = Bus.CpuRead((ushort)addr, true); addr++;
                    hi = Bus.CpuRead((ushort)addr, true); addr++;
                    sInst += "$" + HexConvert((uint)((ushort)(hi << 8) | lo), 4) + ", Y {ABY}";
                }
                else if (Lookup[opcode].Addrmode == IND)
                {
                    lo = Bus.CpuRead((ushort)addr, true); addr++;
                    hi = Bus.CpuRead((ushort)addr, true); addr++;
                    sInst += "($" + HexConvert((uint)((ushort)(hi << 8) | lo), 4) + ") {IND}";
                }
                else if (Lookup[opcode].Addrmode == REL)
                {
                    value = Bus.CpuRead((ushort)addr, true); addr++;
                    sInst += "$" + HexConvert(value, 2) + " [$" + HexConvert((uint)(addr +((sbyte)value)), 4) + "] {REL}";
                }

                mapLines[line_addr] = sInst;
                adrsMap.AddLast(line_addr);
            }

            return mapLines;

        }

        private string HexConvert(uint n, byte d)
        {
            char[] s = new char[d];
            for (int i = d - 1; i >= 0; i--, n >>= 4)
                s[i] = "0123456789ABCDEF"[(int)(n & 0xF)];


            return new string(s);
        }


        #region ADDRESS OP FUNCTION

        //Reset Cpu
        public void Reset()
        {
            Addr_abs = 0xFFFC;
            ushort lo = Read((ushort)(Addr_abs + 0));
            ushort hi = Read((ushort)(Addr_abs + 1));

            Pc = (ushort)((hi << 8) | lo);

            A = 0;
            X = 0;
            Y = 0;
            Stkp = 0xFD;
            Status = (byte)(0x00 | FLAGS6502.U);

            Addr_rel = 0;
            Addr_abs = 0;
            Fetched = 0;

            Cycles = 8;
        }


        // FLAG FUNCTIONS

        public byte GetFlag(FLAGS6502 f)
        {
            return (byte)(((Status & (byte)f) > 0) ? 1 : 0);
        }

        public void SetFlag(FLAGS6502 f, bool v)
        {
            if (v)
                Status |= (byte)f;
            else
                Status &= (byte)~f;
        }


        
        public void Irq()
        {
            if (GetFlag(FLAGS6502.I) == 0)
            {
                Write((ushort)(0x0100 + Stkp), (byte)((Pc >> 8) & 0x00FF));
                Stkp--;
                Write((ushort)(0x0100 + Stkp), (byte)(Pc & 0x00FF));
                Stkp--;

                SetFlag(FLAGS6502.B, false);
                SetFlag(FLAGS6502.U, true);
                SetFlag(FLAGS6502.I, false);
                Write((ushort)(0x0100 + Stkp), Status);
                Stkp--;

                Addr_abs = 0xFFFE;
                ushort lo = Read((ushort)(Addr_abs + 0));
                ushort hi = Read((ushort)(Addr_abs + 1));
                Pc = (ushort)((hi << 8) | lo);

                Cycles = 7;
            }
        }





        public void Nmi()
        {
            Write((ushort)(0x0100 + Stkp), (byte)((Pc >> 8) & 0x00FF));
            Stkp--;
            Write((ushort)(0x0100 + Stkp), (byte)(Pc & 0x00FF));
            Stkp--;

            SetFlag(FLAGS6502.B, false);
            SetFlag(FLAGS6502.U, true);
            SetFlag(FLAGS6502.I, true);
            Write((ushort)(0x0100 + Stkp), Status);
            Stkp--;

            Addr_abs = 0xFFFA;
            ushort lo = Read((ushort)(Addr_abs + 0));
            ushort hi = Read((ushort)(Addr_abs + 1));
            Pc = (ushort)((hi << 8) | lo);

            Cycles = 8;
        }

        //Funzione di clock della CPU
        public void Clock()
        {

            if (Cycles == 0)
            {
                Opcode = Read(Pc);
                SetFlag(FLAGS6502.U, true);

                Pc++;

                //Debug test
                //File.AppendAllText("C:\\temp\\nnemu.txt", Opcode + " | " + Pc + " | " + A + " | " + X + " | " + Y + " | " + CpuClockCount + "\n");

                Cycles = Lookup[Opcode].Cycles;

                byte additional_cycle1 = (Lookup[Opcode].Addrmode)();

                byte additional_cycle2 = (Lookup[Opcode].Operate)();

                Cycles += (byte)(additional_cycle1 & additional_cycle2);

                SetFlag(FLAGS6502.U, true);
            }

            Cycles--;
            CpuClockCount++;
        }

        //I nomi delle funzioni della CPU sono standard, guardare il manuale per capire cosa fanno.

        public byte IMP()
        {
            Fetched = A;
            return 0;
        }

        public byte IMM()
        {
            Addr_abs = Pc++;
            return 0;
        }

        public byte ZP0()
        {
            Addr_abs = Read(Pc);
            Pc++;
            Addr_abs &= 0x00FF;
            return 0;
        }

        public byte ZPX()
        {
            Addr_abs = ((ushort)(Read(Pc) + X));
            Pc++;
            Addr_abs &= 0x00FF;
            return 0;
        }

        public byte ZPY()
        {
            Addr_abs = ((ushort)(Read(Pc) + Y));
            Pc++;
            Addr_abs &= 0x00FF;
            return 0;
        }

        byte REL()
        {
            Addr_rel = Read(Pc);
            Pc++;
            if ((Addr_rel & 0x80) != 0)
                Addr_rel |= 0xFF00;
            return 0;
        }


        public byte ABS()
        {
            ushort lo = Read(Pc);
            Pc++;
            ushort hi = Read(Pc);
            Pc++;

            Addr_abs = (ushort)((hi << 8) | lo);

            return 0;
        }

        public byte ABX()
        {
            ushort lo = Read(Pc);
            Pc++;
            ushort hi = Read(Pc);
            Pc++;

            Addr_abs = (ushort)((hi << 8) | lo);
            Addr_abs += X;

            if ((Addr_abs & 0xFF00) != (hi << 8))
                return 1;
            else
                return 0;
        }


        public byte ABY()
        {
            ushort lo = Read(Pc);
            Pc++;
            ushort hi = Read(Pc);
            Pc++;

            Addr_abs = (ushort)((hi << 8) | lo);
            Addr_abs += Y;

            if ((Addr_abs & 0xFF00) != (hi << 8))
                return 1;
            else
                return 0;
        }

        public byte IND()
        {
            ushort ptr_lo = Read(Pc);
            Pc++;
            ushort ptr_hi = Read(Pc);
            Pc++;

            ushort ptr = (ushort)((ptr_hi << 8) | ptr_lo);

            if (ptr_lo == 0x00FF) //Simulazione del bug della CPU 6502 xD
            {
                Addr_abs = (ushort)((Read((ushort)(ptr & 0xFF00)) << 8) | Read((ushort)(ptr + 0)));
            }
            else
            {
                Addr_abs = (ushort)((Read((ushort)(ptr + 1)) << 8) | Read((ushort)(ptr + 0)));
            }

            return 0;
        }

        public byte IZX()
        {
            ushort t = Read(Pc);
            Pc++;

            ushort lo = Read((ushort)((ushort)(t + X) & 0x00FF));
            ushort hi = Read((ushort)((ushort)(t + X + 1) & 0x00FF));

            Addr_abs = (ushort)((hi << 8) | lo);

            return 0;
        }

        public byte IZY()
        {
            ushort t = Read(Pc);
            Pc++;

            ushort lo = Read((ushort)(t & 0x00FF));
            ushort hi = Read((ushort)((t + 1) & 0x00FF));

            Addr_abs = (ushort)((hi << 8) | lo);
            Addr_abs += Y;

            if ((Addr_abs & 0xFF00) != (hi << 8))
                return 1;
            else
                return 0;
        }


        public byte Fetch()
        {
            if (!(Lookup[Opcode].Addrmode == IMP))
                Fetched = Read(Addr_abs);

            return Fetched;
        }


        #endregion

        #region OP IMPLEMENTS

        public byte ADC()
        {
            Fetch();

            Temp = (ushort)(A + Fetched + GetFlag(FLAGS6502.C));

            SetFlag(FLAGS6502.C, Temp > 255);

            SetFlag(FLAGS6502.Z, (Temp & 0x00FF) == 0);

            SetFlag(FLAGS6502.V, (~(A ^ Fetched) & (A ^ Temp) & 0x0080) != 0);

            SetFlag(FLAGS6502.N, (Temp & 0x80) != 0);

            A = (byte)(Temp & 0x00FF);

            return 1;
        }


        public byte SBC()
        {
            Fetch();

            ushort value = (ushort)((Fetched) ^ 0x00FF);

            Temp = (ushort)(A + value + GetFlag(FLAGS6502.C));
            SetFlag(FLAGS6502.C, (Temp & 0xFF00) != 0);
            SetFlag(FLAGS6502.Z, ((Temp & 0x00FF) == 0));
            SetFlag(FLAGS6502.V, ((Temp ^ A) & (Temp ^ value) & 0x0080) != 0);
            SetFlag(FLAGS6502.N, (Temp & 0x0080) != 0);
            A = (byte)(Temp & 0x00FF);
            return 1;
        }

        public byte AND()
        {
            Fetch();
            A = (byte)(A & Fetched);
            SetFlag(FLAGS6502.Z, A == 0x00);
            SetFlag(FLAGS6502.N, (A & 0x80) != 0);
            return 1;
        }

        public byte ASL()
        {
            Fetch();
            Temp = (ushort)(Fetched << 1);
            SetFlag(FLAGS6502.C, (Temp & 0xFF00) > 0);
            SetFlag(FLAGS6502.Z, (Temp & 0x00FF) == 0x00);
            SetFlag(FLAGS6502.N, (Temp & 0x80) != 0);
            if (Lookup[Opcode].Addrmode == IMP)
                A = (byte)(Temp & 0x00FF);
            else
                Write(Addr_abs, (byte)(Temp & 0x00FF));
            return 0;
        }

        public byte BCC()
        {
            if (GetFlag(FLAGS6502.C) == 0)
            {
                Cycles++;
                Addr_abs = (ushort)(Pc + Addr_rel);

                if ((Addr_abs & 0xFF00) != (Pc & 0xFF00))
                    Cycles++;

                Pc = Addr_abs;
            }
            return 0;
        }

        public byte BCS()
        {
            if (GetFlag(FLAGS6502.C) == 1)
            {
                Cycles++;
                Addr_abs = (ushort)(Pc + Addr_rel);

                if ((Addr_abs & 0xFF00) != (Pc & 0xFF00))
                    Cycles++;

                Pc = Addr_abs;
            }
            return 0;
        }

        public byte BEQ()
        {
            if (GetFlag(FLAGS6502.Z) == 1)
            {
                Cycles++;
                Addr_abs = (ushort)(Pc + Addr_rel);

                if ((Addr_abs & 0xFF00) != (Pc & 0xFF00))
                    Cycles++;

                Pc = Addr_abs;
            }
            return 0;
        }

        public byte BIT()
        {
            Fetch();
            Temp = (ushort)(A & Fetched);
            SetFlag(FLAGS6502.Z, (Temp & 0x00FF) == 0x00);
            SetFlag(FLAGS6502.N, (Fetched & (1 << 7)) != 0);
            SetFlag(FLAGS6502.V, (Fetched & (1 << 6)) != 0);
            return 0;
        }

        public byte BMI()
        {
            if (GetFlag(FLAGS6502.N) == 1)
            {
                Cycles++;
                Addr_abs = (ushort)(Pc + Addr_rel);

                if ((Addr_abs & 0xFF00) != (Pc & 0xFF00))
                    Cycles++;

                Pc = Addr_abs;
            }
            return 0;
        }

        public byte BNE()
        {
            if (GetFlag(FLAGS6502.Z) == 0)
            {
                Cycles++;
                Addr_abs = (ushort)(Pc + Addr_rel);

                if ((Addr_abs & 0xFF00) != (Pc & 0xFF00))
                    Cycles++;

                Pc = Addr_abs;
            }
            return 0;
        }

        public byte BPL()
        {
            if (GetFlag(FLAGS6502.N) == 0)
            {
                Cycles++;
                Addr_abs = (ushort)(Pc + Addr_rel);

                if ((Addr_abs & 0xFF00) != (Pc & 0xFF00))
                    Cycles++;

                Pc = Addr_abs;
            }
            return 0;
        }

        public byte BRK()
        {
            Pc++;

            SetFlag(FLAGS6502.I, true);
            Write((ushort)(0x0100 + Stkp), (byte)((Pc >> 8) & 0x00FF));
            Stkp--;
            Write((ushort)(0x0100 + Stkp), (byte)(Pc & 0x00FF));
            Stkp--;

            SetFlag(FLAGS6502.B, true);
            Write((ushort)(0x0100 + Stkp), Status);
            Stkp--;
            SetFlag(FLAGS6502.B, false);

            Pc = (ushort)(Read(0xFFFE) | (Read(0xFFFF) << 8));
            return 0;
        }

        public byte BVC()
        {
            if (GetFlag(FLAGS6502.V) == 0)
            {
                Cycles++;
                Addr_abs = (ushort)(Pc + Addr_rel);

                if ((Addr_abs & 0xFF00) != (Pc & 0xFF00))
                    Cycles++;

                Pc = Addr_abs;
            }
            return 0;
        }

        public byte BVS()
        {
            if (GetFlag(FLAGS6502.V) == 1)
            {
                Cycles++;
                Addr_abs = (ushort)(Pc + Addr_rel);

                if ((Addr_abs & 0xFF00) != (Pc & 0xFF00))
                    Cycles++;

                Pc = Addr_abs;
            }
            return 0;
        }

        public byte CLC()
        {
            SetFlag(FLAGS6502.C, false);
            return 0;
        }

        public byte CLD()
        {
            SetFlag(FLAGS6502.D, false);
            return 0;
        }

        public byte CLI()
        {
            SetFlag(FLAGS6502.I, false);
            return 0;
        }

        public byte CLV()
        {
            SetFlag(FLAGS6502.V, false);
            return 0;
        }

        public byte CMP()
        {
            Fetch();
            Temp = (ushort)(A - Fetched);
            SetFlag(FLAGS6502.C, A >= Fetched);
            SetFlag(FLAGS6502.Z, (Temp & 0x00FF) == 0x0000);
            SetFlag(FLAGS6502.N, (Temp & 0x0080) != 0);
            return 1;
        }

        public byte CPX()
        {
            Fetch();
            Temp = (ushort)(X - Fetched);
            SetFlag(FLAGS6502.C, X >= Fetched);
            SetFlag(FLAGS6502.Z, (Temp & 0x00FF) == 0x0000);
            SetFlag(FLAGS6502.N, (Temp & 0x0080) != 0);
            return 0;
        }

        public byte CPY()
        {
            Fetch();
            Temp = (ushort)(Y - Fetched);
            SetFlag(FLAGS6502.C, Y >= Fetched);
            SetFlag(FLAGS6502.Z, (Temp & 0x00FF) == 0x0000);
            SetFlag(FLAGS6502.N, (Temp & 0x0080) != 0);
            return 0;
        }

        public byte DEC()
        {
            Fetch();
            Temp = (ushort)(Fetched - 1);
            Write(Addr_abs, (byte)(Temp & 0x00FF));
            SetFlag(FLAGS6502.Z, (Temp & 0x00FF) == 0x0000);
            SetFlag(FLAGS6502.N, (Temp & 0x0080) != 0);
            return 0;
        }

        public byte DEX()
        {
            X--;
            SetFlag(FLAGS6502.Z, X == 0x00);
            SetFlag(FLAGS6502.N, (X & 0x80) != 0);
            return 0;
        }

        public byte DEY()
        {
            Y--;
            SetFlag(FLAGS6502.Z, Y == 0x00);
            SetFlag(FLAGS6502.N, (Y & 0x80) != 0);
            return 0;
        }

        public byte EOR()
        {
            Fetch();
            A = (byte)(A ^ Fetched);
            SetFlag(FLAGS6502.Z, A == 0x00);
            SetFlag(FLAGS6502.N, (A & 0x80) != 0);
            return 1;
        }

        public byte INC()
        {
            Fetch();
            Temp = (ushort)(Fetched + 1);
            Write(Addr_abs, (byte)(Temp & 0x00FF));
            SetFlag(FLAGS6502.Z, (Temp & 0x00FF) == 0x0000);
            SetFlag(FLAGS6502.N, (Temp & 0x0080) != 0);
            return 0;
        }

        public byte INX()
        {
            X++;
            SetFlag(FLAGS6502.Z, X == 0x00);
            SetFlag(FLAGS6502.N, (X & 0x80) != 0);
            return 0;
        }

        public byte INY()
        {
            Y++;
            SetFlag(FLAGS6502.Z, Y == 0x00);
            SetFlag(FLAGS6502.N, (Y & 0x80) != 0);
            return 0;
        }

        public byte JMP()
        {
            Pc = Addr_abs;
            return 0;
        }

        public byte JSR()
        {
            Pc--;

            Write((ushort)(0x0100 + Stkp), (byte)((Pc >> 8) & 0x00FF));
            Stkp--;
            Write((ushort)(0x0100 + Stkp), (byte)(Pc & 0x00FF));
            Stkp--;

            Pc = Addr_abs;
            return 0;
        }

        public byte LDA()
        {
            Fetch();
            A = Fetched;
            SetFlag(FLAGS6502.Z, A == 0x00);
            SetFlag(FLAGS6502.N, (A & 0x80) != 0);
            return 1;
        }

        public byte LDX()
        {
            Fetch();
            X = Fetched;
            SetFlag(FLAGS6502.Z, X == 0x00);
            SetFlag(FLAGS6502.N, (X & 0x80) != 0);
            return 1;
        }

        public byte LDY()
        {
            Fetch();
            Y = Fetched;
            SetFlag(FLAGS6502.Z, Y == 0x00);
            SetFlag(FLAGS6502.N, (Y & 0x80) != 0);
            return 1;
        }

        public byte LSR()
        {
            Fetch();
            SetFlag(FLAGS6502.C, (Fetched & 0x0001) != 0);
            Temp = (ushort)(Fetched >> 1);
            SetFlag(FLAGS6502.Z, (Temp & 0x00FF) == 0x0000);
            SetFlag(FLAGS6502.N, (Temp & 0x0080) != 0);
            if (Lookup[Opcode].Addrmode == IMP)
                A = (byte)(Temp & 0x00FF);
            else
                Write(Addr_abs, (byte)(Temp & 0x00FF));
            return 0;
        }

        public byte NOP()
        {
            switch (Opcode)
            {
                case 0x1C:
                case 0x3C:
                case 0x5C:
                case 0x7C:
                case 0xDC:
                case 0xFC:
                    return 1;
            }
            return 0;
        }

        public byte ORA()
        {
            Fetch();
            A = (byte)(A | Fetched);
            SetFlag(FLAGS6502.Z, A == 0x00);
            SetFlag(FLAGS6502.N, (A & 0x80) != 0);
            return 1;
        }

        public byte PHA()
        {
            Write((ushort)(0x0100 + Stkp), A);
            Stkp--;
            return 0;
        }

        public byte PHP()
        {
            Write((ushort)(0x0100 + Stkp), (byte)(Status | (byte)FLAGS6502.B | (byte)FLAGS6502.U));
            SetFlag(FLAGS6502.B, false);
            SetFlag(FLAGS6502.U, false);
            Stkp--;
            return 0;
        }

        public byte PLA()
        {
            Stkp++;
            A = Read((ushort)(0x0100 + Stkp));
            SetFlag(FLAGS6502.Z, A == 0x00);
            SetFlag(FLAGS6502.N, (A & 0x80) != 0);
            return 0;
        }

        public byte PLP()
        {
            Stkp++;
            Status = Read((ushort)(0x0100 + Stkp));
            SetFlag(FLAGS6502.U, true);
            return 0;
        }

        public byte ROL()
        {
            Fetch();
            Temp = (ushort)((ushort)(Fetched << 1) | GetFlag(FLAGS6502.C));
            SetFlag(FLAGS6502.C, (Temp & 0xFF00) != 0);
            SetFlag(FLAGS6502.Z, (Temp & 0x00FF) == 0x0000);
            SetFlag(FLAGS6502.N, (Temp & 0x0080) != 0);
            if (Lookup[Opcode].Addrmode == IMP)
                A = (byte)(Temp & 0x00FF);
            else
                Write(Addr_abs, (byte)(Temp & 0x00FF));
            return 0;
        }

        public byte ROR()
        {
            Fetch();
            Temp = (ushort)((ushort)(GetFlag(FLAGS6502.C) << 7) | (Fetched >> 1));
            SetFlag(FLAGS6502.C, (Fetched & 0x01) != 0);
            SetFlag(FLAGS6502.Z, (Temp & 0x00FF) == 0x00);
            SetFlag(FLAGS6502.N, (Temp & 0x0080) != 0);
            if (Lookup[Opcode].Addrmode == IMP)
                A = (byte)(Temp & 0x00FF);
            else
                Write(Addr_abs, (byte)(Temp & 0x00FF));
            return 0;
        }

        public byte RTI()
        {
            Stkp++;
            Status = Read((ushort)(0x0100 + Stkp));
            
            unchecked
            {
                Status &= (byte)~FLAGS6502.B;
                Status &= (byte)~FLAGS6502.U;
            }
            
            Stkp++;
            ushort pop_1 = Read((ushort)(0x0100 + Stkp));
            Stkp++;
            ushort pop_2 = (ushort)(Read((ushort)(0x0100 + Stkp)) << 8);

            Pc = (ushort)(pop_1 | pop_2);

            return 0;
        }

        public byte RTS()
        {
            Stkp++;
            ushort pop_1 = Read((ushort)(0x0100 + Stkp));
            Stkp++;
            ushort pop_2 = (ushort)(Read((ushort)(0x0100 + Stkp)) << 8);

            Pc = (ushort)(pop_1 | pop_2);

            Pc++;
            return 0;
        }


        public byte SEC()
        {
            SetFlag(FLAGS6502.C, true);
            return 0;
        }

        public byte SED()
        {
            SetFlag(FLAGS6502.D, true);
            return 0;
        }

        public byte SEI()
        {
            SetFlag(FLAGS6502.I, true);
            return 0;
        }

        public byte STA()
        {
            Write(Addr_abs, A);
            return 0;
        }

        public byte STX()
        {
            Write(Addr_abs, X);
            return 0;
        }

        public byte STY()
        {
            Write(Addr_abs, Y);
            return 0;
        }

        public byte TAX()
        {
            X = A;
            SetFlag(FLAGS6502.Z, X == 0x00);
            SetFlag(FLAGS6502.N, (X & 0x80) != 0);
            return 0;
        }

        public byte TAY()
        {
            Y = A;
            SetFlag(FLAGS6502.Z, Y == 0x00);
            SetFlag(FLAGS6502.N, (Y & 0x80) != 0);
            return 0;
        }

        public byte TSX()
        {
            X = Stkp;
            SetFlag(FLAGS6502.Z, X == 0x00);
            SetFlag(FLAGS6502.N, (X & 0x80) != 0);
            return 0;
        }

        public byte TXA()
        {
            A = X;
            SetFlag(FLAGS6502.Z, A == 0x00);
            SetFlag(FLAGS6502.N, (A & 0x80) != 0);
            return 0;
        }

        public byte TXS()
        {
            Stkp = X;
            return 0;
        }

        public byte TYA()
        {
            A = Y;
            SetFlag(FLAGS6502.Z, A == 0x00);
            SetFlag(FLAGS6502.N, (A & 0x80) != 0);
            return 0;
        }
        //OPCODES ILLEGALI
        public byte XXX()
        {
            return 0;
        }

        public bool Complete()
        {
            return Cycles == 0;
        }

        #endregion

    }

}
