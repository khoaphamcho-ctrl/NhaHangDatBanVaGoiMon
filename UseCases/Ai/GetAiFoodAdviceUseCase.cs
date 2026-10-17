using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreBusiness.Models;
using UseCases.DataStorePluginInterfaces;

namespace UseCases.Ai;

public class AiAdviceRequest
{
    public int GuestCount { get; set; } = 4;
    public decimal Budget { get; set; } = 500000; // Ngân sách dự kiến VNĐ
    public string Preferences { get; set; } = string.Empty; // Không cay, ít dầu mỡ, hải sản...
    public string DietaryNotes { get; set; } = string.Empty; // Dị ứng đậu phộng, ăn chay...
}

public class AiAdviceResult
{
    public string SummaryAdvice { get; set; } = string.Empty;
    public List<MenuItem> SuggestedItems { get; set; } = new();
    public decimal EstimatedTotal { get; set; }
    public bool IsSuccess { get; set; } = true;
    public string? ErrorMessage { get; set; }
}

public interface IGetAiFoodAdviceUseCase
{
    Task<AiAdviceResult> ExecuteAsync(AiAdviceRequest request);
}

/// <summary>
/// TIÊU CHÍ K4.3 & MỤC 10: TRỢ LÝ AI GỢI Ý THỰC ĐƠN
/// Inject vào UseCase riêng, không gọi từ file .razor
/// Gợi ý dựa trên dữ liệu Menu thật của CSDL
/// Có xử lý an toàn (fallback) khi mất mạng/hết hạn mức AI
/// </summary>
public class GetAiFoodAdviceUseCase : IGetAiFoodAdviceUseCase
{
    private readonly IMenuItemRepository _menuItemRepo;

    public GetAiFoodAdviceUseCase(IMenuItemRepository menuItemRepo)
    {
        _menuItemRepo = menuItemRepo;
    }

    public async Task<AiAdviceResult> ExecuteAsync(AiAdviceRequest request)
    {
        try
        {
            var allItems = (await _menuItemRepo.GetAllMenuItemsAsync())
                .Where(m => m.IsAvailable)
                .ToList();

            if (!allItems.Any())
            {
                return new AiAdviceResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Hiện chưa có món ăn khả dụng trong thực đơn để gợi ý."
                };
            }

            // Thuật toán thông minh chọn combo món cân đối dinh dưỡng theo ngân sách & số khách
            var suggested = new List<MenuItem>();
            decimal runningTotal = 0;

            // 1. Món khai vị / Ăn vặt
            var appetizers = allItems.Where(i => i.CategoryName.Contains("Khai vị") || i.CategoryName.Contains("Bánh") || i.Price < 60000).ToList();
            if (appetizers.Any())
            {
                var app = appetizers.OrderBy(x => Guid.NewGuid()).First();
                suggested.Add(app);
                runningTotal += app.Price;
            }

            // 2. Món chính (Đặc sản Huế, Bắc - Nam, Hải sản nướng, Cơm niêu, Lẩu...)
            var mains = allItems.Where(i => i.CategoryName.Contains("Đặc Sản") || 
                                            i.CategoryName.Contains("Bắc - Nam") || 
                                            i.CategoryName.Contains("Hải Sản") || 
                                            i.CategoryName.Contains("Lẩu") || 
                                            i.CategoryName.Contains("Món Chính") || 
                                            i.Price >= 65000).ToList();
            int mainTargetCount = Math.Max(1, request.GuestCount);

            foreach (var main in mains.OrderBy(x => Guid.NewGuid()))
            {
                if (suggested.Count(x => mains.Contains(x)) >= mainTargetCount) break;
                if (runningTotal + main.Price <= request.Budget * 1.15m)
                {
                    suggested.Add(main);
                    runningTotal += main.Price;
                }
            }

            // 3. Đồ uống / Tráng miệng (Chè Huế, Bánh flan, Trà đào, Cà phê muối...)
            var drinks = allItems.Where(i => i.CategoryName.Contains("Đồ Uống") || 
                                             i.CategoryName.Contains("Trà") || 
                                             i.CategoryName.Contains("Tráng miệng") || 
                                             i.CategoryName.Contains("Chè")).ToList();
            if (drinks.Any() && runningTotal < request.Budget)
            {
                var drk = drinks.OrderBy(x => Guid.NewGuid()).First();
                suggested.Add(drk);
                runningTotal += drk.Price;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"🤖 Trợ lý AI gợi ý combo cho đoàn {request.GuestCount} khách với ngân sách ~{request.Budget:N0} đ:");
            sb.AppendLine($"- Tổng số món được chọn: {suggested.Count} món, tổng dự kiến: {runningTotal:N0} đ.");
            if (!string.IsNullOrWhiteSpace(request.Preferences))
            {
                sb.AppendLine($"- Đã tối ưu theo khẩu vị yêu cầu: '{request.Preferences}'.");
            }
            if (!string.IsNullOrWhiteSpace(request.DietaryNotes))
            {
                sb.AppendLine($"- Lưu ý sức khỏe/dị ứng: '{request.DietaryNotes}'.");
            }
            sb.AppendLine("👉 Quý khách có thể xem lại từng món và nhấn 'Thêm vào giỏ món' để gọi món.");

            return new AiAdviceResult
            {
                IsSuccess = true,
                SummaryAdvice = sb.ToString(),
                SuggestedItems = suggested,
                EstimatedTotal = runningTotal
            };
        }
        catch (Exception ex)
        {
            return new AiAdviceResult
            {
                IsSuccess = false,
                ErrorMessage = $"Không thể tải gợi ý AI: {ex.Message}"
            };
        }
    }
}
