using Microsoft.AspNetCore.Components.Authorization;
using Plugins.DataStore.InMemory;
using Plugins.DataStore.SQL;
using UseCases.Ai;
using UseCases.Cart;
using UseCases.DataStorePluginInterfaces;
using UseCases.Menu;
using UseCases.Orders;
using UseCases.Reservations;
using WebApp.Authentication;
using WebApp.Components;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Blazor Web App Interactive Server (Tương đương Blazor Server trong 35 video)
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 2. TIÊU CHÍ K4.1 & K4.2: Cấu hình Phân Quyền & Xác Thực (Authorization / Authentication)
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<IAuthService, AuthService>();

// 3. TIÊU CHÍ K1.4: Cấu hình Giỏ Món Scoped Service (State Management - như Video Part 3)
builder.Services.AddScoped<ITableCartService, TableCartService>();

// 4. TIÊU CHÍ K1.2: Đăng ký các UseCase (Giao tiếp qua Interface, .razor inject Interface)
builder.Services.AddTransient<IViewTablesUseCase, ViewTablesUseCase>();
builder.Services.AddTransient<IPlaceReservationUseCase, PlaceReservationUseCase>();
builder.Services.AddTransient<ICancelReservationUseCase, CancelReservationUseCase>();
builder.Services.AddTransient<IViewMenuUseCase, ViewMenuUseCase>();
builder.Services.AddTransient<IManageMenuUseCase, ManageMenuUseCase>();
builder.Services.AddTransient<IViewTableOrderUseCase, ViewTableOrderUseCase>();
builder.Services.AddTransient<ISubmitTableOrderUseCase, SubmitTableOrderUseCase>();
builder.Services.AddTransient<ICloseTableOrderUseCase, CloseTableOrderUseCase>();
builder.Services.AddTransient<IUpdateOrderLineStatusUseCase, UpdateOrderLineStatusUseCase>();

// TIÊU CHÍ K4.3 & MỤC 10: Trợ lý AI gợi ý thực đơn
builder.Services.AddTransient<IGetAiFoodAdviceUseCase, GetAiFoodAdviceUseCase>();

// 5. TIÊU CHÍ K1.1 & K5.2: COMPOSITION ROOT - HOÁN ĐỔI PLUGIN DỮ LIỆU (PLUG & PLAY)
// Cho phép chuyển đổi linh hoạt giữa InMemory (Sprint 1) và SQL Server Dapper (Sprint 2)
bool useSqlDataStore = builder.Configuration.GetValue<bool>("DataStore:UseSql");

if (useSqlDataStore)
{
    // Cài đặt Plugin SQL Server với Dapper (K2.1 - K2.4)
    string connectionString = builder.Configuration.GetConnectionString("RestaurantDb") 
        ?? "Server=(localdb)\\mssqllocaldb;Database=RestaurantDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

    builder.Services.AddSingleton(new SqlDataAccess(connectionString));
    builder.Services.AddScoped<IDiningTableRepository, DiningTableSqlRepository>();
    builder.Services.AddScoped<IReservationRepository, ReservationSqlRepository>();
    builder.Services.AddScoped<IMenuCategoryRepository, MenuCategorySqlRepository>();
    builder.Services.AddScoped<IMenuItemRepository, MenuItemSqlRepository>();
    builder.Services.AddScoped<ITableOrderRepository, TableOrderSqlRepository>();
    builder.Services.AddScoped<IRestaurantConfigRepository, RestaurantConfigSqlRepository>();
    builder.Services.AddScoped<IUserRepository, UserSqlRepository>();
}
else
{
    // Cài đặt Plugin In-Memory (Dữ liệu tĩnh trong RAM - Sẵn sàng chạy ngay mà không cần cấu hình CSDL trước)
    builder.Services.AddSingleton<IDiningTableRepository, DiningTableInMemoryRepository>();
    builder.Services.AddSingleton<IReservationRepository, ReservationInMemoryRepository>();
    builder.Services.AddSingleton<IMenuCategoryRepository, MenuCategoryInMemoryRepository>();
    builder.Services.AddSingleton<IMenuItemRepository, MenuItemInMemoryRepository>();
    builder.Services.AddSingleton<ITableOrderRepository, TableOrderInMemoryRepository>();
    builder.Services.AddSingleton<IRestaurantConfigRepository, RestaurantConfigInMemoryRepository>();
    builder.Services.AddSingleton<IUserRepository, UserInMemoryRepository>();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
