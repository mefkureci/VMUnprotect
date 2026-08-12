namespace VMUnprotect.Devirtualizer.Model;

/// <summary>
/// Sanal makine komutlarını temsil eden soyut sınıf
/// </summary>
public abstract class VmInstruction
{
    public ulong Address { get; set; }
    public byte[] OpCode { get; set; } = Array.Empty<byte>();
    
    public abstract string Mnemonic { get; }
    public abstract VmInstructionType Type { get; }
}

public enum VmInstructionType
{
    Unknown,
    Arithmetic,    // ADD, SUB, MUL, DIV
    Logical,       // AND, OR, XOR, NOT
    Memory,        // MOV, PUSH, POP
    ControlFlow,   // JMP, JE, JNE, CALL, RET
    Stack          // PUSHF, POPF
}

/// <summary>
/// Aritmetik işlemler için özel komut
/// </summary>
public class ArithmeticVmInstruction : VmInstruction
{
    public string Operation { get; set; } = string.Empty; // ADD, SUB, etc.
    public int Operand1 { get; set; }
    public int Operand2 { get; set; }
    
    public override string Mnemonic => Operation;
    public override VmInstructionType Type => VmInstructionType.Arithmetic;
}

/// <summary>
/// Kontrol akışı komutları için özel sınıf
/// </summary>
public class ControlFlowVmInstruction : VmInstruction
{
    public ulong TargetAddress { get; set; }
    public string Condition { get; set; } = string.Empty; // EQ, NE, GT, etc. (empty for unconditional)
    
    public override string Mnemonic => Condition switch
    {
        "" => "JMP",
        "EQ" => "JE",
        "NE" => "JNE",
        "GT" => "JG",
        "LT" => "JL",
        _ => "J?"
    };
    
    public override VmInstructionType Type => VmInstructionType.ControlFlow;
}
