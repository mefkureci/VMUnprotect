using VMUnprotect.Devirtualizer.Model;

namespace VMUnprotect.Devirtualizer.Engine;

/// <summary>
/// Sanal bytecode'u analiz edip native kodu çıkarmaya çalışan motor
/// Eğitim amaçlı basitleştirilmiş implementasyon
/// </summary>
public class DevirtualizationEngine
{
    private readonly List<IVmHandler> _handlers;
    private readonly Dictionary<ulong, VmInstruction> _decodedInstructions = new();

    public DevirtualizationEngine()
    {
        // Handler'ları kaydet - gerçek senaryoda bunlar dinamik olarak bulunur
        _handlers = new List<IVmHandler>
        {
            new AddHandler(),
            new JmpHandler(),
            // Diğer handler'lar buraya eklenebilir
        };
    }

    /// <summary>
    /// Bytecode'u analiz edip sanal komutlara dönüştürür
    /// </summary>
    public IEnumerable<VmInstruction> Analyze(byte[] bytecode, ulong startAddress = 0)
    {
        int offset = 0;
        
        while (offset < bytecode.Length)
        {
            var instruction = DecodeInstruction(bytecode, offset, startAddress + (ulong)offset);
            
            if (instruction == null)
            {
                // Bilinmeyen opcode - atla veya hata ver
                offset++;
                continue;
            }

            _decodedInstructions[instruction.Address] = instruction;
            yield return instruction;

            offset += instruction.OpCode.Length;
        }
    }

    private VmInstruction? DecodeInstruction(byte[] bytecode, int offset, ulong address)
    {
        var remainingBytes = bytecode.Skip(offset).ToArray();

        foreach (var handler in _handlers)
        {
            if (handler.CanHandle(remainingBytes))
            {
                return handler.Decode(remainingBytes, address);
            }
        }

        return null;
    }

    /// <summary>
    /// Control Flow Graph (CFG) oluşturur - eğitim için görselleştirme
    /// </summary>
    public Dictionary<ulong, List<ulong>> BuildCfg()
    {
        var cfg = new Dictionary<ulong, List<ulong>>();

        foreach (var instruction in _decodedInstructions.Values)
        {
            if (!cfg.ContainsKey(instruction.Address))
                cfg[instruction.Address] = new List<ulong>();

            if (instruction is ControlFlowVmInstruction jmp)
            {
                cfg[instruction.Address].Add(jmp.TargetAddress);
                
                // Koşullu jump ise bir sonraki komuta da git
                if (!string.IsNullOrEmpty(jmp.Condition))
                {
                    var nextAddr = instruction.Address + (ulong)instruction.OpCode.Length;
                    if (_decodedInstructions.ContainsKey(nextAddr))
                        cfg[instruction.Address].Add(nextAddr);
                }
            }
            else
            {
                var nextAddr = instruction.Address + (ulong)instruction.OpCode.Length;
                if (_decodedInstructions.ContainsKey(nextAddr))
                    cfg[instruction.Address].Add(nextAddr);
            }
        }

        return cfg;
    }

    /// <summary>
    /// Basit deobfuscation - dead code elimination (eğitim amaçlı)
    /// </summary>
    public List<VmInstruction> EliminateDeadCode(ulong entryPoint)
    {
        var reachable = new HashSet<ulong>();
        var queue = new Queue<ulong>();
        
        queue.Enqueue(entryPoint);
        reachable.Add(entryPoint);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var cfg = BuildCfg();

            if (cfg.TryGetValue(current, out var successors))
            {
                foreach (var successor in successors)
                {
                    if (!reachable.Contains(successor))
                    {
                        reachable.Add(successor);
                        queue.Enqueue(successor);
                    }
                }
            }
        }

        return _decodedInstructions
            .Where(kvp => reachable.Contains(kvp.Key))
            .Select(kvp => kvp.Value)
            .ToList();
    }
}
