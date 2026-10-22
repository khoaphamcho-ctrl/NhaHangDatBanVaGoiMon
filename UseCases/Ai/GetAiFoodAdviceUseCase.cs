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
    public List<string> MatchedPreferenceHighlights { get; set; } = new();
    public List<string> DietaryComplianceNotes { get; set; } = new();
}

public interface IGetAiFoodAdviceUseCase
{
    Task<AiAdviceResult> ExecuteAsync(AiAdviceRequest request);
}

/// <summary>
/// TIÊU CHÍ K4.3 & MỤC 10: TRỢ LÝ AI GỢI Ý THỰC ĐƠN
/// Thuật toán đề xuất thông minh khớp chính xác Sở Thích Ẩm Thực và Ghi Chú Sức Khỏe/Dị Ứng
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

            var guestCount = Math.Max(1, request.GuestCount);
            var budget = request.Budget > 0 ? request.Budget : 300000m;
            var prefLower = (request.Preferences ?? string.Empty).Trim().ToLowerInvariant();
            var dietLower = (request.DietaryNotes ?? string.Empty).Trim().ToLowerInvariant();

            var matchedHighlights = new List<string>();
            var complianceNotes = new List<string>();

            // 1. XỬ LÝ GHI CHÚ ĂN KIÊNG / SỨC KHỎE / DỊ ỨNG (DIETARY / HEALTH FILTERS)
            var availablePool = new List<MenuItem>(allItems);

            // 1.1 Dị ứng hải sản
            if (ContainsAny(dietLower, "dị ứng hải sản", "không ăn hải sản", "kiêng hải sản", "dị ứng tôm", "dị ứng mực", "dị ứng cua", "dị ứng cá", "không hải sản", "không tôm", "kiêng tôm"))
            {
                availablePool = availablePool.Where(m => 
                    !ContainsAny(m.CategoryName, "Hải Sản") &&
                    !ContainsAny(m.Name, "Tôm", "Mực", "Cá", "Nghêu", "Cua", "Hải Sản", "Lẩu Thái") &&
                    !ContainsAny(m.Description, "Tôm", "Mực", "Cá", "Nghêu", "Cua", "Hải Sản")
                ).ToList();
                complianceNotes.Add("Đã loại trừ toàn bộ các món chứa hải sản (tôm, mực, cá, nghêu...) để đảm bảo an toàn dị ứng.");
            }

            // 1.2 Dị ứng đậu phộng / lạc / hạt
            if (ContainsAny(dietLower, "đậu phộng", "dị ứng lạc", "lạc", "mè rang", "dị ứng hạt"))
            {
                availablePool = availablePool.Where(m => 
                    !ContainsAny(m.Name, "Mè Rang", "Đậu Phộng", "Lạc", "Gỏi Vả", "Ram Ít") &&
                    !ContainsAny(m.Description, "đậu phộng", "mè rang", "lạc")
                ).ToList();
                complianceNotes.Add("Đã sàng lọc và loại bỏ các món có đậu phộng, mè rang hoặc hạt dễ gây dị ứng.");
            }

            // 1.3 Ăn chay / thuần chay
            if (ContainsAny(dietLower, "ăn chay", "chay", "thuần chay"))
            {
                var vegPool = availablePool.Where(m => 
                    ContainsAny(m.CategoryName, "Tráng Miệng", "Chè", "Đồ Uống", "Trà") ||
                    ContainsAny(m.Name, "Chè", "Trái Cây", "Rau", "Nấm", "Salad", "Flan", "Trà", "Cà Phê")
                ).ToList();
                if (vegPool.Any())
                {
                    availablePool = vegPool;
                    complianceNotes.Add("Ưu tiên chọn các món thanh đạm, rau củ, đồ ngọt tráng miệng và trà không chứa đạm động vật.");
                }
            }

            // 1.4 Không ăn cay / Ít cay
            if (ContainsAny(dietLower, "không cay", "ít cay", "không ăn cay", "kiêng cay") ||
                ContainsAny(prefLower, "không cay", "ít cay", "không ăn cay"))
            {
                availablePool = availablePool.Where(m => 
                    !ContainsAny(m.Name, "Chua Cay", "Sa Tế", "Ớt Xanh", "Cay Nồng", "Lẩu Thái", "Nghêu Hấp Sả Ớt") &&
                    !ContainsAny(m.Description, "chua cay", "sa tế", "cay nồng", "ớt xanh ngọt cay")
                ).ToList();
                complianceNotes.Add("Đã loại bỏ các món có vị cay nồng (lẩu chua cay, mực sa tế, ớt) để phù hợp khẩu vị thanh dịu.");
            }

            // 1.5 Người già / Trẻ em / Ít dầu mỡ / Dễ tiêu
            if (ContainsAny(dietLower, "người già", "trẻ em", "ít dầu mỡ", "dễ tiêu", "mềm", "thanh đạm"))
            {
                availablePool = availablePool.Where(m =>
                    !ContainsAny(m.Name, "Chiên Xù", "Nướng Sa Tế")
                ).ToList();
                complianceNotes.Add("Tối ưu hóa các món mềm, hấp, nước dùng thanh ngọt dễ tiêu hóa (Bánh nậm, Phở thố đá, Canh chua, Chè hạt sen...) tốt cho sức khỏe.");
            }

            // 1.6 Kiêng thịt bò
            if (ContainsAny(dietLower, "không ăn bò", "kiêng bò", "dị ứng bò"))
            {
                availablePool = availablePool.Where(m => !ContainsAny(m.Name, "Bò", "Bún Bò", "Phở Bò")).ToList();
                complianceNotes.Add("Đã loại trừ toàn bộ các món chứa thịt bò.");
            }

            // 1.7 Kiêng thịt heo
            if (ContainsAny(dietLower, "không ăn heo", "kiêng heo", "không ăn lợn", "kiêng lợn"))
            {
                availablePool = availablePool.Where(m => !ContainsAny(m.Name, "Heo", "Sườn", "Chả", "Nem", "Thịt Luộc")).ToList();
                complianceNotes.Add("Đã loại trừ toàn bộ các món chứa thịt heo.");
            }

            // Nếu bộ lọc loại bỏ hết thì fallback về danh sách an toàn
            if (!availablePool.Any())
            {
                availablePool = allItems.ToList();
            }

            // 2. HỆ THỐNG TÍNH ĐIỂM SỞ THÍCH ẨM THỰC (PREFERENCE SCORING)
            var prefKeywords = ExtractKeywords(prefLower);

            var scoredItems = availablePool.Select(item =>
            {
                int score = 0;
                var matchedReasons = new List<string>();

                var itemName = item.Name.ToLowerInvariant();
                var itemCat = item.CategoryName.ToLowerInvariant();
                var itemDesc = item.Description.ToLowerInvariant();

                // 2.1 So khớp từ khóa trực tiếp
                foreach (var kw in prefKeywords)
                {
                    if (string.IsNullOrWhiteSpace(kw)) continue;

                    if (itemName.Contains(kw))
                    {
                        score += 50;
                        matchedReasons.Add($"Khớp với sở thích '{kw}'");
                    }
                    else if (itemCat.Contains(kw))
                    {
                        score += 30;
                        matchedReasons.Add($"Khớp nhóm ẩm thực '{item.CategoryName}'");
                    }
                    else if (itemDesc.Contains(kw))
                    {
                        score += 20;
                        matchedReasons.Add($"Khớp hương vị '{kw}'");
                    }
                }

                // 2.2 So khớp theo ngữ nghĩa nhóm (Semantic groups)
                // Hải sản
                if (ContainsAny(prefLower, "hải sản", "tôm", "mực", "cá", "nghêu", "cua"))
                {
                    if (ContainsAny(itemCat, "hải sản") || ContainsAny(itemName, "tôm", "mực", "cá", "nghêu", "hải sản"))
                    {
                        score += 45;
                        matchedReasons.Add("Thuộc nhóm hải sản tươi ngon yêu thích");
                    }
                }

                // Bánh Huế
                if (ContainsAny(prefLower, "bánh", "bánh huế", "bánh bèo", "bánh nậm", "bánh lọc", "bánh khoái", "bánh ram ít", "bánh xèo"))
                {
                    if (ContainsAny(itemCat, "bánh") || ContainsAny(itemName, "bánh bèo", "bánh nậm", "bánh bột lọc", "bánh khoái", "bánh ram ít", "bánh xèo"))
                    {
                        score += 45;
                        matchedReasons.Add("Đặc sản bánh truyền thống Cố Đô Huế");
                    }
                }

                // Bún bò / Món nước / Phở
                if (ContainsAny(prefLower, "bún bò", "bún", "phở", "mì", "mì quảng", "món nước"))
                {
                    if (ContainsAny(itemName, "bún bò", "phở bò", "mì quảng", "bún hến", "bún chả"))
                    {
                        score += 45;
                        matchedReasons.Add("Món nước/món sợi đậm đà theo sở thích");
                    }
                }

                // Thịt bò
                if (ContainsAny(prefLower, "bò", "thịt bò"))
                {
                    if (ContainsAny(itemName, "bò lúc lắc", "bún bò", "phở bò"))
                    {
                        score += 40;
                        matchedReasons.Add("Món bò hảo hạng theo yêu cầu");
                    }
                }

                // Cơm các loại
                if (ContainsAny(prefLower, "cơm", "cơm niêu", "cơm tấm", "cơm hến"))
                {
                    if (ContainsAny(itemName, "cơm niêu", "cơm tấm", "cơm hến"))
                    {
                        score += 40;
                        matchedReasons.Add("Món cơm đặc sản thơm ngon chắc dạ");
                    }
                }

                // Lẩu / Canh nóng
                if (ContainsAny(prefLower, "lẩu", "canh", "món nóng"))
                {
                    if (ContainsAny(itemName, "lẩu thái", "canh chua"))
                    {
                        score += 40;
                        matchedReasons.Add("Món lẩu/canh nóng hổi sum vầy");
                    }
                }

                // Món nướng
                if (ContainsAny(prefLower, "nướng", "đồ nướng"))
                {
                    if (ContainsAny(itemName, "nướng muối ớt", "nướng sa tế", "nướng than hoa"))
                    {
                        score += 40;
                        matchedReasons.Add("Món nướng thơm lừng theo sở thích");
                    }
                }

                // Món cay nồng
                if (ContainsAny(prefLower, "cay", "cay nồng", "thích cay"))
                {
                    if (ContainsAny(itemName, "chua cay", "sa tế", "ớt xanh", "bún bò") || ContainsAny(itemDesc, "chua cay", "sa tế", "ớt"))
                    {
                        score += 35;
                        matchedReasons.Add("Vị cay nồng đúng gu ẩm thực");
                    }
                }

                // Đồ ngọt / Tráng miệng / Đồ uống
                if (ContainsAny(prefLower, "tráng miệng", "chè", "đồ ngọt", "cà phê", "trà", "nước", "uống"))
                {
                    if (ContainsAny(itemCat, "tráng miệng", "chè", "đồ uống", "trà") || ContainsAny(itemName, "chè", "flan", "trà", "cà phê", "sinh tố", "trái cây"))
                    {
                        score += 35;
                        matchedReasons.Add("Tráng miệng/thức uống giải nhiệt");
                    }
                }

                return new { Item = item, Score = score, Reasons = matchedReasons };
            }).ToList();

            // 3. XÂY DỰNG COMBO MÓN THÔNG MINH
            var selectedItems = new List<MenuItem>();
            decimal currentTotal = 0;

            // 3.1 BƯỚC 1: Chọn TẤT CẢ các món có điểm sở thích cao nhất trước
            var topPreferenceItems = scoredItems
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Item.Price)
                .ToList();

            foreach (var top in topPreferenceItems)
            {
                if (currentTotal + top.Item.Price <= budget * 1.15m || selectedItems.Count == 0)
                {
                    if (!selectedItems.Any(i => i.MenuItemId == top.Item.MenuItemId))
                    {
                        selectedItems.Add(top.Item);
                        currentTotal += top.Item.Price;

                        if (top.Reasons.Any())
                        {
                            var topReason = top.Reasons.First();
                            var entry = $"✓ {top.Item.Name}: {topReason}";
                            if (!matchedHighlights.Contains(entry))
                            {
                                matchedHighlights.Add(entry);
                            }
                        }
                    }
                }

                // Nếu đã chọn đủ số món và vượt 75% ngân sách thì dừng
                if (selectedItems.Count >= Math.Min(6, guestCount + 2) && currentTotal >= budget * 0.75m)
                {
                    break;
                }
            }

            // 3.2 BƯỚC 2: Cân bằng bữa ăn (Khai vị + Món chính + Tráng miệng/Đồ uống)
            // Khai vị
            bool hasAppetizer = selectedItems.Any(i => i.CategoryName.Contains("Khai Vị") || i.CategoryName.Contains("Bánh"));
            if (!hasAppetizer)
            {
                var candidateApp = scoredItems
                    .Where(x => x.Item.CategoryName.Contains("Khai Vị") || x.Item.CategoryName.Contains("Bánh"))
                    .Where(x => !selectedItems.Any(s => s.MenuItemId == x.Item.MenuItemId))
                    .OrderByDescending(x => x.Score)
                    .ThenBy(x => x.Item.Price)
                    .FirstOrDefault();

                if (candidateApp != null && (currentTotal + candidateApp.Item.Price <= budget * 1.2m || selectedItems.Count < 2))
                {
                    selectedItems.Insert(0, candidateApp.Item);
                    currentTotal += candidateApp.Item.Price;
                }
            }

            // Món chính bổ sung nếu số món ít hơn số khách
            int targetMainCount = Math.Max(2, (guestCount <= 2 ? 2 : guestCount));
            while (selectedItems.Count < targetMainCount && currentTotal < budget)
            {
                var candidateMain = scoredItems
                    .Where(x => !selectedItems.Any(s => s.MenuItemId == x.Item.MenuItemId))
                    .Where(x => !x.Item.CategoryName.Contains("Tráng Miệng") && !x.Item.CategoryName.Contains("Đồ Uống"))
                    .OrderByDescending(x => x.Score)
                    .ThenBy(x => Math.Abs(budget - (currentTotal + x.Item.Price)))
                    .FirstOrDefault();

                if (candidateMain == null) break;
                if (currentTotal + candidateMain.Item.Price > budget * 1.2m && selectedItems.Count >= 2) break;

                selectedItems.Add(candidateMain.Item);
                currentTotal += candidateMain.Item.Price;
            }

            // Đồ uống / Tráng miệng giải nhiệt
            bool hasDessertOrDrink = selectedItems.Any(i => i.CategoryName.Contains("Tráng Miệng") || i.CategoryName.Contains("Đồ Uống"));
            if (!hasDessertOrDrink && currentTotal < budget * 1.15m)
            {
                var candidateDessert = scoredItems
                    .Where(x => x.Item.CategoryName.Contains("Tráng Miệng") || x.Item.CategoryName.Contains("Đồ Uống"))
                    .Where(x => !selectedItems.Any(s => s.MenuItemId == x.Item.MenuItemId))
                    .OrderByDescending(x => x.Score)
                    .ThenBy(x => x.Item.Price)
                    .FirstOrDefault();

                if (candidateDessert != null)
                {
                    selectedItems.Add(candidateDessert.Item);
                    currentTotal += candidateDessert.Item.Price;
                }
            }

            // Fallback an toàn nếu vẫn rỗng
            if (!selectedItems.Any())
            {
                var defaultItems = availablePool.Take(3).ToList();
                selectedItems.AddRange(defaultItems);
                currentTotal = defaultItems.Sum(x => x.Price);
            }

            // 4. BIÊN SOẠN BẢN TƯ VẤN TỔNG THỂ (SUMMARY ADVICE)
            var sb = new StringBuilder();
            sb.AppendLine($"🤖 Trợ lý AI đã xây dựng combo thực đơn tối ưu cho đoàn {guestCount} khách:");
            sb.AppendLine();

            if (!string.IsNullOrWhiteSpace(request.Preferences))
            {
                sb.AppendLine($"🎯 Khớp với sở thích ẩm thực: '{request.Preferences}'");
                if (matchedHighlights.Any())
                {
                    foreach (var hl in matchedHighlights.Take(4))
                    {
                        sb.AppendLine($"   {hl}");
                    }
                }
                else
                {
                    sb.AppendLine($"   - Hệ thống đã ưu tiên các món ngon Cố Đô đáp ứng sát nhất gu thưởng thức của bạn.");
                }
                sb.AppendLine();
            }

            if (!string.IsNullOrWhiteSpace(request.DietaryNotes))
            {
                sb.AppendLine($"🛡️ Tuân thủ ghi chú sức khỏe / Dị ứng: '{request.DietaryNotes}'");
                if (complianceNotes.Any())
                {
                    foreach (var note in complianceNotes)
                    {
                        sb.AppendLine($"   - {note}");
                    }
                }
                else
                {
                    sb.AppendLine($"   - Đã kiểm tra nguyên liệu và hướng dẫn nhà bếp lưu ý khẩu vị riêng của đoàn.");
                }
                sb.AppendLine();
            }

            sb.AppendLine($"💰 Ngân sách & Định lượng:");
            sb.AppendLine($"   - Tổng cộng {selectedItems.Count} món, tổng chi phí dự kiến: {currentTotal:N0} đ (Ngân sách ban đầu: {budget:N0} đ).");
            sb.AppendLine($"   - Bữa ăn được phân bổ hài hòa: có món khai vị thanh nhẹ, món chính đậm đà tròn vị và tráng miệng thanh mát.");
            sb.AppendLine();
            sb.AppendLine("👉 Quý khách có thể xem chi tiết từng món bên dưới và bấm 'Xác Nhận Thêm Vào Bàn' để nhà bếp chuẩn bị phục vụ ngay!");

            return new AiAdviceResult
            {
                IsSuccess = true,
                SummaryAdvice = sb.ToString(),
                SuggestedItems = selectedItems,
                EstimatedTotal = currentTotal,
                MatchedPreferenceHighlights = matchedHighlights,
                DietaryComplianceNotes = complianceNotes
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

    private static bool ContainsAny(string source, params string[] terms)
    {
        if (string.IsNullOrEmpty(source)) return false;
        foreach (var t in terms)
        {
            if (source.Contains(t, StringComparison.OrdinalIgnoreCase)) return true;
        }
        return false;
    }

    private static List<string> ExtractKeywords(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return new List<string>();

        var tokens = text.Split(new[] { ',', ';', '.', '-', '+', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim().ToLowerInvariant())
            .Where(t => t.Length >= 2)
            .ToList();

        var subWords = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(w => w.Trim().ToLowerInvariant())
            .Where(w => w.Length >= 3 && !IsStopWord(w))
            .ToList();

        tokens.AddRange(subWords);
        return tokens.Distinct().ToList();
    }

    private static bool IsStopWord(string word)
    {
        var stopWords = new HashSet<string> { "thích", "muốn", "cho", "và", "với", "các", "loại", "món", "của", "tôi", "đoàn", "người", "ăn", "uống", "nhiều", "chút", "thêm" };
        return stopWords.Contains(word);
    }
}
