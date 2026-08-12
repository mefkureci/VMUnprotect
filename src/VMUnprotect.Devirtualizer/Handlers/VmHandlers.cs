namespace VMUnprotect.Devirtualizer.Engine;

/// <summary>
/// Sanal makine komutlarını çözmek için handler arayüzü
/// Eğitim amaçlı: Her komut tipi için ayrı handler
/// </summary>
public interface IVmHandler
{
    bool CanHandle(byte[] opcode);
    VmInstruction? Decode(byte[] opcode, ulong address);
}

/// <summary>
/// Temel handler base sınıfı - ortak mantığı içerir
/// </summary>
public abstract class BaseVmHandler : IVmHandler
{
    protected abstract byte[] Signature { get; }
    protected abstract int MinLength { get; }

    public virtual bool CanHandle(byte[] opcode)
    {
        if (opcode.Length < MinLength)
            return false;

        for (int i = 0; i < Signature.Length; i++)
        {
            if (Signature[i] != 0xFF && opcode[i] != Signature[i])
                return false;
        }

        return true;
    }

    public abstract VmInstruction? Decode(byte[] opcode, ulong address);
}

/// <summary>
/// Basit ADD işlemi handler'ı (Eğitim amaçlı örnek)
/// Format: [OPCODE_ADD][operand1_index][operand2_index]
/// </summary>
public class AddHandler : BaseVmHandler
{
    protected override byte[] Signature => new byte[] { 0x01 }; // Örnek opcode
    protected override int MinLength => 3;

    public override VmInstruction? Decode(byte[] opcode, ulong address)
    {
        if (!CanHandle(opcode))
            return null;

        return new ArithmeticVmInstruction
        {
            Address = address,
            OpCode = opcode,
            Operation = "ADD",
            Operand1 = opcode[1],
            Operand2 = opcode[2]
        };
    }
}

/// <summary>
/// Basit JMP işlemi handler'ı (Eğitim amaçlı örnek)
/// Format: [OPCODE_JMP][target_address_8bytes]
/// </summary>
public class JmpHandler : BaseVmHandler
{
    protected override byte[] Signature => new byte[] { 0xE9 }; // Örnek opcode
    protected override int MinLength => 9;

    public override VmInstruction? Decode(byte[] opcode, ulong address)
    {
        if (!CanHandle(opcode))
            return null;

        var targetBytes = opcode.Skip(1).Take(8).ToArray();
        var targetAddress = BitConverter.ToUInt64(targetBytes, 0);

        return new ControlFlowVmInstruction
        {
            Address = address,
            OpCode = opcode,
            TargetAddress = targetAddress,
            Condition = ""
        };
    }
}
