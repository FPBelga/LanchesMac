using FastReport.Data;
using LanchesMac.Areas.Admin.Services;
using LanchesMac.Context;
using LanchesMac.Models;
using LanchesMac.Repositories;
using LanchesMac.Repositories.Interfaces;
using LanchesMac.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReflectionIT.Mvc.Paging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
           options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

FastReport.Utils.RegisteredObjects.AddConnection(typeof(MsSqlDataConnection));

//Registrando os servi�os do Identity para Controle de Usu�rio
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
     .AddEntityFrameworkStores<AppDbContext>()
     .AddDefaultTokenProviders();

builder.Services.Configure<ConfigurationImagens>(builder.Configuration.GetSection("ConfigurationPastaImagens"));

//Criando a injen��o de depend�ncia no container nativo para criar a inst�ncia do objeto da interface
builder.Services.AddTransient<ILancheRepository, LancheRepository>();
builder.Services.AddTransient<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddTransient<IPedidoRepository, PedidoRepository>();
builder.Services.AddScoped<ISeedUserRoleInitial, SeedUserRoleInitial>();

builder.Services.AddScoped<RelatorioVendasService>();
builder.Services.AddScoped<GraficoVendasService>();
builder.Services.AddScoped<RelatorioLanchesService>();

//Incluindo a Politica de acesso informando o perfil "Admin" como necess�rio
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin",
        politica =>
        {
            politica.RequireRole("Admin");
        });
});

//Habilitando os recursos do HTTPContext
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

//Cria o carrinho a cada requisi��o. Cria um objeto scoped, ou seja um objeto que esta associado a requisi��o
//Isso significa que se  duas pessoas solicitarem o objeto CarrinhoCompra ao mesmo tempo
//elas v�o obter inst�ncias diferentes
builder.Services.AddScoped(sp => CarrinhoCompra.GetCarrinho(sp));

builder.Services.AddControllersWithViews();

builder.Services.AddPaging(options =>
{
    options.ViewName = "Bootstrap4";
    options.PageParameterName = "pageindex";
});
//Habilitando o cache da sess�o para otimizar a sess�o
builder.Services.AddMemoryCache();
//builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession();
//{
//    options.IdleTimeout = TimeSpan.FromSeconds(10);
//    options.Cookie.HttpOnly = true;
//    options.Cookie.IsEssential = true;
//});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseFastReport();
app.UseRouting();

CriarPerfisUsuarios(app);

////Cria os usu�rios e atribui ao perfil
//seedUserRoleInitial.SeedRoles();
////cria os usu�rios e atribui ao perfil
//seedUserRoleInitial.SeedUsers();

//Ativando o middleware da sess�o
app.UseSession();
app.UseSession();

//Ativando o middleware de autentica��o
app.UseAuthentication();

app.UseAuthorization();


app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
     name: "areas",
     pattern: "{area:exists}/{controller=Admin}/{action=Index}/{id?}");

    endpoints.MapControllerRoute(
       name: "categoriaFiltro",
       pattern: "Lanche/{action}/{categoria?}",
       defaults: new { Controller = "Lanche", action = "List" });

    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.Run();

void CriarPerfisUsuarios(WebApplication app)
{
    var scopedFactory = app.Services.GetService<IServiceScopeFactory>();
    using (var scope = scopedFactory.CreateScope())
    {
        var service = scope.ServiceProvider.GetService<ISeedUserRoleInitial>();
        service.SeedUsers();
        service.SeedRoles();
    }
}