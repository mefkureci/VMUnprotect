using VMUnprotect.Devirtualizer.Engine;
using VMUnprotect.Devirtualizer.Model;

namespace VMUnprotect.Devirtualizer;

/// <summary>
/// Eğitim amaçlı devirtualization örneği
/// Gerçek bir VMProtect analizi için bu kodun önemli ölçüde genişletilmesi gerekir
/// </summary>
public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("=== VMUnprotect Devirtualizer (Eğitim Amaçlı) ===\n");

        // Örnek bytecode (gerçek VMProtect bytecode'u değil, eğitim amaçlı sahte veri)
        // Format: [0x01][op1][op2] = ADD op1, op2
        //         [0xE9][8-byte-address] = JMP address
        byte[] sampleBytecode = new byte[]
        {
            0x01, 0x00, 0x01,        // ADD R0, R1
            0x01, 0x02, 0x03,        // ADD R2, R3
            0xE9, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,  // JMP 0x10
            0x01, 0x04, 0x05,        // ADD R4, R5 (bu komut unreachable olacak)
            0x01, 0x06, 0x07,        // ADD R6, R7 (bu komut unreachable olacak)
            0x00, 0x10, 0x00,        // Bilinmeyen opcode
            0x01, 0x08, 0x09         // ADD R8, R9 (address 0x10'da)
        };

        var engine = new DevirtualizationEngine();

        Console.WriteLine("Bytecode analizi başlatılıyor...\n");
        
        var instructions = engine.Analyze(sampleBytecode, 0).ToList();

        Console.WriteLine($"Toplam {instructions.Count} komut çözümlendi:\n");
        
        foreach (var instr in instructions)
        {
            Console.WriteLine($"[0x{instr.Address:X4}] {instr.Mnemonic} - Tip: {instr.Type}");
            
            if (instr is ArithmeticVmInstruction arith)
            {
                Console.WriteLine($"        İşlem: {arith.Operation} R{arith.Operand1}, R{arith.Operand2}");
            }
            else if (instr is ControlFlowVmInstruction jmp)
            {
                Console.WriteLine($"        Hedef: 0x{jmp.TargetAddress:X4} {(string.IsNullOrEmpty(jmp.Condition) ? "(Koşulsuz)" : $"({jmp.Condition})")}");
            }
        }

        Console.WriteLine("\n=== Control Flow Graph (CFG) ===\n");
        
        var cfg = engine.BuildCfg();
        foreach (var node in cfg)
        {
            var targets = string.Join(", ", node.Value.Select(addr => $"0x{addr:X4}"));
            Console.WriteLine($"0x{node.Key:X4} -> [{targets}]");
        }

        Console.WriteLine("\n=== Dead Code Elimination ===\n");
        
        var optimized = engine.EliminateDeadCode(0);
        Console.WriteLine($"Ulaşılabilir komut sayısı: {optimized.Count} / {instructions.Count}");
        Console.WriteLine("Optimize edilmiş komutlar:");
        
        foreach (var instr in optimized)
        {
            Console.WriteLine($"  [0x{instr.Address:X4}] {instr.Mnemonic}");
        }

        Console.WriteLine("\n=== Eğitim Notları ===");
        Console.WriteLine(@"
Bu örnek, devirtualization işleminin temel mantığını göstermektedir:

1. HANDLER TABANLI DECODE: Her VM komut tipi için özel handler sınıfları
2. CFG OLUŞTURMA: Komutlar arası akış grafiği çıkarma
3. DEAD CODE ELIMINATION: Ulaşılamayan kod bloklarını temizleme

GERÇEK VMPROTECT İÇİN GEREKENLER:
- VM context yapısının reverse engineering ile bulunması
- Her VM handler'ının dinamik olarak keşfedilmesi
- Stack-based VM mimarisinin emülasyonu
- Sembolik yürütme (Z3 solver) ile karmaşık ifadelerin sadeleştirilmesi
- Native assembly'e dönüştürme (IR -> x86/x64)

UYARI: Bu kod sadece eğitim amaçlıdır. Gerçek VMProtect korumalarını 
kırmak için yetersizdir ve yasal olmayan kullanım sorumluluğu kullanıcıya aittir.
");
    }
}
